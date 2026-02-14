using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RadioCab.Models;

namespace RadioCab.Controllers
{
    public class DriverController : Controller
    {
        private readonly RadioCabContext context;

        private bool IsFreeDriver(int userId)
        {
            return context.Drivers.Any(c =>
                c.UserId == userId &&
                c.MembershipId == 1 &&
                c.RegisterationStatus == "Approved");
        }
        public DriverController(RadioCabContext context)
        {
            this.context = context;
        }


        private IActionResult EnforceDriverAccess()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            // FREE MEMBERSHIP FIRST
            if (driver.MembershipId == 1)
            {
                if (driver.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");

                return null; // ACCESS GRANTED
            }

            // PAID MEMBERSHIP
            var state = GetSubscriptionState(userId.Value);

            if (state == "Pending")
                return RedirectToAction("PaymentPendingApproval");

            if (state == "Expired")
                return RedirectToAction("SubscriptionExpired");

            if (state == "None")
                return RedirectToAction("MembershipPlans");

            if (driver.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            return null; // ACCESS GRANTED
        }

        private Payment GetLatestPayment(int userId, int? membershipId = null)
        {
            var query = context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p =>
                    p.UserId == userId &&
                    p.PaymentPurpose == "Membership");

            if (membershipId.HasValue)
            {
                query = query.Where(p =>
                    p.PaymentAmount.MembershipId == membershipId.Value);
            }

            return query
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();
        }


         private bool HasActiveSubscription(int userId)
        {
            var driver = context.Drivers.FirstOrDefault(c => c.UserId == userId);
            if (driver == null)
                return false;

            if (driver.MembershipId == 1)
                return true;

            var latestPayment = context.Payments
                .Where(p =>
                    p.UserId == userId &&
                    p.PaymentPurpose == "Membership")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            return latestPayment != null
                && latestPayment.PaymentStatus == "Paid"
                && latestPayment.ExpiryDate > DateTime.Now;
        }

        [Authorize(Roles = "Driver")]
      
        public IActionResult SubscriptionExpired()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(c => c.UserId == userId.Value);
            // FREE MEMBERSHIP → REDIRECT TO DASHBOARD
            if (driver == null)
                return RedirectToAction("MembershipPlans");

            if (driver.MembershipId == 1)
                return RedirectToAction("PendingApproval");



            var latestPayment = GetLatestPayment(userId.Value, driver.MembershipId);
            ViewBag.ExpiryDate = latestPayment?.ExpiryDate;

            return View();
        }
        private bool IsApproved()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            return driver != null && driver.RegisterationStatus == "Approved";
        }

        //    if (!IsApproved())
        //return RedirectToAction("PendingApproval");


        private string GetSubscriptionState(int userId)
        {
            var Driver = context.Drivers.FirstOrDefault(c => c.UserId == userId);
            if (Driver == null)
                return "None";

            // FREE MEMBERSHIP → NEVER EXPIRES, NO PAYMENT CHECK
            if (Driver.MembershipId == 1)
                return "Active";

            var latestPayment = context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p =>
                    p.UserId == userId &&
                    p.PaymentPurpose == "Membership" &&
                    p.PaymentAmount.MembershipId == Driver.MembershipId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();


            if (latestPayment == null)
                return "None";

            if (latestPayment.PaymentStatus == "Pending")
                return "Pending";

            if ((latestPayment.PaymentStatus == "Paid" || latestPayment.PaymentStatus == "Approved")
                && latestPayment.ExpiryDate > DateTime.Now)
                return "Active";

            return "Expired";
        }

        [Authorize(Roles = "Driver")]
        public IActionResult Index()
        {

            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
           
            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (driver.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            var driverId = driver.DriverId;

            ViewBag.totalPayment = context.Payments.Where(p => p.UserId == userId).Count();

            // ✅ CHANGE: Use DriverId instead of UserId for feedback
            ViewBag.FeedBack = context.DriverFeedbacks
                .Where(f => f.DriverId == driverId) // Changed from UserId to DriverId
                .Count();

            ViewBag.Service = context.DriverServices
                .Where(s => s.DriverId == driverId && s.IsActive == true)
                .Count();

            var model = new DriverFeedbackandPayment
            {
                Payment = context.Payments
                    .Where(p => p.UserId == userId.Value)
                    .Take(3)
                    .ToList(),

                // ✅ CHANGE: Use DriverId instead of UserId
                DriverFeedback = context.DriverFeedbacks
                    .Where(f => f.DriverId == driverId) // Changed from UserId to DriverId
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(3)
                    .ToList()
            };
            return View(model);
        }
        [Authorize(Roles = "Driver")]
        public IActionResult PendingApproval()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            // 🔒 FREE MEMBERSHIP FIRST
            if (driver.MembershipId == 1)
            {
                if (driver.RegisterationStatus == "Approved")
                    return RedirectToAction("Index");

                return View();
            }

            // PAID MEMBERSHIP CHECK
            var state = GetSubscriptionState(userId.Value); // Should handle driver subscriptions

            if (state == "Pending")
                return RedirectToAction("PaymentPendingApproval");

            if (state == "Expired")
                return RedirectToAction("SubscriptionExpired");

            if (state == "None")
                return RedirectToAction("MembershipPlans");

            if (driver.RegisterationStatus == "Approved")
                return RedirectToAction("Index");

            return View();
        }

        // API endpoint for AJAX polling
        [Authorize(Roles = "Driver")]
        public IActionResult CheckStatus()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { status = "NoUser" });

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return Json(new { status = "NoDriver" });

            return Json(new { status = driver.RegisterationStatus });
        }
        [Authorize(Roles = "Driver")]
        public IActionResult MembershipPlans()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);

            // 🔒 Check subscription state
            var latestPayment = context.Payments
                .Where(p => p.UserId == userId.Value && p.PaymentPurpose == "Membership")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            var state = GetSubscriptionState(userId.Value); // same method as CompanyController

            if (state == "Expired")
                return RedirectToAction("SubscriptionExpired");

            if (state == "Pending")
                return RedirectToAction("PaymentPendingApproval");

           
            // Load memberships and pricing
            var memberships = context.Memberships
                  .Where(m => m.IsActive == true)
                .Include(m => m.MembershipFeatures)
                    .ThenInclude(mf => mf.Feature)
                .ToList();

            var price = context.PaymentAmounts
                .Where(p => p.EntityType == role)
                .ToList();

            bool hasActiveSubscription =
                latestPayment != null &&
                (latestPayment.PaymentStatus == "Paid" || latestPayment.PaymentStatus == "Approved") &&
                latestPayment.ExpiryDate > DateTime.Now;

            ViewBag.Price = price;
            ViewBag.Role = role;
            ViewBag.HasDriverProfile = driver != null;
            ViewBag.CurrentMembershipId = driver?.MembershipId;
            ViewBag.HasActiveSubscription = hasActiveSubscription;
            ViewBag.RegisterationStatus = driver?.RegisterationStatus;

            return View(memberships);
        }

        [HttpGet]
        public IActionResult ProfileRegister(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new DriverValidate
            {
                MembershipId = id,
                DriverName = user.FullName,
                Email = user.Email,
                Telephone = user.Phone
            };

            ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");

            return View(model);
        }

        [HttpPost]
        public IActionResult ProfileRegister(DriverValidate dv, IFormFile driverPhoto, IFormFile licenseFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Duplicate checks
            var driverExist = context.Drivers.FirstOrDefault(d =>
                d.Cnic == dv.Cnic ||
                d.DrivingLicenseNumber == dv.DrivingLicenseNumber ||
                d.VehicleInfo.Contains(dv.VehiclePlate));

            if (driverExist != null)
            {
                if (driverExist.Cnic == dv.Cnic)
                    ModelState.AddModelError("", "CNIC already exists");
                if (driverExist.DrivingLicenseNumber == dv.DrivingLicenseNumber)
                    ModelState.AddModelError("", "Driving License Number already exists");
                if (driverExist.VehicleInfo.Contains(dv.VehiclePlate))
                    ModelState.AddModelError("", "Vehicle Plate already exists");

                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
                return View(dv);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
                return View(dv);
            }

            // File uploads
            string UploadFile(IFormFile file, string folder)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var filePath = Path.Combine(path, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Path.Combine("/" + folder, fileName).Replace("\\", "/");
            }

            var dbDriverPhoto = UploadFile(driverPhoto, "Upload/DriverPhotos");
            var dbLicense = UploadFile(licenseFile, "Upload/Driverlicense");

            // Build vehicle info string
            dv.VehicleInfo = $"{dv.VehicleMake} | {dv.VehicleModel} | {dv.VehicleYear} | {dv.VehiclePlate} | {dv.VehicleColor}";

            // Create driver
            var driver = new Driver
            {
                UserId = userId.Value,
                MembershipId = dv.MembershipId,
                DriverName = user.FullName,
                Address = dv.Address,
                Cnic = dv.Cnic,
                CityId = dv.CityId,
                Email = user.Email,
                Telephone = user.Phone,
                Experience = dv.Experience,
                DrivingLicenseNumber = dv.DrivingLicenseNumber,
                Description = dv.Description,
                VehicleInfo = dv.VehicleInfo,
                DriverPhoto = dbDriverPhoto,
                DrivingLicenseFile = dbLicense,
                RegisterationStatus = "Pending" // default
            };

            context.Drivers.Add(driver);
            context.SaveChanges();

            // Redirect based on membership type
            var membership = context.Memberships.FirstOrDefault(m => m.MembershipId == dv.MembershipId);
            if (membership != null && membership.MembershipName.ToLower() == "free")
                return RedirectToAction("Index", "Driver");

            return RedirectToAction("Payment", "Driver", new { membershipid = driver.MembershipId });
        }
        [HttpGet]
        public IActionResult Payment(int membershipid, bool isRenewal = false, bool isUpgrade = false)
        {
            if (membershipid == 1)
                return RedirectToAction("PendingApproval");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            HttpContext.Session.SetInt32("MembershipId", membershipid);

            var paymentAmounts = context.PaymentAmounts
                .Where(p => p.MembershipId == membershipid && p.EntityType == HttpContext.Session.GetString("Role"))
                .ToList();

            ViewBag.TotalAmount = paymentAmounts.Sum(p => p.Amount);

            ViewBag.Duration = paymentAmounts.Select(pa => new
            {
                pa.PaymentAmountId,
                pa.PaymentType,
                pa.Amount,
                pa.DurationInMonths
            }).ToList();

            ViewBag.MembershipId = membershipid;
            ViewBag.PaymentMethod = new SelectList(context.PaymentMethods.Where(pm => pm.IsActive == true).ToList(), "PaymentMethodId", "MethodName");
            ViewBag.IsRenewal = isRenewal;
            ViewBag.IsUpgrade = isUpgrade;

            return View();
        }

        [HttpPost]
        public IActionResult Payment(PaymentValidate pv, IFormFile paymentScreenshot, int membershipid, bool isRenewal = false)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Duplicate Transaction Check
            if (context.Payments.Any(p => p.TransactionId == pv.TransactionId))
            {
                ModelState.AddModelError("", "Transaction ID already exists.");
                SetupDriverPaymentViewBag(membershipid);
                return View(pv);
            }

            if (!ModelState.IsValid)
            {
                SetupDriverPaymentViewBag(membershipid);
                return View(pv);
            }

            var paymentAmount = context.PaymentAmounts
                .FirstOrDefault(pa => pa.PaymentAmountId == pv.PaymentAmountId);

            if (paymentAmount == null)
            {
                ModelState.AddModelError("", "Invalid payment selection.");
                SetupDriverPaymentViewBag(membershipid);
                return View(pv);
            }

            // File Upload
            string screenshotPath = null;
            if (paymentScreenshot != null && paymentScreenshot.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(paymentScreenshot.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload/Payments");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    paymentScreenshot.CopyTo(stream);
                }
                screenshotPath = "/Upload/Payments/" + fileName;
            }

            // Expiry calculation
            DateTime paymentDate = pv.PaymentDate != default ? pv.PaymentDate : DateTime.Now;
            DateTime expiryDate = paymentDate.AddMonths(paymentAmount.DurationInMonths);

            // Create Payment
            var payment = new Payment
            {
                UserId = userId.Value,
                PaymentAmountId = pv.PaymentAmountId,
                PaymentMethodId = pv.PaymentMethodId,
                TransactionId = pv.TransactionId,
                PaymentDate = paymentDate,
                ExpiryDate = expiryDate,
                PaymentStatus = pv.PaymentStatus,
                PaymentScreenshot = screenshotPath,
                PaymentPurpose = "Membership"
            };

            context.Payments.Add(payment);

            // Update Driver Membership
            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver != null)
                driver.MembershipId = membershipid;

            context.SaveChanges();

            return !isRenewal
                ? RedirectToAction("PaymentPendingApproval")
                : RedirectToAction("MembershipPlans");
        }

        // Setup ViewBag helper
        private void SetupDriverPaymentViewBag(int membershipid)
        {
            var paymentAmounts = context.PaymentAmounts
                .Where(p => p.MembershipId == membershipid && p.EntityType == HttpContext.Session.GetString("Role"))
                .ToList();

            ViewBag.Duration = paymentAmounts.Select(pa => new
            {
                pa.PaymentAmountId,
                pa.PaymentType,
                pa.Amount,
                pa.DurationInMonths
            }).ToList();

            ViewBag.MembershipId = membershipid;
            ViewBag.PaymentMethod = new SelectList(context.PaymentMethods.Where(pm => pm.IsActive == true).ToList(), "PaymentMethodId", "MethodName");
        }

        [Authorize(Roles = "Driver")]
        public IActionResult PaymentPendingApproval()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("MembershipPlans");

            if (driver.MembershipId == 1) // Free membership
                return RedirectToAction("Index");

            // Get latest membership payment
            var latestPayment = context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p => p.UserId == userId.Value && p.PaymentPurpose == "Membership")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            // If payment is already approved or paid → dashboard
            if (latestPayment != null &&
                (latestPayment.PaymentStatus == "Approved" || latestPayment.PaymentStatus == "Paid"))
                return RedirectToAction("Index");

            // If no pending payment → show membership plans
            if (latestPayment == null || latestPayment.PaymentStatus != "Pending")
                return RedirectToAction("MembershipPlans");

            ViewBag.PaymentDate = latestPayment.PaymentDate;
            ViewBag.TransactionId = latestPayment.TransactionId;
            ViewBag.Amount = latestPayment.PaymentAmount?.Amount;

            return View();
        }
        [Authorize(Roles = "Driver")]
        public IActionResult Service()
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            // 🔒 FREE MEMBERSHIP OR SUBSCRIPTION CHECK
            if (driver.MembershipId == 1)
            {
                if (driver.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value); // implement similar to company

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }

            var driverId = driver.DriverId;

            var model = new DriverServiceVM
            {
                DriverServices = context.DriverServices
                    .Include(x => x.Service)
                    .Where(x => x.DriverId == driverId)
                    .ToList(),

                AvailableServices = context.Services
                    .Where(x => x.IsForDriver && x.IsActive)
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Driver")]
        public IActionResult Service(DriverServiceVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driverId = context.Drivers
                .Where(x => x.UserId == userId)
                .Select(x => x.DriverId)
                .FirstOrDefault();

            bool exists = context.DriverServices.Any(x =>
                x.DriverId == driverId &&
                x.ServiceId == vm.DriverSerVal.ServiceId);

            if (exists)
                ModelState.AddModelError("", "Service already selected");

            if (!ModelState.IsValid)
            {
                vm.DriverServices = context.DriverServices
                    .Include(x => x.Service)
                    .Where(x => x.DriverId == driverId)
                    .ToList();

                vm.AvailableServices = context.Services
                    .Where(x => x.IsForDriver && x.IsActive)
                    .ToList();

                return View(vm);
            }

            context.DriverServices.Add(new DriverService
            {
                DriverId = driverId,
                ServiceId = vm.DriverSerVal.ServiceId
            });

            context.SaveChanges();
            TempData["added"] = "Service added successfully";
            return RedirectToAction(nameof(Service));
        }

        [Authorize(Roles = "Driver")]
        public IActionResult DelSer(int id)
        {
            var item = context.DriverServices.FirstOrDefault(x => x.DriverServiceId == id);
            if (item != null)
            {
                context.DriverServices.Remove(item);
                context.SaveChanges();
            }
            return RedirectToAction(nameof(Service));
        }

        [Authorize(Roles = "Driver")]
        public IActionResult Feedback()
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!HasActiveSubscription(userId.Value))
                return RedirectToAction("SubscriptionExpired");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (driver.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            var feedbacks = context.DriverFeedbacks
        .Where(f => f.DriverId == driver.DriverId)
        .OrderByDescending(f => f.CreatedAt)
        .ToList();
            // Calculate rating distribution
            var totalFeedbacks = feedbacks.Count;
            var averageRating = feedbacks.Any() ? Math.Round(feedbacks.Average(f => f.Rating), 1) : 0;

            var ratingDistribution = new Dictionary<int, int>();
            var ratingPercentages = new Dictionary<int, double>();

            for (int i = 1; i <= 5; i++)
            {
                var count = feedbacks.Count(f => f.Rating == i);
                ratingDistribution[i] = count;
                ratingPercentages[i] = totalFeedbacks > 0 ? Math.Round((count * 100.0) / totalFeedbacks, 1) : 0;
            }

            ViewBag.Feedbacks = feedbacks;
            ViewBag.AverageRating = averageRating;
            ViewBag.TotalFeedbacks = totalFeedbacks;
            ViewBag.RatingDistribution = ratingDistribution;
            ViewBag.RatingPercentages = ratingPercentages;

            return View();
        }

        [Authorize(Roles = "Driver")]
        public IActionResult ContactRequests(string statusFilter = "", DateTime? dateFrom = null, DateTime? dateTo = null)
        {


            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (!HasActiveSubscription(userId.Value))
                return RedirectToAction("SubscriptionExpired");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (driver.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            var query = context.ContactRequests
                .Where(r => r.TargetType == "Driver" && r.TargetId == driver.DriverId);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                query = query.Where(r => r.Status == statusFilter);

            if (dateFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.CreatedAt <= dateTo.Value.AddDays(1));

            var requests = query.OrderByDescending(r => r.CreatedAt).ToList();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.NewRequestsCount = context.ContactRequests
                .Count(r => r.TargetType == "Driver" &&
                            r.TargetId == driver.DriverId &&
                            r.Status == "New");

            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult MarkAsViewed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);

            if (driver == null) return Json(new { success = false });

            var request = context.ContactRequests
                .FirstOrDefault(r => r.ContactRequestId == id &&
                                     r.TargetType == "Driver" &&
                                     r.TargetId == driver.DriverId);

            if (request != null && request.Status == "New")
            {
                request.Status = "Viewed";
                context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);

            if (driver == null) return Json(new { success = false });

            var request = context.ContactRequests
                .FirstOrDefault(r => r.ContactRequestId == id &&
                                     r.TargetType == "Driver" &&
                                     r.TargetId == driver.DriverId);

            if (request != null)
            {
                request.Status = status;
                context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        // ===============================
        // ADVERTISEMENT MANAGEMENT
        // ===============================

        [Authorize(Roles = "Driver")]
        public IActionResult AdvertisementList()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            // Authorization checks
            if (!IsAuthorizedForAdvertisement(driver, userId.Value))
                return RedirectToAction("Index");

            var advertisements = context.Advertisements
                .Include(a => a.Payment)
                .Where(a => a.AdvertiserType == "Driver" && a.AdvertiserId == driver.DriverId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdvertisementListVM
                {
                    AdvertisementId = a.AdvertisementId,
                    Title = a.Title,
                    Description = a.Description,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    AdImage = a.AdImage,
                    ApprovalStatus = a.ApprovalStatus,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    PaymentStatus = a.Payment != null ? a.Payment.PaymentStatus : "Unpaid",
                    CanEdit = a.StartDate > DateTime.Now,
                    ActiveAdCount = context.Advertisements.Count(x => x.AdvertiserType == "Driver" && 
                                                                    x.AdvertiserId == driver.DriverId && 
                                                                    x.IsActive && 
                                                                    x.StartDate <= DateTime.Now && 
                                                                    x.EndDate >= DateTime.Now)
                })
                .ToList();

            ViewBag.MaxActiveAds = GetMaxActiveAds(driver.MembershipId);
            ViewBag.MembershipId = driver.MembershipId;

            return View(advertisements);
        }

        [Authorize(Roles = "Driver")]
        public IActionResult CreateAdvertisement()
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (!IsAuthorizedForAdvertisement(driver, userId.Value))
                return RedirectToAction("Index");

            // Check active ad limit
            if (!CanCreateAdvertisement(driver.DriverId, driver.MembershipId))
            {
                TempData["Error"] = $"You have reached the maximum limit of {GetMaxActiveAds(driver.MembershipId)} active advertisements.";
                return RedirectToAction("AdvertisementList");
            }

            var userMembershipId = driver.MembershipId;
            ViewBag.PaymentAmounts = context.PaymentAmounts
                .Where(p => p.EntityType == "Advertiser" && p.MembershipId == userMembershipId)
                .ToList();

            ViewBag.PaymentMethods = context.PaymentMethods.Where(p=>p.IsActive == true).ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult CreateAdvertisement(AdvertisementCreateVM model, IFormFile adImage, IFormFile paymentScreenshot, int paymentAmountId, int paymentMethodId, string transactionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (!IsAuthorizedForAdvertisement(driver, userId.Value))
                return RedirectToAction("Index");

            // Validate image upload
            if (adImage == null || adImage.Length == 0)
            {
                ModelState.AddModelError("", "Advertisement image is required.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }

            // Check active ad limit
            if (!CanCreateAdvertisement(driver.DriverId, driver.MembershipId))
            {
                TempData["Error"] = $"You have reached the maximum limit of {GetMaxActiveAds(driver.MembershipId)} active advertisements.";
                return RedirectToAction("AdvertisementList");
            }

            // Basic validation for ad creation
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError("", "Title and Description are required.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Start date cannot be in the past.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }

            // Validate payment details
            if (paymentAmountId == 0 || paymentMethodId == 0 || string.IsNullOrWhiteSpace(transactionId))
            {
                ModelState.AddModelError("", "Payment details are required.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }

            // Validate payment screenshot
            if (paymentScreenshot == null || paymentScreenshot.Length == 0)
            {
                ModelState.AddModelError("", "Payment screenshot is required.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }

            try
            {
                // Upload advertisement image
                var imagePath = UploadFileSafely(adImage, "Upload/Advertisements", 5242880); // 5MB limit

                // Upload payment screenshot
                var screenshotPath = UploadFileSafely(paymentScreenshot, "Upload/Payments", 5242880); // 5MB limit

                // Validate payment amount exists for advertisements with user's membership
                var paymentAmount = context.PaymentAmounts
                    .FirstOrDefault(pa => pa.PaymentAmountId == paymentAmountId && 
                                         pa.EntityType == "Advertiser" && 
                                         pa.MembershipId == driver.MembershipId);

                if (paymentAmount == null)
                {
                    ModelState.AddModelError("", "Invalid payment amount selected.");
                    SetupAdvertisementViewBag(driver.MembershipId);
                    return View(model);
                }

                // Create advertisement
                var advertisement = new Advertisement
                {
                    AdvertiserType = "Driver",
                    AdvertiserId = driver.DriverId,
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.StartDate.AddMonths(paymentAmount.DurationInMonths), // Set end date based on payment duration
                    AdImage = imagePath,
                    ApprovalStatus = "Pending",
                    IsActive = false,
                    CreatedAt = DateTime.Now
                };

                context.Advertisements.Add(advertisement);
                context.SaveChanges(); // Generate AdvertisementId

                // Create payment record
                var payment = new Payment
                {
                    UserId = userId.Value,
                    PaymentAmountId = paymentAmountId,
                    PaymentMethodId = paymentMethodId,
                    TransactionId = transactionId,
                    PaymentDate = DateTime.Now,
                    ExpiryDate = advertisement.EndDate,
                    PaymentStatus = "Pending",
                    PaymentScreenshot = screenshotPath,
                    PaymentPurpose = "Advertisement"
                };

                context.Payments.Add(payment);
                context.SaveChanges(); // Generate PaymentId

                // Link payment to advertisement
                advertisement.PaymentId = payment.PaymentId;
                context.SaveChanges();

                TempData["Success"] = "Advertisement created successfully! Payment is pending approval.";
                return RedirectToAction("AdvertisementList");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while creating the advertisement. Please try again.");
                SetupAdvertisementViewBag(driver.MembershipId);
                return View(model);
            }
        }

        [Authorize(Roles = "Driver")]
        public IActionResult EditAdvertisement(int id)
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (!IsAuthorizedForAdvertisement(driver, userId.Value))
                return RedirectToAction("Index");

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == id && 
                                   a.AdvertiserType == "Driver" && 
                                   a.AdvertiserId == driver.DriverId);

            if (advertisement == null)
                return NotFound();

            // Check if edit is allowed (before StartDate)
            if (advertisement.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot edit advertisement after start date. Only active/inactive toggle is allowed.";
                return RedirectToAction("AdvertisementList");
            }

            var model = new AdvertisementEditVM
            {
                AdvertisementId = advertisement.AdvertisementId,
                Title = advertisement.Title,
                Description = advertisement.Description,
                StartDate = advertisement.StartDate,
                ExistingImage = advertisement.AdImage
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult EditAdvertisement(int id, AdvertisementEditVM model, IFormFile? newImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return RedirectToAction("ProfileRegister", "Driver");

            if (!IsAuthorizedForAdvertisement(driver, userId.Value))
                return RedirectToAction("Index");

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == model.AdvertisementId && 
                                   a.AdvertiserType == "Driver" && 
                                   a.AdvertiserId == driver.DriverId);

            if (advertisement == null)
                return NotFound();

            // Check if edit is allowed (before StartDate)
            if (advertisement.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot edit advertisement after start date. Only active/inactive toggle is allowed.";
                return RedirectToAction("AdvertisementList");
            }


            // Update fields
            advertisement.Title = model.Title;
            advertisement.Description = model.Description;
            advertisement.StartDate = model.StartDate;

            // Handle new image upload if provided
            if (newImage != null && newImage.Length > 0)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(advertisement.AdImage))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", advertisement.AdImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Upload new image
                var fileName = Guid.NewGuid() + Path.GetExtension(newImage.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Upload/Advertisements");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    newImage.CopyTo(stream);
                }

                advertisement.AdImage = "/Upload/Advertisements/" + fileName;
            }

            context.SaveChanges();

            return RedirectToAction("AdvertisementList");
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult ToggleAdvertisementStatus(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Unauthorized" });

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return Json(new { success = false, message = "Driver not found" });

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == id &&
                                    a.AdvertiserType == "Driver" &&
                                    a.AdvertiserId == driver.DriverId);

            if (advertisement == null)
                return Json(new { success = false, message = "Advertisement not found" });

            // Only allow toggle if StartDate has passed or payment is made
            if (advertisement.StartDate > DateTime.Now && advertisement.PaymentId == null)
                return Json(new { success = false, message = "Cannot toggle status before start date or without payment" });

            // Check max active ads if activating
            if (!advertisement.IsActive) // About to activate
            {
                int maxActive = GetMaxActiveAds(driver.MembershipId);
                if (maxActive != -1) // Not unlimited
                {
                    var currentActive = context.Advertisements.Count(a =>
                        a.AdvertiserType == "Driver" &&
                        a.AdvertiserId == driver.DriverId &&
                        a.IsActive &&
                        a.StartDate <= DateTime.Now &&
                        a.EndDate >= DateTime.Now);

                    if (currentActive >= maxActive)
                        return Json(new
                        {
                            success = false,
                            message = $"Cannot activate ad. Maximum of {maxActive} active advertisements reached."
                        });
                }
            }

            advertisement.IsActive = !advertisement.IsActive;
            context.SaveChanges();

            return Json(new
            {
                success = true,
                isActive = advertisement.IsActive,
                message = $"Advertisement {(advertisement.IsActive ? "activated" : "deactivated")} successfully."
            });
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public IActionResult DeleteAdvertisement(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Unauthorized" });

            var driver = context.Drivers.FirstOrDefault(d => d.UserId == userId.Value);
            if (driver == null)
                return Json(new { success = false, message = "Driver not found" });

            var advertisement = context.Advertisements
                .Include(a => a.Payment)
                .FirstOrDefault(a => a.AdvertisementId == id && 
                                   a.AdvertiserType == "Driver" && 
                                   a.AdvertiserId == driver.DriverId);

            if (advertisement == null)
                return Json(new { success = false, message = "Advertisement not found" });

            // Business rule: Cannot delete if advertisement is active and has started
            if (advertisement.IsActive && advertisement.StartDate <= DateTime.Now)
            {
                return Json(new { success = false, message = "Cannot delete active advertisement. Please deactivate it first." });
            }

            // Business rule: Cannot delete if payment is approved and active
            if (advertisement.Payment != null && advertisement.Payment.PaymentStatus == "Paid" && advertisement.IsActive)
            {
                return Json(new { success = false, message = "Cannot delete paid active advertisement. Please contact support." });
            }

            try
            {
                // Delete associated image file
                if (!string.IsNullOrEmpty(advertisement.AdImage))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", advertisement.AdImage.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Delete payment record if exists
                if (advertisement.Payment != null)
                {
                    // Delete payment screenshot if exists
                    if (!string.IsNullOrEmpty(advertisement.Payment.PaymentScreenshot))
                    {
                        var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", advertisement.Payment.PaymentScreenshot.TrimStart('/'));
                        if (System.IO.File.Exists(screenshotPath))
                            System.IO.File.Delete(screenshotPath);
                    }

                    context.Payments.Remove(advertisement.Payment);
                }

                // Delete advertisement
                context.Advertisements.Remove(advertisement);
                context.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Advertisement deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Error deleting advertisement: {ex.Message}" 
                });
            }
        }

        // ===============================
        // HELPER METHODS
        // ===============================

        private string UploadFileSafely(IFormFile file, string uploadFolder, long maxSize = 5242880) // 5MB default
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Check file size
            if (file.Length > maxSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {maxSize / 1024 / 1024}MB");

            // Check file type (allow common image types)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("File type not allowed");

            try
            {
                // Create unique filename to prevent conflicts
                var fileName = Guid.NewGuid() + fileExtension;
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", uploadFolder);

                // Ensure directory exists
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fullPath = Path.Combine(uploadPath, fileName);

                // Upload file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Return relative path for database storage
                return Path.Combine("/" + uploadFolder, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}");
            }
        }

        private bool IsAuthorizedForAdvertisement(Driver driver, int userId)
        {
            if (driver.RegisterationStatus != "Approved")
                return false;

            if (driver.MembershipId == 1)
                return true; // Free membership always authorized

            return HasActiveSubscription(userId);
        }

        private bool CanCreateAdvertisement(int driverId, int membershipId)
        {
            var maxActiveAds = GetMaxActiveAds(membershipId);
            if (maxActiveAds == -1) // Unlimited
                return true;

            var currentActiveAds = context.Advertisements.Count(a => 
                a.AdvertiserType == "Driver" && 
                a.AdvertiserId == driverId && 
                a.IsActive && 
                a.StartDate <= DateTime.Now && 
                a.EndDate >= DateTime.Now);

            return currentActiveAds < maxActiveAds;
        }

        private int GetMaxActiveAds(int membershipId)
        {
            return membershipId switch
            {
                2 => 2,  // Max 2 active ad for drivers
                3 => -1, // Unlimited ads
                _ => 0   // No ads for other memberships
            };
        }

        private void SetupAdvertisementViewBag(int membershipId)
        {
            ViewBag.PaymentAmounts = context.PaymentAmounts
                .Where(p => p.EntityType == "Advertiser" && p.MembershipId == membershipId)
                .ToList();

            ViewBag.PaymentMethods = new SelectList(context.PaymentMethods.Where(pm => pm.IsActive == true).ToList(), "PaymentMethodId", "MethodName");
        }


        public IActionResult DriverProfile()
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            return View();
        }
        public IActionResult ViewProfileDriver()


        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var driver = context.Drivers
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);
            return View(driver);
        }

        public IActionResult EditProfileDriver()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var driver = context.Drivers.FirstOrDefault(c => c.UserId == userId);
            if (driver == null)
            {
                return NotFound();
            }
            var vehicleParts = driver.VehicleInfo?.Split('|');
            var cv = new DriverValidate
            {
                DriverId = driver.DriverId,
                DriverName = driver.DriverName,
                Address = driver.Address,
                Email = driver.Email,
                CityId = driver.CityId,
                Description = driver.Description,
                Telephone = driver.Telephone,
                Cnic = driver.Cnic,
                Experience = driver.Experience,


                VehicleMake = vehicleParts?[0].Trim(),
                VehicleModel = vehicleParts?[1].Trim(),
                VehicleYear = vehicleParts?[2].Trim(),
                VehiclePlate = vehicleParts?[3].Trim(),
                VehicleColor = vehicleParts?[4].Trim()

            };
            ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");

            return View(cv);
        }
        [HttpPost]
        public IActionResult EditProfileDriver(DriverValidate cv)
        {
            var existprofile = context.Drivers.Find(cv.DriverId);
            if (existprofile == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");

                return View(cv);
            }
            existprofile.DriverName = cv.DriverName;
            existprofile.Address = cv.Address;
            existprofile.Cnic = cv.Cnic;
            existprofile.CityId = cv.CityId;
            existprofile.Description = cv.Description;
            existprofile.Experience = cv.Experience;
            existprofile.VehicleInfo =
                $"{cv.VehicleMake} |" +
                $"{cv.VehicleModel} |" +
                $"{cv.VehicleYear} |" +
                $"{cv.VehiclePlate} |" +
                $"{cv.VehicleColor}";
            context.Drivers.Update(existprofile);
            context.SaveChanges();
            TempData["added"] = "Profile Updated Successfully!";
            return RedirectToAction("DriverProfile");
        }
        public IActionResult PaymentPageDriver()
        {
            var guard = EnforceDriverAccess();
            if (guard != null) return guard;

            var userid = HttpContext.Session.GetInt32("UserId");
            var userrole = HttpContext.Session.GetString("Role");
            ViewBag.Payments = context.Payments.Where(p => p.UserId == userid).Count();
            var totalpayment = context.Payments.Where(p => p.UserId == userid).Sum(p => (decimal?)p.PaymentAmount.Amount ?? 00);
            ViewBag.TotalPayment = totalpayment;
            ViewBag.PendingPayment = context.Payments.Where(p => p.UserId == userid && p.PaymentStatus == "Pending").Count();
            ViewBag.PaidPayment = context.Payments.Where(p => p.UserId == userid && p.PaymentStatus == "Paid").Count();
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var PaymentVM = context.Payments.Include(p => p.PaymentAmount).Include(p => p.PaymentMethod).Include(p => p.User).Where(p => p.UserId == userid && p.User.Role == userrole).ToList();
            return View(PaymentVM);
        }
        public IActionResult ViewPaymentDriver(int id)
        {
            var payment = context.Payments.Include(p => p.PaymentAmount).ThenInclude(x => x.Membership).Include(p => p.PaymentMethod).Include(p => p.User).FirstOrDefault(p => p.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }
    }
}
