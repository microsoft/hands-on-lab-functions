using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;

namespace FuncDurable
{
    public static class AudioTranscriptionOrchestration
    {
        [Function(nameof(AudioBlobUploadStart))]
        public static async Task AudioBlobUploadStart(
                [BlobTrigger("%STORAGE_ACCOUNT_CONTAINER%/{name}", Connection = "STORAGE_ACCOUNT_CONNECTION_STRING")] BlobClient blobClient,
                [DurableClient] DurableTaskClient client,
                FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("AudioBlobUploadStart");

            var blobSasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(10));
            var audioBlobSasUri = blobClient.GenerateSasUri(blobSasBuilder);

            var audioFile = new AudioFile
            {
                Id = Guid.NewGuid().ToString(),
                Path = blobClient.Uri.ToString(),
                UrlWithSasToken = audioBlobSasUri
            };

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AudioTranscriptionOrchestration), audioFile);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        }

        [Function(nameof(AudioTranscriptionOrchestration))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context, 
            AudioFile audioFile)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(AudioTranscriptionOrchestration));
            logger.LogInformation("Processing audio file");

            var jobUrl = await context.CallActivityAsync<string>(nameof(StartTranscription), audioFile.UrlWithSasToken);

            DateTime endTime = context.CurrentUtcDateTime.AddMinutes(2);

            while (context.CurrentUtcDateTime < endTime)
            {
                // Check transcription
                // if (!context.IsReplaying) { logger.LogInformation($"Checking current weather conditions for {input.Location} at {context.CurrentUtcDateTime}."); }

                string? transcription = await context.CallActivityAsync<string?>(nameof(GetTranscription), jobUrl);
                logger.LogInformation($"transcription: {transcription}");

                if (transcription != null)
                {
                    // It's not raining! Or snowing. Or misting. Tell our user to take advantage of it.
                    // if (!context.IsReplaying) { logger.LogInformation($"Detected clear weather for {input.Location}. Notifying {input.Phone}."); }
                    var audioTranscription = new AudioTranscription
                    {
                        Id = audioFile.Id,
                        Path = audioFile.Path,
                        Transcription = transcription
                    };
                    await context.CallActivityAsync(nameof(SaveTranscription), audioTranscription);
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
                var url = $"{Environment.GetEnvironmentVariable("SPEECH_TO_TEXT_ENDPOINT")}speechtotext/v3.1/transcriptions";
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

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var job = JsonSerializer.Deserialize<TranscriptionJob>(serializedJob, options);

                logger.LogInformation($"_____________ job {job?.Self}");

                return job.Self;
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

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var job = JsonSerializer.Deserialize<TranscriptionJob>(serializedJob, options);

                logger.LogInformation($"job status {job?.Status}");

                if (job != null && job.Status != "Succeeded")
                {
                    return null;
                }

                var files = job?.Links.Files;

                HttpResponseMessage resultsHttpResponse = await httpClient.GetAsync(files);
                var serializedJobResults = await resultsHttpResponse.Content.ReadAsStringAsync();
                var transcriptionResult = JsonSerializer.Deserialize<TranscriptionResult>(serializedJobResults, options);
                var transcriptionFileUrl = transcriptionResult?.Values.Where(value => value.Kind == "Transcription").First().Links.ContentUrl;

                if (transcriptionFileUrl == null)
                {
                    throw new Exception("Transcription file url not found");
                }

                HttpResponseMessage transcriptionDetailsHttpResponse = await httpClient.GetAsync(transcriptionFileUrl);
                var serializedTranscriptionDetails = await transcriptionDetailsHttpResponse.Content.ReadAsStringAsync();
                var transcriptionDetails = JsonSerializer.Deserialize<TranscriptionDetails>(serializedTranscriptionDetails, options);
                var transcription = transcriptionDetails?.CombinedRecognizedPhrases.First().Display;

                if (transcription == null)
                {
                    throw new Exception("Transcription result not found");
                }

                logger.LogInformation($"transcription {transcription}");

                return transcription;
            }
        }

        [Function(nameof(SaveTranscription))]
        [CosmosDBOutput("%COSMOS_DB_DATABASE_NAME%",
                         "%COSMOS_DB_CONTAINER_ID%",
                         Connection = "COSMOS_DB_CONNECTION_STRING_SETTING",
                         CreateIfNotExists = true)]
        public static AudioTranscription SaveTranscription([ActivityTrigger] AudioTranscription audioTranscription, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(SaveTranscription));
            logger.LogInformation("Saving the audio transcription...");
         
            return audioTranscription;
        }
    }
}
