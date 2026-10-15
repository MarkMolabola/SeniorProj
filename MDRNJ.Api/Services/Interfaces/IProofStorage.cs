using MDRNJ.Shared.DTOs.Challenge;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    /// <summary>
    /// Where challenge proof videos / photos live. The app uploads straight to a
    /// short-lived URL (so big videos never pass through the API) and views them
    /// through short-lived links; only the two players are ever handed either.
    /// </summary>
    public interface IProofStorage
    {
        /// <summary>A URL the app can PUT the file to for the next few minutes.</summary>
        ProofUploadDto CreateUpload(string path, string contentType, long maxBytes);

        /// <summary>The stored file's size, or null if nothing was uploaded there.</summary>
        Task<long?> GetSizeAsync(string path);

        /// <summary>A link to view the file for the next hour.</summary>
        string CreateReadUrl(string path);

        Task DeleteAsync(string path);

        /// <summary>Deletes every file under a folder, e.g. one challenge's proofs.</summary>
        Task DeleteFolderAsync(string prefix);
    }

    public static class ProofPaths
    {
        /// <summary>challenges/{challengeId}/{userId}/{random}.ext — the folder says who may use it.</summary>
        public static string ForUpload(Guid challengeId, Guid userId, string extension) =>
            $"{UserFolder(challengeId, userId)}{Guid.NewGuid():N}{extension}";

        public static string UserFolder(Guid challengeId, Guid userId) => $"{ChallengeFolder(challengeId)}{userId}/";

        public static string ChallengeFolder(Guid challengeId) => $"challenges/{challengeId}/";
    }
}
