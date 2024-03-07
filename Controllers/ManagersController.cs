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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GleamBoutiqueProject.Controllers
{
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

            return RedirectToAction("ManagerHome","Home");
        }

    }
}

