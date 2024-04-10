using Microsoft.AspNetCore.Mvc;
using System;
using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using GleamBoutiqueProject.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using GleamBoutiqueProject.ViewModel;

namespace GleamBoutiqueProject.Controllers
{
    [NoCache]
    public class UserController : Controller
    {

        public IConfiguration _configuration;
        string connectionString = "";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthenticationService _authenticationService;

        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger,IAuthenticationService authenticationService,IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;

            _authenticationService = authenticationService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }



        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult SignIn()
        {
            string userEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(userEmail)) //guest mode
            {
                User newUser = new User();
                newUser.Password = "";
                return View(newUser);
            }
            else
            {
                return RedirectToAction("UserPage");
            }
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
                    string checkExistingEmailQuery = "SELECT COUNT(*) FROM [User] WHERE Email = @Email";

                    using (SqlCommand checkEmailCommand = new SqlCommand(checkExistingEmailQuery, connection))
                    {
                        checkEmailCommand.Parameters.AddWithValue("@Email", myUser.Email);
                        int existingEmailCount = (int)checkEmailCommand.ExecuteScalar();

                        if (existingEmailCount > 0)
                        {
                            // Email already exists, show error message
                            ModelState.AddModelError("Email", "This email already exists in the system. Please log in with this email.");

                            return View("SignUpS", myUser);
                        }
                    }

                    // Email doesn't exist, proceed with registration
                    string insertUserQuery = "INSERT INTO [User] VALUES (@Value1, @Value2, @Value3, @Value4)";

                    using (SqlCommand command = new SqlCommand(insertUserQuery, connection))
                    {
                        //Set the parameter values to the SQL
                        command.Parameters.AddWithValue("@Value1", myUser.FirstName);
                        command.Parameters.AddWithValue("@Value2", myUser.LastName);
                        command.Parameters.AddWithValue("@Value3", myUser.Email);
                        command.Parameters.AddWithValue("@Value4", myUser.Password);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            HttpContext.Session.SetString("UserName", myUser.FirstName);
                            HttpContext.Session.SetString("LastUserName", myUser.LastName);
                            HttpContext.Session.SetString("Email", myUser.Email);
                            connection.Close();
                            return RedirectToAction("Index", "Home", myUser);
                        }
                        else
                            return View("SignUpS", myUser);
                    }
                }
            }
            else
            {
                return View("SignUpS", myUser);
            }
        }



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
                    object mangPass = GetManagerPass(connection, myUser.Email);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string email = reader.GetString(2);
                            string password = reader.GetString(3);
                            if ((email == myUser.Email) && (password == myUser.Password))
                            {
                                if (myUser.Email == "sapir@gmail.com" && myUser.Password == mangPass)
                                {
                                    HttpContext.Session.SetString("ManageUser", reader.GetString(0));
                                    connection.Close();
                                    return RedirectToAction("ManagerHome", "Home");
                                }
                                else
                                {
                                    HttpContext.Session.SetString("UserName", reader.GetString(0));
                                    HttpContext.Session.SetString("LastUserName", reader.GetString(1));
                                    HttpContext.Session.SetString("Email", reader.GetString(2));
                                    HttpContext.Session.SetString("UserPassword", reader.GetString(3));

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
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View("SignIn", myUser);
        }

        private object GetManagerPass(SqlConnection connection, string userEmail)
        {
            
            string selectQuery = "SELECT Password FROM [User] WHERE Email = @userEmail";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@userEmail", userEmail);
                object result = selectCommand.ExecuteScalar();

                if (result != null)  //Manager found
                    return result;
                else
                    return null; // Manager not found
            }
        }


        public async Task<IActionResult> LogOut()
        {
            try
            {
                // Create a connection to your database using the connection string from appsettings.json
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("dbConnect")))
                {
                    connection.Open();

                    // Clear session data or perform any other necessary cleanup tasks
                    HttpContext.Session.Clear();

                    // Sign out the user using ASP.NET Core Identity's built-in method
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    _logger.LogInformation("User logged out successfully.");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during logout: {ex.Message}");
                return RedirectToAction("Error", "Home"); // Redirect to an error page or handle the error as needed
            }
        }

        public IActionResult UserPage()
        {

            var email = HttpContext.Session.GetString("Email");           
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(email);

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignIn");
            }

            else
            {
                // Assuming you have a method like GetOrdersForUser(email) that returns a List<Orders>
                var orders = GetOrdersForUser(email);
                var FirstNames = HttpContext.Session.GetString("UserName");

                var viewModelUser = new UserAndOrderViewModel
                {
                    OrdersLists = orders,
                    userToUpdate = new User
                    {
                        FirstName = HttpContext.Session.GetString("UserName"),
                        LastName = HttpContext.Session.GetString("LastUserName"),
                        Email = HttpContext.Session.GetString("Email"),
                        Password = HttpContext.Session.GetString("UserPassword")
                    }
                };

                return View(viewModelUser);
            }

        }

        [HttpPost]
        public IActionResult UpdateUserDetails(UserAndOrderViewModel updatedUser)
        {
            if (!ModelState.IsValid)
            {
                // If the model state is not valid, return the user to the form with the current input.
                return View("UserPage", updatedUser);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE [User] SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Password = @Password WHERE Email = @OriginalEmail";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", updatedUser.userToUpdate.FirstName);
                        command.Parameters.AddWithValue("@LastName", updatedUser.userToUpdate.LastName);
                        command.Parameters.AddWithValue("@Email", updatedUser.userToUpdate.Email);
                        command.Parameters.AddWithValue("@Password", updatedUser.userToUpdate.Password);
                        command.Parameters.AddWithValue("@OriginalEmail", HttpContext.Session.GetString("Email")); // Assuming you have the user's original email in session

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {

                            HttpContext.Session.SetString("Email", updatedUser.userToUpdate.Email);
                            HttpContext.Session.SetString("UserName", updatedUser.userToUpdate.FirstName); // Update first name in session
                            HttpContext.Session.SetString("LastUserName", updatedUser.userToUpdate.LastName);
                            HttpContext.Session.SetString("UserPassword", updatedUser.userToUpdate.Password);
                            return RedirectToAction("UserPage"); // Or wherever you want to redirect the user after successful update
                        }
                        else
                        {
                            ModelState.AddModelError("", "Update failed. Please try again.");
                            return View("UserPage", updatedUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("UserPage", updatedUser);
            }
        }

        private List<Orders> GetOrdersForUser(string userEmail)
        {
            List<Orders> orderItems = new List<Orders>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Correct the query to select all columns from the [Order] table where the EmailUser matches.
                string query = "SELECT OrderId, EmailUser, OrderDate, TotalPrice, ShipDate  FROM [Order] WHERE EmailUser = @UserEmail";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", userEmail);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Properly read each column value using the appropriate reader methods.
                            Orders item = new Orders
                            {
                                OrderId = reader.GetInt32(0),
                                Email = reader.GetString(1), 
                                OrderDate = reader.GetDateTime(2),
                                TotalPrice = reader.GetDecimal(3),
                                ShipDate = reader.GetDateTime(4) 
                            };
                            orderItems.Add(item);
                        }
                    }
                }
            }
            return orderItems;
        }


        public IActionResult ShippingTracker(int orderId)
        {
            Orders newOrder = new Orders();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM [Order] WHERE OrderId = @orderId";

                using (SqlCommand selectCommand = new SqlCommand(sqlQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@OrderId", orderId);
                    SqlDataReader reader = selectCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        newOrder.OrderId = reader.GetInt32(0);
                        newOrder.Email = reader.GetString(1);
                        newOrder.OrderDate = reader.GetDateTime(2);
                        newOrder.TotalPrice = reader.GetDecimal(3);
                        newOrder.ShipDate = reader.GetDateTime(4);
                    }
                    reader.Close();
                }
                connection.Close();
            }

            var currentDate = DateTime.Now;
            var shipDate = newOrder.ShipDate;
            var orderDate = newOrder.OrderDate;
            var totalDays = (shipDate - currentDate).Days;  ///1111
            var daysPassed = (currentDate - shipDate).Days;

            return View(newOrder);
        }

    }
}