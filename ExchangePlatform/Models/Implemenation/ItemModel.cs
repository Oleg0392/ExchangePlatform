using System.Collections.Generic;
using System.Data;
using System.Xml;
using System;
using Microsoft.Data.SqlClient;
using ExchangePlatform.Models.Interfaces;

namespace ExchangePlatform.Models.Implemenation
{
    public class ItemModel : IQueryModel
    {
        public string Name { get; set; }
        public string Art { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
        public int ItemId { get; set; }
        public int DocId { get; set; }

        protected static Dictionary<string, DbType> ItemModelInfo = new Dictionary<string, DbType>()
        {
            { "Name", DbType.String},
            { "Art", DbType.String },
            { "Count", DbType.Int32 },
            { "Price", DbType.Decimal },
            { "Sum", DbType.Decimal },
            { "ItemId", DbType.Int32 },
            { "DocId", DbType.Int32 }
        };

        public ItemModel() { }

        public ItemModel(XmlNode itemNode)
        {
            Name = itemNode.SelectSingleNode("/lineItem/posName").InnerText;
            Art = itemNode.SelectSingleNode("/lineItem/posArt").InnerText;
            Count = Convert.ToInt32(itemNode.SelectSingleNode("/lineItem/posCount").Value);
            Price = Convert.ToDecimal(itemNode.SelectSingleNode("/lineItem/posPrice").Value);
            Sum = Convert.ToDecimal(itemNode.SelectSingleNode("/lineItem/posSum").Value);
        }

        public SqlCommand GetInsertCommand(int docId)
        {
            string query = "INSERT INTO OrderItems(Name,Art,Count,Price,Sum,DocId) VALUES (@Name,@Art,@Count,@Price,@Sum,@DocId) ";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Name", DbType = ItemModelInfo["Name"], Value = Name });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Art", DbType = ItemModelInfo["Art"], Value = Art });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Count", DbType = ItemModelInfo["Count"], Value = Count });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Price", DbType = ItemModelInfo["Price"], Value = Price });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Sum", DbType = ItemModelInfo["Sum"], Value = Sum });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocId", DbType = ItemModelInfo["DocId"], Value = docId });
            return command;
        }

        public SqlCommand GetSelectCommand(int itemId = 0)
        {
            if (itemId == 0) itemId = ItemId;
            string query = "SELECT Name, Art, Count, Price, Sum, DocId FROM OrderItems WHERE ItemId = @ItemId";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@ItemId",
                DbType = ItemModelInfo["ItemId"],
                Value = itemId
            });
            return command;
        }

        public SqlCommand GetUpdateCommand(int Id, object newValue)
        {
            return null;
        }

        public SqlCommand GetDeleteCommand(int Id = 0)
        {
            return null;
        }

        public static SqlCommand GetSelectAllCommand(int docId = 0)
        {
            string query = "SELECT Name, Art, Count, Price, Sum, DocId FROM OrderItems WHERE DocId = @DocId";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@DocId",
                DbType = ItemModelInfo["DocId"],
                Value = docId
            });
            return command;
        }

        public ItemModel(object[] QueryResult)
        {
            Name = QueryResult[0].ToString();
            Art = QueryResult[1].ToString();
            Count = Convert.ToInt32(QueryResult[2]);
            Price = Convert.ToDecimal(QueryResult[3]);
            Sum = Convert.ToDecimal(QueryResult[4]);
            DocId = Convert.ToInt32(QueryResult[5]);
        }

        public void LoadItemModel(object[] QueryResult)
        {
            Name = QueryResult[0].ToString();
            Art = QueryResult[1].ToString();
            Count = Convert.ToInt32(QueryResult[2]);
            Price = Convert.ToDecimal(QueryResult[3]);
            Sum = Convert.ToDecimal(QueryResult[4]);
            DocId = Convert.ToInt32(QueryResult[5]);
        }
    }
}
