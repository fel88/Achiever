using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Achiever.Common.Model;
using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Achiever.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
       

        [HttpPost]
        public async Task<IActionResult> Login(SiteUserDto user)
        {
            var ctx = new AchieverContext();
            User f = null;
            foreach (var item in ctx.Users.ToArray())
            {
                //if (!Request.IsLocal())                
                if (item.Login == "local_admin")
                    if (ConfigLoader.ReadBoolSetting("allowLocalAdmin") != true)
                        continue;

                if (item.Login == user.login && user.password.ComputeSha256Hash().ToLower() == item.Password.ToLower())
                {
                    f = item;
                    break;
                }
            }


            if (f != null)
            {
                if (!f.Enabled && !f.IsAdmin)
                {
                    return BadRequest(new { code = 2, message = "disabled user" });
                }
                HttpContext.Session.SetObject("user", f);
                return Ok();
            }
            return BadRequest(new { code = 1, message = "user not found" });
        }

        [HttpGet("/api/[controller]/logout")]

        public IActionResult Logout()
        {
            HttpContext.Session.SetObject("user", null);
            return Redirect("/Login");
        }
    }

}
