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
                return View("UserPage",userEmail);
            }
        }

        public IActionResult UserPage(string email)
        {
            return View();
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
                                    HttpContext.Session.SetString("LastUserName", reader.GetString(1));
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

    }
}