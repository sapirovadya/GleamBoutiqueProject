using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using GleamBoutiqueProject.Models;
using GleamBoutiqueProject.ViewModel;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace GleamBoutiqueProject.Controllers
{
    public class PaymentController : Controller
    {

        public IConfiguration _configuration;
        string connectionString = "";
        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("dbConnect");
        }

        public IActionResult Payment()
        {
            string userEmail = HttpContext.Session.GetString("Email");
            OrderViewModel Order = new OrderViewModel();

            if (string.IsNullOrEmpty(userEmail))
            {
                var cartJson = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    Order.OrderList = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    HttpContext.Session.Remove("GuestCart"); 
                }
            }
            else
            {
                Order.OrderList = GetCartItemsForUser(userEmail);
            }
            return View(Order);
        }


        public IActionResult ThankYou()
        {
            return View();
        }

        
        

        public IActionResult MakePayment(Payment newPayment)
        {
            if (ModelState.IsValid)
            {
                if (newPayment.IsExpiryDateValid())
                {

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlQuery = "INSERT INTO [Payment] VALUES (@Value1, @Value2, @Value3, @Value4, @value5)";

                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Value1", newPayment.CreditCardNumber);
                            DateTime expiryDate = newPayment.ConvertToDateTime(newPayment.ExpiryDate);
                            command.Parameters.AddWithValue("@Value2", expiryDate);
                            command.Parameters.AddWithValue("@Value3", newPayment.CVV);
                            command.Parameters.AddWithValue("@Value4", newPayment.ID);
                            command.Parameters.AddWithValue("@Value5", newPayment.FullName);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                                return RedirectToAction("ThankYou", newPayment);
                            else

                                return View("Payment", newPayment);
                        }
                        connection.Close();
                    }
                }

                else
                {
                    ModelState.AddModelError("ExpiryDate", "The expiry date has passed.");
                    return View("Payment", newPayment);
                }
            }

            else
            {
                return View("Payment", newPayment);
            }

        }
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
                            CartItem item = new CartItem(reader.GetString(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5));
                            cartItems.Add(item);
                        }
                    }
                }
            }
            return cartItems;
        }

    }

}

