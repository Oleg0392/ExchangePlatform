using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangePlatform.DataProviders.Intrefaces
{
    public interface IQueryManager
    {
        int ExecuteNonQuery(string queryString, string[] queryParams = null);
    }
}
