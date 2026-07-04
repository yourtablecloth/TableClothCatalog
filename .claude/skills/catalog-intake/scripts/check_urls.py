# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27"]
# ///
"""
check_urls.py — 보안 플러그인(설치 파일) URL을 '브라우저인 척'(Windows Edge) 요청 헤더로
접속해 생존/최종 URL/파일명/콘텐츠 형식을 확인한다.

왜 스푸핑이 필요한가
  국내 공공/금융 사이트의 다운로드 엔드포인트는 curl/봇 User-Agent 를 403 으로 막는 경우가
  많다. 식탁보가 실제로 도는 Edge IE Mode 환경과 헤더를 일치시켜야 정상 응답을 받는다.
  (프로파일은 src/checksites.cs 의 Edge 131 UA + client-hints 와 동일하게 유지.)

한계
  이 스크립트는 정적 파일 서버/직링크 확인용(httpx, JS 렌더링 없음)이다.
  JS 게이트가 걸린 '사이트 페이지'까지 검증해야 하면 무거운 케이스이므로
  src/checksites.cs(Playwright Edge) 로 폴백한다.

보안
  - http/https 만 허용, 사설/루프백 대역 차단(SSRF 방지).
  - 설치 파일 본문은 실행하지 않는다. 가능하면 HEAD, 아니면 1바이트 Range GET 으로 헤더만 본다.

사용법
  uv run check_urls.py <url> [<url> ...] [--json] [--timeout 30]
"""
from __future__ import annotations

import argparse
import ipaddress
import json
import re
import socket
import sys
from urllib.parse import urlparse, unquote

import httpx

for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")  # type: ignore[union-attr]
    except Exception:  # noqa: BLE001
        pass

# src/checksites.cs 와 동일 프로파일 (일관성 유지)
EDGE_UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
           "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0")
EDGE_HEADERS = {
    "User-Agent": EDGE_UA,
    "sec-ch-ua": '"Microsoft Edge";v="131", "Chromium";v="131", "Not_A Brand";v="24"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Windows"',
    "sec-ch-ua-platform-version": '"15.0.0"',
    "sec-ch-ua-arch": '"x86"',
    "sec-ch-ua-bitness": '"64"',
    "Accept": ("text/html,application/xhtml+xml,application/xml;q=0.9,"
               "application/octet-stream;q=0.9,*/*;q=0.8"),
    "Accept-Language": "ko-KR,ko;q=0.9,en;q=0.8",
    "Upgrade-Insecure-Requests": "1",
}


def is_public_url(url: str) -> tuple[bool, str]:
    """http/https + 공개 IP 만 허용."""
    try:
        p = urlparse(url)
    except Exception as exc:  # noqa: BLE001
        return False, f"parse-error: {exc}"
    if p.scheme not in ("http", "https"):
        return False, f"scheme-not-allowed: {p.scheme or '(none)'}"
    host = p.hostname
    if not host:
        return False, "no-host"
    try:
        infos = socket.getaddrinfo(host, None)
    except socket.gaierror as exc:
        return False, f"dns-failed: {exc}"
    for info in infos:
        ip = ipaddress.ip_address(info[4][0])
        if (ip.is_private or ip.is_loopback or ip.is_link_local
                or ip.is_reserved or ip.is_multicast):
            return False, f"blocked-ip: {ip}"
    return True, "ok"


def filename_from(url: str, content_disposition: str | None) -> str:
    if content_disposition:
        m = re.search(r"filename\*?=(?:UTF-8'')?\"?([^\";]+)", content_disposition)
        if m:
            return unquote(m.group(1)).strip()
    path = urlparse(url).path
    name = path.rsplit("/", 1)[-1]
    return unquote(name)


def check_one(client: httpx.Client, url: str) -> dict:
    result: dict = {"url": url, "ok": False}
    allowed, reason = is_public_url(url)
    if not allowed:
        result["error"] = f"skipped ({reason})"
        return result
    try:
        # HEAD 우선(가벼움). 일부 서버는 HEAD 미지원 → Range GET 으로 폴백.
        resp = client.head(url, follow_redirects=True)
        if resp.status_code >= 400 or resp.status_code == 405:
            resp = client.get(url, follow_redirects=True,
                              headers={"Range": "bytes=0-0"})
    except httpx.HTTPError as exc:
        result["error"] = f"{type(exc).__name__}: {exc}"
        return result

    cd = resp.headers.get("content-disposition")
    result.update({
        "ok": resp.status_code < 400,
        "status": resp.status_code,
        "final_url": str(resp.url),
        "redirected": str(resp.url) != url,
        "content_type": resp.headers.get("content-type", ""),
        "content_length": resp.headers.get("content-length", ""),
        "filename": filename_from(str(resp.url), cd),
    })
    return result


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description="보안 플러그인 URL 생존/파일명 확인 (Edge 스푸핑)")
    ap.add_argument("urls", nargs="+", help="확인할 URL(들)")
    ap.add_argument("--json", action="store_true", help="JSON 출력")
    ap.add_argument("--timeout", type=float, default=30.0, help="타임아웃 초(기본 30)")
    args = ap.parse_args(argv)

    results: list[dict] = []
    with httpx.Client(headers=EDGE_HEADERS, timeout=args.timeout,
                      verify=True) as client:
        for url in args.urls:
            results.append(check_one(client, url))

    if args.json:
        print(json.dumps(results, ensure_ascii=False, indent=2))
    else:
        for r in results:
            if r["ok"]:
                mark = "✅"
                detail = (f"{r['status']} · {r['filename'] or '-'} · "
                          f"{r['content_type'] or '-'}"
                          + ("  ↪redirect" if r.get("redirected") else ""))
            else:
                mark = "❌"
                detail = r.get("error") or f"status {r.get('status')}"
            print(f"{mark} {r['url']}")
            print(f"    {detail}")

    return 0 if all(r["ok"] for r in results) else 1


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
