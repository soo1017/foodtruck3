using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FoodTruck3.Models.Data
{
    [Table("tblFoodtruck")]
    public class FoodtruckDTO
    {
        [Key]
        public int Id { get; set; }
        public string Day { get; set; }
        public string Location { get; set; }
        public int Sorting { get; set; }
    }
}