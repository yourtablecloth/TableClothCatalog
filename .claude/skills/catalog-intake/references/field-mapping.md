# 제보 폼 → 카탈로그 스키마 매핑 참조

이 문서는 Google Forms **"식탁보 사이트 추가/수정/삭제 요청"** 응답을 `docs/Catalog.xml`
스키마로 옮길 때 쓰는 매핑·규칙·시드 데이터를 담는다. `catalog-intake` 스킬과
`scripts/normalize_csv.py` 가 이 표를 기준으로 동작한다.

## 1. 필드 매핑

| 폼 질문(키워드) | 논리 필드 | 카탈로그 대상 | 비고 |
|---|---|---|---|
| 타임스탬프 | `timestamp` | (이슈 메타) | Google 자동 |
| 이메일 주소 | `reporter_email` | (이슈 메타) | 로그인 필수 → verified |
| 어떤 유형의 변경 사항 | `change_type` | 처리 분기 | 추가/수정/삭제 → add/modify/delete |
| 웹 사이트의 한국어 이름 | `display_name_ko` | `Service@DisplayName` | |
| 웹 사이트의 영어 이름 | `display_name_en` | `Service@en-US-DisplayName` | Id 파생 소스 |
| 웹 사이트의 대표 주소 | `url` | `Service@Url` | |
| 웹 사이트의 아이콘 | `icon_ref` | `docs/images/<Category>/<Id>.png` | CSV엔 Drive 링크(인증 필요) |
| 웹 사이트의 종류 | `category_ko` → `category` | `Service@Category` | 아래 2절 |
| 보안 플러그인 …URL(한 줄씩) | `package_urls[]` | `<Package Url=…>` | Name·Arguments는 미수집 |
| …Windows Sandbox 창이 닫히나요 | `compat_notes` | `CompatNotes` | AST 관련은 생략 가능 |
| 플러그인 사용 메뉴/위치 | `usage_notes` | (판단용 메타) | 개별 사이트 vs 다른 형태 판단 |
| 개인 정보 수집 동의 | `consent` | 게이트 | 비면 `consent-missing` |
| 없거나 반영되지…먼저 확인 | `dedup_ack` | 게이트 | 비면 `dedup-missing` |

> 헤더는 **부분 문자열 키워드**로 매칭하므로 폼 문구가 조금 바뀌어도 견딘다.
> 매칭 안 된 열은 normalize 요약의 "매칭 안 된 CSV 열"에 출력된다 — 무시하지 말 것.

## 2. Category 매핑 (한국어 종류 → XSD enum)

XSD `CatalogInternetServiceCategory` enum:
`Other, Banking, Financing, Security, Insurance, CreditCard, Government, Education`

| 폼 선택지 | enum | 상태 |
|---|---|---|
| 은행 | `Banking` | 1:1 |
| 카드 | `CreditCard` | 1:1 |
| 공공기관 | `Government` | 1:1 |
| 금융 | `Financing` | 1:1 |
| 보험 | `Insurance` | 1:1 |
| 교육 | `Education` | 1:1 |
| **증권** | `Financing`(잠정) | ⚠️ 대응 enum 없음. **운영자 결정(2026-07): 잠정적으로 `Financing`**. 향후 XSD에 `Securities` 추가 시 재검토 |
| 기타 | `Other` | 1:1 |

> enum에는 있지만 폼엔 없는 `Security`(보안)는 제보로 들어올 일이 거의 없다.
> 증권은 이제 결정된 매핑이므로 `category-needs-decision` 플래그를 남기지 않는다.

## 3. 기계가 못 채우는 빈칸(gaps) — 스킬이 반드시 개입

| gap 플래그 | 의미 | 해결 방법 |
|---|---|---|
| `arguments-missing` | 폼이 사일런트 스위치를 안 받음 | 4절 시드표 → 없으면 `src/ussf.cs` 또는 설치기 `/?`·`/help` |
| `package-name-derive` | Package Name 미수집(URL만) | 4절 시드표 + `check_urls.py` 파일명 |
| `category-needs-decision` | 증권 등 enum 없는 종류 | 사람 결정 |
| `id-collision` | 기존 Service/Companion Id와 충돌 | 이름 조정 or 기존 항목 수정으로 처리 |
| `consent-missing` / `dedup-missing` | 게이트 미동의 | 스팸/오제출 의심 → 처리 보류 후 확인 |

## 4. 설치 프로그램 시드표 (Catalog.xml 실측 빈도 기반)

URL 파일명 → **Package Name** 및 **Arguments(사일런트 스위치)** 추론용. 신규 URL이
아래 파일명 패턴과 맞으면 그대로 쓰고, 아니면 `detect_switch.py`(설치기 다운로드 → `ussfc`
탐지)로 조사한다. (스위치는 벤더마다 다르니 `/silent`로 넘겨짚지 말 것 — 특히 TouchEn 계열은
`/silence`, IPInside는 `/nodlg`.)

| 파일명 패턴(예) | Package Name | Arguments |
|---|---|---|
| `veraport-g3*`, `veraport*` | `Veraport` | `/silent` |
| `nos_setup*` | `nProtect Online Security` | `/silent` |
| `astx_setup*`, `astxdn*` | `AhnLabSafeTx` | `/silent` |
| `delfino-g3*`, `delfino*` | `Delfino G3` | `/silent` |
| `AnySign_Installer*`, `AnySign*` | `AnySign` | `/S` |
| `TouchEn_nxKey*32*` | `TouchEnKey32` | `/silence` |
| `TouchEn_nxKey*64*` | `TouchEnKey64` | `/silence` |
| `INIS_EX*`, `INISAFE*CrossWeb*` | `INISAFECrossWeb` | `/S` |
| `I3GSvcManager*`, `IPinside*` | `IPInside` | `/nodlg` |
| `ePageSafer*`, `ePageSAFER*` | `ePageSafer` | `/silent` |
| `UniSign*` | `UniSign` | `/silent` |
| `iSASService*` | `iSASService` | `/silent` |
| `MagicLine4NX*` | `MagicLine4NX` | `/silent` |
| `SecuKit*` | `SecuKitNXS` | `/silent` |

> 이 표는 힌트다. 최종 확인은 실제 설치기 동작(무인 설치 테스트) 기준.
> 새 벤더/버전이면 `detect_switch.py <url>`(내부적으로 [`ussfc`](https://www.nuget.org/packages/ussfc))로
> 스위치를 탐색하고 이 표를 갱신하는 것을 권장. 단, `ussfc`의 일반 EXE 판별은 국내 보안 설치기에서
> 틀릴 수 있으므로(예: Veraport를 Inno로 보고 `/VERYSILENT` 제안) **시드표 값이 있으면 우선**한다.

## 5. Id 파생 규칙

영어 이름 → PascalCase. 이미 대문자 2자 이상인 토큰(약어: KB, KEB, NH)은 원형 보존.
예: `Woori Bank`→`WooriBank`, `KB Kookmin Bank`→`KBKookminBank`.
파생 후 **반드시** 기존 `Service/Companion Id`와 충돌 검사(`normalize_csv.py --catalog`).
