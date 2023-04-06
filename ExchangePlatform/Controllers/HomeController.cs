using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Web;
using System.IO;
using System.Collections.Generic;
using ExchangePlatform.Models;
using ExchangePlatform.DataProviders.Intrefaces;

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
                int RowsAffected = queryManager.ExecuteNonQuery(model.GetInsertCommand());
                ViewBag.Title = "Upload complete. " + RowsAffected.ToString() + " rows affected.";
                return View("Complete", new DocumentFile() { fileName = document.FileName });
            }
            ViewBag.Title = "Upload failed.";
            return View();
        }

        /*public FileResult DownloadTest()
        {
            string f_path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files\testXml.xml");
            byte[] fileRaw = System.IO.File.ReadAllBytes(f_path);
            string f_type = "application/xml";
            string f_name = "testXml.xml";         // [необязательный]
            return File(fileRaw, f_type, f_name);
        }*/

        public IActionResult OrderList()
        {
            ViewBag.Title = "Orders List Page";
            List<OrderModel> list = new List<OrderModel>();

            queryManager.ExecuteQuery(OrderModel.GetSelectAllCommand());      //new SqlCommand("SELECT DocNumber, DocDate, Buyer, Reciever, Sender, Reason, DocAllSum, DocAllCount FROM Orders"));  // тут важен порядок
            object[,] queryResult = queryManager.GetResultObjectArray2D();

            if (queryResult == null) return Redirect("/");
            for (int i = 0; i < queryResult.GetLength(0); i++)
            {
                object[] temp = new object[queryResult.GetLength(1)];
                for (int j = 0; j < temp.Length; j++)  temp[j] = queryResult[i,j];
                list.Add(new OrderModel(temp));
            }
            return View(list);
        }

        [HttpGet]
        public IActionResult EditOrder(string id)
        {
            ViewBag.Title = "Edit Order Page";
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
            return View("OrderList");
        }
    }
}
