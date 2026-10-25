# 원격 스킬 카탈로그의 추가와 갱신

[English](SkillCatalog.README.md)

2026년 10월 25일 기준으로 [SkillCatalog.xml](SkillCatalog.xml)은 스킬 5개를 활성 항목으로 제공합니다. 초기 목록에서 보류한 후보 5개와 `SKILL.md`가 없는 MCP 후보 2개는 XML 주석으로 보존합니다.

이 문서는 항목 추가와 제거, 고정 버전 갱신, 실행 선행 조건, 라이선스 및 게시 전 검증을 다룹니다. 활성 `Skill` 요소는 별도 검토 상태 없이 설치 대상으로 표시합니다.

항목 편집 방식부터 정리하겠습니다. 이후 버전과 실행 조건을 구분하고 검증 및 배포 절차를 설명하겠습니다.

## 활성 요소와 주석을 통한 항목 관리

활성 항목의 등록 방식을 살펴보겠습니다. `Skill` 요소에 고유 ID, 실제 스킬 디렉터리, 전체 커밋 SHA와 같은 저장소 및 커밋의 라이선스 URL을 기록합니다. `Install.Mode`는 `upstream`을 사용하며 목록 상태와 검토 상태 속성은 기록하지 않습니다.

이어서 항목 제거 방식을 짚겠습니다. 해당 `Skill` 요소 전체를 XML 주석으로 감싸고 제거 사유를 짧게 남깁니다. 요소 본문에 연속된 하이픈 두 개가 있으면 `&#45;&#45;`로 표기해 블록을 유효한 XML 주석으로 감쌀 수 있습니다. XML 파서는 원래 문자를 읽으며 주석을 설치 목록에 포함하지 않습니다. 원격 목록에서 항목을 제거해도 사용자는 기존 로컬 설치를 계속 관리하거나 제거할 수 있습니다. [현재 XML](SkillCatalog.xml)은 순수 MCP 후보 2개의 블록을 이 방식으로 보존합니다.

## 고정 커밋을 기준으로 한 업데이트

같은 스킬을 갱신할 때는 `Id`, `Source.Repository`와 `Source.Path`를 유지합니다. `Source.Ref`를 새 전체 커밋 SHA로 바꾸고 `License.Source`도 같은 커밋으로 갱신합니다. 식탁보는 설치한 원본 커밋과 카탈로그 커밋을 비교해 업데이트를 제공합니다.

다만 원본 지침이 실행 중 내려받는 패키지는 자체 버전 동작을 따릅니다. [NomaDamas 원본](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39)의 스킬은 현재 `npx` CLI로 지침을 읽으므로 Git 커밋 고정만으로 해당 CLI의 배포 버전까지 고정하지는 않습니다.

## 스킬 파일 설치와 실행 선행 조건

설치 관리자는 스킬 파일을 복사하고 같은 커밋의 라이선스를 `LICENSE.upstream`으로 보존합니다. 런타임, MCP 서버 등록과 API 키는 별도로 설정합니다. `Network`는 스킬 실행 시 네트워크 의존성을 나타내며 원격 설치에는 항상 다운로드를 사용합니다.

[법령 스킬](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/skills/korean-law/SKILL.md)은 파일 설치 후 별도 `korean-law` MCP 서버와 법제처 API 인증키를 사용합니다. [장부 스킬](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/skills/korean-jangbu-for/SKILL.md)도 진입점 외에 MCP 서버와 하위 스킬을 사용합니다. 각 항목의 `Notes`에서 실행 조건을 확인할 수 있습니다.

## 원본 라이선스와 데이터 이용 조건

활성 항목은 [NomaDamas MIT](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/LICENSE), [kimlawtech Apache-2.0](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/LICENSE), [chrisryugj MIT](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/LICENSE)와 [mouseco MIT](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/LICENSE)를 원본 고정 커밋에서 참조합니다.

또한 mouseco의 [출처 기록](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/docs/attribution.md)은 후보 4개를 직접 작성한 스킬로 구분합니다. API 응답 데이터와 참조 도구에는 각 원 출처의 조건을 적용합니다. 카탈로그는 자격증명을 저장하거나 원본 코드를 실행하지 않습니다.

## 스키마 검사와 기존 Pages 배포

[SkillCatalog.xsd](SkillCatalog.xsd)는 고정 커밋과 라이선스 URL을 필수로 요구하고 활성 항목을 최대 256개로 제한합니다. [checkskills.cs](../src/checkskills.cs)는 XML 크기를 512 KiB로 제한하며 라이선스 URL의 저장소 및 커밋 일치와 경로를 검사합니다. 디렉터리 이름 내부의 점은 허용하지만 끝점은 거부합니다. 루트 표시 `.`는 계속 허용합니다.

저장소 루트에서 `dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd`와 `dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd`를 실행할 수 있습니다. [PR 워크플로](../.github/workflows/skill-catalog.yml)와 [Pages 워크플로](../.github/workflows/publish.yml)가 두 검사를 함께 실행합니다. 기존 카탈로그 빌더는 `docs`를 복사해 `https://catalog.yourtablecloth.app/SkillCatalog.xml`에 게시합니다.

## 카탈로그와 로컬 설치의 수명 주기

여기까지 정리하면 활성 XML 요소가 설치 목록을 구성하고 고정 커밋의 변경이 업데이트를 구분합니다. XML 주석은 원본 기록을 남기면서 항목을 목록에서 제거합니다.

원격 항목 편집은 앞으로 제공할 설치와 업데이트에 영향을 줍니다. 사용자는 기존 로컬 설치를 계속 관리할 수 있습니다. MCP 서버와 API 키 같은 실행 조건은 각 스킬의 `Notes`와 원본 문서에 따라 설정할 수 있습니다.
