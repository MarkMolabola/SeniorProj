using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Challenge;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Development fallback when Storage:ConnectionString isn't set: proof files in a
    /// folder next to the API, uploaded and viewed through ProofsController with
    /// signed, expiring tokens standing in for SAS URLs.
    /// </summary>
    public class LocalProofStorage : IProofStorage
    {
        private static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan ReadWindow = TimeSpan.FromHours(1);

        private readonly string _root;
        private readonly ITimeLimitedDataProtector _uploadTokens;
        private readonly ITimeLimitedDataProtector _readTokens;

        public LocalProofStorage(string root, IDataProtectionProvider protection)
        {
            _root = Path.GetFullPath(root);
            Directory.CreateDirectory(_root);
            _uploadTokens = protection.CreateProtector("ProofUpload").ToTimeLimitedDataProtector();
            _readTokens = protection.CreateProtector("ProofRead").ToTimeLimitedDataProtector();
        }

        public ProofUploadDto CreateUpload(string path, string contentType, long maxBytes) => new()
        {
            UploadUrl = $"api/proofs/local/{_uploadTokens.Protect($"{path}|{maxBytes}", UploadWindow)}",
            ProofPath = path,
            MaxBytes = maxBytes
        };

        public Task<long?> GetSizeAsync(string path)
        {
            var file = new FileInfo(FullPath(path));
            return Task.FromResult<long?>(file.Exists ? file.Length : null);
        }

        public string CreateReadUrl(string path) =>
            $"api/proofs/local/{_readTokens.Protect(path, ReadWindow)}";

        public Task DeleteAsync(string path)
        {
            File.Delete(FullPath(path));
            return Task.CompletedTask;
        }

        public Task DeleteFolderAsync(string prefix)
        {
            var folder = FullPath(prefix);
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
            return Task.CompletedTask;
        }

        // ── Used by ProofsController ────────────────────────────────────────

        /// <summary>The path and size limit an upload token allows, or null if invalid / expired.</summary>
        public (string Path, long MaxBytes)? ReadUploadToken(string token)
        {
            try
            {
                var parts = _uploadTokens.Unprotect(token).Split('|');
                return (parts[0], long.Parse(parts[1]));
            }
            catch
            {
                return null;
            }
        }

        public string? ReadViewToken(string token)
        {
            try { return _readTokens.Unprotect(token); }
            catch { return null; }
        }

        /// <summary>Paths come from our own tokens, but never let one escape the root.</summary>
        public string FullPath(string path)
        {
            var full = Path.GetFullPath(Path.Combine(_root, path));
            if (!full.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid proof path.");
            return full;
        }
    }
}
