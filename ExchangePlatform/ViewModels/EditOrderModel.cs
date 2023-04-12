using ExchangePlatform.Models.Implemenation;
using System.Collections.Generic;

namespace ExchangePlatform.ViewModels
{
    public class EditOrderModel
    {
        public OrderModel Target { get; set; }
        public List<ProviderModel> AllProviders { get; set; }

        public EditOrderModel()
        {
            Target = new OrderModel(); 
            AllProviders = new List<ProviderModel>();
        }
    }

}
