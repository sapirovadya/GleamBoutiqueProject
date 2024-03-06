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



        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult SignIn()
        {
            User newUser = new User();
            newUser.Password = "";
            return View(newUser);
        }

        //public IActionResult UserSign(User myUser)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            string sqlQuery = "INSERT INTO [User] VALUES (@Value1, @Value2, @Value3, @Value4)";

        //            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
        //            {
        //                //Set the parameter values to the SQL
        //                command.Parameters.AddWithValue("@Value1", myUser.FirstName);
        //                command.Parameters.AddWithValue("@Value2", myUser.LastName);
        //                command.Parameters.AddWithValue("@Value3", myUser.Email);
        //                command.Parameters.AddWithValue("@Value4", myUser.Password);

        //                int rowsAffected = command.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                    return RedirectToAction("Index", "Home", myUser);
        //                else

        //                    return View("SignInUp", myUser);
        //            }
        //            connection.Close();
        //        }
        //    }
        //    else
        //    {
        //        return View("SignInUp", myUser);
        //    }
        //}


        public IActionResult SignIn(User myUser)
        {
            // Check if the ModelState is valid for the sign-in form
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "SELECT COUNT(*) FROM [User] WHERE Email = @Email AND Password = @Password";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Email", myUser.Email);
                        command.Parameters.AddWithValue("@Password", myUser.Password);

                        int count = (int)command.ExecuteScalar();
                        if (count > 0)
                        {
                            // Authentication successful
                            // Redirect to the home page or perform other actions
                            return RedirectToAction("Index", "Home",myUser);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid email or password");
                            return View("SignIn", myUser);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                        return View("SignIn", myUser);
                    }
                }
            }
            else
            {
                // ModelState is not valid, return to the sign-in form
                return View("SignIn", myUser);
            }
        }

    }
}
