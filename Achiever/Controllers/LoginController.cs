using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Achiever.Common.Model;
using Achiever.Dtos;
using Achiever.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Telegram.Bot;

namespace Achiever.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{

		
		[HttpPost("/api/[controller]/otp")]
		public async Task<IActionResult> OTP(SiteUserDto user)
		{
			using var ctx = AchieverContextHolder.GetContext();
			
			foreach (var item in ctx.Users.ToArray())
			{
				if (item.Login == "local_admin")
					if (ConfigLoader.ReadBoolSetting("allowLocalAdmin") != true)
						continue;

				var otp1 = item.GetXmlProp("otpTimestamp");

				if (otp1 == null)
					continue;

				if (DateTime.UtcNow.Subtract(DateTime.Parse(otp1)).TotalSeconds >= 60)
					continue;

				if (item.GetXmlProp("otpUsed") == "true")
					continue;		

				if (item.Login == user.login && user.password == item.GetXmlProp("otp"))
				{
					item.UpdateXmlProp("otpUsed", "true");
					ctx.SaveChanges();
					
					HttpContext.Session.SetObject("user", item);
					return Ok();				
					
				}
			}
			return BadRequest(new { code = 1, message = "login error" }); 

		}

		[HttpPost("/api/[controller]/login")]
		public async Task<IActionResult> Login(SiteUserDto user)
		{
			using var ctx = AchieverContextHolder.GetContext();
			User f = null;
			var fp = ctx.GetDatabaseFilePath();
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
				if (Program.bot.Bot != null && f.TelegramChatId != null)// check use2FATelegram
				{
					var otp = OtpGenerator.GenerateNumericOTP(6);
					var res = f.GetXmlProp("otpTimestamp");
					if (res == null || DateTime.UtcNow.Subtract(DateTime.Parse(res)).TotalSeconds > 60)
					{
						f.UpdateXmlProp("otpUsed", "false");
						f.UpdateXmlProp("otp", otp);
						f.UpdateXmlProp("otpTimestamp", DateTime.UtcNow.ToString());
						await ctx.SaveChangesAsync();
						await Program.bot.Bot.SendTextMessageAsync(chatId: f.TelegramChatId, text:
					$"OTP: {otp}", cancellationToken: Program.bot.CancellationToken);

                        var retData = new
                        {
                            message = "otp",
                            timestamp = DateTime.UtcNow
                        };
                        return Ok(retData);
                    }

				}
				else
				{
					HttpContext.Session.SetObject("user", f);
                }
                var resultData = new
                {
                    message = "passed",                    
                    timestamp = DateTime.UtcNow
                };
                return Ok(resultData);
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
