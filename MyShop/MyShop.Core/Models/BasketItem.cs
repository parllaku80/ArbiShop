using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.Models
{
    public class BasketItem : BaseEntity
    {
        public string BasketId { get; set; } //we could either just copy all our, or just store tha id that links
                                             // the product to the item added to the basket, but if there are eventual 
                                             // updates on products in the meanwhile that the product is in the basket, like this 
                                             //it all updates fluently
        public string ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
