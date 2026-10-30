using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.TestHost;
using Xunit;
using Xunit.MicrosoftTestingPlatform;
using Xunit.Sdk;

public static class TestPlatformExecutionMessageSinkTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public static void DelegatesMessages(bool returnValue)
	{
		var message = TestData.DiagnosticMessage();
		var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
		classUnderTest.InnerSink.Callback = _ => returnValue;

		var result = classUnderTest.OnMessage(message);

		Assert.Equal(returnValue, result);
		var received = Assert.Single(classUnderTest.InnerSink.Messages);
		Assert.Same(message, received);
	}

	public static class Attachments
	{
		[Fact]
		public static void WritesAttachmentsToResultsDirectory()
		{
			var resultsPath = Path.Combine(Path.GetTempPath(), "attachment-test-" + Guid.NewGuid().ToString("n"));
			Directory.CreateDirectory(resultsPath);

			try
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(resultsPath: resultsPath);
				var attachments = new Dictionary<string, TestAttachment>
				{
					["foo"] = TestAttachment.Create("bar"),
					["baz"] = TestAttachment.Create([1, 2, 3], "application/octet"),
				};
				var testStarting = TestData.TestStarting(testUniqueID: "def456");
				var testPassed = TestData.TestPassed(testUniqueID: "def456");
				var testFinished = TestData.TestFinished(testUniqueID: "def456", attachments: attachments);

				classUnderTest.OnMessage(testStarting);
				classUnderTest.OnMessage(testPassed);
				classUnderTest.OnMessage(testFinished);

				var basePath = Path.Combine(resultsPath, "def456");
				Assert.Equal("bar", File.ReadAllText(Path.Combine(basePath, "foo.txt")));
				Assert.Equal([1, 2, 3], File.ReadAllBytes(Path.Combine(basePath, "baz.bin")));
			}
			finally
			{
				// Best effort to clean up the temp folder behind us
				try
				{
					Directory.Delete(resultsPath, recursive: true);
				}
				catch { }
			}
		}
	}

	public static class MessageMapping
	{
		public static class ITestAssemblyCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestAssemblyCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Assembly Cleanup Failure (./test-assembly.dll)]: ", "test-case-id");
			}

			[Fact]
			public static void WithoutTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest, includeTestStarting: false);

				classUnderTest.OnMessage(TestData.TestAssemblyCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Assembly Cleanup Failure (./test-assembly.dll)]: ");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestAssemblyCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Assembly Cleanup Failure (<unknown test assembly>)]: ");
			}
		}

		public static class ITestCaseCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestCaseCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Case Cleanup Failure]: ", "test-case-id", "test-display-name");
			}

			[Fact]
			public static void WithoutTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest, includeTestStarting: false);

				classUnderTest.OnMessage(TestData.TestCaseCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Case Cleanup Failure]: ", "test-case-id", "test-case-display-name");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestCaseCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Case Cleanup Failure]: ", "test-case-id", verifyMetadata: false);
			}
		}

		public static class ITestClassCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestClassCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Class Cleanup Failure (test-class-name)]: ", "test-case-id");
			}

			[Fact]
			public static void WithoutTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest, includeTestStarting: false);

				classUnderTest.OnMessage(TestData.TestClassCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Class Cleanup Failure (test-class-name)]: ");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestClassCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Class Cleanup Failure (<unknown test class>)]: ");
			}
		}

		public static class ITestCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Cleanup Failure]: ", "test-case-id");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Cleanup Failure]: ", "test-case-id", verifyMetadata: false);
			}
		}

		public static class ITestCollectionCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestCollectionCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Collection Cleanup Failure (test-collection-display-name)]: ", "test-case-id");
			}

			[Fact]
			public static void WithoutTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest, includeTestStarting: false);

				classUnderTest.OnMessage(TestData.TestCollectionCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Collection Cleanup Failure (test-collection-display-name)]: ");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestCollectionCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Collection Cleanup Failure (<unknown test collection>)]: ");
			}
		}

		public static class ITestFailed
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public static void Assertion(bool trxEnabled)
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(trxEnabled: trxEnabled);
				SendStartingMessages(classUnderTest, traits: trxEnabled ? TestData.DefaultTraitsWithCategory : TestData.DefaultTraits);

				SendTestFailed(classUnderTest, FailureCause.Assertion);

				var testNode = AssertStandardMetadata(classUnderTest, expectTrx: trxEnabled, failure: true);
				var failed = testNode.Properties.Single<FailedTestNodeStateProperty>();

				Assert.Null(failed.Explanation);
				Assert.NotNull(failed.Exception);
				Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", failed.Exception.Message);
				Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", failed.Exception.StackTrace);
			}

			[Fact]
			public static void Timeout()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				SendTestFailed(classUnderTest, FailureCause.Timeout);

				var testNode = AssertStandardMetadata(classUnderTest);
				var timeout = testNode.Properties.Single<TimeoutTestNodeStateProperty>();

				Assert.Null(timeout.Explanation);
				Assert.NotNull(timeout.Exception);
				Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", timeout.Exception.Message);
				Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", timeout.Exception.StackTrace);
			}

			[Theory]
			[InlineData(FailureCause.Exception)]
			[InlineData(FailureCause.Other)]
			[InlineData(FailureCause.Unknown)]
			public static void Error(FailureCause failureCause)
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				SendTestFailed(classUnderTest, failureCause);

				var testNode = AssertStandardMetadata(classUnderTest);
				var error = testNode.Properties.Single<ErrorTestNodeStateProperty>();

				Assert.Null(error.Explanation);
				Assert.NotNull(error.Exception);
				Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", error.Exception.Message);
				Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", error.Exception.StackTrace);
			}
		}

		public static class ITestMethodCleanupFailure
		{
			[Fact]
			public static void WithTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestMethodCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Method Cleanup Failure (test-class-name.test-method)]: ", "test-case-id");
			}

			[Fact]
			public static void WithoutTests()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest, includeTestStarting: false);

				classUnderTest.OnMessage(TestData.TestMethodCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Method Cleanup Failure (test-class-name.test-method)]: ");
			}

			[Fact]
			public static void NoMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();

				classUnderTest.OnMessage(TestData.TestMethodCleanupFailure());

				AssertCleanupFailure(classUnderTest, "[Test Method Cleanup Failure (<unknown test method>)]: ");
			}
		}

		public static class ITestNotRun
		{
			[Fact]
			public static void ServerModeTrue()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(serverMode: true);
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestNotRun());
				classUnderTest.OnMessage(TestData.TestFinished());

				Assert.Empty(classUnderTest.TestNodeMessageBus.PublishedData);
			}

			[Fact]
			public static void RepeatedNotRunUpdatesProduceOneFinalResultKind()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestNotRun());
				classUnderTest.OnMessage(TestData.TestNotRun());
				classUnderTest.OnMessage(TestData.TestFinished());

				var testNode = AssertStandardMetadata(classUnderTest, expectedResultKind: "not-run");
				Assert.Single(testNode.Properties.OfType<TestMetadataProperty>(), metadata => metadata.Key == "xunit.result-kind");
			}

			[Fact]
			public static void WithoutTestStartingMetadata()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				classUnderTest.OnMessage(TestData.TestNotRun());
				classUnderTest.OnMessage(TestData.TestFinished());

				var update = Assert.IsType<TestNodeUpdateMessage>(Assert.Single(classUnderTest.TestNodeMessageBus.PublishedData));
				Assert.Equal(TestData.DefaultTestCaseUniqueID, update.TestNode.Uid.Value);
				Assert.Equal("<unknown test display name>", update.TestNode.DisplayName);
				Assert.Equal("not-run", Assert.Single(update.TestNode.Properties.OfType<TestMetadataProperty>(), metadata => metadata.Key == "xunit.result-kind").Value);
				Assert.Equal("Not run (due to explicit test filtering)", update.TestNode.Properties.Single<SkippedTestNodeStateProperty>().Explanation);
				Assert.Empty(update.TestNode.Properties.OfType<TestMethodIdentifierProperty>());
				Assert.Empty(update.TestNode.Properties.OfType<TestFileLocationProperty>());
				Assert.Empty(update.TestNode.Properties.OfType<TimingProperty>());
			}

			[Fact]
			public static void CancellationWithoutResultLeavesOnlyInProgressUpdate()
			{
				// A cancelled xUnit test emits ITestStarting and ITestFinished without a result message.
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
				classUnderTest.OnMessage(TestData.TestStarting());
				classUnderTest.OnMessage(TestData.TestFinished());

				var update = Assert.IsType<TestNodeUpdateMessage>(Assert.Single(classUnderTest.TestNodeMessageBus.PublishedData));
				Assert.Single(update.TestNode.Properties.OfType<InProgressTestNodeStateProperty>());
				Assert.DoesNotContain(update.TestNode.Properties.OfType<TestMetadataProperty>(), metadata => metadata.Key == "xunit.result-kind");
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public static void ServerModeFalse(bool trxEnabled)
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(serverMode: false, trxEnabled: trxEnabled);
				SendStartingMessages(classUnderTest, traits: trxEnabled ? TestData.DefaultTraitsWithCategory : TestData.DefaultTraits);

				classUnderTest.OnMessage(TestData.TestNotRun());
				classUnderTest.OnMessage(TestData.TestFinished());

				var testNode = AssertStandardMetadata(classUnderTest, expectTrx: trxEnabled, expectedResultKind: "not-run");
				var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
				Assert.Equal("Not run (due to explicit test filtering)", skipped.Explanation);
			}
		}

		[Fact]
		public static void AuthoredExplicitExclusionReasonIsAddedToNotRunExplanation()
		{
			const string reason = "Requires a live database";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest,
				expectedResultKind: "not-run"
			);
			AssertExplicitReasonMetadata(testNode, reason);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal("Not run (due to explicit test filtering): Requires a live database", skipped.Explanation);
		}

		[Fact]
		public static void AuthoredReasonDoesNotApplyToNonExplicitTheoryRow()
		{
			const string reason = "Requires a live database";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: false);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest,
				expectedResultKind: "not-run"
			);
			AssertExplicitReasonMetadata(testNode, reason);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal("Not run (due to explicit test filtering)", skipped.Explanation);
		}

		[Fact]
		public static void AuthoredReasonDoesNotApplyWhenTraitHasMultipleValues()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = ["Reason one", "Reason two"];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest,
				expectedResultKind: "not-run"
			);
			AssertExplicitReasonMetadata(testNode, "Reason one", "Reason two");
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal("Not run (due to explicit test filtering)", skipped.Explanation);
		}

		[Fact]
		public static void SingleTraitValueCanImpersonateAnAuthoredReason()
		{
			const string spoofedReason = "Supplied by a user trait";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [spoofedReason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest, expectedResultKind: "not-run");
			AssertExplicitReasonMetadata(testNode, spoofedReason);
			Assert.Equal(
				$"Not run (due to explicit test filtering): {spoofedReason}",
				testNode.Properties.Single<SkippedTestNodeStateProperty>().Explanation
			);
		}

		[Fact]
		public static void AuthoredWhitespaceReasonFallsBackToGenericExplanation()
		{
			const string reason = " \t ";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest,
				expectedResultKind: "not-run"
			);
			AssertExplicitReasonMetadata(testNode, reason);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal("Not run (due to explicit test filtering)", skipped.Explanation);
		}

		[Fact]
		public static void AuthoredReasonDoesNotReplaceNativeSkipReason()
		{
			const string reason = "Requires a live database";
			const string skipReason = "Skipped by this data row";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestSkipped(reason: skipReason));
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest,
				expectedResultKind: "skipped"
			);
			AssertExplicitReasonMetadata(testNode, reason);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal(skipReason, skipped.Explanation);
		}

		[Fact]
		public static void AuthoredReasonDoesNotAffectPass()
		{
			const string reason = "Requires a live database";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestPassed());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest
			);
			AssertExplicitReasonMetadata(testNode, reason);
			var passed = testNode.Properties.Single<PassedTestNodeStateProperty>();
			Assert.Null(passed.Explanation);
		}

		[Fact]
		public static void AuthoredReasonDoesNotAffectSelectedFailure()
		{
			const string reason = "Requires a live database";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(pair => pair.Key, pair => pair.Value);
			traits["xunit.explicit-reason"] = [reason];
			SendStartingMessages(classUnderTest, traits: traits, explicitTest: true);

			classUnderTest.OnMessage(TestData.TestFailed());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(
				classUnderTest
			);
			AssertExplicitReasonMetadata(testNode, reason);
			Assert.Single(testNode.Properties.OfType<FailedTestNodeStateProperty>());
			Assert.Empty(testNode.Properties.OfType<SkippedTestNodeStateProperty>());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static void ITestPassed(bool trxEnabled)
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(trxEnabled: trxEnabled);
			SendStartingMessages(classUnderTest, traits: trxEnabled ? TestData.DefaultTraitsWithCategory : TestData.DefaultTraits);

			classUnderTest.OnMessage(TestData.TestPassed());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest, expectTrx: trxEnabled);
			var passed = testNode.Properties.Single<PassedTestNodeStateProperty>();
			Assert.Null(passed.Explanation);
		}

		[Fact]
		public static void ITestSkipped()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestSkipped());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest, expectedResultKind: "skipped");
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal(TestData.DefaultSkipReason, skipped.Explanation);
		}

		[Fact]
		public static void NativeSkipWithExplicitFilterTextRetainsNativeResultKind()
		{
			const string matchingExplanation = "Not run (due to explicit test filtering)";
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(serverMode: false);
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestSkipped(reason: matchingExplanation));
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest, expectedResultKind: "skipped");
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal(matchingExplanation, skipped.Explanation);
		}

		[Fact]
		public static void ResultKindDoesNotDuplicateCaseInsensitiveTraitKey()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			var traits = TestData.DefaultTraits.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
			traits["XUNIT.RESULT-KIND"] = ["consumer metadata"];
			SendStartingMessages(classUnderTest, traits: traits);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = GetTestNode(classUnderTest);
			var resultKindProperties = testNode.Properties.OfType<TestMetadataProperty>()
				.Where(metadata => string.Equals(metadata.Key, "xunit.result-kind", StringComparison.OrdinalIgnoreCase))
				.ToArray();
			Assert.Equal("XUNIT.RESULT-KIND", Assert.Single(resultKindProperties).Key);
			Assert.Equal("consumer metadata", resultKindProperties[0].Value);
			Assert.Equal("Not run (due to explicit test filtering)", testNode.Properties.Single<SkippedTestNodeStateProperty>().Explanation);
		}

		[Fact]
		public static void MtpConsumerCanDistinguishNotRunFromNativeSkipWithMatchingText()
		{
			const string matchingExplanation = "Not run (due to explicit test filtering)";
			var notRunConsumer = new TestNodeUpdateConsumer();
			var notRunSink = TestableTestPlatformExecutionMessageSink.Create(
				testNodeMessageBus: new SpyTestPlatformMessageBus(consumer: notRunConsumer)
			);
			SendStartingMessages(notRunSink);
			notRunSink.OnMessage(TestData.TestNotRun());
			notRunSink.OnMessage(TestData.TestFinished());

			var notRunUpdates = notRunConsumer.ConsumedUpdates
				.Where(update => update.TestNode.Properties.OfType<SkippedTestNodeStateProperty>().Any())
				.ToArray();
			Assert.Single(notRunUpdates);
			foreach (var update in notRunUpdates)
			{
				Assert.Equal("not-run", Assert.Single(update.TestNode.Properties.OfType<TestMetadataProperty>(), metadata => metadata.Key == "xunit.result-kind").Value);
				Assert.Equal(matchingExplanation, update.TestNode.Properties.Single<SkippedTestNodeStateProperty>().Explanation);
			}

			var skippedConsumer = new TestNodeUpdateConsumer();
			var skippedSink = TestableTestPlatformExecutionMessageSink.Create(
				testNodeMessageBus: new SpyTestPlatformMessageBus(consumer: skippedConsumer)
			);
			SendStartingMessages(skippedSink);
			skippedSink.OnMessage(TestData.TestSkipped(reason: matchingExplanation));
			skippedSink.OnMessage(TestData.TestFinished());

			var skippedUpdates = skippedConsumer.ConsumedUpdates
				.Where(update => update.TestNode.Properties.OfType<SkippedTestNodeStateProperty>().Any())
				.ToArray();
			Assert.Single(skippedUpdates);
			foreach (var update in skippedUpdates)
			{
				Assert.Equal("skipped", Assert.Single(update.TestNode.Properties.OfType<TestMetadataProperty>(), metadata => metadata.Key == "xunit.result-kind").Value);
				Assert.Equal(matchingExplanation, update.TestNode.Properties.Single<SkippedTestNodeStateProperty>().Explanation);
			}
		}

		[Fact]
		public static void ITestStarting()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest, includeTestStarting: false);

			classUnderTest.OnMessage(TestData.TestStarting());

			var testNode = AssertStandardMetadata(classUnderTest, expectTiming: false);
			var inProgress = testNode.Properties.Single<InProgressTestNodeStateProperty>();
			Assert.Null(inProgress.Explanation);
		}

		public static class ITestOutput
		{
			[Fact]
			public static void Off()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(showLiveOutput: false);

				classUnderTest.OnMessage(TestData.TestOutput());

				Assert.Empty(classUnderTest.OutputDevice.DisplayedData);
			}

			[Fact]
			public static void On()
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(showLiveOutput: true);
				SendStartingMessages(classUnderTest);

				classUnderTest.OnMessage(TestData.TestOutput());

				var data = Assert.Single(classUnderTest.OutputDevice.DisplayedData);
				AssertColorOutput(data, $"OUTPUT: [{TestData.DefaultTestDisplayName}] {TestData.DefaultOutput}", ConsoleColor.DarkGray);
			}
		}

		[Fact]
		public static void Warnings()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestPassed(warnings: ["w1", "w2"]));
			classUnderTest.OnMessage(TestData.TestFinished());

			AssertStandardMetadata(classUnderTest, expectWarnings: true);
		}

		static void AssertCleanupFailure(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			string exceptionHeader,
			string? expectedUid = null,
			string? expectedDisplayName = null,
			bool verifyMetadata = true)
		{
			var testNode = GetTestNode(classUnderTest);

			if (expectedUid is null)
			{
				Assert.True(Guid.TryParse(testNode.Uid.Value, out _));
				Assert.Equal("Non-test cleanup failure", testNode.DisplayName);
			}
			else
			{
				Assert.Equal(expectedUid, testNode.Uid.Value);

				if (verifyMetadata)
					AssertStandardMetadata(classUnderTest, expectTiming: false, failure: true, expectedDisplayName: expectedDisplayName, exceptionHeader: exceptionHeader);
			}

			var error = testNode.Properties.Single<ErrorTestNodeStateProperty>();

			Assert.Null(error.Explanation);
			Assert.NotNull(error.Exception);
			Assert.Equal($"{exceptionHeader}{TestData.DefaultExceptionTypes[0]} : {TestData.DefaultExceptionMessages[0]}", error.Exception.Message);
			Assert.Equal(TestData.DefaultStackTraces[0], error.Exception.StackTrace);
		}

		static void AssertColorOutput(
			IOutputDeviceData data,
			string expectedMessage,
			ConsoleColor expectedForegroundColor)
		{
			var formattedData = Assert.IsType<FormattedTextOutputDeviceData>(data);
			var foregroundColor = Assert.IsType<SystemConsoleColor>(formattedData.ForegroundColor);
			Assert.Equal(expectedForegroundColor, foregroundColor.ConsoleColor);
			Assert.Equal(expectedMessage, formattedData.Text);
		}

		static TestNode AssertStandardMetadata(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			bool expectTiming = true,
			bool expectWarnings = false,
			bool expectTrx = false,
			bool failure = false,
			string? expectedDisplayName = null,
			string? exceptionHeader = null,
			string? expectedResultKind = null)
		{
			var testNode = GetTestNode(classUnderTest);
			Assert.Equal(TestData.DefaultTestCaseUniqueID, testNode.Uid.Value);
			Assert.Equal(expectedDisplayName ?? TestData.DefaultTestDisplayName, testNode.DisplayName);

			var resultKinds = testNode.Properties.OfType<TestMetadataProperty>()
				.Where(metadata => metadata.Key == "xunit.result-kind")
				.ToArray();
			if (expectedResultKind is null)
				Assert.Empty(resultKinds);
			else
				Assert.Equal(expectedResultKind, Assert.Single(resultKinds).Value);

			var testMethod = testNode.Properties.Single<TestMethodIdentifierProperty>();
			Assert.Equal(TestData.DefaultAssemblyName, testMethod.AssemblyFullName);
			Assert.Equal(TestData.DefaultMethodName, testMethod.MethodName);
			Assert.Equal(TestData.DefaultTestClassNamespace, testMethod.Namespace);
			Assert.Equal(TestData.DefaultMethodParameterTypes, testMethod.ParameterTypeFullNames);
			Assert.Equal(TestData.DefaultMethodReturnType, testMethod.ReturnTypeFullName);
			Assert.Equal(TestData.DefaultTestClassSimpleName, testMethod.TypeName);

			var testLocation = testNode.Properties.Single<TestFileLocationProperty>();
			Assert.Equal("/source/file/path.cs", testLocation.FilePath);
			Assert.Equal(42, testLocation.LineSpan.Start.Line);
			Assert.Equal(-1, testLocation.LineSpan.Start.Column);
			Assert.Equal(42, testLocation.LineSpan.End.Line);
			Assert.Equal(-1, testLocation.LineSpan.End.Column);

			if (expectTiming)
			{
				var timing = testNode.Properties.Single<TimingProperty>();
				Assert.Equal(TestData.DefaultStartTime, timing.GlobalTiming.StartTime);
				Assert.Equal(TestData.DefaultFinishTime, timing.GlobalTiming.EndTime);
			}

			if (expectWarnings)
			{
				var data = classUnderTest.OutputDevice.DisplayedData;

				Assert.Equal(2, data.Count);
				AssertColorOutput(data[0], $"WARNING: [{TestData.DefaultTestDisplayName}] w1", ConsoleColor.Yellow);
				AssertColorOutput(data[1], $"WARNING: [{TestData.DefaultTestDisplayName}] w2", ConsoleColor.Yellow);
			}

			if (expectTrx)
			{
				// Include a 'category' trait in this metadata, for TRX testing
				var testMetadata = testNode.Properties.OfType<TestMetadataProperty>()
					.Where(metadata => metadata.Key is not "xunit.result-kind" and not "xunit.explicit-reason");
				Assert.Collection(
					testMetadata.OrderBy(p => p.Key).ThenBy(p => p.Value).Select(p => $"'{p.Key}' = '{p.Value}'"),
					keyValue => Assert.Equal("'biff' = 'bang'", keyValue),
					keyValue => Assert.Equal("'category' = 'interesting'", keyValue),
					keyValue => Assert.Equal("'foo' = 'bar'", keyValue),
					keyValue => Assert.Equal("'foo' = 'baz'", keyValue)
				);

				var trxTestDefinitionName = testNode.Properties.Single<TrxTestDefinitionName>();
				Assert.Equal(TestData.DefaultTestCaseDisplayName, trxTestDefinitionName.TestDefinitionName);

				var trxFQTN = testNode.Properties.Single<TrxFullyQualifiedTypeNameProperty>();
				Assert.Equal(TestData.DefaultTestClassName, trxFQTN.FullyQualifiedTypeName);

				var trxCategories = testNode.Properties.Single<TrxCategoriesProperty>();
				var category = Assert.Single(trxCategories.Categories);
				Assert.Equal("interesting", category);

				var trxMessages = testNode.Properties.Single<TrxMessagesProperty>();
				var output = trxMessages.Messages.Single();
				Assert.Equal(TestData.DefaultOutput, output.Message);

				if (failure)
				{
					var trxException = testNode.Properties.Single<TrxExceptionProperty>();
					Assert.Equal($"{exceptionHeader}exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", trxException.Message);
					Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", trxException.StackTrace);
				}
			}
			else
			{
				// No 'category' trait in this metadata, we only add that for TRX testing
				var testMetadata = testNode.Properties.OfType<TestMetadataProperty>()
					.Where(metadata => metadata.Key is not "xunit.result-kind" and not "xunit.explicit-reason");
				Assert.Collection(
					testMetadata.OrderBy(p => p.Key).ThenBy(p => p.Value).Select(p => $"'{p.Key}' = '{p.Value}'"),
					keyValue => Assert.Equal("'biff' = 'bang'", keyValue),
					keyValue => Assert.Equal("'foo' = 'bar'", keyValue),
					keyValue => Assert.Equal("'foo' = 'baz'", keyValue)
				);
			}

			return testNode;
		}

		static void AssertExplicitReasonMetadata(
			TestNode testNode,
			params string[] expectedReasons)
		{
			var actual = testNode.Properties.OfType<TestMetadataProperty>()
				.Where(metadata => metadata.Key == "xunit.explicit-reason")
				.Select(metadata => metadata.Value)
				.OrderBy(value => value);
			Assert.Equal(expectedReasons.OrderBy(value => value), actual);
		}

		static TestNode GetTestNode(TestableTestPlatformExecutionMessageSink classUnderTest)
		{
			var message = Assert.Single(classUnderTest.TestNodeMessageBus.PublishedData);
			var updateMessage = Assert.IsType<TestNodeUpdateMessage>(message);

			return updateMessage.TestNode;
		}

		static void SendStartingMessages(
			TestableTestPlatformExecutionMessageSink messageSink,
			IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
			bool includeTestStarting = true,
			bool explicitTest = false)
		{
			messageSink.OnMessage(TestData.TestAssemblyStarting());
			messageSink.OnMessage(TestData.TestCollectionStarting());
			messageSink.OnMessage(TestData.TestClassStarting());
			messageSink.OnMessage(TestData.TestMethodStarting());
			messageSink.OnMessage(TestData.TestCaseStarting(sourceFilePath: "/source/file/path.cs", sourceLineNumber: 42));

			if (includeTestStarting)
				messageSink.OnMessage(TestData.TestStarting(@explicit: explicitTest, traits: traits ?? TestData.DefaultTraits));

			// Clear the published data cache of the ITestStarting translation
			messageSink.TestNodeMessageBus.PublishedData.Clear();
		}

		static void SendTestFailed(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			FailureCause failureCause)
		{
			classUnderTest.OnMessage(
				TestData.TestFailed(
					cause: failureCause,
					exceptionParentIndices: [-1, 0],
					exceptionTypes: ["exception 1", "exception 2"],
					messages: ["message 1", "message 2"],
					stackTraces: ["stack trace 1", "stack trace 2"]
				)
			);
			classUnderTest.OnMessage(TestData.TestFinished());
		}
	}

	class TestableTestPlatformExecutionMessageSink(
		SpyMessageSink innerSink,
		string assemblyFullName,
		SessionUid sessionUid,
		SpyTestPlatformMessageBus testNodeMessageBus,
		XunitTrxCapability trxCapability,
		SpyTestPlatformOutputDevice outputDevice,
		IConfiguration configuration,
		bool showLiveOutput,
		bool serverMode) :
			TestPlatformExecutionMessageSink(innerSink, assemblyFullName, sessionUid, testNodeMessageBus, trxCapability, outputDevice, configuration, showLiveOutput, serverMode)
	{
		public SpyMessageSink InnerSink { get; } = innerSink;
		public SpyTestPlatformOutputDevice OutputDevice { get; } = outputDevice;
		public SpyTestPlatformMessageBus TestNodeMessageBus { get; } = testNodeMessageBus;

		public static TestableTestPlatformExecutionMessageSink Create(
			bool trxEnabled = false,
			bool showLiveOutput = false,
			bool serverMode = false,
			string? resultsPath = null,
			SpyTestPlatformMessageBus? testNodeMessageBus = null) =>
				new(SpyMessageSink.Capture(), "test-assembly", new(), testNodeMessageBus ?? new(), new(trxEnabled), new(), Mocks.MicrosoftTestingPlatform.Configuration(resultsPath), showLiveOutput, serverMode);
	}
}
