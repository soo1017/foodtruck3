using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FoodTruck3.Models.ViewModel.Pages
{
    public class ContactVM
    {
        [Required, Display(Name = "Your name")]
        public string Name { get; set; }
        [Required, Display(Name = "Your email"), EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
    }
}