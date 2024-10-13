using System.Web.Http;
using Curupira.WindowsService.Services;
using Curupira.WindowsService.Attributes;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Curupira.WindowsService.Controllers
{
    [ApiKeyAuthorize]
    [RoutePrefix("api/v1/files")]
    public class FileUploadController : ApiController
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> UploadFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            foreach (var file in provider.Contents)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                var fileStream = await file.ReadAsStreamAsync();

                try
                {
                    _fileUploadService.UploadFile(fileStream, fileName);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error uploading file: {ex.Message}");
                }
            }

            return Ok("File uploaded successfully.");
        }
    }
}
