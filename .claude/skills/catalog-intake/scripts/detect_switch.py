# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27"]
# ///
"""
detect_switch.py — 보안 플러그인 설치 파일을 내려받아 **무인(사일런트) 설치 스위치**를 추론한다.

동작
  1. 설치 파일을 **브라우저(Windows Edge)로 위장**해 내려받는다(봇 차단 우회). 헤더 판별만
     필요하므로 앞부분 일부(기본 4MB)만 Range로 받는다.
  2. `ussfc`(Universal Silent Setup Finder Console, dotnet tool)로 패키징 형식과 사일런트
     명령을 탐지한다.  설치: `dotnet tool install -g ussfc`  (없으면 `dnx ussfc`로 폴백)
  3. 파일명을 **알려진 벤더 시드표**와 대조한다. 국내 보안 설치기(Veraport/TouchEn/AhnLab 등)는
     ussfc의 일반 EXE 판별(대개 NSIS `/S`)이 틀리기 쉬우므로 **시드표 값을 우선**한다.

정확도 원칙
  - 시드표에 있으면 시드표 값을 최종 후보로(= source: seed), ussfc와 다르면 conflict 표시.
  - 시드표에 없으면 ussfc 결과를 후보로(= source: ussf), **needs_verification=true**.
  - 어느 쪽이든 최종 확정은 **실제 무인 설치 테스트**(Windows Sandbox/VM)로 사람이 검증.

사용법
  uv run detect_switch.py <installer_url> [--json] [--max-bytes 4194304] [--keep]

전제: `ussfc`가 PATH에 있거나(.NET 전역 도구) `dnx`(.NET 10 SDK)가 사용 가능해야 함.
"""
from __future__ import annotations

import argparse
import ipaddress
import json
import os
import re
import shutil
import socket
import subprocess
import sys
import tempfile
from pathlib import Path
from urllib.parse import urlparse, unquote

import httpx

for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")  # type: ignore[union-attr]
    except Exception:  # noqa: BLE001
        pass

EDGE_UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
           "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0")
EDGE_HEADERS = {
    "User-Agent": EDGE_UA,
    "sec-ch-ua": '"Microsoft Edge";v="131", "Chromium";v="131", "Not_A Brand";v="24"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Windows"',
    "Accept": "application/octet-stream,application/x-msdownload,*/*;q=0.8",
    "Accept-Language": "ko-KR,ko;q=0.9,en;q=0.8",
}

# 알려진 국내 보안 설치기(시드표 부분집합). 파일명 정규식 → (Package Name, Arguments).
# 전체 표는 references/field-mapping.md 4절. ussfc가 틀리기 쉬운 벤더 위주.
KNOWN_INSTALLERS: list[tuple[str, str, str]] = [
    (r"veraport",                         "Veraport",                 "/silent"),
    (r"nos_setup",                        "nProtect Online Security", "/silent"),
    (r"astx|astxdn",                      "AhnLabSafeTx",             "/silent"),
    (r"delfino",                          "Delfino G3",               "/silent"),
    (r"anysign",                          "AnySign",                  "/S"),
    (r"touchen.*nxkey.*64",               "TouchEnKey64",             "/silence"),
    (r"touchen.*nxkey",                   "TouchEnKey32",             "/silence"),
    (r"touchen.*nxweb|nxweb",             "TouchEnWeb",               "/silence"),
    (r"inis_ex|inisafe.*crossweb",        "INISAFECrossWeb",          "/S"),
    (r"i3gsvcmanager|ipinside",           "IPInside",                 "/nodlg"),
    (r"inst_mawebdrm|mawebdrm",           "MaWebDRM",                 "/silent"),
    (r"unisign",                          "UniSign",                  "/silent"),
    (r"epagesafer|epagesafe",             "ePageSafer",               "/silent"),
    (r"delfino-g3|delfino_g3",            "Delfino G3",               "/silent"),
]


def is_public_url(url: str) -> tuple[bool, str]:
    try:
        p = urlparse(url)
    except Exception as exc:  # noqa: BLE001
        return False, f"parse-error: {exc}"
    if p.scheme not in ("http", "https"):
        return False, f"scheme-not-allowed: {p.scheme or '(none)'}"
    if not p.hostname:
        return False, "no-host"
    try:
        infos = socket.getaddrinfo(p.hostname, None)
    except socket.gaierror as exc:
        return False, f"dns-failed: {exc}"
    for info in infos:
        ip = ipaddress.ip_address(info[4][0])
        if (ip.is_private or ip.is_loopback or ip.is_link_local
                or ip.is_reserved or ip.is_multicast):
            return False, f"blocked-ip: {ip}"
    return True, "ok"


def filename_from(url: str, content_disposition: str | None) -> str:
    if content_disposition:
        m = re.search(r"filename\*?=(?:UTF-8'')?\"?([^\";]+)", content_disposition)
        if m:
            return unquote(m.group(1)).strip()
    name = urlparse(url).path.rsplit("/", 1)[-1]
    return unquote(name) or "installer.bin"


def download_head(url: str, max_bytes: int, timeout: float) -> tuple[Path, str, int]:
    """설치 파일 앞부분을 Edge 위장으로 내려받아 임시 파일로 저장. (path, filename, bytes)"""
    headers = dict(EDGE_HEADERS)
    headers["Range"] = f"bytes=0-{max_bytes - 1}"
    with httpx.Client(headers=headers, timeout=timeout, follow_redirects=True) as client:
        with client.stream("GET", url) as resp:
            resp.raise_for_status()
            fname = filename_from(str(resp.url), resp.headers.get("content-disposition"))
            ext = os.path.splitext(fname)[1] or ".bin"
            fd, tmp = tempfile.mkstemp(suffix=ext, prefix="ussf_")
            written = 0
            with os.fdopen(fd, "wb") as f:
                for chunk in resp.iter_bytes(65536):
                    f.write(chunk)
                    written += len(chunk)
                    if written >= max_bytes:
                        break
    return Path(tmp), fname, written


def run_ussfc(path: Path) -> dict:
    """ussfc --json 실행. ussfc → dnx ussfc → dotnet tool exec 순으로 폴백."""
    env = dict(os.environ)
    tools = str(Path.home() / ".dotnet" / "tools")
    if tools not in env.get("PATH", ""):
        env["PATH"] = env.get("PATH", "") + os.pathsep + tools
    candidates = [["ussfc", "--json", str(path)]]
    if shutil.which("dnx"):
        candidates.append(["dnx", "ussfc", "--json", str(path)])
    if shutil.which("dotnet"):
        candidates.append(["dotnet", "tool", "exec", "ussfc", "--", "--json", str(path)])
    last_err = ""
    for cmd in candidates:
        try:
            out = subprocess.run(cmd, capture_output=True, text=True, env=env, timeout=120)
        except (FileNotFoundError, subprocess.TimeoutExpired) as exc:
            last_err = f"{cmd[0]}: {exc}"
            continue
        if out.returncode == 0 and out.stdout.strip():
            try:
                return json.loads(out.stdout)
            except json.JSONDecodeError:
                last_err = f"{cmd[0]}: non-JSON output: {out.stdout[:200]}"
                continue
        last_err = f"{cmd[0]}: rc={out.returncode} {out.stderr.strip()[:200]}"
    raise RuntimeError(f"ussfc 실행 실패 (설치: dotnet tool install -g ussfc). 마지막 오류: {last_err}")


def arguments_from_usage(usage: str) -> str:
    """ussf usage 문자열에서 스위치 부분만 추출. usage는 분석 파일(임시명)로 시작하므로
    첫 따옴표 토큰(= 파일 경로) 이후만 취한다. 예) '"x.exe" /S' → '/S'."""
    if not usage:
        return ""
    u = usage.strip()
    m = re.search(r'"[^"]*"', u)          # 첫 "..." = 분석 대상 파일 경로
    if m:
        return u[m.end():].strip()
    parts = u.split(None, 1)              # 따옴표 없으면 첫 토큰 제거
    return parts[1].strip() if len(parts) == 2 else u


def match_seed(filename: str) -> tuple[str, str] | None:
    low = filename.lower()
    for pattern, name, args in KNOWN_INSTALLERS:
        if re.search(pattern, low):
            return name, args
    return None


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description="설치 파일 → 무인 설치 스위치 추론 (Edge 위장 + ussfc)")
    ap.add_argument("url", help="설치 파일 URL")
    ap.add_argument("--json", action="store_true", help="JSON 출력")
    ap.add_argument("--max-bytes", type=int, default=4 * 1024 * 1024,
                    help="내려받을 최대 바이트(기본 4MB, 헤더 판별용)")
    ap.add_argument("--keep", action="store_true", help="임시 파일 삭제 안 함")
    ap.add_argument("--timeout", type=float, default=60.0)
    args = ap.parse_args(argv)

    allowed, reason = is_public_url(args.url)
    if not allowed:
        print(f"[error] URL 거부됨: {reason}", file=sys.stderr)
        return 2

    tmp: Path | None = None
    try:
        tmp, fname, nbytes = download_head(args.url, args.max_bytes, args.timeout)
        print(f"[info] 다운로드(Edge 위장): {fname} · {nbytes} bytes", file=sys.stderr)
        ussf = run_ussfc(tmp)
    except (httpx.HTTPError, RuntimeError) as exc:
        print(f"[error] {exc}", file=sys.stderr)
        return 1
    finally:
        if tmp and not args.keep and tmp.exists():
            try:
                tmp.unlink()
            except OSError:
                pass

    ussf_args = arguments_from_usage(ussf.get("usage", ""))
    seed = match_seed(fname)

    if seed:
        name, s_args = seed
        conflict = (s_args != ussf_args)
        result = {
            "url": args.url, "filename": fname,
            "ussf": {"type": ussf.get("type"), "usage": ussf.get("usage"),
                     "arguments": ussf_args},
            "seed_match": {"name": name, "arguments": s_args},
            "suggested": {"name": name, "arguments": s_args, "source": "seed"},
            "conflict": conflict,
            "needs_verification": False,
        }
    else:
        result = {
            "url": args.url, "filename": fname,
            "ussf": {"type": ussf.get("type"), "usage": ussf.get("usage"),
                     "arguments": ussf_args},
            "seed_match": None,
            "suggested": {"name": None, "arguments": ussf_args, "source": "ussf"},
            "conflict": False,
            "needs_verification": True,
        }

    if args.json:
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        s = result["suggested"]
        print(f"파일: {fname}")
        print(f"  ussf 판별 : {result['ussf']['type']} → {result['ussf']['arguments'] or '(없음)'}")
        if result["seed_match"]:
            print(f"  시드표    : {result['seed_match']['name']} → {result['seed_match']['arguments']}")
        print(f"  ▶ 후보    : Name={s['name'] or '(직접 확인)'} Arguments={s['arguments'] or '(없음)'} "
              f"[source: {s['source']}]")
        if result["conflict"]:
            print("  ⚠️ ussf와 시드표가 다름 — 시드표 우선. 실제 무인설치 테스트로 확정 요망.")
        if result["needs_verification"]:
            print("  ⚠️ 시드표에 없는 설치기 — ussf 추정값이므로 실제 무인설치 테스트로 확정 요망.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
