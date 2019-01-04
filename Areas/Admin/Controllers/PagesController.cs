using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
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

        // GET: Admin/Pages
        public ActionResult Index()
        {
            // Declare list of pagesVM
            List<PagesVM> pagesList;

            using (Db db = new Db())
            {
                // Init the list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PagesVM(x)).ToList(); // "x" in Select is PagesDTO 
            }

            // Return
            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            // declare PagesVM
            PagesVM model = new PagesVM();

            return View(model);
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PagesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                // Declare PagesDTO
                PagesDTO dto = new PagesDTO();

                if (string.IsNullOrWhiteSpace(model.Slug)) {
                    slug = model.Title.Replace(" ", "-").ToLower();
                } else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                // Page Title is unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "This title is already taken!!");
                    return View(model);
                }

                dto.Title = UppercaseFirst(model.Title.Trim());
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.Sorting = 100;
                dto.HasSidebar = model.HasSidebar;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["Success"] = "You have added a new page.";

            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Declare PagesVM
            PagesVM model;

            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                model = new PagesVM(dto);
            }
            
            return View(model);
        }

        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PagesVM model)
        {
            int id;
            string slug;
            id = model.Id;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                if (model.Slug != "home")
                {
                    PagesDTO dto = db.Pages.Find(id);

                    if (dto == null)
                    {
                        ModelState.AddModelError("", "The page does not exist.");
                        return View(model);
                    }

                    if (!string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }

                    if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title)
                        || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                    {
                        ModelState.AddModelError("", "The page is already taken.");
                        return View(model);
                    }

                    dto.Title = UppercaseFirst(model.Title.Trim());
                    dto.Slug = slug;
                    dto.Body = model.Body;
                    dto.HasSidebar = model.HasSidebar;

                    db.SaveChanges();
                }
            }

            TempData["Success"] = "You have edited a page.";

            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            PagesVM model;
            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                model = new PagesVM(dto);
            }

            return View(model);
        }

        // GET: Admin/Pages/DeletePage
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            string title;

            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                title = '"' + dto.Title + '"';

                db.Pages.Remove(dto);
                db.SaveChanges();
            }

            TempData["Success"] = "You have deleted " + title + " page.";

            return RedirectToAction("Index");
        }

        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                PagesDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }
    }
}