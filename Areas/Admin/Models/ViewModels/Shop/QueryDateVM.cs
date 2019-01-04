using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FoodTruck3.Areas.Admin.Models.ViewModels.Shop
{
    public class QueryDateVM
    {
        public QueryDateVM()
        {
        }
        [Required]
        public DateTime Date { get; set; }
    }
}