using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

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

        public IActionResult SignUpS()
        {
            User newUsers = new User();
            newUsers.Password = "";
            return View(newUsers);
        }

        public IActionResult Registers(User myUser)
        {
            if (ModelState.IsValid)
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
                            return RedirectToAction("Index", "Home", myUser);
                        else

                            return View("SignUpS", myUser);
                    }
                    connection.Close();
                }
            }
            else
            {
                return View("SignUpS", myUser);
            }
        }



        //// the best
        public IActionResult LogIn(User myUser)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM [User] WHERE Email = @Email AND Password = @Password";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Email", myUser.Email);
                    command.Parameters.AddWithValue("@Password", myUser.Password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string email = reader.GetString(2);
                            string password = reader.GetString(3);
                            if ((email == myUser.Email) && (password == myUser.Password))
                            {
                                if (myUser.Email == "sapir@gmail.com" && myUser.Password == "s1234")
                                {
                                    HttpContext.Session.SetString("ManageUser", reader.GetString(0));
                                    connection.Close();
                                    return RedirectToAction("ManagerHome", "Home");
                                }
                                else
                                {
                                    HttpContext.Session.SetString("UserName", reader.GetString(0));
                                    HttpContext.Session.SetString("Email", reader.GetString(2));

                                    connection.Close();
                                    return RedirectToAction("Index", "Home", myUser);
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid email or password");
                                connection.Close();
                                return View("SignIn", myUser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);

                return View("SignIn", myUser);
            }
            ModelState.AddModelError(string.Empty, "User does not exist");
            return View("SignIn", myUser);
        }


    }
}
