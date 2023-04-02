using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace ExchangePlatform.Models
{
    public class OrderModel
    {
        string DocNumber { get; set; }
        DateTime DocDate { get; set; }
        string Sender { get; set; }
        string Buyer { get; set; }
        string Reason { get; set; }
        string Reciever { get; set; }
        float DocSum { get; set; }
        int DocCount { get; set; }


        public OrderModel(string fileFullPath)
        {
            if (Directory.Exists(fileFullPath))
            {
                XmlDocument document = new XmlDocument();
                // здесь будем расставлять поля из хмл
            }
            else
            {
                throw new IOException("The Order xml-file does not exists!!!");
            }
        }

        public OrderModel(object[] queryResult)
        {
            // здесь нужно будет запись из БД превратить в модель
        }
    }
}
