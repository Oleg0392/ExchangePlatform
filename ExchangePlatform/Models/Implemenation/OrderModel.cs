using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using Microsoft.Data.SqlClient;
using ExchangePlatform.Models.Interfaces;

namespace ExchangePlatform.Models.Implemenation
{
    public class OrderModel : IQueryModel
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
        public int SenderId { get; set; }

        // метаданные модели
        protected static Dictionary<string, DbType> ModelInfo = new Dictionary<string, DbType>()
        {
            { "DocNumber",DbType.String },    //0
            { "DocDate", DbType.DateTime },   //1
            { "Sender", DbType.String },      //2
            { "Buyer", DbType.String },       //3
            { "Reason", DbType.String },      //4
            { "Reciever", DbType.String },    //5
            { "DocAllSum", DbType.Decimal },  //6
            { "DocAllCount", DbType.Int32 },   //7
            { "DocId", DbType.Int32 },         //8
            { "SenderId", DbType.Int32 }       //9
        };

        public List<ItemModel> Items { get; set; }


        public OrderModel(string fileFullPath)
        {
            if (File.Exists(fileFullPath))
            {

                XmlDocument document = new XmlDocument();
                document.Load(fileFullPath);
                DocNumber = document.SelectSingleNode("/document/docHeader/docNumber").InnerText;
                DocDate = DateTime.ParseExact(document.SelectSingleNode("/document/docHeader/docDate").InnerText, "dd.MM.yyyy", null);
                Sender = document.SelectSingleNode("/document/docHeader/sender").InnerText;
                Buyer = document.SelectSingleNode("/document/docHeader/buyer").InnerText;
                Reason = document.SelectSingleNode("/document/docHeader/reason").InnerText;
                Reciever = document.SelectSingleNode("/document/docFooter/reciever").InnerText;
                DocSum = Convert.ToDecimal(document.SelectSingleNode("/document/docFooter/summary/sumSum").InnerText);
                DocCount = Convert.ToInt32(document.SelectSingleNode("/document/docFooter/summary/sumCount").InnerText);

                XmlNodeList itemNodeList = document.SelectNodes("/document/docLines/lineItem");
                Items = new List<ItemModel>();
                foreach (XmlNode itemNode in itemNodeList)
                {
                    Items.Add(new ItemModel()
                    {
                        LineNumber = Convert.ToInt32(itemNode.Attributes["number"].Value),
                        Name = itemNode.SelectSingleNode("posName").InnerText,
                        Art = itemNode.SelectSingleNode("posArt").InnerText,
                        Count = Convert.ToInt32(itemNode.SelectSingleNode("posCount").InnerText),
                        Price = Convert.ToDecimal(itemNode.SelectSingleNode("posPrice").InnerText),
                        Sum = Convert.ToDecimal(itemNode.SelectSingleNode("posSum").InnerText)
                    });
                }
            }
            else
            {
                throw new IOException("The Order xml-file does not exists!!!");
            }
        }

        public OrderModel(object[] queryResult)
        {
            LoadOrderModel(queryResult);
        }

        public OrderModel()
        {

        }

        public SqlCommand GetInsertCommand(int Id = 0)
        {
            string query = "INSERT INTO Orders(DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount) ";
            query += "VALUES (@DocNum, @DocDate, @Buyer, @Reciever, @Sender, @Reason, @DocSum, @DocCount)";
            SqlCommand command = new SqlCommand(query);

            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocNum", DbType = ModelInfo["DocNumber"], Value = DocNumber });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocDate", DbType = ModelInfo["DocDate"], Value = DocDate });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Buyer", DbType = ModelInfo["Buyer"], Value = Buyer });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Reciever", DbType = ModelInfo["Reciever"], Value = Reciever });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Sender", DbType = ModelInfo["Sender"], Value = Sender });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@Reason", DbType = ModelInfo["Reason"], Value = Reason });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocSum", DbType = ModelInfo["DocAllSum"], Value = DocSum });
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocCount", DbType = ModelInfo["DocAllCount"], Value = DocCount });

            return command;
        }

        public SqlCommand GetSelectCommand(int docId = 0)
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

        public SqlCommand GetDeleteCommand(int Id = 0)
        {
            string query = "DELETE FROM Orders WHERE DocId = @DocId";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter()
            {
                ParameterName = "@DocId",
                DbType = ModelInfo["DocId"],
                Value = Id
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
            Buyer = QueryResult[2].ToString();
            Reciever = QueryResult[3].ToString();
            Sender = QueryResult[4].ToString();
            Reason = QueryResult[5].ToString();
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

        public SqlCommand GetDocIdByDocNumber()
        {
            SqlCommand command = new SqlCommand("SELECT DocId FROM Orders WHERE DocNumber = @DocNumber");
            command.Parameters.Add(new SqlParameter() { ParameterName = "@DocNumber", DbType = ModelInfo["DocNumber"], Value = DocNumber });
            return command;
        }

        public void LoadItems(object[,] QueryResults)
        {
            Items = new List<ItemModel>();
            if (QueryResults == null) return;
            for (int i = 0; i < QueryResults.GetLength(0); i++)
            {
                object[] temp = new object[QueryResults.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = QueryResults[i,j];
                }
                Items.Add(new ItemModel());
                Items[i].LoadItemModel(temp);
            }
        }

        public void ToXmlDocument(string filePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.AppendChild(xmlDeclaration);
            XmlElement document = xmlDocument.CreateElement("document");
            xmlDocument.AppendChild(document);
            XmlElement docHeader = xmlDocument.CreateElement("docHeader");
            XmlElement docLines = xmlDocument.CreateElement("docLines");
            XmlElement docFooter = xmlDocument.CreateElement("docFooter");
            document.AppendChild(docHeader);
            document.AppendChild(docLines);
            document.AppendChild(docFooter);
            XmlElement docNumber = xmlDocument.CreateElement("docNumber");           
            XmlElement sender = xmlDocument.CreateElement("sender");
            XmlElement buyer = xmlDocument.CreateElement("buyer");
            XmlElement reason = xmlDocument.CreateElement("reason");
            XmlElement docDate = xmlDocument.CreateElement("docDate");
            docNumber.InnerText = DocNumber;
            sender.InnerText = Sender;
            buyer.InnerText = Buyer;
            reason.InnerText = Reason;
            docDate.InnerText = DocDate.ToString("dd.MM.yyyy");
            docHeader.AppendChild(docNumber);
            docHeader.AppendChild(sender);
            docHeader.AppendChild(buyer);
            docHeader.AppendChild(reason);
            docHeader.AppendChild(docDate);
            int i = 1;
            foreach (var item in Items)
            {
                XmlElement lineItem = xmlDocument.CreateElement("lineItem");
                if (item.LineNumber > 0) lineItem.SetAttribute("number", item.LineNumber.ToString());
                else lineItem.SetAttribute("number", i.ToString());
                XmlElement posName = xmlDocument.CreateElement("posName");
                XmlElement posArt = xmlDocument.CreateElement("posArt");
                XmlElement posCount = xmlDocument.CreateElement("posCount");
                XmlElement posPrice = xmlDocument.CreateElement("posPrice");
                XmlElement posSum = xmlDocument.CreateElement("posSum");
                lineItem.AppendChild(posName);
                lineItem.AppendChild(posArt);
                lineItem.AppendChild(posCount);
                lineItem.AppendChild(posPrice);
                lineItem.AppendChild(posSum);
                posName.InnerText = item.Name;
                posArt.InnerText = item.Art;
                posCount.InnerText = item.Count.ToString();
                posPrice.InnerText = item.Price.ToString();
                posSum.InnerText = item.Sum.ToString();
                docLines.AppendChild(lineItem);
                i++;
            }
            XmlElement summary = xmlDocument.CreateElement("summary");
            XmlElement reciever = xmlDocument.CreateElement("reciever");
            XmlElement application = xmlDocument.CreateElement("application");
            reciever.InnerText = Reciever;
            docFooter.AppendChild(summary);
            docFooter.AppendChild(reciever);
            docFooter.AppendChild(application);
            XmlElement sumCount = xmlDocument.CreateElement("sumCount");
            XmlElement sumSum = xmlDocument.CreateElement("sumSum");
            sumCount.InnerText = DocCount.ToString();
            sumSum.InnerText = DocSum.ToString();
            summary.AppendChild(sumCount);
            summary.AppendChild(sumSum);

            xmlDocument.Save(filePath);
            return;
        }
    }
}
