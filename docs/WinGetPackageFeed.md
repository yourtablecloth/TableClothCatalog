# WinGet Package Feed

## 개요

WinGet Package Feed 도구는 Microsoft의 [winget-pkgs](https://github.com/microsoft/winget-pkgs) 리포지터리에서 최신 패키지 정보를 가져와 PackageFeed.xml 파일로 생성하는 도구입니다.

## 지원 패키지

현재 다음 패키지들의 최신 버전 정보를 자동으로 수집합니다:

- **Google Chrome** (`Google.Chrome`)
- **Microsoft Edge** (`Microsoft.Edge`)
- **Adobe Acrobat Reader 64-bit** (`Adobe.Acrobat.Reader.64-bit`)

## 생성되는 XML 파일 구조

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<PackageFeed GeneratedAt="2025-12-06T00:00:00.0000000+00:00">
  <Packages>
    <Package Id="Google.Chrome" 
             Version="131.0.6778.86" 
             InstallerUrl="https://dl.google.com/dl/chrome/install/googlechromestandaloneenterprise64.msi" />
    <Package Id="Microsoft.Edge" 
             Version="131.0.2903.63" 
             InstallerUrl="https://msedge.sf.dl.delivery.mp.microsoft.com/.../MicrosoftEdgeEnterpriseX64.msi" />
    <Package Id="Adobe.Acrobat.Reader.64-bit" 
             Version="24.004.20220" 
             InstallerUrl="https://ardownload2.adobe.com/.../AcroRdrDCx642400420220_MUI.exe" />
  </Packages>
</PackageFeed>
```

## 사용 방법

### 로컬 실행

```bash
# .NET 10 SDK 필요
dotnet run --file src/wingetfeed.cs -- ./outputs/
```

### GitHub Actions 실행

GitHub Actions 워크플로우에서는 자동으로 실행되며, `GITHUB_TOKEN` 환경 변수를 사용하여 GitHub API 인증을 수행합니다.

```yaml
- name: Run WinGet Package Feed Tool
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: dotnet run --file src/wingetfeed.cs -- ./outputs/
```

## 동작 방식

1. GitHub API를 통해 winget-pkgs 리포지터리의 매니페스트 디렉터리를 조회합니다.
2. 각 패키지의 최신 버전 디렉터리를 찾습니다 (버전 번호 기준 정렬).
3. `.installer.yaml` 파일을 다운로드하여 파싱합니다.
4. YAML 파일에서 설치 프로그램 URL을 추출합니다.
5. 수집된 정보를 XML 파일로 생성합니다.

## 인증

- GitHub API는 인증 없이 사용 시 시간당 60회 요청 제한이 있습니다.
- `GITHUB_TOKEN` 환경 변수가 설정되어 있으면 자동으로 인증을 수행합니다.
- GitHub Actions에서는 자동으로 토큰이 제공됩니다.

## 활용 사례

생성된 `PackageFeed.xml` 파일은 다음과 같은 용도로 활용될 수 있습니다:

- Chrome 수동 설치가 필요한 경우 최신 다운로드 URL 제공
- Adobe Reader 익스텐션 자동 설치 구현
- 기타 패키지 관리 도구에서 최신 버전 URL 참조

## 관련 이슈

- [yourtablecloth/TableClothCatalog#49](https://github.com/yourtablecloth/TableClothCatalog/issues/49)
