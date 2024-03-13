using System;
namespace GleamBoutiqueProject.ViewModel
{
    public class CartItemViewModel
    {
        public string ProId { get; set; }
        public int ProAmount { get; set; }
        public string PName { get; set; }
        public int OriginPrice { get; set; }
        public int salePrice { get; set; }
    }
}
