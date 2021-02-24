using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MulipleFileUpload.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class JsonWithFileRequestController
    {
        private const long MaxFileSize = 1024L * 1024L * 1024L; // 1GB

        [HttpPost]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        public async Task Upload(
            [FromForm] string json,
            [FromForm] IEnumerable<IFormFile> fileList1,
            [FromForm] IEnumerable<IFormFile> fileList2)
        {
            // Jsonパース
            TextModel model = JsonConvert.DeserializeObject<TextModel>(json);
            // 保存など
            foreach (var file in fileList1) 
            {
                var stream = file.OpenReadStream();
            }
        }

        class TextModel
        {
            public string Text { get; set; }
        }
    }
}
