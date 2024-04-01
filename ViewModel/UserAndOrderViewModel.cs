using GleamBoutiqueProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GleamBoutiqueProject.ViewModel
{
    public class UserAndOrderViewModel
    {
        public List<Orders> OrdersLists { get; set; }
        public User userToUpdate { get; set; }
    }
}
