using System;
using System.Collections.Generic;
using GleamBoutiqueProject.Models;

namespace GleamBoutiqueProject.ViewModel
{
    public class CartItemViewModel
    {
        public List<CartItem> UserCart { get; set; }
        public List<CartItem> guestCart { get; set; }
    }
}
