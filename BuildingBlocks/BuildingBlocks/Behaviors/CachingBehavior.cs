using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ICachedQuery<TResponse> cachedQuery)
            {
                return await next();
            }

            var cacheKey = cachedQuery.CacheKey;
            
            try
            {
                var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedResponse))
                {
                    _logger.LogInformation("Fetched request {RequestName} from cache for Key: {CacheKey}", typeof(TRequest).Name, cacheKey);
                    
                    if (typeof(Microsoft.AspNetCore.Http.IResult).IsAssignableFrom(typeof(TResponse)))
                    {
                        var result = Microsoft.AspNetCore.Http.Results.Text(cachedResponse, "application/json");
                        return (TResponse)(object)result;
                    }
                    
                    return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch {RequestName} from cache for Key: {CacheKey}", typeof(TRequest).Name, cacheKey);
            }

            var response = await next();

            try
            {
                var expiration = cachedQuery.Expiration ?? TimeSpan.FromMinutes(15);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                object? dataToCache = response;
                if (response is Microsoft.AspNetCore.Http.IValueHttpResult valueResult)
                {
                    dataToCache = valueResult.Value;
                }

                var serializedData = JsonSerializer.Serialize(dataToCache);
                await _cache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);
                _logger.LogInformation("Added {RequestName} to cache for Key: {CacheKey}", typeof(TRequest).Name, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save {RequestName} to cache for Key: {CacheKey}", typeof(TRequest).Name, cacheKey);
            }

            return response;
        }
    }
}
