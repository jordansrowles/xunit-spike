using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class FactExplicitReasonTraitGenerator() : TraitGenerator(Types.Xunit.FactAttribute)
{
	protected override IEnumerable<(string name, string value)> GetTraitValues(AttributeData attribute) =>
		ExplicitReasonTraitGeneratorHelper.GetTraitValues(attribute);
}

[Generator(LanguageNames.CSharp)]
public sealed class TheoryExplicitReasonTraitGenerator() : TraitGenerator(Types.Xunit.TheoryAttribute)
{
	protected override IEnumerable<(string name, string value)> GetTraitValues(AttributeData attribute) =>
		ExplicitReasonTraitGeneratorHelper.GetTraitValues(attribute);
}

static class ExplicitReasonTraitGeneratorHelper
{
	public static IEnumerable<(string name, string value)> GetTraitValues(AttributeData attribute)
	{
		string? reason = null;

		foreach (var argument in attribute.NamedArguments)
		{
			if (argument.Key == "ExplicitReason")
				reason = argument.Value.Value as string;
		}

		// A data row can opt into explicit execution even when its method is not explicit.
		// Register authored reasons independently of the method-level selection flag;
		// the runtime only uses the trait when the effective test row is explicit.
		if (!string.IsNullOrWhiteSpace(reason))
			yield return ("xunit.explicit-reason", reason);
	}
}
