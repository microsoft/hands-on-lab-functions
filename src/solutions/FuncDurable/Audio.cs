using System.Text.Json.Serialization;

namespace FuncDurable
{
    public abstract class Audio
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    public class AudioFile : Audio
    {
        [JsonPropertyName("urlWithSasToken")]
        public Uri UrlWithSasToken { get; set; }
    }

    public class AudioTranscription : Audio
    {
        [JsonPropertyName("transcription")]
        public string Transcription { get; set; }
    }
}