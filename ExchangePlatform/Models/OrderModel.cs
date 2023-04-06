using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Microsoft.Data.SqlClient;

namespace ExchangePlatform.Models
{
    public class OrderModel
    {
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string Sender { get; set; }
        public string Buyer { get; set; }
        public string Reason { get; set; }
        public string Reciever { get; set; }
        public decimal DocSum { get; set; }
        public int DocCount { get; set; }
        public int DocId { get; set; }

        // метаданные для модели
        protected static Dictionary<string, DbType> ModelInfo = new Dictionary<string, DbType>()
        {
            { "DocNumber",DbType.String },
            { "DocDate", DbType.DateTime },
            { "Sender", DbType.String },
            { "Buyer", DbType.String },
            { "Reason", DbType.String },
            { "Reciever", DbType.String },
            { "DocSum", DbType.Decimal },
            { "DocCuont", DbType.Int32 },
            { "DocId", DbType.Int32 }
        };


        public OrderModel(string fileFullPath)
        {
            if (Directory.Exists(fileFullPath))
            {

                XmlDocument document = new XmlDocument();
                document.LoadXml(fileFullPath);
                DocNumber = document.SelectSingleNode("/document/docHeader/docNumber").InnerText;
                DocDate = DateTime.ParseExact(document.SelectSingleNode("/document/docHeader/docDate").InnerText, "dd.MM.yyyy", null);
                Sender = document.SelectSingleNode("/document/docHeader/sender").InnerText;
                Buyer = document.SelectSingleNode("/document/docHeader/buyer").InnerText;
                Reason = document.SelectSingleNode("/document/docHeader/reason").InnerText;
                Reciever = document.SelectSingleNode("/document/docFooter/reciever").InnerText;
                DocSum = Convert.ToDecimal(document.SelectSingleNode("/document/docFooter/summary/sumSum").InnerText);
                DocCount = Convert.ToInt32(document.SelectSingleNode("/document/docFooter/summary/sumCount").InnerText);
            }
            else
            {
                throw new IOException("The Order xml-file does not exists!!!");
            }
        }

        public OrderModel(object[] queryResult)
        {
            DocNumber = queryResult[0].ToString();
            DocDate = Convert.ToDateTime(queryResult[1]);
            Sender = queryResult[2].ToString();
            Buyer = queryResult[3].ToString();
            Reason = queryResult[4].ToString();
            Reciever = queryResult[5].ToString();
            DocSum = Convert.ToDecimal(queryResult[6].ToString());
            DocCount = Convert.ToInt32(queryResult[7].ToString());
            DocId = Convert.ToInt32(queryResult[8].ToString());
        }

        public OrderModel()
        {

        }

        public SqlCommand GetInsertCommand()
        {
            string query = "INSERT INTO Orders(DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount) ";
            query += "VALUES (@DocNum, @DocDate, @Buyer, @Reciever, @Sender, @Reason, @DocSum, @DocCount)";
            SqlCommand command = new SqlCommand(query);

            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocNum", DbType = ModelInfo["DocNumber"], Value = DocNumber });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocDate", DbType = ModelInfo["DocDate"] , Value = DocDate });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Buyer", DbType = ModelInfo["Buyer"] , Value = Buyer });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Reciever", DbType = ModelInfo["Reciever"], Value = Reciever });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Sender", DbType = ModelInfo["Sender"], Value = Sender });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Reason", DbType = ModelInfo["Reason"], Value = Reason });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocSum", DbType = ModelInfo["DocSum"], Value = DocSum });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocCount", DbType = ModelInfo["DocCount"], Value = DocCount });
                       
            return command;
        }

        public SqlCommand GetSelectCommand(string docNumber)
        {
            string query = "SELECT DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount, DocId ";
            query += "FROM Orders WHERE DocNumber = @DocNum";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            { 
                ParameterName = "@DocNum",
                DbType = ModelInfo["DocNumber"],
                Value = docNumber
            });
            return command;
        }

        public SqlCommand GetSelectCommand(int docId)
        {
            string query = "SELECT DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount, DocId ";
            query += "FROM Orders WHERE DocId = @DocId";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            { 
                ParameterName = "@DocId",
                DbType = ModelInfo["DocId"],
                Value = docId
            });
            return command;
        }

        public static SqlCommand GetSelectAllCommand()
        {
            string query = "SELECT DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount, DocId FROM Orders";
            return new SqlCommand(query);
        }

        public void LoadOrderModel(object[] QueryResult)
        {
            DocNumber = QueryResult[0].ToString();
            DocDate = Convert.ToDateTime(QueryResult[1]);
            Sender = QueryResult[2].ToString();
            Buyer = QueryResult[3].ToString();
            Reason = QueryResult[4].ToString();
            Reciever = QueryResult[5].ToString();
            DocSum = Convert.ToDecimal(QueryResult[6].ToString());
            DocCount = Convert.ToInt32(QueryResult[7].ToString());
            DocId = Convert.ToInt32(QueryResult[8].ToString());
        }

        // массив из 0 и 1, если нет различия в значениях то 0, иначе 1
        public static int[] EqualsFields(OrderModel model1, OrderModel model2)
        {
            int[] equals = new int[8];
            equals[0] = model1.DocNumber == model2.DocNumber ? 0 : 1;
            equals[1] = model1.DocDate == model2.DocDate ? 0 : 1;
            equals[2] = model1.Sender == model2.Sender ? 0 : 1;
            equals[3] = model1.Buyer == model2.Buyer ? 0 : 1;
            equals[4] = model1.Reason == model2.Reason ? 0 : 1;
            equals[5] = model1.Reciever == model2.Reciever ? 0 : 1;
            equals[6] = model1.DocSum == model2.DocSum ? 0 : 1;
            equals[7] = model1.DocCount == model2.DocCount ? 0 : 1;

            return equals;
        }

        public SqlCommand GetUpdateCommand(int ModelInfoIndex, object NewValue)
        {
            string query = "UPDATE Orders SET " + ModelInfo.ElementAt(ModelInfoIndex).Key + " = @NewValue \n";
            query += "WHERE DocId = @DocId";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@NewValue",
                DbType = ModelInfo.ElementAt(ModelInfoIndex).Value,
                Value = NewValue
            });
            command.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@DocId",
                DbType = ModelInfo["DocId"],
                Value = DocId
            });
            return command;
        }

        public object[] ToArray()
        {
            object[] array = new object[9];
            array[0] = DocNumber;
            array[1] = DocDate;
            array[2] = Sender;
            array[3] = Buyer;
            array[4] = Reason;
            array[5] = Reciever;
            array[6] = DocSum;
            array[7] = DocCount;
            array[8] = DocId;
            return array;
        }
    }
}
