using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;
using System;

namespace OrleansPoc.Processor.SiloHost.Storage
{
	public static class CosmosDbGrainStorageFactory
	{
		internal static IGrainStorage Create(IServiceProvider services, string name)
		{
			using (var scope = services.CreateScope())
			{
				var scopedProvider = scope.ServiceProvider;
				var options = scopedProvider.GetRequiredService<IOptionsSnapshot<CosmosDbGrainStorageOptions>>();
				return ActivatorUtilities.CreateInstance<CosmosDbGrainStorage>(services, name, options.Get(name), services.GetProviderClusterOptions(name));
			}
		}
	}
}
