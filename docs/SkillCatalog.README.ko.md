# 원격 스킬 카탈로그의 추가와 갱신

[English](SkillCatalog.README.md)

2026년 10월 25일 기준으로 [SkillCatalog.xml](SkillCatalog.xml)은 보고서 작성 지침 1개와 기존 식탁보 호스트 브리지 2개를 활성 후보로 제공합니다. 기존 활성 8개의 검토로 보류한 5개를 포함해 나머지 후보 12개는 XML 주석으로 보존합니다. 현재 대화는 설치한 Python과 Node 스크립트 실행이나 MCP 서버 자동 설정을 지원하지 않습니다. [등재 기준과 고정 원본 검증](SKILL_ELIGIBILITY.ko.md)에서 파일 설치, 스킬 발견과 실제 기능 실행을 구분합니다.

이 문서는 항목 추가와 제거, 고정 버전 갱신, 실행 선행 조건, 라이선스 및 게시 전 검증을 다룹니다. 활성 `Skill` 요소는 별도 검토 상태 없이 설치 대상으로 표시합니다.

항목 편집 방식부터 정리하겠습니다. 이후 버전과 실행 조건을 구분하고 검증 및 배포 절차를 설명하겠습니다.

## 활성 요소와 주석을 통한 항목 관리

활성 항목의 등록 방식을 살펴보겠습니다. [현재 호스트의 등재 기준](SKILL_ELIGIBILITY.ko.md)에 따라 필수 실행 경로와 검증 범위를 먼저 기록합니다. `Skill` 요소에 고유 ID, 실제 스킬 디렉터리, 전체 커밋 SHA와 같은 저장소 및 커밋의 라이선스 URL을 기록합니다. `Install.Mode`는 `upstream`을 사용하며 목록 상태와 검토 상태 속성은 기록하지 않습니다.

이어서 항목 제거 방식을 짚겠습니다. 해당 `Skill` 요소 전체를 XML 주석으로 감싸고 제거 사유를 짧게 남깁니다. 요소 본문에 연속된 하이픈 두 개가 있으면 `&#45;&#45;`로 표기해 블록을 유효한 XML 주석으로 감쌀 수 있습니다. XML 파서는 원래 문자를 읽으며 주석을 설치 목록에 포함하지 않습니다. 원격 목록에서 항목을 제거해도 사용자는 기존 로컬 설치를 계속 관리하거나 제거할 수 있습니다. [현재 XML](SkillCatalog.xml)은 순수 MCP 후보 2개의 블록을 이 방식으로 보존합니다.

## 환경 변수로 자격 증명을 받는 신규 스킬의 접수 보류

사용자 API 키, 토큰이나 클라이언트 시크릿을 환경 변수로 전달받는 신규 스킬의 등록 접수를 현재 보류합니다. 환경 변수의 비밀값은 평문 노출과 자식 프로세스 상속의 위험이 있으며 식탁보의 스킬별 자격 증명 저장 및 전달 기능은 아직 구현하지 않았습니다. [TableCloth #321](https://github.com/yourtablecloth/TableCloth/issues/321)의 대응 기능 개발과 검증을 마친 뒤 접수를 재개합니다. [기여 정책](../CONTRIBUTING.md)에도 같은 기준을 적용합니다.

사용자 키 없이 독립적으로 동작하는 기능을 제안하는 경우에는 지원 명령, 출력 범위와 추가 의존성을 명시하고 사용자 키를 사용하는 경로와 구분합니다. 기존 카탈로그 항목에 키 없는 독립 기능이 있더라도 신규 자격 증명 전달 경로는 계속 접수를 보류합니다. 시스템 또는 사용자 환경 변수, `.env` 파일이나 채팅 프롬프트로 키를 입력하게 하는 우회 사용법은 안내하지 않습니다. 코드, 카탈로그, 문서와 PR에는 실제 키, 토큰이나 클라이언트 시크릿을 포함하지 않으며 예시는 비밀값이 아닌 자리표시자를 사용합니다.

## 고정 커밋을 기준으로 한 업데이트

같은 스킬을 갱신할 때는 `Id`, `Source.Repository`와 `Source.Path`를 유지합니다. `Source.Ref`를 새 전체 커밋 SHA로 바꾸고 `License.Source`도 같은 커밋으로 갱신합니다. 활성화하기 전에 필수 의존성을 다시 검토하고 검증 기록과 회귀 검사의 허용 목록을 함께 갱신합니다. 식탁보는 설치한 원본 커밋과 카탈로그 커밋을 비교해 업데이트를 제공합니다.

다만 원본 지침이 실행 중 내려받는 패키지는 자체 버전 동작을 따릅니다. 주석으로 보류한 [NomaDamas 원본](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39)의 후보는 `npx` CLI로 지침을 읽으므로 Git 커밋 고정만으로 해당 CLI의 배포 버전까지 고정하지는 않습니다.

## 스킬 파일 설치와 실행 선행 조건

설치 관리자는 스킬 파일을 복사하고 같은 커밋의 라이선스를 `LICENSE.upstream`으로 보존합니다. 파일 설치만으로 런타임, MCP 서버나 API 키를 설정하지 않으며 현재 AI 대화의 실행 권한도 바꾸지 않습니다. `Network`는 스킬 실행 시 네트워크 의존성을 나타내며 원격 설치에는 항상 다운로드를 사용합니다.

[공동인증서 만료 스킬](https://github.com/yourtablecloth/tablecloth-certificate-expiry)과 [Windows Sandbox 스킬](https://github.com/yourtablecloth/tablecloth-windows-sandbox)은 별도 저장소에서 배포하며 기본 구성에는 포함하지 않습니다. 추가 API 키 없이 식탁보 호스트가 제공한 로컬 보고서를 해석합니다. 인증서 조회는 사용자 동의를 받은 건수 요약으로 제한하며 Sandbox 작업은 호환되는 Windows 환경과 식탁보 호스트 브리지를 요구합니다. 원본 AGPL과 별도 상용 라이선스 문의 안내를 보존합니다.

주석으로 보류한 [금융 및 생활 계산기](https://github.com/yourtablecloth/tablecloth-calculators)는 Python 3.11 이상과 표준 라이브러리만 사용합니다. 계산 스크립트는 사용자 입력값을 사용하며 API 키, 외부 데이터 조회와 추가 패키지 다운로드를 요구하지 않습니다. 현재 식탁보 대화는 필요한 스크립트 실행 경로를 제공하지 않으며 런타임을 설치해도 이 제한은 유지합니다. `Network="none"`은 계산 스크립트의 조건을 나타내며 Codex 대화는 기존 온라인 연결을 사용합니다. 고정 원본의 [저작권 준수 기준](https://github.com/yourtablecloth/tablecloth-calculators/blob/877b58cf0741db21342445f2b3fcb54ad1ada85c/docs/COPYRIGHT.ko.md)도 해당 후보에 계속 적용합니다.

주석으로 보류한 [법령 스킬](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/skills/korean-law/SKILL.md)은 별도로 연결한 `korean-law` MCP 서버를 사용합니다. [장부 스킬](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/skills/korean-jangbu-for/SKILL.md)도 설치한 진입점 경로 밖의 Python MCP 서버와 하위 스킬을 사용합니다. 스크립트 파일이 없는 진입점도 필수 도구에 따라 실행 의존성을 판단합니다.

## 원본 라이선스와 데이터 이용 조건

주석으로 보존한 후보까지 포함한 원본 라이선스는 [NomaDamas MIT](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/LICENSE), [kimlawtech Apache-2.0](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/LICENSE), [chrisryugj MIT](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/LICENSE)와 [mouseco MIT](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/LICENSE)를 고정 커밋에서 참조합니다. 식탁보 호스트 브리지 원본은 앞 절에서 설명한 AGPL과 상용 문의 안내를 보존합니다.

또한 mouseco의 [출처 기록](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/docs/attribution.md)은 후보 4개를 직접 작성한 스킬로 구분합니다. API 응답 데이터와 참조 도구에는 각 원 출처의 조건을 적용합니다. 카탈로그는 자격증명을 저장하거나 원본 코드를 실행하지 않습니다.

## 스키마 검사와 기존 Pages 배포

[SkillCatalog.xsd](SkillCatalog.xsd)는 고정 커밋과 라이선스 URL을 필수로 요구하고 활성 항목을 최대 256개로 제한합니다. [checkskills.cs](../src/checkskills.cs)는 XML 크기를 512 KiB로 제한하며 라이선스 URL의 저장소 및 커밋 일치와 경로를 검사합니다. 디렉터리 이름 내부의 점은 허용하지만 끝점은 거부합니다. 루트 표시 `.`는 계속 허용합니다.

저장소 루트에서 `dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd`와 `dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd`를 실행할 수 있습니다. 두 번째 도구는 현재 등재한 ID와 고정 원본도 제한하므로 목록을 바꿀 때 [검증 기록](SKILL_ELIGIBILITY.ko.md)을 함께 갱신합니다. [PR 워크플로](../.github/workflows/skill-catalog.yml)와 [Pages 워크플로](../.github/workflows/publish.yml)가 두 검사를 함께 실행합니다. 기존 카탈로그 빌더는 `docs`를 복사해 `https://catalog.yourtablecloth.app/SkillCatalog.xml`에 게시합니다. 이 검사는 원본 스킬을 실행하지 않습니다.

## 카탈로그와 로컬 설치의 수명 주기

여기까지 정리하면 활성 XML 요소가 설치 목록을 구성하고 고정 커밋의 변경이 업데이트를 구분합니다. XML 주석은 원본 기록을 남기면서 항목을 목록에서 제거합니다.

원격 항목 편집은 앞으로 제공할 설치와 업데이트에 영향을 줍니다. 사용자는 기존 로컬 설치를 계속 관리할 수 있습니다. 활성화 여부는 각 스킬의 필수 실행 경로와 현재 호스트의 지원 범위를 기준으로 판단합니다.
