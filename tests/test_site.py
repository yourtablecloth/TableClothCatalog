"""Check published HTML data fidelity, navigation and hostile XML handling."""

from html.parser import HTMLParser
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest
from urllib.parse import urlsplit
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


class Page(HTMLParser):
    def __init__(self, text):
        super().__init__(convert_charrefs=True)
        self.links = []
        self.ids = []
        self.scripts = []
        self.meta = []
        self.feed(text)

    def handle_starttag(self, tag, attributes):
        attributes = dict(attributes)
        if 'id' in attributes:
            self.ids.append(attributes['id'])
        if tag == 'a' and 'href' in attributes:
            self.links.append(attributes['href'])
        if tag == 'script':
            self.scripts.append(attributes)
        if tag == 'meta':
            self.meta.append(attributes)


class SiteTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.temp = tempfile.TemporaryDirectory()
        cls.addClassCleanup(cls.temp.cleanup)
        cls.source = Path(cls.temp.name) / 'source'
        cls.output = Path(cls.temp.name) / 'output'
        shutil.copytree(ROOT / 'docs', cls.source)
        shutil.copytree(cls.source, cls.output)
        subprocess.run([sys.executable, str(ROOT / 'src/buildsite.py'), str(cls.source), str(cls.output)],
                       check=True, capture_output=True, text=True)
        subprocess.run([sys.executable, str(ROOT / 'src/buildwsb.py'), str(cls.source / 'Catalog.xml'),
                        str(cls.output / 'wsb')], check=True, capture_output=True, text=True)
        cls.site_text = (cls.output / 'index.html').read_text(encoding='utf-8')
        cls.skill_text = (cls.output / 'skills/index.html').read_text(encoding='utf-8')
        cls.site = Page(cls.site_text)
        cls.skills = Page(cls.skill_text)

    def test_all_active_sites_have_exactly_one_wsb_link(self):
        services = ET.parse(self.source / 'Catalog.xml').findall('./InternetServices/Service')
        expected = {f'/wsb/{s.get("Id")}.wsb' for s in services}
        links = [href for href in self.site.links if href.startswith('/wsb/')]
        self.assertEqual(set(links), expected)
        self.assertEqual(len(links), len(expected))
        for service in services:
            self.assertIn(service.get('DisplayName'), self.site_text)

    def test_active_skills_preserve_pinned_sources_and_licenses(self):
        active = ET.parse(self.source / 'SkillCatalog.xml').findall('./Skill')
        for skill in active:
            with self.subTest(skill=skill.get('Id')):
                self.assertIn(skill.get('DisplayName'), self.skill_text)
                source = skill.find('Source')
                self.assertIn(source.get('Ref'), self.skill_text)
                self.assertIn(skill.find('License').get('Source'), self.skills.links)
        self.assertNotIn('k-dart/SKILL.md', self.skill_text)
        self.assertNotIn('korea-stock-mcp/SKILL.md', self.skill_text)

    def test_navigation_assets_and_unique_ids(self):
        for page in (self.site, self.skills):
            self.assertEqual(len(page.ids), len(set(page.ids)))
            self.assertIn('/', page.links)
            self.assertIn('/skills/', page.links)
            self.assertFalse(any(m.get('http-equiv', '').lower() == 'refresh' for m in page.meta))
            for link in page.links:
                self.assertIn(urlsplit(link).scheme, ('', 'http', 'https'))
                if link.startswith('/'):
                    path = self.output / urlsplit(link).path.lstrip('/')
                    if path.is_dir():
                        path = path / 'index.html'
                    self.assertTrue(path.is_file(), link)
            for script in page.scripts:
                if script.get('src', '').startswith('/'):
                    self.assertTrue((self.output / script['src'].lstrip('/')).is_file())
        self.assertTrue((self.output / 'Catalog.xml').is_file())
        self.assertTrue((self.output / 'SkillCatalog.xml').is_file())

    def test_hostile_markup_is_escaped_and_urls_do_not_execute(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / 'source'
            output = Path(directory) / 'output'
            shutil.copytree(self.source, source)
            tree = ET.parse(source / 'Catalog.xml')
            service = tree.find('./InternetServices/Service')
            service.set('DisplayName', '<script>alert("injected")</script>')
            service.set('Url', 'javascript:alert(123)')
            tree.write(source / 'Catalog.xml', encoding='utf-8', xml_declaration=True)
            result = subprocess.run([sys.executable, str(ROOT / 'src/buildsite.py'), str(source), str(output)],
                                    capture_output=True, text=True)
            if result.returncode == 0:
                text = (output / 'index.html').read_text(encoding='utf-8')
                self.assertNotIn('<script>alert(', text)
                self.assertIn('&lt;script&gt;', text)
                self.assertFalse(any(urlsplit(h).scheme == 'javascript' for h in Page(text).links))
            else:
                self.assertFalse((output / 'index.html').exists())

    def test_dtd_is_rejected_before_publication(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / 'source'
            output = Path(directory) / 'output'
            shutil.copytree(self.source, source)
            for encoding in ('utf-8', 'utf-16', 'utf-16-le'):
                with self.subTest(encoding=encoding):
                    (source / 'Catalog.xml').write_bytes(
                        '<!DOCTYPE x [<!ENTITY a "boom">]><TableClothCatalog/>'.encode(encoding))
                    result = subprocess.run([sys.executable, str(ROOT / 'src/buildsite.py'), str(source), str(output)],
                                            capture_output=True, text=True)
                    self.assertNotEqual(result.returncode, 0)
                    self.assertFalse((output / 'index.html').exists())


if __name__ == '__main__':
    unittest.main()
