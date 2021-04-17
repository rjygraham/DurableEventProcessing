using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System;

await Host.CreateDefaultBuilder(args)
	.UseOrleans((ctx, siloBuilder) =>
	{
		if (ctx.HostingEnvironment.IsDevelopment())
		{
			siloBuilder.UseLocalhostClustering(siloPort: 11111, gatewayPort: 30000, serviceId: "processor", clusterId: "processor");
		}
		else
		{
			// For hosting Azure we'll use AKS to orchestrate and Azure Storage for Orleans clustering.
			siloBuilder
				.UseKubernetesHosting()
				.UseAzureStorageClustering(options =>
				{
					options.ConnectionString = ctx.Configuration.GetSection("STORAGEACCOUNT_CONNECTIONSTRING").Value;
					options.TableName = "clusters";
				});
		}

		// Get secrets and other config from environment variables (user-secrets) for local dev.
		// More info on dotnet user-secrets: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows
		siloBuilder
			.AddAzureTableGrainStorage("PubSubStore", options => options.ConnectionString = ctx.Configuration.GetSection("STORAGEACCOUNT_CONNECTIONSTRING").Value)
			.AddApplicationInsightsTelemetryConsumer("INSTRUMENTATION_KEY")
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
	.ConfigureWebHostDefaults(webBuilder =>
	{
		webBuilder
			.ConfigureServices(services => services.AddControllers())
			.Configure((ctx, app) =>
			{
				if (ctx.HostingEnvironment.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
				}

				app.UseRouting();
				app.UseAuthorization();
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapControllers();
				});
			});
	})
	.ConfigureServices(services => {})
	.RunConsoleAsync();