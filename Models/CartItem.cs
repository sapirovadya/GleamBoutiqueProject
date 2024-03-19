using System;

namespace GleamBoutiqueProject.Models
{
    public class CartItem
    {
        public CartItem(string pid, int amount1, string pName, int originPrice, int salePrice, int amount2)
        {
            this.ProId = pid;
            this.ProAmount = amount1;
            this.PName = pName;
            this.OriginPrice = originPrice;
            this.SalePrice = salePrice;  // Updated property name to use PascalCase
            this.ProStock = amount2;
        }

        public string ProId { get; set; }
        public int ProAmount { get; set; }
        public string PName { get; set; }
        public int OriginPrice { get; set; }
        public int SalePrice { get; set; }  // Updated property name to use PascalCase
        public int ProStock { get; set; }
    }
}
