using GearWeb.Shared;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;

namespace GearWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly StringCache cache;
        public DownloadController(StringCache sharedCache)
            => cache = sharedCache;

        [HttpGet]
        public ActionResult Get(string id)
        {
            if (cache.Contains(id))
            {
                var buffer = Encoding.UTF8.GetBytes(cache.Get(id));
                var stream = new MemoryStream(buffer);
                //var stream = new FileStream(filename);

                var result = new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = id
                };
                return result;
            }
            else
                return NotFound();
        }
    }
}
