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
using System.Security.Cryptography;
using System.Reflection;
using System.Transactions;

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

        public IActionResult Payment(OrderViewModel ord)
        {
            string checkuser = HttpContext.Session.GetString("Email");
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(checkuser);

            if (!string.IsNullOrEmpty(checkuser))
            {
                var cartItems = GetCartItemsForUser(checkuser);
                if (cartItems == null || cartItems.Count == 0)
                {
                    return RedirectToAction("cart", "Shop");
                }

            }
            else
            {

                if (ShopController.guestList.Count == 0)
                {
                    return RedirectToAction("cart", "Shop");
                }

            }

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


        public IActionResult ThankYou(int receiptNumber)
        {
            string checkuser = HttpContext.Session.GetString("Email");
            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(checkuser);

            var payment = GetPaymentByReceiptNumber(receiptNumber);
            if (payment == null)
            {
                // Handle the case where no payment is found
                return NotFound(); // Or redirect to another view as appropriate
            }
            return View(payment);

        }


        public IActionResult MakePayment(OrderViewModel model)
        {
            string userEmail = HttpContext.Session.GetString("Email");
            string userName = HttpContext.Session.GetString("UserName");
            string userLastName = HttpContext.Session.GetString("LastUserName");

            if (!string.IsNullOrEmpty(userEmail))
            {
                if (ShopController.buynowList != null && ShopController.buynowList.Count > 0)
                {
                    model.OrderList = ShopController.buynowList;
                }
                else
                {
                    model.OrderList = GetCartItemsForUser(userEmail); // For logged-in users
                }
            }
            else
            {
                if (ShopController.buynowList != null && ShopController.buynowList.Count > 0)
                {
                    model.OrderList = ShopController.buynowList;
                }
                else
                {
                    var cartJson = HttpContext.Session.GetString("GuestCart");
                    if (!string.IsNullOrEmpty(cartJson))
                    {
                        model.OrderList = JsonSerializer.Deserialize<List<CartItem>>(cartJson); // For guests
                    }
                }
            }

            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.UserLastName = userLastName;

            Payment newPayment = model.OrderPayment;
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    try
                    {
                        int ReceiptNumber = NewReceiptNumber(connection, transaction);
                        
                        string insertPaymentSql = @"INSERT INTO Payment (CreditCardNumber, ExpiryDate, CVV, ID, FullName, FirstName, LastName, Email, City, Street, Apartment, PostalCode, Phone, Receipt)
                    VALUES (@CreditCardNumber, @ExpiryDate, @CVV, @ID, @FullName, @FirstName, @LastName, @Email, @City, @Street, @Apartment, @PostalCode, @Phone, @Receipt);
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
                        paymentCommand.Parameters.AddWithValue("@Receipt", ReceiptNumber);

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
                            if (ShopController.buynowList != null && ShopController.buynowList.Count > 0)
                            {
                                UpdateDataStock(connection, transaction, ShopController.buynowList);
                            }
                            else
                            {
                                UpdateDataStock(connection, transaction, model.OrderList);
                                DeleteCartItems(connection, transaction, userEmail);
                            }
                        }
                        else
                        {
                            if (ShopController.buynowList != null && ShopController.buynowList.Count > 0)
                            {
                                UpdateDataStock(connection, transaction, ShopController.buynowList);
                            }
                            else
                            {
                                UpdateDataStock(connection, transaction, ShopController.guestList);
                                HttpContext.Session.Remove("GuestCart");
                                ShopController.guestList.Clear();
                            }
                        }

                        transaction.Commit();

                        int ReceiptNumbertoThanks = ReceiptNumber;
                        return RedirectToAction("ThankYou", new { receiptNumber = ReceiptNumbertoThanks });


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


        private int NewReceiptNumber(SqlConnection connection, SqlTransaction transaction)
        {
            string getMaxReceiptSql = "SELECT ISNULL(MAX(Receipt), 0) + 1 FROM Payment";
            SqlCommand getMaxReceiptCmd = new SqlCommand(getMaxReceiptSql, connection, transaction);
            return (int)getMaxReceiptCmd.ExecuteScalar();
        }



        private Payment GetPaymentByReceiptNumber(int receiptNumber)
        {
            Payment payment = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM Payment WHERE Receipt = @ReceiptNumber";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReceiptNumber", receiptNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            payment = new Payment
                            {
                                // Assign properties from reader
                                Receipt = (int)reader["Receipt"],
                                shipId = (int)reader["shipId"],
                                
                            };
                        }
                    }
                }
            }
            return payment;

        }


        public IActionResult PayBuyNow()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            var userName = HttpContext.Session.GetString("UserName");
            var lastName = HttpContext.Session.GetString("LastUserName");
            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.UserLastName = lastName;

            OrderViewModel order = new OrderViewModel();


                //check if the buynowList has items.
                if (ShopController.buynowList.Any())
                {
                    order.OrderList = ShopController.buynowList;
                }

            return View("Payment",order);
        }

        private void UpdateDataStock(SqlConnection connection, SqlTransaction transaction, List<CartItem> Cart)
        {
            foreach (CartItem Item in Cart)
            {
                int newAmount = Item.ProStock - Item.ProAmount;
                string updateQuery = "UPDATE Product SET Amount = @ProAmount WHERE Pid = @ProId";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                {
                    updateCommand.Parameters.AddWithValue("@ProAmount", newAmount);
                    updateCommand.Parameters.AddWithValue("@ProId", Item.ProId);
                    updateCommand.ExecuteNonQuery();
                }
            }
        }
        private void DeleteCartItems(SqlConnection connection, SqlTransaction transaction, string userEmail)
        {
            string deleteCartSql = "DELETE FROM Cart WHERE UserEmail = @Email";
            SqlCommand deleteCartCmd = new SqlCommand(deleteCartSql, connection, transaction);
            deleteCartCmd.Parameters.AddWithValue("@Email", userEmail);
            deleteCartCmd.ExecuteNonQuery();
        }

    }

}

