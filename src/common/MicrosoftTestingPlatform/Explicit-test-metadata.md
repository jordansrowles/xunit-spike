# Native result-kind metadata in Microsoft Testing Platform

This note describes the experimental metadata added by xUnit's Microsoft Testing Platform (MTP) integration.

## Native result kind

MTP currently represents both native xUnit `ITestNotRun` and `ITestSkipped` results with its skipped test-node state. xUnit adds a `TestMetadataProperty` named `xunit.result-kind` so consumers can distinguish the source event:

| Native xUnit event | `xunit.result-kind` | MTP state |
|---|---|---|
| `ITestNotRun` | `not-run` | Skipped |
| `ITestSkipped` | `skipped` | Skipped |

The property does not change test selection, native result messages, or MTP counts. A native skip keeps its original reason; a not-run keeps xUnit's generic explicit-filtering explanation unless the authored reason below applies. A skip whose text happens to match xUnit's explicit-filtering sentinel is still identified as `skipped`.

If test traits already contain `xunit.result-kind` (case-insensitively), xUnit preserves that trait and omits the generated result-kind property to avoid duplicate or overwritten metadata. Consumers should therefore treat the key as experimental and account for this collision case.
