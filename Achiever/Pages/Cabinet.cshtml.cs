using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Achiever.Pages
{
    public class CabinetModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (!Helper.IsAuthorized(HttpContext.Session)) return Redirect("Login");
            return null;
        }
    }
}