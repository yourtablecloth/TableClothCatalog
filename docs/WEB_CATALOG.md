# Integrated web catalog and standalone WSB launchers

[한국어](WEB_CATALOG.ko.md)

The October 29, 2026 implementation for [issue #149](https://github.com/yourtablecloth/TableClothCatalog/issues/149) generates ordinary HTML during CI and publishes it through the existing GitHub Pages deployment at `catalog.yourtablecloth.app`. Browsers no longer transform the catalog with XSLT.

The web catalog covers site discovery, companion download links, skill discovery and contribution, and standalone Windows Sandbox launchers.

The sections below describe the public routes, contribution process, launcher behavior, build sequence, and verification limits.

## Published routes and preserved XML feeds

The [site generator](../src/buildsite.py) renders the complete catalog into HTML. JavaScript adds search and category filtering; entries remain available without JavaScript.

- `/`: website catalog and companion download pages
- `/skills/`: active skill entries, pinned sources, licenses, runtime and network requirements
- `/wsb/{ServiceId}.wsb`: one standalone launcher for each active site
- `/Catalog.xml` and `/SkillCatalog.xml`: preserved application feeds

The XML files remain data sources. XML comments do not become active website or skill entries. The static pages share navigation and the existing domain; no separate Sites hosting is involved.

## Skill discovery and reviewed contributions

The [skill page](https://catalog.yourtablecloth.app/skills/) links to upstream sources, pinned revisions, licenses, and the contribution guidance. Skill installation continues through the TableCloth application's marketplace. A web listing does not qualify a skill's execution path or install its runtimes, credentials, or MCP connections. The [skill lifecycle](SkillCatalog.README.md) and [eligibility criteria](SKILL_ELIGIBILITY.md) describe these boundaries.

1. Find the relevant site or skill and inspect its source and requirements.
2. Report a change through the linked GitHub issue form, or edit `docs/Catalog.xml` or `docs/SkillCatalog.xml` on a branch.
3. Include official source URLs, the verification date, actual test results, and unresolved items.
4. Run the applicable checks and submit a pull request for review under [CONTRIBUTING.md](../CONTRIBUTING.md).

## Standalone WSB launchers

Download a site's `.wsb` file and open the downloaded file on a Windows device with Windows Sandbox supported and enabled. This route does not require installing the TableCloth application. Microsoft documents the [Windows Sandbox requirements](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/) and [opening WSB files](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-configure-using-wsb-file).

The [launcher generator](../src/buildwsb.py) embeds its bootstrap and site data in that single file. Inside the guest, the bootstrap downloads and installs the site's catalog packages in sequence, registers listed Edge extension policies, runs guest bootstrap commands, then opens the site in Edge. Third-party extensions can require manual installation because Sandbox is not domain joined; the guest also provides an `extension-setup.html` page with installation links. Microsoft's [extension policy documentation](https://learn.microsoft.com/deployedge/microsoft-edge-policies/extensioninstallforcelist) explains that restriction. An installer without unattended arguments can display a wizard. Failed downloads, rejected payloads, failed installers, or timeouts stop the sequence and leave diagnostics at the guest desktop's `TableCloth-{ServiceId}/bootstrap.log`.

Networking is enabled for guest downloads and site access, including possible access to the local network. The launcher shares no host folders or certificates and disables clipboard, microphone, camera, printer, and GPU redirection. It does not reproduce TableCloth's IE mode, certificate sharing, ASTx adjustments, or architecture compatibility handling. Closing the guest discards its files, installed software, and state; the launcher provides no host export folder.

## Build order and GitHub Pages publication

Run the existing .NET builder first because it prepares the output directory. Then generate WSB files and HTML with Python's standard library. The [Pages workflow](../.github/workflows/publish.yml) validates the catalogs, runs the generators and regression checks, and publishes `outputs` to `gh-pages`.

Run these commands from the repository root with .NET 10 SDK and Python 3 available:

```sh
dotnet run --file src/catalogutil.cs -- ./docs/ ./outputs/
python src/buildwsb.py docs/Catalog.xml outputs/wsb
python src/buildsite.py docs outputs
python -m unittest discover -s tests -v
```

Generated pages and launchers belong in `outputs`; catalog changes belong in the source XML. Deployment runs on the relevant `main` changes defined in the workflow, keeping HTML, WSB files, and XML feeds in the same publication.

## Verification limits and qualification evidence

The [regression tests](../tests/) check generation contracts, escaping, links, and launcher data. Schema validation and successful generation do not establish that an installer succeeds, a security plugin supports Windows Sandbox, or a skill executes in the current host.

The integrated catalog publishes discoverable data and generated launchers. Long-term host qualification remains documented separately in the [skill eligibility record](SKILL_ELIGIBILITY.md); each catalog change can immediately affect the next publication.

For site or installer decisions, distinguish static generation checks from a recorded Windows Sandbox session. For skill decisions, distinguish pinned file installation from actual execution. Record the tested OS, architecture, site, and package versions when adding runtime evidence.
