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
            proViewModel.cartProducts = new List<Product>();


            //SQL connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "select * from Product";

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
            return View("shop" , proViewModel);
        }



        //public IActionResult ShopFilter(string sqlQuery)
        //{
        //    ProductViewModel proViewModel = new ProductViewModel();
        //    proViewModel.productsList = new List<Product>();

        //    //SQL connection
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
        //        {
        //            //Read from the SQL table
        //            SqlDataReader reader = command.ExecuteReader();
        //            while (reader.Read())
        //            {
        //                Product newProduct = new Product();
        //                newProduct.Pid = reader.GetString(0);
        //                newProduct.PName = reader.GetString(1);
        //                newProduct.OriginPrice = reader.GetInt32(2);
        //                newProduct.Amount = reader.GetInt32(3);
        //                newProduct.Notify_Count = reader.GetInt32(4);
        //                newProduct.category = reader.GetString(5);
        //                newProduct.Material = reader.GetString(6);
        //                newProduct.Sale_price = reader.GetInt32(7);
        //                newProduct.karat = reader.GetInt32(8);
        //                //newProduct.ProImage = reader.GetString(9);

        //                proViewModel.productsList.Add(newProduct);
        //            }
        //            reader.Close();
        //        }
        //        connection.Close();
        //    }
        //    return View("shop", proViewModel);
        //}



        //public IActionResult FilterResults(string[] category, string[] material, int[] karat, int? salePrice)
        //{
        //    ProductViewModel proViewModel = new ProductViewModel();
        //    proViewModel.productsList = new List<Product>();

        //    // SQL connection
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        string sqlQuery = "SELECT * FROM Product WHERE " +
        //            "(@category IS NULL OR category IN (@category)) " +
        //            "AND (@material IS NULL OR Material IN (@material)) " +
        //            "AND (@karat IS NULL OR karat IN (@karat)) " +
        //            "AND (@salePrice IS NULL OR Sale_Price = @salePrice)";

        //        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
        //        {
        //            // Add parameters based on selected filters
        //            command.Parameters.AddWithValue("@category", category == null || category.Length == 0 ? DBNull.Value : (object)category);
        //            command.Parameters.AddWithValue("@material", material == null || material.Length == 0 ? DBNull.Value : (object)material);
        //            command.Parameters.AddWithValue("@karat", karat == null || karat.Length == 0 ? DBNull.Value : (object)karat);
        //            command.Parameters.AddWithValue("@salePrice", salePrice.HasValue ? (object)salePrice.Value : DBNull.Value);

        //            // Read from the SQL table
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Product newProduct = new Product();
        //                    newProduct.Pid = reader.GetString(0);
        //                    newProduct.PName = reader.GetString(1);
        //                    newProduct.OriginPrice = reader.GetInt32(2);
        //                    newProduct.Amount = reader.GetInt32(3);
        //                    newProduct.Notify_Count = reader.GetInt32(4);
        //                    newProduct.category = reader.GetString(5);
        //                    newProduct.Material = reader.GetString(6);
        //                    newProduct.Sale_price = reader.GetInt32(7);
        //                    newProduct.karat = reader.GetInt32(8);

        //                    proViewModel.productsList.Add(newProduct);
        //                }
        //            }
        //        }
        //    }
        //    return View("shop", proViewModel);
        //}


        //this is the best
        //public IActionResult FilterResults(List<string> categories, string material, int? karat, int? salePrice)
        //{
        //    ProductViewModel proViewModel = new ProductViewModel
        //    {
        //        productsList = new List<Product>()
        //    };

        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        var sqlQuery = new StringBuilder("SELECT * FROM Product WHERE 1=1");

        //        // Handling multiple categories
        //        if (categories != null && categories.Any())
        //        {
        //            var categoryParams = categories.Select((category, index) => $"@category{index}").ToList();
        //            sqlQuery.Append(" AND category IN (");
        //            sqlQuery.Append(string.Join(", ", categoryParams));
        //            sqlQuery.Append(")");
        //        }

        //        // Adding conditions for material, karat, and salePrice
        //        if (!string.IsNullOrEmpty(material)) sqlQuery.Append(" AND Material = @material");
        //        if (karat.HasValue) sqlQuery.Append(" AND karat = @karat");
        //        if (salePrice.HasValue) sqlQuery.Append(" AND Sale_Price <= @salePrice");

        //        //sqlQuery.Append("ORDER BY category");

        //        using (var command = new SqlCommand(sqlQuery.ToString(), connection))
        //        {
        //            // Adding parameters for categories
        //            for (int i = 0; i < categories.Count; i++)
        //            {
        //                command.Parameters.AddWithValue($"@category{i}", categories[i]);
        //            }
        //            // Adding parameters for material, karat, and salePrice
        //            if (!string.IsNullOrEmpty(material)) command.Parameters.AddWithValue("@material", material);
        //            if (karat.HasValue) command.Parameters.AddWithValue("@karat", karat.Value);
        //            if (salePrice.HasValue) command.Parameters.AddWithValue("@salePrice", salePrice.Value);

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var newProduct = new Product
        //                    {
        //                        Pid = reader["Pid"].ToString(),
        //                        PName = reader["PName"].ToString(),
        //                        OriginPrice = Convert.ToInt32(reader["OriginPrice"]),
        //                        Amount = Convert.ToInt32(reader["Amount"]),
        //                        Notify_Count = Convert.ToInt32(reader["Notify_Count"]),
        //                        category = reader["category"].ToString(),
        //                        // Assuming additional fields like Material are part of your Product model
        //                    };
        //                    proViewModel.productsList.Add(newProduct);
        //                }
        //            }
        //        }
        //    }
        //    return View("shop", proViewModel);
        //}



        //public IActionResult FilterResults(List<string> categories, List<string> materials, List<int> karats, int? salePrice)
        //{
        //    ProductViewModel proViewModel = new ProductViewModel
        //    {
        //        productsList = new List<Product>()
        //    };

        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        var sqlQuery = new StringBuilder("SELECT * FROM Product WHERE 1=1");

        //        // Handle multiple categories
        //        if (categories != null && categories.Any())
        //        {
        //            var categoryConditions = categories.Select((category, index) => $"@category{index}");
        //            sqlQuery.Append($" AND category IN ({string.Join(", ", categoryConditions)})");
        //            for (int i = 0; i < categories.Count; i++)
        //            {
        //               //command.Parameters.AddWithValue($"@category{i}", categories[i]);

        //                connection.Parameters.AddWithValue($"@category{i}", categories[i]);
        //            }
        //        }

        //        // Handle multiple materials
        //        if (materials != null && materials.Any())
        //        {
        //            var materialConditions = materials.Select((material, index) => $"@material{index}");
        //            sqlQuery.Append($" AND Material IN ({string.Join(", ", materialConditions)})");
        //            for (int i = 0; i < materials.Count; i++)
        //            {
        //                connection.Parameters.AddWithValue($"@material{i}", materials[i]);
        //            }
        //        }

        //        // Handle multiple karats
        //        if (karats != null && karats.Any())
        //        {
        //            var karatConditions = karats.Select((karat, index) => $"@karat{index}");
        //            sqlQuery.Append($" AND karat IN ({string.Join(", ", karatConditions)})");
        //            for (int i = 0; i < karats.Count; i++)
        //            {
        //                connection.Parameters.AddWithValue($"@karat{i}", karats[i]);
        //            }
        //        }

        //        // Handle salePrice
        //        if (salePrice.HasValue)
        //        {
        //            sqlQuery.Append(" AND Sale_Price <= @salePrice");
        //            connection.Parameters.AddWithValue("@salePrice", salePrice.Value);
        //        }

        //        using (var command = new SqlCommand(sqlQuery.ToString(), connection))
        //        {
        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var newProduct = new Product
        //                    {
        //                        Pid = reader["Pid"].ToString(),
        //                        PName = reader["PName"].ToString(),
        //                        OriginPrice = Convert.ToInt32(reader["OriginPrice"]),
        //                        Amount = Convert.ToInt32(reader["Amount"]),
        //                        Notify_Count = Convert.ToInt32(reader["Notify_Count"]),
        //                        category = reader["category"].ToString(),
        //                        Material = reader["Material"].ToString(), // Assuming 'Material' is a string
        //                        Sale_price = Convert.ToInt32(reader["Sale_price"]), // Assuming 'Sale_price' is stored as int
        //                        karat = reader.IsDBNull(reader.GetOrdinal("karat")) ? 0 : reader.GetInt32(reader.GetOrdinal("karat")), // Assuming 'karat' is an int and can be null
        //                    };
        //                    proViewModel.productsList.Add(newProduct);
        //                }
        //            }
        //        }
        //    }
        //    return View("shop", proViewModel);
        //}






        public IActionResult FilterResults(List<string> categories, List<string> materials, List<int> karats, int? salePrice)
        {
            ProductViewModel proViewModel = new ProductViewModel
            {
                productsList = new List<Product>()
            };

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
                    sqlQuery.Append(" AND material IN (");
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

                // Adding conditions for material, karat, and salePrice
                //if (!string.IsNullOrEmpty(material)) sqlQuery.Append(" AND Material = @material");
                //if (karat.HasValue) sqlQuery.Append(" AND karat = @karat");
                if (salePrice.HasValue) sqlQuery.Append(" AND Sale_Price <= @salePrice");

                //sqlQuery.Append("ORDER BY category");

                using (var command = new SqlCommand(sqlQuery.ToString(), connection))
                {
                    // Adding parameters for categories
                    for (int i = 0; i < categories.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@category{i}", categories[i]);
                    }

                    for (int i = 0; i < materials.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@material{i}", materials[i]);
                    }

                    for (int i = 0; i < karats.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@karat{i}", karats[i]);
                    }

                    // Adding parameters for material, karat, and salePrice
                    //if (!string.IsNullOrEmpty(material)) command.Parameters.AddWithValue("@material", material);
                    //if (karat.HasValue) command.Parameters.AddWithValue("@karat", karat.Value);
                    if (salePrice.HasValue) command.Parameters.AddWithValue("@salePrice", salePrice.Value);

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


    }
}
