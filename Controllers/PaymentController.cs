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


      /**  public IActionResult Payment()
        {
            return View();
        } **/

        public IActionResult Payment()
        {
            Payment newPayment = new Payment();
            return View(newPayment);
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
                            //Set the parameter values to the SQL
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
                    // If the expiry date is invalid, add a model error
                    ModelState.AddModelError("ExpiryDate", "The expiry date has passed.");
                    return View("Payment", newPayment);
                }
            }

            else
            {
                return View("Payment", newPayment);
            }

        }

    }

}

