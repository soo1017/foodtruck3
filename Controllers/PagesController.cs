using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Foodtruck;
using FoodTruck3.Models.ViewModel.Pages;
using FoodTruck3.Models.ViewModel.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;

namespace FoodTruck3.Controllers
{
    public class PagesController : Controller
    {
        private string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return s.First().ToString().ToUpper() + s.Substring(1).ToString().ToLower();
        }
        // GET: Index/{page}
        public ActionResult Index(string page="")
        {
            // Get/set page slug
            if (page == "")
                page = "home";

            // Declare model and DTO
            PagesDTO dto;
            PagesVM model;

            // Check if page exists
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            // Get page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // Set page title
            ViewBag.PageTitle = dto.Title;

            // Check for sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            // Init model
            model = new PagesVM(dto);
            
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            // Declare a list of PageVM
            List<PagesVM> pageVMList;
        
            // Get all pages except home
            using (Db db = new Db())
            {
                pageVMList = db.Pages
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PagesVM(x))
                    .ToList();
            }
            return PartialView(pageVMList);
        }

        public ActionResult PagesFTLocPartial()
        {
            // Declare a list of PageVM
            List<FoodtruckVM> foodtruckVMList;

            // Get all pages except home
            using (Db db = new Db())
            {
                foodtruckVMList = db.Foodtrucks
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new FoodtruckVM(x))
                    .ToList();
            }
            return PartialView(foodtruckVMList);
        }

        public ActionResult PagesMapFTLocPartial()
        {
            // Declare a list of PageVM
            FoodtruckVM model;

            int d = (int)System.DateTime.Now.DayOfWeek;

            // Get all pages except home
            using (Db db = new Db())
            {
                FoodtruckDTO dto = db.Foodtrucks.FirstOrDefault(x => x.Sorting == d);
                model = new FoodtruckVM(dto);
            }
            return PartialView(model);
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }

            return PartialView(categoryVMList);
        }

        // GET: Pages
        public ActionResult ProductAllCategoryPartial()
        {
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                productVMList = db.Products
                    .ToArray()
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Populate categories select list
                //ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

       
            // Return view with list
            return PartialView(productVMList);
        }

        // GET: Pages
        public ActionResult Contact()
        {
            return PartialView();
        }

        // POST: Pages
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> SendContact(ContactVM model)
        {
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress("recipient@gmail.com"));  // replace with valid value 
                message.From = new MailAddress("user@gmail.com");  // replace with valid value
                message.Subject = "Your email subject";
                message.Body = string.Format(body, model.Name, model.Email, model.Message);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "user@gmail.com",  // replace with valid value
                        Password = "password"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Sent");
                }
            }
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult Sent()
        {
            return View();
        }
    }
}