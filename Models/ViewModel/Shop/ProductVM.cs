using FoodTruck3.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Models.ViewModel.Shop
{
    public class ProductVM
    {
        public ProductVM() {
        }
        public ProductVM(ProductDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Description = row.Description;
            Price = row.Price;
            CategoryName = row.CategoryName;
            CategoryId = row.CategoryId;
            Image = row.Image;
            InitialQty = row.InitialQty;
            CurrentQty = row.CurrentQty;
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public string Image { get; set; }
        public int InitialQty { get; set; }
        public int CurrentQty { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}