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
    }

    public static class ProcessOrchestration
    {
        [Function(nameof(BlobTriggerFunction))]
        public static async Task<OrchestrationMetadata?> BlobTriggerFunction(
                [BlobTrigger("%STORAGE_ACCOUNT_CONTAINER%/{name}", Connection = "STORAGE_ACCOUNT_CONNECTION_STRING")] string input,
                [DurableClient] DurableTaskClient client,
                FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(BlobTriggerFunction));

            logger.LogInformation("========= Blob Storage FUNCTION =========");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(ProcessOrchestration));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return await client.GetInstancesAsync(instanceId);
        }

        [Function(nameof(ProcessOrchestration))]
        public static async Task RunOrchestrator(
          [OrchestrationTrigger] TaskOrchestrationContext context, AudioItem audio)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(ProcessOrchestration));
            logger.LogInformation("Start orchestration.");

            await context.CallActivityAsync<object>(nameof(SaveDataToCosmosDB), "name");
        }

        [Function(nameof(SaveDataToCosmosDB))]
        [CosmosDBOutput("%COSMOS_DB_DATABASE_NAME%",
                        "%COSMOS_DB_CONTAINER_ID%",
                        Connection = "COSMOS_DB_CONNECTION_STRING_SETTING",
                        CreateIfNotExists = true)]
        public static object SaveDataToCosmosDB(
            [ActivityTrigger] string name,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SaveDataToCosmosDB");
            logger.LogInformation("Saying hello SaveDataToCosmosDB.");

            return new { Id = Guid.NewGuid(), Path = "/file/path" };
        }

        // [Function(nameof(DownloadAudio))]
        //  public static async Task<string> DownloadAudio([ActivityTrigger] AudioItem audio, FunctionContext executionContext)
        // {
        //     // Download the audio file from the storage account
        //     ILogger logger = executionContext.GetLogger("SpeechToText");

        //     var connectionString = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONNECTION_STRING");
        //     var container = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONTAINER");
        //     var client = new BlobServiceClient(connectionString);
        //     var containerClient = client.GetBlobContainerClient(container);

        //     logger.LogInformation($"_____________ ConnectionString {connectionString}");
        //     logger.LogInformation($"_____________ Container {container}");
        //     logger.LogInformation($"_____________ Blob to download {audio.Path.Split("/").Last()}");

        //     var blobClient = containerClient.GetBlobClient(audio.Path.Split("/").Last());

        //     var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(10));
        //     var sasUri = blobClient.GenerateSasUri(builder);

        //     HttpClient httpClient = new HttpClient();
        //     var response = await httpClient.GetAsync(sasUri);
        //     logger.LogInformation($"_____________ Response {response}");

        //     return "";
        // }

        // [Function(nameof(SpeechToText))]
        // public static async Task<string> SpeechToText([ActivityTrigger] string audioPath, FunctionContext executionContext)
        // {
        //     // Download the audio file from the storage account
        //     ILogger logger = executionContext.GetLogger("SpeechToText");

        //     logger.LogInformation($"_____________ URI: {Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed");

        //     // Send the audio file to the Speech to Text API
        //     using (HttpClient httpClient = new HttpClient())
        //     {
        //         var url = $"{Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}speech/recognition/conversation/cognitiveservices/v1?language=en-US&format=detailed";
        //         HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        //         request.Headers.Add("Accept", "application/json");
        //         request.Headers.Add("Content-Type", "audio/wav");
        //         request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_API_KEY"));

        //         HttpResponseMessage httpResponse = await httpClient.SendAsync(request);
        //         logger.LogInformation($"_____________ Result Audio {httpResponse.Content}");
        //     }

        //     return "Audio text {audio.Id} is ready.";
        // }

    }
}
