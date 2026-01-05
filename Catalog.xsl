<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:output method="html" indent="yes" encoding="UTF-8" />
    <!-- 루트 템플릿 -->
    <xsl:template match="/">
        <html>
            <head>
                <title>식탁보 카탈로그 페이지</title>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <meta name="description" content="우리은행, KB국민은행 등 다양한 인터넷 뱅킹 서비스에서 필요한 플러그인 목록을 카테고리별로 정리한 레포트입니다." />
                <link rel="canonical" href="https://yourtablecloth.app/TableClothCatalog/" />
                <link rel="preconnect" href="https://fonts.googleapis.com" />
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="" />
                <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+KR:wght@400;500;700&amp;display=swap" rel="stylesheet" />
                <style type="text/css" media="all">
                    :root {
                        --primary-color: #2563eb;
                        --primary-hover: #1d4ed8;
                        --bg-color: #f8fafc;
                        --card-bg: #ffffff;
                        --text-primary: #1e293b;
                        --text-secondary: #64748b;
                        --border-color: #e2e8f0;
                        --shadow: 0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06);
                        --shadow-lg: 0 10px 15px -3px rgba(0,0,0,0.1), 0 4px 6px -2px rgba(0,0,0,0.05);
                        --radius: 12px;
                    }
                    * { box-sizing: border-box; margin: 0; padding: 0; }
                    body {
                        font-family: 'Noto Sans KR', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                        background-color: var(--bg-color);
                        color: var(--text-primary);
                        line-height: 1.6;
                        padding: 2rem 1rem;
                    }
                    .container {
                        max-width: 1200px;
                        margin: 0 auto;
                    }
                    header {
                        text-align: center;
                        margin-bottom: 2.5rem;
                        padding: 2rem;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        border-radius: var(--radius);
                        color: white;
                        box-shadow: var(--shadow-lg);
                    }
                    header h1 {
                        font-size: 2rem;
                        font-weight: 700;
                        margin-bottom: 0.75rem;
                    }
                    header p {
                        font-size: 0.95rem;
                        opacity: 0.95;
                        max-width: 700px;
                        margin: 0 auto;
                    }
                    header a {
                        color: #fef08a;
                        text-decoration: none;
                        font-weight: 500;
                    }
                    header a:hover {
                        text-decoration: underline;
                    }
                    .lang-switch {
                        position: absolute;
                        top: 1rem;
                        right: 1rem;
                        display: flex;
                        gap: 0.5rem;
                    }
                    .lang-btn {
                        padding: 0.4rem 0.8rem;
                        border: 1px solid rgba(255,255,255,0.3);
                        border-radius: 6px;
                        background: rgba(255,255,255,0.1);
                        color: white;
                        font-size: 0.8rem;
                        font-weight: 500;
                        cursor: pointer;
                        transition: all 0.2s;
                    }
                    .lang-btn:hover {
                        background: rgba(255,255,255,0.2);
                    }
                    .lang-btn.active {
                        background: white;
                        color: #667eea;
                    }
                    header {
                        position: relative;
                    }
                    .search-box {
                        display: flex;
                        align-items: center;
                        gap: 0.5rem;
                        margin-bottom: 1rem;
                        padding: 0.75rem 1rem;
                        background: var(--card-bg);
                        border: 1px solid var(--border-color);
                        border-radius: var(--radius);
                        box-shadow: var(--shadow);
                        transition: all 0.2s;
                    }
                    .search-box:focus-within {
                        border-color: var(--primary-color);
                        box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
                    }
                    .search-box input {
                        flex: 1;
                        border: none;
                        outline: none;
                        font-size: 0.95rem;
                        font-family: inherit;
                        color: var(--text-primary);
                        background: transparent;
                    }
                    .search-box input::placeholder {
                        color: var(--text-secondary);
                    }
                    .search-clear-btn {
                        background: none;
                        border: none;
                        cursor: pointer;
                        color: var(--text-secondary);
                        font-size: 1.2rem;
                        padding: 0;
                        display: none;
                        transition: color 0.2s;
                    }
                    .search-clear-btn:hover {
                        color: var(--text-primary);
                    }
                    .no-results {
                        grid-column: 1 / -1;
                        text-align: center;
                        padding: 3rem 1rem;
                        color: var(--text-secondary);
                        font-size: 1rem;
                        display: none;
                    }
                    .filter-bar {
                        display: flex;
                        flex-wrap: wrap;
                        gap: 0.5rem;
                        margin-bottom: 1.5rem;
                        padding: 1rem;
                        background: var(--card-bg);
                        border-radius: var(--radius);
                        box-shadow: var(--shadow);
                    }
                    .filter-btn {
                        padding: 0.5rem 1rem;
                        border: 1px solid var(--border-color);
                        border-radius: 9999px;
                        background: white;
                        color: var(--text-secondary);
                        font-size: 0.875rem;
                        font-weight: 500;
                        cursor: pointer;
                        transition: all 0.2s;
                    }
                    .filter-btn:hover, .filter-btn.active {
                        background: var(--primary-color);
                        color: white;
                        border-color: var(--primary-color);
                    }
                    .services-grid {
                        display: grid;
                        grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
                        gap: 1.25rem;
                    }
                    .service-card {
                        background: var(--card-bg);
                        border-radius: var(--radius);
                        box-shadow: var(--shadow);
                        overflow: hidden;
                        transition: transform 0.2s, box-shadow 0.2s;
                    }
                    .service-card:hover {
                        transform: translateY(-4px);
                        box-shadow: var(--shadow-lg);
                    }
                    .card-header {
                        display: flex;
                        align-items: center;
                        gap: 1rem;
                        padding: 1.25rem;
                        border-bottom: 1px solid var(--border-color);
                    }
                    .service-icon {
                        width: 48px;
                        height: 48px;
                        border-radius: 10px;
                        object-fit: contain;
                        background: #f1f5f9;
                        padding: 4px;
                        flex-shrink: 0;
                    }
                    .service-icon-placeholder {
                        width: 48px;
                        height: 48px;
                        border-radius: 10px;
                        background: linear-gradient(135deg, #e0e7ff 0%, #c7d2fe 100%);
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        font-size: 1.25rem;
                        font-weight: 700;
                        color: #4f46e5;
                        flex-shrink: 0;
                    }
                    .service-info {
                        flex: 1;
                        min-width: 0;
                    }
                    .service-name {
                        font-size: 1rem;
                        font-weight: 600;
                        color: var(--text-primary);
                        margin-bottom: 0.25rem;
                    }
                    .service-name a {
                        color: inherit;
                        text-decoration: none;
                    }
                    .service-name a:hover {
                        color: var(--primary-color);
                    }
                    .category-badge {
                        display: inline-flex;
                        align-items: center;
                        gap: 0.35rem;
                        padding: 0.2rem 0.6rem;
                        border-radius: 9999px;
                        font-size: 0.75rem;
                        font-weight: 500;
                    }
                    .category-Banking { background: #dbeafe; color: #1e40af; }
                    .category-CreditCard { background: #fce7f3; color: #9d174d; }
                    .category-Government { background: #d1fae5; color: #065f46; }
                    .category-Financing { background: #fef3c7; color: #92400e; }
                    .category-Insurance { background: #e0e7ff; color: #3730a3; }
                    .category-Education { background: #f3e8ff; color: #6b21a8; }
                    .category-Security { background: #fee2e2; color: #991b1b; }
                    .category-Other { background: #f1f5f9; color: #475569; }
                    .warning-badge {
                        display: inline-flex;
                        align-items: center;
                        gap: 0.35rem;
                        padding: 0.2rem 0.6rem;
                        border-radius: 9999px;
                        font-size: 0.75rem;
                        font-weight: 500;
                        background: #fee2e2;
                        color: #991b1b;
                        margin-left: 0.35rem;
                        cursor: pointer;
                        transition: transform 0.2s, box-shadow 0.2s;
                    }
                    .warning-badge:hover {
                        transform: scale(1.05);
                        box-shadow: 0 2px 8px rgba(153, 27, 27, 0.2);
                    }
                    .modal {
                        display: none;
                        position: fixed;
                        z-index: 1000;
                        left: 0;
                        top: 0;
                        width: 100%;
                        height: 100%;
                        background-color: rgba(0, 0, 0, 0.5);
                    }
                    .modal.show {
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }
                    .modal-content {
                        background-color: white;
                        padding: 1.5rem;
                        border-radius: var(--radius);
                        box-shadow: var(--shadow-lg);
                        max-width: 500px;
                        width: 90%;
                        max-height: 80vh;
                        overflow-y: auto;
                        animation: slideIn 0.3s ease-out;
                    }
                    @keyframes slideIn {
                        from {
                            transform: translateY(-50px);
                            opacity: 0;
                        }
                        to {
                            transform: translateY(0);
                            opacity: 1;
                        }
                    }
                    .modal-header {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        margin-bottom: 1rem;
                        padding-bottom: 1rem;
                        border-bottom: 1px solid var(--border-color);
                    }
                    .modal-header h2 {
                        font-size: 1.25rem;
                        color: var(--text-primary);
                        margin: 0;
                        display: flex;
                        align-items: center;
                        gap: 0.5rem;
                    }
                    .modal-close-btn {
                        background: none;
                        border: none;
                        font-size: 1.5rem;
                        cursor: pointer;
                        color: var(--text-secondary);
                        transition: color 0.2s;
                    }
                    .modal-close-btn:hover {
                        color: var(--text-primary);
                    }
                    .modal-body {
                        color: var(--text-primary);
                        line-height: 1.6;
                        white-space: pre-wrap;
                        word-wrap: break-word;
                    }
                    .modal-footer {
                        margin-top: 1.5rem;
                        padding-top: 1rem;
                        border-top: 1px solid var(--border-color);
                        text-align: right;
                    }
                    .modal-close-footer-btn {
                        padding: 0.6rem 1.5rem;
                        background: var(--primary-color);
                        color: white;
                        border: none;
                        border-radius: 8px;
                        font-size: 0.9rem;
                        font-weight: 500;
                        cursor: pointer;
                        transition: background 0.2s;
                    }
                    .modal-close-footer-btn:hover {
                        background: var(--primary-hover);
                    }
                    .keywords-title {
                        font-size: 0.75rem;
                        font-weight: 600;
                        color: var(--text-secondary);
                        text-transform: uppercase;
                        letter-spacing: 0.05em;
                        margin-top: 0.75rem;
                        margin-bottom: 0.5rem;
                    }
                    .keywords-list {
                        display: flex;
                        flex-wrap: wrap;
                        gap: 0.4rem;
                    }
                    .keyword-tag {
                        display: inline-block;
                        padding: 0.25rem 0.5rem;
                        background: #e0e7ff;
                        border-radius: 4px;
                        font-size: 0.75rem;
                        color: #3730a3;
                    }
                    .card-body {
                        padding: 1rem 1.25rem 1.25rem;
                    }
                    .packages-title {
                        font-size: 0.75rem;
                        font-weight: 600;
                        color: var(--text-secondary);
                        text-transform: uppercase;
                        letter-spacing: 0.05em;
                        margin-bottom: 0.5rem;
                    }
                    .packages-list {
                        list-style: none;
                        display: flex;
                        flex-wrap: wrap;
                        gap: 0.4rem;
                    }
                    .packages-list li {
                        display: inline-block;
                    }
                    .package-link {
                        display: inline-block;
                        padding: 0.3rem 0.65rem;
                        background: #f1f5f9;
                        border-radius: 6px;
                        font-size: 0.8rem;
                        color: var(--text-secondary);
                        text-decoration: none;
                        transition: all 0.15s;
                    }
                    .package-link:hover {
                        background: var(--primary-color);
                        color: white;
                    }

                    footer {
                        text-align: center;
                        margin-top: 3rem;
                        padding: 1.5rem;
                        color: var(--text-secondary);
                        font-size: 0.875rem;
                    }
                    footer a {
                        color: var(--primary-color);
                        text-decoration: none;
                    }
                    footer a:hover {
                        text-decoration: underline;
                    }
                    @media (max-width: 640px) {
                        body { padding: 1rem 0.75rem; }
                        header { padding: 1.5rem 1rem; }
                        header h1 { font-size: 1.5rem; }
                        .services-grid { grid-template-columns: 1fr; }
                        .filter-bar { justify-content: center; }
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <header>
                        <div class="lang-switch">
                            <button class="lang-btn active" onclick="switchLang('ko')">KO</button>
                            <button class="lang-btn" onclick="switchLang('en')">EN</button>
                        </div>
                        <h1 data-i18n="title">🍽️ 식탁보 카탈로그</h1>
                        <p data-i18n="desc1">이 카탈로그는 <a href="https://yourtablecloth.app/TableClothCatalog/Catalog.xml" target="_blank" data-i18n="catalogXml">식탁보 카탈로그 XML</a>을 웹에서 쉽게 탐색할 수 있도록 만든 페이지입니다.</p>
                        <p style="margin-top: 1rem; font-size: 0.9rem;" data-i18n="desc2">🔧 웹 사이트 추가, 수정, 삭제 건의는 <a href="https://forms.gle/28ZTZyorVCYd4N8F6" target="_blank">Google Forms</a> 또는 <a href="https://github.com/yourtablecloth/TableClothCatalog" target="_blank">GitHub 리포지터리</a>에 제출해주세요.</p>
                        <p style="margin-top: 0.5rem; font-size: 0.9rem;" data-i18n="desc3">📥 전체 기능을 사용하려면 최신 버전의 <a href="https://yourtablecloth.app/" target="_blank" data-i18n="tablecloth">식탁보</a>를 다운로드하여 설치하세요.</p>
                    </header>

                    <div class="search-box">
                        <span style="font-size: 1.1rem;">🔍</span>
                        <input type="text" id="searchInput" data-i18n-placeholder="searchPlaceholder" placeholder="서비스명, 카테고리, 패키지명 검색..." oninput="performSearch()" />
                        <button class="search-clear-btn" id="searchClear" onclick="clearSearch()" data-i18n-title="clearSearch" title="검색 초기화">✕</button>
                    </div>

                    <div class="filter-bar">
                        <button class="filter-btn active" onclick="filterCards('all')" data-i18n="filterAll">전체</button>
                        <button class="filter-btn" onclick="filterCards('Banking')" data-i18n="filterBanking">🏦 은행</button>
                        <button class="filter-btn" onclick="filterCards('CreditCard')" data-i18n="filterCreditCard">💳 카드</button>
                        <button class="filter-btn" onclick="filterCards('Government')" data-i18n="filterGovernment">🏛️ 공공기관</button>
                        <button class="filter-btn" onclick="filterCards('Financing')" data-i18n="filterFinancing">💰 금융</button>
                        <button class="filter-btn" onclick="filterCards('Insurance')" data-i18n="filterInsurance">🛡️ 보험</button>
                        <button class="filter-btn" onclick="filterCards('Education')" data-i18n="filterEducation">📚 교육</button>
                        <button class="filter-btn" onclick="filterCards('Security')" data-i18n="filterSecurity">💹 증권</button>
                        <button class="filter-btn" onclick="filterCards('Other')" data-i18n="filterOther">📁 기타</button>
                    </div>

                    <div class="services-grid">
                        <xsl:for-each select="TableClothCatalog/InternetServices/Service">
                            <div class="service-card" data-category="{@Category}" data-id="{@Id}" data-name="{@DisplayName}" data-name-en="{@en-US-DisplayName}" data-packages="{Packages/Package/@Name}" data-keywords="{SearchKeywords}">
                                <div class="card-header">
                                    <img class="service-icon" 
                                         src="images/{@Category}/{@Id}.png" 
                                         alt="{@DisplayName}"
                                         onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';" />
                                    <div class="service-icon-placeholder" style="display:none;">
                                        <xsl:value-of select="substring(@DisplayName, 1, 1)"/>
                                    </div>
                                    <div class="service-info">
                                        <div class="service-name">
                                            <a href="{@Url}" target="_blank" data-name-ko="{@DisplayName}" data-name-en="{@en-US-DisplayName}"><xsl:value-of select="@DisplayName"/></a>
                                        </div>
                                        <span class="category-badge category-{@Category}">
                                            <xsl:choose>
                                                <xsl:when test="@Category='Banking'">🏦 은행</xsl:when>
                                                <xsl:when test="@Category='CreditCard'">💳 카드</xsl:when>
                                                <xsl:when test="@Category='Government'">🏛️ 공공기관</xsl:when>
                                                <xsl:when test="@Category='Financing'">💰 금융</xsl:when>
                                                <xsl:when test="@Category='Insurance'">🛡️ 보험</xsl:when>
                                                <xsl:when test="@Category='Education'">📚 교육</xsl:when>
                                                <xsl:when test="@Category='Security'">💹 증권</xsl:when>
                                                <xsl:otherwise>📁 기타</xsl:otherwise>
                                            </xsl:choose>
                                        </span>
                                        <xsl:if test="CompatNotes">
                                            <span class="warning-badge" onclick="openModal(event)" data-compat-notes="{CompatNotes}" data-compat-notes-en="{en-US-CompatNotes}" data-i18n-title="viewDetails" title="상세 정보 보기" data-i18n="warning">⚠️ 주의</span>
                                        </xsl:if>
                                    </div>
                                </div>
                                <div class="card-body">
                                    <xsl:if test="Packages/Package">
                                        <div class="packages-title" data-i18n="requiredPackages">필요 패키지</div>
                                        <ul class="packages-list">
                                            <xsl:for-each select="Packages/Package">
                                                <li>
                                                    <a class="package-link" href="{@Url}" target="_blank">
                                                        <xsl:value-of select="@Name"/>
                                                    </a>
                                                </li>
                                            </xsl:for-each>
                                        </ul>
                                    </xsl:if>
                                    <xsl:if test="SearchKeywords">
                                        <div class="keywords-title" data-i18n="searchKeywords">검색 키워드</div>
                                        <div class="keywords-list" data-keywords="{SearchKeywords}"></div>
                                    </xsl:if>
                                </div>
                            </div>
                        </xsl:for-each>
                        <div class="no-results" id="noResults" data-i18n="noResults">
                            🔍 검색 결과가 없습니다.
                        </div>
                    </div>

                    <footer>
                        <p>© 2024 <a href="https://rkttu.com" target="_blank">rkttu.com</a>. All rights reserved.</p>
                        <p style="margin-top: 0.5rem;" data-i18n="madeWith">Made with ❤️ for Korean Internet Banking Users</p>
                    </footer>
                </div>

                <div id="compatModal" class="modal">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h2 data-i18n="compatTitle">⚠️ 호환성 안내</h2>
                            <button class="modal-close-btn" onclick="closeModal()">✕</button>
                        </div>
                        <div class="modal-body" id="modalText"></div>
                        <div class="modal-footer">
                            <button class="modal-close-footer-btn" onclick="closeModal()" data-i18n="closeBtn">닫기</button>
                        </div>
                    </div>
                </div>

                <script>
                    let currentFilter = 'all';
                    let currentLang = 'ko';

                    const translations = {
                        ko: {
                            title: '🍽️ 식탁보 카탈로그',
                            desc1: '이 카탈로그는 <a href="https://yourtablecloth.app/TableClothCatalog/Catalog.xml" target="_blank">식탁보 카탈로그 XML</a>을 웹에서 쉽게 탐색할 수 있도록 만든 페이지입니다.',
                            desc2: '🔧 웹 사이트 추가, 수정, 삭제 건의는 <a href="https://forms.gle/28ZTZyorVCYd4N8F6" target="_blank">Google Forms</a> 또는 <a href="https://github.com/yourtablecloth/TableClothCatalog" target="_blank">GitHub 리포지터리</a>에 제출해주세요.',
                            desc3: '📥 전체 기능을 사용하려면 최신 버전의 <a href="https://yourtablecloth.app/" target="_blank">식탁보</a>를 다운로드하여 설치하세요.',
                            catalogXml: '식탁보 카탈로그 XML',
                            tablecloth: '식탁보',
                            searchPlaceholder: '서비스명, 카테고리, 패키지명 검색...',
                            clearSearch: '검색 초기화',
                            filterAll: '전체',
                            filterBanking: '🏦 은행',
                            filterCreditCard: '💳 카드',
                            filterGovernment: '🏛️ 공공기관',
                            filterFinancing: '💰 금융',
                            filterInsurance: '🛡️ 보험',
                            filterEducation: '📚 교육',
                            filterSecurity: '💹 증권',
                            filterOther: '📁 기타',
                            requiredPackages: '필요 패키지',
                            searchKeywords: '검색 키워드',
                            warning: '⚠️ 주의',
                            viewDetails: '상세 정보 보기',
                            noResults: '🔍 검색 결과가 없습니다.',
                            madeWith: 'Made with ❤️ for Korean Internet Banking Users',
                            compatTitle: '⚠️ 호환성 안내',
                            closeBtn: '닫기',
                            categoryBanking: '🏦 은행',
                            categoryCreditCard: '💳 카드',
                            categoryGovernment: '🏛️ 공공기관',
                            categoryFinancing: '💰 금융',
                            categoryInsurance: '🛡️ 보험',
                            categoryEducation: '📚 교육',
                            categorySecurity: '💹 증권',
                            categoryOther: '📁 기타'
                        },
                        en: {
                            title: '🍽️ TableCloth Catalog',
                            desc1: 'This catalog is a page for easily browsing the <a href="https://yourtablecloth.app/TableClothCatalog/Catalog.xml" target="_blank">TableCloth Catalog XML</a> on the web.',
                            desc2: '🔧 To request additions, modifications, or deletions of websites, please submit via <a href="https://forms.gle/28ZTZyorVCYd4N8F6" target="_blank">Google Forms</a> or <a href="https://github.com/yourtablecloth/TableClothCatalog" target="_blank">GitHub repository</a>.',
                            desc3: '📥 To use all features, download and install the latest version of <a href="https://yourtablecloth.app/" target="_blank">TableCloth</a>.',
                            catalogXml: 'TableCloth Catalog XML',
                            tablecloth: 'TableCloth',
                            searchPlaceholder: 'Search services, categories, packages...',
                            clearSearch: 'Clear search',
                            filterAll: 'All',
                            filterBanking: '🏦 Banking',
                            filterCreditCard: '💳 Credit Card',
                            filterGovernment: '🏛️ Government',
                            filterFinancing: '💰 Financing',
                            filterInsurance: '🛡️ Insurance',
                            filterEducation: '📚 Education',
                            filterSecurity: '💹 Securities',
                            filterOther: '📁 Other',
                            requiredPackages: 'Required Packages',
                            searchKeywords: 'Search Keywords',
                            warning: '⚠️ Warning',
                            viewDetails: 'View details',
                            noResults: '🔍 No results found.',
                            madeWith: 'Made with ❤️ for Korean Internet Banking Users',
                            compatTitle: '⚠️ Compatibility Notice',
                            closeBtn: 'Close',
                            categoryBanking: '🏦 Banking',
                            categoryCreditCard: '💳 Credit Card',
                            categoryGovernment: '🏛️ Government',
                            categoryFinancing: '💰 Financing',
                            categoryInsurance: '🛡️ Insurance',
                            categoryEducation: '📚 Education',
                            categorySecurity: '💹 Securities',
                            categoryOther: '📁 Other'
                        }
                    };

                    const categoryMap = {
                        'Banking': 'categoryBanking',
                        'CreditCard': 'categoryCreditCard',
                        'Government': 'categoryGovernment',
                        'Financing': 'categoryFinancing',
                        'Insurance': 'categoryInsurance',
                        'Education': 'categoryEducation',
                        'Security': 'categorySecurity',
                        'Other': 'categoryOther'
                    };

                    function switchLang(lang) {
                        currentLang = lang;
                        const t = translations[lang];
                        
                        // 언어 버튼 활성화 상태 변경
                        document.querySelectorAll('.lang-btn').forEach(btn => {
                            btn.classList.toggle('active', btn.textContent === lang.toUpperCase());
                        });

                        // data-i18n 속성이 있는 요소들 번역
                        const htmlKeys = ['desc1', 'desc2', 'desc3'];
                        document.querySelectorAll('[data-i18n]').forEach(el => {
                            const key = el.getAttribute('data-i18n');
                            if (t[key]) {
                                if (htmlKeys.includes(key)) {
                                    el.innerHTML = t[key];
                                } else {
                                    el.textContent = t[key];
                                }
                            }
                        });

                        // placeholder 번역
                        document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
                            const key = el.getAttribute('data-i18n-placeholder');
                            if (t[key]) {
                                el.placeholder = t[key];
                            }
                        });

                        // title 속성 번역
                        document.querySelectorAll('[data-i18n-title]').forEach(el => {
                            const key = el.getAttribute('data-i18n-title');
                            if (t[key]) {
                                el.title = t[key];
                            }
                        });

                        // 카테고리 뱃지 번역
                        document.querySelectorAll('.category-badge').forEach(badge => {
                            const card = badge.closest('.service-card');
                            if (card) {
                                const category = card.dataset.category;
                                const key = categoryMap[category] || 'categoryOther';
                                if (t[key]) {
                                    badge.textContent = t[key];
                                }
                            }
                        });

                        // 서비스명 번역
                        document.querySelectorAll('.service-name a').forEach(link => {
                            const nameKo = link.getAttribute('data-name-ko');
                            const nameEn = link.getAttribute('data-name-en');
                            if (lang === 'en' &amp;&amp; nameEn) {
                                link.textContent = nameEn;
                            } else if (nameKo) {
                                link.textContent = nameKo;
                            }
                        });

                        // 페이지 제목 변경
                        document.title = lang === 'ko' ? '식탁보 카탈로그 페이지' : 'TableCloth Catalog';
                    }

                    function filterCards(category) {
                        currentFilter = category;
                        const buttons = document.querySelectorAll('.filter-btn');
                        buttons.forEach(btn => btn.classList.remove('active'));
                        event.target.classList.add('active');
                        applyFiltersAndSearch();
                    }

                    function performSearch() {
                        const searchInput = document.getElementById('searchInput');
                        const clearBtn = document.getElementById('searchClear');
                        clearBtn.style.display = searchInput.value ? 'block' : 'none';
                        applyFiltersAndSearch();
                    }

                    function clearSearch() {
                        document.getElementById('searchInput').value = '';
                        document.getElementById('searchClear').style.display = 'none';
                        applyFiltersAndSearch();
                    }

                    function applyFiltersAndSearch() {
                        const cards = document.querySelectorAll('.service-card');
                        const searchTerm = document.getElementById('searchInput').value.toLowerCase();
                        let visibleCount = 0;
                        
                        cards.forEach(card => {
                            const categoryMatch = currentFilter === 'all' || card.dataset.category === currentFilter;
                            
                            let searchMatch = true;
                            if (searchTerm) {
                                const serviceId = (card.dataset.id || '').toLowerCase();
                                const serviceName = (card.dataset.name || '').toLowerCase();
                                const category = (card.dataset.category || '').toLowerCase();
                                const packages = (card.dataset.packages || '').toLowerCase();
                                const keywords = (card.dataset.keywords || '').toLowerCase();
                                searchMatch = serviceId.includes(searchTerm) ||
                                            serviceName.includes(searchTerm) || 
                                            category.includes(searchTerm) || 
                                            packages.includes(searchTerm) ||
                                            keywords.includes(searchTerm);
                            }
                            
                            const shouldShow = categoryMatch &amp;&amp; searchMatch;
                            card.style.display = shouldShow ? 'block' : 'none';
                            if (shouldShow) visibleCount++;
                        });

                        document.getElementById('noResults').style.display = visibleCount === 0 ? 'block' : 'none';
                    }

                    function openModal(event) {
                        const compatTextKo = event.target.getAttribute('data-compat-notes');
                        const compatTextEn = event.target.getAttribute('data-compat-notes-en');
                        const modalText = document.getElementById('modalText');
                        
                        if (currentLang === 'en' &amp;&amp; compatTextEn) {
                            modalText.textContent = compatTextEn;
                        } else {
                            modalText.textContent = compatTextKo;
                        }
                        
                        const modal = document.getElementById('compatModal');
                        modal.classList.add('show');
                    }

                    function closeModal() {
                        const modal = document.getElementById('compatModal');
                        modal.classList.remove('show');
                    }

                    // 모달 외부 클릭 시 닫기
                    document.addEventListener('click', function(e) {
                        const modal = document.getElementById('compatModal');
                        if (e.target === modal) {
                            closeModal();
                        }
                    });

                    // ESC 키 누를 시 모달 닫기
                    document.addEventListener('keydown', function(e) {
                        if (e.key === 'Escape') {
                            closeModal();
                        }
                    });

                    // 페이지 로드 시 키워드 렌더링
                    document.addEventListener('DOMContentLoaded', function() {
                        document.querySelectorAll('.keywords-list').forEach(container => {
                            const keywords = container.getAttribute('data-keywords');
                            if (keywords) {
                                const keywordArray = keywords.split(';').map(k => k.trim()).filter(k => k);
                                container.innerHTML = keywordArray.map(keyword => 
                                    '<span class="keyword-tag">' + keyword + '</span>'
                                ).join('');
                            }
                        });
                    });
                </script>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
