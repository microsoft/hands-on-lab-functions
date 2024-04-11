using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.Text;
using System.Text.Json;

namespace FuncDurable
{
    public class AudioFile
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public string urlWithSasToken { get; set; }
    }

    public class TranscriptionJobFiles
    {
        public string files { get; set; }
    }
    public class TranscriptionJob
    {
        public string self { get; set; }

        public string status { get; set; }

        public TranscriptionJobFiles links { get; set; }
    }

    public class TranscriptionResultValueFile
    {
        public string contentUrl { get; set; }
    }

    public class TranscriptionResultValue
    {
        public string kind { get; set; }
        public TranscriptionResultValueFile links { get; set; }
    }

    public class TranscriptionResult
    {
        public TranscriptionResultValue[] values { get; set; }

    }

    public class Transcription
    {
        public string display { get; set; }

    }

    public class TranscriptionDetails
    {
        public Transcription[] combinedRecognizedPhrases { get; set; }

    }

    public static class AudioTranscriptionOrchestration
    {
        [Function(nameof(AudioTranscriptionOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context, string audioBlobSasUri)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(AudioTranscriptionOrchestration));
            logger.LogInformation("Processing audio file");

            var jobUrl = await context.CallActivityAsync<string>(nameof(StartTranscription), audioBlobSasUri);

            DateTime endTime = context.CurrentUtcDateTime.AddMinutes(2);

            while (context.CurrentUtcDateTime < endTime)
            {
                // Check transcription
                // if (!context.IsReplaying) { logger.LogInformation($"Checking current weather conditions for {input.Location} at {context.CurrentUtcDateTime}."); }

                string? transcription = await context.CallActivityAsync<string?>("GetTranscription", jobUrl);
                logger.LogInformation($"transcription: {transcription}");

                if (transcription != null)
                {
                    // It's not raining! Or snowing. Or misting. Tell our user to take advantage of it.
                    // if (!context.IsReplaying) { logger.LogInformation($"Detected clear weather for {input.Location}. Notifying {input.Phone}."); }

                    await context.CallActivityAsync("SaveTranscription", transcription);
                    break;
                }
                else
                {
                    // Wait for the next checkpoint
                    var nextCheckpoint = context.CurrentUtcDateTime.AddSeconds(5);
                    // if (!context.IsReplaying) { logger.LogInformation($"Next check for {input.Location} at {nextCheckpoint}."); }

                    await context.CreateTimer(nextCheckpoint, CancellationToken.None);
                }
            }
        }

        [Function(nameof(StartTranscription))]
        public static async Task<string> StartTranscription([ActivityTrigger] string audioBlobSasUri, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(StartTranscription));
            logger.LogInformation("StartTranscription {audioBlobSasUri}.", audioBlobSasUri);

            using (HttpClient httpClient = new HttpClient())
            {
                var url = $"{Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}/speechtotext/v3.1/transcriptions";
                var apiKey = Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_API_KEY");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                using StringContent jsonContent = new(
                    JsonSerializer.Serialize(new
                    {
                        contentUrls = new List<string> { audioBlobSasUri },
                        locale = "en-US",
                        displayName = "My Transcription",

                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey);

                HttpResponseMessage httpResponse = await httpClient.PostAsync(url, jsonContent);
                var serializedJob = await httpResponse.Content.ReadAsStringAsync();

                var job = JsonSerializer.Deserialize<TranscriptionJob>(serializedJob);

                logger.LogInformation($"_____________ job {job.self}");

                return job.self;
            }
        }

        
        [Function(nameof(GetTranscription))]
        public static async Task<string?> GetTranscription([ActivityTrigger] string jobUrl, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(StartTranscription));

            using (HttpClient httpClient = new HttpClient())
            {
                var apiKey = Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_API_KEY");

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", apiKey);

                HttpResponseMessage httpResponse = await httpClient.GetAsync(jobUrl);
                var serializedJob = await httpResponse.Content.ReadAsStringAsync();

                var job = JsonSerializer.Deserialize<TranscriptionJob>(serializedJob);

                logger.LogInformation($"job status {job.status}");

                if (job.status != "Succeeded")
                {
                    return null;
                }

                var files = job.links.files;

                HttpResponseMessage resultsHttpResponse = await httpClient.GetAsync(files);
                var serializedJobResults = await resultsHttpResponse.Content.ReadAsStringAsync();
                var transcriptionResult = JsonSerializer.Deserialize<TranscriptionResult>(serializedJobResults);
                var transcriptionFileUrl = transcriptionResult?.values.Where(value => value.kind == "Transcription").First().links.contentUrl;

                if (transcriptionFileUrl == null) {
                    return ""; // TODO: throw error instead of returning an empty string
                }

                HttpResponseMessage transcriptionDetailsHttpResponse = await httpClient.GetAsync(transcriptionFileUrl);
                var serializedTranscriptionDetails = await transcriptionDetailsHttpResponse.Content.ReadAsStringAsync();
                var transcriptionDetails = JsonSerializer.Deserialize<TranscriptionDetails>(serializedTranscriptionDetails);
                var transcription = transcriptionDetails?.combinedRecognizedPhrases.First().display ?? ""; // TODO: throw error instead of returning an empty string

                logger.LogInformation($"transcription {transcription}");

                return transcription;
            }
        }

        // [Function(nameof(SaveDataToCosmosDB))]
        // [CosmosDBOutput("%COSMOS_DB_DATABASE_NAME%",
        //                 "%COSMOS_DB_CONTAINER_ID%",
        //                 Connection = "COSMOS_DB_CONNECTION_STRING_SETTING",
        //                 CreateIfNotExists = true)]
        // public static object SaveDataToCosmosDB(
        //     [ActivityTrigger] string name,
        //     FunctionContext executionContext)
        // {
        //     ILogger logger = executionContext.GetLogger("SaveDataToCosmosDB");
        //     logger.LogInformation("Saying hello SaveDataToCosmosDB.");

        //     return new { Id = Guid.NewGuid(), Path = "/file/path" };
        // }

        [Function(nameof(SaveTranscription))]
        public static string SaveTranscription([ActivityTrigger] string transcription, FunctionContext executionContext)
        {
            // TODO
            ILogger logger = executionContext.GetLogger(nameof(SaveTranscription));
            logger.LogInformation("SaveTranscription");
            return $"Hello {transcription}!";
        }


        [Function(nameof(AudioBlobUploadStart))]
        public static async Task AudioBlobUploadStart(
            [BlobTrigger("%STORAGE_ACCOUNT_CONTAINER%/{name}", Connection="STORAGE_ACCOUNT_CONNECTION_STRING")] BlobClient blobClient,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("AudioBlobUploadStart");

            var blobSasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(10));
            var audioBlobSasUri = blobClient.GenerateSasUri(blobSasBuilder);

            // TODO: pass a AudioFile instance to the orchestration function instead of just the blob sas uri

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AudioTranscriptionOrchestration), audioBlobSasUri);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        }
    }
}
