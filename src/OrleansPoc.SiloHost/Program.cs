using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System;


await Host.CreateDefaultBuilder(args)
	.UseOrleans(ConfigureOrleans)
	.ConfigureWebHostDefaults(ConfigureWebHost)
	.ConfigureServices(services =>
	{
		//services.AddSingleton<ReferenceDataService>();
	})
	.RunConsoleAsync();

void ConfigureOrleans(Microsoft.Extensions.Hosting.HostBuilderContext ctx, ISiloBuilder siloBuilder)
{
	if (ctx.HostingEnvironment.IsDevelopment())
	{
		siloBuilder.UseLocalhostClustering()
		//.AddMemoryGrainStorage("sensors")
		.AddAzureTableGrainStorage("PubSubStore", options => options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=aksjhfglfgkhfj;AccountKey=IFh9VT6Ns+Btn/ENe4JTSsvsZ02UUiN8QdktCznFkw6j67RAAIlVf9Q+6yMwOZ2oe5voXDJjvzZASBrGh07xhg==;EndpointSuffix=core.windows.net")
		.AddEventHubStreams("my-stream-provider", b =>
		{
			b.ConfigureEventHub(ob => ob.Configure(options =>
			{
				options.ConnectionString = "Endpoint=sb://rgom-tpoc-eus-evhub.servicebus.windows.net/;SharedAccessKeyName=default;SharedAccessKey=zpooXwanlG/LBvxsn4K0wjN6YkrrIi5jJG7cdhjTo8s=;EntityPath=sensors";
				options.Path = "sensors";
				options.ConsumerGroup = "silohost";
			}));
			b.UseAzureTableCheckpointer(ob => ob.Configure(options =>
			{
				options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=aksjhfglfgkhfj;AccountKey=IFh9VT6Ns+Btn/ENe4JTSsvsZ02UUiN8QdktCznFkw6j67RAAIlVf9Q+6yMwOZ2oe5voXDJjvzZASBrGh07xhg==;EndpointSuffix=core.windows.net";
				options.PersistInterval = TimeSpan.FromSeconds(10);
			}));
		});
	}
	else
	{
		// In Kubernetes, we use environment variables and the pod manifest
		siloBuilder.UseKubernetesHosting();
	}
}

void ConfigureWebHost(IWebHostBuilder webBuilder)
{
	webBuilder.ConfigureServices(services => services.AddControllers());
	webBuilder.Configure((ctx, app) =>
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
}