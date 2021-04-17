using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrleansPoc.Http.Producer;
using OrleansPoc.Http.Producer.Config;

await Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((ctx, builder) =>
	{
		if (ctx.HostingEnvironment.IsDevelopment())
		{
			builder.AddUserSecrets<ProducerHostedService>();
		}
	})
	.ConfigureServices((ctx, services) =>
	{
		services.AddProducerOptions(ctx);
		services.AddHostedService<ProducerHostedService>();
	})
	.RunConsoleAsync();
