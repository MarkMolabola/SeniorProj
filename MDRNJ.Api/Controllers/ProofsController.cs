using MDRNJ.Api.Services;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    /// <summary>
    /// Local-storage stand-in for blob SAS URLs (development only). The signed token in
    /// the URL is the permission, like a SAS, so these don't need a login.
    /// </summary>
    [ApiController]
    [Route("api/proofs/local")]
    [AllowAnonymous]
    public class ProofsController : ControllerBase
    {
        private readonly LocalProofStorage? _local;

        public ProofsController(IProofStorage storage) => _local = storage as LocalProofStorage;

        [HttpPut("{token}")]
        [RequestSizeLimit(DuelRules.MaxVideoBytes)]
        public async Task<IActionResult> Upload(string token)
        {
            if (_local?.ReadUploadToken(token) is not var (path, maxBytes))
                return NotFound();
            if (Request.ContentLength > maxBytes)
                return StatusCode(StatusCodes.Status413PayloadTooLarge);

            var full = _local.FullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            await using (var file = System.IO.File.Create(full))
                await Request.Body.CopyToAsync(file);
            return Created();
        }

        [HttpGet("{token}")]
        public IActionResult View(string token)
        {
            if (_local?.ReadViewToken(token) is not string path)
                return NotFound();

            var full = _local.FullPath(path);
            if (!System.IO.File.Exists(full))
                return NotFound();

            if (!new FileExtensionContentTypeProvider().TryGetContentType(full, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(full, contentType, enableRangeProcessing: true);
        }
    }
}
