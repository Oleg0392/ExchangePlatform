using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ExchangePlatform.Models;
using ExchangePlatform.DataProviders.Intrefaces;
using ExchangePlatform.Models.Implemenation;

namespace ExchangePlatform.Controllers
{
    public class HomeController : Controller
    {
        IQueryManager queryManager { get; set; }

        public HomeController(IQueryManager manager)
        {
            queryManager = manager;
        }
        public IActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            ViewBag.Title = "Document Upload Page";
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile document)
        {
            if (document != null)
            {
                string newPath = Path.Combine(@"C:\Users\Oleg\source\repos\ExchangePlatform\ExchangePlatform\wwwroot\files\", document.FileName);  //@"C:\Temp\"
                using(var stream = new FileStream(newPath,FileMode.Create))
                {
                    document.CopyTo(stream);
                }
                               
                OrderModel model = new OrderModel(newPath);              
                int RowsAffected = queryManager.ExecuteNonQuery(model.GetInsertCommand());    // вставка заголовок заказа в БД
             
                queryManager.ExecuteQuery(model.GetDocIdByDocNumber());      // взятие DocId заказа из БД
                int orderId = Convert.ToInt32(queryManager.GetResultObject());
               
                foreach (var item in model.Items)  { queryManager.ExecuteNonQuery(item.GetInsertCommand(orderId)); }   // вставка позиций заказа в БД с взятым DocId для связки с заказом        

                ViewBag.Title = "Upload complete. " + RowsAffected.ToString() + " rows affected.";
                return View("Complete", new DocumentFile() { fileName = document.FileName });
            }
            ViewBag.Title = "Upload failed.";
            return View();
        }

        [HttpGet]
        public IActionResult OrderList()
        {
            ViewBag.Title = "Orders List Page";
            //List<OrderModel> list = new List<OrderModel>();
            OrdersTableModel tableModel = new();

            queryManager.ExecuteQuery(OrderModel.GetSelectAllCommand());      //new SqlCommand("SELECT DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount FROM Orders"));  // тут важен порядок
            object[,] orders = queryManager.GetResultObjectArray2D();

            if (orders == null) return Redirect("/");

            for (int i = 0; i < orders.GetLength(0); i++)
            {
                object[] temp = new object[orders.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)  temp[j] = orders[i,j];
                tableModel.Orders.Add(new OrderModel(temp));
            }

            queryManager.ExecuteQuery(ProviderModel.GetSelectAllCommand());
            object[,] providers = queryManager.GetResultObjectArray2D();

            for (int i = 0; i < providers.GetLength(0); i++)
            {
                object[] temp = new object[providers.GetLength(1)];
                for (int j = 0; j < temp.Length; j++) temp[j] = providers[i,j];
                tableModel.Providers.Add(new ProviderModel(temp));
            }

            return View(tableModel);
        }

        [HttpPost]
        public IActionResult OrderList(OrdersTableModel model,IFormCollection keyValuePairs)
        {
            if (keyValuePairs == null) return View(model);
            if (keyValuePairs.ContainsKey("provider"))
            {
                string provider = keyValuePairs["provider"];
                var filteredOrder = model.Orders.Where(ord => ord.Sender == provider);
                model.Orders.Clear();
                foreach (var pair in filteredOrder) { model.Orders.Add(pair); }
            }
            if (keyValuePairs.ContainsKey("dateBegin"))
            {
                DateTime dtBegin = DateTime.ParseExact(keyValuePairs["dateBegin"],"yyyy-MM-dd",null);
                DateTime dtEnd;
                if (keyValuePairs["dateEnd"] != "") dtEnd = DateTime.ParseExact(keyValuePairs["dateEnd"], "yyyy-MM-dd", null);
                else dtEnd = DateTime.MaxValue;

                var filtered = model.Orders.Where(ord => (ord.DocDate > dtBegin) && ord.DocDate < dtEnd);
                foreach (var pair in filtered) { model.Orders.Add(pair); }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult GetOrder(int id)
        {           
            OrderModel model = new ();
            queryManager.ExecuteQuery(model.GetSelectCommand(id));
            model.LoadOrderModel(queryManager.GetResultObjectArray1D());

            queryManager.ExecuteQuery(ItemModel.GetSelectAllCommand(model.DocId));
            model.LoadItems(queryManager.GetResultObjectArray2D());

            ViewBag.Title = "Order " + model.DocNumber + " Page";

            return View(model);
        }
        
        [HttpGet]
        public IActionResult EditOrder(int id)
        {
            OrderModel model = new OrderModel();
            queryManager.ExecuteQuery(model.GetSelectCommand(id));
            model.LoadOrderModel(queryManager.GetResultObjectArray1D());
            return View(model);
        }

        [HttpPost]
        public IActionResult EditOrder(OrderModel model)
        {
            object[] newValues = model.ToArray();
            OrderModel modelOld = new OrderModel();

            queryManager.ExecuteQuery(modelOld.GetSelectCommand(model.DocId));
            modelOld.LoadOrderModel(queryManager.GetResultObjectArray1D());

            int[] differents = OrderModel.EqualsFields(model,modelOld);
            for (int i = 0; i < differents.Length; i++)
            {
                if (differents[i] == 0) continue;
                queryManager.ExecuteNonQuery(model.GetUpdateCommand(i, newValues[i]));
            }
            ViewBag.Title = "Order was updated successeful!";
            return View("ResultAction");
        }

        public IActionResult DeleteOrder(int id)
        {
            OrderModel model = new();
            queryManager.ExecuteNonQuery(model.GetDeleteCommand(id));
            ItemModel itemModel = new();
            queryManager.ExecuteNonQuery(itemModel.GetDeleteCommand(id));
            ViewBag.Title = "Order deleted successeful!";
            return View("ResultAction");
        }

        [HttpGet]
        public IActionResult CreateOrder()
        {
            OrderModel model = new();
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateOrder(OrderModel model)
        {
            int Inserted = queryManager.ExecuteNonQuery(model.GetInsertCommand());

            if (Inserted > 0) ViewBag.Title = "Заказ успешно создан.";
            else ViewBag.Title = "Не удалось записать заказ в таблицу";
            ViewBag.Message = "/Home/OrderList/";
            return View("ResultAction");
        }

        [HttpGet]
        public IActionResult CreatePosition(int id)
        {
            ItemModel itemModel = new();
            itemModel.DocId = id;
            return View(itemModel);
        }

        [HttpPost]
        public IActionResult CreatePosition(ItemModel model)
        {
            
            OrderModel orderModel = new();
            queryManager.ExecuteQuery(orderModel.GetSelectCommand(model.DocId));
            orderModel.LoadOrderModel(queryManager.GetResultObjectArray1D());

            model.Sum = model.Price * model.Count;
            decimal NewSum = orderModel.DocSum + model.Sum;
            int NewCount = orderModel.DocCount + model.Count;

            queryManager.ExecuteNonQuery(orderModel.GetUpdateCommand(6, NewSum));
            queryManager.ExecuteNonQuery(orderModel.GetUpdateCommand(7, NewCount));
            queryManager.ExecuteNonQuery(model.GetInsertCommand(orderModel.DocId));

            ViewBag.Title = "Position was inserted successeful!";
            ViewBag.Message = "/Home/GetOrder/" + model.DocId.ToString();
            return View("ResultAction");
        }


        public FileResult DownloadOrder(int id)
        {
            OrderModel orderModel = new();
            queryManager.ExecuteQuery(orderModel.GetSelectCommand(id));
            orderModel.LoadOrderModel(queryManager.GetResultObjectArray1D());
            queryManager.ExecuteQuery(ItemModel.GetSelectAllCommand(id));
            orderModel.LoadItems(queryManager.GetResultObjectArray2D());

            string f_path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files\testXml.xml");
            orderModel.ToXmlDocument(f_path);

            byte[] fileRaw = System.IO.File.ReadAllBytes(f_path);
            string f_type = "application/xml";
            string f_name = "testXml.xml";         // [необязательный]
            return File(fileRaw, f_type, f_name);
        }
    }
}
