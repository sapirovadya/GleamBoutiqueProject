using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GleamBoutiqueProject.Models;

namespace GleamBoutiqueProject.ViewModel
{
    public class ProductViewModel
    {
        public Product product { get; set; }
        public List<Product> productsList { get; set; }
        public List<Product> cartProducts { get; set; }
    }
}