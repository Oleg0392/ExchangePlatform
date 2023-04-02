using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangePlatform.DataProviders.Intrefaces;

namespace ExchangePlatform.DataProviders.Implementation
{
    public class TestDbManager : IQueryManager
    {
        public int ExecuteNonQuery(string queryString, string[] queryParams = null)
        {
            return 1;
        }

        public void ExecuteQuery(string queryString, string[] queryParams)
        {

        }
    }
}
