using Microsoft.Data.SqlClient;
using System;
using ExchangePlatform.Models.Interfaces;

namespace ExchangePlatform.Models.Implemenation
{
    public class ProviderModel : IQueryModel
    {
        public string ProviderName { get; set; }   
        public int ProviderId { get; set; }

        public SqlCommand GetDeleteCommand(int Id = 0)
        {
            throw new NotImplementedException();
        }

        public SqlCommand GetInsertCommand(int Id = 0)
        {
            throw new NotImplementedException();
        }

        public SqlCommand GetSelectCommand(int id = 0)
        {
            return null;
        }

        public SqlCommand GetUpdateCommand(int Id, object NewValue)
        {
            throw new NotImplementedException();
        }

        public static SqlCommand GetSelectAllCommand()
        {
            return new SqlCommand("SELECT Id, Name FROM Providers");           
        }

        public ProviderModel() { }

        public ProviderModel(object[] QueryResult)
        {
            if (QueryResult == null)
            {
                ProviderId = 0;
                ProviderName = "";
            }
            else
            {
                ProviderId = Convert.ToInt32(QueryResult[0]);
                ProviderName = QueryResult[1].ToString();
            }
        }
    }
}
