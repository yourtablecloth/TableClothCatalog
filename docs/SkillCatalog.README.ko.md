# 원격 스킬 카탈로그 후보

[English](SkillCatalog.README.md)

2026년 10월 25일 기준으로 [SkillCatalog.xml](SkillCatalog.xml)에 커뮤니티 스킬과 MCP 연동 후보 12개를 등록했습니다. 모든 항목의 라이선스 검토와 설치 검토는 `pending`을 유지합니다.

이 문서는 배포 위치, 원본 고정 방식, 검토 상태와 검증 범위를 다룹니다. 항목을 등록해도 식탁보가 설치나 MCP 서버 설정을 수행하지 않습니다.

카탈로그의 배포 경로부터 설명하고 검토 상태와 원본 확인 근거를 정리하겠습니다. 마지막으로 설치 승인 전 검증 범위를 짚겠습니다.

## 기존 Pages 배포 경로

카탈로그 배포 경로를 살펴보겠습니다. [publish.yml](../.github/workflows/publish.yml)은 카탈로그 빌더가 복사한 `docs` 전체를 GitHub Pages에 게시합니다. 이 절차를 거친 XML은 `https://catalog.yourtablecloth.app/SkillCatalog.xml`에서 제공합니다. 식탁보는 저장소 체크아웃 없이 XML을 읽습니다.

## 커밋 고정과 외부 패키지 의존성

원본 고정 범위를 검토하겠습니다. 각 `Source`는 공개 GitHub 저장소, 상대 디렉터리와 전체 커밋 SHA를 기록합니다. `Path="."`는 스킬 디렉터리가 없는 MCP 후보의 저장소 루트를 가리킵니다.

디렉터리 이름 내부의 점은 `skills/example.v1`처럼 사용할 수 있지만 각 이름의 끝에는 점을 붙일 수 없습니다. 루트 표시 `.`는 계속 허용합니다. 이 규칙으로 마켓플레이스 클라이언트가 거부하는 경로의 게시를 방지합니다.

다만 Git 커밋을 고정해도 원본 지침이 실행 중 내려받는 패키지까지 고정하지는 않습니다. NomaDamas 후보는 현재 `npx` CLI로 지침을 읽습니다. 패키지 버전과 실행 환경 검토는 계속 `pending`으로 남깁니다.

## 목록 노출과 설치 승인 조건

검토 상태의 역할을 확인하겠습니다. `Status`는 `listed`와 `unlisted`를 허용합니다. `License.ReviewStatus`는 `pending`, `verified`, `rejected`를 허용합니다. `Install.ReviewStatus`는 `pending`, `approved`, `rejected`를 허용합니다.

이어서 설치 조건을 구체적으로 적겠습니다. 식탁보는 목록 상태 `listed`, 라이선스 검토 `verified`, 설치 검토 `approved`, 설치 방식 `upstream`, 런타임 `Codex`와 고정 커밋을 모두 충족하는 항목의 설치를 허용합니다. 라이선스 근거도 같은 저장소와 커밋으로 고정하며 설치 관리자가 해당 파일을 `LICENSE.upstream`으로 보존합니다. `adapter` 항목은 연동 개발 후보로 표시하며 MCP 서버를 설치하거나 설정하지 않습니다.

## 고정 원본의 라이선스 확인 근거

원본 LICENSE에서 [NomaDamas MIT](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/LICENSE), [jjlabsio ISC](https://github.com/jjlabsio/korea-stock-mcp/blob/210595eec73a39cd04e2fb175f6e69b1516f0dd4/LICENSE), [kimlawtech Apache-2.0](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/LICENSE), [chrisryugj MIT](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/LICENSE), [mouseco MIT](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/LICENSE)와 [tae0y MIT](https://github.com/tae0y/real-estate-mcp/blob/1119eb1069d1b1bae15f7b38c6c0268fe07995c0/LICENSE)를 확인했습니다.

또한 mouseco의 [출처 기록](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/docs/attribution.md)은 이번 후보 4개를 직접 작성한 스킬로 구분합니다. API 응답 데이터와 참조 도구에는 각 원 출처의 조건을 적용합니다. LICENSE 확인만으로 재배포, 보안 또는 호환성 검토를 완료하지 않습니다.

## 스키마 검사와 실행 검증 범위

[SkillCatalog.xsd](SkillCatalog.xsd)는 메타데이터, 검토 상태와 ID 중복을 검사합니다. 저장소 루트에서 `dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd`로 실행할 수 있습니다. Pages 워크플로도 게시 전에 같은 검사를 실행합니다. 설치 검토에서는 전체 파일과 라이선스 고지, 보조 스크립트 경로, 패키지 버전, 식탁보 전용 Codex 환경 호환성, API 키와 문서 접근 범위를 함께 다룹니다.

경로 회귀 검사는 `dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd`로 실행할 수 있습니다. [PR 워크플로](../.github/workflows/skill-catalog.yml)가 실제 스키마를 대상으로 카탈로그 검증과 회귀 검사를 함께 실행합니다.

카탈로그는 자격증명을 저장하거나 원본 코드를 실행하지 않습니다. `Network`는 스킬 실행 시 네트워크 의존성을 표시합니다. 원격 스킬 다운로드에는 별도로 네트워크를 사용합니다. Windows Sandbox 내부 스킬 실행은 별도 구현과 검증을 전제로 합니다.

## 후보 등록 이후의 검토 범위

여기까지 정리하면 카탈로그는 원본과 채택 조건을 고정해 검토 후보를 제공합니다. 현재 12개 후보 중 설치 승인을 마친 항목은 없습니다.

우선 각 후보의 라이선스 고지와 실행 환경을 검증할 수 있습니다. MCP 어댑터 구현, 민감정보 통제와 외부 패키지 고정은 후속 개발 범위로 남깁니다. 검토 상태와 `Notes`를 함께 읽으면 각 후보의 진행 범위를 판단할 수 있습니다.
