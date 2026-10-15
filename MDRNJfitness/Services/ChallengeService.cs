using MDRNJ.Shared.DTOs.Challenge;
using MDRNJfitness.Helpers;
using MDRNJfitness.Helpers.Extensions;
using System.Net.Http.Headers;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Uploads go straight to storage (signed URL, no login header) and videos are big:
        /// a separate client with a long timeout.
        /// </summary>
        private static readonly HttpClient Uploader = new() { Timeout = TimeSpan.FromMinutes(20) };

        public ChallengeService(HttpClient http) => _http = http;

        public Task<List<ChallengeDto>> GetMyChallengesAsync() =>
            _http.GetAsync<List<ChallengeDto>>("api/challenges/my")!;

        public Task<ChallengeDto> CreateAsync(CreateChallengeDto dto) =>
            _http.PostAsync<ChallengeDto>("api/challenges", dto)!;

        public Task<ChallengeDto> AcceptAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/accept", new { })!;

        public Task<ChallengeDto> DeclineAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/decline", new { })!;

        public Task<ChallengeDto> CancelAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/cancel", new { })!;

        public Task<ChallengeDto> SubmitAttemptAsync(Guid challengeId, float value, string? proofPath = null) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/attempts",
                new SubmitAttemptDto { Value = value, ProofPath = proofPath })!;

        public async Task<string> UploadProofAsync(Guid challengeId, FileResult file)
        {
            var contentType = ContentTypeOf(file)
                ?? throw new InvalidOperationException("That file type isn't supported. Use an MP4/MOV video or a JPG/PNG photo.");

            // Blob storage needs the length up front, so make sure the stream can report it
            await using var stream = await OpenSeekableAsync(file);

            var upload = await _http.PostAsync<ProofUploadDto>($"api/challenges/{challengeId}/proof-upload",
                new ProofUploadRequestDto { ContentType = contentType })
                ?? throw new InvalidOperationException("Couldn't start the upload.");
            if (stream.Length > upload.MaxBytes)
                throw new InvalidOperationException(
                    $"That file is {stream.Length / (1024 * 1024)} MB; the limit is {upload.MaxBytes / (1024 * 1024)} MB. Try a shorter clip.");

            using var request = new HttpRequestMessage(HttpMethod.Put, Resolve(upload.UploadUrl))
            {
                Content = new StreamContent(stream)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            foreach (var (name, value) in upload.Headers)
                request.Headers.TryAddWithoutValidation(name, value);

            using var response = await Uploader.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"The upload failed ({(int)response.StatusCode}). Try again.");
            return upload.ProofPath;
        }

        public async Task<Uri> GetProofLinkAsync(Guid challengeId, Guid attemptId)
        {
            var link = await _http.GetAsync<ProofLinkDto>($"api/challenges/{challengeId}/attempts/{attemptId}/proof")
                ?? throw new InvalidOperationException("Proof not found.");
            return Resolve(link.Url);
        }

        public Task<ChallengeDto> ApproveAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/approve", new { })!;

        public Task<ChallengeDto> DisputeAsync(Guid challengeId, string? reason) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/dispute", new DisputeChallengeDto { Reason = reason })!;

        /// <summary>Cloud storage hands out absolute URLs; local dev storage, ones relative to the API.</summary>
        private static Uri Resolve(string url) => new(new Uri(ApiConstants.BaseUrl), url);

        private static string? ContentTypeOf(FileResult file)
        {
            if (!string.IsNullOrEmpty(file.ContentType) && file.ContentType.Contains('/'))
                return file.ContentType.ToLowerInvariant();
            return Path.GetExtension(file.FileName).ToLowerInvariant() switch
            {
                ".mp4" or ".m4v" => "video/mp4",
                ".mov" => "video/quicktime",
                ".3gp" => "video/3gpp",
                ".webm" => "video/webm",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".heic" => "image/heic",
                ".webp" => "image/webp",
                _ => null
            };
        }

        private static async Task<Stream> OpenSeekableAsync(FileResult file)
        {
            var stream = await file.OpenReadAsync();
            if (stream.CanSeek) return stream;

            var copy = Path.Combine(FileSystem.CacheDirectory, $"proof-{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");
            await using (var target = File.Create(copy))
            await using (stream)
                await stream.CopyToAsync(target);
            return new FileStream(copy, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.DeleteOnClose);
        }

        public Task MarkResultSeenAsync(Guid challengeId) =>
            _http.PostAsync($"api/challenges/{challengeId}/seen", new { });
    }
}
