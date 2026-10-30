using Xunit;
using Xunit.Generators;

public static class ExplicitReasonTraitGeneratorTests
{
	public class FactCases : CoreGeneratorTest<FactExplicitReasonTraitGenerator>
	{
		[Fact]
		public void RegistersExplicitReason()
		{
			var sources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Fact(Explicit = true, ExplicitReason = "Needs database")]
					public void RequiresDatabase() { }
				}
				""");

			var generated = Assert.Single(sources);
			Assert.Contains("RegisterCodeGenTestMethodTrait(", generated);
			Assert.Contains("\"xunit.explicit-reason\"", generated);
			Assert.Contains("\"Needs database\"", generated);
		}

		[Fact]
		public void RegistersReasonWhenMethodIsNotExplicit()
		{
			var sources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Fact(Explicit = false, ExplicitReason = "Row can opt in")]
					public void RequiresDatabase() { }
				}
				""");

			var generated = Assert.Single(sources);
			Assert.Contains("RegisterCodeGenTestMethodTrait(", generated);
			Assert.Contains("\"xunit.explicit-reason\"", generated);
			Assert.Contains("\"Row can opt in\"", generated);
		}

		[Fact]
		public void IgnoresMissingOrWhitespaceReason()
		{
			var noReasonSources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Fact(Explicit = true)]
					public void NoReason() { }
				}
				""");

			var whitespaceReasonSources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Fact(Explicit = true, ExplicitReason = " \t ")]
					public void WhitespaceReason() { }
				}
				""");

			Assert.Empty(noReasonSources);
			Assert.Empty(whitespaceReasonSources);
		}

		[Fact]
		public void EscapesSpecialCharactersAndPreservesUnicode()
		{
			var sources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Fact(ExplicitReason = "Needs \"database\"\n雪 \\path")]
					public void RequiresDatabase() { }
				}
				""");

			var generated = Assert.Single(sources);
			Assert.Contains("\"Needs \\\"database\\\"\\n\\u96EA \\\\path\"", generated);
		}
	}

	public class TheoryCases : CoreGeneratorTest<TheoryExplicitReasonTraitGenerator>
	{
		[Fact]
		public void RegistersTheoryReason()
		{
			var sources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Theory(Explicit = true, ExplicitReason = "Needs external service")]
					[InlineData(1)]
					public void RequiresService(int value) { }
				}
				""");

			var generated = Assert.Single(sources);
			Assert.Contains("RegisterCodeGenTestMethodTrait(", generated);
			Assert.Contains("\"xunit.explicit-reason\"", generated);
			Assert.Contains("\"Needs external service\"", generated);
		}

		[Fact]
		public void RegistersTheoryReasonWhenMethodIsNotExplicit()
		{
			var sources = GenerateSources(/* lang=c#-test */ """
				using Xunit;

				public class IntegrationTests
				{
					[Theory(Explicit = false, ExplicitReason = "Row opts in")]
					[InlineData(1, Explicit = true)]
					public void RequiresService(int value) { }
				}
				""");

			var generated = Assert.Single(sources);
			Assert.Contains("RegisterCodeGenTestMethodTrait(", generated);
			Assert.Contains("\"xunit.explicit-reason\"", generated);
			Assert.Contains("\"Row opts in\"", generated);
		}
	}
}
