# Skill eligibility and pinned-source verification

[한국어](SKILL_ELIGIBILITY.ko.md) | [Catalog lifecycle](SkillCatalog.README.md) | [Contributing](../CONTRIBUTING.md)

As of October 25, 2026, the current TableCloth chat keeps `shell_tool=false` and `unified_exec=false`. It does not support executing installed-skill Python or Node scripts or automatically configuring an MCP server. This review covers the eight active entries in catalog commit `12b1622da1b69ec1d24c0729bbebd70cbb19a902`. Three remain eligible candidates; five retain their complete XML blocks as comments.

This record covers eligibility criteria, pinned-source decisions, installation/discovery results and application regression tests. It distinguishes successful package handling from a supported feature execution path.

The current-host criteria come first, followed by the eight pinned sources and their evidence. Entry changes and catalog checks follow; live service and model-driven execution remain outside this verification.

## Eligibility depends on the required execution path

The existing [source and installation requirements](SkillCatalog.README.md) continue to apply: a public GitHub repository, a fixed 40-character commit, a valid `SKILL.md`, and preservation of the license from that same commit. The execution criteria below add to those requirements.

An active entry supplies its core function through instructions over conversation-provided material or an existing TableCloth host bridge. The fixed source, license and required dependencies are reviewed together. A readable `SKILL.md`, a successful download or a plausible model response does not prove that the function ran. Installing a Python or Node runtime does not register an execution tool in the current AI conversation.

Python, Node, shell and `npx` execution required by a skill's workflow remain incompatible with the current chat. MCP-dependent instructions also remain incompatible when the necessary server and tools are not connected, even if their installed folder contains no script. Host-bridge skills interpret the host's bounded report; they do not acquire general shell access. Missing reports and unsupported local Windows features remain explicit limitations.

The credential policy is an additional requirement. New environment-delivered API keys, tokens and client secrets remain deferred until [TableCloth #321](https://github.com/yourtablecloth/TableCloth/issues/321) is implemented and verified. A keyless path does not waive execution prerequisites. The [contribution policy](../CONTRIBUTING.md) excludes system/user environment variables, `.env` files and chat prompts as credential workarounds.

## Review of all eight previously active pinned skills

Each row records the fixed source and the result of reviewing its required workflow. The [installation audit](skill-eligibility-audit-2026-10-25.json) also records successful installation, provenance/license preservation, native discovery, enable/disable and removal for all eight. “Eligible candidate” does not claim a successful live service, certificate scan, Sandbox operation or model-driven execution. Source links use the exact catalog commit, rather than a moving branch.

| Skill | Pinned source commit | Required execution path | Current decision |
| --- | --- | --- | --- |
| [official-report-skillset](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/skills/official-report-skillset/SKILL.md) | `a55816b9b44e8391576204a095bf6337940d80f3` | Instructions and two local reference documents; draft from supplied facts | Eligible candidate, active |
| [tablecloth-certificate-expiry](https://github.com/yourtablecloth/tablecloth-certificate-expiry/blob/9d2eca6431d1cba34bc872b80fc55fa71fa2dcb2/SKILL.md) | `9d2eca6431d1cba34bc872b80fc55fa71fa2dcb2` | Existing host bridge supplies consented expiry counts; Codex interprets the report | Eligible candidate, active |
| [tablecloth-windows-sandbox](https://github.com/yourtablecloth/tablecloth-windows-sandbox/blob/b9e52bcd303624dbba81e7de66a3f56344e922fe/SKILL.md) | `b9e52bcd303624dbba81e7de66a3f56344e922fe` | Existing host bridge supplies an allowlisted operation result; compatible Windows support applies | Eligible candidate, active |
| [bok-ecos-stats](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats/SKILL.md) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | Mandatory `npx` instruction loader and Python helper execution | Incompatible, commented out |
| [kosis-stats](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats/SKILL.md) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | Mandatory `npx` instruction loader and Python helper execution | Incompatible, commented out |
| [tablecloth-calculators](https://github.com/yourtablecloth/tablecloth-calculators/blob/877b58cf0741db21342445f2b3fcb54ad1ada85c/SKILL.md) | `877b58cf0741db21342445f2b3fcb54ad1ada85c` | Execute packaged `scripts/calculate.py`; no guessed-result fallback | Incompatible, commented out |
| [korean-jangbu-for](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/skills/korean-jangbu-for/SKILL.md) | `592f6d7c7be138ae578be9ed6769239cc527ddea` | Separate Python MCP server and six sibling skills; installed entry contains only `SKILL.md` | Incompatible, commented out |
| [korean-law-mcp](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/skills/korean-law/SKILL.md) | `4ccedb0dc89fce164493ffc9019b5ad35cb9b782` | Separately connected `korean-law` MCP search and citation-verification tools | Incompatible, commented out |

The report-writing skill has no script or MCP dependency in its pinned [reference directory](https://github.com/mouseco/k-gov-skills/tree/a55816b9b44e8391576204a095bf6337940d80f3/skills/official-report-skillset/references). Its scope covers conversation drafts, not automatic local-document reading or HWP/Word export. Jangbu's [Python project](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/mcp-server/pyproject.toml) and sibling skills are outside its installed source directory. Its LLM-assisted classification still depends on MCP-provided masked transactions and persistence tools.

## Evidence levels and unverified behavior

Keep these outcomes separate in qualification records:

1. Static source and structure inspection: pinned paths, manifest, license and required workflow.
2. Installation and discovery: production installer, preserved provenance and native Codex skill discovery in an isolated profile.
3. Application behavior: report handling and host-bridge fixtures with the existing execution policy.
4. Live feature execution: the actual required tool call and its result in the intended AI conversation.

The [audit JSON](skill-eligibility-audit-2026-10-25.json) reaches level 2 for all eight pinned entries: installation, native Codex 0.156.1 `skills/list` discovery, enable/disable and removal succeeded. It records one native app-server process, no model or skill-script calls, no live service calls, no certificate scans, no Sandbox state changes, no runtime mutations and removal of its unique temporary store. Successful discovery includes MCP-dependent manifests and does not establish that their MCP tools are connected.

The production Windows profile's ACL setter rejected the audit token. The audit instead stored the exact `WindowsManagedAiProfile.Configuration` in an artifact-owned profile with both execution features false. This verifies the listed installation/discovery behavior in that configuration; it is not a successful profile ACL or production-security qualification.

At level 3, the local Release suites passed 197/197 ManagedAi tests and 322/322 application tests, 519 total. The application suite includes host-bridge fixtures, chat, two-tab UI and installed-skill management. The retained TRX records are `managed-ai-removal.trx` and `app-runtime-removal.trx`. Fixture success does not establish real certificate access, live Sandbox operations or model-driven script execution. Spork was unchanged and was not rerun for this change.

Level 4 remains unverified: the audit made no real service API requests, certificate scans, Sandbox operations or model calls. Unit tests or a direct `codex sandbox` subprocess do not establish model tool registration or automatic script execution in TableCloth chat. Changing execution settings is outside this catalog review.

## Adding, updating and deferring entries

Document the fixed source, core function, required execution dependencies, available host mechanism, tested scope and unverified scope before activating an entry. Recheck these facts when `Source.Ref` changes. A source update can introduce an executable dependency even when the manifest still exists. New active IDs and revisions also update the qualification table in [testskills.cs](../src/testskills.cs) and both language versions of this record.

If the current host cannot perform a required step, preserve the entire XML block as a comment with its reason. Do not describe optional installation of runtimes or missing tools as completed integration. Reconsider an entry after the necessary application mechanism is implemented and independently verified. Existing local installations remain manageable after catalog deferral, as described in the [lifecycle guide](SkillCatalog.README.md).

## Catalog checks and their limits

The schema and [checkskills.cs](../src/checkskills.cs) check source and license coordinates. [testskills.cs](../src/testskills.cs) additionally guards the current qualified pinned sources, rejects reactivation of unsupported script/MCP candidates, and checks raw-block commenting. It allows removal of qualified entries. No execution-type field or review state is added to the XML.

Run the catalog-only checks from the repository root:

```text
dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd
dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd
dotnet run --file src/catalogutil.cs -- docs/ outputs/
```

Successful checks establish catalog structure, policy consistency and generated output. They do not execute upstream skills or establish application runtime support. The [PR workflow](../.github/workflows/skill-catalog.yml) and [publication workflow](../.github/workflows/publish.yml) run the catalog checks; runtime qualification remains a separate activity.

## Current decisions and later requalification

The catalog keeps instruction-only drafting and existing host-report interpretation as active candidates. The [eight-source audit](skill-eligibility-audit-2026-10-25.json) establishes package installation, discovery and management separately from the dependency-based eligibility decisions. Five incompatible entries remain commented out with their original fixed coordinates.

Users can continue managing existing local installations. Future execution or MCP support requires application work and verification before a candidate is reactivated. Contributors can use the pinned-source table and evidence levels to distinguish a package update from a newly supported execution path.
