# 식탁보 카탈로그

[![식탁보 카탈로그 빌드 상황](https://github.com/yourtablecloth/TableClothCatalog/actions/workflows/publish.yml/badge.svg)](https://github.com/yourtablecloth/TableClothCatalog/actions)
[![식탁보 Discord](https://img.shields.io/discord/1443777680418930761?label=Discord&logo=discord&color=7289DA)](https://discord.gg/eT2UnUXyTV)
[![식탁보 후원](https://img.shields.io/github/sponsors/yourtablecloth)](https://github.com/sponsors/yourtablecloth)

이 리포지터리는 식탁보에서 참조하는 카탈로그 XML 파일을 관리하는 리포지터리입니다.

기존 사이트에 변경이 발생했을 경우 이 리포지터리에 풀 리퀘스트를 올려주세요.

그 외에 식탁보 프로그램 자체에 대한 이슈 제보나 최신 버전 다운로드는 [이곳](https://github.com/yourtablecloth/TableCloth)을 참고해주세요.

## 카탈로그 피드백 전달 방법

식탁보에서 접속할 수 있는 특정 웹 사이트와 관련된 문제는 다음 중 한 가지 방법을 통하여 제보 또는 기여를 부탁드립니다.

* **권장**: [식탁보 카탈로그 리포지터리에 이슈 등록 또는 PR 제출](https://github.com/yourtablecloth/TableClothCatalog)
* [Google Forms를 통한 제보](https://forms.gle/Pw6pBKhqF1e5Nesw6)
* [Discord 채널을 통한 제보/토론](https://discord.gg/eT2UnUXyTV)

기관명, 로고, 표지의 사용 중단이나 항목 삭제, 정보 정정을 요청하시는 경우에는 [정정 및 삭제 요청 양식](https://github.com/yourtablecloth/TableClothCatalog/issues/new?template=removal-request.yml)을 이용해주세요. 처리 절차는 [docs/REMOVAL_REQUEST.md](docs/REMOVAL_REQUEST.md)에 정리되어 있습니다.

## PR 제출 시 컨트리뷰션 방법

카탈로그 리포지터리는 다음의 파일들로 구성됩니다.

- [docs/Catalog.xml](docs/Catalog.xml): 각 기관, 서비스 웹 사이트마다 필요로 하는 추가 소프트웨어들의 목록을 저장하고 관리하는 XML 파일입니다.
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
- [docs/SkillCatalog.xml](docs/SkillCatalog.xml): 원격 스킬과 MCP 연동 후보, 고정 원본 및 검토 상태를 관리합니다. 형식과 설치 승인 조건은 [원격 스킬 카탈로그 안내](docs/SkillCatalog.README.ko.md)를 참고하실 수 있습니다.
- [docs/sites.xml](docs/sites.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.5 버전부터 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
  - Microsoft Edge 그룹 정책에서 <https://yourtablecloth.app/TableClothCatalog/sites.xml> 주소를 직접 지정하여 설정을 적용할 수 있습니다.
- [docs/images/&lt;Category&gt;/&lt;Id&gt;.png](docs/images/): `<Category>`와 `<Id>`에 해당하는 식별자를 넣어 고해상도 투명 PNG 로고 이미지 파일을 등록하여 관리합니다. (예: [docs/images/Banking/WooriBank.png](docs/images/Banking/WooriBank.png))

마크다운 문서를 수정하실 때에는 [마크다운 린터 플러그인](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint)을 사용하여 커밋/PR 제출 전 확인하는 것을 권장합니다.

아래 파일은 구 버전의 식탁보 프로그램에서 사용했던 파일로, 현재는 사용되지 않습니다.

- [docs/Compatibility.xml](docs/Compatibility.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.3, 0.5.4 버전에서 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.

위의 두 개의 파일에 새로운 항목을 추가하고 Pull Request를 보내주시면, 검토 후에 반영하도록 하겠습니다.

### 에이전트형 브라우저를 활용한 카탈로그 최신화

카탈로그 최신화에는 [Aside](https://aside.com/)와 같이 브라우저와 AI를 함께 활용하는 에이전트형 웹 브라우저를 권장합니다. 공식 홈페이지의 메뉴와 설치 안내를 따라가며 현재 카탈로그의 서비스 주소, 프로그램 목록, 설치 파일 URL을 비교합니다. Aside의 작업 방식은 [공식 안내](https://docs.aside.com/help/tasks)에서 확인할 수 있습니다.

1. 대상 서비스 ID와 기존 XML 항목을 브라우저에 전달하고 공식 공개 안내 페이지를 조사합니다.
2. 확인한 변경의 XML 변경안을 작성하고 공식 출처 URL, 확인일, 미확인 항목을 남깁니다.
3. 카탈로그 빌더로 검증한 뒤 결과와 함께 PR을 제출합니다. 사람이 근거와 diff를 검토하여 반영합니다.

요청 예시와 검증 범위는 [카탈로그 최신화 절차](docs/SITE_HEALTH_WORKFLOW.md)에서 정리합니다. 기존 헬스 체크, 품질 리포트, 제보 인테이크 도구는 조사 범위에 따라 함께 사용할 수 있습니다.

### 카탈로그 빌더를 통한 변경안 검증

카탈로그 빌더는 스키마 검증, URL 접속성 점검, 출력 리소스 생성을 수행합니다. [배포 워크플로](.github/workflows/publish.yml)는 main 브랜치의 관련 파일 변경 시 빌더를 실행하고 `outputs` 디렉터리의 결과물을 `gh-pages` 브랜치에 배포합니다.

```bash
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
```

빌더의 오류와 경고를 검토하고 해결하지 못한 항목은 PR 본문에 사유를 적습니다. URL 접속 확인과 Windows Sandbox에서의 실제 설치 시험 결과는 구분하여 기록합니다.

### 카탈로그 품질 관리 도구

아래 도구는 브라우저 조사 결과의 검증과 다수 항목의 상태 확인을 지원합니다. 카탈로그 빌더로 변경안을 검증하고 이미지나 접속 상태는 작업 범위에 맞는 도구로 추가 점검할 수 있습니다.

#### 카탈로그 유틸리티 (catalogutil.cs)

XML 스키마 검증, 이미지 최적화, 출력 파일 생성 등을 수행하는 도구입니다.

```powershell
# 리포지터리 루트 디렉터리에서 실행
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
```

#### 이미지 검증 도구 (checkimages.cs)

각 서비스 ID에 해당하는 로고 이미지가 올바르게 존재하는지 검증하는 도구입니다. 누락된 이미지나 사용되지 않는 이미지를 찾아줍니다.

```powershell
# 리포지터리 루트 디렉터리에서 실행
dotnet run --file src/checkimages.cs -- ./docs/Catalog.xml ./docs/images
```

이 도구는 다음을 검사합니다:

- **누락된 이미지**: Catalog.xml에 서비스는 등록되어 있지만 `docs/images/<Category>/<Id>.png` 파일이 없는 경우
- **미사용 이미지**: 이미지 파일은 존재하지만 Catalog.xml에 해당 서비스가 없는 경우

새로운 서비스를 추가할 때는 반드시 해당 서비스의 로고 이미지도 함께 추가해주세요.

#### 파비콘 추출 도구 (fetchfavicon.cs)

새로운 서비스를 추가할 때 로고 이미지를 쉽게 가져올 수 있는 도구입니다. 웹사이트에서 파비콘을 자동으로 추출하고 지정한 크기로 업스케일합니다.

```powershell
# 기본 사용법 (256x256으로 업스케일)
dotnet run .\src\fetchfavicon.cs https://www.example.com .\docs\images\Banking\Example.png

# 크기 지정 (512x512)
dotnet run .\src\fetchfavicon.cs https://www.example.com .\docs\images\Banking\Example.png 512
```

이 도구는 다음 순서로 아이콘을 찾습니다:

1. HTML의 `<link rel="icon">` 태그에서 지정된 아이콘
2. Apple Touch Icon (일반적으로 가장 고해상도)
3. Open Graph 이미지
4. 기본 `/favicon.ico` 또는 `/favicon.png`

**참고**: 이 도구로 생성된 이미지는 임시용입니다. 가능하면 해당 기관의 공식 로고 이미지로 교체해주세요.

#### AI를 활용한 이미지 품질 개선

`fetchfavicon.cs` 도구는 규칙 기반으로 파비콘을 추출하고 업스케일하지만, 저해상도(16x16) 아이콘을 확대할 경우 품질이 저하될 수 있습니다. 더 나은 결과를 위해 AI 도구 활용을 권장합니다:

- **이미지 업스케일링**: [Google Gemini](https://gemini.google.com/), [ChatGPT](https://chatgpt.com/) 등의 AI 에이전트에게 저해상도 아이콘을 고해상도로 업스케일해달라고 요청할 수 있습니다.
- **로고 검색**: AI에게 특정 기관의 공식 로고 이미지를 찾아달라고 요청하면 더 정확한 결과를 얻을 수 있습니다.
- **SVG 변환**: 저해상도 래스터 이미지를 벡터(SVG)로 변환한 후 다시 고해상도 PNG로 내보내는 방법도 효과적입니다.

특히 파비콘이 없거나 매우 작은 아이콘만 제공하는 사이트의 경우, AI 도구를 활용하면 훨씬 깔끔한 로고 이미지를 얻을 수 있습니다.

#### 사이트 헬스 체크 도구 (checksites.cs)

[checksites.cs](src/checksites.cs)는 Edge 헤드리스 접속 결과와 스크린샷을 수집하는 보조 도구입니다. 대상 서비스만 확인할 때에는 다음 명령을 사용할 수 있습니다.

```powershell
dotnet run --file src/checksites.cs -- probe ./docs/ ./health-report/ --only WooriBank,NHInternetBank
```

`triage`는 사람의 판정을 저장하고 `issue`는 판정 결과에 따라 GitHub 이슈를 등록합니다. 전체 단계를 거치지 않고 브라우저 조사에서 바로 XML 변경안과 PR을 작성할 수도 있습니다. 도구의 자동 분류는 설치 파일의 최신성이나 IE Mode 및 Windows Sandbox의 동작을 보증하지 않습니다. 활용 범위는 [카탈로그 최신화 절차](docs/SITE_HEALTH_WORKFLOW.md)에서 설명합니다.

#### 품질 리포트와 금융기관 통폐합 조사 도구

기존 워크플로는 여러 항목의 문제나 조사 대상을 모아서 관리할 때 사용할 수 있습니다. 두 워크플로의 주간 일정은 현재 비활성화되어 있으며 `workflow_dispatch`로 수동 실행할 수 있습니다.

- [catalog-quality.yml](.github/workflows/catalog-quality.yml): 이미지와 스키마 검사 결과를 `quality/` 파일과 단일 이슈로 관리
- [financial-merger-watch.yml](.github/workflows/financial-merger-watch.yml): 저축은행과 보험사 목록을 추출하여 통폐합 조사 이슈 생성 또는 갱신

워크플로가 만든 조사 대상도 에이전트형 브라우저로 공식 공지를 확인하여 처리할 수 있습니다. 수동 실행과 AI 배정 옵션은 [품질 리포트 참고 절차](docs/CATALOG_QUALITY_WORKFLOW.md)에서 설명합니다.

#### 제보 인테이크 스킬 (catalog-intake)

[Google Forms 제보](https://forms.gle/Pw6pBKhqF1e5Nesw6)를 CSV로 모아서 처리할 때에는 기존 [catalog-intake 스킬](.claude/skills/catalog-intake/SKILL.md)을 사용할 수 있습니다. 필드 정규화, URL 검사, 로고 수집과 PR 후보 작성을 지원합니다. 조사 결과의 근거를 검토하고 미확인 항목을 남기는 원칙은 브라우저를 활용할 때에도 동일하게 적용합니다.

### 자동 응답 설치 옵션을 찾는 방법

일반적으로 무인 설치 옵션은 설치 프로그램 명령줄 뒤에 `/?`나 `/help`, `/h`, `--help` 등 도움말을 표시하는 것과 관련된 스위치를 대입하여 실행하면 도움말과 함께 자세한 자동 응답 설치 옵션을 사용하는 방법을 표시합니다.

만약 위의 방법을 통하여 옵션을 찾기 어려울 때에는 Universal Silent Setup Finder / Console (`ussfc`) 도구를 이용하는 것을 권장합니다. 설치 파일을 분석하여 패키징 형식을 판별하고, 예상되는 무인 설치 명령줄을 출력해줍니다.

[yourtablecloth/ussfc](https://github.com/yourtablecloth/ussfc)는 기존의 Windows 전용 도구였던 [USSF](https://github.com/alexandruavadanii/USSF)를 C#으로 포팅한 것으로, .NET 도구로 배포되며 Windows뿐 아니라 Linux와 macOS에서도 명령줄로 실행할 수 있습니다. 새로 사용하실 때에는 이 버전을 사용해주세요.

```bash
# .NET 10 SDK가 있으면 설치 없이 바로 실행
dnx ussfc setup.exe

# 반복해서 사용하실 경우 전역 도구로 설치
dotnet tool install -g ussfc
ussfc setup.exe
```

.NET 런타임 설치 없이 사용하시려면 [릴리스 페이지](https://github.com/yourtablecloth/ussfc/releases)에서 플랫폼별 자체 포함 실행 파일을 내려받으실 수 있습니다. Windows x64/ARM64, Linux x64/ARM64, macOS ARM64용 빌드가 제공됩니다.

자동 설치 옵션이 잘 작동하는지 확인하기 위해, Windows Sandbox, 또는 디스크 이미지 수준의 복원 스냅샷 포인트를 확보한 가상 컴퓨터 인스턴스를 활용하여 반복 테스트를 실행해볼 수 있습니다.

## 고지 및 정책 문서

식탁보 카탈로그는 독립적으로 운영되는 오픈소스 프로젝트이며, 카탈로그에 수록된 어떠한 금융기관, 공공기관, 소프트웨어 제공사와도 제휴, 승인, 후원 관계가 없습니다. 카탈로그 수록은 해당 소프트웨어에 대한 검증이나 추천을 의미하지 않으며, 항목의 정확성과 최신성은 커뮤니티 기여에 기반한 best effort로 유지됩니다.

* [NOTICE.md](NOTICE.md): 제휴 관계 및 수록 의미에 관한 고지 (국문 및 영문)
* [TRADEMARKS.md](TRADEMARKS.md): 제3자 기관명, 서비스명, 로고의 표시 정책
* [docs/CATALOG_POLICY.md](docs/CATALOG_POLICY.md): 카탈로그 품질 및 갱신 방침
* [docs/REMOVAL_REQUEST.md](docs/REMOVAL_REQUEST.md): 정정 및 삭제 요청 처리 절차

## 라이선스

이 프로젝트는 Apache License 2.0을 따릅니다. 자세한 내용은 [LICENSE](LICENSE)를 참고해주세요.

제3자 상표와 로고 자산은 이 프로젝트의 라이선스가 사용을 허락하는 대상이 아닙니다. [TRADEMARKS.md](TRADEMARKS.md)를 함께 참고해주세요.
