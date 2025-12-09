using Achiever.Common.Model;
using Achiever.Controllers;
using Achiever.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Achiever.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        public class NewUserDto
        {
            public string login { get; set; }
            public string name { get; set; }
            public string password { get; set; }
        }

        [HttpPost("/api/[controller]")]
        public async Task<IActionResult> NewUser(NewUserDto dto)
        {

            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var ctx = new AchieverContext();

            ctx.Users.Add(new User()
            {
                Login = dto.login,
                Name = dto.name,
                Password = dto.password.ComputeSha256Hash()
            }
                );

            await ctx.SaveChangesAsync();

            return BadRequest();
        }

        [HttpDelete("/api/[controller]/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!Helper.IsAuthorized(HttpContext.Session))
            {
                return Unauthorized();
            }
            var user = Helper.GetUser(HttpContext.Session);
            if (!user.IsAdmin)
            {
                return Unauthorized();
            }

            var ctx = new AchieverContext();
            var ch = ctx.Users.Find(id);
            ctx.Users.Remove(ch);
            await ctx.SaveChangesAsync();
            return Ok();
        }
    }
}
