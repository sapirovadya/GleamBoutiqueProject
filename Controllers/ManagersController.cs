using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GleamBoutiqueProject.ViewModel;
using GleamBoutiqueProject.Filters;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GleamBoutiqueProject.Controllers
{
    [NoCache]
    public class ManagersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly string connectionString;

        public ManagersController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }


        // GET: /<controller>/
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult AddProducts(Product NewProduct)
        {
            //Product NewProduct = new Product();  
            return View(NewProduct);
        }

        public IActionResult AddToData(Product NewOne)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "INSERT INTO Product (Pid, PName, OriginPrice, Amount, category, Material, Sale_price, karat) " +
                              "VALUES (@Pid, @PName, @OriginPrice, @Amount, @category, @Material, @Sale_price, @karat)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        //Set the parameter values to the SQL
                        command.Parameters.AddWithValue("@Pid", NewOne.Pid);
                        command.Parameters.AddWithValue("@PName", NewOne.PName);
                        command.Parameters.AddWithValue("@OriginPrice", NewOne.OriginPrice);
                        command.Parameters.AddWithValue("@Amount", NewOne.Amount);
                        command.Parameters.AddWithValue("@category", NewOne.category);
                        command.Parameters.AddWithValue("@Material", NewOne.Material);
                        command.Parameters.AddWithValue("@Sale_price", NewOne.Sale_price);
                        command.Parameters.AddWithValue("@karat", NewOne.karat);


                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return RedirectToAction("ManagerHome", "Home");
                        else

                            return View("AddProducts", NewOne);
                    }
                    connection.Close();
                }
            }
            else
            {
                return View("AddProducts", NewOne);
            }
        }

        public IActionResult RemoveProductsS()
        {
            RemoveProductViewModel RemViewModel = new RemoveProductViewModel();
            RemViewModel.ProductsList = new List<Product>();

            //SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

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

                        RemViewModel.ProductsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(RemViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProducts(List<string> selectedProducts)
        {
            if (selectedProducts != null && selectedProducts.Any())
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var productId in selectedProducts)
                    {
                        string deleteQuery = "DELETE FROM Product WHERE Pid = @ProductId";

                        using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

            return RedirectToAction("ManagerHome", "Home");
        }

        public IActionResult InventoryCheck()
        {
            RemoveProductViewModel InventoryViewModel = new RemoveProductViewModel();
            InventoryViewModel.ProductsList = new List<Product>();

            //SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

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

                        InventoryViewModel.ProductsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(InventoryViewModel);
        }

        public async Task<IActionResult> UpdateAmount(Dictionary<string, int> selectedProducts)
        {
            if (selectedProducts != null && selectedProducts.Any())
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var productId in selectedProducts.Keys)
                    {
                        // Get the current amount from the database
                        string selectQuery = "SELECT Amount,Notify_Count FROM Product WHERE Pid = @ProductId";

                        int currentAmount, notifyNum = 0;

                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                        {
                            selectCommand.Parameters.AddWithValue("@ProductId", productId);
                            // Execute the command to retrieve the current amount
                            using (SqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    currentAmount = reader.GetInt32(0);
                                    notifyNum = reader.GetInt32(1);
                                }
                                else
                                {
                                    // Handle the case where the product ID is not found
                                    continue;
                                }
                            }
                        }

                        // Check if the new amount is provided and not empty
                        if (selectedProducts[productId] >= 0)
                        {
                            // Update the amount only if the text box is not empty
                            int newAmount = selectedProducts[productId] + currentAmount;
                            int NewNotify = Math.Max(0, notifyNum - selectedProducts[productId]);

                            // Construct the SQL query to update the amount for the specified product ID
                            string updateQuery = "UPDATE Product SET Amount = @NewAmount, Notify_Count = @NewNotify WHERE Pid = @ProductId";

                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                // Set the parameters for the SQL query
                                command.Parameters.AddWithValue("@NewAmount", newAmount);
                                command.Parameters.AddWithValue("@ProductId", productId);
                                command.Parameters.AddWithValue("@NewNotify", NewNotify);
                                // Execute the SQL command to update the amount
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // If the input is invalid, return with an error message
                            TempData["ErrorMessage"] = "Input must contain only numbers greater than or equal to 0";
                            return RedirectToAction("InventoryCheck");
                        }
                    }
                }

                // Redirect to the ManagerHome action of the Home controller after updating amounts
                return RedirectToAction("ManagerHome", "Home");
            }

            // If no products were selected or the dictionary is null, redirect to ManagerHome
            return RedirectToAction("InventoryCheck");
        }

        public IActionResult ProductLists()
        {
            RemoveProductViewModel InventoryViewModel = new RemoveProductViewModel();
            InventoryViewModel.ProductsList = new List<Product>();

            //SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

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

                        InventoryViewModel.ProductsList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(InventoryViewModel);
        }

        public IActionResult ProductsDetailsS(string id)
        {
            // Retrieve the product details based on the provided ID
            var product = GetProductDetailsById(id);

            if (product == null)
            {
                // Handle the case where the product is not found
                return NotFound();
            }

            return View(product);
        }

        private Product GetProductDetailsById(string id)
        {
            Product product = null;

            // Connection string to your database
            //string connectionString = "your_connection_string_here";

            // SQL query to retrieve product details by ID
            string sqlQuery = "SELECT * FROM Product WHERE Pid = @ProductId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    // Add parameter for the product ID
                    command.Parameters.AddWithValue("@ProductId", id);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        // Check if any rows were returned
                        if (reader.Read())
                        {
                            // Create a new Product object and populate it with data from the query result
                            product = new Product
                            {
                                Pid = reader.GetString(0),
                                PName = reader.GetString(1),
                                OriginPrice = reader.GetInt32(2),
                                Amount = reader.GetInt32(3),
                                Notify_Count = reader.GetInt32(4),
                                category = reader.GetString(5),
                                Material = reader.GetString(6),
                                Sale_price = reader.GetInt32(7),
                                karat = reader.GetInt32(8)
                                // Continue populating other properties as needed
                            };
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }

            return product;
        }

        public IActionResult EditPro(Product updatedProduct)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        string updateQuery = "UPDATE Product SET PName = @PName, OriginPrice = @OriginPrice,Amount = @Amount, category = @Category, Material = @Material,Sale_price = @SalePrice, karat = @Karat WHERE Pid = @Pid";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@PName", updatedProduct.PName);
                            command.Parameters.AddWithValue("@OriginPrice", updatedProduct.OriginPrice);
                            command.Parameters.AddWithValue("@Amount", updatedProduct.Amount);
                            command.Parameters.AddWithValue("@Category", updatedProduct.category);
                            command.Parameters.AddWithValue("@Material", updatedProduct.Material);
                            command.Parameters.AddWithValue("@SalePrice", updatedProduct.Sale_price);
                            command.Parameters.AddWithValue("@Karat", updatedProduct.karat);
                            command.Parameters.AddWithValue("@Pid", updatedProduct.Pid);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Redirect to ManagerHome if update successful
                                return RedirectToAction("ManagerHome", "Home");
                            }
                            else
                            {
                                // Handle case where no rows were affected
                                ModelState.AddModelError("", "Product not found.");
                                return View("ProductsDetailsS", updatedProduct);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log exception and return error view
                        _logger.LogError($"Error updating product: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while updating the product.");
                        return View("ProductsDetailsS", updatedProduct);
                    }
                }
            }
            else
            {
                // If model state is not valid, return to ProductDetails view with validation errors
                return View("ProductsDetailsS", updatedProduct);
            }
        }
        public IActionResult ManagerShop()
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
            return View(proViewModel);
        }


    }
}
