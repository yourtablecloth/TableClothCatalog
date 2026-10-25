# AppContainer 스킬 평가 결과

[English](APP_CONTAINER_POC.md) | [스킬 등재 기준](SKILL_ELIGIBILITY.ko.md) | [카탈로그 관리 안내](SkillCatalog.README.ko.md)

2026년 10월 25일에 AppContainer PoC는 Windows 빌드 26300 x64에서 중계기, 실행과 정책 검사 46개를 모두 통과했습니다. 별도의 Codex 0.156.1 검증은 고정된 로컬 Responses 응답 2회를 사용해 MCP와 AppContainer를 거친 계산기 호출을 완료했습니다. 실제 모델 추론과 인증은 수행하지 않았습니다.

고정 스킬의 어댑터, 격리 실행, 공개 서비스 표본 조회, CLI 도구 연결과 보존한 증거를 설명합니다. 이번 결과는 평가 분기에 한정하며 운영 호환성 통과로 승격하지 않습니다.

분기와 원본 범위부터 살펴보겠습니다. 이후 작업 프로세스의 제한, 측정 결과, Codex 연결, 증거 기록과 남은 검증을 순서대로 정리하겠습니다.

## 평가 분기와 고정 원본의 범위

`poc/appcontainer-skill-evaluation` 카탈로그는 기존 운영 후보 3개에 이번 실험의 스크립트 후보 3개를 추가해 총 6개를 활성화합니다. 나머지 블록 9개는 주석을 유지합니다. 식탁보 구현은 `poc/appcontainer-skill-runner` 분기를 사용합니다. 운영 대화는 `shell_tool=false`와 `unified_exec=false`를 유지하며 이번 중계기를 자동 등록하지 않습니다. [카탈로그 등재 기준](SKILL_ELIGIBILITY.ko.md)과 [TableCloth #321](https://github.com/yourtablecloth/TableCloth/issues/321)의 자격 증명 접수 보류 정책도 계속 적용합니다.

| 평가 스크립트 | 고정 원본 커밋 | 실행 경로의 변경 |
| --- | --- | --- |
| [BOK ECOS](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | 원본 `scripts/bok_ecos.py` 직접 호출 |
| [KOSIS](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | 원본 `scripts/run_kosis_stats.py` 직접 호출 |
| [금융 및 생활 계산기](https://github.com/yourtablecloth/tablecloth-calculators/tree/877b58cf0741db21342445f2b3fcb54ad1ada85c) | `877b58cf0741db21342445f2b3fcb54ad1ada85c` | 원본 `scripts/calculate.py`에 stdin JSON 전달 |

BOK와 KOSIS의 `SKILL.md`는 `npx` 지침 로더를 필수 단계로 안내합니다. 이번 실험에서는 고정 지침을 읽고 원본 Python 보조 스크립트를 수정 없이 직접 호출했습니다. 원래의 npm 로더 절차를 실행하거나 검증하지 않았습니다. Node.js는 고정한 로컬 fixture로 검사했으며 원본 npm 패키지를 실행하지 않았습니다. 구현 저장소의 `tools/appcontainer-skill-poc/skills/ORIGINS.json`은 원본 라이선스와 함께 보존한 파일 31개의 고정 Git blob 일치를 기록합니다.

## 호스트 중계기와 작업 프로세스의 제한

호스트 소유 MCP 중계기는 등록한 ID와 크기를 제한한 JSON 입력만 받습니다. 모델이 지정한 실행 경로, 셸 명령이나 소스 코드는 받지 않습니다. 실행 직전에 인터프리터와 스크립트의 해시를 다시 검사합니다. Python은 `-I -S -B -X utf8`을 사용하며 Node fixture는 고정 진입 파일을 사용합니다. 네이티브 실행기는 AppContainer 토큰을 확인하고 소유 디렉터리의 ACL과 Job Object로 실행 시간, 메모리, 프로세스 수와 출력을 제한합니다. Microsoft의 [AppContainer 격리 문서](https://learn.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation)에서 기반 자원 접근 모델을 설명합니다.

일반 중계기는 네트워크 capability를 부여하지 않으며 네트워크가 필요한 등록을 거절합니다. 검증한 작업 프로세스는 소유한 임시 홈과 작업 디렉터리 및 비운 환경을 사용했습니다. 비공개 합성 파일 접근 거부, 상속 값 제외, 자식 프로세스 제한, UTF-8 왕복 처리, 취소, 시간과 메모리 및 출력 제한, 표본 ACL 원복과 실행별 작업 디렉터리 제거를 통과했습니다. 일반 AppContainer는 호스트 커널을 공유하며 ACL이 애플리케이션 패키지에 이미 허용한 호스트 자원은 접근할 수 있습니다. 이번 검사만으로 VM 수준의 경계나 모든 호스트 파일의 읽기 차단을 입증하지 않습니다.

별도 공개 API 평가기는 호스트가 고정한 명령에 범용 `internetClient` capability를 부여했습니다. 도메인 허용 목록은 적용하지 않았습니다. 서비스 조회 성공만으로 모델이 임의 URL을 선택하는 네트워크 정책까지 검증하지는 않습니다. 작업 프로세스 검사나 카탈로그 활성화로 일반 셸, npm 설치나 임의 패키지 갱신을 허용하지 않습니다.

## 실행 검사와 공개 서비스의 측정 결과

최종 평가 기록은 최상위 검사 수와 그 안에서 실행한 단위 테스트 수를 구분합니다.

| 검증 | 관찰한 결과 | 범위 |
| --- | --- | --- |
| 오프라인 중계기 검사 | 20개 전체 통과 | 입력, 등록, 해시, 전송과 실패 처리 |
| 실행과 정책 검사 | 26개 전체 통과 | AppContainer의 Python과 Node, 대조군, 제한, 정리와 공개 서비스 2개 |
| 통합 평가 | 46개 전체 통과 | 앞선 20개와 26개의 합계 |
| 계산기 원본 단위 테스트 | 28개 전체 통과 | AppContainer에서 실행하며 최상위 26개 중 한 검사에 포함 |
| BOK 원본 단위 테스트 | 19개 전체 통과 | AppContainer에서 실행하며 최상위 26개 중 한 검사에 포함 |
| ManagedAi Release 회귀 검사 | 197개 전체 통과 | 별도로 다시 실행한 앱 회귀 검사 |

계산기 사례는 할인, 퍼센트, 윤년 날짜 보정, 고정 시차의 날짜 경계 이동, BMI 수치와 금리 0% 대출을 포함했습니다. KOSIS의 고정 dry-run과 Node JSON fixture도 통과했습니다. 이 표본은 계산기 전체 연산이나 원래의 KOSIS 단위 테스트 전체를 포함하지 않습니다.

실제 서비스 표본에서 [BOK 보조 스크립트](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats/scripts/bok_ecos.py)는 공개 `sample` 키로 ECOS 결과 1행을 받았습니다. [KOSIS 보조 스크립트](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats/scripts/run_kosis_stats.py)는 공유 `k-skill-proxy.nomadamas.org`를 통해 `인구` 검색 결과 1행을 받았습니다. 사용자 API 키는 전달하지 않았습니다. 프록시 접근성과 서버 측 자격 증명은 외부 서비스에 의존합니다. KOSIS 직접 호출과 대용량 조회의 자격 증명 경로는 검사하지 않았습니다.

## 고정된 로컬 응답을 사용한 실제 Codex 연결

고정 CLI는 계산기를 namespace `mcp__tablecloth_skill_poc`와 정규화한 함수 이름 `tablecloth_calculators`로 광고했습니다. 로컬 HTTP 수신기는 고정 함수 호출 하나를 반환하고 실제 도구 출력을 받은 뒤 최종 응답을 보냈습니다. CLI의 MCP 완료 이벤트는 원래 도구 이름 `tablecloth-calculators`를 기록했습니다. 작업 프로세스는 `wasAppContainer=true`, 종료 코드 0과 할인 결과 `80.00`을 반환했습니다.

격리 프로필은 `approval_policy="never"`, `shell_tool=false`와 `unified_exec=false`를 유지했습니다. 호스트 소유의 등록한 계산기 한 개에만 도구별 `approval_mode="approve"`를 명시했습니다. 서버 기본값은 `prompt`이며 활성 도구 목록도 해당 계산기 한 개로 제한했습니다. 허위 읽기 전용 annotation은 사용하지 않았습니다. OpenAI의 [MCP 설정 안내](https://developers.openai.com/codex/mcp)에서 도구별 승인 제어를 설명하며 [함수 호출 안내](https://developers.openai.com/api/docs/guides/function-calling#defining-namespaces)에서 namespace 구성을 설명합니다.

이번 실행은 loopback Responses 요청 2회, 실제 모델 추론 0회와 인증 시도 0회를 기록했습니다. 기존 인증을 복사하지 않았으며 새 Codex 홈에 인증 파일을 만들지 않았습니다. 설정, manifest와 계산기 해시도 유지했습니다. 미리 정한 응답으로 실제 CLI와 중계기의 연결 및 격리 실행을 검증한 범위로 한정합니다. 실제 모델의 도구 선택과 운영 식탁보 대화의 자동 실행은 미검증으로 남깁니다.

## 증거 기록과 플랫폼 검증 범위

아래 기록은 로컬 식탁보 작업 공간의 `Artifacts/appcontainer-skill-poc/` 아래에 보존합니다. 카탈로그 게시 자산에는 포함하지 않습니다. 구현과 재현 안내는 식탁보 구현 분기의 `tools/appcontainer-skill-poc/`에서 관리합니다.

| 기록 | SHA-256 | 기록 범위 |
| --- | --- | --- |
| `evaluation-public-api.json` | `5489b04ad12cbce500cfab5e41b904d113a00206e5fccd9829bce835fa48fdfa` | 실제 서비스 표본 2개를 포함한 최종 46개 검사 |
| `codex-probe-evidence.json` | `4c283d01b2d355407c0af708c8b389b1b9d44b024f82b8e3590aa8c57f06daeb` | 고정된 로컬 응답을 사용한 고정 CLI 도구 연결 성공 |

실측 호스트는 Windows NT `10.0.26300.0`, x64를 보고합니다. Python 3.14.9와 Node.js 24.22.1을 준비해 사용했습니다. Windows 10과 ARM64는 실측하지 않았으며 네이티브 API의 제공 여부만으로 호환성을 판단하지 않습니다. 기존 [스킬 8개 설치 감사](skill-eligibility-audit-2026-10-25.json)는 변경하지 않았으며 당시의 설치와 발견 범위 및 ACL 제한을 유지합니다. 이전 fixture 519개를 새로운 AppContainer 실행 검사로 다시 합산하지 않습니다.

## 현재 판단과 남은 검증

여기까지 정리하면 이번 PoC는 실측 호스트에서 제한한 스크립트 실행과 고정 응답을 사용한 CLI 및 MCP 연결을 확인했습니다. [등재 기준](SKILL_ELIGIBILITY.ko.md)은 이 근거와 운영 통합, 실제 모델 동작 및 플랫폼 확장을 구분합니다. 이 분기의 활성 항목 6개는 평가 범위를 유지하며 MCP 의존 항목과 자격 증명 의존 후보의 보류도 유지합니다.

운영 적용은 앱 통합, 네트워크 정책, 취소와 정리 동작의 검토, 대상 플랫폼 검증과 별도로 허용한 실제 모델 검사를 전제로 판단합니다. 당장의 결과로 고정 스크립트 3개의 추가 평가를 이어갈 수 있습니다. 일반 셸을 활성화하거나 런타임 설치만으로 출시한 대화의 스크립트 실행을 지원한다고 표시하지 않습니다.
