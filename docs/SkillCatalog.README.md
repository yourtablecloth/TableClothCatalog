# Remote skill catalog lifecycle

[한국어](SkillCatalog.README.ko.md)

## Installable catalog entries

As of October 25, 2026, [SkillCatalog.xml](SkillCatalog.xml) contains ten active skill entries. Each active `Skill` element offers an upstream skill for installation. There are no listing or review states. The catalog also retains two removed MCP candidates as XML comments because their pinned repositories provide no `SKILL.md`.

## Adding and removing a skill

Add an active `Skill` element with a stable ID, an existing upstream skill directory, a full commit SHA and a license URL pinned to that same repository and commit. `Install.Mode` is `upstream`. To remove a skill from the remote catalog, comment out its entire `Skill` element and record a short reason outside or inside that comment. XML comments never become marketplace entries. A catalog removal leaves existing local installations available to manage and remove.

## Pinned versions and updates

Keep `Id`, `Source.Repository` and `Source.Path` unchanged when updating the same skill. Change `Source.Ref` to the new full commit SHA and change `License.Source` to the matching commit. The application compares the installed source commit with the current catalog commit to offer an update. A pinned Git commit does not pin packages downloaded later by upstream instructions. The NomaDamas skills currently load instructions through an `npx` CLI, so those packages have their own version behavior.

## Skill files and execution prerequisites

Installing a skill copies its files and preserves the pinned license as `LICENSE.upstream`. It does not install runtimes, register MCP servers or configure API keys. The [Korean law skill](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/skills/korean-law/SKILL.md) is installable as a skill file and requires a separately configured `korean-law` MCP server. The [jangbu entry](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/skills/korean-jangbu-for/SKILL.md) also depends on a separate MCP server and sibling skills. Each entry's `Notes` lists the relevant prerequisites. `Network` describes execution; remote installation always downloads files.

## License and source boundaries

The pinned licenses identify [NomaDamas MIT](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/LICENSE), [kimlawtech Apache-2.0](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/LICENSE), [chrisryugj MIT](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/LICENSE) and [mouseco MIT](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/LICENSE). The four mouseco entries have original-contribution records in the pinned [attribution notes](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/docs/attribution.md). API response data and referenced tools retain their own terms. The catalog stores no credentials and executes no upstream code.

## Validation and publication

[SkillCatalog.xsd](SkillCatalog.xsd) requires immutable references and license sources, limits the catalog to 256 active entries and rejects review attributes. [checkskills.cs](../src/checkskills.cs) also limits the XML to 512 KiB, checks source paths and requires each license URL to match the source repository and commit. Directory segments may contain internal dots but cannot end with a dot; the root marker `.` remains valid. The application separately bounds downloads, rejects symlinks, submodules and nested `SKILL.md` files, and verifies Git blob identities before installation.

Run `dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd` and `dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd` from the repository root. The [PR workflow](../.github/workflows/skill-catalog.yml) and [Pages workflow](../.github/workflows/publish.yml) run both checks. The existing catalog builder copies `docs` to the Pages output, publishing the XML at `https://catalog.yourtablecloth.app/SkillCatalog.xml` without requiring a client-side Git checkout.
