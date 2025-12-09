using Achiever.Common.Model;
using Achiever.Controllers;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Achiever.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CabinetController : ControllerBase
    {


        [HttpGet("/api/[controller]/badge/{id}")]
        public async Task<IActionResult> GetCompetitionBadge(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }

            using var ctx = AchieverContextHolder.GetContext();
            var ch = ctx.Challenges.Find(id);
            if (ch == null)
            {
                return BadRequest();
            }


            if (ch.BadgeSettings == null)
            {
                var str1 = @"<div>
                     " + ch.Name + @"
                     <svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 64 64"" width=""10%""><defs><linearGradient gradientTransform=""matrix(1.31117 0 0 1.30239 737.39 159.91)"" gradientUnits=""userSpaceOnUse"" id=""0"" y2=""-.599"" x2=""0"" y1=""45.47""><stop stop-color=""#ffc515"" /><stop offset=""1"" stop-color=""#ffd55b"" /></linearGradient></defs><g transform=""matrix(.85714 0 0 .85714-627.02-130.8)""><path d=""m797.94 212.01l-25.607-48c-.736-1.333-2.068-2.074-3.551-2.074-1.483 0-2.822.889-3.569 2.222l-25.417 48c-.598 1.185-.605 2.815.132 4 .737 1.185 1.921 1.778 3.404 1.778h51.02c1.483 0 2.821-.741 3.42-1.926.747-1.185.753-2.667.165-4"" fill=""url(#0)"" /><path d=""m-26.309 18.07c-1.18 0-2.135.968-2.135 2.129v12.82c0 1.176.948 2.129 2.135 2.129 1.183 0 2.135-.968 2.135-2.129v-12.82c0-1.176-.946-2.129-2.135-2.129zm0 21.348c-1.18 0-2.135.954-2.135 2.135 0 1.18.954 2.135 2.135 2.135 1.181 0 2.135-.954 2.135-2.135 0-1.18-.952-2.135-2.135-2.135z"" transform=""matrix(1.05196 0 0 1.05196 796.53 161.87)"" fill=""#000"" stroke=""#40330d"" 
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             fill-opacity="".75"" /></g></svg>

                     <p>Bagde not setted!</p>
                 </div>";
                return Content(str1, "image/svg+xml; charset=utf-8");

            }

            var b = ResourceFile.GetFileText("badge.svg");
            var txt = Helper.GetText(ch.BadgeSettings);
            var fs1 = Helper.GetFontSize(ch.BadgeSettings);
            var bc1 = Helper.GetBackColor(ch.BadgeSettings);
            var clr = Helper.GetColor(ch.BadgeSettings);
            var fs = 21;
            if (fs1 != null)
            {
                fs = fs1.Value;
            }


            var bdi = new BadgeDrawInfo()
            {
                FontSize = fs,
                BackColor = bc1 == null ? "#ffc600" : bc1,
                Color = clr == null ? "#0000ff" : clr,
                Hardness = Helper.GetHardness(ch.BadgeSettings)
            };
            //replace all
            b = b.Replace("@back1", bdi.BackColor);
            b = b.Replace("@bdi.Color", bdi.Color);
            b = b.Replace("@(bdi.FontSize)", bdi.FontSize.ToString());
            b = b.Replace("@bdi.Hardness", bdi.Hardness.ToString());

            StringBuilder sb = new StringBuilder();
            int yy = 133;
            foreach (var item in txt)
            {
                sb.AppendLine(@"<tspan sodipodi:role=""line""
				   id=""tspan7549""
				   x=""139.83978""
				   y=""" + yy + @""">" + item + "</tspan>");
                yy += 27;

            }
            b = b.Replace("@Lines", sb.ToString());
            b = b.Replace("hidden", "visible");
           /* b = @"<svg>
  <rect id=""box""  fill="""+ bdi.BackColor + @"""  stroke="""+ bdi .Color+ @""" x=""0"" y=""0"" width=""450"" height=""150""/>
<!-- The Text -->
    <text 
        x=""50%"" 
        y=""50%"" 
        dominant-baseline=""middle"" 
        text-anchor=""middle"" 
        fill=""black"" 
        font-family=""sans-serif"" 
        font-size=""20px"">
        " + txt[0] +@"
    </text>
</svg>";*/
            return Content(b, "image/svg+xml; charset=utf-8");
        }
    }
}
