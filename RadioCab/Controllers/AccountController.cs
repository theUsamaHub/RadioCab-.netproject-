using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RadioCab.Models;
using System.Security.Claims;


namespace RadioCab.Controllers
{
    public class AccountController : Controller
    {

        private readonly RadioCabContext context;

        public AccountController(RadioCabContext context)
        {
           this.context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateAdmin()
        {
            var check = context.Users.FirstOrDefault(x => x.Email == "admin@gmail.com");
            if (check == null)
            {
                var register = new User
                {
                    FullName = "Super Admin",
                    Email = "admin@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Phone = "89856427890",
                    CreatedAt = DateTime.Now,
                    Status = "Active",
                    Role = "Admin"
                };

                context.Users.Add(register);
                context.SaveChanges();
                return RedirectToAction("Login");
            }
            return Content("Admin already exists in database.");
        }



        public IActionResult CompanyRegister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CompanyRegister(UserValidate vm)
        {

            var userExists = context.Users.FirstOrDefault(u => u.Email == vm.Email || u.Phone == vm.Phone);

            if (userExists != null)
            {
                if (userExists.Email == vm.Email)
                    ModelState.AddModelError("Email", "Email already exists");

                if (userExists.Phone == vm.Phone)
                    ModelState.AddModelError("Phone", "Phone number already exists");

                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Phone = vm.Phone,
                Role = "Company",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login");
        }


        public IActionResult DriverRegister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DriverRegister(UserValidate vm)
        {

            var userExists = context.Users.FirstOrDefault(u => u.Email == vm.Email || u.Phone == vm.Phone);

            if (userExists != null)
            {
                if (userExists.Email == vm.Email)
                    ModelState.AddModelError("Email", "Email already exists");

                if (userExists.Phone == vm.Phone)
                    ModelState.AddModelError("Phone", "Phone number already exists");

                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Phone = vm.Phone,
                Role = "Driver",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserValidate vm)
        {

            var existingEmail = context.Users.Any(u => u.Email == vm.Email);
            var existingPhone = context.Users.Any(p => p.Phone == p.Phone);

            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(vm);
            }

            if (existingPhone != null)
            {
                ModelState.AddModelError("Phone", "Phone Number already exists");
                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(vm.Password),   
                Phone = vm.Phone,
                Role = "User",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            context.Users.Add(user);
            context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(User vm)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == vm.Email);

            if (user == null)
            {
                ViewBag.Error = "Email Not Exits";
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(vm.Password, user.Password))
            {
                ViewBag.Error = "Invalid  password";
                return View();
            }

      
            var claims = new List<Claim>
           {
               new Claim(ClaimTypes.Name, user.FullName),
               new Claim(ClaimTypes.Email, user.Email),
               new Claim(ClaimTypes.MobilePhone, user.Phone),
               new Claim("UserId", user.UserID.ToString()),
               new Claim(ClaimTypes.Role, user.Role)

            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));
            
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("FullName", user.FullName);
            var comp = context.Companies.FirstOrDefault(c => c.UserId == user.UserID);
           var driver = context.Drivers.FirstOrDefault(d => d.UserId == user.UserID);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (user.Role == "Driver")
            {
                if (driver == null)
                {
                    return RedirectToAction("MembershipPlans", "Driver");
                }
                var membership = context.Memberships.FirstOrDefault(m => m.MembershipId == driver.MembershipId);
                if (membership == null)
                {
                    return RedirectToAction("MembershipPlans", "Driver");
                }
                if (membership.MembershipName == "Free")
                {
                    return RedirectToAction("Index", "Driver");
                }
                var payment = context.Payments
                    .Join(context.PaymentAmounts,
                    p => p.PaymentAmountId, pa => pa.PaymentAmountId,
                    (p, pa) => new { p, pa })
                    .Where(x => x.p.UserId == driver.UserId
                    && x.pa.MembershipId == driver.MembershipId).OrderByDescending(x => x.p.ExpiryDate).FirstOrDefault();
                if (payment == null)
                {
                    return RedirectToAction("Payment", "Driver", new { membershipId = driver.MembershipId });
                }
                if (payment.p.ExpiryDate < DateTime.Now)
                {
                    return RedirectToAction("MembershipPlans", "Driver");
                }

                return RedirectToAction("Index", "Driver");
            }
            else if (user.Role == "Company")
            {
                if(comp == null)
                {
                    return RedirectToAction("MembershipPlans", "Company");
                }
                var membership = context.Memberships.FirstOrDefault(m => m.MembershipId == comp.MembershipId);
                if(membership == null)
                {
                    return RedirectToAction("MembershipPlans", "Company");
                }
                if(membership.MembershipName == "Free")
                {
                    return RedirectToAction("Index", "Company");
                }
                var payment = context.Payments
                    .Join(context.PaymentAmounts,
                    p => p.PaymentAmountId, pa => pa.PaymentAmountId,
                    (p, pa) => new { p, pa })
                    .Where(x => x.p.UserId == comp.UserId 
                    && x.pa.MembershipId == comp.MembershipId).OrderByDescending(x=>x.p.ExpiryDate).FirstOrDefault();
                if(payment == null)
                {
                    return RedirectToAction("Payment", "Company", new { membershipId = comp.MembershipId });
                }
                if(payment.p.ExpiryDate < DateTime.Now)
                {
                    return RedirectToAction("MembershipPlans", "Company");
                }

                return RedirectToAction("Index", "Company");
            }
            else if (user.Role == "User")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Invalid Credantials";
                return View();
            }
        }


        public IActionResult RegCard()
        {
            // Fetch basic membership ID (usually ID 2 as per your data)
            var basicMembershipId = context.Memberships
                .Where(m => m.MembershipName == "Basic" || m.MembershipName == "Basic Membership")
                .Select(m => m.MembershipId)
                .FirstOrDefault();

            if (basicMembershipId == 0)
            {
                // Fallback to MembershipId = 2 if not found
                basicMembershipId = 2;
            }

            // Fetch payment amounts for basic membership only
            var companyPayment = context.PaymentAmounts
                .Where(p => p.EntityType == "Company" &&
                           p.IsActive &&
                           p.MembershipId == basicMembershipId)
                .OrderBy(p => p.PaymentType)
                .ToList();

            var driverPayment = context.PaymentAmounts
                .Where(p => p.EntityType == "Driver" &&
                           p.IsActive &&
                           p.MembershipId == basicMembershipId)
                .OrderBy(p => p.PaymentType)
                .ToList();

            ViewBag.CompanyPayment = companyPayment;
            ViewBag.DriverPayment = driverPayment;

            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

    }
}
