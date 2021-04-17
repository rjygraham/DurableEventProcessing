using Microsoft.Azure.Cosmos;

namespace OrleansPoc.Processor.SiloHost.Storage
{
	public class CosmosDbGrainStorageOptions
	{
		public string AccountEndpoint { get; set; }
		public string AccountKey { get; set; }
		public string DatabaseId { get; set; }
		public string ContainerId { get; set; }
		public CosmosClientOptions CosmosClientOptions { get; set; }
	}
}
