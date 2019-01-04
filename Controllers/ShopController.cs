using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
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

        // GET: Shop
        public ActionResult ProductOnCategory(string name)
        {
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                if (name == "all")
                {
                    productVMList = db.Products
                                .ToArray()
                                .OrderBy(x => x.CategoryId)
                                .Select(x => new ProductVM(x))
                                .ToList();
                    ViewBag.CategoryName = "All";
                }
                else
                {
                    // Get category id
                    CategoryDTO dto = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                    int catId = dto.Id;

                    // Init the list
                    productVMList = db.Products
                                .ToArray()
                                .Where(x => x.CategoryId == catId)
                                .Select(x => new ProductVM(x))
                                .ToList();

                    // Get category name
                    var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                    ViewBag.CategoryName = productCat.CategoryName;
                }  
            }
            return View(productVMList);
        }
    }
}