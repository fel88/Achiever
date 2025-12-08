using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(SiteUserDto user)
        {
            var ctx = new AchieverContext();
            User f = null;
            foreach (var item in ctx.Users.ToArray())
            {
                if (!Request.IsLocal())                
                    if (item.Login == "local_admin")
                        continue;                

                if (item.Login == user.login && ComputeSha256Hash(user.password).ToLower() == item.Password.ToLower())
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
