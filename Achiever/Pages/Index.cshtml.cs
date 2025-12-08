using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Achiever;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Achiever.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }



        public IActionResult OnGet()
        {
            if (!Helper.IsAuthorized(HttpContext.Session)) return Redirect("Login");
            return null;
        }
    }


}
