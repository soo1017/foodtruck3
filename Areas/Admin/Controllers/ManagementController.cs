using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagementController : Controller
    {
        // GET: Admin/Management
        public ActionResult Index()
        {
            return View();
        }
    }
}