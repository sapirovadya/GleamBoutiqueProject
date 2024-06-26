﻿using Microsoft.AspNetCore.Mvc;
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
using System.Security.Cryptography;
using System.Text.Json;

namespace GleamBoutiqueProject.Controllers
{
    public class ShopController : Controller
    {
        public List<CartItemViewModel> allCartItems;
        public static List<CartItem> guestList = new List<CartItem>();
        public static List<CartItem> buynowList = new List<CartItem>();
        List<CartItem> UserList = new List<CartItem>(); 

        public static List<Product> productsList = new List<Product>();

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
            buynowList.Clear();
            productsList = new List<Product>();
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

                        productsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View("shop", productsList);
        }

        public IActionResult FilterResults(List<string> categories, List<string> materials, List<int> karats, int? salePrice)
        {
            productsList = new List<Product>();

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
                            //proViewModel.productsList.Add(newProduct);
                            productsList.Add(newProduct);
                        }
                    }
                }
            }
            return View("shop", productsList);
        }

        public IActionResult sortProduct(string sortOption)
        {
            switch (sortOption)
            {
                case "lowToHigh":
                    productsList = productsList.OrderBy(p => p.Sale_price != 0 ? p.Sale_price : p.OriginPrice).ToList();
                    break;
                case "highToLow":
                    productsList = productsList.OrderByDescending(p => p.Sale_price != 0 ? p.Sale_price : p.OriginPrice).ToList();
                    break;
                default:
                    break;
            }
            return View("shop", productsList);
        }

        public IActionResult SearchProduct(string searchKeyword)
        {
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
                    List<Product> searchResults = new List<Product>(); // Create a list to store search results
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

                        searchResults.Add(newProduct); // Add each product to the search results list
                    }
                    reader.Close();

                    return View("shop", searchResults);
                }
            }
        }



        public IActionResult AddToCart(string proid, int amount,int stock)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    if (string.IsNullOrEmpty(userEmail)) //guest mode
                    {
                        foreach (CartItem item in guestList)
                        {
                            if (item.ProId == proid)
                            {
                                if (item.ProAmount + amount <= item.ProStock)
                                {
                                    item.ProAmount += amount;
                                    return Json(new { successMessage = "The product has been successfully added to the cart!" });
                                }
                                else
                                    return Json(new { successMessage = "You have reached the maximum quantity of the product available in stock. There are " + (stock- item.ProAmount) + " more available in stock." });
                            }    
                        }
                        InsertProductToCartList(proid, amount, connection);
                        return Json(new { successMessage = "The product has been successfully added to the cart!" });     
                    }
                    else
                    {
                        int currentAmount = GetCartProductAmount(connection, userEmail, proid);
                        if (currentAmount != -1)// Product found in the cart
                        {
                            if (currentAmount + amount <= stock)
                            {
                                UpdateCartProductAmount(connection, userEmail, proid, currentAmount + amount);
                                return Json(new { successMessage = "The product has been successfully added to the cart!" });
                            }
                            else
                                return Json(new { successMessage = "You have reached the maximum quantity of the product available in stock. There are " + (stock- currentAmount) + " more available in stock."});

                        }
                        // Product not found in the cart
                        InsertCartProduct(connection, proid, amount, userEmail);
                        return Json(new { successMessage = "The product has been successfully added to the cart!" });

                    }
                }
        }
            catch (Exception ex)
            {
                return Json(new { errorMessage = ex });
            }
        }

        private Product GetProduct(SqlConnection connection, string proid)
        {
            Product newPro = new Product();

            string selectQuery = "SELECT * FROM Product WHERE Pid = @proid";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@proid", proid);
                SqlDataReader reader = selectCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        newPro.Pid = reader.GetString(0);
                        newPro.PName = reader.GetString(1);
                        newPro.OriginPrice = reader.GetInt32(2);
                        newPro.Amount = reader.GetInt32(3);
                        newPro.Notify_Count = reader.GetInt32(4);
                        newPro.category = reader.GetString(5);
                        newPro.Material = reader.GetString(6);
                        newPro.Sale_price = reader.GetInt32(7);
                        newPro.karat = reader.GetInt32(8);
                    }
                    reader.Close();
            }
            return newPro;

        }
        private void InsertProductToCartList(string proid,int amount, SqlConnection connection)
        {
            Product NewPro = new Product(GetProduct(connection, proid));
            CartItem NewItem = new CartItem(NewPro.Pid, amount, NewPro.PName, NewPro.OriginPrice, NewPro.Sale_price, NewPro.Amount);
            guestList.Add(NewItem);
        }
        public IActionResult UpdateCartProductAmountFromCart(string proid, int amount)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int productStock = GetProductStock(connection, proid);
                    if (amount > productStock)   //Max stock
                        return Json(new { successMessage = "No more left in stock", Quantity = productStock });
                    else   
                    {
                        if (userEmail != null)  //registered user
                            UpdateCartProductAmount(connection, userEmail, proid, amount);
                        else  //Guest mode
                            UpdateCartProductAmountForGuest(proid, amount);
                        return Json(new { successMessage = "The product amount update successfully!", Quantity = productStock });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex });
            }
        }

        private void UpdateCartProductAmountForGuest(string proid, int amount)
        {
            foreach (CartItem item in guestList)
            {
                if (item.ProId == proid)
                    item.ProAmount = amount;
            }
        }

  
        private int GetProductStock(SqlConnection connection, string proid)
        {
            string selectQuery = "SELECT Amount FROM Product WHERE Pid = @proid";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@proid", proid);
                object result = selectCommand.ExecuteScalar();

                if (result != null)  //product found
                    return Convert.ToInt32(result);
                else
                    return -1; // Product not found in the cart
            }
        }

        public IActionResult DeleteProduct(string proid)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            try
            {
                if (userEmail != null)  //registered user
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        DeleteProductFromCart(connection, proid, userEmail);
                    }
                }
                else   //Guest mode
                    DeleteProductForGuest(proid);
                return Json(new { successMessage = "The product deleted" });
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex });
            }
        }
        private void DeleteProductForGuest(string proid)
        {
            foreach (CartItem item in guestList)
            {
                if (item.ProId == proid)
                {
                    guestList.Remove(item);
                    break;
                }
            }
        }

        private void DeleteProductFromCart(SqlConnection connection, string proid,string userEmail)
        {
            string selectQuery = "DELETE FROM Cart WHERE Proid = @proid AND UserEmail = @userEmail";
            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
            {
                selectCommand.Parameters.AddWithValue("@proid", proid);
                selectCommand.Parameters.AddWithValue("@userEmail", userEmail);
                selectCommand.ExecuteScalar();
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
            string userEmail = HttpContext.Session.GetString("Email");
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(userEmail);

            CartItemViewModel viewModel = new CartItemViewModel();

            if (!string.IsNullOrEmpty(userEmail))
            {
                List<CartItem> userCartItems = GetCartItemsForUser(userEmail);
                viewModel.UserCart = userCartItems;
            }
            else
            {
                List<CartItem> guestCartItems = GetGuestCartItems(); // Get guest cart items
                viewModel.guestCart = guestCartItems;

                // Serialize and store the guestCart in the session if not empty
                if (viewModel.guestCart != null && viewModel.guestCart.Count > 0)
                {
                    var cartJson = System.Text.Json.JsonSerializer.Serialize(viewModel.guestCart);
                    HttpContext.Session.SetString("GuestCart", cartJson);
                }
            }

            return View(viewModel);
        }

        // Helper method to retrieve cart items for a specific user
        private List<CartItem> GetCartItemsForUser(string userEmail)
        {
            List<CartItem> cartItems = new List<CartItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT C.Proid, C.proAmount, P.PName, P.OriginPrice, P.Sale_Price, P.Amount FROM Cart C INNER JOIN Product P ON C.Proid = P.Pid WHERE C.UserEmail = @UserEmail";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserEmail", userEmail);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CartItem item = new CartItem(reader.GetString(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3),reader.GetInt32(4), reader.GetInt32(5));
                            cartItems.Add(item);
                        }
                    }
                }
            }
            return cartItems;
        }

        public IActionResult ProductDetails(string id)
        {
            string userEmail = HttpContext.Session.GetString("Email");
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

            ViewBag.UserEmail = userEmail;
            return View("ProductDetails", product);
        }

        public List<CartItem> GetGuestCartItems()
        {
            // Assuming guestList is the list of guest cart items in your controller
            return guestList.ToList();
        }


        public IActionResult BuyNow(string proid, int amount)
        {

            if (amount <= 0)
            {
                return BadRequest("Invalid quantity.");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open(); 

                // Retrieve the product details
                var product = GetProduct(connection, proid); // Pass the open connection
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                var cartItem = new CartItem(proid, amount, product.PName, product.OriginPrice, product.Sale_price != 0 ? product.Sale_price : product.OriginPrice, product.Amount);


                buynowList.Clear(); // Clear existing items for buy now scenario
                buynowList.Add(cartItem);


                // Redirect to Payment Page
                return View("ProductDetails",product);
            }
        }

        public IActionResult PriceRangeFilter(string MinNum, string MaxNum)
        {
            productsList = new List<Product>();
            int minPriceAsInt = int.Parse(MinNum);
            int maxPriceAsInt = int.Parse(MaxNum);


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = @"SELECT * FROM Product
        WHERE (Sale_Price != 0 AND Sale_Price >= @MinPrice AND Sale_Price <= @MaxPrice)
        OR (Sale_Price = 0 AND OriginPrice >= @MinPrice AND OriginPrice <= @MaxPrice)";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPriceAsInt);
                    command.Parameters.AddWithValue("@MaxPrice", maxPriceAsInt);

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

                        productsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View("shop",productsList);
        }


    }
}

