using System;

namespace BuildingBlocks.CQRS
{
    public interface ICachedQuery<out TResponse> : IQuery<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}
