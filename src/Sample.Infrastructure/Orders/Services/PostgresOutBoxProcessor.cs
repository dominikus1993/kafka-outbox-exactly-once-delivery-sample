using Microsoft.EntityFrameworkCore;
using Sample.Common.Types;
using Sample.Infrastructure.Messaging;
using Sample.Infrastructure.Orders.DbContexts;

namespace Sample.Infrastructure.Orders.Services;

public sealed class PostgresOutBoxProcessor
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;
    private readonly IEventPublisher _messagePublisher;
    private readonly TimeProvider _timeProvider;

    public PostgresOutBoxProcessor(IDbContextFactory<OrdersDbContext> dbContextFactory, IEventPublisher messagePublisher, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _messagePublisher = messagePublisher;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Unit>> ProcessEvents(CancellationToken cancellationToken = default)
    {
        await using var readContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var writeContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var events = readContext.GetUnprocessedOutBoxEvents();
        await foreach (var @event in events.WithCancellation(cancellationToken))
        {
            var messagePublicationResult = await _messagePublisher.Publish(@event, cancellationToken);
            if (!messagePublicationResult.IsSuccess)
            {
                return Result.Failure<Unit>(messagePublicationResult.ErrorValue);
            }

            var timestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
            await writeContext.OutBox.Where(x => x.Id == @event.Id)
                .ExecuteUpdateAsync(calls => calls.SetProperty(x => x.ProcessedAtTimestamp, timestamp), cancellationToken: cancellationToken);
        }

        return Result.UnitResult;
    }
    
}