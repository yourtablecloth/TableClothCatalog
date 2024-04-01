# 식탁보 카탈로그

이 리포지터리는 식탁보에서 참조하는 카탈로그 XML 파일을 관리하는 리포지터리입니다.

기존 사이트에 변경이 발생했을 경우 이 리포지터리에 풀 리퀘스트를 올려주세요.

그 외에 식탁보 프로그램 자체에 대한 이슈 제보나 최신 버전 다운로드는 [이곳](https://github.com/yourtablecloth/TableCloth)을 참고해주세요.

## 컨트리뷰션 방법

카탈로그 리포지터리는 다음의 파일들로 구성됩니다.

- [docs/Catalog.xml](docs/Catalog.xml): 각 기관, 서비스 웹 사이트마다 필요로 하는 추가 소프트웨어들의 목록을 저장하고 관리하는 XML 파일입니다.
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
- [docs/sites.xml](docs/sites.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.5 버전부터 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.
  - Microsoft Edge 그룹 정책에서 https://yourtablecloth.app/TableClothCatalog/sites.xml 주소를 직접 지정하여 설정을 적용할 수 있습니다.
- [docs/images/&lt;Category&gt;/&lt;Id&gt;.png](docs/images/): `<Category>`와 `<Id>`에 해당하는 식별자를 넣어 고해상도 투명 PNG 로고 이미지 파일을 등록하여 관리합니다. (예: [docs/images/Banking/WooriBank.png](docs/images/Banking/WooriBank.png))

아래 파일은 구 버전의 식탁보 프로그램에서 사용했던 파일로, 현재는 사용되지 않습니다.

- [docs/Compatibility.xml](docs/Compatibility.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.3, 0.5.4 버전에서 사용)
  - XML 파일 편집은 [Microsoft XML Notepad](https://microsoft.github.io/XmlNotepad/)를 사용하시는 것을 권장합니다.

위의 두 개의 파일에 새로운 항목을 추가하고 Pull Request를 보내주시면, 검토 후에 반영하도록 하겠습니다.

### 카탈로그 빌더 실행하기

각종 유효성 검사 및 리소스 자동 생성을 위하여 CatalogBuilder를 새롭게 추가하였습니다. 새로운 커밋이 브랜치에 푸시되면, CatalogBuilder가 `outputs` 디렉터리에 `gh-pages` 브랜치에 내보낼 파일들을 복사하면서 추가 리소스 생성이나 유효성 검사를 진행하게 됩니다.

```bash
# 리포지터리 루트 디렉터리에서 아래 명령어를 실행한다고 가정합니다.

# 빌드
dotnet build src/TableCloth.CatalogBuilder/TableCloth.CatalogBuilder.csproj --configuration Release

# 실행
dotnet run --project src/TableCloth.CatalogBuilder/TableCloth.CatalogBuilder.csproj --configuration Release -- ./docs/ ./outputs/
```

도구를 실행하여 나타나는 Error나 Warning을 최대한 제거하는 것이 좋습니다. Catalog.xml 파일을 수정하여 제출하기 전에 검사를 진행하는 것을 권장합니다.

### 자동 응답 설치 옵션을 찾는 방법

일반적으로 무인 설치 옵션은 설치 프로그램 명령줄 뒤에 `/?`나 `/help`, `/h`, `--help` 등 도움말을 표시하는 것과 관련된 스위치를 대입하여 실행하면 도움말과 함께 자세한 자동 응답 설치 옵션을 사용하는 방법을 표시합니다.

만약 위의 방법을 통하여 옵션을 찾기 어려울 때에는 Universal Silent Switch Finder (USSF) 도구를 이용하여 찾는 것을 시도할 수 있습니다. 이 도구는 Windows에서 실행할 수 있으며, EXE 파일을 열면 콘텐츠를 분석하여 예상되는 자동 응답 설치 옵션 방법이나 스위치를 표시합니다.

[Universal Silent Switch Finder 홈페이지](https://www.capstanservices.com/tools-blog/2018/4/4/the-ultimate-silent-switch-finder-ussf)

자동 설치 옵션이 잘 작동하는지 확인하기 위해, Windows Sandbox, 또는 디스크 이미지 수준의 복원 스냅샷 포인트를 확보한 가상 컴퓨터 인스턴스를 활용하여 반복 테스트를 실행해볼 수 있습니다.
