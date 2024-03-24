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
            string userName = HttpContext.Session.GetString("UserName");
            string lastName = HttpContext.Session.GetString("LastUserName");
            OrderViewModel Order = new OrderViewModel();

            if (string.IsNullOrEmpty(userEmail))
            {
                var cartJson = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    Order.OrderList = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    //HttpContext.Session.Remove("GuestCart"); 
                }
            }
            else
            {
                Order.OrderList = GetCartItemsForUser(userEmail);
            }
            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.UserLastName = lastName;
            return View(Order);
        }


        public IActionResult ThankYou()
        {
            return View();
        }


        public IActionResult MakePayment(OrderViewModel model)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            if (!string.IsNullOrEmpty(userEmail))
            {
                model.OrderList = GetCartItemsForUser(userEmail); // For logged-in users
            }
            else
            {
                var cartJson = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    model.OrderList = JsonSerializer.Deserialize<List<CartItem>>(cartJson); // For guests
                }
            }

            Payment newPayment = model.OrderPayment;
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    try
                    {

                        string insertPaymentSql = @"INSERT INTO Payment (CreditCardNumber, ExpiryDate, CVV, ID, FullName, FirstName, LastName, Email, City, Street, Apartment, PostalCode, Phone)
                    VALUES (@CreditCardNumber, @ExpiryDate, @CVV, @ID, @FullName, @FirstName, @LastName, @Email, @City, @Street, @Apartment, @PostalCode, @Phone);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var paymentCommand = new SqlCommand(insertPaymentSql, connection, transaction);

                        paymentCommand.Parameters.AddWithValue("@CreditCardNumber", newPayment.CreditCardNumber);
                        DateTime expiryDate = newPayment.ConvertToDateTime(newPayment.ExpiryDate);
                        paymentCommand.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                        paymentCommand.Parameters.AddWithValue("@CVV", newPayment.CVV);
                        paymentCommand.Parameters.AddWithValue("@ID", newPayment.ID);
                        paymentCommand.Parameters.AddWithValue("@FullName", newPayment.FullName);
                        paymentCommand.Parameters.AddWithValue("@FirstName", newPayment.FirstName);
                        paymentCommand.Parameters.AddWithValue("@LastName", newPayment.LastName);
                        paymentCommand.Parameters.AddWithValue("@Email", newPayment.Email);
                        paymentCommand.Parameters.AddWithValue("@City", newPayment.City);
                        paymentCommand.Parameters.AddWithValue("@Street", newPayment.Street);
                        paymentCommand.Parameters.AddWithValue("@Apartment", newPayment.Apartment);
                        paymentCommand.Parameters.AddWithValue("@PostalCode", newPayment.PostalCode);
                        paymentCommand.Parameters.AddWithValue("@Phone", newPayment.Phone);

                        int shipId = (int)paymentCommand.ExecuteScalar(); 
                        decimal totalPrice = model.OrderList.Sum(item => (item.SalePrice != 0 ? item.SalePrice : item.OriginPrice) * item.ProAmount);

                        string insertOrderSql = @"INSERT INTO [Order] (OrderId, EmailUser, OrderDate, TotalPrice) VALUES (@OrderId, @EmailUser, GETDATE(), @TotalPrice)";

                        var orderCommand = new SqlCommand(insertOrderSql, connection, transaction);
                        orderCommand.Parameters.AddWithValue("@OrderId", shipId);
                        orderCommand.Parameters.AddWithValue("@EmailUser", newPayment.Email);
                        orderCommand.Parameters.AddWithValue("@TotalPrice", totalPrice);

                        orderCommand.ExecuteNonQuery();

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            // For logged-in users, delete cart items from the database
                            string deleteCartSql = "DELETE FROM Cart WHERE UserEmail = @Email";
                            SqlCommand deleteCartCmd = new SqlCommand(deleteCartSql, connection, transaction);
                            deleteCartCmd.Parameters.AddWithValue("@Email", userEmail);
                            deleteCartCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            HttpContext.Session.Remove("GuestCart");
                        }

                        transaction.Commit();
                        return RedirectToAction("ThankYou");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError(string.Empty, "An error occurred saving the order.");
                        return View("Payment", model);
                    }
                }
            }
            else
            {
                return View("Payment", model); // Return with errors if model state is invalid
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

