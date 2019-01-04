using FoodTruck3.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FoodTruck3.Models.ViewModel.Account
{
    public class UserVM
    {
        public UserVM()
        {
        }
        public UserVM(UserDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Email = row.Email;
            Password = row.Password;
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}