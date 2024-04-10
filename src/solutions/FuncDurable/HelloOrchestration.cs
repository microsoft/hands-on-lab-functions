using System.Net.Http.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FuncDurable
{
    public class AudioItem
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string EventType { get; set; }
    }

    public class BlobEvent
    {
        public string Id { get; set; }

        public string Topic { get; set; }

        public string Subject { get; set; }

        public string EventType { get; set; }

        public IDictionary<string, object> Data { get; set; }
    }

    public static class ProcessOrchestration
    {
        [Function(nameof(EventGridFunction))]
        public static async Task<OrchestrationMetadata?> EventGridFunction(
                [EventGridTrigger] BlobEvent input,
                [DurableClient] DurableTaskClient client,
                FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(EventGridFunction));

            logger.LogInformation("========= EVENT GRID FUNCTION =========");
            logger.LogInformation(input.Id);
            logger.LogInformation(input.EventType);
            logger.LogInformation(input.Data["url"].ToString());

            var outputAudio = new AudioItem()
            {
                Id = input.Id,
                Path = input.Data["url"].ToString() ?? "",
                EventType = input.EventType
            };

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(ProcessOrchestration), outputAudio);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return await client.GetInstancesAsync(instanceId);
        }

        [Function(nameof(ProcessOrchestration))]
        public static async Task RunOrchestrator(
          [OrchestrationTrigger] TaskOrchestrationContext context, AudioItem audio)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(ProcessOrchestration));
            logger.LogInformation("Saying hello.");

            var audioFile = await context.CallActivityAsync<string>(nameof(DownloadAudio), audio);
            var text = await context.CallActivityAsync<string>(nameof(SpeechToText), audio);
            await context.CallActivityAsync<string>(nameof(SaveDataToCosmosDB), text);
        }

        [Function(nameof(DownloadAudio))]
         public static async Task<string> DownloadAudio([ActivityTrigger] AudioItem audio, FunctionContext executionContext)
        {
            // Download the audio file from the storage account
            ILogger logger = executionContext.GetLogger("SpeechToText");

            var connectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING");
            var container = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONTAINER");
            var client = new BlobServiceClient(connectionString);
            var containerClient = client.GetBlobContainerClient(container);

            logger.LogInformation($"_____________ ConnectionString {connectionString}");
            logger.LogInformation($"_____________ Container {container}");
            logger.LogInformation($"_____________ Blob to download {audio.Path.Split("/").Last()}");

            var blobClient = containerClient.GetBlobClient(audio.Path.Split("/").Last());

            var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(10));
            var sasUri = blobClient.GenerateSasUri(builder);
            
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(sasUri);
            logger.LogInformation($"_____________ Response {response}");

            return "";
        }

        [Function(nameof(SpeechToText))]
        public static async Task<string> SpeechToText([ActivityTrigger] string audioPath, FunctionContext executionContext)
        {
            // Download the audio file from the storage account
            ILogger logger = executionContext.GetLogger("SpeechToText");

            logger.LogInformation($"_____________ URI: {Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed");
            
            // Send the audio file to the Speech to Text API
            using (HttpClient httpClient = new HttpClient())
            {
                var url = $"{Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Content-Type", "audio/wav");
                request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_API_KEY"));
            
                HttpResponseMessage httpResponse = await httpClient.SendAsync(request);
                logger.LogInformation($"_____________ Result Audio {httpResponse.Content}");
            }
        
            return "Audio text {audio.Id} is ready.";
        }

        [Function(nameof(SaveDataToCosmosDB))]
        public static string SaveDataToCosmosDB(
            // AudioItem audioItem,
            [CosmosDBTrigger(
                databaseName: "%COSMOS_DB_DATABASE_NAME%",
                containerName: "%COSMOS_DB_CONTAINER_ID%",
                Connection = "COSMOS_DB_CONNECTION_STRING_SETTING",
                CreateLeaseContainerIfNotExists = true,
                LeaseContainerName = "leases")
            ] out dynamic document,
            FunctionContext executionContext)
        {
            var name = "Seattle";
            ILogger logger = executionContext.GetLogger("SaveDataToCosmosDB");
            logger.LogInformation("Saying hello to =====================  {name}.", name);
            document = new { id = Guid.NewGuid().ToString(), Name = name };

            return $"===================== Hello {name}!";
        }


    }
}
