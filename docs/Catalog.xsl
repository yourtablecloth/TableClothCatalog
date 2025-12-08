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
                    .compat-note {
                        margin-top: 0.75rem;
                        padding: 0.6rem 0.8rem;
                        background: #fef3c7;
                        border-left: 3px solid #f59e0b;
                        border-radius: 0 6px 6px 0;
                        font-size: 0.8rem;
                        color: #92400e;
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
                        <h1>🍽️ 식탁보 카탈로그</h1>
                        <p>이 카탈로그는 <a href="https://yourtablecloth.app/TableClothCatalog/Catalog.xml" target="_blank">식탁보 카탈로그 XML</a>을 웹에서 쉽게 탐색할 수 있도록 만든 페이지입니다. 전체 기능을 사용하시려면 <a href="https://yourtablecloth.app" target="_blank">식탁보 프로그램</a>을 다운로드하세요.</p>
                    </header>

                    <div class="filter-bar">
                        <button class="filter-btn active" onclick="filterCards('all')">전체</button>
                        <button class="filter-btn" onclick="filterCards('Banking')">🏦 은행</button>
                        <button class="filter-btn" onclick="filterCards('CreditCard')">💳 카드</button>
                        <button class="filter-btn" onclick="filterCards('Government')">🏛️ 공공기관</button>
                        <button class="filter-btn" onclick="filterCards('Financing')">💰 금융</button>
                        <button class="filter-btn" onclick="filterCards('Insurance')">🛡️ 보험</button>
                        <button class="filter-btn" onclick="filterCards('Education')">📚 교육</button>
                        <button class="filter-btn" onclick="filterCards('Security')">🔒 보안</button>
                        <button class="filter-btn" onclick="filterCards('Other')">📁 기타</button>
                    </div>

                    <div class="services-grid">
                        <xsl:for-each select="TableClothCatalog/InternetServices/Service">
                            <div class="service-card" data-category="{@Category}">
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
                                            <a href="{@Url}" target="_blank"><xsl:value-of select="@DisplayName"/></a>
                                        </div>
                                        <span class="category-badge category-{@Category}">
                                            <xsl:choose>
                                                <xsl:when test="@Category='Banking'">🏦 은행</xsl:when>
                                                <xsl:when test="@Category='CreditCard'">💳 카드</xsl:when>
                                                <xsl:when test="@Category='Government'">🏛️ 공공기관</xsl:when>
                                                <xsl:when test="@Category='Financing'">💰 금융</xsl:when>
                                                <xsl:when test="@Category='Insurance'">🛡️ 보험</xsl:when>
                                                <xsl:when test="@Category='Education'">📚 교육</xsl:when>
                                                <xsl:when test="@Category='Security'">🔒 보안</xsl:when>
                                                <xsl:otherwise>📁 기타</xsl:otherwise>
                                            </xsl:choose>
                                        </span>
                                    </div>
                                </div>
                                <div class="card-body">
                                    <xsl:if test="Packages/Package">
                                        <div class="packages-title">필요 패키지</div>
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
                                    <xsl:if test="CompatNotes">
                                        <div class="compat-note">
                                            ⚠️ <xsl:value-of select="CompatNotes"/>
                                        </div>
                                    </xsl:if>
                                </div>
                            </div>
                        </xsl:for-each>
                    </div>

                    <footer>
                        <p>© 2024 <a href="https://rkttu.com" target="_blank">rkttu.com</a>. All rights reserved.</p>
                        <p style="margin-top: 0.5rem;">Made with ❤️ for Korean Internet Banking Users</p>
                    </footer>
                </div>

                <script>
                    function filterCards(category) {
                        const cards = document.querySelectorAll('.service-card');
                        const buttons = document.querySelectorAll('.filter-btn');
                        
                        buttons.forEach(btn => btn.classList.remove('active'));
                        event.target.classList.add('active');
                        
                        cards.forEach(card => {
                            if (category === 'all' || card.dataset.category === category) {
                                card.style.display = 'block';
                            } else {
                                card.style.display = 'none';
                            }
                        });
                    }
                </script>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
