using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GleamBoutiqueProject.Models
{
    public class Product
    {
        public Product()
        {
        }

        public Product(Product product)
        {
            this.Pid = product.Pid;
            this.PName = product.PName;
            this.OriginPrice = product.OriginPrice;
            this.Amount = product.Amount;
            this.Notify_Count = product.Notify_Count;
            this.category = product.category;
            this.Material = product.Material;
            this.Sale_price = product.Sale_price;
            this.karat = product.karat;
        }

        [Key]
        [Required(ErrorMessage = "Catalog number is required ")]
        //[RegularExpression("^#200[a-zA-Z]{0,2}$", ErrorMessage = "Catalog number must start with #200")]
        public string Pid { get; set; }

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

        [Required(ErrorMessage = "Product Matrial is required")]
        public string Material { get; set; }
        public int Sale_price { get; set; }
        public int karat { get; set; }
        public Product Product1 { get; }
    }
}
