using GleamBoutiqueProject.Models;
using GleamBoutiqueProject.ViewModel;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GleamBoutiqueProject.Controllers
{
    public class ProductDetailsController : Controller
    {

        public IConfiguration _configuration;
        string connectionString = "";
        public ProductDetailsController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IncrementNotifyCount(string userEmail, string productId)
        {
 

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // SQL to increment the notify count for the specified product
                string sqlQuery = "UPDATE Product SET Notify_Count = Notify_Count + 1 WHERE Pid = @ProductId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    // Set the parameter value to the SQL command
                    command.Parameters.AddWithValue("@ProductId", productId);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // If the update was successful, redirect or return a success response
                        return RedirectToAction("ProductDetails", "Shop", new { id = productId });
                    }
                    else
                    {
                        // Handle the case where the product was not found or the update failed
                        return NotFound();
                    }
                }
            }
        }

    }
}
