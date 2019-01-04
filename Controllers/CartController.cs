using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace FoodTruck3.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Cart is empty.";
                return View();
            }

            // Calculate total and save to ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Return view with model
            return View(cart);
        }

        // Get
        public ActionResult CartPartial()
        {
            // Init CartVM
            CartVM model = new CartVM();

            // Init quantity 
            int qty = 0;

            // Init price
            decimal price = 0m;

            // Check for cart session
            if (Session["cart"] != null)
            {
                // Get total cart session
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // Or set qty and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            // Return partial view with model
            return PartialView(model);
        }

        // Get
        [HttpGet]
        public ActionResult AddToCartPartial(int id)
        {
            // Init CArtVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Init CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Get the product
                ProductDTO product = db.Products.Find(id);

                // Check if this product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // If not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price
                    });
                }
                else
                {
                    // If it is, increment
                    productInCart.Quantity++;
                }
            }

            // Get total qty and price and add to model

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            // Return partial view with model
            return PartialView(model);
        }

        // Get: Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Increment qty
                model.Quantity++;

                // Store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                // return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }

        // Get: Cart/DecrementProduct
        [HttpGet]
        public JsonResult DecrementProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                if (model.Quantity > 1)
                {
                    // Decrement qty
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                var result = new { qty = model.Quantity, price = model.Price };

                // return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Get: Cart/DeleteProduct
        [HttpGet]
        public void DeleteProduct(int productId)
        {
            // Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        // Get: Cart/PaypalPartial
        [HttpGet]
        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            return PartialView(cart);
        }

        // Post: Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            bool IsStockShort = false;
            List<string> stockShortMessage = new List<string>();

            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get username
            string userName;
            if (User.Identity.Name != null)
            {
                userName = User.Identity.Name;
            }

            // Declare orderId
            int orderId = 0;

            using(Db db = new Db())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user Id
                int userId = 0;
                if (User.Identity.Name != null)
                {
                    var q = db.Users.FirstOrDefault(x => x.Email.Equals(User.Identity.Name));
                    if (q != null)
                    {
                        userId = q.Id;
                    }
                    orderDTO.UserId = userId;
                }
                else
                {
                    orderDTO.UserId = userId;
                }
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Check if there is enough in stock
                foreach (var item in cart)
                {
                    ProductDTO productDTO = db.Products.Find(item.ProductId);
                    int tempQty = productDTO.CurrentQty;
                    tempQty -= item.Quantity;

                    if (tempQty < 0)
                    {
                        IsStockShort = true;
                        int shortage = item.Quantity - productDTO.CurrentQty;
                        stockShortMessage.Add(shortage + " shor of " + productDTO.Name);
                    }
                }
                // Add to OrderDetailsDTO
                if (IsStockShort == false)
                {
                    foreach (var item3 in cart)
                    {
                        ProductDTO productDTO = db.Products.Find(item3.ProductId);

                        productDTO.CurrentQty -= item3.Quantity;

                        db.SaveChanges();
                    }

                    foreach (var item in cart)
                    {
                        orderDetailsDTO.OrderId = orderId;
                        orderDetailsDTO.UserId = userId;
                        orderDetailsDTO.ProductId = item.ProductId;
                        orderDetailsDTO.Quantity = item.Quantity;

                        db.OrderDetails.Add(orderDetailsDTO);

                        db.SaveChanges();
                    }
                }
                
            }

            if (IsStockShort == false)
            {
                // Mail to admin
                var client = new SmtpClient("smtp.mailtrap.io", 25)
                {
                    Credentials = new NetworkCredential("?", "?"),

                    EnableSsl = true
                };
                client.Send("admin@example.com", "admin@example.com", "UpTaste- New Order", "You have a new order. Order number is" + orderId);

                // Reset session
                Session["cart"] = null;
            }
            else
            {
                string[] shortMessage = stockShortMessage.ToArray();
                ViewBag.shortMessage = shortMessage;
            }
        }
    }
}