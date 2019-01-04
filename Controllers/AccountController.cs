using FoodTruck3.Models.Data;
using FoodTruck3.Models.ViewModel.Account;
using FoodTruck3.Models.ViewModel.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace FoodTruck3.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/Account/SignIn");
        }
        
        // GET: Account/SignUp
        [HttpGet]
        public ActionResult SignUp()
        {
            return View("SignUp");
        }

        // GET: Account/SignIn
        [HttpGet]
        public ActionResult SignIn()
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("UserProfile");

            return View();
        }

        // POST: Account/SignUp
        [HttpPost]
        public ActionResult SignUp(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("SignUp", model);
            }

            // Check if password matches
            if (!(model.Password == model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password does not match");
                return View("SignUp", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.Email.Equals(model.Email)))
                {
                    ModelState.AddModelError("", "Your email is already taken");
                    model.Email = "";
                    return View("SignUp", model);
                }

                // Create UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password
                };

                // Add the DTO
                db.Users.Add(userDTO);
                db.SaveChanges();

                // Add to UserRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }
            // Create a TempData Message
            TempData["Success"] = "You are now signed up.";
            // Redirect
            return Redirect("~/Account/SignIn");
        }

        // POST: Account/SignIn
        [HttpPost]
        public ActionResult SignIn(SignInVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("SignIn", model);
            }

            bool IsValid = false;

            // Compare with its password
            using (Db db = new Db())
            {
                // Check user and password
                if (db.Users.Any(x => x.Email.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    IsValid = true;
                }
            }
            if (IsValid == false)
            {

                ModelState.AddModelError("", "Invalid Username or Password");
                model.Username = "";
                model.Password = "";
                return View("SignIn", model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
            }
            return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
        }

        // GET: Account/SignOut
        [HttpGet]
        [Authorize]
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/Account/SignIn");
        }

        // GET: Account/UserProfile
        [HttpGet]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Get username
            string userName = User.Identity.Name;
            Console.WriteLine(userName);

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Email.Equals(userName));

                // Build model
                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        // POST: Account/UserProfile
        [HttpPost]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            int id = model.Id;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Password does not match");
                    model.Password = "";
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string userName = User.Identity.Name;

                // Make sure userName is unique
                if (db.Users.Where(x => x.Id != id).Any(x => x.Email.Equals(model.Email)))
                {
                    ModelState.AddModelError("", "Username is already taken");
                    model.Email = "";
                    return View("UserProfile", model);
                }

                UserDTO dto = db.Users.Find(id);

                dto.Id = model.Id;
                dto.Name = model.Name;
                dto.Email = model.Email;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {

                    dto.Password = model.Password;
                }
                db.SaveChanges();
            }

            TempData["Success"] = "You have edited your profile";

            return Redirect("~/Account/UserProfile");
        }

        // GET: Account/UserOrders
        [HttpGet]
        [Authorize(Roles="User")]
        public ActionResult UserOrders()
        {
            // Init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // Get user id
                if (User.Identity.Name != null)
                {
                    UserDTO user = db.Users.Where(x => x.Email.Equals(User.Identity.Name)).FirstOrDefault();
                    int userId = user.Id;

                    // Init list of OrderVM
                    List<OrderVM> orders = db.Orders
                            .Where(x => x.UserId.Equals(userId))
                            .ToArray()
                            .Select(x => new OrderVM(x))
                            .ToList();

                    // Loop thru list of OrderVM
                    foreach (var order in orders)
                    {
                        // Init products dict
                        Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                        // Declare total
                        decimal total = 0m;

                        // Init list of OrderDetailsDTO
                        List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                        // Loop thru list of OrderDetailsDTO
                        foreach (var orderDetails in orderDetailsDTO)
                        {
                            // Get product
                            ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                            // Get product price
                            decimal price = product.Price;

                            // Get product name
                            string productName = product.Name;

                            // Add to products dict
                            productsAndQty.Add(productName, orderDetails.Quantity);

                            // Get total
                            total += orderDetails.Quantity * price;
                        }

                        // Add to OrdersForUserVM
                        ordersForUser.Add(new OrdersForUserVM()
                        {
                            OrderNumber = order.OrderId,
                            Total = total,
                            ProductsAndQty = productsAndQty,
                            CreatedAt = order.CreatedAt
                        });
                    }
                }
                else
                {
                    return Redirect("~/Account/UserProfile");
                }

                
            }

            return View(ordersForUser);
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("UserProfile");

            return View();
        }

        private string CreateConfirmationToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            string token;
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword", model);
            }

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Email.Equals(model.Email));

                if(dto == null) {
                    ModelState.AddModelError("", "Invalid email");
                    model.Email = "";
                    return View("ForgotPassword", model);
                }

                token = CreateConfirmationToken();
                dto.ConfirmationToken = token;
                dto.IsConfirmed = true;

                db.SaveChanges();
            }

            string ResetMessage = "Please click the link below to reset your password\n"
                + "http://localhost:37955//Account/ForgotPasswordConfirm/" + token + "\n";
            var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message = new MailMessage();
                message.To.Add(new MailAddress(model.Email));  // replace with valid value 
                message.From = new MailAddress("user@gmail.com");  // replace with valid value
                message.Subject = "Password Reset for UpTaste FoodTruck";
                message.Body = string.Format(body, "UpTaste", model.Email, ResetMessage);
                message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "user@gmail.com",  // replace with valid value
                    Password = "password"  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);

                TempData["Success"] = "An email sent to you";

                return Redirect("~/Account/SignIn");
            }
        }

        // GET: Account/ForgotPasswordConfirm
        [HttpGet]
        public ActionResult ForgotPasswordConfirm(string token)
        {
            UserVM model;

            using (Db db = new Db())
            {
                if (!db.Users.Any(x => x.ConfirmationToken.Equals(token))) {
                    ModelState.AddModelError("", "Cannot find your account");
                    return Redirect("~/Account/SignIn");
                }

                UserDTO userDTO = db.Users.Where(x => x.ConfirmationToken.Equals(token)).FirstOrDefault(x => x.IsConfirmed == true);
                
                if (userDTO == null)
                {
                    ModelState.AddModelError("", "Your password reset has expired");
                    return Redirect("~/Account/SignIn");
                }

                model = new UserVM(userDTO);
            }

            return View(model);
        }

        // Post: Account/ForgotPasswordConfirm
        [HttpPost]
        public ActionResult ForgotPasswordConfirm(UserVM model)
        {
            int id = model.Id;

            if (!ModelState.IsValid)
            {
                return View("ForgotPasswordConfirm", model);
            }

            if (!(model.Password == model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password does not match");
                model.Password = "";
                model.ConfirmPassword = "";
                return View("ForgotPasswordConfirm", model);
            }

            using (Db db = new Db())
            {
                UserDTO userDTO = db.Users.Where(x => x.Email.Equals(model.Email)).FirstOrDefault(x => x.Id.Equals(id));

                if (userDTO == null || userDTO.IsConfirmed == false)
                {
                    ModelState.AddModelError("", "Your account is wrong");
                    model.Password = "";
                    model.ConfirmPassword = "";
                    return View("ForgotPasswordConfirm", model);
                }

                userDTO.Password = model.Password;
                userDTO.IsConfirmed = false;

                db.SaveChanges();
            }

            return Redirect("~/Account/SignIn");
        }
    }
}