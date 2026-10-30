using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

public sealed class TestNodeUpdateConsumer : IDataConsumer
{
	public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

	public List<TestNodeUpdateMessage> ConsumedUpdates { get; } = [];

	public string Uid => "xunit.tests.test-node-update-consumer";

	public string Version => "1.0.0";

	public string DisplayName => "Test node update consumer";

	public string Description => "Observes test node updates through the Microsoft.Testing.Platform data contract.";

	public Task<bool> IsEnabledAsync() => Task.FromResult(true);

	public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
	{
		if (value is TestNodeUpdateMessage update)
			ConsumedUpdates.Add(update);

		return Task.CompletedTask;
	}
}
