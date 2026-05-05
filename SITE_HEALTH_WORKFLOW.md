# 사이트 헬스 체크 워크플로

이 문서는 카탈로그에 등록된 웹 사이트들의 접속 가능성을 주기적으로 점검하고, AI 어시스트(Claude Code 등)와 함께 카탈로그를 갱신하는 절차를 설명합니다.

## 전체 흐름

```text
[1] probe   (자동, 헤드리스)       →   report.json + 스크린샷
        │
        ▼
[2] triage  (대화형, 사람 판정)    →   triage.json (사람 verdict 포함)
        │
        ▼
[3] issue   (gh CLI 자동화)        →   GitHub 이슈 등록 (verdict별 1건씩)
        │
        ▼
[4] AI 어시스트 (Claude Code 등)   →   Catalog.xml / sites.xml 패치 후보
        │
        ▼
[5] 본인 검토 + commit/PR (이슈 닫기)
        │
        ▼
[6] 변경분 재검증 (probe --only)   →   다음 라운드까지 stale 제거
```

자동화는 **검출(detect)**까지만 — "이 사이트가 정상이다"의 최종 판정은 사람이 합니다. 휴리스틱은 우선순위를 매기는 도구이지 판결문이 아닙니다.

## 1. probe — 자동 헬스 체크

```bash
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/
```

각 카탈로그 항목에 대해 Microsoft Edge(헤드리스, Windows x64로 가장)로 접속을 시도하고 다음을 캡처합니다. 식탁보가 실제로 동작하는 Edge IE Mode 환경과 일치시키기 위해 User-Agent, Client Hints 헤더(`sec-ch-ua-*`), `navigator.platform`, `navigator.userAgentData`를 모두 Windows Edge 값으로 설정합니다.

- 최종 URL (리다이렉트 후), HTTP status, 페이지 타이틀, 에러
- 뷰포트 스크린샷 (1280×800)

결과는 `health-report/<UTC-timestamp>/`에 저장됩니다.

```text
health-report/2026-05-06T010203Z/
├── report.json          # 머신리더블 결과 (probe 단계)
├── report.md            # 사람이 읽기 쉬운 요약
└── screenshots/
    ├── WooriBank.png
    └── ...
```

### 분류 (3-tier)

| Tier | 의미 | 트리거 (strong signals) |
|------|------|--------|
| `AutoOk` | 자동으로 정상 추정 | strong signal 없음 |
| `NeedsReview` | 사람 검토 필요 | 4xx/5xx, 도메인 변경 redirect, 종료 키워드(`서비스 종료`, `통합되었습니다`, `이전되었습니다` 등), timeout/SSL/connection-reset 등 |
| `AutoDead` | 사망 추정 | DNS 실패, connection refused |

타이틀이 비어있는 것(`empty-title`)은 weak signal로 기록만 되며 단독으론 NeedsReview를 트리거하지 않습니다 — 한국 공공/금융 사이트는 JS 렌더링이 끝난 뒤에 타이틀을 채우는 경우가 많아 false positive가 너무 많기 때문입니다. (probe는 빈 타이틀일 때 1.5초 대기 후 재조회로 자동 보정합니다.)

`AutoOk`도 "사람이 봐도 정상"이 아니라 "자동 검사로 의심점이 없음"입니다. 의심되면 `triage --all`로 전수 검토 가능.

### 자주 쓰는 옵션

```bash
# 일부만 재검증 (예: 방금 수정한 항목만)
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --only WooriBank,KookminBank

# 동시성/타임아웃 조정
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --concurrency 8 --timeout-ms 45000
```

## 2. triage — 대화형 사람 판정

```bash
dotnet run --file src/checksites.cs -- triage ./health-report/2026-05-06T010203Z/
```

`NeedsReview` 항목을 한 건씩 보여주며, 자동으로 시스템 브라우저에 라이브 URL을 띄워 직접 클릭 확인할 수 있게 해줍니다. 본인의 쿠키·인증서·확장 프로그램이 그대로 사용되므로 한국 공공/금융 사이트의 인증 흐름까지 검증할 수 있습니다.

각 항목마다 verdict를 입력합니다:

| 키 | 의미 | 추가 입력 |
|----|------|----------|
| `o` | healthy — 사이트 정상 | (메모만) |
| `d` | dead — 사이트 폐쇄 | (메모만) |
| `c` | url-changed — URL이 바뀜 | 새 URL + 메모 |
| `m` | merged-or-renamed — 기관 통폐합/명칭변경 | 후속 기관 Id(선택) + 메모 |
| `u` | unsure — 추가 조사 필요 | (메모) |
| `s` | skip — 이번엔 건너뜀 | (저장 안 함, 다음 실행 시 재출현) |
| `q` | quit — 즉시 종료 (지금까지 저장은 보존) | — |

verdict는 입력 즉시 `triage.json`에 저장되어 도중에 끊어도 다음 실행 시 이어서 진행됩니다.

### triage 옵션

```bash
# AutoOk까지 모두 전수 검토
dotnet run --file src/checksites.cs -- triage <report-dir> --all

# 시스템 브라우저 자동 오픈 비활성화 (URL만 출력)
dotnet run --file src/checksites.cs -- triage <report-dir> --no-browser
```

## 3. issue — GitHub 이슈 자동 등록

triage 결과에서 후속 작업이 필요한 verdict들을 GitHub 이슈로 자동 등록합니다. `gh` CLI가 설치·로그인되어 있어야 합니다.

```bash
dotnet run --file src/checksites.cs -- issue ./health-report/2026-05-06T010203Z/
```

이슈가 생성되는 verdict:

- `dead` — 사이트 폐쇄 의심
- `merged-or-renamed` — 기관 통폐합/명칭변경 의심
- `unsure` — 추가 조사 필요
- `url-changed` — **도메인이 변경된 경우만** (같은 도메인 내 URL 변경은 단순 PR로 충분하므로 이슈를 만들지 않습니다)

`healthy`나 verdict가 없는 항목은 이슈가 만들어지지 않습니다.

이슈 본문은 자기완결적입니다 — 카탈로그 항목 메타정보, 사람 verdict + 메모, 권장 조치, ProbedAt/스크린샷 경로 등 후속 처리에 필요한 모든 컨텍스트가 포함되어 PR 작성 시 따로 설명을 짜낼 필요가 없습니다.

이슈 등록 결과(`IssueUrl`)는 `triage.json`의 해당 verdict에 다시 저장되므로, **여러 번 실행해도 같은 항목을 중복 등록하지 않습니다**.

### issue 옵션

```bash
# 미리보기 (실제 이슈 안 만듦, 본문/타이틀만 출력)
dotnet run --file src/checksites.cs -- issue <report-dir> --dry-run

# 다른 리포에 등록
dotnet run --file src/checksites.cs -- issue <report-dir> --repo yourtablecloth/TableClothCatalog

# 라벨 부여 (라벨이 리포에 미리 존재해야 함)
dotnet run --file src/checksites.cs -- issue <report-dir> --label site-health
```

## 4. AI 어시스트 — Claude Code와 카탈로그 갱신

issue까지 끝나면 `triage.json`이 사람 verdict + 등록된 이슈 URL을 포함한 자기완결 입력이 됩니다. Claude Code에 다음을 전달하세요.

### 진입 프롬프트 (복사해서 사용)

```text
triage.json 경로: <여기에 본인 경로 채우기>

이 파일을 읽고 verdict별로 다음 규칙대로 처리해줘.

verdict별 처리 규칙:
- url-changed:
  - newUrl과 originalUrl이 같은 등록 가능 도메인(eTLD+1)에 속하면 docs/Catalog.xml(또는 sites.xml)의 해당 항목 Url 속성을 newUrl로 자동 패치.
  - 도메인이 다르면 자동 적용하지 말고 먼저 보고 → 본인 승인 후 적용.
- merged-or-renamed:
  - 기관 조사 결과(공식 발표·뉴스 출처 URL 포함)를 보고 → 처리 방향(URL 교체/항목 삭제/항목 분할/새 항목 추가) 제안 → 본인 승인 후 적용.
- dead:
  - 항목 삭제 후보로 보고. 자동 삭제 금지. 삭제 결정 시 commit 메시지에 사유 명시.
- unsure:
  - 추가로 확인이 필요한 정보(예: 정확한 후속 도메인, 인수합병 시점)를 정리해 알려줘.
- healthy / pending(verdict 없음):
  - 건드리지 않음.

작업 단위로 commit 분리:
  (1) 단순 URL 갱신 (같은 도메인)
  (2) 기관 변경/통폐합
  (3) 삭제

각 항목에 IssueUrl이 있으면 commit/PR 메시지에 `Closes <IssueUrl>` 형식으로 언급해 자동으로 이슈가 닫히게 해줘.

편집 후:
  1. dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/ 로 스키마 검증 통과 확인
  2. 변경된 Service Id 목록을 모아 다음을 실행:
     dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --only <id1,id2,...>
  3. 새 report에서 Tier가 NeedsReview/AutoDead로 떨어지지 않는지 확인 후 보고.
```

### 자동 vs 확인-먼저 경계선

| 작업 | 자동 패치 | 본인 승인 필요 |
|------|:---:|:---:|
| `Url=` 한 속성 변경 (같은 도메인) | ✓ | |
| 도메인이 바뀌는 URL 교체 | | ✓ |
| `Service` 항목 삭제 | | ✓ |
| `Service Id` 변경 | | ✓ |
| 새 항목 추가 / 항목 분할 | | ✓ |
| `Packages`·`CompatNotes` 등 부속 필드 수정 | | ✓ |

규칙: **diff가 한 줄이고 의미가 보존되는 변경**만 자동, 그 외는 보고 후 결정.

## 5. 대화 위생

- **triage 한 번 = 대화 한 번**이 이상적. 여러 라운드를 한 대화에 끌고 가면 stale verdict가 컨텍스트에 쌓입니다.
- 시작할 때 triage.json의 절대 경로와 이번 작업 범위(`url-changed만`, `m verdict만 같이 검토` 등)를 한 줄로 명시.
- 큰 변경은 commit 단위로 끊어서 — 되돌리기 쉬워집니다.

## 6. 재검증 루프

편집을 마친 뒤 변경된 항목만 재-probe해서 상태가 회복되었는지 확인합니다.

```bash
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --only WooriBank,KookminBank
```

새 report의 Tier가 모두 `AutoOk`이면 라운드 종료. 여전히 `NeedsReview`/`AutoDead`로 떨어지면 다시 triage로.

## triage.json 스키마 (참고)

```jsonc
{
  "SchemaVersion": 1,
  "CatalogRoot": "/abs/path/to/docs/",
  "ProbedAt": "2026-05-06T01:02:03Z",
  "UpdatedAt": "2026-05-06T01:30:00Z",
  "Entries": [
    {
      "Probe": {
        "Id": "WooriBank",
        "DisplayName": "우리은행 개인뱅킹",
        "Category": "Banking",
        "Source": "Catalog.xml",
        "Url": "https://www.wooribank.com/",
        "FinalUrl": "https://www.wooribank.com/",
        "HttpStatus": 200,
        "Title": "우리은행",
        "Error": null,
        "ScreenshotPath": "screenshots/WooriBank.png",
        "Tier": "AutoOk",
        "Signals": [],
        "ProbedAt": "2026-05-06T01:02:03Z"
      },
      "Verdict": {
        "Kind": "healthy",        // healthy | dead | url-changed | merged-or-renamed | unsure
        "NewUrl": null,           // url-changed일 때 채워짐
        "SuccessorId": null,      // merged-or-renamed일 때 선택적
        "Notes": null,
        "VerdictAt": "2026-05-06T01:30:00Z",
        "IssueUrl": null          // issue 단계 후 채워짐 (멱등성에 사용)
      }
    }
  ]
}
```

`Verdict`가 `null`이면 아직 사람 판정이 없는 항목입니다. AI는 verdict가 채워진 항목만 처리해야 합니다.
