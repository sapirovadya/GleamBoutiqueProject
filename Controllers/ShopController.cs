using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GleamBoutiqueProject.ViewModel;
using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GleamBoutiqueProject.Controllers
{
    public class ShopController : Controller
    {
        public List<string> PidCartList = new List<string>();


        public IConfiguration _configuration;
        string connectionString = "";
        public ShopController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult exs()
        {
            return View();
        }

        public IActionResult Shop()
        {
            ProductViewModel proViewModel = new ProductViewModel();
            proViewModel.productsList = new List<Product>();
            //proViewModel.cartProducts = new List<Product>();

            //SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Product";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    //Read from the SQL table
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Product newProduct = new Product();
                        newProduct.Pid = reader.GetString(0);
                        newProduct.PName = reader.GetString(1);
                        newProduct.OriginPrice = reader.GetInt32(2);
                        newProduct.Amount = reader.GetInt32(3);
                        newProduct.Notify_Count = reader.GetInt32(4);
                        newProduct.category = reader.GetString(5);
                        newProduct.Material = reader.GetString(6);
                        newProduct.Sale_price = reader.GetInt32(7);
                        newProduct.karat = reader.GetInt32(8);
                        //newProduct.ProImage = reader.GetString(9);

                        proViewModel.productsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View("shop", proViewModel);
        }

        public IActionResult FilterResults(List<string> categories, List<string> materials, List<int> karats, int? salePrice)
        {
            ProductViewModel proViewModel = new ProductViewModel();
            proViewModel.productsList = new List<Product>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sqlQuery = new StringBuilder("SELECT * FROM Product WHERE 1=1");

                // Handling multiple categories
                if (categories != null && categories.Any())
                {
                    var categoryParams = categories.Select((category, index) => $"@category{index}").ToList();
                    sqlQuery.Append(" AND category IN (");
                    sqlQuery.Append(string.Join(", ", categoryParams));
                    sqlQuery.Append(")");
                }

                if (materials != null && materials.Any())
                {
                    var materialsParams = materials.Select((material, index) => $"@material{index}").ToList();
                    sqlQuery.Append(" AND Material IN (");
                    sqlQuery.Append(string.Join(", ", materialsParams));
                    sqlQuery.Append(")");
                }

                if (karats != null && karats.Any())
                {
                    var karatsParams = karats.Select((karat, index) => $"@karat{index}").ToList();
                    sqlQuery.Append(" AND karat IN (");
                    sqlQuery.Append(string.Join(", ", karatsParams));
                    sqlQuery.Append(")");
                }

                if (salePrice.HasValue && salePrice != 0) // Filter out products with sale price equal to 0
                    sqlQuery.Append(" AND Sale_Price != 0");

                using (var command = new SqlCommand(sqlQuery.ToString(), connection))
                {
                    // Adding parameters for categories
                    for (int i = 0; i < categories.Count; i++)
                        command.Parameters.AddWithValue($"@category{i}", categories[i]);

                    for (int i = 0; i < materials.Count; i++)
                        command.Parameters.AddWithValue($"@material{i}", materials[i]);

                    for (int i = 0; i < karats.Count; i++)
                        command.Parameters.AddWithValue($"@karat{i}", karats[i]);

                    // Adding parameters for salePrice
                    if (salePrice.HasValue && salePrice != 0)
                        command.Parameters.AddWithValue("@salePrice", salePrice.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var newProduct = new Product
                            {
                                Pid = reader["Pid"].ToString(),
                                PName = reader["PName"].ToString(),
                                OriginPrice = Convert.ToInt32(reader["OriginPrice"]),
                                Amount = Convert.ToInt32(reader["Amount"]),
                                Notify_Count = Convert.ToInt32(reader["Notify_Count"]),
                                category = reader["category"].ToString(),
                                Material = reader["Material"].ToString(), // Assuming 'Material' is a string
                                Sale_price = Convert.ToInt32(reader["Sale_price"]), // Assuming 'Sale_price' is stored as int
                                karat = reader.IsDBNull(reader.GetOrdinal("karat")) ? 0 : reader.GetInt32(reader.GetOrdinal("karat")), // Assuming 'karat' is an int and can be null
                            };
                            proViewModel.productsList.Add(newProduct);
                        }
                    }
                }
            }
            return View("shop", proViewModel);
        }

        public IActionResult SearchProduct(string searchKeyword)
        {
            ProductViewModel proViewModel = new ProductViewModel();
            proViewModel.productsList = new List<Product>();
            // SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Product WHERE PName LIKE @proNameToSearch";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    // Set the parameter value
                    command.Parameters.AddWithValue("@proNameToSearch", "%" + searchKeyword + "%");

                    // Read from the SQL table
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Product newProduct = new Product();
                        newProduct.Pid = reader.GetString(0);
                        newProduct.PName = reader.GetString(1);
                        newProduct.OriginPrice = reader.GetInt32(2);
                        newProduct.Amount = reader.GetInt32(3);
                        newProduct.Notify_Count = reader.GetInt32(4);
                        newProduct.category = reader.GetString(5);
                        newProduct.Material = reader.GetString(6);
                        newProduct.Sale_price = reader.GetInt32(7);
                        newProduct.karat = reader.GetInt32(8);

                        proViewModel.productsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }

            return View("shop", proViewModel);
        }


        public IActionResult AddToCart(string proid, int amount, int stock)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int currentAmount = GetCartProductAmount(connection, userEmail, proid);
                    if (currentAmount != -1) // Product found in the cart
                    {
                        int newAmount = Math.Min(stock, currentAmount + amount);
                        UpdateCartProductAmount(connection, userEmail, proid, newAmount);
                    }
                    else // Product not found in the cart
                    {
                        InsertCartProduct(connection, proid, amount, userEmail);
                    }
                }
                // Return a success message
                return Json(new { successMessage = "The product has been successfully added to the cart!" });
            }
            catch (Exception ex)
            {
                // Handle exception
                return Json(new { errorMessage = "ERROR. Please try again" });
            }
        }


        private int GetCartProductAmount(SqlConnection connection, string userEmail, string proid)
        {
            string selectQuery = "SELECT proAmount FROM Cart WHERE userEmail = @UserEmail AND Proid = @proid";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@UserEmail", userEmail);
                selectCommand.Parameters.AddWithValue("@proid", proid);
                object result = selectCommand.ExecuteScalar();

                if (result != null)  //product found in the cart
                    return Convert.ToInt32(result);  //return 1
                else
                    return -1; // Product not found in the cart
            }
        }

        private void UpdateCartProductAmount(SqlConnection connection, string userEmail, string proid, int newAmount)
        {
            string updateQuery = "UPDATE Cart SET proAmount = @NewAmount WHERE UserEmail = @userEmail AND Proid = @proid";
            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@NewAmount", newAmount);
                updateCommand.Parameters.AddWithValue("@UserEmail", userEmail);
                updateCommand.Parameters.AddWithValue("@Proid", proid);
                updateCommand.ExecuteNonQuery();
            }
        }

        private void InsertCartProduct(SqlConnection connection, string proid, int amount, string userEmail)
        {
            string insertQuery = "INSERT INTO Cart (Proid, proAmount, UserEmail) VALUES (@Proid, @proAmount, @UserEmail)";
            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@Proid", proid);
                insertCommand.Parameters.AddWithValue("@proAmount", amount);
                insertCommand.Parameters.AddWithValue("@UserEmail", userEmail);
                insertCommand.ExecuteNonQuery();
            }
        }

        public IActionResult Cart()
        {
            // Retrieve the email of the current user from session
            string userEmail = HttpContext.Session.GetString("Email");

            // List to store cart items for all users
            List<CartItemViewModel> allCartItems = new List<CartItemViewModel>();

            // Check if user email is available
            if (!string.IsNullOrEmpty(userEmail))
            {
                // Retrieve cart items for the current user
                List<CartItemViewModel> userCartItems = GetCartItemsForUser(userEmail);
                allCartItems.AddRange(userCartItems);
            }
            // Pass the list of cart items to the view
            return View(allCartItems);
        }

        // Helper method to retrieve cart items for a specific user
        private List<CartItemViewModel> GetCartItemsForUser(string userEmail)
        {
            List<CartItemViewModel> cartItems = new List<CartItemViewModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT C.Proid, C.proAmount, P.PName, P.OriginPrice, P.Sale_Price FROM Cart C INNER JOIN Product P ON C.Proid = P.Pid WHERE C.UserEmail = @UserEmail";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", userEmail);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Create CartItemViewModel objects and add them to the list
                            CartItemViewModel item = new CartItemViewModel
                            {
                                ProId = reader.GetString(0),
                                ProAmount = reader.GetInt32(1),
                                PName = reader.GetString(2),
                                OriginPrice = reader.GetInt32(3),
                                salePrice = reader.GetInt32(4)
                            };
                            cartItems.Add(item);
                        }
                    }
                }
            }
            return cartItems;
        }

        public IActionResult ProductDetails(string id)
        {
            Product product = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Product WHERE Pid = @Pid";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    // Parameterize the query to prevent SQL injection
                    command.Parameters.AddWithValue("@Pid", id);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) // Assuming Pid is unique and there's only one product per Pid
                    {
                        product = new Product();
                        product.Pid = reader.GetString(0);
                        product.PName = reader.GetString(1);
                        product.OriginPrice = reader.GetInt32(2);
                        product.Amount = reader.GetInt32(3);
                        product.Notify_Count = reader.GetInt32(4);
                        product.category = reader.GetString(5);
                        product.Material = reader.GetString(6);
                        product.Sale_price = reader.GetInt32(7);
                        product.karat = reader.GetInt32(8);
                        // product.ProImage = reader.GetString(9); // Uncomment and handle if you're using product images
                    }
                    reader.Close();
                }
                connection.Close();
            }

            // Handle the case where no product is found
            if (product == null)
            {
                return NotFound();
            }

            return View("ProductDetails", product);
        }
    }
}