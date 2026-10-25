# AppContainer skill evaluation results

[한국어](APP_CONTAINER_POC.ko.md) | [Eligibility criteria](SKILL_ELIGIBILITY.md) | [Catalog lifecycle](SkillCatalog.README.md)

On October 25, 2026, the AppContainer proof of concept passed 46 of 46 broker, execution and policy checks on Windows build 26300 x64. A separate pinned Codex 0.156.1 run completed a calculator call through MCP and AppContainer using two deterministic loopback Responses replies. No real model inference or authentication participated.

This report covers the pinned skill adaptations, contained execution, public-service samples, CLI routing and retained evidence. The public `main` catalog provides the three pinned script candidates for hands-on testing. These outcomes do not promote them to production compatibility.

The publication and source boundary come first, followed by worker controls, measured results, Codex routing, evidence records and remaining qualification work.

## Public evaluation catalog and pinned source boundary

The public `main` catalog contains six active entries for hands-on testing: the three existing production candidates and three pinned script candidates for isolated evaluation. Nine other blocks remain commented out. Publishing the script candidates does not qualify them for production compatibility. The corresponding TableCloth implementation uses `poc/appcontainer-skill-runner` in [application PR #322](https://github.com/yourtablecloth/TableCloth/pull/322), which has not been integrated. The current product chat retains `shell_tool=false` and `unified_exec=false`; it does not automatically register this broker or execute installed scripts. The [catalog criteria](SKILL_ELIGIBILITY.md) and credential deferral in [TableCloth #321](https://github.com/yourtablecloth/TableCloth/issues/321) continue to apply.

| Evaluated script | Pinned upstream commit | Execution adaptation |
| --- | --- | --- |
| [BOK ECOS](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | Direct call to the original `scripts/bok_ecos.py` |
| [KOSIS](https://github.com/NomaDamas/k-skill/tree/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats) | `0102e5133865f00886e37ac8cf39a49e2c7a3d39` | Direct call to the original `scripts/run_kosis_stats.py` |
| [Everyday calculators](https://github.com/yourtablecloth/tablecloth-calculators/tree/877b58cf0741db21342445f2b3fcb54ad1ada85c) | `877b58cf0741db21342445f2b3fcb54ad1ada85c` | Original `scripts/calculate.py`, JSON input on stdin |

The BOK and KOSIS `SKILL.md` files require an `npx` instruction loader. This experiment read the pinned instructions and called their Python helpers directly without modifying those helpers. It did not run or qualify the original npm loader workflow. Node.js coverage used a fixed local fixture, not an upstream npm package. The copies retain their upstream licenses and 31 matching pinned Git blobs in the implementation's `tools/appcontainer-skill-poc/skills/ORIGINS.json`.

## Host broker and contained worker controls

The host-owned MCP broker accepts registered IDs and bounded JSON inputs, not model-provided executable paths, shell commands or source code. It rechecks interpreter and script hashes before execution. Python uses `-I -S -B -X utf8`; the Node fixture has a fixed entry file. The native runner verifies an AppContainer token, uses owned-directory ACL grants and a Job Object, and bounds time, memory, process count and output. Microsoft's [AppContainer isolation documentation](https://learn.microsoft.com/en-us/windows/win32/secauthz/appcontainer-isolation) describes the underlying resource-access model.

The normal broker grants no network capability and rejects network-required registrations. Its tested workers received owned temporary home and work directories with a cleared environment. Private synthetic-file denial, inherited-value exclusion, child-process limits, UTF-8 round trips, cancellation, timeout, memory/output limits, sampled ACL restoration and per-run workspace removal passed. Ordinary AppContainer shares the host kernel, and host resources whose ACLs already permit application packages can remain accessible. These tests do not establish a VM boundary or universal denial of host-file reads.

The separate public-API evaluator granted broad `internetClient` capability to fixed host-owned commands. It had no domain allowlist. Its successful service reads do not establish a network policy suitable for model-selected arbitrary URLs. Neither the worker test nor catalog activation grants general shell execution, npm installation or arbitrary package updates.

## Measured execution and service results

The final evaluation record separates top-level checks from the unit-test counts executed inside two of those checks.

| Verification | Observed result | Scope |
| --- | --- | --- |
| Offline broker checks | 20/20 | Input, registration, hash, transport and failure handling |
| Execution and policy checks | 26/26 | Contained Python/Node, control cases, limits, cleanup and two public-service cases |
| Combined evaluation | 46/46 | The preceding 20 and 26 checks |
| Calculator upstream unit tests | 28/28 | AppContainer run; included in one of the 26 checks |
| BOK upstream unit tests | 19/19 | AppContainer run; included in one of the 26 checks |
| ManagedAi Release regression suite | 197/197 | Separate application regression run |

The calculator cases covered discounts, percentages, leap-day adjustment, fixed-offset date rollover, numeric BMI and a zero-rate loan. KOSIS's fixed dry run and the Node JSON fixture also passed. These samples do not cover every calculator operation or the original KOSIS unit suite.

For the live samples, the [BOK helper](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/bok-ecos-stats/scripts/bok_ecos.py) returned one row from ECOS using its public `sample` key. The [KOSIS helper](https://github.com/NomaDamas/k-skill/blob/0102e5133865f00886e37ac8cf39a49e2c7a3d39/kosis-stats/scripts/run_kosis_stats.py) returned one search row for `인구` through the shared `k-skill-proxy.nomadamas.org` service. No user API key was supplied. Proxy availability and server-side credentials remain external dependencies. Direct KOSIS and big-data credential paths were not tested.

## Actual Codex routing with deterministic local replies

The pinned CLI advertised the calculator under namespace `mcp__tablecloth_skill_poc` and normalized function name `tablecloth_calculators`. A local HTTP listener sent one fixed function call and then a final reply after receiving the actual tool output. The CLI completed its MCP event using original tool name `tablecloth-calculators`; the worker reported `wasAppContainer=true`, exit code 0 and discount result `80.00`.

The isolated profile kept `approval_policy="never"`, `shell_tool=false` and `unified_exec=false`. Only the host-owned registered calculator received an explicit per-tool `approval_mode="approve"`; the server default remained `prompt` and its enabled-tool list contained that one calculator. No false read-only annotation was used. OpenAI documents these controls in the [MCP configuration guide](https://developers.openai.com/codex/mcp), and the [function-calling guide](https://developers.openai.com/api/docs/guides/function-calling#defining-namespaces) describes namespace grouping.

This run made two loopback Responses requests, zero real model inferences and zero authentication attempts. It copied no existing authentication and created no auth file in its new Codex home. Configuration, manifest and calculator hashes remained unchanged. It verifies actual CLI-to-broker routing and contained execution with predetermined replies. Real model tool selection and automatic execution in production TableCloth chat remain unverified.

## Evidence records and platform limits

The following records remain under the local TableCloth workspace's `Artifacts/appcontainer-skill-poc/` directory. They are not published as catalog assets. The source and reproduction guide reside in `tools/appcontainer-skill-poc/` of the TableCloth implementation branch.

| Record | SHA-256 | Recorded scope |
| --- | --- | --- |
| `evaluation-public-api.json` | `5489b04ad12cbce500cfab5e41b904d113a00206e5fccd9829bce835fa48fdfa` | Final 46-check run, including the two live service samples |
| `codex-probe-evidence.json` | `4c283d01b2d355407c0af708c8b389b1b9d44b024f82b8e3590aa8c57f06daeb` | Successful pinned CLI routing with deterministic local replies |

The measured host reports Windows NT `10.0.26300.0`, x64. It staged Python 3.14.9 and Node.js 24.22.1. Windows 10 and ARM64 were not measured; native API availability does not establish compatibility there. The earlier [eight-skill installation audit](skill-eligibility-audit-2026-10-25.json) remains unchanged and retains its original installation/discovery scope and ACL limitation. Its prior 519 fixtures are not recounted as new AppContainer execution tests.

## Current decision and remaining qualification

The PoC establishes bounded script execution and deterministic CLI/MCP routing on the measured host. The [eligibility criteria](SKILL_ELIGIBILITY.md) continue to separate that evidence from production integration, live model behavior and broader platform qualification. The three pinned script candidates published in the public catalog retain evaluation-only scope; MCP-dependent and credential-dependent deferred candidates remain inactive.

Production use still depends on reviewed application integration, network policy, cancellation and cleanup behavior, platform qualification and a separately authorized real-model test. The immediate result supports further evaluation of the three exact script pins. It does not activate a general shell or convert runtime installation into script support in the shipped chat.
