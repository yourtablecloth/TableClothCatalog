# Remote skill catalog candidates

[한국어](SkillCatalog.README.ko.md)

## Candidate scope

As of October 25, 2026, [SkillCatalog.xml](SkillCatalog.xml) lists twelve community skill and MCP integration candidates. Every license review and installation review remains pending. Listing an entry does not enable installation or endorse its upstream service.

## Catalog location

The catalog is published at `https://catalog.yourtablecloth.app/SkillCatalog.xml` after the repository's normal Pages deployment. [publish.yml](../.github/workflows/publish.yml) copies the complete `docs` directory through the catalog builder. The application reads this XML without checking out the repository.

## Immutable upstream references

Each `Source` records the public GitHub repository, a relative directory and a full commit SHA. `Path="."` denotes the repository root for MCP candidates without a skill directory. A pinned Git commit does not pin packages subsequently downloaded by an upstream instruction. The NomaDamas candidates currently load instructions through an `npx` CLI; package pinning and runtime review remain open.

Directory segments can contain internal dots, such as `skills/example.v1`, but cannot end with a dot. The single root marker `.` remains valid. This prevents publishing paths that the marketplace client rejects.

## Independent review states

`Status` accepts `listed` or `unlisted`. `License.ReviewStatus` accepts `pending`, `verified` or `rejected`. `Install.ReviewStatus` accepts `pending`, `approved` or `rejected`. Installation requires `listed`, a verified license, an approved installation review, `Mode="upstream"`, `Runtime="Codex"`, an immutable source ref and a license source pinned to the same repository and commit. The installer preserves that file as `LICENSE.upstream`. Other entries remain visible for review. `Mode="adapter"` records integration work; it does not install or configure an MCP server.

## Upstream license evidence

The license identifiers come from the pinned source files: [NomaDamas MIT](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/LICENSE), [jjlabsio ISC](https://github.com/jjlabsio/korea-stock-mcp/blob/210595eec73a39cd04e2fb175f6e69b1516f0dd4/LICENSE), [kimlawtech Apache-2.0](https://github.com/kimlawtech/korean-jangbu-for/blob/592f6d7c7be138ae578be9ed6769239cc527ddea/LICENSE), [chrisryugj MIT](https://github.com/chrisryugj/korean-law-mcp/blob/4ccedb0dc89fce164493ffc9019b5ad35cb9b782/LICENSE), [mouseco MIT](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/LICENSE) and [tae0y MIT](https://github.com/tae0y/real-estate-mcp/blob/1119eb1069d1b1bae15f7b38c6c0268fe07995c0/LICENSE). The four mouseco candidates are identified as original contributions in the pinned [attribution notes](https://github.com/mouseco/k-gov-skills/blob/a55816b9b44e8391576204a095bf6337940d80f3/docs/attribution.md). Their API data and referenced tools retain separate terms. These observations do not complete redistribution, security or compatibility review.

## Validation and qualification

[SkillCatalog.xsd](SkillCatalog.xsd) validates the metadata, review values and unique IDs. Run `dotnet run --file src/checkskills.cs -- docs/SkillCatalog.xml docs/SkillCatalog.xsd` from the repository root. The Pages workflow runs the same check before publishing. Installation review also covers the complete file set and license notices, helper paths, package versions, compatibility with TableCloth's dedicated Codex environment, API keys and document access. Executing skills inside Windows Sandbox would require separate implementation and qualification. The catalog stores no credentials and runs no upstream code. `Network` describes skill execution; installing any remote skill still requires a download. The initial catalog intentionally offers no installable entries.

Run `dotnet run --file src/testskills.cs -- docs/SkillCatalog.xsd` to check path regressions against the actual schema. The [PR workflow](../.github/workflows/skill-catalog.yml) runs this check alongside catalog validation.
