# /// script
# requires-python = ">=3.11"
# dependencies = []
# ///
"""
normalize_csv.py — Google Forms(식탁보 사이트 추가/수정/삭제 요청) 응답 CSV를
카탈로그 스키마에 가까운 정규화 JSON으로 변환한다. (네트워크 접근 없음, 순수 로직)

역할
  - CSV 헤더를 '질문 제목 키워드'로 관대하게 매칭한다(헤더 문구가 조금 바뀌어도 견딤).
  - 한국어 종류(은행/카드/…) → Catalog.xsd Category enum으로 매핑.
  - 영어 이름 → Service Id 후보(PascalCase) 파생.
  - 보안 플러그인 URL(한 줄에 하나) → 리스트로 분해.
  - 기계가 못 채우는 빈칸(gaps)을 명시적으로 표시한다:
      * arguments-missing      : 폼이 사일런트 스위치를 안 받음 → 조사 필요
      * package-name-derive    : Package Name은 URL에서 추론/확정 필요
      * category-needs-decision: '증권' 등 enum에 직접 대응 없는 종류
      * id-collision           : (--catalog 지정 시) 기존 Id와 충돌
      * consent-missing/dedup-missing : 게이트 미동의(스팸/오제출 의심)

사용법
  uv run normalize_csv.py <responses.csv> [--catalog ../../docs/Catalog.xml] [--out out.json] [--pretty]

출력
  - stdout: 사람용 요약(행별 한 눈에 보이는 표 형태)
  - --out 지정 시 정규화 레코드 JSON 배열을 파일로 저장
"""
from __future__ import annotations

import argparse
import csv
import io
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

# Windows 콘솔 기본 인코딩(cp949)에서 한글/이모지 출력이 깨지거나 죽는 것을 방지
for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")  # type: ignore[union-attr]
    except Exception:  # noqa: BLE001
        pass

# XSD CatalogInternetServiceCategory enum (docs/Catalog.xsd 기준)
VALID_CATEGORIES = {
    "Other", "Banking", "Financing", "Security",
    "Insurance", "CreditCard", "Government", "Education",
}

# 폼 라디오(한국어 종류) → enum.
CATEGORY_MAP = {
    "은행": "Banking",
    "카드": "CreditCard",
    "공공기관": "Government",
    "금융": "Financing",
    "보험": "Insurance",
    "교육": "Education",
    # '증권'은 XSD에 대응 enum이 없음. 운영자 결정(2026-07): 잠정적으로 Financing 매핑.
    # 향후 XSD에 Securities 를 추가하면 여기와 field-mapping.md 를 함께 갱신할 것.
    "증권": "Financing",
    "기타": "Other",
}
# 대응 enum이 없어 사람 판단이 필요한 종류. 현재는 모두 매핑 결정되어 비어 있다.
CATEGORY_NEEDS_DECISION: set[str] = set()

CHANGE_TYPE_MAP = {"추가": "add", "수정": "modify", "삭제": "delete"}

# 헤더 → 필드. (부분 문자열 키워드 리스트 중 하나라도 포함되면 매칭)
# 순서 중요: 더 구체적인 규칙을 앞에 둔다.
HEADER_RULES: list[tuple[str, list[str]]] = [
    ("timestamp",       ["타임스탬프", "timestamp"]),
    ("reporter_email",  ["이메일", "사용자"]),  # "이메일 주소" 또는 "사용자 이름"(identity) 열
    ("change_type",     ["유형의 변경", "변경 사항"]),
    ("display_name_ko", ["한국어 이름"]),
    ("display_name_en", ["영어 이름"]),
    ("url",             ["대표 주소"]),
    ("icon_ref",        ["아이콘"]),
    ("category_ko",     ["종류"]),
    ("package_urls",    ["보안 플러그인", "플러그인 프로그램"]),  # "…URL을 한 줄씩…"
    ("compat_notes",    ["windows sandbox", "sandbox"]),
    ("usage_notes",     ["메뉴", "위치"]),
    ("consent",         ["개인 정보 수집", "개인정보 수집"]),
    ("dedup_ack",       ["없거나 반영되지", "먼저 확인"]),
]


def build_header_index(fieldnames: list[str]) -> dict[str, str]:
    """CSV 실제 헤더 → 논리 필드명. 각 논리 필드는 최초 매칭 헤더 하나만 취한다."""
    mapping: dict[str, str] = {}
    used_headers: set[str] = set()
    for field, keywords in HEADER_RULES:
        if field in mapping.values():
            continue
        for header in fieldnames:
            if header in used_headers:
                continue
            low = header.lower()
            if any(kw.lower() in low for kw in keywords):
                mapping[header] = field
                used_headers.add(header)
                break
    return mapping


def derive_id(english_name: str) -> str:
    """영어 이름 → PascalCase Id 후보. 이미 대문자/내부대문자 토큰은 보존(KEB, NH, KB)."""
    if not english_name:
        return ""
    tokens = re.split(r"[^0-9A-Za-z]+", english_name.strip())
    parts: list[str] = []
    for tok in tokens:
        if not tok:
            continue
        # 이미 두 글자 이상 대문자를 포함하면 약어로 보고 원형 유지
        if sum(1 for c in tok if c.isupper()) >= 2:
            parts.append(tok)
        else:
            parts.append(tok[:1].upper() + tok[1:])
    return "".join(parts)


def split_package_urls(raw: str) -> list[str]:
    """줄바꿈/공백/쉼표로 구분된 URL 텍스트에서 http(s) URL만 추출."""
    if not raw:
        return []
    candidates = re.split(r"[\s,]+", raw.strip())
    urls: list[str] = []
    for c in candidates:
        c = c.strip().strip("<>\"'")
        if re.match(r"^https?://", c, re.IGNORECASE):
            urls.append(c)
    # 중복 제거(순서 보존)
    seen: set[str] = set()
    out: list[str] = []
    for u in urls:
        if u not in seen:
            seen.add(u)
            out.append(u)
    return out


def load_existing_ids(catalog_path: Path) -> set[str]:
    ids: set[str] = set()
    try:
        tree = ET.parse(catalog_path)
    except Exception as exc:  # noqa: BLE001
        print(f"[warn] 카탈로그를 읽지 못해 Id 충돌 검사를 건너뜁니다: {exc}", file=sys.stderr)
        return ids
    for el in tree.iter():
        tag = el.tag.split("}")[-1]
        if tag in ("Service", "Companion"):
            sid = el.get("Id")
            if sid:
                ids.add(sid.strip())
    return ids


def yes_like(value: str) -> bool:
    """라디오 단일 옵션(동의/확인)이 채워졌으면 True."""
    return bool(value and value.strip())


def normalize_row(row: dict[str, str], header_index: dict[str, str],
                  existing_ids: set[str]) -> dict:
    def get(field: str) -> str:
        for header, logical in header_index.items():
            if logical == field:
                return (row.get(header) or "").strip()
        return ""

    display_name_en = get("display_name_en")
    category_ko = get("category_ko")
    category = CATEGORY_MAP.get(category_ko, "Other")
    package_urls = split_package_urls(get("package_urls"))
    id_suggested = derive_id(display_name_en)

    gaps: list[str] = []
    # 폼은 사일런트 스위치를 받지 않으므로 항상 조사 필요
    if package_urls:
        gaps.append("arguments-missing")
        gaps.append("package-name-derive")
    if category_ko in CATEGORY_NEEDS_DECISION:
        gaps.append("category-needs-decision")
    id_collision = bool(id_suggested and id_suggested in existing_ids)
    if id_collision:
        gaps.append("id-collision")
    if not yes_like(get("consent")):
        gaps.append("consent-missing")
    if not yes_like(get("dedup_ack")):
        gaps.append("dedup-missing")

    rec = {
        "timestamp": get("timestamp"),
        "reporter_email": get("reporter_email"),
        "change_type": CHANGE_TYPE_MAP.get(get("change_type"), get("change_type")),
        "display_name_ko": get("display_name_ko"),
        "display_name_en": display_name_en,
        "url": get("url"),
        "category_ko": category_ko,
        "category": category,
        "id_suggested": id_suggested,
        "id_collision": id_collision,
        "package_urls": package_urls,
        "compat_notes": get("compat_notes"),
        "usage_notes": get("usage_notes"),
        "icon_ref": get("icon_ref"),
        "gaps": gaps,
    }
    return rec


def read_csv_rows(path: Path) -> tuple[list[str], list[dict[str, str]]]:
    # Google/Excel 내보내기는 utf-8-sig(BOM) 인 경우가 많다.
    text = path.read_text(encoding="utf-8-sig")
    reader = csv.DictReader(io.StringIO(text))
    fieldnames = reader.fieldnames or []
    rows = [dict(r) for r in reader]
    return fieldnames, rows


def print_summary(records: list[dict], header_index: dict[str, str],
                  fieldnames: list[str]) -> None:
    unmatched = [h for h in fieldnames if h not in header_index]
    print(f"# 정규화 결과: {len(records)}건")
    if unmatched:
        print(f"# 매칭 안 된 CSV 열({len(unmatched)}): " + ", ".join(unmatched))
    print()
    for i, r in enumerate(records, start=1):
        print(f"[{i}] {r['change_type'] or '?'} · {r['display_name_ko'] or '(이름없음)'} "
              f"({r['display_name_en'] or '-'})")
        print(f"    Id후보 : {r['id_suggested'] or '-'}"
              + ("  ⚠️충돌" if r['id_collision'] else ""))
        print(f"    분류   : {r['category_ko'] or '-'} → {r['category']}")
        print(f"    URL    : {r['url'] or '-'}")
        print(f"    패키지 : {len(r['package_urls'])}개")
        for u in r["package_urls"]:
            print(f"             - {u}")
        if r["gaps"]:
            print(f"    빈칸   : {', '.join(r['gaps'])}")
        print()


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description="Google Forms 제보 CSV → 정규화 JSON")
    ap.add_argument("csv", type=Path, help="응답 CSV 경로")
    ap.add_argument("--catalog", type=Path, default=None,
                    help="docs/Catalog.xml 경로(지정 시 Id 충돌 검사)")
    ap.add_argument("--out", type=Path, default=None, help="정규화 JSON 저장 경로")
    ap.add_argument("--pretty", action="store_true", help="JSON 들여쓰기")
    args = ap.parse_args(argv)

    if not args.csv.exists():
        print(f"[error] CSV 없음: {args.csv}", file=sys.stderr)
        return 2

    fieldnames, rows = read_csv_rows(args.csv)
    header_index = build_header_index(fieldnames)

    matched_fields = set(header_index.values())
    required = {"display_name_ko", "display_name_en", "url", "category_ko", "package_urls"}
    missing = required - matched_fields
    if missing:
        print(f"[warn] 필수 열 매칭 실패: {sorted(missing)} — 헤더를 확인하세요.",
              file=sys.stderr)

    existing_ids = load_existing_ids(args.catalog) if args.catalog else set()

    records = [normalize_row(r, header_index, existing_ids) for r in rows]

    print_summary(records, header_index, fieldnames)

    if args.out:
        args.out.write_text(
            json.dumps(records, ensure_ascii=False,
                       indent=2 if args.pretty else None),
            encoding="utf-8",
        )
        print(f"# JSON 저장: {args.out}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
