using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System;

namespace OrleansPoc.Processor.SiloHost.Storage
{
	public static class CosmosDbSiloBuilderExtensions
	{
		public static ISiloBuilder AddCosmosDbGrainStorage(this ISiloBuilder builder, string providerName, Action<CosmosDbGrainStorageOptions> options)
		{
			return builder.ConfigureServices(services => services.AddCosmosDbGrainStorage(providerName, options));
		}

		public static IServiceCollection AddCosmosDbGrainStorage(this IServiceCollection services, string providerName, Action<CosmosDbGrainStorageOptions> options)
		{
			services.AddOptions<CosmosDbGrainStorageOptions>(providerName).Configure(options);
			return services
				.AddSingletonNamedService(providerName, CosmosDbGrainStorageFactory.Create)
				.AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
		}
	}
}
