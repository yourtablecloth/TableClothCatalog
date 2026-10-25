# 스킬 등재 기준과 고정 원본 검증

[English](SKILL_ELIGIBILITY.md) | [카탈로그 관리 안내](SkillCatalog.README.ko.md) | [기여 안내](../CONTRIBUTING.md)

2026년 10월 25일 기준으로 현재 식탁보 대화는 `shell_tool=false`와 `unified_exec=false`를 유지합니다. 설치한 스킬의 Python과 Node 스크립트 실행이나 MCP 서버 자동 설정을 지원하지 않습니다. 카탈로그 커밋 `12b1622da1b69ec1d24c0729bbebd70cbb19a902`의 활성 항목 8개를 검토한 운영 기준에서는 호환 후보 3개를 남기고 나머지 5개를 보류했습니다. 이 `poc/appcontainer-skill-evaluation` 분기에서만 고정 스크립트 후보 3개를 AppContainer 격리 평가용으로 다시 활성화해 총 6개를 제공합니다. 운영 대화의 스크립트 실행을 승인하지 않으며 MCP 의존 항목 2개와 자격 증명이 필요한 후보의 주석은 유지합니다.

등재 조건, 고정 버전별 판단, 실제 설치와 발견 결과, 앱 회귀 검사와 재검증 절차를 설명합니다. 패키지 처리의 성공과 필수 기능의 실행 지원을 구분합니다.

등재 기준부터 살펴보겠습니다. 이후 고정 원본 8개와 확보한 근거를 대조하고 항목 편집 및 카탈로그 검사 절차를 정리하겠습니다. 실제 서비스와 모델을 통한 기능 실행은 이번 검증에서 제외합니다.

## 필수 실행 경로를 기준으로 한 등재

PoC 예외는 아래 표의 BOK, KOSIS와 계산기 고정 커밋에만 적용합니다. `testskills.cs`의 허용 목록은 이 분기의 평가를 허용하며 카탈로그 검사 성공만으로 AppContainer 격리, 서비스 접근이나 AI 대화의 도구 실행을 입증하지 않습니다. [AppContainer 별도 평가 보고서](APP_CONTAINER_POC.ko.md)는 검사 46개 통과와 고정 로컬 응답을 사용한 실제 CLI 및 MCP 연결을 기록합니다. 실제 모델의 도구 선택이나 운영 통합까지 입증하지는 않습니다. 이전 원본 8개 감사와 앱 검사 519개는 당시의 범위를 유지하는 기존 근거로 구분합니다.

기존 [원본과 설치 조건](SkillCatalog.README.ko.md)을 계속 적용합니다. 공개 GitHub 저장소, 40자리 고정 커밋, 유효한 `SKILL.md`와 같은 커밋의 라이선스 보존을 전제로 실행 조건을 함께 검토합니다.

활성 항목은 대화에 제공한 자료를 다루는 지침이나 기존 식탁보 호스트 브리지로 핵심 기능을 제공합니다. 고정 원본, 라이선스와 필수 의존성을 함께 검토합니다. `SKILL.md`를 읽거나 파일을 내려받았다는 사실과 모델의 그럴듯한 답변만으로 기능 실행의 성공을 판단하지 않습니다. Python이나 Node 런타임을 설치해도 현재 AI 대화에 실행 도구를 등록하지는 않습니다.

스킬의 필수 절차에 Python, Node, shell이나 `npx` 실행이 있으면 현재 대화와 호환되지 않습니다. 설치 폴더에 스크립트가 없어도 필요한 MCP 서버와 도구를 연결하지 않았다면 같은 기준으로 보류합니다. 호스트 브리지 스킬은 호스트가 제공하는 제한된 보고서를 해석하며 일반 셸 실행 권한을 얻지 않습니다. 보고서가 없거나 로컬 Windows 기능을 지원하지 않는 경우에는 그 제한을 설명합니다.

자격 증명 정책도 함께 적용합니다. [TableCloth #321](https://github.com/yourtablecloth/TableCloth/issues/321)의 대응 기능 개발과 검증을 마칠 때까지 환경 변수로 사용자 API 키, 토큰과 클라이언트 시크릿을 받는 신규 스킬의 접수를 보류합니다. 키 없는 기능이 있어도 실행 의존성 검토를 생략하지 않습니다. [기여 정책](../CONTRIBUTING.md)에 따라 시스템과 사용자 환경 변수, `.env` 파일이나 대화 입력으로 자격 증명을 받는 우회 안내도 제공하지 않습니다.

## 기존 활성 스킬 8개의 고정 버전 검토

각 행에 고정 원본과 필수 절차의 검토 결과를 기록했습니다. [설치 검증 기록](skill-eligibility-audit-2026-10-25.json)은 8개 모두의 설치, 출처와 라이선스 보존, Codex의 발견, 활성화 전환과 제거 성공도 기록합니다. 호환 후보라는 표기는 실제 서비스 호출, 인증서 검사, Sandbox 작업이나 모델을 통한 실행 성공을 뜻하지 않습니다. 출처 링크는 이동하는 브랜치 대신 카탈로그의 고정 커밋을 가리킵니다.

| 스킬 | 고정 원본 커밋 | 필수 실행 경로 | 현재 판단 |
| --- | --- | --- | --- |
| [official-report-skillset](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/skills/official-report-skillset/SKILL.md) | `a55816b9b44e8391576204a095bf6337940d80f3` | 지침과 참고 문서 2개로 제공한 사실을 초안으로 구성 | 호환 후보, 활성 유지 |
| [tablecloth-certificate-expiry](https://github.com/yourtablecloth/tablecloth-certificate-expiry/blob/9d2eca6431d1cba34bc872b80fc55fa71fa2dcb2/SKILL.md) | `9d2eca6431d1cba34bc872b80fc55fa71fa2dcb2` | 사용자 동의를 받은 호스트 브리지가 만료 건수를 전달하고 Codex가 해석 | 호환 후보, 활성 유지 |
| [tablecloth-windows-sandbox](https://github.com/yourtablecloth/tablecloth-windows-sandbox/blob/b9e52bcd303624dbba81e7de66a3f56344e922fe/SKILL.md) | `b9e52bcd303624dbba81e7de66a3f56344e922fe` | 호스트 브리지가 허용한 작업 결과를 전달하며 Windows 호환 조건 적용 | 호환 후보, 활성 유지 |
| [bok-ecos-stats](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats/SKILL.md) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | 필수 `npx` 지침 로더와 Python 보조 스크립트 실행 | 운영 비호환, PoC 평가에서만 활성 |
| [kosis-stats](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats/SKILL.md) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | 필수 `npx` 지침 로더와 Python 보조 스크립트 실행 | 운영 비호환, PoC 평가에서만 활성 |
| [tablecloth-calculators](https://github.com/yourtablecloth/tablecloth-calculators/blob/877b58cf0741db21342445f2b3fcb54ad1ada85c/SKILL.md) | `877b58cf0741db21342445f2b3fcb54ad1ada85c` | 동봉한 `scripts/calculate.py` 실행을 요구하며 추측 결과로 대체 금지 | 운영 비호환, PoC 평가에서만 활성 |
| [korean-jangbu-for](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/skills/korean-jangbu-for/SKILL.md) | `592f6d7c7be138ae578be9ed6769239cc527ddea` | 별도 Python MCP 서버와 하위 스킬 6개가 필요하며 설치 진입점에는 `SKILL.md`만 존재 | 비호환, 주석 보류 |
| [korean-law-mcp](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/skills/korean-law/SKILL.md) | `4ccedb0dc89fce164493ffc9019b5ad35cb9b782` | 별도로 연결한 `korean-law` MCP 조회와 인용 검증 도구 사용 | 비호환, 주석 보류 |

보고서 스킬의 고정 [참고 문서 디렉터리](https://github.com/mouseco/k-gov-skills/tree/a55816b9b44e8391576204a095bf6337940d80f3/skills/official-report-skillset/references)에는 스크립트나 MCP 의존성이 없습니다. 대화 초안을 지원하며 로컬 문서 자동 읽기나 HWP, Word 파일 내보내기는 포함하지 않습니다. 장부 스킬의 [Python 프로젝트](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/mcp-server/pyproject.toml)와 하위 스킬은 설치한 진입점 경로 밖에 있습니다. LLM 보조 분류도 MCP가 전달한 마스킹 거래와 저장 도구를 전제합니다.

## 검증 단계와 미검증 범위

검증 기록에서는 다음 결과를 구분합니다.

1. 고정 경로, 진입점, 라이선스와 필수 절차를 정적으로 검토합니다.
2. 격리한 프로필에서 실제 설치 관리자, 출처 보존과 Codex의 스킬 발견을 검증합니다.
3. 기존 실행 정책 아래에서 보고서 처리와 호스트 브리지 fixture의 앱 동작을 검증합니다.
4. 실제 AI 대화에서 필요한 도구 호출과 반환 결과로 기능 실행을 검증합니다.

[검증 JSON](skill-eligibility-audit-2026-10-25.json)은 8개 모두에 대해 2단계의 설치, Codex 0.156.1 `skills/list` 발견, 활성화 전환과 제거 성공을 기록합니다. 네이티브 앱 서버 프로세스 1회를 실행했으며 모델과 스킬 스크립트 호출, 실제 서비스 호출, 인증서 검사, Sandbox 상태 변경과 런타임 변경은 각각 0회입니다. 고유 임시 저장소도 제거했습니다. MCP 의존 진입점을 발견했다고 해서 해당 MCP 도구를 연결한 것으로 판단하지 않습니다.

실제 Windows 프로필의 ACL 설정 함수는 검증 토큰의 권한을 거부했습니다. 이후 artifact 전용 프로필에 원래의 `WindowsManagedAiProfile.Configuration`을 그대로 저장하고 두 실행 기능을 false로 유지한 상태에서 검증했습니다. 따라서 위 설정에서 설치와 발견 동작을 확인한 결과로 한정하며 프로필 ACL이나 운영 보안 검증의 성공으로 표시하지 않습니다.

3단계의 로컬 Release 회귀 검사에서는 ManagedAi 197개와 앱 322개가 모두 통과해 총 519개를 확인했습니다. 앱 검사는 호스트 브리지 fixture, 대화, 두 탭 UI와 설치한 스킬 관리를 포함합니다. 원본 TRX 기록은 `managed-ai-removal.trx`와 `app-runtime-removal.trx`로 보존합니다. Fixture 성공만으로 실제 인증서 접근, Sandbox 작업이나 모델을 통한 스크립트 실행을 입증하지 않습니다. Spork는 변경하지 않았으며 이번 변경으로 다시 실행하지 않았습니다.

4단계는 미검증으로 남깁니다. 기존 스킬 8개 설치 감사에서는 실제 서비스 API, 인증서 검사, Sandbox 작업이나 모델을 호출하지 않았습니다. 후속 [AppContainer 보고서](APP_CONTAINER_POC.ko.md)는 실제 모델 추론 없이 수행한 격리 실행과 고정 공개 서비스 표본 2개를 별도로 기록합니다. 단위 테스트나 직접 실행한 `codex sandbox` 하위 프로세스의 성공만으로 모델의 도구 등록이나 식탁보 대화의 자동 스크립트 실행을 판단하지 않습니다. 이번 카탈로그 검토는 실행 설정을 변경하지 않습니다.

## 등재와 버전 변경 시 재검토

활성 항목을 추가하기 전에 고정 원본, 핵심 기능, 필수 실행 의존성, 사용 가능한 호스트 기능, 검증한 범위와 미검증 범위를 기록합니다. `Source.Ref`를 바꿀 때도 같은 내용을 다시 검토합니다. 진입점 파일을 유지한 채 원본 버전이 새로운 실행 의존성을 도입할 수 있습니다. 새 활성 ID나 고정 버전은 [testskills.cs](../src/testskills.cs)의 등재 허용 목록과 두 언어의 검증표에 함께 반영합니다.

현재 호스트가 필수 단계를 수행할 수 없으면 사유와 XML 블록 전체를 주석으로 보존합니다. 런타임이나 빠진 도구를 설치할 수 있다는 설명을 연동 완료로 표시하지 않습니다. 필요한 앱 기능을 구현하고 독립 검증한 뒤 다시 등재를 검토합니다. 원격 보류 후에도 기존 로컬 설치를 관리할 수 있으며 [카탈로그 관리 안내](SkillCatalog.README.ko.md)에서 수명 주기를 설명합니다.

## 카탈로그 검사의 범위

스키마와 [checkskills.cs](../src/checkskills.cs)는 원본과 라이선스 좌표를 검사합니다. [testskills.cs](../src/testskills.cs)는 현재 등재한 고정 원본을 제한하고 PoC 스크립트 예외의 정확한 고정 버전과 미연결 MCP 및 자격 증명 후보의 재활성화 차단, 원시 XML 블록의 주석 처리를 검증합니다. 호환 후보의 제거는 허용합니다. XML에 실행 유형이나 검토 상태 필드를 추가하지 않습니다.

저장소 루트에서 카탈로그 전용 검사를 실행할 수 있습니다.

```text
dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd
dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd
dotnet run --file src/catalogutil.cs -- docs/ outputs/
```

검사 성공은 카탈로그 구조, 등재 정책과 생성 결과의 일치를 확인합니다. 원본 스킬 실행이나 앱 런타임 지원을 입증하지는 않습니다. [PR 워크플로](../.github/workflows/skill-catalog.yml)와 [게시 워크플로](../.github/workflows/publish.yml)는 카탈로그 검사를 실행하며 실제 기능 검증은 별도로 진행합니다.

## 현재 판단과 재검증 범위

여기까지 정리하면 카탈로그는 대화 초안 지침과 기존 호스트 보고서 해석을 활성 후보로 남깁니다. [원본 8개 검증](skill-eligibility-audit-2026-10-25.json)은 패키지 설치, 발견과 관리의 성공을 확인하며 필수 의존성에 따른 등재 판단과 구분합니다. 운영 기준에서는 비호환 항목 5개를 보류합니다. 이번 PoC 분기는 고정 스크립트 후보 3개만 평가용으로 활성화하며 MCP 항목 2개의 주석은 유지합니다. [AppContainer 평가](APP_CONTAINER_POC.ko.md)는 실측한 x64 호스트의 격리 실행과 고정 응답을 사용한 CLI 연결을 기록합니다. 실제 모델 실행과 운영 호환성은 미검증으로 유지합니다.

사용자는 기존 로컬 설치를 계속 관리할 수 있습니다. 새로운 실행 경로나 MCP 지원은 앱 기능 개발과 검증을 마친 뒤 재등재를 판단합니다. 기여자는 고정 원본 표와 검증 단계를 기준으로 패키지 갱신과 실행 기능 지원의 변화를 구분할 수 있습니다.
