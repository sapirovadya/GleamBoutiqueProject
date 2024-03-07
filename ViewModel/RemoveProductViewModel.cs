using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GleamBoutiqueProject.Models;

namespace GleamBoutiqueProject.ViewModel
{
    public class RemoveProductViewModel
    {
        public List<Product> ProductsList { get; set; }
        public List<string> SelectedProducts { get; set; }
    }
}

