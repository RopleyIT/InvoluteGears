using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GearWeb.Shared;

namespace GearWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get(string id)
        {
            if (StringCache.Instance.Contains(id))
            {
                var svg = StringCache.Instance.Get(id);
                var buffer = Encoding.UTF8.GetBytes(svg);
                var stream = new MemoryStream(buffer);
                //var stream = new FileStream(filename);

                var result = new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = $"{id}.svg"
                };
                return result;
            }
            else
                return NotFound();
        }

        //[HttpPost("api/invzip")]
        //public async Task<IActionResult> CalcInvoluteSvgZip(GearParams gParams)
        //{
        //    GearProfiles profiles = CalcInvoluteImage(gParams);
        //    Stream zipStream = Zipper.ZipStringToStream(profiles.ShortName, profiles.SvgData);
        //    return File(zipStream, "application/zip");
        //}

    }
}
