using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
 using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GleamBoutiqueProject.Controllers
{
    public class UserController : Controller
    {

        public IConfiguration _configuration;
        string connectionString = "";
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignInUp()
        {
            User newUser = new User();
            return View(newUser);
        }

        public IActionResult UserSign(User myUser)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "INSERT INTO [User] VALUES (@Value1, @Value2, @Value3, @Value4)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    //Set the parameter values to the SQL
                    command.Parameters.AddWithValue("@Value1", myUser.FirstName);
                    command.Parameters.AddWithValue("@Value2", myUser.LastName);
                    command.Parameters.AddWithValue("@Value3", myUser.Email);
                    command.Parameters.AddWithValue("@Value4", myUser.Password);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return View("index", myUser);
                    else
                        return View("SignInUp", myUser);
                }
                connection.Close();
            }
        }

    }
}
