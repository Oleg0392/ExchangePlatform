using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using ExchangePlatform.DataProviders.Intrefaces;

namespace ExchangePlatform.DataProviders.Implenetation
{
    public class QueryManager : IQueryManager
    {
        //public static IConfigurationProvider ConfigurationProvider { get; set; }
        IConfiguration configuration { get; set; }
        public QueryManager(IConfiguration config)
        {
            configuration = config;
        }
        public int ExecuteNonQuery(string sqlQuery, string[] queryParams = null)
        {
            int queryResult = 0;
            if (queryParams != null) for (int i = 0; i < queryParams.Length; i++) sqlQuery = sqlQuery.Replace("prm[" + i.ToString() + "]", queryParams[i]);
            SqlConnection sqlConnection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
            sqlConnection.Open();
            using(sqlConnection)
            {
                queryResult = sqlCommand.ExecuteNonQuery();
            }
            sqlConnection.Close();
            return queryResult;
        }
    }
}
