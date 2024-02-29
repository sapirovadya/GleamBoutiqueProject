﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GleamBoutiqueProject.Models
{
    public class Product
    {
        [Key]
        [Required(ErrorMessage = "Catalog number is required ")]
        [RegularExpression("^#200[a-zA-Z]{0,3}$", ErrorMessage = "Catalog number must start with #200")]
        public int Pid { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(12, MinimumLength = 2, ErrorMessage = "Product name must be between 2-12 letters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name should contain only letters")]
        public string PName { get; set; }

        [Required(ErrorMessage = "Product price is required")]
        [RegularExpression(@"^\d{2,4}$", ErrorMessage = "Product price must contain 2-4 digits")]
        public int OriginPrice { get; set; }

        [Required(ErrorMessage = "Product amount is required")]
        [RegularExpression(@"^\d{1,3}$", ErrorMessage = "Amount must contain only 1-3 digits")]
        public int Amount { get; set; }
        public int Notify_Count { get; set; }

        [Required(ErrorMessage = "Product category is required")]
        [StringLength(12, MinimumLength = 4, ErrorMessage = "Product name must be between 4-12 letters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Category name should contain only letters")]
        public string category { get; set; }

        [Required(ErrorMessage = "Release Date is required")]
        public DateTime Release_Date { get; set; }

        [Required(ErrorMessage = "Product Matrial is required")]
        public string Matrial { get; set; }
        public int Sale_price { get; set; }
        public int karat { get; set; }
        public int ProImage { get; set; }



    }



}