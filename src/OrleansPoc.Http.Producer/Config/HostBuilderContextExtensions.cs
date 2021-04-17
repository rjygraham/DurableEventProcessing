using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace OrleansPoc.Http.Producer.Config
{
	public static class HostBuilderContextExtensions
	{
		public static void AddProducerOptions(this IServiceCollection services, HostBuilderContext ctx)
		{
			var options = new ProducerOptions
			{
				ApiBaseAddress = ctx.Configuration.GetSection("API_BASEADDRESS").Value,
				SensorId = ctx.Configuration.GetSection("SENSOR_ID").Value ?? Guid.NewGuid().ToString(),
				MillisecondsDelay = int.Parse(ctx.Configuration.GetSection("DELAY").Value)
			};

			services.AddSingleton(options);
		}
	}
}
