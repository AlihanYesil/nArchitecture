﻿using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, ICahableRequest
{
	private readonly CacheSettings _cacheSettings;
	private readonly IDistributedCache _cache;
	private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;	

	public CachingBehavior( IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger, IConfiguration configuration)
	{
		_cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ??throw new InvalidOperationException();
		_cache = cache;
		_logger = logger;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (request.BypassCache) 
		{
			return await next();
		}
		TResponse response;
		byte[]? cacheResponse = await _cache.GetAsync(request.Cachekey, cancellationToken);
		if (cacheResponse != null)
		{
		  response=JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cacheResponse));
			_logger.LogInformation($"Fetched from cache ->{request.Cachekey}");
		}
		else
		{
			response = await getResponseAndAddToCache(request,next, cancellationToken);
		}
		return response;
	}

	private async Task<TResponse?> getResponseAndAddToCache(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		
		TResponse response = await next();
		TimeSpan SlidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSettings.SlidingExpiration);
		DistributedCacheEntryOptions cacheOptions = new() { SlidingExpiration=SlidingExpiration };

		byte[] serializeData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

		await _cache.SetAsync(request.Cachekey, serializeData,cacheOptions, cancellationToken);
		_logger.LogInformation($"Added to Cache -> : {request.Cachekey}");
		 
		if (request.CacheGroupKey != null)
			 await addCacheKeyToGroup(request,SlidingExpiration, cancellationToken);
		return response;
	}
	private async Task addCacheKeyToGroup(TRequest request, TimeSpan slidingExpiration, CancellationToken cancellationToken)
	{
		byte[]? cacheGroupCache = await _cache.GetAsync(key: request.CacheGroupKey!, cancellationToken);
		HashSet<string> cacheKeysInGroup;
		if (cacheGroupCache != null)
		{
			cacheKeysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cacheGroupCache))!;
			if (!cacheKeysInGroup.Contains(request.Cachekey))
				cacheKeysInGroup.Add(request.Cachekey);
		}
		else
			cacheKeysInGroup = new HashSet<string>(new[] { request.Cachekey });
		byte[] newCacheGroupCache = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup);

		byte[]? cacheGroupCacheSlidingExpirationCache = await _cache.GetAsync(
			key: $"{request.CacheGroupKey}SlidingExpiration",
			cancellationToken
		);
		int? cacheGroupCacheSlidingExpirationValue = null;
		if (cacheGroupCacheSlidingExpirationCache != null)
			cacheGroupCacheSlidingExpirationValue = Convert.ToInt32(Encoding.Default.GetString(cacheGroupCacheSlidingExpirationCache));
		if (cacheGroupCacheSlidingExpirationValue == null || slidingExpiration.TotalSeconds > cacheGroupCacheSlidingExpirationValue)
			cacheGroupCacheSlidingExpirationValue = Convert.ToInt32(slidingExpiration.TotalSeconds);
		byte[] serializeCachedGroupSlidingExpirationData = JsonSerializer.SerializeToUtf8Bytes(cacheGroupCacheSlidingExpirationValue);

		DistributedCacheEntryOptions cacheOptions =
			new() { SlidingExpiration = TimeSpan.FromSeconds(Convert.ToDouble(cacheGroupCacheSlidingExpirationValue)) };

		await _cache.SetAsync(key: request.CacheGroupKey!, newCacheGroupCache, cacheOptions, cancellationToken);
		_logger.LogInformation($"Added to Cache -> {request.CacheGroupKey}");

		await _cache.SetAsync(
			key: $"{request.CacheGroupKey}SlidingExpiration",
			serializeCachedGroupSlidingExpirationData,
			cacheOptions,
			cancellationToken
		);
		_logger.LogInformation($"Added to Cache -> {request.CacheGroupKey}SlidingExpiration");
	}
}