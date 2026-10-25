# 카탈로그 최신화를 보조하는 품질 관리 도구

2026년 10월 25일 기준으로 이 저장소는 카탈로그 품질 리포트와 금융기관 통폐합 조사 이슈를 만드는 워크플로를 제공합니다. 두 워크플로는 주간 실행 설정을 주석 처리한 상태이며 수동 실행만 지원합니다.

이 문서는 브라우저를 활용한 최신화와 품질 검사의 관계, 워크플로 실행 조건, 검사 결과의 추적 방식, 금융기관 조사 지원 범위, 검증 및 배포 빌더의 동작을 다룹니다.

기본 최신화 절차를 먼저 설명한 뒤 필요에 따라 선택할 수 있는 보조 도구와 현재 실행 조건을 살펴보겠습니다.

## 1. 브라우저 조사를 기준으로 한 최신화

카탈로그 최신화에는 Aside 등 브라우저와 AI를 결합한 에이전트형 웹 브라우저를 활용하는 방식을 권장합니다. 공식 안내 페이지를 조사하고 XML 변경안을 작성한 뒤 검증과 사람의 PR 검토를 거치는 절차는 [사이트 확인 및 최신화 안내](SITE_HEALTH_WORKFLOW.md)에서 정리합니다.

이어서 품질 검사 도구의 역할을 짚어보겠습니다. 도구는 이미지 누락, 스키마 오류, URL 응답 등을 확인하는 데 활용할 수 있습니다. 공식 페이지에서 확인한 변경의 의미와 반영 범위는 기여자와 검토자가 판단합니다. 출처와 확인일을 기록하는 방법은 [기여 안내](../CONTRIBUTING.md)를 따릅니다.

## 2. 수동 실행 상태와 Copilot 배정 조건

두 워크플로의 현재 트리거와 배정 설정은 다음과 같습니다. 실행 조건은 각 워크플로 파일의 `on`과 이슈 처리 단계에서 확인할 수 있습니다.

| 워크플로 | 현재 트리거 | `assign_copilot` 동작 |
| --- | --- | --- |
| [catalog-quality.yml](../.github/workflows/catalog-quality.yml) | `workflow_dispatch`, 주간 `schedule` 주석 상태 | 기본값 `true`, 문제가 있는 리포트에 대해 값이 `false`가 아니면 Copilot 배정 시도 |
| [financial-merger-watch.yml](../.github/workflows/financial-merger-watch.yml) | `workflow_dispatch`, 주간 `schedule` 주석 상태 | 기본값 `true`, 값이 `false`가 아니면 조사 이슈에 Copilot 배정 시도 |

다만 품질 리포트에서 문제가 사라졌고 기존 리포트 이슈가 열려 있으면 워크플로는 `assign_copilot` 값과 관계없이 Copilot 배정을 시도합니다. 이 단계는 `close-requested` 라벨과 검증 후 종료를 요청하는 코멘트를 추가하며 이슈를 직접 닫지는 않습니다. 배정 성공 여부는 실행 시의 계정 권한과 서비스 이용 조건에 따라 달라집니다.

## 3. 단일 품질 리포트의 생성과 추적

[catalog-quality.yml](../.github/workflows/catalog-quality.yml)은 [checkimages.cs](../src/checkimages.cs)로 이미지 누락과 미사용 이미지를 검사하고 [catalogutil.cs](../src/catalogutil.cs)로 스키마와 URL 응답을 검사합니다. 두 도구는 워크플로가 지정한 환경 변수에 따라 JSON 결과도 출력합니다.

리포트 생성 단계에서는 [qualityreport.cs](../src/qualityreport.cs)가 검사 결과를 이전 상태와 병합해 신규 문제, 지속 중인 문제, 해결된 문제를 구분합니다. 워크플로는 변경이 있으면 `quality/report.md`와 `quality/state.json`을 커밋하고 `catalog-quality` 라벨의 단일 리포트 이슈를 생성하거나 갱신합니다. 문제가 다시 발생하면 닫힌 리포트 이슈를 재개합니다.

URL 접속 경고는 참고 항목으로 집계하며 리포트 이슈의 문제 유무 판단에는 포함하지 않습니다. 일시적인 HTTP 오류나 검사 결과만으로 서비스 종료나 항목 삭제를 결정하지 않습니다. 변경의 근거와 반영 여부는 [카탈로그 품질 및 갱신 방침](CATALOG_POLICY.md)에 따라 검토합니다.

## 4. 금융기관 조사 이슈의 지원 범위

[financial-merger-watch.yml](../.github/workflows/financial-merger-watch.yml)은 이름에 `저축은행`이 포함된 항목과 `Category="Insurance"`인 항목을 카탈로그에서 추출합니다. 워크플로는 이 목록을 담은 조사 이슈를 생성하거나 기존의 열린 `financial-merger-watch` 이슈를 갱신합니다.

공식 발표 검색과 통폐합 여부 판단은 브라우저를 사용하는 AI 에이전트나 사람이 수행합니다. 조사 결과에는 공식 출처 URL과 확인한 시점을 남깁니다. 출처를 열 수 없거나 사실관계를 확인하지 못하면 추정하지 않고 미확인 상태로 기록합니다. 항목 삭제, 식별자 변경, 통폐합 반영은 변경 근거와 함께 PR에서 검토합니다. 공개 자료의 수집 범위는 [데이터 경계 및 접근 원칙](DATA_BOUNDARY.md)을 따릅니다.

## 5. 변경안 검증과 배포 빌더의 URL 조회

XML 변경안을 작성한 뒤에는 [checkimages.cs](../src/checkimages.cs)와 [catalogutil.cs](../src/catalogutil.cs)로 이미지와 스키마를 검증할 수 있습니다. 저장소 루트에서 다음 명령을 실행합니다.

```bash
dotnet run --file src/checkimages.cs -- ./docs/Catalog.xml ./docs/images
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
```

또한 [publish.yml](../.github/workflows/publish.yml)은 `main` 브랜치의 `docs/`, `src/`, `.github/workflows/` 변경을 감지하면 배포 빌더를 실행합니다. 이때 호출하는 `catalogutil.cs`도 URL 응답을 조회합니다. 따라서 품질 리포트의 주간 실행을 중단한 상태에서도 배포 빌더의 URL 조회는 실행됩니다. URL 응답과 XML 검증 결과만으로 Windows Sandbox에서 설치나 서비스 이용까지 검증했다고 판단하지 않습니다.

## 6. 조사 범위에 따른 보조 도구 선택

여기까지 정리하면 품질 관리 도구는 브라우저 조사와 PR 검토를 보조합니다. 개별 사이트나 설치 경로를 갱신할 때에는 [사이트 확인 및 최신화 안내](SITE_HEALTH_WORKFLOW.md)에 따라 공식 페이지와 변경안을 확인할 수 있습니다. 여러 항목의 구조 및 이미지 문제를 함께 추적할 때에는 품질 리포트를 활용할 수 있습니다.

현재 최신화 작업은 수동 조사와 검증으로 진행할 수 있으며 주간 워크플로 재개 여부는 향후 운영 조건에 따라 결정합니다. 도구의 실행 주기나 검사 통과가 갱신 시한 또는 최신성을 보증하지는 않습니다. 갱신과 정보 처리의 범위는 [카탈로그 품질 및 갱신 방침](CATALOG_POLICY.md)에서 확인할 수 있습니다.
