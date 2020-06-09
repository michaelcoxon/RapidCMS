using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blazor.FileReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RapidCMS.Core.Abstractions.Handlers;
using RapidCMS.Core.Models.Request.Api;

namespace RapidCMS.Core.Controllers
{
    [ApiController]
    public class ApiFileUploadController<THandler> : ControllerBase
        where THandler : IFileUploadHandler
    {
        private readonly THandler _handler;

        public ApiFileUploadController(THandler handler)
        {
            _handler = handler;
        }

        [HttpPost("file/validate")]
        public async Task<ActionResult<IEnumerable<string>>> ValidateFileAsync([FromForm] UploadFileRequestModel fileInfo)
        {
            return Ok(await _handler.ValidateFileAsync(fileInfo));
        }

        [HttpPost("file")]
        public async Task<ActionResult<object>> SaveFileAsync([FromForm] UploadFileRequestModel fileInfo, [FromForm] IFormFile file)
        {
            if (DoesFileMatchFileInfo(fileInfo, file, out var downloadedFile))
            {
                try
                {
                    var result = await _handler.SaveFileAsync(fileInfo, downloadedFile);
                    return Ok(result);
                }
                catch { }
            }

            return BadRequest();
        }

        // TODO
        private bool DoesFileMatchFileInfo(IFileInfo fileInfo, IFormFile file, out Stream memoryStream)
        {
            memoryStream = file.OpenReadStream();

            return true;
        }
    }
}
