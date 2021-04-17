using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using OrleansPoc.Processor.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansPoc.Processor.SiloHost.Storage
{
	public class CosmosDbGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
	{
		private readonly string storageName;
		private readonly CosmosDbGrainStorageOptions options;
		private readonly ClusterOptions clusterOptions;
		private readonly IGrainFactory grainFactory;
		private readonly ITypeResolver typeResolver;
		private JsonSerializerSettings jsonSettings;
		private readonly Container container;

		public CosmosDbGrainStorage(string storageName, CosmosDbGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions, IGrainFactory grainFactory, ITypeResolver typeResolver)
		{
			this.storageName = storageName;

			this.options = options;
			this.clusterOptions = clusterOptions.Value;
			this.grainFactory = grainFactory;
			this.typeResolver = typeResolver;

			this.container = BuildCosmosContainer(options);
		}

		private Container BuildCosmosContainer(CosmosDbGrainStorageOptions options)
		{
			var client = new CosmosClient(options.AccountEndpoint, options.AccountKey, options.CosmosClientOptions);
			var db = client.GetDatabase(options.DatabaseId);
			return db.GetContainer(options.ContainerId);
		}

		public void Participate(ISiloLifecycle lifecycle)
		{
			lifecycle.Subscribe(OptionFormattingUtilities.Name<CosmosDbGrainStorage>(storageName), ServiceLifecycleStage.ApplicationServices, Init);
		}

		private Task Init(CancellationToken ct)
		{
			// Settings could be made configurable from Options.
			jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(OrleansJsonSerializer.GetDefaultSerializerSettings(typeResolver, grainFactory), false, false, null);

			return Task.CompletedTask;
		}

		public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			var key = grainReference.GetPrimaryKey().ToString();
			var query = new QueryDefinition("SELECT * FROM grains g where g.pk = @partitionKey").WithParameter("@partitionKey", key);

			var results = new List<SensorTypeAState>();
			var iterator = container.GetItemQueryIterator<SensorTypeAState>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(key) });
			while (iterator.HasMoreResults)
			{
				var response = await iterator.ReadNextAsync();
				results.AddRange(response);
			}

			if (results.Count == 0)
			{
				grainState.State = new SensorTypeAState(new Dictionary<DateTimeOffset, int>());
			}
			else
			{
				grainState.State = new SensorTypeAState(results.OrderBy(o => o.Timestamp).ToDictionary(k => k.Timestamp, v => v.CurrentMeasurement));
			}
		}

		public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			var key = grainReference.GetPrimaryKey().ToString();
			((SensorTypeAState)grainState.State).PartitionKey = key;
			await container.CreateItemAsync(grainState.State, new PartitionKey(key));
		}
		public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			return Task.CompletedTask;
		}
	}
}
