# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27", "pillow>=10.3", "beautifulsoup4>=4.12", "numpy>=1.26"]
# ///
"""
fetch_logo.py — 기관/서비스 공식 로고를 확보해 '투명 배경 정사각 PNG'로 만든다.
(카탈로그 규격: docs/images/<Category>/<Id>.png)

두 가지 입력 경로
  1) 사이트에서 자동 수집: 대표 URL을 브라우저(Edge)로 위장해 fetch → favicon/
     apple-touch-icon/og:image/웹매니페스트/관용 경로에서 로고 후보를 모아 가장 큰 것 선택.
     (src/fetchfavicon.cs 의 탐색 전략을 Python으로 포팅.)
  2) 로컬 이미지 지정: --from-image 로 이미 받은 아이콘 파일을 그대로 가공.
     (폼 업로드 아이콘은 Drive 링크라 인증이 필요하므로, 로컬로 내려받았다면 이 경로를 쓴다.)

fetchfavicon.cs 와의 차이(= 이 스크립트가 존재하는 이유)
  - C# 도구는 정사각 패딩/업스케일까지만 하고 '배경 제거'를 못 한다.
  - 여기서는 흰/단색 배경을 투명으로 키잉(corner-key)하거나, 필요 시 rembg(ML)로 매팅한다.

배경 제거 모드(--bg)
  auto   : 네 모서리가 균일한 단색이면 그 색을 투명 처리(플랫 로고에 안전, 기본값)
  corner : auto 와 동일하되 균일성 검사 없이 강제 키잉
  rembg  : ML 매팅(사진/복잡한 배경용). `uv run --with rembg fetch_logo.py ...` 로 실행해야 함
  none   : 배경 유지(알파만 보장)

사용법
  uv run fetch_logo.py <site_url> <output.png> [--size 256] [--bg auto|corner|rembg|none]
  uv run fetch_logo.py --from-image logo.jpg <output.png> [--size 256] [--bg auto]
"""
from __future__ import annotations

import argparse
import io
import json
import re
import sys
from urllib.parse import urljoin, urlparse

import httpx
import numpy as np
from bs4 import BeautifulSoup
from PIL import Image

for _stream in (sys.stdout, sys.stderr):
    try:
        _stream.reconfigure(encoding="utf-8")  # type: ignore[union-attr]
    except Exception:  # noqa: BLE001
        pass

EDGE_UA = ("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
           "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0")
EDGE_HEADERS = {
    "User-Agent": EDGE_UA,
    "sec-ch-ua": '"Microsoft Edge";v="131", "Chromium";v="131", "Not_A Brand";v="24"',
    "sec-ch-ua-mobile": "?0",
    "sec-ch-ua-platform": '"Windows"',
    "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/*;q=0.9,*/*;q=0.8",
    "Accept-Language": "ko-KR,ko;q=0.9,en;q=0.8",
}

COMMON_PATHS = [
    "/apple-touch-icon.png", "/apple-touch-icon-180x180.png",
    "/android-chrome-512x512.png", "/android-chrome-192x192.png",
    "/icon-512x512.png", "/logo.png", "/favicon-32x32.png",
    "/favicon.png", "/favicon.ico",
]


def fetch_bytes(client: httpx.Client, url: str) -> bytes | None:
    try:
        r = client.get(url, follow_redirects=True)
        if r.status_code < 400 and r.content:
            return r.content
    except httpx.HTTPError:
        return None
    return None


def discover_candidates(client: httpx.Client, site_url: str) -> list[str]:
    """페이지 HTML + 관용 경로 + 파비콘 서비스에서 로고 후보 URL 목록."""
    candidates: list[str] = []
    seen: set[str] = set()

    def add(u: str | None) -> None:
        if not u:
            return
        if u.lower().endswith(".svg"):
            return  # Pillow가 SVG를 못 여니 제외(필요 시 별도 래스터화)
        if u not in seen:
            seen.add(u)
            candidates.append(u)

    html = None
    try:
        r = client.get(site_url, follow_redirects=True)
        if r.status_code < 400:
            html = r.text
            base = str(r.url)
    except httpx.HTTPError:
        base = site_url

    if html:
        soup = BeautifulSoup(html, "html.parser")
        for link in soup.find_all("link"):
            rel = " ".join(link.get("rel", [])).lower()
            if "icon" in rel:
                add(urljoin(base, link.get("href", "")))
        og = soup.find("meta", property="og:image")
        if og and og.get("content"):
            add(urljoin(base, og["content"]))
        for man in soup.find_all("link", rel="manifest"):
            man_url = urljoin(base, man.get("href", ""))
            data = fetch_bytes(client, man_url)
            if data:
                try:
                    icons = json.loads(data).get("icons", [])
                    for ic in icons:
                        if ic.get("src"):
                            add(urljoin(man_url, ic["src"]))
                except (json.JSONDecodeError, AttributeError):
                    pass

    parsed = urlparse(site_url)
    root = f"{parsed.scheme}://{parsed.netloc}"
    for p in COMMON_PATHS:
        add(root + p)

    # 최후의 폴백: 파비콘 서비스(작으니 우선순위 낮게 = 맨 뒤)
    domain = parsed.netloc
    add(f"https://www.google.com/s2/favicons?domain={domain}&sz=128")
    add(f"https://icons.duckduckgo.com/ip3/{domain}.ico")
    return candidates


def load_largest(data: bytes) -> Image.Image | None:
    """바이트 → PIL 이미지. ICO는 내장된 가장 큰 프레임을 고른다."""
    try:
        im = Image.open(io.BytesIO(data))
    except Exception:  # noqa: BLE001
        return None
    if getattr(im, "format", "") == "ICO":
        try:
            sizes = im.ico.sizes()  # {(w,h), ...}
            best = max(sizes, key=lambda s: s[0] * s[1])
            im = im.ico.getimage(best)
        except Exception:  # noqa: BLE001
            pass
    return im.convert("RGBA")


def pick_best(client: httpx.Client, candidates: list[str]) -> tuple[Image.Image, str] | None:
    best: Image.Image | None = None
    best_url = ""
    best_area = -1
    for url in candidates:
        data = fetch_bytes(client, url)
        if not data:
            continue
        im = load_largest(data)
        if im is None:
            continue
        area = im.width * im.height
        # 파비콘 서비스는 폴백이므로 동점일 때 밀리도록 약간 감점
        if "s2/favicons" in url or "duckduckgo" in url:
            area = int(area * 0.5)
        if area > best_area:
            if best is not None:
                best.close()
            best, best_url, best_area = im, url, area
        else:
            im.close()
    if best is None:
        return None
    return best, best_url


def key_out_background(im: Image.Image, mode: str,
                       tol: int = 32, uniform_tol: int = 25) -> Image.Image:
    """모서리 단색 배경을 투명 처리."""
    arr = np.array(im.convert("RGBA")).astype(np.int16)
    h, w = arr.shape[:2]
    corners = np.array([arr[0, 0, :3], arr[0, w - 1, :3],
                        arr[h - 1, 0, :3], arr[h - 1, w - 1, :3]], dtype=np.int16)
    bg = corners.mean(axis=0)
    spread = np.max(np.linalg.norm(corners - bg, axis=1))
    if mode == "auto" and spread > uniform_tol:
        # 모서리가 균일하지 않음 → 이미 투명이거나 배경이 단색이 아님. 손대지 않음.
        return im.convert("RGBA")
    dist = np.linalg.norm(arr[:, :, :3] - bg, axis=2)
    arr[dist < tol, 3] = 0
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def key_out_rembg(im: Image.Image) -> Image.Image:
    try:
        from rembg import remove  # 지연 임포트(무거움)
    except ImportError:
        print("[error] rembg 미설치. `uv run --with rembg fetch_logo.py ...` 로 실행하세요.",
              file=sys.stderr)
        raise SystemExit(3)
    return remove(im.convert("RGBA"))


def trim_and_square(im: Image.Image, size: int) -> Image.Image:
    """알파 경계로 트림 → 정사각 캔버스 중앙 배치 → 목표 크기로 리샘플."""
    im = im.convert("RGBA")
    alpha = np.array(im)[:, :, 3]
    ys, xs = np.where(alpha > 0)
    if len(xs) and len(ys):
        im = im.crop((int(xs.min()), int(ys.min()),
                      int(xs.max()) + 1, int(ys.max()) + 1))
    side = max(im.width, im.height)
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.paste(im, ((side - im.width) // 2, (side - im.height) // 2), im)
    if side != size:
        canvas = canvas.resize((size, size), Image.LANCZOS)
    return canvas


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description="공식 로고 → 투명 정사각 PNG")
    ap.add_argument("site_url", nargs="?", help="대표 사이트 URL(자동 수집)")
    ap.add_argument("output", type=str, help="출력 PNG 경로")
    ap.add_argument("--from-image", type=str, default=None,
                    help="사이트 대신 로컬 이미지 파일을 가공")
    ap.add_argument("--size", type=int, default=256, help="정사각 한 변 픽셀(기본 256)")
    ap.add_argument("--bg", choices=["auto", "corner", "rembg", "none"],
                    default="auto", help="배경 제거 모드")
    ap.add_argument("--timeout", type=float, default=30.0)
    args = ap.parse_args(argv)

    if args.from_image:
        im = Image.open(args.from_image).convert("RGBA")
        source = args.from_image
    else:
        if not args.site_url:
            print("[error] site_url 또는 --from-image 중 하나는 필요합니다.", file=sys.stderr)
            return 2
        with httpx.Client(headers=EDGE_HEADERS, timeout=args.timeout) as client:
            candidates = discover_candidates(client, args.site_url)
            print(f"[info] 로고 후보 {len(candidates)}개 발견", file=sys.stderr)
            picked = pick_best(client, candidates)
        if picked is None:
            print("[error] 로고 후보를 하나도 불러오지 못했습니다.", file=sys.stderr)
            return 1
        im, source = picked

    print(f"[info] 원본: {source} ({im.width}x{im.height})", file=sys.stderr)

    if args.bg in ("auto", "corner"):
        im = key_out_background(im, mode=args.bg)
    elif args.bg == "rembg":
        im = key_out_rembg(im)

    out = trim_and_square(im, args.size)
    out.save(args.output, format="PNG", optimize=True)
    print(f"[ok] 저장: {args.output} ({args.size}x{args.size}, bg={args.bg})", file=sys.stderr)
    print("[note] 생성 이미지는 임시본일 수 있습니다. 가능하면 공식 로고로 교체하세요.",
          file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
