using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GleamBoutiqueProject.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GleamBoutiqueProject.Filters;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GleamBoutiqueProject.Controllers
{
    [NoCache]
    public class ManagersController : Controller
    {

        public static List<Product> productsList = new List<Product>();
        public static List<Product> RemoveList = new List<Product>();
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly string connectionString;

        public ManagersController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }

        public IActionResult AddProducts(Product NewProduct)
        {
            return View(NewProduct);
        }

        public IActionResult AddToData(Product NewOne)
        {
            if (NewOne.Sale_price > NewOne.OriginPrice)
            {
                ModelState.AddModelError("Sale_price", "Sale price cannot be more than the original price.");
            }


            if (ModelState.IsValid)
            {
                if (NewOne.Material.ToLower() == "silver")
                {
                    NewOne.karat = 0;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "INSERT INTO Product (Pid, PName, OriginPrice, Amount, category, Material, Sale_price, karat) " +
                              "VALUES (@Pid, @PName, @OriginPrice, @Amount, @category, @Material, @Sale_price, @karat)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
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
                }
            }
            else
            {
                ViewData["formSubmitted"] = true;
                return View("AddProducts", NewOne);
            }
        }

        public IActionResult RemoveProductsS()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
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

                        RemoveList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(RemoveList);
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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
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

                        RemoveList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(RemoveList);
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
                        string selectQuery = "SELECT Amount,Notify_Count FROM Product WHERE Pid = @ProductId";

                        int currentAmount, notifyNum = 0;

                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                        {
                            selectCommand.Parameters.AddWithValue("@ProductId", productId);
                            using (SqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    currentAmount = reader.GetInt32(0);
                                    notifyNum = reader.GetInt32(1);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        if (selectedProducts[productId] >= 0)
                        {
                            int newAmount = selectedProducts[productId] + currentAmount;
                            int NewNotify = Math.Max(0, notifyNum - selectedProducts[productId]);

                            string updateQuery = "UPDATE Product SET Amount = @NewAmount, Notify_Count = @NewNotify WHERE Pid = @ProductId";

                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@NewAmount", newAmount);
                                command.Parameters.AddWithValue("@ProductId", productId);
                                command.Parameters.AddWithValue("@NewNotify", NewNotify);
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Input must contain only numbers greater than or equal to 0";
                            return RedirectToAction("InventoryCheck");
                        }
                    }
                }

                return RedirectToAction("ManagerHome", "Home");
            }

            return RedirectToAction("InventoryCheck");
        }

        public IActionResult ProductLists()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
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

                        RemoveList.Add(newProduct);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return View(RemoveList);
        }

        public IActionResult ProductsDetailsS(string id)
        {
            var product = GetProductDetailsById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        private Product GetProductDetailsById(string id)
        {
            Product product = null;

            string sqlQuery = "SELECT * FROM Product WHERE Pid = @ProductId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", id);

                    try
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read())
                        {
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
                            };
                        }

                        reader.Close();
                    }
                    catch (Exception ex)
                    {
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
                                return RedirectToAction("ManagerHome", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Product not found.");
                                return View("ProductsDetailsS", updatedProduct);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error updating product: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while updating the product.");
                        return View("ProductsDetailsS", updatedProduct);
                    }
                }
            }
            else
            {
                return View("ProductsDetailsS", updatedProduct);
            }
        }
        public IActionResult ManagerShop()
        {
            productsList = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Product";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
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
            return View(productsList);
        }

        public IActionResult SearchProduct(string searchKeyword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Product WHERE PName LIKE @proNameToSearch";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@proNameToSearch", "%" + searchKeyword + "%");

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

                    return View("ManagerShop", searchResults);
                }
            }
        }


        public IActionResult FilterResults(List<string> categories, List<string> materials, List<int> karats, int? salePrice)
        {
            productsList = new List<Product>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sqlQuery = new StringBuilder("SELECT * FROM Product WHERE 1=1");

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
                    for (int i = 0; i < categories.Count; i++)
                        command.Parameters.AddWithValue($"@category{i}", categories[i]);

                    for (int i = 0; i < materials.Count; i++)
                        command.Parameters.AddWithValue($"@material{i}", materials[i]);

                    for (int i = 0; i < karats.Count; i++)
                        command.Parameters.AddWithValue($"@karat{i}", karats[i]);
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
                            productsList.Add(newProduct);
                        }
                    }
                }
            }
            return View("ManagerShop", productsList);
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
            return View("ManagerShop", productsList);
        }
        public IActionResult ProductDetailsManager(string id)
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
            return View("ProductDetailsManager", product);
        }
        public IActionResult PriceRangeFilter(string MinNum, string MaxNum)
        {
            //productsList = new List<Product>();
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
            return View("ManagerShop", productsList);
        }


    }
}
