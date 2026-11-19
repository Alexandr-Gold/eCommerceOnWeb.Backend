namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;
internal sealed class SaveChangesTestBarrier
{
    private readonly object _sync = new();

    private BarrierState? _state;
    private int _arrivals;

    public void Arm(
        int participantCount,
        TimeSpan? timeout = null)
    {
        if (participantCount < 2)
            throw new ArgumentOutOfRangeException(nameof(participantCount));

        lock (_sync)
        {
            if (_state is not null)
                throw new InvalidOperationException(
                    "Barrier is already armed.");

            _arrivals = 0;

            _state = new BarrierState(
                participantCount,
                timeout ?? TimeSpan.FromSeconds(20));
        }
    }

    public async Task SignalAndWaitAsync(
        CancellationToken cancellationToken = default)
    {
        BarrierState? state;

        lock (_sync)
        {
            state = _state;

            if (state is null)
                return;

            _arrivals++;

            if (_arrivals > state.ParticipantCount)
                throw new InvalidOperationException(
                    "Unexpected number of barrier participants.");

            if (_arrivals == state.ParticipantCount)
                state.Signal.TrySetResult(true);
        }

        await state.Signal.Task.WaitAsync(
            state.Timeout,
            cancellationToken);
    }

    public void Disarm()
    {
        lock (_sync)
        {
            _state = null;
            _arrivals = 0;
        }
    }

    private sealed class BarrierState
    {
        public int ParticipantCount { get; }

        public TimeSpan Timeout { get; }

        public TaskCompletionSource<bool> Signal { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public BarrierState(
            int participantCount,
            TimeSpan timeout)
        {
            ParticipantCount = participantCount;
            Timeout = timeout;
        }
    }
}
