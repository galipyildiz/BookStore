using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Entities
{
    public class Order : BaseEntity
    {
        public string BuyerId { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public Address ShiptoAddress { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
