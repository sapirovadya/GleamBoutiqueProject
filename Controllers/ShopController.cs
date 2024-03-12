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
                    while(reader.Read())
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



        [HttpPost]
        public IActionResult AddToCart(string pid)
        {
            //ProductViewModel proViewModel = new ProductViewModel();
            //proViewModel.cartProducts = new List<Product>();

            if (pid != null)
            {
                // Find the product with the given Pid
                //Product product = proViewModel.productsList.FirstOrDefault(p => p.Pid == pid);

                // Add the product to the cartProducts list
                PidCartList.Add(pid);
                Console.WriteLine("the product was added" + pid);
                return Ok(); // Return a success response
            }
            return NotFound(); // Return a not found response if the product is not found or pid is null
        }
    }
}
