using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ExchangePlatform.DataProviders.Intrefaces
{
    public interface IQueryManager
    {
        public int ExecuteNonQuery(SqlCommand command);

        public void ExecuteQuery(SqlCommand command);

        //async public Task<int> ExecuteNonQueryAsync(SqlCommand command);

        public object[,] GetResultObjectArray2D();
        public object[] GetResultObjectArray1D();
        public object GetResultObject();
    }
}
