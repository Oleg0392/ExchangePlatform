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
using ExchangePlatform.ViewModels;

namespace ExchangePlatform.Controllers
{
    public class HomeController : Controller
    {
        IQueryManager queryManager { get; set; }

        public HomeController(IQueryManager manager)
        {
            queryManager = manager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Title = "Home Page";
            if (HttpContext.Session.GetString("UserName") == null)
            {
                HttpContext.Session.SetString("UserName", "admin");
                HttpContext.Session.SetInt32("Pass", 1234);
            }
            ViewBag.Meaage = HttpContext.Session.GetString("UserName");
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

            if (orders == null) { ViewBag.Message = "Не одного заказа не найдео."; return View("ResultAction"); }
            for (int i = 0; i < orders.GetLength(0); i++)
            {
                object[] temp = new object[orders.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = orders[i, j];
                }
                tableModel.Orders.Add(new OrderModel(temp));
            }

            queryManager.ExecuteQuery(ProviderModel.GetSelectAllCommand());
            object[,] providers = queryManager.GetResultObjectArray2D();

            for (int i = 0; i < providers.GetLength(0); i++)
            {
                object[] temp = new object[providers.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = providers[i, j];                   
                }
                tableModel.Providers.Add(new ProviderModel(temp));
            }
            ViewBag.DateBegin = "2023-03-31";
            ViewBag.DateEnd = "2023-04-07";

            return View(tableModel);
        }

        [HttpPost]
        public IActionResult OrderList(OrdersTableModel model)
        {
            model.Orders.Clear();
            queryManager.ExecuteQuery(OrderModel.GetSelectAllCommand());
            object[,] orders = queryManager.GetResultObjectArray2D();

            if (orders == null) { ViewBag.Message = "Не одного заказа не найдео."; return View("ResultAction"); }
            for (int i = 0; i < orders.GetLength(0); i++)
            {
                object[] temp = new object[orders.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = orders[i, j];
                }
                model.Orders.Add(new OrderModel(temp));
            }
            ViewBag.DateBegin = Request.Form["dateBegin"];
            ViewBag.DateEnd = Request.Form["dateEnd"];

            if (Request.Form.ContainsKey("cancel")) { return View(model); }

            if (Request.Form.ContainsKey("provider"))
            {
                int provider = Convert.ToInt32(Request.Form["provider"]);
                List<OrderModel> tempList = new();
                foreach (var pair in model.Orders.Where(ord => ord.SenderId == provider)) { tempList.Add(pair); }
                model.Orders.Clear();
                model.Orders = tempList;
            }
            if (Request.Form.ContainsKey("dateBegin"))
            {
                DateTime dtBegin = DateTime.ParseExact(Request.Form["dateBegin"], "yyyy-MM-dd", null);
                DateTime dtEnd = DateTime.ParseExact(Request.Form["dateEnd"], "yyyy-MM-dd", null);

                List<OrderModel> tempList = new();
                foreach (var pair in model.Orders.Where(ord => (ord.DocDate >= dtBegin) && ord.DocDate <= dtEnd)) { tempList.Add(pair); }
                model.Orders.Clear();
                model.Orders = tempList;
            }
            if (model.Providers == null)
            {
                int i = 0;
                foreach (var keyValue in Request.Form.Keys)
                {
                    if (keyValue.Contains("ProviderId"))
                    {
                        model.Providers.Add(new());
                        model.Providers[i].ProviderId = Convert.ToInt32(Request.Form[keyValue]);
                    }
                    if (keyValue.Contains("ProviderName"))
                    {
                        model.Providers[i].ProviderName = Request.Form[keyValue];
                        i++;
                    }
                }
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
            EditOrderModel model = new();
            queryManager.ExecuteQuery(model.Target.GetSelectCommand(id));
            object[] order = queryManager.GetResultObjectArray1D();
            if (order != null) model.Target.LoadOrderModel(order);
            queryManager.ExecuteQuery(ProviderModel.GetSelectAllCommand());
            object[,] providers = queryManager.GetResultObjectArray2D();
            if (providers != null)
            {
                for(int i = 0; i < providers.GetLength(0); i++)
                {
                    object[] temp = new object[providers.GetLength(0)]; 
                    for(int j = 0; j < providers.GetLength(1); j++) { temp[j] = providers[i,j]; }
                    model.AllProviders.Add(new ProviderModel(temp));
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult EditOrder(EditOrderModel model)
        {
            // установка поставщика по коду
            queryManager.ExecuteQuery(model.AllProviders[0].GetSelectCommand(model.Target.SenderId));
            model.Target.Sender = queryManager.GetResultObject().ToString();

            object[] newValues = model.Target.ToArray();
            OrderModel modelOld = new OrderModel();

            queryManager.ExecuteQuery(modelOld.GetSelectCommand(model.Target.DocId));
            modelOld.LoadOrderModel(queryManager.GetResultObjectArray1D());           

            int[] differents = OrderModel.EqualsFields(model.Target,modelOld);
            for (int i = 0; i < differents.Length; i++)
            {
                if (differents[i] == 0) continue;
                queryManager.ExecuteNonQuery(model.Target.GetUpdateCommand(i, newValues[i]));
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
            EditOrderModel model = new();
            queryManager.ExecuteQuery(ProviderModel.GetSelectAllCommand());
            object[,] providers = queryManager.GetResultObjectArray2D();
            if (providers != null)
            {
                for (int i = 0; i < providers.GetLength(0); i++)
                {
                    object[] temp = new object[providers.GetLength(0)];
                    for (int j = 0; j < providers.GetLength(1); j++) { temp[j] = providers[i, j]; }
                    model.AllProviders.Add(new ProviderModel(temp));
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateOrder(EditOrderModel model)
        {
            queryManager.ExecuteQuery(model.AllProviders[0].GetSelectCommand(model.Target.SenderId));  // установка поставщика
            model.Target.Sender = queryManager.GetResultObject().ToString();

            int Inserted = queryManager.ExecuteNonQuery(model.Target.GetInsertCommand());

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
            queryManager.ExecuteQuery(ItemModel.GetSelectAllCommand(orderModel.DocId));
            int lineNum = queryManager.GetResultObjectArray2D().GetLength(0);

            model.LineNumber = lineNum + 1;
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
                string newPath = Path.Combine(@"D:\Users\khusainovom\source\repos\ExchangePlatform\ExchangePlatform\wwwroot\files\", document.FileName);  //@"  C:\Users\Oleg\source\repos\ExchangePlatform\ExchangePlatform\wwwroot\files\
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    document.CopyTo(stream);
                }

                OrderModel model = new OrderModel(newPath);
                int RowsAffected = queryManager.ExecuteNonQuery(model.GetInsertCommand());    // вставка заголовок заказа в БД

                queryManager.ExecuteQuery(model.GetDocIdByDocNumber());      // взятие DocId заказа из БД
                int orderId = Convert.ToInt32(queryManager.GetResultObject());

                foreach (var item in model.Items) { queryManager.ExecuteNonQuery(item.GetInsertCommand(orderId)); }   // вставка позиций заказа в БД с взятым DocId для связки с заказом        

                ViewBag.Title = "Upload complete. " + RowsAffected.ToString() + " rows affected.";
                return View("Complete", new DocumentFile() { fileName = document.FileName });
            }
            ViewBag.Title = "Upload failed.";
            return View();
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
