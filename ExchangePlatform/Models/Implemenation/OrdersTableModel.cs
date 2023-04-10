﻿using System.Collections.Generic;

namespace ExchangePlatform.Models.Implemenation
{
    public class OrdersTableModel
    {
        public List<OrderModel> Orders { get; set; }
        public List<ProviderModel> Providers { get; set; }

        public OrdersTableModel()
        {
            Orders = new List<OrderModel>();
            Providers = new List<ProviderModel>();
        }

    }
}
