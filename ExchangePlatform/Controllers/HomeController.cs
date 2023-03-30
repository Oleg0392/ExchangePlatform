using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.IO;
using ExchangePlatform.Models;

namespace ExchangePlatform.Controllers
{
    public class HomeController : Controller
    {
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
                string newPath = Path.Combine(Directory.GetCurrentDirectory(), document.FileName);
                using(var stream = new FileStream(newPath,FileMode.Create))
                {
                    document.CopyTo(stream);
                }
                ViewBag.Title = "Upload complete.";
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
            string f_name = "testXml.xml";  // [необязательный]
            return File(fileRaw, f_type, f_name);
        }
    }
}
