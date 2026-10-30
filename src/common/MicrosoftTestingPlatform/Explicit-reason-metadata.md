# Authored explicit reasons in Microsoft Testing Platform

`FactAttribute` and `TheoryAttribute` accept an optional `ExplicitReason`. It is descriptive only: it does not set `Explicit`, change selection or execution, or replace a native skip or failure. Null, empty and whitespace-only values are treated as absent.

```csharp
[Theory(Explicit = false, ExplicitReason = "Requires release infrastructure")]
[InlineData(1, Explicit = true)]
public void PublishCheck(int value) { }
```

When row `1` is excluded as explicit, MTP receives a skipped-state node with `xunit.result-kind = not-run` and explanation `Not run (due to explicit test filtering): Requires release infrastructure`. When a different row is excluded because its effective `Explicit` value is false, the generic explanation is retained.

For the MTP integration, a non-whitespace reason is carried as the method trait `xunit.explicit-reason`. The reason is included in an MTP not-run explanation only when the test's effective explicit flag is true, the native result is `ITestNotRun`, and exactly one non-whitespace reason value is present. Otherwise xUnit retains the generic explanation. This allows a theory row with `Explicit=true` to use a method-authored reason even when the method's `Explicit` value is false; rows whose effective value is false do not receive the reason in their not-run explanation.

The trait is ordinary public metadata, not authenticated provenance. A user-supplied trait with the same key can be indistinguishable from an authored `ExplicitReason`; missing, whitespace-only or multiple values use the generic explanation. Reflection supports custom attributes derived from xUnit's `FactAttribute`/`TheoryAttribute` model. Arbitrary independent `IFactAttribute` implementations do not have an `ExplicitReason` contract. No separate guarantee is made that this metadata appears in console, TRX or XML reports.
