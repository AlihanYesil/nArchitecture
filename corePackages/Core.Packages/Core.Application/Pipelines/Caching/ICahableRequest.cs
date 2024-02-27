using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Pipelines.Caching;

public  interface ICahableRequest
{
	string Cachekey { get; }
	bool BypassCache {  get; }
	string? CacheGroupKey { get; }
	TimeSpan? SlidingExpiration { get; }

}
