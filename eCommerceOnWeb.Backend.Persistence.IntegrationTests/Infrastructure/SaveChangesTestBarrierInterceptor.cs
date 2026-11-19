using Microsoft.EntityFrameworkCore.Diagnostics;

namespace eCommerceOnWeb.Backend.Persistence.IntegrationTests.Infrastructure;
internal sealed class SaveChangesTestBarrierInterceptor
    : SaveChangesInterceptor
{
    private readonly SaveChangesTestBarrier _barrier;

    public SaveChangesTestBarrierInterceptor(
        SaveChangesTestBarrier barrier)
    {
        _barrier = barrier;
    }

    public override async ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        await _barrier.SignalAndWaitAsync(cancellationToken);

        return result;
    }
}
