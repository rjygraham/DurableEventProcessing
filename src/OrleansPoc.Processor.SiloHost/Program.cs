using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using OrleansPoc.Processor.Grains;
using OrleansPoc.Processor.SiloHost.Storage;
using System;

await Host.CreateDefaultBuilder(args)
	.UseOrleans((ctx, siloBuilder) =>
	{
		if (ctx.HostingEnvironment.IsDevelopment())
		{
			// Change ports, serviceId, and clusterId so we don't conflict with the API silo.
			siloBuilder.UseLocalhostClustering(siloPort: 11112, gatewayPort: 30001, serviceId: "processor", clusterId: "processor");
		}
		else
		{
			// For hosting Azure we'll use AKS to orchestrate and Azure Storage for Orleans clustering.
			siloBuilder.UseKubernetesHosting()
				.UseAzureStorageClustering(options =>
				{
					options.ConnectionString = ctx.Configuration.GetSection("STORAGEACCOUNT_CONNECTIONSTRING").Value;
					options.TableName = "clusters";
				});
		}

		// Get secrets and other config from environment variables (user-secrets) for local dev.
		// More info on dotnet user-secrets: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows
		siloBuilder.AddAzureTableGrainStorage("PubSubStore", options => options.ConnectionString = ctx.Configuration.GetSection("STORAGEACCOUNT_CONNECTIONSTRING").Value)
		.AddApplicationInsightsTelemetryConsumer("INSTRUMENTATION_KEY")
		.AddCosmosDbGrainStorage(nameof(SensorTypeAProcessorGrain), options =>
		{
			options.AccountEndpoint = ctx.Configuration.GetSection("COSMOSDB_ACCOUNTENDPOINT").Value;
			options.AccountKey = ctx.Configuration.GetSection("COSMOSDB_ACCOUNTKEY").Value;
			options.DatabaseId = ctx.Configuration.GetSection("COSMOSDB_DATABASE").Value;
			options.ContainerId = ctx.Configuration.GetSection("COSMOSDB_GRAIN_COLLECTION").Value;
		})
		.AddEventHubStreams("my-stream-provider", b =>
		{
			b.ConfigureEventHub(ob => ob.Configure(options =>
			{
				options.ConnectionString = ctx.Configuration.GetSection("EVENTHUB_CONNECTIONSTRING").Value;
				options.Path = ctx.Configuration.GetSection("EVENTHUB_PATH").Value;
				options.ConsumerGroup = ctx.Configuration.GetSection("EVENTHUB_CONSUMERGROUP").Value;
			}));
			b.UseAzureTableCheckpointer(ob => ob.Configure(options =>
			{
				options.ConnectionString = ctx.Configuration.GetSection("STORAGEACCOUNT_CONNECTIONSTRING").Value;
				options.PersistInterval = TimeSpan.FromSeconds(10);
			}));
		});
	})
	.ConfigureServices(services => {})
	.RunConsoleAsync();