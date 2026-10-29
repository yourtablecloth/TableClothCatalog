#!/usr/bin/env python3
"""Render the two public catalog pages without XSLT or third-party packages."""

import argparse
import html
from pathlib import Path
import re
import shutil
from urllib.parse import quote, urlsplit
import xml.etree.ElementTree as ET


REPOSITORY = "https://github.com/yourtablecloth/TableClothCatalog"
GUIDE = f"{REPOSITORY}/blob/main/"
CATEGORIES = {
    "Banking": "은행", "Financing": "저축은행 및 금융", "Security": "증권",
    "Insurance": "보험", "CreditCard": "카드", "Government": "공공기관",
    "Education": "교육", "Other": "기타", "Companions": "보조 프로그램",
    "Economics": "경제", "Statistics": "통계", "Documents": "문서",
    "System": "시스템", "Utilities": "생활 도구", "Finance": "금융",
}
EVALUATION_SKILLS = {"bok-ecos-stats", "kosis-stats", "tablecloth-calculators"}
BRIDGE_SKILLS = {"tablecloth-certificate-expiry", "tablecloth-windows-sandbox"}


def escape(value):
    return html.escape(str(value or ""), quote=True)


def safe_url(value):
    """Only public HTTP(S) links can become links in the rendered catalog."""
    if not value or any(ord(char) < 32 for char in value) or "\\" in value:
        return ""
    try:
        parts = urlsplit(value)
        if parts.scheme.lower() in {"http", "https"} and parts.hostname and not parts.username and not parts.password:
            return value
    except ValueError:
        pass
    return ""


def link(url, label, css=""):
    safe = safe_url(url)
    if not safe:
        return f'<span class="unavailable">{escape(label)}</span>'
    class_attribute = f' class="{escape(css)}"' if css else ""
    return f'<a href="{escape(safe)}"{class_attribute}>{escape(label)}</a>'


def read_xml(path, max_bytes=8 * 1024 * 1024):
    with path.open("rb") as stream:
        data = stream.read(max_bytes + 1)
    if len(data) > max_bytes:
        raise ValueError(f"Unsafe or oversized catalog: {path}")
    # Catalog sources use UTF-8. Decode before declaration checks so UTF-16 or
    # other encodings cannot hide a DTD behind interleaved NUL bytes.
    document = data.decode("utf-8-sig")
    if "\x00" in document or "<!DOCTYPE" in document.upper() or "<!ENTITY" in document.upper():
        raise ValueError(f"Unsafe catalog: {path}")
    return ET.fromstring(document)


def text(element, name):
    return (element.findtext(name) or "").strip()


def search_attributes(element, category, extra="", skills=False):
    values = [*element.attrib.values(), *element.itertext(), extra, category_label(category, skills)]
    # Include descendant attribute values such as package names and source authors.
    values.extend(value for child in element.iter() if child is not element for value in child.attrib.values())
    return f'data-entry data-category="{escape(category)}" data-search="{escape(" ".join(values))}"'


def category_label(category, skills=False):
    if skills and category == "Security":
        return "보안"
    return CATEGORIES.get(category, category)


def filters(categories, count, placeholder, skills=False):
    options = "".join(f'<option value="{escape(category)}">{escape(category_label(category, skills))}</option>' for category in sorted(set(categories), key=lambda value: category_label(value, skills)))
    return f'''<form class="filters" data-filters hidden role="search">
      <div class="search-field"><label for="search">검색어</label><input id="search" type="search" name="q" placeholder="{escape(placeholder)}" autocomplete="off"></div>
      <div class="category-field"><label for="category">카테고리</label><select id="category" name="category"><option value="">모든 카테고리</option>{options}</select></div>
      <button class="button secondary" type="reset">초기화</button>
    </form>
    <div class="result-line"><p data-result-count role="status" aria-live="polite">전체 {count}개</p><span>원본 XML에서 생성한 목록</span></div>
    <noscript><p class="notice">모든 항목을 표시합니다. 브라우저의 페이지 내 찾기로 검색할 수 있습니다.</p></noscript>
    <div class="empty-state" data-empty hidden><h2>검색 결과 없음</h2><p>다른 검색어나 카테고리로 탐색할 수 있습니다.</p><button type="button" class="button secondary" data-reset>전체 목록 보기</button></div>'''


def service_card(service):
    identifier = service.get("Id", "")
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]*", identifier) or ".." in identifier:
        raise ValueError(f"Unsafe service identifier: {identifier!r}")
    name = service.get("DisplayName", identifier)
    category = service.get("Category", "Other")
    english = service.get("en-US-DisplayName", "")
    packages = service.findall("./Packages/Package")
    extensions = service.findall("./EdgeExtensions/EdgeExtension")
    package_rows = []
    for package in packages:
        args = package.get("Arguments", "")
        argument = f'<code class="argument">{escape(args)}</code>' if args else '<span class="muted">설치 인수 없음</span>'
        package_rows.append(f'<li>{link(package.get("Url"), package.get("Name"))}{argument}</li>')
    package_content = f'<ul class="package-list">{"".join(package_rows)}</ul>' if packages else '<p class="muted">등록한 설치 패키지가 없습니다.</p>'
    extension_content = ""
    if extensions:
        rows = "".join(f'<li>{link(item.get("CrxUrl"), item.get("Name"))}<code>{escape(item.get("ExtensionId", ""))}</code></li>' for item in extensions)
        extension_content = f'<h4>브라우저 확장</h4><ul class="package-list">{rows}</ul>'
    notes = text(service, "CompatNotes")
    compatibility = f'<details class="compatibility"><summary>호환성 안내</summary><p>{escape(notes)}</p></details>' if notes else ""
    bootstrap = '<p class="muted">추가 시작 스크립트를 사용하는 사이트입니다.</p>' if text(service, "CustomBootstrap") else ""
    return f'''<article class="catalog-card site-card" id="site-{escape(identifier)}" {search_attributes(service, category)}>
      <div class="card-top"><span class="tag">{escape(category_label(category))}</span><a class="permalink" href="#site-{escape(identifier)}" aria-label="{escape(name)} 항목 링크">#</a></div>
      <h3>{link(service.get("Url"), name)}</h3><p class="english-name">{escape(english or identifier)}</p>
      <div class="card-actions"><a class="button primary" href="/wsb/{escape(identifier)}.wsb" download="TableCloth-{escape(identifier)}.wsb">WSB 다운로드</a>{link(service.get("Url"), "사이트 방문", "text-link")}</div>
      {compatibility}<details class="package-details"><summary>설치 패키지 {len(packages)}개{f' 및 확장 {len(extensions)}개' if extensions else ''}</summary>{package_content}{extension_content}{bootstrap}</details>
    </article>'''


def companion_card(companion):
    name = companion.get("DisplayName", companion.get("Id", ""))
    args = companion.get("Arguments", "")
    argument = f'<p class="muted">설치 인수 <code>{escape(args)}</code></p>' if args else ""
    return f'''<article class="catalog-card companion-card" {search_attributes(companion, 'Companions')}>
      <span class="tag">보조 프로그램</span><h3>{escape(name)}</h3><p class="english-name">{escape(companion.get('en-US-DisplayName', ''))}</p>{argument}<div class="card-actions">{link(companion.get('Url'), '다운로드 안내', 'text-link')}</div>
    </article>'''


def pinned_skill_url(source):
    repository = safe_url(source.get("Repository", ""))
    ref = source.get("Ref", "")
    path = source.get("Path", "")
    parts = urlsplit(repository)
    if parts.scheme != "https" or parts.netloc.lower() != "github.com" or not re.fullmatch(r"/[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+/?", parts.path) or parts.query or parts.fragment:
        raise ValueError("Skill source must identify a public GitHub repository")
    if not re.fullmatch(r"[0-9a-fA-F]{40}", ref):
        raise ValueError("Skill source must use a full commit SHA")
    segments = [] if path == "." else path.split("/")
    if any(segment in {"", ".", ".."} or "\\" in segment for segment in segments):
        raise ValueError("Unsafe skill source path")
    tail = "/".join(quote(segment, safe="") for segment in [*segments, "SKILL.md"])
    return f"{repository.rstrip('/')}/blob/{ref}/{tail}"


def skill_card(skill):
    identifier = skill.get("Id", "")
    category = skill.get("Category", "")
    source = skill.find("Source")
    author = skill.find("Author")
    license_element = skill.find("License")
    requirements = skill.find("Requirements")
    if any(item is None for item in (source, author, license_element, requirements)):
        raise ValueError(f"Missing required metadata for skill: {identifier}")
    source_url = pinned_skill_url(source)
    network = {"none": "불필요", "optional": "선택", "required": "필수"}.get(requirements.get("Network"), requirements.get("Network", "미지정"))
    if identifier in EVALUATION_SKILLS:
        condition = '<p class="skill-condition evaluation"><strong>평가용 스크립트</strong>스크립트 실행을 지원하는 호스트가 필요합니다. 운영 호환성 판단과 평가 근거는 등재 기준에서 제공합니다.</p>'
    elif identifier in BRIDGE_SKILLS:
        condition = '<p class="skill-condition"><strong>호스트 연동 조건</strong>호환되는 식탁보 호스트가 제공한 보고서를 사용합니다. 스킬 설치만으로 Windows 기능을 추가하지 않습니다.</p>'
    else:
        condition = '<p class="skill-condition"><strong>실행 조건</strong>원본 지침과 아래 요구 사항에 따라 기능 범위를 확인할 수 있습니다.</p>'
    return f'''<article class="catalog-card skill-card" id="skill-{escape(identifier)}" {search_attributes(skill, category, skills=True)}>
      <div class="card-top"><span class="tag">{escape(category_label(category, skills=True))}</span><span class="skill-id">{escape(identifier)}</span></div>
      <h3>{escape(skill.get('DisplayName', identifier))}</h3><p class="description">{escape(text(skill, 'Description'))}</p>{condition}
      <dl class="metadata"><div><dt>작성자</dt><dd>{link(author.get('Url'), author.get('Name'))}</dd></div><div><dt>라이선스</dt><dd>{link(license_element.get('Source'), license_element.get('SPDX'))}</dd></div><div><dt>런타임</dt><dd>{escape(requirements.get('Runtime'))}</dd></div><div><dt>실행 중 네트워크</dt><dd>{escape(network)}</dd></div></dl>
      <div class="card-actions">{link(source_url, '고정 원본 SKILL.md', 'button primary')}{link(source.get('Repository'), '저장소', 'text-link')}</div>
      <details class="skill-details"><summary>설치 및 실행 조건</summary><p>{escape(text(skill, 'Notes'))}</p><dl class="source-metadata"><dt>원본 경로</dt><dd><code>{escape(source.get('Path'))}</code></dd><dt>고정 커밋</dt><dd><code>{escape(source.get('Ref'))}</code></dd></dl></details>
    </article>'''


def contribute(skills=False):
    target = "스킬" if skills else "사이트"
    source = "SkillCatalog.xml" if skills else "Catalog.xml"
    criteria = f'{GUIDE}docs/SKILL_ELIGIBILITY.ko.md' if skills else f'{GUIDE}docs/SITE_HEALTH_WORKFLOW.md'
    first = "공개 저장소의 스킬 경로와 라이선스, 필수 실행 도구를 조사합니다." if skills else "기관과 제공사의 공식 안내에서 주소, 설치 패키지와 호환성 정보를 조사합니다."
    second = "40자리 원본 커밋과 같은 커밋의 라이선스, 네트워크 및 호스트 조건을 기록합니다." if skills else "변경한 항목의 공식 출처, 확인 날짜와 확인하지 못한 조건을 기록합니다."
    policy = '<p class="notice">사용자 API 키, 토큰과 클라이언트 시크릿을 환경 변수로 받는 신규 스킬의 접수는 자격 증명 처리 기능 검증까지 보류합니다. 실제 비밀 값은 기여 내용에 포함하지 않습니다.</p>' if skills else ""
    return f'''<section class="contribution" id="contribute" aria-labelledby="contribute-heading">
      <div class="section-heading"><div><p class="eyebrow">COMMUNITY</p><h2 id="contribute-heading">{target} 카탈로그 기여 절차</h2></div>{link(f'{GUIDE}CONTRIBUTING.md', '전체 기여 안내', 'text-link')}</div>
      <ol class="contribution-steps"><li><h3>공식 출처 조사</h3><p>{first}</p>{link(criteria, '조사 및 등재 기준')}</li><li><h3>변경안 작성</h3><p>{second}</p>{link(f'{REPOSITORY}/edit/main/docs/{source}', f'{source} 편집')}</li><li><h3>검증과 검토 요청</h3><p>기여 안내의 검사를 실행하고 변경 근거와 검증 범위를 담아 Pull Request를 제출합니다.</p>{link(f'{REPOSITORY}/compare', 'Pull Request 작성')}</li></ol>{policy}
      <p class="muted">GitHub 이슈로 정정과 삭제를 제보할 수 있습니다. {link(f'{REPOSITORY}/issues/new/choose', '제보 및 정정 요청')}</p>
    </section>'''


def sites_page(services, companions, skill_count):
    cards = "".join(service_card(item) for item in services)
    companion_cards = "".join(companion_card(item) for item in companions)
    categories = [item.get("Category", "Other") for item in services] + (["Companions"] if companions else [])
    return f'''<section class="hero"><div class="hero-copy"><p class="eyebrow">TABLECLOTH CATALOG</p><h1>필요한 사이트와 도구를<br>한곳에서 탐색합니다.</h1><p class="hero-description">은행, 공공기관과 생활 서비스의 설치 정보를 찾고<br class="desktop-break"> 사이트별 Windows Sandbox 파일을 내려받을 수 있습니다.</p><div class="hero-links"><a class="button primary" href="#catalog">사이트 탐색</a><a class="button secondary" href="/skills/">스킬 마켓플레이스</a></div></div><div class="hero-stats" aria-label="카탈로그 현황"><div><strong>{len(services)}</strong><span>등록 사이트</span></div><div><strong>{len(companions)}</strong><span>보조 프로그램</span></div><div><strong>{skill_count}</strong><span>탐색 가능한 스킬</span></div></div></section>
    <section class="launch-guide" aria-labelledby="launch-heading"><div><span class="eyebrow">WINDOWS SANDBOX</span><h2 id="launch-heading">설치 없이 시작하는 식탁보</h2><p>사이트의 WSB 파일을 내려받아 Windows Sandbox를 지원하는 PC에서 열 수 있습니다. Windows Sandbox 기능을 미리 활성화한 환경에서 사용합니다. 사이트별 패키지와 호환성 안내도 함께 제공합니다.</p></div><a class="text-link" href="{GUIDE}docs/WEB_CATALOG.ko.md#사이트별-wsb-실행-범위">웹 카탈로그와 실행 안내</a></section>
    <section id="catalog" class="catalog-section" aria-labelledby="catalog-heading"><div class="section-heading"><div><p class="eyebrow">BROWSE SITES</p><h2 id="catalog-heading">사이트 카탈로그</h2></div><a class="text-link" href="/Catalog.xml">원본 XML</a></div>{filters(categories, len(services) + len(companions), '서비스명, 영문명 또는 설치 패키지 검색')}
    <section data-result-section aria-label="사이트 목록"><div class="catalog-grid">{cards}</div></section>
    <section class="companions-section" data-result-section aria-labelledby="companions-heading"><div class="section-heading"><div><p class="eyebrow">COMPANIONS</p><h2 id="companions-heading">함께 사용하는 보조 프로그램</h2><p>각 제공사의 다운로드 안내 페이지로 이동합니다.</p></div></div><div class="catalog-grid companions-grid">{companion_cards}</div></section></section>{contribute()}'''


def skills_page(skills):
    cards = "".join(skill_card(item) for item in skills)
    categories = [item.get("Category", "") for item in skills]
    return f'''<section class="hero skill-hero"><div class="hero-copy"><p class="eyebrow">SKILL MARKETPLACE</p><h1>스킬의 원본과 조건을<br>살펴보고 선택합니다.</h1><p class="hero-description">문서 작성, 통계 조회와 생활 도구까지.<br class="desktop-break"> 작성자와 고정 버전, 라이선스 및 실행 조건을 함께 제공합니다.</p><div class="hero-links"><a class="button primary" href="#catalog">스킬 {len(skills)}개 탐색</a><a class="button secondary" href="#contribute">스킬 기여</a></div></div><aside class="install-guide"><span class="eyebrow">GET STARTED</span><h2>식탁보에서 스킬 설치</h2><ol><li>최신 식탁보 앱을 설치합니다.</li><li>앱의 스킬 마켓플레이스에서 항목을 선택합니다.</li><li>원본과 실행 조건을 검토한 후 설치합니다.</li></ol><a class="text-link" href="https://yourtablecloth.app/">식탁보 다운로드</a></aside></section>
    <div class="qualification-notice"><strong>설치와 실행 조건</strong><p>스킬 설치는 파일을 복사합니다. 런타임 설치, API 키 설정과 MCP 서버 연결은 별도 조건을 따릅니다. 평가용 스크립트와 호스트 연동 항목의 실행 지원 범위는 사용하는 식탁보 버전에 따라 달라집니다.</p><div>{link(f'{GUIDE}docs/SKILL_ELIGIBILITY.ko.md', '등재 기준과 검증 범위')}{link(f'{GUIDE}docs/APP_CONTAINER_POC.ko.md', '격리 실행 평가 기록')}{link(f'{GUIDE}docs/SkillCatalog.README.ko.md', '설치 및 관리 안내')}</div></div>
    <section id="catalog" class="catalog-section" aria-labelledby="catalog-heading"><div class="section-heading"><div><p class="eyebrow">BROWSE SKILLS</p><h2 id="catalog-heading">스킬 탐색</h2></div><a class="text-link" href="/SkillCatalog.xml">원본 XML</a></div>{filters(categories, len(skills), '스킬명, 작성자 또는 실행 조건 검색', skills=True)}<section data-result-section aria-label="스킬 목록"><div class="catalog-grid skills-grid">{cards}</div></section></section>{contribute(True)}'''


def render(template, content, skills=False):
    replacements = {
        "TITLE": "스킬 마켓플레이스" if skills else "사이트 탐색",
        "DESCRIPTION": "식탁보 스킬의 원본, 라이선스와 실행 조건을 탐색하고 기여 절차를 확인합니다." if skills else "사이트별 설치 정보, 보조 프로그램과 Windows Sandbox 파일을 탐색합니다.",
        "PATH": "/skills/" if skills else "/", "CONTENT": content,
        "SITES_CURRENT": "" if skills else 'aria-current="page"',
        "SKILLS_CURRENT": 'aria-current="page"' if skills else "",
    }
    # Expand only tokens in the trusted layout, never tokens that appear in data.
    return re.sub(r"@@([A-Z_]+)@@", lambda match: replacements[match[1]], template)


def build(source, output):
    source, output = Path(source).resolve(), Path(output).resolve()
    if source == output or source.is_relative_to(output) or output.is_relative_to(source):
        raise ValueError("Source and output must be separate, non-overlapping directories")
    catalog = read_xml(source / "Catalog.xml")
    skills = read_xml(source / "SkillCatalog.xml", 512 * 1024)
    if catalog.tag != "TableClothCatalog" or skills.tag != "SkillCatalog":
        raise ValueError("Unexpected catalog document root")
    services = catalog.findall("./InternetServices/Service")
    companions = catalog.findall("./Companions/Companion")
    entries = skills.findall("Skill")
    template = (source / "index.html").read_text(encoding="utf-8")
    output.mkdir(parents=True, exist_ok=True)
    (output / "skills").mkdir(exist_ok=True)
    (output / "assets").mkdir(exist_ok=True)
    (output / "index.html").write_text(render(template, sites_page(services, companions, len(entries))), encoding="utf-8")
    (output / "skills" / "index.html").write_text(render(template, skills_page(entries), True), encoding="utf-8")
    for filename in ("catalog.css", "catalog.js"):
        shutil.copyfile(source / "assets" / filename, output / "assets" / filename)
    print(f"Rendered {len(services)} sites, {len(companions)} companions and {len(entries)} skills to {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    arguments = parser.parse_args()
    build(arguments.source, arguments.output)
