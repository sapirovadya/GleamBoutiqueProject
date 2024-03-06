using GleamBoutiqueProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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



        //public IActionResult Index()
        //{
        //    return View();
        //}

        //public IActionResult Privacy()
        //{
        //    return View();
        //}






        [Route("")]
        public IActionResult HomePage(User NewUser) {
            //User MyUser = NewUser;
            if (NewUser == null)
            {
                return View("index");
            }
            return View("index", NewUser);
        }

        


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
