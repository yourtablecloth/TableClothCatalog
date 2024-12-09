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
                <style type="text/css" media="all">
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid black; padding: 8px; text-align: left; }
                    th { background-color: #f2f2f2; }
                </style>
            </head>
            <body>
                <h1>식탁보 카탈로그 페이지</h1>
                <p>이 카탈로그 페이지는 <a href="https://yourtablecloth.app/TableClothCatalog/Catalog.xml" target="_blank">식탁보 카탈로그 XML 페이지</a>를 쉽게 검색해볼 수 있게 만든 웹 페이지입니다. 편의를 위하여 제공되는 웹 페이지로, 전체 기능을 사용하시려면 <a href="https://yourtablecloth.app" target="_blank">식탁보 프로그램</a>을 다운로드하여 사용하시는 것을 권장합니다.</p>
                <table>
                    <thead>
                        <tr>
                            <th>카테고리</th>
                            <th>서비스 이름</th>
                            <th>패키지 정보</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Service 요소 반복 -->
                        <xsl:for-each select="TableClothCatalog/InternetServices/Service">
                            <tr>
                                <!-- Category -->
                                <td><xsl:value-of select="@Category"/></td>

                                <!-- Display Name -->
                                <td><a href="{@Url}" target="_blank"><xsl:value-of select="@DisplayName"/></a></td>

                                <!-- Packages -->
                                <td>
                                    <!-- Package 요소 반복 -->
                                    <ul>
                                        <xsl:for-each select="Packages/Package">
                                            <li>
                                                <a href="{@Url}" target="_blank"><xsl:value-of select="@Name"/></a>
                                            </li>
                                        </xsl:for-each>
                                    </ul>
                                </td>
                            </tr>
                        </xsl:for-each>
                    </tbody>
                </table>
                <p>&copy; 2024 rkttu.com, All rights reserved.</p>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
