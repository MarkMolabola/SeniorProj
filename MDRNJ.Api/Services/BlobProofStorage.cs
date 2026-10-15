using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Proof files in a private Azure Blob Storage container. The app gets SAS URLs:
    /// write-only for the upload, read-only for viewing. Needs a connection string with
    /// an account key (Storage:ConnectionString) so the API can sign them.
    /// </summary>
    public class BlobProofStorage : IProofStorage
    {
        private static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan ReadWindow = TimeSpan.FromHours(1);

        private readonly BlobContainerClient _container;

        public BlobProofStorage(string connectionString, string containerName)
        {
            _container = new BlobContainerClient(connectionString, containerName);
            _container.CreateIfNotExists(PublicAccessType.None);
        }

        public ProofUploadDto CreateUpload(string path, string contentType, long maxBytes)
        {
            var uri = _container.GetBlobClient(path).GenerateSasUri(
                BlobSasPermissions.Create | BlobSasPermissions.Write,
                DateTimeOffset.UtcNow.Add(UploadWindow));

            return new ProofUploadDto
            {
                UploadUrl = uri.ToString(),
                ProofPath = path,
                MaxBytes = maxBytes,
                Headers = new Dictionary<string, string>
                {
                    ["x-ms-blob-type"] = "BlockBlob",
                    ["x-ms-blob-content-type"] = contentType
                }
            };
        }

        public async Task<long?> GetSizeAsync(string path)
        {
            var blob = _container.GetBlobClient(path);
            if (!await blob.ExistsAsync()) return null;
            return (await blob.GetPropertiesAsync()).Value.ContentLength;
        }

        public string CreateReadUrl(string path) =>
            _container.GetBlobClient(path)
                .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ReadWindow))
                .ToString();

        public Task DeleteAsync(string path) =>
            _container.GetBlobClient(path).DeleteIfExistsAsync();

        public async Task DeleteFolderAsync(string prefix)
        {
            await foreach (var blob in _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, default))
                await _container.GetBlobClient(blob.Name).DeleteIfExistsAsync();
        }
    }
}
