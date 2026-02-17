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

## PR 제출 시 컨트리뷰션 방법

카탈로그 리포지터리는 다음의 파일들로 구성됩니다.

- [docs/Catalog.xml](docs/Catalog.xml): 각 기관, 서비스 웹 사이트마다 필요로 하는 추가 소프트웨어들의 목록을 저장하고 관리하는 XML 파일입니다.
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
- [docs/sites.xml](docs/sites.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.5 버전부터 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
  - Microsoft Edge 그룹 정책에서 <https://yourtablecloth.app/TableClothCatalog/sites.xml> 주소를 직접 지정하여 설정을 적용할 수 있습니다.
- [docs/images/&lt;Category&gt;/&lt;Id&gt;.png](docs/images/): `<Category>`와 `<Id>`에 해당하는 식별자를 넣어 고해상도 투명 PNG 로고 이미지 파일을 등록하여 관리합니다. (예: [docs/images/Banking/WooriBank.png](docs/images/Banking/WooriBank.png))
- [docs/instruction.md](docs/instruction.md): 식탁보 AI 서비스에서 사용할 시스템 프롬프트를 담는 마크다운 형식의 파일입니다. 이 파일에 금융/공공 서비스에 관한 지식, 주요 금융/공공 서비스 업무 별 웹 페이지 주소 (바로 가기)를 기재할 수 있습니다.
  - 마크다운 형식의 파일이므로, [마크다운 린터 플러그인](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint)를 사용하여 커밋/PR 제출 전 확인하는 것을 권장합니다.

아래 파일은 구 버전의 식탁보 프로그램에서 사용했던 파일로, 현재는 사용되지 않습니다.

- [docs/Compatibility.xml](docs/Compatibility.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.3, 0.5.4 버전에서 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.

위의 두 개의 파일에 새로운 항목을 추가하고 Pull Request를 보내주시면, 검토 후에 반영하도록 하겠습니다.

### 카탈로그 빌더 실행하기

각종 유효성 검사 및 리소스 자동 생성을 위하여 CatalogBuilder를 새롭게 추가하였습니다. 새로운 커밋이 브랜치에 푸시되면, CatalogBuilder가 `outputs` 디렉터리에 `gh-pages` 브랜치에 내보낼 파일들을 복사하면서 추가 리소스 생성이나 유효성 검사를 진행하게 됩니다.

```bash
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
```

도구를 실행하여 나타나는 Error나 Warning을 최대한 제거하는 것이 좋습니다. Catalog.xml 파일을 수정하여 제출하기 전에 검사를 진행하는 것을 권장합니다.

### 카탈로그 품질 관리 도구

카탈로그의 품질을 유지하기 위해 아래의 도구들을 활용할 수 있습니다. Pull Request를 제출하기 전에 이 도구들을 실행하여 문제가 없는지 확인해주세요.

#### 카탈로그 유틸리티 (catalogutil.cs)

XML 스키마 검증, 이미지 최적화, 출력 파일 생성 등을 수행하는 도구입니다.

```powershell
# 리포지터리 루트 디렉터리에서 실행
dotnet run .\src\catalogutil.cs .\docs\ .\outputs\
```

#### 이미지 검증 도구 (checkimages.cs)

각 서비스 ID에 해당하는 로고 이미지가 올바르게 존재하는지 검증하는 도구입니다. 누락된 이미지나 사용되지 않는 이미지를 찾아줍니다.

```powershell
# 리포지터리 루트 디렉터리에서 실행
dotnet run .\src\checkimages.cs .\docs\Catalog.xml .\docs\images
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

#### 💡 AI를 활용한 이미지 품질 개선

`fetchfavicon.cs` 도구는 규칙 기반으로 파비콘을 추출하고 업스케일하지만, 저해상도(16x16) 아이콘을 확대할 경우 품질이 저하될 수 있습니다. 더 나은 결과를 위해 AI 도구 활용을 권장합니다:

- **이미지 업스케일링**: [Google Gemini](https://gemini.google.com/), [ChatGPT](https://chatgpt.com/) 등의 AI 에이전트에게 저해상도 아이콘을 고해상도로 업스케일해달라고 요청할 수 있습니다.
- **로고 검색**: AI에게 특정 기관의 공식 로고 이미지를 찾아달라고 요청하면 더 정확한 결과를 얻을 수 있습니다.
- **SVG 변환**: 저해상도 래스터 이미지를 벡터(SVG)로 변환한 후 다시 고해상도 PNG로 내보내는 방법도 효과적입니다.

특히 파비콘이 없거나 매우 작은 아이콘만 제공하는 사이트의 경우, AI 도구를 활용하면 훨씬 깔끔한 로고 이미지를 얻을 수 있습니다.

### 자동 응답 설치 옵션을 찾는 방법

일반적으로 무인 설치 옵션은 설치 프로그램 명령줄 뒤에 `/?`나 `/help`, `/h`, `--help` 등 도움말을 표시하는 것과 관련된 스위치를 대입하여 실행하면 도움말과 함께 자세한 자동 응답 설치 옵션을 사용하는 방법을 표시합니다.

만약 위의 방법을 통하여 옵션을 찾기 어려울 때에는 Universal Silent Switch Finder (USSF) 도구를 이용하여 찾는 것을 시도할 수 있습니다. 이 도구는 Windows에서 실행할 수 있으며, EXE 파일을 열면 콘텐츠를 분석하여 예상되는 자동 응답 설치 옵션 방법이나 스위치를 표시합니다.

[Universal Silent Switch Finder 홈페이지](https://www.capstanservices.com/tools-blog/2018/4/4/the-ultimate-silent-switch-finder-ussf)

자동 설치 옵션이 잘 작동하는지 확인하기 위해, Windows Sandbox, 또는 디스크 이미지 수준의 복원 스냅샷 포인트를 확보한 가상 컴퓨터 인스턴스를 활용하여 반복 테스트를 실행해볼 수 있습니다.

## 라이선스

이 프로젝트는 Apache License 2.0을 따릅니다. 자세한 내용은 [LICENSE](LICENSE)를 참고해주세요.
