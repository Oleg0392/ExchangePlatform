using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.IO;
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
                string newPath = Path.Combine(@"C:\Users\Oleg\source\repos\ExchangePlatform\ExchangePlatform\wwwroot\files\", document.FileName);
                using(var stream = new FileStream(newPath,FileMode.Create))
                {
                    document.CopyTo(stream);
                }
                int RowsAffected = 0;
                string queryString = "INSERT INTO Orders(DocNumber,DocDate,Buyer,Reciever,Sender,Reason,DocAllSum,DocAllCount) " +
                    "VALUES ('00001','20230329','test','test','test','test',123.00,5)";
                //OrderModel orderModel = new OrderModel(newPath);
                RowsAffected = queryManager.ExecuteNonQuery(queryString);
                ViewBag.Title = "Upload complete. " + RowsAffected.ToString() + " rows affected.";
                return View("Complete", new DocumentFile() { fileName = document.FileName });
            }
            ViewBag.Title = "Upload failed.";
            return View();
        }

        public FileResult DownloadTest()
        {
            string f_path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files\testXml.xml");
            byte[] fileRaw = System.IO.File.ReadAllBytes(f_path);
            string f_type = "application/xml";
            string f_name = "testXml.xml";         // [необязательный]
            return File(fileRaw, f_type, f_name);
        }
    }
}
