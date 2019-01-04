using FoodTruck3.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FoodTruck3.Models.ViewModel.Foodtruck
{
    public class FoodtruckVM
    {
        public FoodtruckVM()
        {
        }
        public FoodtruckVM(FoodtruckDTO row)
        {
            Id = row.Id;
            Day = row.Day;
            Location = row.Location;
            Sorting = row.Sorting;
        }
        public int Id { get; set; }
        [Required]
        public string Day { get; set; }
        [Required]
        public string Location { get; set; }
        public int Sorting { get; set; }
    }
}