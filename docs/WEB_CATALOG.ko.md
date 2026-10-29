# 통합 웹 카탈로그와 사이트별 WSB 구성 파일

[English](WEB_CATALOG.md)

2026년 10월 29일 [이슈 149번](https://github.com/yourtablecloth/TableClothCatalog/issues/149)의 구현은 CI에서 일반 HTML을 생성하고 기존 GitHub Pages의 `catalog.yourtablecloth.app`에 게시하는 구조를 사용합니다. 브라우저는 카탈로그를 XSLT로 변환하지 않습니다.

웹 카탈로그는 사이트 탐색, 보조 프로그램의 다운로드 안내, 스킬 탐색과 기여, 사이트별 Windows Sandbox 구성 파일을 제공합니다.

공개 경로와 기여 절차를 설명한 뒤 WSB 실행 범위, 빌드 순서와 검증 한계를 차례로 정리하겠습니다.

## 공개 경로와 XML 피드 보존

[사이트 생성기](../src/buildsite.py)의 출력 경로부터 살펴보겠습니다. 생성기는 전체 항목을 HTML에 포함합니다. JavaScript가 검색과 분류 필터를 제공하며 JavaScript를 사용하지 않아도 항목을 읽을 수 있습니다.

- `/`: 사이트 카탈로그와 보조 프로그램 다운로드 안내
- `/skills/`: 활성 스킬의 고정 원본, 라이선스, 런타임과 네트워크 조건
- `/wsb/{ServiceId}.wsb`: 활성 사이트마다 생성하는 단일 WSB 파일
- `/Catalog.xml`, `/SkillCatalog.xml`: 기존 애플리케이션용 XML 피드

원본 XML은 계속 데이터를 공급합니다. 생성기는 XML 주석을 활성 항목으로 표시하지 않습니다. 웹 페이지는 기존 도메인과 탐색 메뉴를 공유하며 별도 Sites 호스팅을 사용하지 않습니다.

## 스킬 탐색과 기여 검토

[스킬 페이지](https://catalog.yourtablecloth.app/skills/)에서 원본, 고정 커밋, 라이선스와 기여 안내를 확인할 수 있습니다. 설치는 식탁보 애플리케이션의 마켓플레이스를 통해 진행합니다. 웹 등재만으로 실행 호환성을 판정하지 않으며 런타임, 자격 증명이나 MCP 연결을 설치하지 않습니다. [스킬 관리 안내](SkillCatalog.README.ko.md)와 [등재 기준](SKILL_ELIGIBILITY.ko.md)에서 적용 범위를 설명합니다.

1. 대상 사이트나 스킬을 찾고 원본과 실행 조건을 확인합니다.
2. 연결된 GitHub 이슈 양식으로 변경을 제보하거나 작업 브랜치에서 `docs/Catalog.xml` 또는 `docs/SkillCatalog.xml`을 편집합니다.
3. 공식 출처 URL, 확인일, 실제 시험 결과와 미확인 항목을 기록합니다.
4. 해당 검사를 실행하고 [기여 안내](../CONTRIBUTING.md)에 따라 PR을 제출합니다.

## 사이트별 WSB 실행 범위

사이트의 `.wsb` 파일을 내려받은 뒤 Windows Sandbox를 지원하고 해당 기능을 활성화한 Windows 장치에서 파일을 엽니다. 식탁보 애플리케이션 설치 없이 이 경로를 이용할 수 있습니다. Microsoft는 [Windows Sandbox 지원 조건](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/)과 [WSB 파일 실행 방법](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file)을 안내합니다.

[WSB 생성기](../src/buildwsb.py)는 단일 파일에 부트스트랩과 사이트 데이터를 포함합니다. 게스트 부트스트랩은 카탈로그의 설치 패키지를 차례로 내려받아 설치하고 Edge 확장 정책과 게스트 초기화 명령을 적용한 뒤 Edge로 사이트를 엽니다. Sandbox는 도메인에 가입하지 않으므로 타사 확장은 수동 설치가 필요할 수 있습니다. 게스트는 설치 링크를 담은 `extension-setup.html`도 제공합니다. Microsoft는 [확장 정책 문서](https://learn.microsoft.com/deployedge/microsoft-edge-policies/extensioninstallforcelist)에서 이 제약을 설명합니다. 무인 설치 인수가 없는 설치기는 마법사를 표시할 수 있습니다. 다운로드 오류, 설치 파일 검사 실패, 설치 오류나 제한 시간 초과가 발생하면 후속 단계를 중단하고 게스트 바탕 화면의 `TableCloth-{ServiceId}/bootstrap.log`에 진단 내용을 남깁니다.

게스트는 다운로드와 사이트 접속을 위해 네트워크를 사용하며 내부망에도 접근할 수 있습니다. WSB 파일은 호스트 폴더와 인증서를 공유하지 않으며 클립보드, 마이크, 카메라, 프린터와 GPU 리디렉션을 비활성화합니다. 식탁보의 IE 모드, 인증서 공유, ASTx 설정 조정과 아키텍처 호환 처리는 지원하지 않습니다. 게스트 세션을 종료하면 파일, 설치한 프로그램과 상태를 잃으며 호스트로 내보내는 공유 폴더도 제공하지 않습니다.

생성기의 패키지 다운로드는 모든 리디렉션을 포함해 HTTPS만 허용합니다. 카탈로그에 HTTP 설치 주소가 남아 있으면 게스트는 패키지를 내려받기 전에 준비를 중단합니다. 해당 사이트의 웹 카드도 이 제한을 안내합니다. 공식 HTTPS 출처를 확인하기 전까지 카탈로그는 원래 주소를 보존합니다. 현재 DonggukUdrims의 별도 curl 다운로드에도 같은 전송 제한을 적용합니다. CustomBootstrap의 실행 코드는 별도로 검토합니다.

## 빌드 순서와 GitHub Pages 게시

출력 디렉터리를 준비하는 기존 .NET 빌더와 후속 생성기의 순서를 짚어보겠습니다. .NET 빌더 다음에 Python 표준 라이브러리를 사용하는 WSB 생성기와 HTML 생성기를 실행합니다. [Pages 워크플로](../.github/workflows/publish.yml)는 카탈로그 검증, 파일 생성과 회귀 검사를 거쳐 `outputs`를 `gh-pages`에 게시합니다.

리포지터리 루트에서 .NET 10 SDK와 Python 3으로 다음 명령을 실행할 수 있습니다.

```sh
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
python src/buildwsb.py docs/Catalog.xml outputs/wsb
python src/buildsite.py docs outputs
python -m unittest discover -s tests -v
```

생성한 페이지와 WSB 파일은 `outputs`에 저장하고 카탈로그 변경은 원본 XML에 반영합니다. 워크플로가 지정한 `main`의 관련 파일을 변경하면 HTML, WSB 파일과 XML 피드를 함께 게시합니다.

## 검증 범위와 실행 시험 근거

[회귀 검사](../tests/)는 파일 생성 계약, 이스케이프, 링크와 WSB 데이터를 검사합니다. 스키마 검사와 파일 생성만으로 설치 성공, 보안 플러그인의 Windows Sandbox 호환성이나 현재 호스트에서의 스킬 실행 성공을 판정하지 않습니다.

여기까지 정리하면 통합 카탈로그는 탐색할 데이터와 생성한 WSB 파일을 게시합니다. 호스트의 장기 실행 검증은 [스킬 등재 기록](SKILL_ELIGIBILITY.ko.md)에서 별도로 다루며 개별 카탈로그 변경은 다음 게시 결과에 바로 영향을 줄 수 있습니다.

사이트와 설치기를 판단할 때에는 정적 생성 검사와 Windows Sandbox 실행 시험을 구분할 수 있습니다. 스킬을 판단할 때에는 고정 파일 설치와 실제 기능 실행을 구분할 수 있습니다. 실행 근거를 추가할 때 시험한 OS, 아키텍처, 사이트와 패키지 버전을 함께 기록하면 적용 범위를 확인할 수 있습니다.
