#!/usr/bin/env dotnet
#:property PublishAot=false

using System.Text;
using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using System.Text.Json;
using System.Xml;

/*
// 브라우저 개발 도구에서 아래 스크립트를 사용하여 내용을 수집합니다.
copy(
  [...document.querySelectorAll('a[href]')]
    .filter(a => {
      const href = a.getAttribute('href')?.trim() || '';
      return href && !/^javascript:/i.test(href) && !href.startsWith('#');
    })
    .filter((a, idx, arr) => {
      const href = a.href;
      return arr.findIndex(x => x.href === href) === idx;
    })
    .map(a => {
      let text = (a.textContent || '').trim();
      if (!text) {
        const img = a.querySelector('img[alt]');
        if (img && img.alt.trim()) {
          text = img.alt.trim();
        }
      }
      if (!text && a.title) {
        text = a.title.trim();
      }
      if (!text) {
        text = '(no text)';
      }
      text = text
        .replace(/\\/g, '\\\\')  // 백슬래시
        .replace(/"/g, '\\"')    // 큰따옴표
        .replace(/\r/g, '\\r')   // CR
        .replace(/\n/g, '\\n')   // LF
        .replace(/\t/g, '\\t');  // 탭

      return `new("${text}", "${a.href}"),`;
    })
    .join('\n')
);
*/

SiteCollection sites = [
     new("wooribank.com", "우리은행", [
          new("개인", "https://spib.wooribank.com/pib/Dream?withyou=ps"),
          new("개인뱅킹", "https://spib.wooribank.com/pib/Dream?withyou=ps"),
          new("개인 조회", "https://spib.wooribank.com/pib/Dream?withyou=PSINQ0001"),
          new("개인 이체", "https://spib.wooribank.com/pib/Dream?withyou=PSTRS0001"),
          new("개인 오픈뱅킹", "https://spib.wooribank.com/pib/Dream?withyou=PSINQ0187"),
          new("개인 공과금", "https://svc.wooribank.com/svc/Dream?withyou=PSTAX0001"),
          new("개인 예금/신탁", "https://spib.wooribank.com/pib/Dream?withyou=PSDEP0010"),
          new("개인 펀드", "https://spot.wooribank.com/pot/Dream?withyou=OWFDM0003"),
          new("개인 보험", "https://spot.wooribank.com/pot/Dream?withyou=is"),
          new("개인 대출", "https://spib.wooribank.com/pib/Dream?withyou=PSLON0001"),
          new("개인 외환/골드", "https://spib.wooribank.com/pib/Dream?withyou=PSFXD0002"),
          new("개인 퇴직연금", "https://spib.wooribank.com/pib/Dream?withyou=PSTRT0086"),
          new("개인 뱅킹관리", "https://spib.wooribank.com/pib/Dream?withyou=PSBKM0001"),
          new("개인 ISA", "https://spib.wooribank.com/pib/Dream?withyou=PSISA0004"),
          new("기업", "https://nbi.wooribank.com/nbi/Dream?withyou=bi"),
          new("기업뱅킹", "https://nbi.wooribank.com/nbi/Dream?withyou=bi"),
          new("기업 조회", "https://nbi.wooribank.com/nbi/Dream?withyou=BIINQ0096"),
          new("기업 이체", "https://nbi.wooribank.com/nbi/Dream?withyou=BITRS0028"),
          new("기업 공과금", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBNK0125"),
          new("기업 전자결제", "https://nbi.wooribank.com/nbi/Dream?withyou=BISTL0253"),
          new("기업 수표어음", "https://nbi.wooribank.com/nbi/Dream?withyou=BIINQ0033"),
          new("기업 자금관리", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBNK0080"),
          new("기업 예금/신탁", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBNK0045"),
          new("기업 대출", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBNK0073"),
          new("기업 펀드/보험", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBNK0012"),
          new("기업 외환", "https://nbi.wooribank.com/nbi/Dream?withyou=BIFXD0031"),
          new("기업 퇴직연금", "https://svc.wooribank.com/svc/Dream?withyou=rp"),
          new("기업 뱅킹관리", "https://nbi.wooribank.com/nbi/Dream?withyou=BIBKM0004"),
          new("은행소개", "https://spot.wooribank.com/pot/Dream?withyou=bp"),
          new("자산관리", "https://spot.wooribank.com/pot/Dream?withyou=wa"),
          new("MY자산 진단", "https://spot.wooribank.com/pot/Dream?withyou=WAASM0006"),
          new("펀드 포트폴리오", "https://spot.wooribank.com/pot/Dream?withyou=WAFND0001"),
          new("퇴직연금 포트폴리오", "https://spot.wooribank.com/pot/Dream?withyou=WARPS0001"),
          new("WON챌린지", "https://spot.wooribank.com/pot/Dream?withyou=WACLG0001"),
          new("미래설계", "https://spot.wooribank.com/pot/Dream?withyou=WAAPL0001"),
          new("금융상품", "https://spot.wooribank.com/pot/Dream?withyou=po"),
          new("금융상품", "https://spot.wooribank.com/pot/Dream?withyou=po"),
          new("추천상품", "https://spot.wooribank.com/pot/Dream?withyou=PORMG0002"),
          new("예금", "https://spot.wooribank.com/pot/Dream?withyou=PODEP0001"),
          new("펀드", "https://spot.wooribank.com/pot/Dream?withyou=OWFDM0003"),
          new("대출", "https://spot.wooribank.com/pot/Dream?withyou=ln"),
          new("외환", "https://spot.wooribank.com/pot/Dream?withyou=fx"),
          new("골드", "https://spot.wooribank.com/pot/Dream?withyou=POGLD0001"),
          new("신탁", "https://spot.wooribank.com/pot/Dream?withyou=POTRT0001"),
          new("보험", "https://spot.wooribank.com/pot/Dream?withyou=POBAC0001"),
          new("퇴직연금", "https://svc.wooribank.com/svc/Dream?withyou=rp"),
          new("ISA", "https://spot.wooribank.com/pot/Dream?withyou=IMISA0044"),
          new("제휴제안", "https://nbi.wooribank.com/nbi/Dream?withyou=BISVC0104"),
          new("전체메뉴", "https://spib.wooribank.com/pib/jcc?withyou=CMCOM0408&__ID=c027277"),
          new("은행권 소상공인 금융지원 방안 안내", "https://spib.wooribank.com/pib/Dream?withyou=BPPCT0013&tabNo=4"),
          new("비정상거처 이주지원 버팀목대출 안내 주거취약계층의 이주를 지원하여 희망을 밝혀드립니다!", "https://spot.wooribank.com/pot/Dream?withyou=BPPBC0009&bbsMode=view&BOARD_ID=B00070&ARTICLE_ID=53503"),
          new("비대면 계좌개설 안심차단 시스템", "https://spot.wooribank.com/pot/Dream?withyou=BPPBC0009&bbsMode=view&BOARD_ID=B00070&ARTICLE_ID=60738"),
          new("우리WON뱅킹 기업 기업금융을 스마트폰으로 언제 어디서나 편리하게~", "https://spot.wooribank.com/pot/Dream?withyou=BPPBC0009&bbsMode=view&BOARD_ID=B00070&ARTICLE_ID=42918"),
          new("다른금융정보조회", "https://spib.wooribank.com/pib/Dream?withyou=PSINQ0180"),
          new("조회", "https://spib.wooribank.com/pib/Dream?withyou=PSINQ0001"),
          new("이체", "https://spib.wooribank.com/pib/Dream?withyou=PSTRS0001"),
          new("환율", "https://spot.wooribank.com/pot/Dream?withyou=FXXRT0011"),
          new("공과금", "https://svc.wooribank.com/svc/Dream?withyou=PSTAX0001"),
          new("뱅킹관리", "https://spib.wooribank.com/pib/Dream?withyou=PSBKM0001"),
          new("고객광장", "https://spot.wooribank.com/pot/Dream?withyou=cq"),
          new("금융 소비자 보호", "https://spot.wooribank.com/pot/Dream?withyou=CQCSD0006"),
          new("보안뉴스", "https://spot.wooribank.com/pot/Dream?withyou=CQSCT0116"),
          new("상품/약관 공시", "https://spot.wooribank.com/pot/Dream?withyou=BPPBC0012"),
          new("금융상품", "https://spot.wooribank.com/pot/Dream?withyou=po"),
          new("예금", "https://spot.wooribank.com/pot/Dream?withyou=PODEP0001"),
          new("대출", "https://spot.wooribank.com/pot/Dream?withyou=ln"),
          new("펀드", "https://spot.wooribank.com/pot/Dream?withyou=OWFDM0003"),
          new("외환", "https://spot.wooribank.com/pot/Dream?withyou=fx"),
          new("신탁", "https://spot.wooribank.com/pot/Dream?withyou=POTRT0001"),
          new("퇴직연금", "https://svc.wooribank.com/svc/Dream?withyou=rp"),
          new("보험", "https://spot.wooribank.com/pot/Dream?withyou=POBAC0001"),
          new("ISA", "https://spot.wooribank.com/pot/Dream?withyou=IMISA0044"),
          new("카드", "https://spib.wooribank.com/pib/Dream?withyou=CMCOM0126"),
          new("우리 아이 통장 만들고 최대 2만원 받으세요!", "https://spot.wooribank.com/pot/Dream?withyou=EVEVT0001&cc=c001308:c001386&NO=2588"),
          new("우리은행 첫 적금 만들고 1만원 받으세요!", "https://spot.wooribank.com/pot/Dream?withyou=EVEVT0001&cc=c001308:c001386&NO=2565"),
          new("청소년 통장 만들고 최대 2만원 받으세요!", "https://spot.wooribank.com/pot/Dream?withyou=EVEVT0001&cc=c001308:c001386&NO=3724"),
          new("우리금융지주", "https://www.woorifg.com/"),
          new("우리신용정보", "https://www.wooricredit.com/"),
          new("우리카드", "https://www.wooricard.com/"),
          new("우리펀드서비스", "http://www.woorifs.co.kr/"),
          new("우리금융캐피탈", "https://www.woorifcapital.com/"),
          new("우리PE", "http://www.wooripe.com/"),
          new("우리투자증권", "https://www.wooriib.com/"),
          new("우리FIS", "http://www.woorifis.com/"),
          new("우리자산신탁", "http://www.wooriat.com/"),
          new("우리금융경영연구소", "http://www.wfri.re.kr/"),
          new("우리자산운용", "http://www.wooriam.kr/"),
          new("우리다문화장학재단", "https://www.woorifoundation.or.kr/"),
          new("우리금융저축은행", "https://www.woorisavingsbank.com/"),
          new("우리미소금융재단", "http://www.woorimiso.or.kr/"),
          new("동양생명", "https://www.myangel.co.kr/"),
          new("ABL생명", "http://www.abllife.co.kr/"),
          new("우리은행 facebook(페이스북) 이동", "https://www.facebook.com/wooribank"),
          new("우리은행 instagram(인스타그램) 이동", "https://www.instagram.com/wooribank_kr/"),
          new("우리은행 네이버 블로그 이동", "https://blog.naver.com/woori_official"),
          new("우리은행 youtube(유튜브) 이동", "https://www.youtube.com/user/wooribank"),
          new("우리은행 네이버TV 이동", "http://tv.naver.com/wooribanktv"),
          new("우리은행", "https://www.wooribank.com/"),
          new("은행소개", "https://spot.wooribank.com/pot/Dream?withyou=bp"),
          new("고객광장", "https://spot.wooribank.com/pot/Dream?withyou=cq"),
          new("개인정보처리방침", "https://spot.wooribank.com/pot/Dream?withyou=CQSCT0048"),
          new("신용정보활용체제", "https://spot.wooribank.com/pot/Dream?withyou=CQSCT0132"),
          new("개인신용정보관리보호", "https://spot.wooribank.com/pot/Dream?withyou=CQSCT0049"),
          new("사고신고", "https://spot.wooribank.com/pot/Dream?withyou=CQACR0001"),
          new("전자민원접수", "https://spot.wooribank.com/pot/Dream?withyou=CQCSD0001"),
          new("보호금융상품등록부", "https://spot.wooribank.com/pot/Dream?withyou=POTRT0033"),
          new("상품공시실", "https://spot.wooribank.com/pot/Dream?withyou=CQPNC0002"),
          new("보안센터", "https://spot.wooribank.com/pot/Dream?withyou=CQSCT0001"),
          new("웹접근성 이용안내", "https://spot.wooribank.com/pot/Dream?withyou=CQIBG0050"),
     ]),
     new("kbstar.com", "KB국민은행", [
          new("KB국민은행", "https://www.kbstar.com/"),
          new("개인", "https://obank.kbstar.com/quics?page=obank&QSL=F"),
          new("개인 조회", "https://obank.kbstar.com/quics?page=C055068&QSL=F"),
          new("개인 이체", "https://obank.kbstar.com/quics?page=C016524&QSL=F"),
          new("개인 공과금", "https://obank.kbstar.com/quics?page=C016526&QSL=F"),
          new("개인 금융상품", "https://obank.kbstar.com/quics?page=C030037&QSL=F"),
          new("개인 외환", "https://obank.kbstar.com/quics?page=C102239&QSL=F"),
          new("개인 뱅킹관리", "https://obank.kbstar.com/quics?page=C016535&QSL=F"),
          new("기업", "https://obiz.kbstar.com/quics?page=obiz&QSL=F"),
          new("기업 조회", "https://obiz.kbstar.com/quics?page=C015661&QSL=F"),
          new("기업 이체", "https://obiz.kbstar.com/quics?page=C064502&QSL=F"),
          new("기업 Star CMS", "https://obiz.kbstar.com/quics?page=C057030&QSL=F"),
          new("기업 외환", "https://obiz.kbstar.com/quics?page=C101540&QSL=F"),
          new("기업 금융상품", "https://obiz.kbstar.com/quics?page=C105749&QSL=F"),
          new("기업 자산관리", "https://omoney.kbstar.com/quics?page=onmoney&QSL=F"),
          new("부동산", "https://kbland.kr/"),
          new("퇴직연금", "https://okbfex.kbstar.com/quics?page=opensn&QSL=F"),
          new("카드", "https://www.kbcard.com/"),
          new("에스크로이체", "https://okbfex.kbstar.com/quics?page=oescrow&QSL=F"),
          new("보안센터", "https://obank.kbstar.com/quics?page=osecure&QSL=F"),
          new("인증센터(개인)", "https://obank.kbstar.com/quics?page=C018872"),
          new("인증센터(기업)", "https://obank.kbstar.com/quics?page=C100996"),
          new("주택청약", "https://oland.kbstar.com/quics?page=ohsubs&QSL=F"),
          new("국민주택채권", "https://okbfex.kbstar.com/quics?page=onhbond&QSL=F"),
          new("주택도시기금", "https://okbfex.kbstar.com/quics?page=onhouse&QSL=F"),
          new("스마트금융서비스", "https://omoney.kbstar.com/quics?page=omobile&QSL=F"),
          new("Liiv M", "https://omoney.kbstar.com/quics?page=oliivm"),
          new("KB골든라이프X", "https://www.kbgoldenlifex.com/"),
          new("KB국민인증서", "https://cert.kbstar.com/quics?page=C112161"),
          new("KB고객우대제도", "https://otalk.kbstar.com/quics?page=omember&QSL=F"),
          new("GOLD&WISE", "https://omoney.kbstar.com/quics?page=ognw&QSL=F"),
          new("은행소개", "https://omoney.kbstar.com/quics?page=oabout&QSL=F"),
          new("지점안내", "https://omoney.kbstar.com/quics?page=C016505&QSL=F"),
          new("고객센터", "https://obank.kbstar.com/quics?page=osupp&QSL=F"),
          new("KB정보광장", "https://otalk.kbstar.com/quics?page=oblog&QSL=F"),
          new("이벤트", "https://omoney.kbstar.com/quics?page=oevent&QSL=F"),
          new("희망금융클리닉", "https://omoney.kbstar.com/quics?page=ohope&QSL=F"),
          new("KB굿잡", "https://kbgoodjob.kbstar.com/"),
          new("KB의 생각", "https://kbthink.com/main.html"),
          new("English", "https://omoney.kbstar.com/quics?page=oeng&QSL=F"),
          new("Chinese", "https://omoney.kbstar.com/quics?page=ochn&QSL=F"),
          new("Japanese", "https://omoney.kbstar.com/quics?page=ojpn&QSL=F"),
          new("KB GLOBAL NETWORK", "https://kbglobal.kbstar.com/"),
          new("Japan", "https://kbglobal.kbstar.com/tk/main"),
          new("UK", "https://kbglobal.kbstar.com/ld/main"),
          new("New Zealand", "https://kbglobal.kbstar.com/auc/main"),
          new("Cambodia", "https://kbglobal.kbstar.com/cbd/main"),
          new("China", "https://www.kbstarchina.com/"),
          new("Vietnam", "https://kbglobal.kbstar.com/vtn/main"),
          new("Hong Kong", "https://omoney.kbstar.com/quics?page=C021267"),
          new("India", "https://kbglobal.kbstar.com/ind/main"),
          new("Myanmar", "https://kbglobal.kbstar.com/mmr/main"),
          new("Singapore", "https://kbglobal.kbstar.com/sg/main"),
          new("로그인", "https://obank.kbstar.com/quics?page=C019088&QSL=F"),
          new("개인", "https://obank.kbstar.com/quics?page=C018872&QSL=F"),
          new("기업", "https://obank.kbstar.com/quics?page=C100996&QSL=F"),
          new("바로가기", "https://otalk.kbstar.com/quics?page=C027979&bbsMode=view&articleId=138965&QSL=F"),
          new("바로가기", "https://omoney.kbstar.com/quics?page=C027860"),
          new("계좌이체", "https://obank.kbstar.com/quics?page=C055027&QSL=F"),
          new("소비자보호", "https://obank.kbstar.com/quics?page=C036336&QSL=F"),
          new("상담/예약", "https://obank.kbstar.com/quics?page=C029656&QSL=F"),
          new("상품공시실", "https://obank.kbstar.com/quics?page=C022180&QSL=F"),
          new("계좌종합관리(구.ID모아보기) 서비스 이용약관 개정 안내\n\t\t\t\t\t\t\t\n\t\t\t\t\t\t\t09.16", "https://otalk.kbstar.com/quics?page=C019391&bbsMode=view&articleId=141025&QSL=F"),
          new("2025년 전 금융권 「숨은 금융자산(예금) 찾아주기」 캠페인 실시 안내\n\t\t\t\t\t\t\t\n\t\t\t\t\t\t\t09.15", "https://otalk.kbstar.com/quics?page=C019391&bbsMode=view&articleId=140939&QSL=F"),
          new("KB GOLD&WISE 고객 대상「KB골든라이프 Plus+센터」신설 안내\n                            \n\t\t\t\t\t\t\t09.12", "https://otalk.kbstar.com/quics?page=C019391&bbsMode=view&articleId=140923&QSL=F"),
          new("바로가기", "https://otalk.kbstar.com/quics?page=C019391&QSL=F"),
          new("케이봇쌤 가입하면 역대급 경품이 와르르♥  투자 꿀잼 케이봇쌤과 함께하는 경품 페스타 이벤트!\n\t\t\t\t\t\t\t\n\t\t\t\t\t\t\t09.16~ 11.30", "https://omoney.kbstar.com/quics?page=C016559&cc=b033091:b032977&%EC%9D%B4%EB%B2%A4%ED%8A%B8%EC%9D%BC%EB%A0%A8%EB%B2%88%ED%98%B8=348881&QSL=F"),
          new("황금빛 노후생활을 위한 평생금융 파트너 KB골든라이프센터!\n\t\t\t\t\t\t\t\n\t\t\t\t\t\t\t08.20 ~ 10.20", "https://omoney.kbstar.com/quics?page=C016559&cc=b033091:b032977&%EC%9D%B4%EB%B2%A4%ED%8A%B8%EC%9D%BC%EB%A0%A8%EB%B2%88%ED%98%B8=348632&QSL=F"),
          new("당신의 연금에 금(金)을 더하다! 연속 수령으로 황금박스 오픈!\n                            \n\t\t\t\t\t\t\t07.21 ~ 12.31", "https://omoney.kbstar.com/quics?page=C016559&cc=b033091:b032977&%EC%9D%B4%EB%B2%A4%ED%8A%B8%EC%9D%BC%EB%A0%A8%EB%B2%88%ED%98%B8=348359&QSL=F"),
          new("추천상품\n\t\t\t\t\t\t\t\tKB마이핏통장\n\t\t\t\t\t\t\t\t높은 이율! 수수료 혜택!목적별 자금관리를 통장 하나로!", "https://obank.kbstar.com/quics?page=C016613&cc=b061496:b061645&isNew=Y&prcode=DP01001243"),
          new("서비스\n\t\t\t\t\t\t\t\t건강한 자산관리 KB마이데이터\n\t\t\t\t\t\t\t\t각 기관에 흩어진 나의 자산을 한 곳에 모아서 관리해보세요.", "https://omoney.kbstar.com/quics?page=C043279&QSL=F"),
          new("서비스\n\t\t\t\t\t\t\t\tKB환전서비스\n\t\t\t\t\t\t\t\t최대 90%까지 환율우대 받고인터넷으로 편하게 환전 하세요", "https://obank.kbstar.com/quics?page=C101339"),
          new("서비스\n\t\t\t\t\t\t\t\tKBot SAM (케이봇쌤)\n\t\t\t\t\t\t\t\t로봇과 인간이 함께 추천하는최적의 포트폴리오", "https://omoney.kbstar.com/quics?page=onmoney"),
          new("추천상품\n\t\t\t\t\t\t\t\tKB Young Youth 적금\n\t\t\t\t\t\t\t\t나와 내아이를 위한 상품다양한 부가서비스까지 한가득", "https://obank.kbstar.com/quics?page=C016613&cc=b061496:b061645&isNew=Y&prcode=DP01000940"),
          new("추천상품\n\t\t\t\t\t\t\t\tKB Star 정기예금\n\t\t\t\t\t\t\t\tDigital KB의 대표 정기예금알아서 자동재예치/자동해지", "https://obank.kbstar.com/quics?page=C016613&cc=b061496:b061645&isNew=Y&prcode=DP01000938"),
          new("서비스\n\t\t\t\t\t\t\t\tKB에스크로 이체\n\t\t\t\t\t\t\t\t안전한 상거래를 위해 매매보호서비스를 이용하세요", "https://okbfex.kbstar.com/quics?page=oescrow"),
          new("예금", "https://obank.kbstar.com/quics?page=C016528"),
          new("펀드", "https://obank.kbstar.com/quics?page=C016529"),
          new("대출", "https://obank.kbstar.com/quics?page=C016530"),
          new("신탁", "https://obank.kbstar.com/quics?page=C016531"),
          new("ISA", "https://obank.kbstar.com/quics?page=C040686"),
          new("보험/공제", "https://obank.kbstar.com/quics?page=C016533"),
          new("골드", "https://obank.kbstar.com/quics?page=C016622"),
          new("외화예금", "https://obank.kbstar.com/quics?page=C101324"),
          new("바로가기", "https://obank.kbstar.com/quics?page=C030037"),
          new("전자금융사기예방 서비스\n\t\t\t\t\t\t\t각종 금융사기수법에 한층 강화된 다양한전자금융사기예방 서비스로 안전한인터넷뱅킹 사용이 가능합니다.", "https://obank.kbstar.com/quics?page=C034288&QSL=F"),
          new("통장(카드) 매매·양도는 불법\n\t\t\t\t\t\t\t고객님의 자산을 보호하고 금융사기를예방하기 위한 최선의 방법은대포통장 근절입니다.", "https://obank.kbstar.com/quics?page=C047785&QSL=F"),
          new("사진촬영·QR스캔 절대금지\n\t\t\t\t\t\t\t타인이 OTP/보안카드 번호를 요구(2개 초과) 하는 경우는 금융사기이니절대 응하지 마십시오.", "https://obank.kbstar.com/quics?page=C034305&boardId=722&compId=b035718&articleId=1489&bbsMode=view&viewPage=2&articleClass=&searchCondition=title&searchStr=&QSL=F"),
          new("금융감독원 바로가기", "http://www.fss.or.kr/"),
          new("미수령주식 찾기", "https://obiz.kbstar.com/quics?page=C039486&QSL=F"),
          new("은행금리비교", "https://portal.kfb.or.kr/compare/receiving_deposit_3.php"),
          new("금융상품한눈에", "http://finlife.fss.or.kr/"),
          new("금융소비자정보포털 '파인'", "https://fine.fss.or.kr/fine/main/main.do?menuNo=900000"),
          new("KB스타뱅킹", "https://omoney.kbstar.com/quics?page=C028156&QSL=F"),
          new("KB기업뱅킹", "https://omoney.kbstar.com/quics?page=C030160&QSL=F"),
          new("리브똑똑", "https://omoney.kbstar.com/quics?page=C056265&QSL=F"),
          new("이용상담", "https://obank.kbstar.com/quics?page=C019763&QSL=F"),
          new("보안프로그램", "https://obank.kbstar.com/quics?page=C040531&QSL=F"),
          new("사고신고", "https://obank.kbstar.com/quics?page=C019933&QSL=F"),
          new("보호금융상품등록부", "https://obank.kbstar.com/quics?page=C023000&QSL=F"),
          new("전자민원접수", "https://obank.kbstar.com/quics?page=C036346&QSL=F"),
          new("개인정보 처리방침", "https://obank.kbstar.com/quics?page=C108662&QSL=F"),
          new("신용정보활용체제", "https://obank.kbstar.com/quics?page=C019924&QSL=F"),
          new("위치기반서비스 이용약관", "https://obank.kbstar.com/quics?page=C110586&QSL=F"),
          new("경영공시", "https://omoney.kbstar.com/quics?page=C017681&QSL=F"),
          new("그룹 내 고객정보 제공안내", "https://obank.kbstar.com/quics?page=C040593&QSL=F"),
          new("스튜어드십 코드", "https://obank.kbstar.com/quics?page=C057315&QSL=F"),
          new("KB국민인증서 제휴문의", "https://cert.kbstar.com//quics?page=C112164"),
          new("KB금융그룹", "https://www.kbfg.com/"),
          new("KB증권", "http://www.kbsec.co.kr/"),
          new("KB손해보험", "http://www.kbinsure.co.kr/"),
          new("KB자산운용", "http://www.kbam.co.kr/"),
          new("KB캐피탈", "http://www.kbcapital.co.kr/"),
          new("KB라이프생명", "https://www.kblife.co.kr/"),
          new("KB부동산신탁", "http://www.kbret.co.kr/"),
          new("KB저축은행", "https://www.kbsavings.com/"),
          new("KB인베스트먼트", "http://www.kbic.co.kr/"),
          new("KB데이타시스템", "http://www.kds.co.kr/"),
          new("KB신용정보", "http://www.kbci.co.kr/"),
          new("KB경영연구소", "https://www.kbfg.com/kbresearch/main.do"),
          new("챗봇/채팅/이메일상담(24시간)", "https://obank.kbstar.com/quics?page=C043954"),
          new("KB국민은행은 KB국민카드의 판매대리·중개업자입니다.", "https://img2.kbstar.com/obj/ocommon/20230228_kbcard.pdf"),
          new("페이스북", "https://www.facebook.com/kbkookminbank"),
          new("인스타그램", "https://instagram.com/kbkookminbank"),
          new("YouTube", "https://www.youtube.com/user/openkbstar"),
          new("blog", "https://blog.naver.com/youngkbblog"),
          new("과학기술정보통신부 WA(WEB접근성) 품질인증 마크, 웹와치(WebWatch) 2025.7.01 ~ 2026.6.30", "http://webwatch.or.kr/"),
          new("ISMS-P 인증, Global CBPR 인증, ISO27701 인증", "https://obank.kbstar.com/quics?page=C034312"),
     ]),
];

CompanionCollection companions = [
    new("EveryonesPrinter", "모두의 프린터", "https://modu-print.com/모두의-프린터-다운로드", "", "Everyone's Printer"),
    new("HancomOfficeViewer", "한컴오피스 뷰어", "https://www.hancom.com/product/office/officeViewer", "", "Hancom Office Viewer"),
    new("AdobeAcrobatReader", "Adobe Acrobat Reader", "https://get.adobe.com/kr/reader", "", "Adobe Acrobat Reader"),
    new("RaiDrive", "RaiDrive", "https://www.raidrive.com/download", "", "RaiDrive"),
];

ServiceCollection services = [
    new("WooriBank", "우리은행 개인뱅킹", Category.Banking, "https://www.wooribank.com/", "WOORI BANK", [
         new("Veraport", "https://www.wooribank.com/download/veraportG3/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.wooribank.com/download/AnySign_Installer/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://www.wooribank.com/download/NOS/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.wooribank.com/download/IPinside/I3GSvcManager_3.0.0.24.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("KookminBank", "KB국민은행 개인뱅킹", Category.Banking, "https://www.kbstar.com/", "KB KOOKMIN BANK", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("WizInDelfino", "https://download.kbstar.com/security/wizvera/delfino/g3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Veraport", "https://download.kbstar.com/security/wizvera/veraport/g3/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KEBHanaBank", "하나은행 개인뱅킹", Category.Banking, "https://www.kebhana.com/", "KEB Hana Bank", [
         new("Veraport", "https://www.kebhana.com/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.kebhana.com/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("Delfino", "https://www.kebhana.com/wizvera/delfino/down/g3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SCWSSP", "https://www.kebhana.com/softcamp/WebSecurityStandard/SCWSSPSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("IPInside", "https://www.kebhana.com/interezen/agent/np_v6/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("ShinhanBank", "신한은행 개인뱅킹", Category.Banking, "https://www.shinhan.com/", "SHINHAN BANK", [
         new("ASTX", "https://bank.shinhan.com/sw/astx/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://bank.shinhan.com/sw/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://bank.shinhan.com/sw/raon/TouchEn/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("Printmade3", "https://bank.shinhan.com/sw/printmade/download_files/Windows/Printmade3_setup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("NHInternetBank", "NH농협은행 개인뱅킹", Category.Banking, "https://banking.nonghyup.com/", "NONGHYUP BANK", [
         new("Veraport", "https://veraport.nonghyup.com/download/20230210/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWebEx", "https://veraport.nonghyup.com/download/20230210/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey64", "https://img.nonghyup.com/install/so/raon/TouchEnNxKey/TouchEn_nxKey_Installer_64bit_new.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnKey32", "https://veraport.nonghyup.com/download/20230210/TouchEn_nxKey_Installer_32bit_new.exe", SilentSwitches.CustomSilenceSwitch),
    ], [
          new("TouchEnExtension", "https://clients2.google.com/service/update2/crx", "dncepekefegjiljlfbihljgogephdhph"),
    ])
    {
        CustomBootstrap = """
          ##### Fake MDM Provider For Microsoft Edge #####
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "EnrollmentState" /t REG_DWORD /d "1" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "EnrollmentType" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "IsFederated" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "Flags" /t REG_DWORD /d "14089087" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "AcctUId" /t REG_SZ /d "0x000000000000000000000000000000000000000000000000000000000000000000000000" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "RoamingCount" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "SslClientCertReference" /t REG_SZ /d "MY;User;0000000000000000000000000000000000000000" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "ProtoVer" /t REG_SZ /d "1.2" /f
          ##### Force Install Extension "TouchEn PC보안 확장" #####
          REG ADD "HKCU\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist" /v "1" /t REG_SZ /d "dncepekefegjiljlfbihljgogephdhph;https://clients2.google.com/service/update2/crx" /f
          """,
    },
    new("SHInternetBank", "SH수협은행 개인뱅킹", Category.Banking, "https://www.suhyup-bank.com/", "Suhyup Bank", [
         new("Veraport", "https://www.suhyup-bank.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.DefaultSilentSwitch),
         new("INISAFECrossWeb", "https://www.suhyup-bank.com/initech/crossweb/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://www.suhyup-bank.com/TouchEn_new/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("IPInside", "https://www.suhyup-bank.com/ipinside/Windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("ASTX", "https://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.DefaultSilentSwitch),
    ], [
         new ("INISAFECrossWebEx", "https://clients2.google.com/service/update2/crx", "dheimbmpmkbepjjcobigjacfepohombn"),
          new("TouchEnExtension", "https://clients2.google.com/service/update2/crx", "dncepekefegjiljlfbihljgogephdhph"),
    ])
    {
        CustomBootstrap = """
          ##### Fake MDM Provider For Microsoft Edge #####
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "EnrollmentState" /t REG_DWORD /d "1" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "EnrollmentType" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Enrollments\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "IsFederated" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "Flags" /t REG_DWORD /d "14089087" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "AcctUId" /t REG_SZ /d "0x000000000000000000000000000000000000000000000000000000000000000000000000" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "RoamingCount" /t REG_DWORD /d "0" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "SslClientCertReference" /t REG_SZ /d "MY;User;0000000000000000000000000000000000000000" /f
          REG ADD "HKLM\SOFTWARE\Microsoft\Provisioning\OMADM\Accounts\FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" /v "ProtoVer" /t REG_SZ /d "1.2" /f
          ##### Force Install Extension "IniSafe CrossWebEx" #####
          REG ADD "HKCU\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist" /v "1" /t REG_SZ /d "dheimbmpmkbepjjcobigjacfepohombn;https://clients2.google.com/service/update2/crx" /f
          ##### Force Install Extension "TouchEn PC Security Extension" #####
          REG ADD "HKCU\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist" /v "2" /t REG_SZ /d "dncepekefegjiljlfbihljgogephdhph;https://clients2.google.com/service/update2/crx" /f
          """,
    },
    new("IBKBank", "IBK기업은행 개인뱅킹", Category.Banking, "https://www.ibk.co.kr/", "Industrial Bank of Korea", [
         new("Veraport", "https://mybank.ibk.co.kr/IBK/uib/sw/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://mybank.ibk.co.kr/IBK/uib/sw/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://mybank.ibk.co.kr/IBK/uib/sw/interezen/agent/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://download.raonsecure.com/TouchEnnxKey/ibk/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("APSEngine", "https://mybank.ibk.co.kr/IBK/uib/sw/yettiesoft/APS/APS_Engine.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KDBBank", "KDB산업은행 개인뱅킹", Category.Banking, "https://www.kdb.co.kr/", "Korea Development Bank", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("AnySign", "https://banking.kdb.co.kr//bp/3rd-party-web/hsecure/anysign/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("StandardCharteredBank", "SC제일은행 개인뱅킹", Category.Banking, "https://www.standardchartered.co.kr/", "Standard Chartered Bank Korea Limited.", [
         new("Veraport", "https://open.standardchartered.co.kr/wizvera/veraport20/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://open.standardchartered.co.kr/initech/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("IPInside", "https://open.standardchartered.co.kr/interezen/install/Non/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("CitiBankKorea", "한국씨티은행 개인뱅킹", Category.Banking, "https://www.citibank.co.kr/", "Citibank Korea Inc.", [
         new("Veraport", "https://www.citibank.co.kr/3rdParty/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.citibank.co.kr/3rdParty/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.citibank.co.kr/3rdParty/raon/TouchEn/nxKey/nxKey/module/TouchEn_nxKey_Installer_32bit_MLWS.exe", SilentSwitches.CustomSilenceSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("IPInside", "https://www.citibank.co.kr/3rdParty/interezen/ipinside/agent/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("KBank", "케이뱅크 개인뱅킹", Category.Banking, "https://www.kbanknow.com/", "Kbank", [
         new("SmartManagerEx", "https://www.kbanknow.com/product/initech/smartmanagerex/extension/down/SmartManagerEX.exe", SilentSwitches.DefaultSilentSwitch),
         new("VCPP2008_Redist_x86", "https://download.microsoft.com/download/5/D/8/5D8C65CB-C849-4025-8E95-C3966CAFD8AE/vcredist_x86.exe", "/q"),
         new("MoaSign", "https://download.kbanknow.com/product/initech/moasign/down/MoaSignEXSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("IPInside", "https://download.kbanknow.com/product/interezen/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("KSCertRelay32", "https://download.kbanknow.com/product/qrcertrelay/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://download.kbanknow.com/product/markany/exe/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KISSCAP", "https://download.kbanknow.com/product/scraping/KISSCAP.exe", "/s /v/qn"),
    ], []),
    new("KakaoBank", "카카오뱅크 개인뱅킹", Category.Banking, "https://www.kakaobank.com/", "KakaoBank Corp.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("CertTool", "https://og.kakaobank.io/download/517591e5-9040-4393-b946-4de3ce14c886", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DGBDaeguBank", "DGB대구은행 개인뱅킹", Category.Banking, "https://www.dgb.co.kr/", "DGB DAEGU BANK", [
         new("Veraport", "http://varaportg3.dl.cdn.cloudn.co.kr/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "http://touchennxkey.dl.cdn.cloudn.co.kr/ebz_TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("WizInDelfino", "http://delfinog3.dl.cdn.cloudn.co.kr/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "http://epagesaferrt.dl.cdn.cloudn.co.kr/ebz_MAWS_DaeguBankRex_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BusanBank", "BNK부산은행 개인뱅킹", Category.Banking, "https://www.busanbank.co.kr/", "BNK BUSAN BANK", [
         new("Veraport", "https://ibank.busanbank.co.kr/product/install/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://ibank.busanbank.co.kr/product/install/INISAFE/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("ASTX", "http://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey64", "https://ibank.busanbank.co.kr/product/install/TouchEn_NX/nxKey/module/TouchEn_nxKey_Installer_64bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("IPInside", "https://ibank.busanbank.co.kr/product/install/IPinside/Windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("KSCertRelay64", "https://ibank.busanbank.co.kr/product/install/TouchEn_NX/nxCR/module/KSCertRelay_nx_Installer_64bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("BusanPFMS", "https://ibank.busanbank.co.kr/product/install/pfms/Windows/2.0.2.2/BusanPFMS_Setup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("GwangjuBank", "광주은행 개인뱅킹", Category.Banking, "https://pib.kjbank.com/", "KWANGJU BANK", [
         new("Veraport", "https://imgs.kjbank.com/resource/product/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://imgs.kjbank.com/resource/product/initech/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://imgs.kjbank.com/resource/product/interezen/windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("KSCertRelay32", "https://imgs.kjbank.com/resource/product/nxwqr/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TDClientAgent", "https://imgs.kjbank.com/resource/product/clipsoft/trustDoc/Install/TDClientforWindowsAgentNX_4.9.0.5.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("JejuBank", "제주은행", Category.Banking, "https://www.e-jejubank.com/", "JEJU BANK", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("UniquePC", "https://open.e-jejubank.com/ib20_com/besoft/uniPc/UniquePC_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("JbBank", "전북은행 개인뱅킹", Category.Banking, "https://www.jbbank.co.kr/", "JEONBUK BANK", [
         new("Veraport", "https://ibs.jbbank.co.kr/wizvera/veraportG3/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.2.6/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://ibs.jbbank.co.kr/TouchEnNxKey/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://ibs.jbbank.co.kr/pcInside/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("BnkKyungNamBank", "BNK경남은행 개인뱅킹", Category.Banking, "https://www.knbank.co.kr/", "BNK KYONGNAM BANK", [
         new("Veraport", "https://ibank.knbank.co.kr/product/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MGSaemaeul", "MG새마을금고 개인뱅킹", Category.Banking, "https://ibs.kfcc.co.kr/", "Korean Federation of Community Credit Cooperatives", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://ibs.kfcc.co.kr/common/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Shinhyub", "신협 개인뱅킹", Category.Banking, "https://openbank.cu.co.kr/", "National Credit Union Federation of Korea", [
         new("Veraport", "https://openbank.cu.co.kr/nonsw/wizvera/veraport/down/veraport-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://openbank.cu.co.kr/nonsw/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://download.raonsecure.com/TouchEnnxkey/shinhyup/nxkey_x86_v1.0.0.64.exe", SilentSwitches.CustomSilenceSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MaWebDRM", "https://openbank.cu.co.kr/nonsw/markany/webdrm/bin/Inst_MaWebDRM.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://openbank.cu.co.kr/nonsw/markany/eps/exe/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ePostBank", "우체국예금 개인뱅킹", Category.Banking, "https://www.epostbank.go.kr/", "Korea Post Deposit Service Division", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnNxKey", "https://www.epostbank.go.kr/sw/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.3.8/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("SecureWeb", "https://www.epostbank.go.kr/sw/softcamp/secureweb/exe/SCWSConSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("CertShare", "https://www.epostbank.go.kr/sw/xCertShare/install/CertShare_Installer.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("SanrimJohab", "SJ산림조합", Category.Banking, "https://banking.nfcf.or.kr/", "National Forestry Cooperative Federation", [
         new("Veraport", "https://banking.nfcf.or.kr/3rdParty/pib/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://banking.nfcf.or.kr/3rdParty/pib/initech/SW/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://banking.nfcf.or.kr/3rdParty/pib/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://banking.nfcf.or.kr/3rdParty/pib/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("IPInside", "https://banking.nfcf.or.kr/3rdParty/pib/interezen/install/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("SMBCSeoul", "미쓰이스미토모은행 서울지점", Category.Banking, "https://www.e-smbc.co.kr/", "Smitomo Mitsui Banking Corp. Seoul Branch", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("WooriBankBiz", "우리은행 기업뱅킹", Category.Banking, "https://nbi.wooribank.com/", "WOORI BANK (Corp.)", [
         new("I3GSvcManager", "https://www.wooribank.com/download/IPinside/I3GSvcManager_3.0.0.24.exe", SilentSwitches.CustomNoDlgSwitch),
         new("ASTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup_woori.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.3.3/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("KookminBankBiz", "KB국민은행 기업뱅킹", Category.Banking, "https://obiz.kbstar.com/quics?page=obiz", "KB KOOKMIN BANK (Corp.)", [
         new("WizInDelfino", "https://download.kbstar.com/security/wizvera/delfino/g3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Veraport", "https://download.kbstar.com/security/wizvera/veraport/g3/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KEBHanaBankBiz", "하나은행 기업뱅킹", Category.Banking, "https://biz.kebhana.com/", "KEB Hana Bank (Corp.)", [
         new("I3GSvcManager", "https://biz.kebhana.com/sw/interezen/agent/np_v6/I3GSvcManager.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEn_nxKey_Installer_32bit", "https://biz.kebhana.com/sw/raon/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("delfino-g3-sha2", "https://biz.kebhana.com/sw/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SCWSSPSetup", "https://biz.kebhana.com/sw/softcamp/WebSecurityStandard/SCWSSPSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Setup_ePageSafer(RT-Html)", "https://biz.kebhana.com/sw/markany/NOAX_MO/bin/Setup_ePageSafer(RT-Html).exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay_nx_Installer_32bit", "https://biz.kebhana.com/sw/raon/TouchEn/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("iSASService_v2.6.5", "https://biz.kebhana.com/sw/coocon/bin/iSASService_v2.6.5.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("veraport-g3-x64", "https://biz.kebhana.com/sw/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ShinhanBankBiz", "신한은행 기업뱅킹", Category.Banking, "https://bizbank.shinhan.com/", "SHINHAN BANK (Corp.)", [
         new("veraport-g3-x64", "https://bizbank.shinhan.com/sw/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INIS_EX", "https://bizbank.shinhan.com/sw/initech/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEn_nxKey_32bit", "https://bizbank.shinhan.com/sw/TouchEn/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("astx_setup_1.3.0.280", "https://bizbank.shinhan.com/sw/aos/astx/astx_setup_1.3.0.280.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WGear_Setup_SHBCIB_Prod_v1.100.1.0412_20220412_AnyCPU", "https://bizbank.shinhan.com/sw/wgear/WGear_Setup_SHBCIB_Prod_v1.100.1.0412_20220412_AnyCPU.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Printmade3_setup", "https://bizbank.shinhan.com/sw/printmade/download_files/Windows/Printmade3_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay_nx_Installer_32bit", "https://bizbank.shinhan.com/sw/TouchEn/raonnx/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("FSWSSSetup", "https://bizbank.shinhan.com/sw/fswss/FSWSSSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NHInternetBankBiz", "NH농협은행 기업뱅킹", Category.Banking, "https://ibz.nonghyup.com/", "NONGHYUP BANK (Corp.)", [
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INIS_EX_SHA2", "https://veraport.nonghyup.com/download/20230515/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEn_nxKey_Installer_32bit_new", "https://veraport.nonghyup.com/download/20230421/TouchEn_nxKey_Installer_32bit_new.exe", SilentSwitches.CustomSilenceSwitch),
         new("veraport-g3-x64", "https://veraport.nonghyup.com/download/20230303/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("SHInternetBankBiz", "SH수협은행 기업뱅킹", Category.Banking, "https://biz.suhyup-bank.com/", "Suhyup Bank (Corp.)", [
         new("veraport-g3-x64", "https://biz.suhyup-bank.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INIS_EX_SHA2", "https://biz.suhyup-bank.com/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEn_nxKey_32bit", "https://biz.suhyup-bank.com/TouchEn/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("I3GSvcManager", "https://biz.suhyup-bank.com/ipinside/Windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("astxdn", "https://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASSetup", "https://biz.suhyup-bank.com/ZeroKeeper/down/ASSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("IBKBankBiz", "IBK기업은행 기업뱅킹", Category.Banking, "https://kiup.ibk.co.kr/", "Industrial Bank of Korea (Corp.)", [
         new("veraport-g3-x64-sha2", "https://kiup.ibk.co.kr/IBK/uib/sw/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3-sha2", "https://kiup.ibk.co.kr/IBK/uib/sw/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("I3GSvcManager", "https://kiup.ibk.co.kr/IBK/uib/sw/interezen/agent/I3GSvcManager.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEn_nxKey_32bit", "https://download.raonsecure.com/TouchEnnxKey/ibk/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("APS_Engine", "https://kiup.ibk.co.kr/IBK/uib/sw/yettiesoft/APS/APS_Engine.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KDBBankBiz", "KDB산업은행 기업뱅킹", Category.Banking, "https://banking.kdb.co.kr/be/", "Korea Development Bank (Corp.)", [
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign_Installer", "https://banking.kdb.co.kr/be/3rd-party-web/hsecure/anysign/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("StandardCharteredBankBiz", "SC제일은행 기업뱅킹", Category.Banking, "https://bb.standardchartered.co.kr/", "Standard Chartered Bank Korea Limited. (Corp.)", [
         new("veraport-g3-x64", "https://bb.standardchartered.co.kr/product/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3", "https://bb.standardchartered.co.kr/product/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CitiBankKoreaBiz", "한국씨티은행 기업뱅킹", Category.Banking, "https://koreacitidirect.citigroup.com/", "Citibank Korea Inc. (Corp.)", [
         new("veraport-g3-x64-sha2", "https://koreacitidirect.citigroup.com/3rdParty/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3", "https://koreacitidirect.citigroup.com/3rdParty/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEn_nxKey_Installer_32bit", "https://koreacitidirect.citigroup.com/3rdParty/raon/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("astxdn", "https://koreacitidirect.citigroup.com/3rdParty/ahnlab/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Setup_ePageSaferRT", "https://koreacitidirect.citigroup.com/3rdParty/markany/exe/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KBankBiz", "케이뱅크 기업뱅킹", Category.Banking, "https://biz.kbanknow.com/", "Kbank (Corp.)", [
         new("INIS_EX", "https://biz.kbanknow.com/product/initech/crossweb/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("I3GSvcManager", "https://download.kbanknow.com/product/interezen/I3GSvcManager.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Setup_ePageSaferRT", "https://download.kbanknow.com/product/markany/exe/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KakaoBankBiz", "카카오뱅크 기업뱅킹", Category.Banking, "https://corp.kakaobank.com/", "KakaoBank Corp. (Corp.)", [
         new("AnySign_Installer", "https://download.softforum.com/Published/AnySign/v1.1.0.11/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("DGBDaeguBankBiz", "DGB대구은행 기업뱅킹", Category.Banking, "https://banking.dgb.co.kr/com_ebz_cib_sub_main.act", "DGB DAEGU BANK (Corp.)", [
         new("veraport-g3-x64-sha2", "https://cdn.dgb.co.kr/veraportg3/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("nonext.ebz_TouchEn_nxKey_32bit", "https://cdn.dgb.co.kr/TouchEnnxkey/nonext.ebz_TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3", "https://cdn.dgb.co.kr/DelfinoG3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ebz_MAWS_DaeguBankRex_Setup_1", "https://cdn.dgb.co.kr/ePageSaferRT/ebz_MAWS_DaeguBankRex_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ebz_MAWS_DaeguBankRex_Setup_2", "https://banking.dgb.co.kr/Markany/bin/ebz_MAWS_DaeguBankRex_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BusanBankBiz", "BNK부산은행 기업뱅킹", Category.Banking, "https://ebank.busanbank.co.kr/", "BNK BUSAN BANK (Corp.)", [
         new("veraport-g3-x64", "https://ebank.busanbank.co.kr/product/install/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INIS_EX_SHA2", "https://ebank.busanbank.co.kr/product/install/INISAFE/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("astxdn", "https://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEn_nxKey_Installer_32bit", "https://ebank.busanbank.co.kr/product/install/TouchEn_NX/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("I3GSvcManager", "https://ebank.busanbank.co.kr/product/install/IPinside/Windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("KSCertRelay_nx_Installer_32bit", "https://ebank.busanbank.co.kr/product/install/TouchEn_NX/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("BusanPFMS_Setup", "https://ebank.busanbank.co.kr/product/install/pfms/Windows/2.0.2.2/BusanPFMS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("GwangjuBankBiz", "광주은행 기업뱅킹", Category.Banking, "https://cib.kjbank.com/", "KWANGJU BANK (Corp.)", [
         new("veraport-g3-x64", "https://imgs.kjbank.com/resource/product/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INIS_EX_SHA2", "https://imgs.kjbank.com/resource/product/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("I3GSvcManager", "https://imgs.kjbank.com/resource/product/interezen/windows/I3GSvcManager.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay_nx_Installer_32bit", "https://imgs.kjbank.com/resource/product/nxwqr/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TDClientforWindowsAgentNX_4.9.0.5", "https://imgs.kjbank.com/resource/product/clipsoft/trustDoc/Install/TDClientforWindowsAgentNX_4.9.0.5.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("JbBankBiz", "전북은행 기업뱅킹", Category.Banking, "https://ibs.jbbank.co.kr/", "JEONBUK BANK (Corp.)", [
         new("veraport-g3-x64", "https://ibs.jbbank.co.kr/wizvera/veraportG3/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign_Installer", "https://www.jbbank.co.kr/AnySign4PC/install/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEn_nxKey_Installer_32bit", "https://ibs.jbbank.co.kr/TouchEnNxKey/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("I3GSvcManager", "https://ibs.jbbank.co.kr/pcInside/I3GSvcManager.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BnkKyungNamBankBiz", "BNK경남은행 기업뱅킹", Category.Banking, "https://ebank.knbank.co.kr/", "BNK KYONGNAM BANK (Corp.)", [
         new("veraport-g3-x64", "https://ebank.knbank.co.kr/product/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3", "https://ebank.knbank.co.kr/product/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("I3GSvcManager", "https://ebank.knbank.co.kr/product/ipinside/agent/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("MGSaemaeulBiz", "MG새마을금고 기업뱅킹", Category.Banking, "https://biz.kfcc.co.kr/", "Korean Federation of Community Credit Cooperatives (Corp.)", [
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("delfino-g3", "https://biz.kfcc.co.kr/3rd/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ShinhyubBiz", "신협 기업뱅킹", Category.Banking, "https://bizbank.cu.co.kr/", "National Credit Union Federation of Korea (Corp.)", [
         new("delfino-g3", "https://bizbank.cu.co.kr/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("astx_setup", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("veraport-g3-x64", "https://bizbank.cu.co.kr/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ePostBankBiz", "우체국예금 기업뱅킹", Category.Banking, "https://www.epostbank.go.kr/EBDMDM0000.do", "Korea Post Deposit Service Division (Corp.)", [
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BNKSavingsBank", "BNK저축은행", Category.Financing, "https://www.bnksb.com/", "BNK SAVINGS BANK", [
         new("SignKoreaCert", "https://www.bnksb.com/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("RealIp", "https://www.bnksb.com/BNKKTBRealipInst.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CKSavingsBank", "CK저축은행", Category.Financing, "https://www.cksavingsbank.co.kr/", "CK SAVINGS BANK", [
         new("Veraport", "https://www.cksavingsbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.cksavingsbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DBSavingsBank", "DB저축은행", Category.Financing, "https://www.idbsb.com/", "Dongbu Savings Bank", [
         new("Veraport", "https://www.idbsb.com/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.idbsb.com/installer/AnySign4PC_v1.1.0.11/AnySign4PC_v1.1.3.3_Windows_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("CertShare", "https://www.idbsb.com/installer/CertShare/CertShare_Installer.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("UniSign", "https://www.idbsb.com/installer/CrossCert/UniSignCRSV3Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("RealIp", "https://www.idbsb.com/installer/KTB/DBSBKTBRealipInst.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TDClientAgent", "https://www.idbsb.com/installer/SGA/TDClientforWindowsAgentNX_4.5.1.1.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DHSavingsBank", "DH저축은행", Category.Financing, "https://www.dhsavingsbank.co.kr/", "DH SAVINGS BANK", [
         new("Veraport", "https://www.dhsavingsbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.dhsavingsbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HBSavingsBank", "HB저축은행", Category.Financing, "https://hbsb.ibs.fsb.or.kr/", "ES SAVINGS BANK", [
         new("Veraport", "https://hbsb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://hbsb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("IBKSavingsBank", "IBK저축은행", Category.Financing, "https://ibksb.ibs.fsb.or.kr/", "IBK Savings Bank Co., Ltd.", [
         new("Veraport", "https://ibksb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://ibksb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("JTSavingsBank", "JT저축은행", Category.Financing, "https://jt.ibs.fsb.or.kr/", "JT SAVINGS BANK", [
         new("Veraport", "https://jt.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://jt.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("JTChinaeSavingsBank", "JT친애저축은행", Category.Financing, "https://jtchinae.ibs.fsb.or.kr/", "JT Chinae Savings Bank", [
         new("Veraport", "https://jtchinae.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://jtchinae.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KBSavingsBank", "KB저축은행", Category.Financing, "https://www.kbsavings.com/", "KB SAVINGS BANK", [
         new("Veraport", "https://www.kbsavings.com/pluginfree/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.kbsavings.com/pluginfree/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/kbsavings/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.kbsavings.com/pluginfree/interezen/IPinsideLWS.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("MSSavingsBank", "MS저축은행", Category.Financing, "https://www.mssb.co.kr/", "MS SAVINGS BANK", [
         new("Veraport", "https://www.mssb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.mssb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("NHSavingsBank", "NH저축은행", Category.Financing, "https://www.nhsavingsbank.co.kr/", "NH SAVINGS BANK", [
         new("Veraport", "https://www.nhsavingsbank.co.kr/vender/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("O2SavingsBank", "O2저축은행", Category.Financing, "https://www.o2banking.com/", "O2 SAVINGS BANK", [
         new("Veraport", "https://www.o2banking.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.o2banking.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("OKSavingsBank", "OK저축은행", Category.Financing, "https://ok.ibs.fsb.or.kr/", "OK SAVINGS BANK", [
         new("Veraport", "https://ok.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://ok.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("OSBSavingsBank", "OSB저축은행", Category.Financing, "https://ibs.osb.co.kr/", "OSB SAVINGS BANK", [
         new("INISAFE", "https://ibs.osb.co.kr/bank/module/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://ibs.osb.co.kr/bank/module/interezen/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("SNTSavingsBank", "SNT저축은행", Category.Financing, "https://hisntm.ibs.fsb.or.kr/", "SNT SAVINGS BANK", [
         new("Veraport", "https://hisntm.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://hisntm.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SBISavingsBank", "SBI저축은행", Category.Financing, "https://www.sbisb.co.kr/", "SBI SAVINGS BANK", [
         new("XecureWeb", "https://www.sbisb.co.kr/XecureObject/xw_install.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey", "https://www.sbisb.co.kr/TouchEnKey/TouchEnKey_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.sbisb.co.kr/TouchEnFw/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("RealIp", "https://www.sbisb.co.kr/SBISBankKTBRealipInst/SBISBankKTBRealipInst.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("GoryoSavingsBank", "고려저축은행", Category.Financing, "https://www.goryosb.co.kr/", "Goryo Savings Bank", [
         new("Veraport", "https://www.goryosb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.goryosb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KukjeSavingsBank", "국제저축은행", Category.Financing, "https://kukje.ibs.fsb.or.kr/", "Kukje Savings Bank", [
         new("Veraport", "https://kukje.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://kukje.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KuemhwaSavingsBank", "금화저축은행", Category.Financing, "https://www.kuemhwabank.co.kr/", "Keumhwa Savings Bank", [
         new("Veraport", "https://www.kuemhwabank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.kuemhwabank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("NamyangSavingsBank", "남양저축은행", Category.Financing, "https://www.nybank.co.kr/", "Namyang Savings Bank", [
         new("Veraport", "https://www.nybank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.nybank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DaemyungSavingsBank", "대명저축은행", Category.Financing, "https://www.daemyungbank.co.kr/", "Daemyung Savings Bank", [
         new("Veraport", "https://www.daemyungbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.daemyungbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DebecSavingsBank", "대백저축은행", Category.Financing, "https://www.debecbank.co.kr/", "DEBEC Savings Bank", [
         new("Veraport", "https://www.debecbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.debecbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DaishinSavingsBank", "대신저축은행", Category.Financing, "https://bank.daishin.com/", "Daishin Savings Bank", [
         new("WebCryptX", "https://bank.daishin.com/WebCryptX.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://bank.daishin.com/sk_ct2010New.exe", SilentSwitches.DefaultSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://bank.daishin.com/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("DaeaSavingsBank", "대아저축은행", Category.Financing, "https://daeabank.com/", "Daea Savings Bank", [
         new("Veraport", "https://daeabank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://daeabank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DSavingsBank", "대원저축은행", Category.Financing, "https://d-banks.co.kr/", "Daewon Savings Bank", [
         new("Veraport", "https://d-banks.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://d-banks.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DaehanSavingsBank", "대한저축은행", Category.Financing, "https://www.daehanbank.co.kr/", "Daehan Savings Bank", [
         new("Veraport", "https://www.daehanbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.daehanbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DoubleSavingsBank", "더블저축은행", Category.Financing, "https://www.doublebank.co.kr/", "Double Savings Bank", [
         new("Veraport", "https://www.doublebank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.doublebank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("TheKSavingsBank", "더케이저축은행", Category.Financing, "https://thek.ibs.fsb.or.kr/", "The K Savings Bank", [
         new("Veraport", "https://thek.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://thek.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DongyangSavingsBank", "동양저축은행", Category.Financing, "https://www.dysbank.com/", "Dongyang Savings Bank", [
         new("Veraport", "https://www.dysbank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.dysbank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.33_264.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DongwonJeilSavingsBank", "동원제일저축은행", Category.Financing, "https://dongwonjeil.ibs.fsb.or.kr/", "Dongwon Jeil Savings Bank", [
         new("Veraport", "https://dongwonjeil.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://dongwonjeil.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DreamSavingsBank", "드림저축은행", Category.Financing, "https://www.dreamsb.com/", "Dream Savings Bank", [
         new("Veraport", "https://www.dreamsb.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.dreamsb.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("RaonSavingsBank", "라온저축은행", Category.Financing, "https://www.raonsb.co.kr/", "Raon Savings Bank", [
         new("Veraport", "https://www.raonsb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.raonsb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SamilSavingsBank", "머스트삼일저축은행", Category.Financing, "https://www.samilbank.co.kr/", "MUST SAMIL SAVINGS BANK", [
         new("Veraport", "https://www.samilbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.samilbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("MoaSavingsBank", "모아저축은행", Category.Financing, "https://www.moasb.co.kr/", "Moa Savings Bank", [
         new("Veraport", "https://www.moasb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.moasb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("MKSavingsBank", "민국저축은행", Category.Financing, "https://www.mkb.co.kr/", "MK SAVINGS BANK", [
         new("Veraport", "https://www.mkb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.mkb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("BaroSavingsBank", "바로저축은행", Category.Financing, "https://www.barosavings.com/", "Baro Savings Bank", [
         new("Veraport", "https://www.barosavings.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.barosavings.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("BulimSavingsBank", "부림저축은행", Category.Financing, "https://www.bulimbank.co.kr/", "Bulim Savings Bank", [
         new("Veraport", "https://www.bulimbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.bulimbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SamjungSavingsBank", "삼정저축은행", Category.Financing, "https://www.samjungsavingsbank.co.kr/", "Samjung Savings Bank", [
         new("Veraport", "https://www.samjungsavingsbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.samjungsavingsbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SamhoSavingsBank", "삼호저축은행", Category.Financing, "https://www.samhosb.co.kr/", "Samho Savings Bank", [
         new("Veraport", "https://www.samhosb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.samhosb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SangSangInSavingsBank", "상상인저축은행", Category.Financing, "https://sangsanginsb.ibs.fsb.or.kr/", "Sang Sang In Savings Bank", [
         new("Veraport", "https://sangsanginsb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://sangsanginsb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SangSangInPlusSavingsBank", "상상인플러스저축은행", Category.Financing, "https://sangsanginplussb.ibs.fsb.or.kr/", "Sang Sang In Plus Savings Bank", [
         new("Veraport", "https://sangsanginplussb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://sangsanginplussb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SeramSavingsBank", "세람저축은행", Category.Financing, "https://seram.ibs.fsb.or.kr/", "Seram Savings Bank", [
         new("Veraport", "https://seram.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://seram.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("CentralSavingsBank", "센트럴저축은행", Category.Financing, "https://www.centralbank.co.kr/", "Cetral Savings Bank", [
         new("Veraport", "https://www.centralbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.centralbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SoulBrainSavingsBank", "솔브레인저축은행", Category.Financing, "https://sbbank.ibs.fsb.or.kr/", "Soul Brain Savings Bank", [
         new("Veraport", "https://sbbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://sbbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SmartSavingsBank", "스마트저축은행", Category.Financing, "https://www.smartbank.co.kr/", "Smart Savings Bank", [
         new("Veraport", "https://bank.smartbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://bank.smartbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SkySavingsBank", "스카이저축은행", Category.Financing, "https://www.skysb.co.kr/", "Sky Savings Bank", [
         new("Veraport", "https://www.skysb.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.skysb.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("StarSavingsBank", "스타저축은행", Category.Financing, "https://www.estarbank.co.kr/", "Star Savings Bank", [
         new("Veraport", "https://www.estarbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.estarbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("ShinhanSavingsBank", "신한저축은행", Category.Financing, "https://www.shinhansavings.com/", "SHINHAN Savings Bank", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.2.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://www.shinhansavings.com/js/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("IPInside", "https://www.shinhansavings.com/common/cab/I3GSvcManager.3.0.0.11.exe", SilentSwitches.CustomNoDlgSwitch),
         new("TouchEnFirewall", "https://www.shinhansavings.com/js/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("AsanSavingsBank", "아산저축은행", Category.Financing, "https://www.asanbank.co.kr/", "ASAN Savings Bank", [
         new("Veraport", "https://www.asanbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.asanbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("AngukSavingsBank", "안국저축은행", Category.Financing, "https://www.angukbank.co.kr/", "Anguk Savings Bank", [
         new("Veraport", "https://www.angukbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.angukbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("AnyangSavingsBank", "안양저축은행", Category.Financing, "https://www.anyangbank.co.kr/", "Anyang Savings Bank", [
         new("Veraport", "https://www.anyangbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.anyangbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("AccuonSavingsBank", "애큐온저축은행", Category.Financing, "https://www.acuonsb.co.kr/", "Accuon Savings Bank", [
         new("Veraport", "https://www.acuonsb.co.kr/wizvera/web/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.acuonsb.co.kr/wizvera/web/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.acuonsb.co.kr/raon/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.acuonsb.co.kr/ipinside/agent/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("TDClientAgent", "https://www.acuonsb.co.kr/sga/down/TDClientforWindowsAgentNX_4.9.0.6.exe", SilentSwitches.DefaultSilentSwitch),
         new("SignKoreaCert", "https://www.acuonsb.co.kr/signkorea/web/SKCertServiceSetup_v2.0.11_r2325.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("YoungjinSavingsBank", "영진저축은행", Category.Financing, "https://yjbank.ibs.fsb.or.kr/", "Youngjin Savings Bank", [
         new("Veraport", "https://yjbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://yjbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("YegaramSavingsBank", "예가람저축은행", Category.Financing, "https://yegaram.ibs.fsb.or.kr/", "Yeegaram Savings Bank", [
         new("Veraport", "https://yegaram.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://yegaram.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("OsungSavingsBank", "오성저축은행", Category.Financing, "https://www.osungbank.co.kr/", "Osung Savings Bank", [
         new("Veraport", "https://www.osungbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.osungbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("WooreeSavingsBank", "우리저축은행", Category.Financing, "https://www.wooleebank.co.kr/", "WOOLEE SAVINGS BANK", [
         new("Veraport", "https://www.wooleebank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.wooleebank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("WooriSavingsBank", "우리금융저축은행", Category.Financing, "https://woori.ibs.fsb.or.kr/", "WOORI SAVINGS BANK", [
         new("Veraport", "https://woori.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://woori.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("WelcomeSavingsBank", "웰컴저축은행", Category.Financing, "https://www.welcomebank.co.kr/", "Welcome Savings Bank", [
         new("SmartManagerEx", "https://www.welcomebank.co.kr/3rdparty/initech/smartmanagerex/extension/down/SmartManagerEX.exe", SilentSwitches.DefaultSilentSwitch),
         new("KeySharpBiz", "https://www.welcomebank.co.kr/3rdparty/raon/nxbiz/download/keysharpnxbiz.exe", SilentSwitches.DefaultSilentSwitch),
         new("SignKoreaCert", "https://www.welcomebank.co.kr/3rdparty/signkorea/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("KSCertRelay32", "https://www.welcomebank.co.kr/3rdparty/raon/TouchEn/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnKey32", "https://www.welcomebank.co.kr/3rdparty/raon/TouchEn/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.welcomebank.co.kr/3rdparty/raon/TouchEn/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("IPInside", "https://www.welcomebank.co.kr/3rdparty/ipinside/module/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("UnionSavingsBank", "유니온저축은행", Category.Financing, "https://unionsb.ibs.fsb.or.kr/", "Union Savings Bank", [
         new("Veraport", "https://unionsb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://unionsb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("YuantaSavingsBank", "유안타저축은행", Category.Financing, "https://www.yuantasavings.co.kr/", "Yuanta Savings Bank", [
         new("Veraport", "https://www.yuantasavings.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.yuantasavings.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("YCSavingsBank", "융창저축은행", Category.Financing, "https://www.ycbank.co.kr/", "Yungchang Savings Bank", [
         new("Veraport", "https://www.ycbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.ycbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("InsungSavingsBank", "인성저축은행", Category.Financing, "https://www.insungsavingsbank.co.kr/", "Insung Savings Bank", [
         new("Veraport", "https://www.insungsavingsbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.insungsavingsbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("IncheonSavingsBank", "인천저축은행", Category.Financing, "https://www.incheonbank.com/", "Inhcheon Savings Bank", [
         new("Veraport", "https://www.incheonbank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.incheonbank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("ChoeunSavingsBank", "조은저축은행", Category.Financing, "https://choeunbank.ibs.fsb.or.kr/", "Choeun Savings Bank", [
         new("Veraport", "https://choeunbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://choeunbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("ChSavingsBank", "조흥저축은행", Category.Financing, "https://chbank.ibs.fsb.or.kr/", "Choheung Savings Bank", [
         new("Veraport", "https://chbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://chbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("JinjuSavingsBank", "진주저축은행", Category.Financing, "https://www.jinjubank.net/", "Jinju Savings Bank", [
         new("Veraport", "https://www.jinjubank.net/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.jinjubank.net/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("CharmSavingsBank", "참저축은행", Category.Financing, "https://charm.ibs.fsb.or.kr/", "Charm Savings Bank", [
         new("Veraport", "https://charm.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://charm.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("CheongjuSavingsBank", "청주저축은행", Category.Financing, "https://www.cheongjubank.com/", "Cheongju Savings Bank", [
         new("Veraport", "https://www.cheongjubank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.cheongjubank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KiwoomYesSavingsBank", "키움예스저축은행", Category.Financing, "https://www.kiwoomyesbank.com/", "KIWOOM Yes Savings Bank", [
         new("Veraport", "https://www.kiwoomyesbank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.kiwoomyesbank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KiwoomSavingsBank", "키움저축은행", Category.Financing, "https://kiwoom.ibs.fsb.or.kr/", "KIWOOM Savings Bank", [
         new("Veraport", "https://kiwoom.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://kiwoom.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("PepperSavingsBank", "페퍼저축은행", Category.Financing, "https://pepperbank.ibs.fsb.or.kr/", "Pepper Savings Bank", [
         new("Veraport", "https://pepperbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://pepperbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("PtSavingsBank", "평택저축은행", Category.Financing, "https://www.ptbank.co.kr/", "Pyeongtaek Savings Bank", [
         new("Veraport", "https://www.ptbank.co.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.ptbank.co.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("PureunSavingsBank", "푸른저축은행", Category.Financing, "https://www.prsb.co.kr/", "Pureun Savings Bank", [
         new("Veraport", "https://www.prsb.co.kr/installer/veraport/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.prsb.co.kr/installer/delfino/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.prsb.co.kr/installer/FDS/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("HanaSavingsBank", "하나저축은행", Category.Financing, "https://www.kebhana.com/efamily/h/hanasavingsbank/main.jsp", "Hana Savings Bank", [
         new("Veraport", "https://www.kebhana.com/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.kebhana.com/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("Delfino", "https://www.kebhana.com/wizvera/delfino/down/g3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SCWSSP", "https://www.kebhana.com/softcamp/WebSecurityStandard/SCWSSPSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("IPInside", "https://www.kebhana.com/interezen/agent/np_v6/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("TrueFriendSavingsBank", "한국투자저축은행", Category.Financing, "https://kisb.ibs.fsb.or.kr/", "Korea Investment Savings Bank", [
         new("Veraport", "https://kisb.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://kisb.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HsSavingsBank", "한성저축은행", Category.Financing, "https://hs.ibs.fsb.or.kr/", "Hanseong Savings Bank", [
         new("Veraport", "https://hs.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://hs.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HanwhaSavingsBank", "한화저축은행", Category.Financing, "https://www.hanwhasbank.com/", "Hanwha Savings Bank", [
         new("Veraport", "https://www.hanwhasbank.com/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.hanwhasbank.com/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("EugeneSavingsBank", "유진저축은행", Category.Financing, "https://eugenebank.ibs.fsb.or.kr/", "Eugene Savings Bank", [
         new("Veraport", "https://eugenebank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://eugenebank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HkSavingsBank", "흥국저축은행", Category.Financing, "https://ehkbank.ibs.fsb.or.kr/", "Heungkuk Savings Bank", [
         new("Veraport", "https://ehkbank.ibs.fsb.or.kr/3rdparty/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://ehkbank.ibs.fsb.or.kr/3rdparty/signkorea/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("IBKSecurity", "IBK투자증권", Category.Security, "https://www.ibks.com/", "IBK Securities", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://download.ibks.com/install/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("MyAssetSecurity", "유안타증권", Category.Security, "https://www.myasset.com/", "Yuanta Securities Korea Co. Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/myasset/all/astx_setup_myasset.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.myasset.com/common/security/KOSInstaller/SKCertService/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("DAmoWebCrypto", "https://www.myasset.com/common/security/KOSInstaller/DAmoWebCrypto/DAmoWebCryptoSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KOS", "https://www.myasset.com/common/security/KOSInstaller/KOS/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KBSecurity", "KB증권", Category.Security, "https://www.kbsec.com/", "KB SECURITIES", [
         new("Veraport", "https://www.kbsec.com/ndataweb/exe/prod/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.kbsec.com/ndataweb/exe/prod/skcert_setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("KOS", "https://www.kbsec.com/ndataweb/exe/prod/kos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MiraeAssetSecurity", "미래에셋증권", Category.Security, "https://securities.miraeasset.com/", "Mirae Asset Securities Co., Ltd.", [
         new("Veraport", "https://download.securities.miraeasset.com/w/veraport/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASTX", "https://download.securities.miraeasset.com/w//astx/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://download.securities.miraeasset.com/w//sign/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SamsungSecurity", "삼성증권", Category.Security, "https://www.samsungpop.com/", "Samsung Securities", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/samsungpop/all/astx_setup_samsungpop.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.samsungpop.com/modules/koscom/file/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
         new("ePageSafer", "https://www.samsungpop.com/modules/report/file/MAWS_SamsungPopOz_Total_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("TrueFriendSecurity", "한국투자증권", Category.Security, "https://www.truefriend.com/", "Korea Investment & Securities Co.,Ltd.", [
         new("KOS", "https://new.real.download.dws.co.kr/download/hpage/KOS_Setup_3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://file.truefriend.com/Storage/Download/I3GSvcManager.3.0.0.24.exe", SilentSwitches.CustomNoDlgSwitch),
         new("Veraport", "https://new.real.download.dws.co.kr/download/hpage/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NHSecurity", "NH투자증권", Category.Security, "https://www.nhqv.com/", "NH INVESTMENT & SECURITIES", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.nhqv.com/security/download/SKCertServiceSetup_v2.5.32_207.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WebPri20", "http://download.wooriwm.com/download/www/print/webpri20.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MarkAny", "https://www.nhqv.com/rptshop/ReportShop520_Web_withMarkAny.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.nhqv.com/rptshop/Setup_ePageSaferH2O.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KyoboSecurity", "교보증권", Category.Security, "https://iprovest.com/", "Kyobo Securities Co., Ltd", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.iprovest.com/upload/customhelp/download/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HiSecurity", "하이투자증권", Category.Security, "https://www.hi-ib.com/", "HI INVESTMENT & SECURITIES", [
         new("SetupNx", "https://www.hi-ib.com/program/HI_setupNx.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.hi-ib.com/SKCertService/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("TouchEnKey64", "https://www.hi-ib.com/raon/e2e/TouchEn/nxKey/module/TouchEn_nxKey_Installer_64bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.hi-ib.com/raon/e2e/TouchEnfw/nxFw/module/TEFW_Installer64.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnWeb", "https://www.hi-ib.com/raon/e2e/TouchEnfw/nxWeb/module/TouchEn_nxWeb_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("HyundaiCarSecurity", "현대차증권", Category.Security, "https://www.hmsec.com/", "HYUNDAI MOTOR SECURITIES", [
         new("SignKoreaCert", "https://www.hmsec.com/cabs/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.hmsec.com/cabs/I3GSvcManager.3.0.0.12.exe", SilentSwitches.CustomNoDlgSwitch),
         new("MarkAny", "https://www.hmsec.com/cabs/ReportShop520_Web_withMarkAny.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KiwoomSecurity", "키움증권", Category.Security, "https://www.kiwoom.com/", "KIWOOM Securities Co., Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default_nonstop/all/astx_setup_nonstop.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.kiwoom.com/h/kws/assets/down/SKCertServiceSetup_v2.5.20_156.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("EBestSecurity", "LS증권", Category.Security, "https://www.ls-sec.co.kr", "LS Securities Co., Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KOS", "https://www.ls-sec.co.kr/download/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.ls-sec.co.kr/download/SKCertServiceSetup.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("SKSecurity", "SK증권", Category.Security, "http://www.sks.co.kr/", "SK Securities Co.,Ltd.", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.sks.co.kr/main/common/security/signkorea/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("DaishinSecurity", "대신증권", Category.Security, "https://www.daishin.com/", "DAISHIN Securities Co., Ltd.", [
         new("SignKoreaCert", "https://www.daishin.com/install/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TDClientAgent", "https://www.daishin.com/install/TDClientforWindowsAgentDS_4.3.0.1_20160727.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("HanwhaSecurity", "한화투자증권", Category.Security, "https://www.hanwhawm.com/", "Hanwha Investment & Securities", [
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/hanwhastock/all/astx_setup_hanwhastock.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASTX", "http://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.hanwhawm.com/common/security/SignKorea_NA/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("KSCertRelay", "https://www.hanwhawm.com/download/down/KSCertRelay.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISafeMail", "https://www.hanwhawm.com/download/down/INISAFEMailv4.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("HanaSecurity", "하나증권", Category.Security, "https://www.hanaw.com/", "Hana Securities Co.,Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.hanaw.com/common/security/SignKorea_NA/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("ShinhanSecurity", "신한투자증권", Category.Security, "https://www.shinhansec.com/", "SHINHAN SECURITIES", [
         new("Veraport", "https://www.shinhansec.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.shinhansec.com/siw/solution/download/SKCertServiceSetup_v2.5.17_149.exe", SilentSwitches.DefaultSilentSwitch),
         new("ASTX", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.shinhansec.com/markany/bin/Setup_ePageSafer.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("DBSecurity", "DB금융투자", Category.Security, "https://www.db-fi.com/", "DB Financial Investment Co., Ltd.", [
         new("Veraport", "https://www.db-fi.com/common/plugin/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.db-fi.com/common/plugin/skcert/exe/SKCertServiceSetup_v2.5.9_113.exe", SilentSwitches.DefaultSilentSwitch),
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("EugeneSecurity", "유진투자증권", Category.Security, "https://www.eugenefn.com/", "EUGENE INVESTMENT & SECURITIES CO.,LTD.", [
         new("SignKoreaCert", "https://www.eugenefn.com/resources/download/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MeritzSecurity", "메리츠증권", Category.Security, "https://home.imeritz.com/", "MERITZ SECURITIES", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://home.imeritz.com/include/down/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("BookookSecurity", "부국증권", Category.Security, "http://www.bookook.co.kr/", "BOOKOOK Securities Co., Ltd.", [
         new("SignKoreaCert", "https://wts.koscom.co.kr:8089/setup/file/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
         new("KOS", "https://wts.koscom.co.kr:8089/setup/file/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("RealIp", "https://wts.koscom.co.kr:8089/setup/file/KOSCOMKTBRealip_enc.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ShinyoungSecurity", "신영증권", Category.Security, "https://www.shinyoung.com/", "SHINYOUNG Securities Co., Ltd.", [
         new("KOS", "http://w2.kings.co.kr/k/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CapeSecurity", "케이프투자증권", Category.Security, "https://www.capefn.com/", "CAPE Investment & Securities Co., Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("SignKoreaCert", "https://www.capefn.com/security/down/SKCertServiceSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("WooriInvest", "우리투자증권", Category.Security, "https://securities.wooriib.com/", "WOORI INVESTMENT SECURITIES", [
         new("SignKoreaCert", "https://fundsupermarket.wooriib.com/plugin/signKorea2/SKCertServiceSetup_v2.5.40_281.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("FDSDetector", "https://fundsupermarket.wooriib.com/plugin/KTBRealip/KOSCOMKTBRealip_enc.exe", SilentSwitches.NsisSilentSwitch),
         new("IPInside", "https://fundsupermarket.wooriib.com/plugin/I3GSvcManager/I3GSvcManager.3.0.0.25.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("Wooricard", "우리카드", Category.CreditCard, "https://www.wooricard.com/", "WOORI CARD", [
         new("Veraport", "https://pc.wooricard.com/dcpc/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://pc.wooricard.com/dcpc/pluginfree/setup/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://pc.wooricard.com/dcpc/eFDS/binary/windows/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("KeySharpBiz", "https://pc.wooricard.com/dcpc/raonx/ksbiz/module/KSbiz_Installer_32bit.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KBKookminCard", "KB국민카드", Category.CreditCard, "https://www.kbcard.com/", "KB KOOKMIN CARD", [
         new("Veraport", "https://download.kbcard.com/security/wizvera/veraport-g3/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://download.kbcard.com/security/wizvera/delfino-g3/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Hanacard", "하나카드", Category.CreditCard, "https://www.hanacard.co.kr/", "Hana Card", [
         new("Veraport", "https://www.hanacard.co.kr/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.hanacard.co.kr/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.hanacard.co.kr/sw/EFDS/agent/I3GSvcManager.3.0.0.25.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("ShinhanCard", "신한카드", Category.CreditCard, "https://www.shinhancard.com/", "SHINHAN CARD", [
         new("Veraport", "https://www.shinhancard.com/psolution/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.shinhancard.com/psolution/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BCCard", "BC카드", Category.CreditCard, "https://www.bccard.com/", "BC Card co., Ltd.", [
         new("INISAFECrossWeb", "https://www.bccard.com/initech/crossweb/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NHCard", "NH농협카드", Category.CreditCard, "https://card.nonghyup.com/", "NH NONGHYUP CARD", [
         new("Veraport", "https://card.nonghyup.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://card.nonghyup.com/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HyundaiCard", "현대카드", Category.CreditCard, "https://www.hyundaicard.com/", "Hyundai Card", [
         new("Veraport", "https://img.hyundaicard.com/wizvera/veraport/down/veraport-g3s-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://img.hyundaicard.com/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("SamsungCard", "삼성카드", Category.CreditCard, "https://www.samsungcard.com/", "Samsung Card", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MagicLine4NX", "https://www.samsungcard.com/MagicLine4Web/ML4Web/install_bin/magicline4nx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://static12.samsungcard.com/stinstall/ipinsideLWS/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("LotteCard", "롯데카드", Category.CreditCard, "https://www.lottecard.co.kr/", "LOTTE CARD", [
         new("NOSAll", "https://image.lottecard.co.kr/webapp/pluginfree/nos_down.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.2.0/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("eHyundaiCard", "현대백화점카드", Category.CreditCard, "https://www.ehyundai.com/newPortal/card/main.do", "Hyundai Department Store Credit Card", [
         new("AnySign", "https://www.ehyundai.com/AnySign4PC/win/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KyoboLifePlanet", "교보라이프플래닛생명", Category.Insurance, "https://www.lifeplanet.co.kr/", "KYOBO LIFEPLANET LIFE INSURANCE COMPANY", [
         new("Veraport", "https://www.lifeplanet.co.kr/commons/3rd-party/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("PrintManager", "https://www.lifeplanet.co.kr/commons/3rd-party/redbc/NX_PRNMAN(x86).exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KyoboLife", "교보생명", Category.Insurance, "https://www.kyobo.co.kr/", "Kyobo Life Insurance Co., Ltd.", [
         new("Veraport", "https://www.kyobo.co.kr/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://www.kyobo.co.kr/AST/Client/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.kyobo.co.kr/AnySign.v2/Client/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://www.kyobo.co.kr/TouchEn.v2/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("DongyangLife", "동양생명", Category.Insurance, "https://www.myangel.co.kr/", "Tong Yang Life Insurance Company", [
         new("Veraport", "https://www.myangel.co.kr/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.myangel.co.kr/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.myangel.co.kr/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.myangel.co.kr/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("CignaLife", "라이나생명", Category.Insurance, "https://www.lina.co.kr/", "LINA Korea, a Chubb Company", [
         new("Veraport", "https://www.lina.co.kr/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.lina.co.kr/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.lina.co.kr/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.lina.co.kr/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("Metlife", "메트라이프생명", Category.Insurance, "https://cyber.metlife.co.kr/", "MetLife Insurance Co. of Korea, Ltd.", [
         new("INISAFECrossWeb", "https://cyber.metlife.co.kr/initech/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("KOS", "https://cyber.metlife.co.kr/kings/files/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MiraeAssetLife", "미래에셋생명", Category.Insurance, "https://life.miraeasset.com/", "Mirae Asset Life Insuarance Co., Ltd.", [
         new("Veraport", "https://life.miraeasset.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("VestCert", "https://life.miraeasset.com/thirdparty/yettie/vestsign/VestCertSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://life.miraeasset.com/thirdparty/raon/nxkey/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("SamsungLife", "삼성생명", Category.Insurance, "https://www.samsunglife.com/", "Samsung Life Insurance", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.samsunglife.com/lib/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ShinhanLife", "신한라이프", Category.Insurance, "https://www.shinhanlife.co.kr/", "SHINHAN LIFE INSURANCE", [
         new("Veraport", "https://cyber.shinhanlife.co.kr/third/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://cyber.shinhanlife.co.kr/third/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay64", "https://cyber.shinhanlife.co.kr/third/nxCR/module/KSCertRelay_nx_Installer_64bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KeySharpBiz", "https://c.shinhanlife.co.kr/library/raonnx/ksbiz/module/KSbiz_Installer_32bit.exe", SilentSwitches.DefaultSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://c.shinhanlife.co.kr/library/markany/module/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay32", "https://c.shinhanlife.co.kr/library/raonnx/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ChubbLife", "처브라이프", Category.Insurance, "https://customercenter.chubblife.co.kr/", "Chubb Life Insurance Company Ltd.", [
         new("MoaSign", "https://customercenter.chubblife.co.kr/initech/moasign/down/MoaSignSetup_20161212_1.0.45_Card_T_.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://customercenter.chubblife.co.kr/raon/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://customercenter.chubblife.co.kr/raon/TouchEn/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("PrudentialLife", "푸르덴셜생명", Category.Insurance, "https://cyber.prudential.co.kr/", "Prudential Life Insurance Company of Korea Ltd.", [
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://cyber.prudential.co.kr/sw/MAWS_PrudentialOz_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("FubonHyundaiLife", "푸본현대생명", Category.Insurance, "https://www.fubonhyundai.com/", "Fubon Hyundai Life Insurance Corp.", [
         new("VestCert", "https://www.fubonhyundai.com/yettie/VestCert/VestCertSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HanaLife", "하나생명", Category.Insurance, "https://www.hanalife.co.kr/", "HANA Life", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://rencyber.hanalife.co.kr/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HanwhaLife", "한화생명", Category.Insurance, "https://www.hanwhalife.com/", "Hanwha Life Insurance Co., Ltd.", [
         new("Veraport", "https://www.hanwhalife.com/security/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "http://download.softforum.com/Published/AnySign/v1.1.2.5/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HeungkukLife", "흥국생명", Category.Insurance, "https://www.heungkuklife.co.kr/", "Heungkuk Life Insurance Co., Ltd.", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("AblLife", "ABL생명", Category.Insurance, "https://cyber.abllife.co.kr/", "ABL LIFE INSURANCE", [
         new("Veraport", "https://cyber.abllife.co.kr/resources/lib/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASTX", "https://cyber.abllife.co.kr/resources/lib/anlab/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://cyber.abllife.co.kr/resources/lib/anySign/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://cyber.abllife.co.kr/resources/lib/NXsecureKeyboard/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("KSCertRelay32", "https://cyber.abllife.co.kr/resources/lib/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("CX60", "https://cyber.abllife.co.kr/resources/lib/m2soft/crownix/Plugin/CX60_Plugin_u_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://cyber.abllife.co.kr/resources/lib/markany/ePageSafer/madownloadrd.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("AiaLife", "AIA생명", Category.Insurance, "https://www.aia.co.kr", "AIA Life Insurance", [
         new("Veraport", "https://cyberservice.aia.co.kr/MYAIA/sol/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://cyberservice.aia.co.kr/MYAIA/sol/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CardifLife", "BNP파리바 카디프생명", Category.Insurance, "https://www.cardif.co.kr/", "BNP PARIBAS CARDIF LIFE INSURANCE", [
         new("TouchEnKey32", "https://www.cardif.co.kr/b2c-portlet/js/touchen/nxkey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("DbLife", "DB생명", Category.Insurance, "https://www.idblife.com/cyber/", "DB Life Insuarance Co., Ltd.", [
         new("Veraport", "https://www.idblife.com/cyber/assets/vender/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.idblife.com/cyber/assets/vender/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPInside", "https://www.idblife.com/cyber/assets/vender/ip/agent/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("InnoGmp", "https://www.idblife.com/cyber/assets/vender/innoex/common/package/innogmp_win.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay32", "https://www.idblife.com/cyber/assets/vender/raon/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("DgbLife", "DGB생명", Category.Insurance, "https://www.dgbfnlife.com/cyber/", "DGB LIFE INSURANCE", [
         new("SmartManagerEx", "https://www.dgbfnlife.com/icc/smartmanagerex/extension/down/SmartManagerEX.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KbLife", "KB라이프", Category.Insurance, "https://www.kblife.co.kr/", "KB Life Insurance Company", [
         new("Veraport", "https://www.kblife.co.kr/res/3rd/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.kblife.co.kr/res/3rd/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay32", "https://www.kblife.co.kr/res/3rd/raon/raonnx/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://rdr.kbli.co.kr/NOAX_RT/bin/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KdbLife", "KDB생명", Category.Insurance, "https://www.kdblife.co.kr/", "KDB LIFE INSURANCE", [
         new("NOS", "https://www.kdblife.co.kr/securepackage/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KCaseAgent", "https://www.kdblife.co.kr/securepackage/KCaseAgent_Installer.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NHLife", "NH농협생명", Category.Insurance, "https://www.nhlife.co.kr/", "NONGHYUP LIFE INSURANCE", [
         new("Veraport", "https://www.nhlife.co.kr/sol/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.nhlife.co.kr/sol/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.nhlife.co.kr/sol/raonsecure/TouchEn/nxKey/module/nxkey_x86.exe", SilentSwitches.CustomSilenceSwitch),
         new("ePageSafer", "https://www.nhlife.co.kr/sol/Markany/ePageSafer/bin/Setup_ePageSafer.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KISSCAP", "https://www.nhlife.co.kr/sol/KWIC/scraping/down/KISSCAP.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("IbkAnnuityInsurance", "IBK연금보험", Category.Insurance, "https://www.ibki.co.kr/", "IBK Insurance", [
         new("Veraport", "https://www.ibki.co.kr/client_install_file/wizvera/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.ibki.co.kr/client_install_file/wizvera/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("iSASService", "https://www.ibki.co.kr/client_install_file/scraping/iSASService_v2.3.2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.ibki.co.kr/client_install_file/markany/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MeritzFire", "메리츠화재", Category.Insurance, "https://www.meritzfire.com/", "Meritz Fire & Marine Insurance Co., Ltd.", [
         new("Veraport", "https://cmdown.meritzfire.com/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KeySharpBiz", "https://cmdown.meritzfire.com/nxbiz/download/keysharpnxbiz.exe", SilentSwitches.DefaultSilentSwitch),
         new("TouchEnKey32", "https://cmdown.meritzfire.com/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://cmdown.meritzfire.com/TouchEn/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("SamsungFire", "삼성화재", Category.Insurance, "https://www.samsungfire.com/", "Samsung Fire & Marine Insuarance", [
         new("NOS", "https://www.samsungfire.com/pluginfree/nos_launcher.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HeungkukFire", "흥국화재", Category.Insurance, "https://www.heungkukfire.co.kr/", "Heungkuk Fire & Marine Insurance Co., Ltd.", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.0.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("LotteInsurance", "롯데손해보험", Category.Insurance, "http://www.lotteins.co.kr/", "Lotte Insurance Co., Ltd", [
         new("Veraport", "http://www.lottehowmuch.com/wizvera/veraportg3/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "http://download.softforum.com/Published/AnySign/v1.1.0.8/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("KOS", "http://kings.nefficient.co.kr/kings/lotteins/KOS_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CarrotInsurance", "캐롯손해보험", Category.Insurance, "https://www.carrotins.com/", "Carrot General Insurance Co., Ltd.", [
    ], []),
    new("Educar", "하나손해보험", Category.Insurance, "https://www.educar.co.kr/", "Hana Insurance Co., Ltd.", [
         new("Veraport", "https://www.educar.co.kr/solution/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.educar.co.kr/solution/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEnKey32", "https://www.educar.co.kr/solution/raon/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnFirewall", "https://www.educar.co.kr/solution/raon/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("KSignCASE", "https://www.educar.co.kr/solution/wizsign/down/KSignCASE_v1.3.14_190122.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HanwhaInsurance", "한화손해보험", Category.Insurance, "https://www.hwgeneralins.com/", "Hanwha General Insurance Co., Ltd.", [
         new("WizInDelfino", "https://www.hwgeneralins.com/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HyundaiInsurance", "현대해상", Category.Insurance, "https://www.hi.co.kr/", "Hyundai Marine & Fire Insurance Co., Ltd", [
         new("Veraport", "https://download.hi.co.kr/distribution/HC/wizvera/veraportG3/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.hi.co.kr/product/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://www.hi.co.kr/product/pluginfree/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.hi.co.kr/product/ReportingDown/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NXiSAS", "https://www.hi.co.kr/product/Coocon/file/NXiSAS.exe", SilentSwitches.DefaultSilentSwitch),
         new("KSCertRelayNX", "https://www.hi.co.kr/product/raonnx/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("AceInsurance", "ACE손해보험", Category.Insurance, "https://ec.aceinsurance.co.kr/jsp/acelimited/mainCert.jsp", "ACE Insurance Co., Ltd.", [
         new("VestCert", "https://ec.aceinsurance.co.kr/third/vestsign/VestCertSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("AigInsurance", "AIG손해보험", Category.Insurance, "https://www.aig.co.kr/", "AIG KOREA CORPORATION", [
         new("AnySign", "https://www.aig.co.kr/common/anysign/AnySign4PC/download/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://www.aig.co.kr/common/nos/download/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("UbiViewer", "https://www.aig.co.kr/common/ubiReport/UbiViewerMarkAny.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KSCertRelay", "https://www.aig.co.kr/common/keyICRS/install_file/KSCertRelay.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("AxaInsurance", "AXA손해보험", Category.Insurance, "https://www.axa.co.kr/", "AXA General Insurance Co., Ltd.", [
         new("Veraport", "https://www.axa.co.kr/wizvera/veraport/down/veraport-g3-x64-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://www.axa.co.kr/wizvera/delfino/down/delfino-g3-sha2.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.axa.co.kr/markany/bin/Setup_ePageSafer.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CardifInsurance", "신한EZ손해보험", Category.Insurance, "https://www.shinhanez.co.kr/", "SHINHAN EZ GENERAL INSURANCE", [
         new("AnySign", "https://www.shinhanez.co.kr/html/anySign4PC/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnFirewall", "https://www.shinhanez.co.kr/raonnx/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnKey32", "https://www.shinhanez.co.kr/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("DbInsurance", "DB손해보험", Category.Insurance, "https://www.idbins.com/", "DB INSURANCE CO., LTD.", [
         new("NOS", "https://idbins.com/nActX/nosdown/prod/nos_down.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KbInsurance", "KB손해보험", Category.Insurance, "https://kbinsure.co.kr/", "KB INSURANCE", [
         new("WizInDelfino", "https://api-storage.cloud.toast.com/v1/AUTH_b3db616b9e614c23a9ffe3728564c8c2/mobile_task_storage_prod/secure/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MgInsurance", "MG손해보험", Category.Insurance, "https://www.mggeneralins.com/", "MG Non-Life Insurance CO., LTD.", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.mggeneralins.com/webdocs/resources/anySign4Pc/install/v1.1.0.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("NhInsurance", "NH농협손해보험", Category.Insurance, "https://www.nhfire.co.kr/", "NONGHYUP PROPERTY & CASUALTY INSURANCE", [
         new("Veraport", "https://www.nhfire.co.kr/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWeb", "https://www.nhfire.co.kr/initech/extension/down/INIS_EX.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ePageSafer", "https://www.nhfire.co.kr/markany/exe/Setup_ePageSaferRT.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("SeoulGuarantyInsurance", "SGI서울보증", Category.Insurance, "https://www.sgic.co.kr/", "Seoul Guarantee Insurance Company", [
         new("Veraport", "https://www.sgic.co.kr/chp/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.0.15/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnFirewall", "https://www.sgic.co.kr/chp/TouchEn/nxFw/module/TEFW_Installer.exe", SilentSwitches.CustomSilenceSwitch),
         new("TouchEnKey32", "https://www.sgic.co.kr/chp/TouchEn/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("KSCertRelay32", "https://www.sgic.co.kr/chp/TouchEn/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("ePostLife", "우체국보험", Category.Insurance, "https://epostlife.go.kr/", "Insurance Service by Korea Post", [
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("TouchEn_nxKey_32bit", "https://epostlife.go.kr/sw/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("Gov24", "정부24", Category.Government, "https://www.gov.kr/", "Government 24", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.0.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "https://download.raonsecure.com/TouchEnnxKey/minwon/TouchEn_nxKey_Installer_32bit_1.0.0.47.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("ReserveForces", "예비군", Category.Government, "https://yebigun1.mil.kr/", "Reserve Forces", [
         new("MagicLine", "https://yebigun1.mil.kr/MagicLine4NPIZ/magicline4np/install_bin/magicline4npiz.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Scourt", "대한민국 법원 대국민서비스", Category.Government, "https://www.scourt.go.kr/", "Korea Supreme Court Public Service", [
         new("AnySign", "https://safind.scourt.go.kr/AnySign/AnySign4PC/install/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("eFamily", "대한민국 법원 전자가족관계등록시스템", Category.Government, "https://efamily.scourt.go.kr/", "Korea Court Electronic Family Registration System", [
         new("TouchEnKey32", "https://efamily.scourt.go.kr/TouchEn/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("AnySign", "https://efamily.scourt.go.kr/AnySign/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Iros", "대한민국 법원 인터넷등기소", Category.Government, "http://www.iros.go.kr/", "Korea Court Internet Registration Office", [
         new("EnableTrustedSite", "http://www.iros.go.kr/iris/TrustedSiteSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.3.3/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("TouchEnKey32", "http://www.iros.go.kr/XecureObject/raonsecure/TouchEn/nxKey/module/10047/TouchEn_nxKey_Installer_32bit.exe?ver=1.0.0.83", SilentSwitches.CustomSilenceSwitch),
         new("Veraport", "https://www.iros.go.kr/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("InstallSafer", "https://www.iros.go.kr/iris/InstallSafer.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], [
         new("TouchEnExtension", "https://clients2.google.com/service/update2/crx", "dncepekefegjiljlfbihljgogephdhph"),
    ])
    {
        CustomBootstrap = """
          takeown.exe /f "$env:WINDIR\winsxs" /a /r /d Y
          icacls.exe "$env:WINDIR\winsxs" /grant "Administrators:(OI)(CI)F" /T
          mkdir "$env:WINDIR\winsxs\Backup" | Out-Null
          mkdir "$env:WINDIR\winsxs\Catalogs" | Out-Null
          mkdir "$env:WINDIR\winsxs\FileMaps" | Out-Null
          mkdir "$env:WINDIR\winsxs\Fusion" | Out-Null
          mkdir "$env:WINDIR\winsxs\InstallTemp" | Out-Null
          Set-Service -StartupType Automatic -ServiceName TrustedInstaller
          Start-Service -ServiceName TrustedInstaller
          [System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true} ;
          (New-Object Net.WebClient).DownloadFile('http://www.iros.go.kr/iris/VCRedist_sudong.exe',"$env:USERPROFILE\VCRedistManual.exe")
          . "$env:USERPROFILE\VCRedistManual.exe"
          """,
    },
    new("Ecfs", "대한민국 법원 전자소송", Category.Government, "https://ecfs.scourt.go.kr/", "Korea Court Electronic Litigation System", [
         new("TouchEnKey32", "https://ecfs.scourt.go.kr/raonsecure/TouchEn/nxKey/module/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
         new("AnySign", "https://ecfs.scourt.go.kr/AnySign/AnySign4PC/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("Markany", "https://ecfs.scourt.go.kr/markany/bin/installer.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INNORIX", "https://ecfs.scourt.go.kr/innorix/install/INNORIX-Agent.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Papyrus", "https://ecfs.scourt.go.kr/ecfdocs/install/etc/Papyrus-PlugIn-agent_5.0.4.186.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Papyrus-PlugIn-web", "https://ecfs.scourt.go.kr/ecfdocs/install/etc/Papyrus-PlugIn-web.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Papyrus-PlugIn-xfa", "https://ecfs.scourt.go.kr/papyrus/install/Papyrus-PlugIn-xfa.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("BankpayEFT", "https://www.bankpay.or.kr/download/BankPayEFT.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("yessign5", "https://www.bankpay.or.kr/download/yessign5Install.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("DetectorX", "https://ecfs.scourt.go.kr/ecfdocs/install/Advscourt/DetectorX_Advscourt.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ezPDF", "https://ecfs.scourt.go.kr/ecfdocs/install/ezPDF%20WorkBoard2.0/ezPDF_WorkBoard_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Ekt", "대한민국 법원 전자공탁", Category.Government, "https://ekt.scourt.go.kr/", "Korea Court Electronic Deposit System", [
         new("AnySign", "https://ekt.scourt.go.kr/pjg/install/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("AhnLabSafeTx", "https://ekt.scourt.go.kr/pjg/install/astx_setup_ekt.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ezPDFReader", "https://ekt.scourt.go.kr/pjg/install/ezPDFReaderScourtEx_SETUP_3.0.0.1.exe", SilentSwitches.NsisSilentSwitch),
         new("Markany", "https://ekt.scourt.go.kr/pjg/install/MarkAny_e_pageSafer_2.5.0.44_installer_20241224.exe", SilentSwitches.NsisSilentSwitch),
         new("INNORIX", "https://ekt.scourt.go.kr/pjg/install/INNORIX-Agent.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://ekt.scourt.go.kr/pjg/install/nos_setup(2025.1.20.2).exe", SilentSwitches.NsisSilentSwitch),
         new("HSMUSBDriver", "https://ekt.scourt.go.kr/pjg/install/XHSMMng_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("DocumentDetector", "https://ekt.scourt.go.kr/pjg/install/DetectorX_Advscourt_250117.exe", SilentSwitches.NsisSilentSwitch),
         new("MarkAnyImageSafer", "https://ekt.scourt.go.kr/pjg/install/markany_ImageSafer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Hometax", "국세청 홈택스", Category.Government, "https://hometax.go.kr/", "National Tax Service Hometax", [
         new("MAGIC-PKI", "https://hometax.speedycdn.net/dn_dir/veraport/magic-pki/magicline4nx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("REXPERT", "https://hometax.speedycdn.net/dn_dir/veraport/rexpert/rexpert30printservice.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KUPLOAD", "https://hometax.speedycdn.net/dn_dir/veraport/kupload/raonkSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MAGIC-XML", "https://hometax.speedycdn.net/dn_dir/veraport/magic-xml/ntsmagicxmlnp_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("IPINSIDE", "https://hometax.speedycdn.net/dn_dir/veraport/ipinside/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
         new("ESAFER", "https://hometax.speedycdn.net/dn_dir/veraport/e-safer/MarkAny_NTS_NOAXRp.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Kibo", "기술보증기금 디지털지점", Category.Government, "https://www.kibo.or.kr/dbranch/", "Korea Technology Credit Guarantee Fund Digital Branch", [
         new("SignKoreaCert", "https://www.kibo.or.kr/app/install/SKCertServiceSetup_v2.5.18_152.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("KorHouseFinance", "주택금융공사", Category.Government, "https://bank.hf.go.kr/", "Korea Housing Finance Corporation", [
         new("Veraport", "https://bank.hf.go.kr/product/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WizInDelfino", "https://bank.hf.go.kr/product/wizvera/delfino/down/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows11/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("iSASService", "https://bank.hf.go.kr/product/coocon/down/NXiSAS.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KFTCVAN", "금융결제원 KFTCVAN", Category.Government, "https://www.kftcvan.or.kr/", "Korea Financial Telecommunications and Clearings Institute VAN", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CMSEDI", "금융결제원 CMS", Category.Government, "https://www.cmsedi.or.kr/cms", "KFTC Cash Management Service", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CMSJiroEdi", "금융결제원 지로 EDI", Category.Government, "https://www.cmsedi.or.kr/edi", "KFTC Giro EDI Service", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BillingOnePlus", "금융결제원 빌링원플러스", Category.Government, "https://www.billingone.or.kr", "KFTC Billing One Plus", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Narabill", "금융결제원 나라빌", Category.Government, "https://www.narabill.kr/", "KFTC Nara Bill", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Trusbill", "금융결제원 트러스빌", Category.Government, "https://www.trusbill.kr/", "KFTC Trust Bill", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KSureCyberCenter", "한국무역보험공사 사이버영업점", Category.Government, "https://cyber.ksure.or.kr/", "K-SURE Cyber Business Center", [
         new("KeySharpBiz", "https://cyber.ksure.or.kr/ws5/lib/raonnx/ksbiz/module/KSbiz_Installer_32bit.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("Kmrs", "착오송금반환지원정보시스템", Category.Government, "https://kmrs.kdic.or.kr/", "Mistaken Remittance Return Support Information System", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("INISAFECrossWebEx", "https://kmrs.kdic.or.kr/initech/SW/initech/extension/down/INIS_EX_SHA2.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("GHealth", "G-health 공공보건포털", Category.Government, "https://www.g-health.kr/", "G-Health Public Health Portal", [
         new("AnySign", "https://www.g-health.kr/UserFiles/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://www.g-health.kr/UserFiles/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("VaccinationHelper", "질병관리청 예방접종도우미", Category.Government, "https://nip.kdca.go.kr/", "KDCA Vaccination Helper", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.2.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Gubspb", "경기도 청소년 교통비 지원 포털", Category.Government, "https://www.gbuspb.kr/", "Gyeonggi Youth Transportation Subsidy Portal", [
         new("iSASService", "https://www.gbuspb.kr/userResource/file/NXiSAS.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ECar", "자동차민원 대국민포털", Category.Government, "https://www.ecar.go.kr/Index.jsp", "Automobile Civil Affairs Portal", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.2.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("KGeoPlatform", "K-Geo 플랫폼", Category.Government, "https://kgeop.go.kr/", "K-Geo Platform", [
         new("UniSign", "https://kgeop.go.kr/myland/plugins/crosscert/CC_WSTD_home/install/UniSignCRSV3Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("SSIS", "희망이음 사회서비스 포털", Category.Government, "https://www.ssis.go.kr/", "Hope Link Social Service Portal", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOSTypeA", "https://www.ssis.go.kr/sw/inca/nos_setup-type_a.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.ssis.go.kr/lib/AnySignLitePlus/install/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("W4C", "사회복지시설정보시스템", Category.Government, "http://www.w4c.go.kr/", "Social Welfare Facility Information System", [
         new("XPlatform", "http://www.w4c.go.kr/userimg/img/notice/XPLATFORM9.2_SetupEngine.exe", SilentSwitches.DefaultSilentSwitch),
         new("NamoWecClient", "http://www.w4c.go.kr/mipf/img/mi_setup/NamoWecClient.exe", SilentSwitches.DefaultSilentSwitch),
         new("MagicLine", "http://www.w4c.go.kr/mipf/img/mi_setup/MagicLineMBXSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("Rexpert", "http://www.w4c.go.kr/userimg/img/notice/rexpert30viewer.exe", SilentSwitches.DefaultSilentSwitch),
         new("EWS", "http://download.signgate.com/download/2048/ews/ewsinstaller_full.exe", SilentSwitches.DefaultSilentSwitch),
         new("MWSW", "http://www.w4c.go.kr/mipf/img/mi_setup/Setup_MWSWRex.exe", SilentSwitches.DefaultSilentSwitch),
         new("ClientSetup", "http://www.w4c.go.kr/userimg/img/notice/SetupClientDirect.exe", SilentSwitches.DefaultSilentSwitch),
    ], [])
    {
        CustomBootstrap = """
          takeown.exe /f "$env:WINDIR\winsxs" /a /r /d Y
          icacls.exe "$env:WINDIR\winsxs" /grant "Administrators:(OI)(CI)F" /T
          mkdir "$env:WINDIR\winsxs\Backup" | Out-Null
          mkdir "$env:WINDIR\winsxs\Catalogs" | Out-Null
          mkdir "$env:WINDIR\winsxs\FileMaps" | Out-Null
          mkdir "$env:WINDIR\winsxs\Fusion" | Out-Null
          mkdir "$env:WINDIR\winsxs\InstallTemp" | Out-Null

          Set-Service -StartupType Automatic -ServiceName TrustedInstaller
          Start-Service -ServiceName TrustedInstaller
          """,
    },
    new("Unipass", "관세청 유니패스", Category.Government, "https://unipass.customs.go.kr/", "Korea Customs Service Unipass", [
         new("MagicLine4nx", "https://unipass.customs.go.kr/csp/MagicLine4Web/ML4Web/install_bin/magicline4nx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("KoreaGovMail", "공직자통합메일", Category.Government, "https://mail.korea.kr", "Korea Government Integrated Mail", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.3.3/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("ASTx", "https://safetx.ahnlab.com/master/win/default/common/astxdn.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Cardrotax", "국고금 신용카드 수납", Category.Government, "https://office.cardrotax.kr/", "National Treasury Credit Card Collection", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NextShare", "행정정보공동이용시스템", Category.Government, "https://next.share.go.kr/", "Administrative Information Sharing System", [
         new("NOS", "https://next.share.go.kr/static/js/vendor/pluginfree/nos_setup_.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("NHIS", "국민건강보험", Category.Government, "https://www.nhis.or.kr/", "National Health Insurance Service", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.3.3/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Work24", "고용24", Category.Government, "https://www.ei.go.kr/", "Employment 24", [
         new("AnySign4PC", "https://www.work24.go.kr/cm/static/solution/AnySignLite_31/module/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("CertShare", "https://www.work24.go.kr/cm/static/solution/AnySignLite_31/XecureCertShare/module/CertShare_Installer.exe", SilentSwitches.DefaultSilentSwitch),
         new("TouchEnNxKey", "https://www.work24.go.kr/cm/static/solution/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("ETaxSeoul", "서울시ETAX", Category.Government, "https://etax.seoul.go.kr/", "Seoul City ETAX", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.0.7/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("ASTx", "https://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Wetax", "위택스", Category.Government, "https://www.wetax.go.kr/", "WeTax", [
         new("KeySharNXBiz", "https://down.wetax.go.kr/ext/raonnx/ksbiz/module/KSbiz_Installer_32bit.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("4Insure", "4대사회보험정보연계센터", Category.Government, "https://www.4insure.or.kr/", "Four Major Social Insurance Information Linkage Center", [
         new("Veraport", "https://www.4insure.or.kr/solution/wizvera/veraport/down/veraport-g3-x64.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("AnySign", "https://www.4insure.or.kr/solution/AnySign_19/AnySign4PC/download/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
         new("ASTxFor4Insure", "https://www.4insure.or.kr/solution/Ahnlab/AOS2/astx_setup_4insure.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("CCRS", "신용회복위원회 사이버상담부", Category.Government, "https://cyber.ccrs.or.kr/", "Credit Counseling & Recovery Service Cyber Department", [
         new("Delfino", "https://dn.ccrs.or.kr/win/delfino-g3.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NXiSAS", "https://dn.ccrs.or.kr/win/NXiSAS.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("RpsKEnergy", "한국에너지공단 신재생에너지 RPS 포털", Category.Government, "https://rps.energy.or.kr/", "Korea Energy Agency Renewable Energy RPS Portal", [
         new("SecuKitNXS", "https://kcert.energy.or.kr/SecuKitNXS/Install/SecuKitNXS.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MyDataPortal", "마이데이터 종합포털", Category.Government, "https://www.mydatacenter.or.kr:3441/", "MyData Comprehensive Portal", [
         new("NOS", "https://www.mydatacenter.or.kr:3441/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MagicLine4NX", "https://www.mydatacenter.or.kr:3441/MagicLine4Web/ML4Web/install_bin/magicline4nx_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Q-net", "Q-net", Category.Government, "https://www.q-net.or.kr/", "Q-net", [
         new("KSignCASE", "https://www.q-net.or.kr/ksign/kcase/lib/download/KSignCASE_For_HTML5_Windows_v1.3.29.exe", SilentSwitches.NsisSilentSwitch),
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("BucheonWaterPay", "부천시 수도요금 납부 사이트", Category.Government, "https://waterpay.bucheon.go.kr/", "Bucheon City Water Bill Payment Site", [
    ], []),
    new("LHApply", "LH청약플러스", Category.Government, "https://apply.lh.or.kr/", "LH Application Plus", [
         new("KeySharp Biz", "https://apply.lh.or.kr/raonnx/ksbiz/module/KSbiz_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("CultureLand", "컬처랜드", Category.Other, "https://www.cultureland.co.kr/", "Culture Land", [
         new("TouchEnKey32", "https://www.cultureland.co.kr/resources/web/js/raonnx/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("HappyMoney", "해피머니", Category.Other, "https://www.happymoney.co.kr/", "Happy Money", [
         new("TouchEnKey32", "https://www.happymoney.co.kr/svc/resources/TouchEn/nxKey/module/TouchEn_nxKey_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("ILogenEnterprise", "로젠택배 기업용", Category.Other, "https://www.ilogen.com/web/enterprise/system", "Logen Express Enterprise", [
         new("wLogenLauncher", "http://cust.ilogen.com/install/newlauncher/TPLSvc_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("iLogen", "https://www.ilogen.com/static/download/setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("ContInsure", "나의 숨은 보험금 찾기", Category.Other, "https://cont.insure.or.kr/", "Find My Hidden Insurance Claims", [
         new("VertCert", "https://cert.niceid.co.kr/vestsignCertWeb/VestCertSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("PayInfo", "계좌정보통합관리", Category.Other, "https://www.payinfo.or.kr", "Integrated Account Information Management", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("MobiSign", "https://download.raonsecure.com/MobiSign/v5.0.5.0/xccr.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("TMoney", "티머니카드", Category.Other, "https://pay.tmoney.co.kr/", "T-Money", [
         new("TouchEnKey32", "http://download.raonsecure.com/TouchEnnxKey/current/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("Cashbee", "캐시비카드", Category.Other, "https://www.cashbee.co.kr", "CashBee", [
         new("TouchEnKey32", "http://download.raonsecure.com/TouchEnnxKey/current/TouchEn_nxKey_Installer_32bit.exe", SilentSwitches.CustomSilenceSwitch),
    ], []),
    new("KTCU", "한국교직원공제회", Category.Other, "https://www.ktcu.or.kr/", "Korea Teachers Credit Union", [
         new("CrossWarpEX", "https://www.ktcu.or.kr/resources/pc/crosswarpex/extension/down/CrossWarpEX.exe", SilentSwitches.DefaultSilentSwitch),
         new("Crosscert", "https://www.ktcu.or.kr/resources/pc/CrossCert/CC_WSTD_home/install/UniSignCRSV3Setup.exe", SilentSwitches.DefaultSilentSwitch),
         new("ASTx", "http://safetx.ahnlab.com/master/win/default/all/astx_setup.exe", SilentSwitches.CustomSilenceSwitch),
         new("KSCertRelay_nx", "https://www.ktcu.or.kr/resources/pc/raonnx/nxCR/module/KSCertRelay_nx_Installer_32bit.exe", SilentSwitches.DefaultSilentSwitch),
         new("iSASService", "https://www.ktcu.or.kr/resources/pc/Coocon/iSASService_v2.4.9.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("Sema", "과학기술인공제회", Category.Other, "https://www.sema.or.kr/", "Science and Engineering Mutual Aid Association", [
         new("AnySign", "https://download.softforum.com/Published/AnySign/v1.1.0.11/AnySign_Installer.exe", SilentSwitches.DefaultSilentSwitch),
         new("nos_setup", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/sha2/nos_setup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("TeachersPension", "사학연금공단", Category.Other, "https://www.tp.or.kr/", "Teachers Pension Fund", [
         new("KSignCASE", "https://www.tp.or.kr/gpki-setup/window/KSignCASE_For_HTML5_Windows_v1.3.28.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("HiPass", "고속도로통행료 홈페이지", Category.Other, "https://www.hipass.co.kr/", "Highway Toll Fee Homepage", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("MSafer", "명의도용방지서비스 MSafer", Category.Other, "https://www.msafer.or.kr/", "Identity Theft Prevention Service MSafer", [
         new("AnySign", "https://www.msafer.or.kr/AnySign4PC_info/install/v1.1.3.3/AnySign_Installer.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Giro", "인터넷지로", Category.Other, "https://www.giro.or.kr/", "Internet Giro", [
         new("I3GSvcManager", "https://www.giro.or.kr/html/ipinside/I3GSvcManager.exe", SilentSwitches.CustomNoDlgSwitch),
    ], []),
    new("BizGiro", "비즈지로", Category.Other, "https://biz.giro.or.kr/", "Business Giro", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Wehago", "위하고", Category.Other, "https://www.wehago.com/", "Wehago", [
         new("WehagoAgent", "https://wu.wehago.com/wehagoupdate/wehagoagent/wehagoagentInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoSetup", "https://www.wehago.com/pctalkpub/wehago/WEHAGO%20Setup%201.3.44.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoRemoteApp", "https://wu.wehago.com/wehagoupdate/wehagoremoteapp/wehagoremoteappInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoPrintInstaller", "https://wu.wehago.com/wehagoupdate/wehagoprint/wehagoprintInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoScrappingInstaller", "https://wu.wehago.com/wehagoupdate/wehagoscrapping/wehagoscrappingInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("NxtSpkiSetup", "https://wu.wehago.com/download/nxtspkisetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoNTSInstaller", "https://wu.wehago.com/wehagoupdate/wehagonts/wehagontsInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoFaxSetup", "https://wu.wehago.com/download/WEHAGOFAXSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("WehagoYeTaxSetup", "https://wu.wehago.com/wehagoupdate/wehagoyetax/wehagoyetaxInstaller.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], [
          new("WehagoRTC", "https://clients2.google.com/service/update2/crx", "eljddogfdgnndbemlmjgmocgjpniamie"),
    ]),
    new("MedCerti", "병원증명발급포털 메드서티 (medcerti.com)", Category.Other, "https://www.medcerti.com/", "Medcerti Hospital Certificate Issuance", [
         new("ICT_REPORTX", "https://www.medcerti.com/hp1.0/activex/ICT_REPORTX_SETUP.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("LemonCare", "레몬케어 병원제증명서발급포털", Category.Other, "https://lemoncare.lemonhc.com/", "Lemon Care", [
         new("ezPDFPrint", "https://unidocs.lemonhc.com/webviewer/ezpdf/print/ezPDFPrintEx_SETUP_1.0.0.19.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("KoreaUnivAnamCert", "고려대학교 안암병원 증명서 발급", Category.Other, "https://anam.kumc.or.kr/kr/reservation-issue/service/issued.do", "Korea University Anam Hospital Certificate Issuance", [
         new("WebCubeAgent", "https://medicopy.lemonhc.com/lct/medicopy/WebCube/components/WebCubeAgentSetup.exe", SilentSwitches.NsisSilentSwitch),
    ], []),
    new("Neis", "나이스 대국민 서비스", Category.Education, "https://neis.go.kr/", "National Education Information System", [
         new("NOS", "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("KCaseAgent", "https://update.ksign.com/eis/neis/KCaseAgent_Installer.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("DonggukUdrims", "동국대학교 정보시스템 UDRIMS", Category.Education, "http://udrims.dongguk.edu/", "Dongguk University Online Services UDRIMS", [
    ], [])
    {
        CustomBootstrap = """
          $TempDir = [IO.Path]::GetTempPath()
          $MsiList = [Collections.Generic.List[String]]@()

          $TargetUri = 'https://udrims.dongguk.edu/MiDongguk/install320U/update_files_msi/MiPlatform_InstallBase320.msi'
          $FileName = "$([IO.Path]::GetFileName($TargetUri))"
          $TargetPath = Join-Path -Path $TempDir -ChildPath $FileName
          curl.exe -L $TargetUri -o $TargetPath
          $MsiList.Add($TargetPath)

          $TargetUri = 'https://udrims.dongguk.edu/MiDongguk/install320U/update_files_msi/MiPlatform_InstallEngine320U.msi'
          $FileName = "$([IO.Path]::GetFileName($TargetUri))"
          $TargetPath = Join-Path -Path $TempDir -ChildPath $FileName
          curl.exe -L $TargetUri -o $TargetPath
          $MsiList.Add($TargetPath)

          $TargetUri = 'https://udrims.dongguk.edu/MiDongguk/install320U/update_files_msi/MiPlatform_FixUACProblem320U.msi'
          $FileName = "$([IO.Path]::GetFileName($TargetUri))"
          $TargetPath = Join-Path -Path $TempDir -ChildPath $FileName
          curl.exe -L $TargetUri -o $TargetPath
          $MsiList.Add($TargetPath)

          $TargetUri = 'https://udrims.dongguk.edu/MiDongguk/install320U/MiPlatform_Updater321_20190129_1120.cab'
          $FileName = "$([IO.Path]::GetFileName($TargetUri))"
          $TargetPath = Join-Path -Path $TempDir -ChildPath $FileName
          curl.exe -L $TargetUri -o $TargetPath

          expand.exe -F:* "$TargetPath" "$TempDir"

          $Target = (Get-ChildItem -Path $TempDir -Filter 'MiPlatform*Updater*.exe')[0].FullName
          Write-Output "Install $Target package."
          Start-Process "$($Target)" -Wait -ArgumentList '/passive'
          foreach ($EachMsi in $MsiList) {
          Write-Output "Install $EachMsi package."
          Start-Process msiexec.exe -Wait -ArgumentList "/I $($EachMsi) /passive"
          }
          """,
    },
    new("Webminwon", "대학증명발급 웹민원센터 (webminwon.com)", Category.Education, "https://www.webminwon.com/", "WebMinwon.com Certificate Issuance", [
         new("ICT_REPORTX", "https://uni.webminwon.com/wm1.0/activex/ICT_REPORTX_SETUP.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Certpia", "Certpia 인터넷증명발급센터 (certpia.com)", Category.Education, "https://www.certpia.com/", "CertPia.com Certificate Issuance", [
         new("ViewerApp", "https://www.certpia.com/upfile/ocx/Plugin/viewerSetup.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
    new("Knou", "한국방송통신대학교 증명서발급", Category.Education, "https://certi.knou.ac.kr/haksa/ass/cint/ASSInetCrtiWebLinkPage.do", "Korea National Open University Certificate Issuance", [
         new("OZReportLauncher", "https://www.knou.ac.kr/bbs/knou/56/74346/download.do", SilentSwitches.DefaultSilentSwitch),
         new("OZReportPrinter", "https://www.knou.ac.kr/bbs/knou/56/6389/download.do", SilentSwitches.DefaultSilentSwitch),
         new("OZWebLauncher", "https://util.knou.ac.kr/ozweblauncher/OnLine_Install_Dialog_UI_SSL.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("InhaUniv", "인하대학교 증명서발급", Category.Education, "https://cert.inha.ac.kr/", "INHA University Certificate Issuance", [
         new("iCertPrintClient", "https://cert.inha.ac.kr/icerti/certInstall/install/iCertPrintClientSetup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("CNU", "충남대학교 인터넷증명서발급", Category.Education, "https://cnu.icerti.com/icerti/index_internet.jsp https://vpn.cnu.ac.kr/", "Chungnam National University Certificate Issuance", [
         new("iCertPrintClient", "https://cnu.icerti.com/icerti/certInstall/install/iCertPrintClientSetup.exe", SilentSwitches.DefaultSilentSwitch),
         new("SecuwaySSLSUV1.0", "https://vpn.cnu.ac.kr/cabfiles/SecuwaySSLUV1.0-Client.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("GTEC", "경기과학기술대학교 종합정보시스템", Category.Education, "https://gctis.gtec.ac.kr/index2022.jsp", "Gyeonggi University of Science and Technology Online Services", [
         new("TPLSvc", "https://gctis.gtec.ac.kr/xui/install/download/TPLSvc_Setup.exe", SilentSwitches.InnoSetupSilentSwitch),
         new("XPlatform", "https://gctis.gtec.ac.kr/xui/install/download/XPLATFORM9.2_SetupEngine.exe", SilentSwitches.DefaultSilentSwitch),
         new("CX60_OCX", "https://gctis.gtec.ac.kr/xui/install/download/CX60_OCX_setup.exe", SilentSwitches.DefaultSilentSwitch),
    ], []),
    new("SCU", "서울사이버대학교", Category.Education, "https://sso.iscu.ac.kr/ptl/sso/SsoLoginForm.scu", "Seoul Cyber University", [
         new("SecuKitNXS", "https://kcert.energy.or.kr/SecuKitNXS/Install/SecuKitNXS.exe", SilentSwitches.InnoSetupSilentSwitch),
    ], []),
];

var catalog = new TableClothCatalog { Companions = companions, InternetServices = services.UpdateRequirements(), };

using var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;
Console.CancelKeyPress += (_sender, _e) => cts.Cancel();

// XML 파일로 저장
var catalogXmlFilePath = Path.GetFullPath("Catalog.xml");
using (var catalogXmlStream = File.Open(catalogXmlFilePath, FileMode.Create, FileAccess.Write))
{
     // XLinq를 사용한 XML 문서 생성
     var xmlDocument = new XDocument(
     new XDeclaration("1.0", "utf-8", "standalone"),
     new XElement("TableClothCatalog",
          new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
          new XAttribute(XName.Get("noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance"), "http://yourtablecloth.app/TableClothCatalog/Catalog.xsd"),
          new XAttribute("Fallback", "ko-KR"),
          new XElement("Companions",
               companions.Select(c =>
                    new XElement("Companion",
                         new XAttribute("Id", c.Id),
                         new XAttribute("DisplayName", c.DisplayName),
                         new XAttribute("Url", c.Url),
                         new XAttribute("Arguments", c.Arguments),
                         new XAttribute("EnglishDisplayName", c.EnglishDisplayName)
                    )
               )
          ),
          new XElement("InternetServices",
               services.Select(s =>
                    new XElement("Service",
                         new XAttribute("Id", s.Id),
                         new XAttribute("DisplayName", s.DisplayName),
                         new XAttribute("Category", s.Category.ToString()),
                         new XAttribute("Url", s.Url),
                         new XAttribute("EnglishDisplayName", s.EnglishDisplayName),
                         s.CompatNotes.Any() ?
                              new XElement("CompatNotes", string.Join(Environment.NewLine, s.CompatNotes)) :
                              default,
                         s.EnglishCompatNotes.Any() ?
                              new XElement("en-US-CompatNotes", string.Join(Environment.NewLine, s.EnglishCompatNotes)) :
                              default,
                         new XElement("Packages",
                         s.Packages.Select(p =>
                              new XElement("Package",
                                   new XAttribute("Name", p.Name),
                                   new XAttribute("Url", p.Url),
                                   new XAttribute("Arguments", p.Arguments)
                              )
                         )
                         ),
                         s.EdgeExtensions.Any() ? new XElement("EdgeExtensions",
                              s.EdgeExtensions.Select(e =>
                                   new XElement("EdgeExtension",
                                        new XAttribute("Name", e.Name),
                                        new XAttribute("CrxUrl", e.CrxUrl),
                                        new XAttribute("ExtensionId", e.ExtensionId)
                                   )
                              )
                         ) : default,
                         !string.IsNullOrWhiteSpace(s.CustomBootstrap) ?
                              new XElement("CustomBootstrap", new XCData(s.CustomBootstrap)) :
                              default
                    )
               )
          )
     )
     );

     using var xmlWriter = XmlWriter.Create(catalogXmlStream, new()
     {
          Async = true,
          Encoding = new UTF8Encoding(false),
          Indent = true,
     });

     await xmlDocument.SaveAsync(xmlWriter, cancellationToken).ConfigureAwait(false);
     Console.WriteLine($"XML document created and saved as '{catalogXmlFilePath}'");
}

var catalogJsonFilePath = Path.GetFullPath("Catalog.json");
using (var catalogJsonStream = File.Open(catalogJsonFilePath, FileMode.Create, FileAccess.Write))
{
     // JSON 문서 생성
     var jsonOptions = new JsonSerializerOptions
     {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
     };

     var jsonData = new
     {
          Fallback = "ko-KR",
          Companions = companions.Select(companion => new
          {
               companion.Id,
               companion.DisplayName,
               companion.Url,
               companion.Arguments,
               companion.EnglishDisplayName
          }),
          InternetServices = services.Select(s => new
          {
               s.Id,
               s.DisplayName,
               Category = s.Category.ToString(),
               s.Url,
               s.EnglishDisplayName,
               Packages = s.Packages.Select(p => new
               {
                    p.Name,
                    p.Url,
                    p.Arguments
               }),
               EdgeExtensions = s.EdgeExtensions.Select(e => new
               {
                    e.Name,
                    e.CrxUrl,
                    e.ExtensionId
               }),
               CompatNotes = string.Join(Environment.NewLine, s.CompatNotes),
               EnglishCompatNotes = string.Join(Environment.NewLine, s.EnglishCompatNotes),
               s.CustomBootstrap,
          }),
     };

     // JSON 파일로 저장
     await JsonSerializer.SerializeAsync(catalogJsonStream, jsonData, jsonOptions, cancellationToken).ConfigureAwait(false);
     Console.WriteLine($"JSON document created and saved as '{catalogJsonFilePath}'");
}

// SiteInfoCatalog을 JSON 문서로 저장
var siteInfoCatalog = new SiteInfoCatalog { Sites = sites };
var siteInfoCatalogJsonFilePath = Path.GetFullPath("Sites.json");
using (var siteInfoCatalogJsonStream = File.Open(siteInfoCatalogJsonFilePath, FileMode.Create, FileAccess.Write))
{
     // JSON 문서 생성
     var jsonOptions = new JsonSerializerOptions
     {
          WriteIndented = true,
          Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
     };

     var jsonData = new
     {
          Sites = sites.Select(site => new
          {
               site.Domain,
               site.Name,
               Submenus = site.Submenus.Select(submenu => new
               {
                    submenu.Name,
                    submenu.Url,
                    submenu.Description
               })
          })
     };

     // JSON 파일로 저장
     await JsonSerializer.SerializeAsync(siteInfoCatalogJsonStream, jsonData, jsonOptions, cancellationToken).ConfigureAwait(false);
     Console.WriteLine($"SiteInfoCatalog JSON document created and saved as '{siteInfoCatalogJsonFilePath}'");
}

public sealed record class Companion(
    string Id, string DisplayName, string Url, string Arguments, string EnglishDisplayName);

public sealed class CompanionCollection : KeyedCollection<string, Companion>
{
    protected override string GetKeyForItem(Companion item) => item.Id;
}

public sealed record class Package(
    string Name, string Url, string Arguments);

public sealed class PackageCollection : KeyedCollection<string, Package>
{
     protected override string GetKeyForItem(Package item) => item.Name;
}

public sealed record class EdgeExtension(
     string Name, string CrxUrl, string ExtensionId);

public sealed class EdgeExtensionCollection : KeyedCollection<string, EdgeExtension>
{
     protected override string GetKeyForItem(EdgeExtension item) => item.Name;
}

public sealed record class Service(
    string Id, string DisplayName, Category Category, string Url, string EnglishDisplayName, PackageCollection Packages, EdgeExtensionCollection EdgeExtensions)
{
     public List<string> CompatNotes { get; set; } = [];
     public List<string> EnglishCompatNotes { get; set; } = [];
     public string CustomBootstrap { get; set; } = "";
}

public sealed class ServiceCollection : KeyedCollection<string, Service>
{
    protected override string GetKeyForItem(Service item) => item.Id;
}

public enum Category { Other = 0, Banking, Financing, Security, Insurance, CreditCard, Government, Education, }

public sealed class TableClothCatalog
{
     public CompanionCollection Companions { get; set; } = [];
     public ServiceCollection InternetServices { get; set; } = [];
}

public sealed record class Submenu(string Name, string Url, string Description = "");

public sealed class SubmenuCollection : List<Submenu> { }

public sealed record class Site(string Domain, string Name, SubmenuCollection Submenus) { }

public sealed class SiteCollection : KeyedCollection<string, Site>
{
     protected override string GetKeyForItem(Site item) => item.Domain;
}

public sealed class SiteInfoCatalog
{
     public SiteCollection Sites { get; set; } = [];
}

public static class Extensions
{
     public static ServiceCollection UpdateRequirements(this ServiceCollection services)
     {
          foreach (var service in services)
          {
               // ASTx Requirements
               if (service.Packages.Any(x => x.Name.Contains("astx", StringComparison.OrdinalIgnoreCase)))
               {
                    service.CompatNotes.Add("이 웹 사이트는 해당 기관의 보안 정책에 따라 AhnLab Safe Transaction이 Windows Sandbox의 필수 구성 요소인 RDP 세션을 강제 종료하도록 구성되어있습니다. https://yourtablecloth.app/troubleshoot.html 페이지를 참고하여 AST가 원격 연결을 허용하도록 사이트 이용 전에 먼저 변경한 후 접속하는 것을 권장합니다.");
                    service.EnglishCompatNotes.Add("This website is configured to force RDP sessions to be terminated by AhnLab Safe Transaction, which is a required component of Windows Sandbox, in accordance with your institution's security policy. We recommend that you refer to https://yourtablecloth.app/troubleshoot.html and change the AST to allow remote connections before using the site.");
               }
          }

          return services;
     }
}

public static class SilentSwitches
{
     public static readonly string InnoSetupSilentSwitch = "/silent";

     public static readonly string NsisSilentSwitch = "/S";

     public static readonly string CustomNoDlgSwitch = "/nodlg";

     public static readonly string CustomSilenceSwitch = "/silence";

     public static readonly string DefaultSilentSwitch = "/S";
}
