---
name: catalog-intake
description: >-
  Google Forms(식탁보 사이트 추가/수정/삭제 요청) 응답 CSV를 읽어 docs/Catalog.xml 등록을
  돕는다. 헤더를 정규화하고, 브라우저(Edge)로 위장해 보안 플러그인 URL 생존/파일명을 확인하고,
  공식 로고를 투명 정사각 PNG로 만들고, catalogutil.cs 검증을 거쳐 사람이 검토할 PR 후보를
  만든다. "제보 CSV 처리", "폼 응답 등록", "catalog intake" 요청 시 사용.
---

# 카탈로그 제보 인테이크 (catalog-intake)

Google Forms 제보(비-GitHub 사용자용)를 `docs/Catalog.xml` 변경으로 옮기는 반자동 절차.
`docs/SITE_HEALTH_WORKFLOW.md`와 같은 철학을 따른다: **기계는 정규화·검증·후보 생성까지,
최종 판정과 반영(merge)은 사람.**

## ⚠️ 안전 경계선 (먼저 읽기)

- 이 카탈로그의 `<Package>`는 **사일런트 스위치로 무인 설치되는 보안 프로그램**이다.
  제보는 익명은 아니지만(폼 로그인 필수) **미검증 외부 입력**이다.
- 따라서 이 스킬은 **PR 후보까지만** 만든다. **자동 commit/merge 금지.** 사람이 검토 후 반영.
- `check_urls.py`·`fetch_logo.py`는 URL을 **읽기만** 한다. 설치 파일을 실행하지 않는다.

## 입력

- 필수: Google Forms 응답을 내보낸 **CSV 경로** (Sheets → 파일 → 다운로드 → CSV).
- 참고 문서: [references/field-mapping.md](references/field-mapping.md) — 필드 매핑·Category·
  gap 정의·설치기/스위치 시드표. 처리 규칙의 근거는 전부 여기에 있다.

## 사전 준비 (도구)

- **uv** (Python) — `scripts/*.py` 실행. PEP 723 자체완결이라 `uv run` 시 의존성 자동 설치.
- **[ussfc](https://www.nuget.org/packages/ussfc)** — 무인 설치 스위치 탐지(.NET 전역 도구). 최초 1회 설치:
  ```bash
  dotnet tool install -g ussfc      # 갱신: dotnet tool update -g ussfc
  ```
  설치 없이 쓰려면 `.NET 10 SDK`의 `dnx ussfc`도 가능(`detect_switch.py`가 자동 폴백).
- **gh** (GitHub CLI) — PR/이슈 생성. **dotnet 10 SDK** — `catalogutil.cs` 등 검증 도구.

## 파이프라인

```text
[0] 최신화 (필수)            main pull → 작업 브랜치 생성  ← 스킬 시작 전 반드시
      │
[1] normalize   (오프라인)   CSV → 정규화 JSON + gap 플래그
      │
[2] 게이트 확인               consent/dedup 누락 건은 보류 표시
      │
[3] 항목별 처리              빈칸 4종 채우기(아래 규칙) — URL확인/로고/스위치/분류
      │
[4] 편집                     Catalog.xml + 이미지 배치 (변경 유형별 commit 분리)
      │
[5] 검증                     catalogutil.cs 스키마 검증 + (선택) checksites.cs probe
      │
[6] PR + 미완 이슈           완전반영은 PR diff / 미완은 추적 이슈 생성 후 PR에 링크. merge는 사람.
```

### 0. 최신화 — 스킬 실행 전 반드시

제보를 처리하기 전에 **로컬 main을 원격 최신으로 맞추고 새 작업 브랜치에서 시작**한다.
stale한 베이스 위에서 편집하면 이미 반영된 항목을 중복 추가하거나 충돌 검사가 어긋난다.

```bash
git switch main
git pull --ff-only origin main          # 최신 카탈로그 확보 (fast-forward만)
git switch -c catalog-intake/<날짜-또는-배치명>   # 예: catalog-intake/2026-07-forms
```

- `--ff-only`가 실패(로컬에 이탈 커밋 존재)하면 사람에게 알리고 멈춘다 — 임의 merge/rebase 금지.
- 이 최신 main 기준으로 [1] normalize의 `--catalog docs/Catalog.xml` 충돌 검사가 정확해진다.

### 1. normalize — CSV 정규화 (오프라인)

```bash
uv run .claude/skills/catalog-intake/scripts/normalize_csv.py \
  <responses.csv> --catalog docs/Catalog.xml --out intake.json --pretty
```

각 레코드에 `change_type`, `display_name_ko/en`, `url`, `category`, `id_suggested`,
`id_collision`, `package_urls[]`, `gaps[]`가 채워진다. **요약의 "매칭 안 된 CSV 열"과
`gaps`를 먼저 확인**하고 처리 계획을 세운다.

### 2. 게이트 확인

`consent-missing` 또는 `dedup-missing`이 있으면 스팸/오제출 의심 → 그 건은 **처리 보류**로
표시하고 사람에게 확인을 요청한다. 임의로 진행하지 않는다.

### 3. 항목별 처리 — 빈칸 4종 채우기

[references/field-mapping.md](references/field-mapping.md) 3~5절을 근거로:

- **`arguments-missing` + `package-name-derive`** — 각 `package_urls`에 대해:
  1. 생존/최종 URL/파일명 확인:
     ```bash
     uv run .claude/skills/catalog-intake/scripts/check_urls.py <url> [<url> ...] --json
     ```
     `check_urls`가 403/차단이면 JS 게이트일 수 있음 → 무거운 케이스이므로
     `dotnet run --file src/checksites.cs -- probe ...`(Playwright Edge)로 폴백.
  2. **무인 설치 스위치 자동 추론** — 설치 파일을 브라우저(Edge)로 위장해 내려받아
     `ussfc`로 판별한다(사전 준비 절 참고):
     ```bash
     uv run .claude/skills/catalog-intake/scripts/detect_switch.py <installer-url>
     ```
     출력의 `▶ 후보`가 `Package Name`/`Arguments` 제안이다. 판정 규칙:
     - **시드표 매칭(source: seed)**: 알려진 벤더 → 시드표 값을 채택. `ussf`가 달라도(conflict)
       시드표 우선(국내 보안 설치기는 ussf의 일반 판별이 틀리기 쉬움. TouchEn=`/silence`, IPInside=`/nodlg`).
     - **시드표에 없음(source: ussf, needs_verification)**: ussf 추정값을 넣되, **넘겨짚지 말고**
       실제 무인 설치 테스트(Windows Sandbox/VM)로 확정할 것을 사람에게 요청.
     - 어느 쪽이든 스위치는 **자동 확정이 아니라 후보**다 — 최종은 실제 설치 테스트로 사람이 확인.

- **로고 → 투명 정사각 PNG** — CSV의 `icon_ref`는 인증이 필요한 Drive 링크이므로 보통
  사이트에서 직접 만든다:
  ```bash
  uv run .claude/skills/catalog-intake/scripts/fetch_logo.py \
    <대표 URL> docs/images/<Category>/<Id>.png --size 256 --bg auto
  # 이미 로컬로 받은 아이콘이 있으면:
  uv run .claude/skills/catalog-intake/scripts/fetch_logo.py \
    --from-image <path> docs/images/<Category>/<Id>.png --bg auto
  ```
  플랫 로고가 아니거나 배경이 복잡하면 `--bg rembg`(무거움: `uv run --with rembg …`).
  결과가 어색하면 사람에게 공식 로고 교체를 권한다.

- **`category-needs-decision`(증권 등)** — 자동 결정 금지. Other 잠정값 그대로 두고
  사람에게 "Other로 둘지 / XSD에 새 enum 추가할지" 결정을 요청.

- **`id-collision`** — 이름을 조정하거나, 사실상 기존 항목 수정이면 `change_type`을
  modify로 재해석. 새 Id를 임의로 만들지 말고 사람과 합의.

### 4. 편집 (변경 유형별 commit 분리)

`references/field-mapping.md` 1절 매핑대로 `docs/Catalog.xml`(또는 `docs/sites.xml`)을
편집하고 이미지를 배치한다. commit은 의미 단위로 분리:
`(1) 신규 추가  (2) 수정  (3) 삭제`.

### 5. 검증

```bash
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/     # 스키마/리소스 검증
dotnet run --file src/checkimages.cs -- ./docs/Catalog.xml ./docs/images   # 이미지 존재 검증
# 새로 추가/수정한 Id만 실제 접속 재검증(선택):
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --only <Id1,Id2>
```
Error/Warning이 남지 않을 때까지 정리한다.

### 6. PR 후보 제출

**모든 제보 항목은 아래 3가지 중 하나로만 분류한다. "수동 확인 필요"를 이유로 조용히
드롭하지 않는다 — 드롭하면 그 제보는 유실된다.**

| 결과 | 처리 | PR에서 |
|---|---|---|
| **완전 반영** | 검증까지 끝나 Catalog.xml에 반영 | diff에 포함 |
| **미완(수동 검토 필요)** | 자동으로 확정 못 한 항목 | **제외 금지.** PR 본문에 남기고 + **추적 이슈를 반드시 생성해 링크** |
| **중복/무효로 제외** | 기존 항목과 중복이거나 정크인 것 | 본문에 사유만 기록(진짜 제외) |

- 미완과 제외는 **다르다**: "확정 못 함"(미완)을 "제외"로 처리하면 안 된다. 제외는 중복·정크에만.

**미완 항목마다 추적 이슈를 생성한다**(유실 방지의 핵심 — PR 본문만으론 merge 후 사라진다):

```bash
gh issue create \
  --title "[제보 미완] <이름> (<도메인>) — <막힌 지점 요약>" \
  --body-file <이슈본문.md> \
  --label "help wanted" --label "enhancement"
# 이슈 본문에 담을 것: 제보 요약 / 자동으로 확인한 것 / 막힌 지점 / 사람이 해야 할 일 / 관련 PR #<n>
```

- 이슈 본문은 **자기완결**로(site-health `issue` 단계와 동일 철학) — 후속 처리에 필요한 컨텍스트를 모두 담아 PR/대화 없이도 처리 가능하게.
- 생성한 이슈 번호를 PR 본문의 해당 미완 항목 옆에 `추적: #<n>`으로 링크한다.
- PR 본문 구성: 변경 요약 · 완전 반영 목록 · **미완(수동 검토 필요) + 추적 이슈 링크** · 중복 제외 목록 · 검증 결과.
- 완전 반영 항목이 기존 제보 이슈를 닫아야 하면 `Closes #<n>`로 연결.
- **여기서 멈춘다.** merge는 사람이 한다.

## 자동 vs 사람 승인 경계 (site-health와 동일)

| 작업 | 자동 | 사람 승인 |
|---|:---:|:---:|
| CSV 정규화 / URL 생존 확인 / 로고 생성 | ✓ | |
| 같은 도메인 내 URL 한 줄 갱신 | ✓ | |
| 새 `Service` 추가 · Id 신설 | | ✓ |
| `Service` 삭제 | | ✓ |
| 증권 등 Category 결정 · XSD 변경 | | ✓ |
| 사일런트 스위치가 시드표에 없어 조사한 값 | | ✓ |
| 게이트(consent/dedup) 누락 건 반영 | | ✓ |
| commit / merge | | ✓ |

## 스크립트 요약

| 스크립트 | 런타임 | 역할 | 네트워크 |
|---|---|---|:---:|
| `normalize_csv.py` | uv(무의존) | CSV→정규화 JSON·gap 플래그·Id 파생·충돌검사 | ✗ |
| `check_urls.py` | uv(httpx) | Edge 위장 URL 생존/파일명 확인 | ✓ |
| `detect_switch.py` | uv(httpx) + ussfc | Edge 위장 설치기 다운로드→ussfc 스위치 탐지→시드 교차검증 | ✓ |
| `fetch_logo.py` | uv(pillow/numpy/bs4) | 로고 수집→배경 투명화→정사각 PNG | ✓* |

`*` `--from-image`는 네트워크 불필요. UA 프로파일은 `src/checksites.cs`의 Edge 131과 동일.
`detect_switch.py`는 `ussfc`(dotnet 전역 도구)가 필요하다 — 사전 준비 절 참고.
