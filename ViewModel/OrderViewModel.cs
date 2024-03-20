using System;
using System.Collections.Generic;
using GleamBoutiqueProject.Models;

namespace GleamBoutiqueProject.ViewModel
{
	public class OrderViewModel
	{
        public List<CartItem> OrderList { get; set; }
        public Payment OrderPayment { get; set; }
    }
}


