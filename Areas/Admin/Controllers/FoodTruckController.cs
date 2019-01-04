using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Foodtruck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FoodTruckController : Controller
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

        // GET: Admin/FoodTruck
        public ActionResult Foodtruck()
        {
            List<FoodtruckVM> foodtruckVMList;

            using (Db db = new Db())
            {
                foodtruckVMList = db.Foodtrucks
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new FoodtruckVM(x))
                    .ToList();
            }

            return View(foodtruckVMList);
        }

        // GET: Admin/FoodTruck
        [HttpGet]
        public ActionResult AddFTLocation()
        {
            FoodtruckVM model = new FoodtruckVM();

            return View(model);
        }

        // POST: Admin/FoodTruck
        [HttpPost]
        public ActionResult AddFTLocation(FoodtruckVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Declare PagesDTO
                FoodtruckDTO dto = new FoodtruckDTO();

                if (db.Foodtrucks.Any(x => x.Day == model.Day))
                {
                    ModelState.AddModelError("", "This day is already taken!!");
                    return View(model);
                }
                var day = UppercaseFirst(model.Day.Trim());
                if (!(day == "Monday" || day == "Tuesday" || day == "Wednesday" || day == "Thursday" || day == "Friday" || day == "Saturday" || day == "Sunday"))
                {
                    ModelState.AddModelError("", "Please insert correct day");
                    return View(model);
                }
                else
                {
                    switch (day)
                    {
                        case "Monday":
                            dto.Sorting = 1;
                            break;
                        case "Tuesday":
                            dto.Sorting = 2;
                            break;
                        case "Wednesday":
                            dto.Sorting = 3;
                            break;
                        case "Thursday":
                            dto.Sorting = 4;
                            break;
                        case "Friday":
                            dto.Sorting = 5;
                            break;
                        case "Saturday":
                            dto.Sorting = 6;
                            break;
                        case "Sunday":
                            dto.Sorting = 7;
                            break;
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }
                }
                dto.Day = day;
                dto.Location = model.Location;

                db.Foodtrucks.Add(dto);
                db.SaveChanges();
            }

            TempData["Success"] = "You have added a new FT location.";

            return RedirectToAction("AddFTLocation");
        }

        // GET: Admin/FoodTruck
        [HttpGet]
        public ActionResult EditFTLocation(int id)
        {
            FoodtruckVM model;

            using (Db db = new Db())
            {
                FoodtruckDTO dto = db.Foodtrucks.Find(id);

                if (dto == null)
                {
                    return Content("The FT location does not exist.");
                }

                model = new FoodtruckVM(dto);
            }

            return View(model);
        }

        // POST: Admin/FoodTruck
        [HttpPost]
        public ActionResult EditFTLocation(FoodtruckVM model)
        {
            int id = model.Id;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Declare PagesDTO
                FoodtruckDTO dto = db.Foodtrucks.Find(id);

                if (dto == null)
                {
                    ModelState.AddModelError("", "This day does not exist.");
                    return View(model);
                }
                var day = UppercaseFirst(model.Day.Trim());
               
                    switch (day)
                    {
                        case "Monday":
                            dto.Sorting = 1;
                            break;
                        case "Tuesday":
                            dto.Sorting = 2;
                            break;
                        case "Wednesday":
                            dto.Sorting = 3;
                            break;
                        case "Thursday":
                            dto.Sorting = 4;
                            break;
                        case "Friday":
                            dto.Sorting = 5;
                            break;
                        case "Saturday":
                            dto.Sorting = 6;
                            break;
                        case "Sunday":
                            dto.Sorting = 7;
                            break;
                        default:
                            Console.WriteLine("Default case");
                            break;
                    }
                
       
                dto.Location = model.Location;

                db.SaveChanges();
            }

            TempData["Success"] = "You have edited a FT location.";

            return RedirectToAction("EditFTLocation");
        }

        // GET: Admin/FoodTruck
        [HttpGet]
        public ActionResult DeleteFTLocation(int id)
        {

            using (Db db = new Db())
            {
                FoodtruckDTO dto = db.Foodtrucks.Find(id);

                if (dto == null)
                {
                    return Content("The FT location does not exist.");
                }

                db.Foodtrucks.Remove(dto);
                db.SaveChanges();
            }

            TempData["Success"] = "You have deleted a FT location.";

            return RedirectToAction("Foodtruck");
        }
    }
}