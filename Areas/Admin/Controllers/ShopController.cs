using FoodTruck3.Areas.Admin.Models.ViewModels.Shop;
using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
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

        // GET: Admin/Shop/Categories
        [HttpGet]
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }

            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddCategory
        [HttpPost]
        public ActionResult AddCategory(string catName)
        {
            string id;
            int count;

            using (Db db = new Db())
            {
                string slug;

                CategoryDTO categoryDTO = new CategoryDTO();

                // Check if catName is unique
                if (db.Categories.Any(x => x.Name == catName))
                {
                    return Json(new { error = "categorytaken" });
                }

                count = db.Categories.Count() + 1;

                slug = catName.Replace(" ", "-").ToLower();

                categoryDTO.Name = UppercaseFirst(catName.Trim());
                categoryDTO.Slug = slug;
                categoryDTO.Sorting = count;

                db.Categories.Add(categoryDTO);
                db.SaveChanges();

                id = categoryDTO.Id.ToString();
            }

            TempData["Success"] = "Category is added successfully.";

            return Json(new { id = id, count = count });
        }

        // GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            CategoryDTO dto;

            using (Db db = new Db())
            {
                dto = db.Categories.Find(id);

                if (dto == null)
                {
                    return Content("The category not found.");
                }

                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            TempData["Success"] = "Category is deleted successfully.";
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                CategoryDTO dto;

                foreach (var categoryId in id)
                {
                    dto = db.Categories.Find(categoryId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                string slug;

                CategoryDTO categoryDTO;

                // Check if catName is unique
                if (db.Categories.Where(x => x.Id != id).Any(x => x.Name == newCatName))
                {
                    return "categorytaken";
                }

                categoryDTO = db.Categories.Find(id);

                slug = newCatName.Replace(" ", "-").ToLower();

                categoryDTO.Name = UppercaseFirst(newCatName.Trim());
                categoryDTO.Slug = slug;

                db.SaveChanges();

            }

            TempData["Success"] = "Category is renamed successfully.";

            return id.ToString();
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> productVMList;

            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                productVMList = db.Products
                    .ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            // Set pagination
            var onePageOfProducts = productVMList.ToPagedList(pageNumber, 10);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            // Return view with list
            return View(productVMList);
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            bool flagImage = false;

            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
                
            }

            int id;

            using (Db db = new Db())
            {           
                ProductDTO dto = new ProductDTO(); 

                // Check id the product name is unique
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Product name is already taken.");
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

                dto.Name = UppercaseFirst(model.Name.Trim());
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                CategoryDTO dto1 = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = dto1.Name;

                dto.InitialQty = model.InitialQty;
                dto.CurrentQty = model.InitialQty;

                db.Products.Add(dto);
                db.SaveChanges();

                id = dto.Id;
                
            }

            // Upload image file
            // Create original dir
            var originalDir = new DirectoryInfo(string.Format("{0}\\Content", Server.MapPath(@"\")));
            var pathString = Path.Combine(originalDir.ToString(), "Products");

            if (!Directory.Exists(pathString))
                Directory.CreateDirectory(pathString);

            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image not uploaded with wrong image extension.");
                        return View(model);
                    }
                }

                // Init image name
                string imageName = file.FileName;

                // Save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.Image = imageName;

                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString, imageName);
                
                file.SaveAs(path);
                flagImage = true;
            }

            // Success message
            if (flagImage == true)
            {
                TempData["Success"] = "You have added a product with image.";
            } else
            {
                TempData["Success"] = "You have added a product without image.";
            }

            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/EditProduct
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);

                if (product == null)
                {
                    return Content("The product does not exist.");
                }
                
                model = new ProductVM(product);
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //model.Image = Directory.EnumerateFiles(Server.MapPath("~/Content/Products"))
                //                    .SingleOrDefault(f => f.Contains("Products/" + model.Image));
            }
            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            bool flagImage = false;

            int id = model.Id;

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                if (dto == null)
                {
                    ModelState.AddModelError("", "Product does not exist.");
                    //model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

                // Check id the product name is unique
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Product name is already taken.");
                    //model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

                dto.Name = UppercaseFirst(model.Name.Trim());
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                CategoryDTO dto1 = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = dto1.Name;
                dto.Image = model.Image;

                if (model.InitialQty != dto.InitialQty)
                {
                    dto.InitialQty = model.InitialQty;
                    dto.CurrentQty = model.InitialQty;
                }

                db.SaveChanges();

            }

            id = model.Id;

            // Upload image file
            // Create original dir
            var originalDir = new DirectoryInfo(string.Format("{0}\\Content", Server.MapPath(@"\")));
            var pathString = Path.Combine(originalDir.ToString(), "Products");

            if (!Directory.Exists(pathString))
                Directory.CreateDirectory(pathString);

            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image not uploaded with wrong image extension.");
                        return View(model);
                    }
                }

                // Init image name
                string imageName = file.FileName;

                // Save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.Image = imageName;

                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString, imageName);

                file.SaveAs(path);
                flagImage = true;
            }

            // Success message
            if (flagImage == true)
            {
                TempData["Success"] = "You have edited a product with image.";
            }
            else
            {
                TempData["Success"] = "You have edited a product without image.";
            }

            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/ProductDetails
        [HttpGet]
        public ActionResult ProductDetails(int id)
        {
            ProductVM model;
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("The product does not exist.");
                }

                model = new ProductVM(dto);
            }

            return View(model);
        }

        // GET: Admin/Shop/DeleteProduct
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            ProductDTO dto;

            using (Db db = new Db())
            {
                dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("The product not found.");
                }

                db.Products.Remove(dto);
                db.SaveChanges();
            }
            TempData["Success"] = "Product is deleted successfully.";
            return RedirectToAction("Products");
        }

        // GET: Admin/Shop/Orders
        [HttpGet]
        public ActionResult Orders()
        {
            QueryDateVM model = new QueryDateVM();
            return View(model);
        }

        // GET: Admin/Shop/DisplayOrdersPartial
        [HttpGet]
        public ActionResult DisplayOrdersPartial(string date)
        {
            //DateTime dateTime = DateTime.Parse(date);
            DateTime startTime;
            if (DateTime.TryParseExact(date, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            {

            }
            DateTime endTime = startTime.AddDays(1);

            // Init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Where(x => x.CreatedAt >= startTime && x.CreatedAt <= endTime).OrderBy(x => x.CreatedAt).Select(x => new OrderVM(x)).ToList();

                // Loop thru list of OrderVM
                foreach (var order in orders)
                {
                    // Init product list
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Get userName
                    UserDTO user;
                    string userEmail = null;
                    if (order.UserId != 0)
                    {
                        user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                        userEmail = user.Email;
                    }

                    // Loop thru list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get Product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }


                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = userEmail,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });

                }
            }

            // Return view with OrdersForAdminVM
            return PartialView(ordersForAdmin);
        }
    }
}