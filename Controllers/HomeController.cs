using GleamBoutiqueProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GleamBoutiqueProject.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly string connectionString;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }


        [Route("")]
        public IActionResult HomePage(User NewUser)
        {
            if (NewUser.Email != null)
            {
                // Set session variable with user's name
                string name = HttpContext.Session.GetString("UserName");
                ViewData["Message"] = $"Hello {name}!";
                return View("index", NewUser);
            }
            ViewData["Message"] = $"Hello Guest!";

            return View("index", NewUser);
        }

        public IActionResult ManagerHome(User NewUser)

        {
            string name = HttpContext.Session.GetString("ManageUser");
            ViewData["Message"] = $"Hello {name}!";
            return View(NewUser);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
