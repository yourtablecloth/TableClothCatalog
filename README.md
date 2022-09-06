# 식탁보 카탈로그

이 리포지터리는 식탁보에서 참조하는 카탈로그 XML 파일을 관리하는 리포지터리입니다.

기존 사이트에 변경이 발생했을 경우 이 리포지터리에 풀 리퀘스트를 올려주세요.

그 외에 식탁보 프로그램 자체에 대한 이슈 제보나 최신 버전 다운로드는 [이곳](https://github.com/yourtablecloth/TableCloth)을 참고해주세요.

## 컨트리뷰션 방법

카탈로그 리포지터리는 다음의 파일들로 구성됩니다.

- [docs/Catalog.xml](docs/Catalog.xml): 각 기관, 서비스 웹 사이트마다 필요로 하는 추가 소프트웨어들의 목록을 저장하고 관리하는 XML 파일입니다.
- [docs/sites.xml](docs/sites.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.5 버전부터 사용)
  - Microsoft Edge 그룹 정책에서 https://yourtablecloth.app/TableClothCatalog/sites.xml 주소를 직접 지정하여 설정을 적용할 수 있습니다.
- [docs/images/&lt;Category&gt;/&lt;Id&gt;.png](docs/images/): `<Category>`와 `<Id>`에 해당하는 식별자를 넣어 고해상도 투명 PNG 로고 이미지 파일을 등록하여 관리합니다. (예: [docs/images/Banking/WooriBank.png](docs/images/Banking/WooriBank.png))

아래 파일은 구 버전의 식탁보 프로그램에서 사용했던 파일로, 현재는 사용되지 않습니다.

- [docs/Compatibility.xml](docs/Compatibility.xml): Internet Explorer Mode로 띄우려는 웹 사이트들의 목록을 저장하고 관리하는 XML 파일입니다. (식탁보 0.5.3, 0.5.4 버전에서 사용)

위의 두 개의 파일에 새로운 항목을 추가하고 Pull Request를 보내주시면, 검토 후에 반영하도록 하겠습니다.
