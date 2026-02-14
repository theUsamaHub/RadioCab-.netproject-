using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RadioCab.Models;
using System.Xml.Linq;

namespace RadioCab.Controllers
{
    public class CompanyController : Controller
    {
        private readonly RadioCabContext context;

        private bool IsFreeCompany(int userId)
        {
            return context.Companies.Any(c =>
                c.UserId == userId &&
                c.MembershipId == 1 &&
                c.RegisterationStatus == "Approved");
        }


        public CompanyController(RadioCabContext context)
        {
            this.context = context;
        }

        private bool HasActiveSubscription(int userId)
        {
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId);
            if (company == null)
                return false;

            if (company.MembershipId == 1)
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



        [Authorize(Roles = "Company")]
        public IActionResult SubscriptionExpired()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            // FREE MEMBERSHIP → REDIRECT TO DASHBOARD
            if (company == null)
                return RedirectToAction("MembershipPlans");

            if (company.MembershipId == 1)
                return RedirectToAction("PendingApproval");



            var latestPayment = GetLatestPayment(userId.Value, company.MembershipId);
            ViewBag.ExpiryDate = latestPayment?.ExpiryDate;

            return View();
        }

        private string GetSubscriptionState(int userId)
        {
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId);
            if (company == null)
                return "None";

            // FREE MEMBERSHIP → NEVER EXPIRES, NO PAYMENT CHECK
            if (company.MembershipId == 1)
                return "Active";

            var latestPayment = context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p =>
                    p.UserId == userId &&
                    p.PaymentPurpose == "Membership" &&
                    p.PaymentAmount.MembershipId == company.MembershipId)
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



        private bool IsApproved()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            return company != null && company.RegisterationStatus == "Approved";
        }


        [Authorize(Roles = "Company")]
        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Login", "Account");

                var company = context.Companies
                    .FirstOrDefault(c => c.UserId == userId.Value);

                if (company == null)
                    return RedirectToAction("ProfileRegister", "Company");

                // 🔒 FREE MEMBERSHIP FIRST
                if (company.MembershipId == 1)
                {
                    if (company.RegisterationStatus != "Approved")
                        return RedirectToAction("PendingApproval");
                }
                else
                {
                    var state = GetSubscriptionState(userId.Value);

                    if (state == "Pending")
                        return RedirectToAction("PaymentPendingApproval");

                    if (state == "Expired")
                        return RedirectToAction("SubscriptionExpired");

                    if (state == "None")
                        return RedirectToAction("MembershipPlans");
                }
                var companyId = company.CompanyId;

                // Get statistics
                ViewBag.application = context.VacancyApplications
                    .Include(a => a.Vacancy)
                    .Where(a => a.Vacancy.CompanyId == companyId)
                    .Count();

                ViewBag.vacancy = context.CompanyVacancies
                    .Where(v => v.CompanyId == companyId)
                    .Count();

                ViewBag.FeedBack = context.CompanyFeedbacks
                    .Where(f => f.CompanyId == companyId)
                    .Count();

                ViewBag.Service = context.CompanyServices
                    .Where(s => s.CompanyId == companyId && s.isActive == true)
                    .Count();

                // Get recent payments
                var recentPayments = context.Payments
                    .Where(p => p.UserId == userId.Value)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(3)
                    .ToList();

                // Get recent feedbacks for the company
                var recentFeedbacks = context.CompanyFeedbacks
                    .Where(f => f.CompanyId == companyId)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(3)
                    .ToList();

                var model = new CompanyFeedbackandPayment
                {
                    Payment = recentPayments,
                    CompanyFeedback = recentFeedbacks
                };

                // Calculate average rating for display
                var allFeedbacks = context.CompanyFeedbacks
                    .Where(f => f.CompanyId == companyId)
                    .ToList();

                ViewBag.AverageRating = allFeedbacks.Any()
                    ? Math.Round(allFeedbacks.Average(f => f.Rating), 1)
                    : 0;

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Error", "Home");
            }
        }


        [Authorize(Roles = "Company")]
        public IActionResult PendingApproval()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus == "Approved")
                    return RedirectToAction("Index");

                return View();
            }

            // PAID ONLY
            var state = GetSubscriptionState(userId.Value);

            if (state == "Pending")
                return RedirectToAction("PaymentPendingApproval");

            if (state == "Expired")
                return RedirectToAction("SubscriptionExpired");

            if (state == "None")
                return RedirectToAction("MembershipPlans");

            if (company.RegisterationStatus == "Approved")
                return RedirectToAction("Index");

            return View();
        }

        [Authorize(Roles = "Company")]
        public IActionResult CheckStatus()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { status = "NoUser" });

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return Json(new { status = "NoCompany" });

            return Json(new { status = company.RegisterationStatus });
        }
        [Authorize(Roles = "Company")]
        public IActionResult MembershipPlans()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies
                .FirstOrDefault(c => c.UserId == userId.Value);

            var memberships = context.Memberships
                  .Where(m => m.IsActive == true)
                .Include(m => m.MembershipFeatures)
                    .ThenInclude(mf => mf.Feature)
                .ToList();

            var price = context.PaymentAmounts
                .Where(p => p.EntityType == role)
                .ToList();

            var latestPayment = context.Payments
      .Where(p =>
          p.UserId == userId.Value &&
          p.PaymentPurpose == "Membership")
      .OrderByDescending(p => p.PaymentDate)
      .FirstOrDefault();


            bool hasActiveSubscription =
                latestPayment != null &&
                latestPayment.PaymentStatus == "Paid" &&
                latestPayment.ExpiryDate > DateTime.Now;

            ViewBag.Price = price;
            ViewBag.Role = role;
            ViewBag.HasCompanyProfile = company != null;
            ViewBag.CurrentMembershipId = company?.MembershipId;
            ViewBag.HasActiveSubscription = hasActiveSubscription;
            ViewBag.RegisterationStatus = company?.RegisterationStatus;

            return View(memberships);
        }


        [HttpGet]
        public IActionResult ProfileRegister(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = context.Users.FirstOrDefault(u => u.UserID == userId);
            var model = new CompanyValidate
            {
                MembershipId = id,
                ContactPerson = user.FullName,
                Email = user.Email,
                Telephone = user.Phone
            };
            ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
            ViewBag.Designation = new SelectList(new List<string>
{
    "Manager",
    "Owner",
    "Director",
    "CEO",
    "CFO",
    "COO",
    "CTO",
    "President",
    "Vice President",
    "Team Lead",
    "Supervisor",
    "Administrator",
    "Consultant",
    "Engineer",
    "Analyst",
    "Coordinator",
    "Intern",
    "Associate",
    "Partner",
    "Founder"
});


            return View(model);
        }

        [HttpPost]
        public IActionResult ProfileRegister(CompanyValidate cv, IFormFile logo, IFormFile fbr_certificate, IFormFile business_license)
        {
            var faxExist = context.Companies.FirstOrDefault(c => c.FaxNumber == cv.FaxNumber);
            if (faxExist != null)
            {
                ModelState.AddModelError("", "Fax Number already exist");
                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
                ViewBag.Designation = new SelectList(new List<string>
{
    "Manager",
    "Owner",
    "Director",
    "CEO",
    "CFO",
    "COO",
    "CTO",
    "President",
    "Vice President",
    "Team Lead",
    "Supervisor",
    "Administrator",
    "Consultant",
    "Engineer",
    "Analyst",
    "Coordinator",
    "Intern",
    "Associate",
    "Partner",
    "Founder"
});

                return View(cv);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
                ViewBag.Designation = new SelectList(new List<string>
{
    "Manager",
    "Owner",
    "Director",
    "CEO",
    "CFO",
    "COO",
    "CTO",
    "President",
    "Vice President",
    "Team Lead",
    "Supervisor",
    "Administrator",
    "Consultant",
    "Engineer",
    "Analyst",
    "Coordinator",
    "Intern",
    "Associate",
    "Partner",
    "Founder"
});

                return View(cv);
            }

            var logoName = Path.GetFileName(logo.FileName);
            var logoPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Upload/companylogo");
            string logoValue = Path.Combine(logoPath, logoName);
            using (var stream = new FileStream(logoValue, FileMode.Create))
            {
                logo.CopyTo(stream);
            }
            var dblogo = Path.Combine("/Upload/companylogo", logoName);

            var certName = Path.GetFileName(fbr_certificate.FileName);
            var certPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Upload/FBRCertificates");
            // Fixed typo: Changed from "FBRCertifocates" to "FBRCertificates"
            string certValue = Path.Combine(certPath, certName);
            using (var stream = new FileStream(certValue, FileMode.Create))
            {
                fbr_certificate.CopyTo(stream);
            }
            var dbcertificate = Path.Combine("/Upload/FBRCertificates", certName);

            var blName = Path.GetFileName(business_license.FileName);
            var blPath = Path.Combine(HttpContext.Request.PathBase.Value, "wwwroot/Upload/business_license");
            string blValue = Path.Combine(blPath, blName);
            using (var stream = new FileStream(blValue, FileMode.Create))
            {
                business_license.CopyTo(stream);
            }
            var dblicense = Path.Combine("/Upload/business_license", blName);

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = context.Users.FirstOrDefault(u => u.UserID == userId);
            var company = new Company
            {
                UserId = userId.Value,
                MembershipId = cv.MembershipId,
                CompanyName = cv.CompanyName,
                ContactPerson = user.FullName,
                Designation = cv.Designation,
                Address = cv.Address,
                FaxNumber = cv.FaxNumber,
                CityId = cv.CityId,
                Email = user.Email,
                Telephone = user.Phone,
                Description = cv.Description,
                CompanyLogo = dblogo,
                FbrCertificate = dbcertificate,
                BusinessLicense = dblicense,
                RegisterationStatus = "Pending" // Set default status
            };

            context.Companies.Add(company);
            context.SaveChanges();

            if (cv.MembershipId == 1)
            {
                return RedirectToAction("Index");
            }


            return RedirectToAction("Payment", "Company", new { membershipid = company.MembershipId });
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
            if (membershipid == 1)
            {
                return RedirectToAction("Index");
            }

            ViewBag.TotalAmount = paymentAmounts.Sum(p => p.Amount); // optional, if multiple durations shown

            ViewBag.Duration = paymentAmounts.Select(pa => new
            {
                pa.PaymentAmountId,
                pa.PaymentType,
                pa.Amount
            }).ToList();

            ViewBag.MembershipId = membershipid;
            ViewBag.PaymentMethod = new SelectList(context.PaymentMethods.Where(pm => pm.IsActive == true).ToList(),"PaymentMethodId","MethodName");

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

            // ❌ Duplicate transaction protection
            if (context.Payments.Any(t => t.TransactionId == pv.TransactionId))
            {
                ModelState.AddModelError("", "Transaction ID already exists.");
                SetupPaymentViewBag(membershipid);
                return View(pv);
            }

            if (!ModelState.IsValid)
            {
                SetupPaymentViewBag(membershipid);
                return View(pv);
            }

            // ✅ Load selected payment plan
            var paymentAmount = context.PaymentAmounts
                .Include(pa => pa.Membership)
                .FirstOrDefault(pa => pa.PaymentAmountId == pv.PaymentAmountId);

            if (paymentAmount == null)
            {
                ModelState.AddModelError("", "Invalid payment selection.");
                SetupPaymentViewBag(membershipid);
                return View(pv);
            }

            // 📂 Upload screenshot
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

            // 🕒 Payment date
            var paymentDate = pv.PaymentDate != default ? pv.PaymentDate : DateTime.Now;

            // ✅ DYNAMIC EXPIRY LOGIC (single source of truth)
            DateTime expiryDate = paymentDate.AddMonths(paymentAmount.DurationInMonths);

            // 🔑 Detect context
            var advertisementId = HttpContext.Session.GetInt32("AdvertisementId");

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

                // ✅ CRITICAL: dynamic purpose
                PaymentPurpose = advertisementId != null ? "Advertisement" : "Membership"
            };

            context.Payments.Add(payment);
            context.SaveChanges(); // ⚠️ PaymentId generated here

            // ===============================
            // 🔗 ADVERTISEMENT PAYMENT LINK
            // ===============================
            if (advertisementId != null)
            {
                var ad = context.Advertisements
                    .FirstOrDefault(a => a.AdvertisementId == advertisementId.Value);

                if (ad != null)
                {
                    ad.PaymentId = payment.PaymentId;

                    // ✅ SINGLE SOURCE OF TRUTH
                    ad.EndDate = ad.StartDate.AddMonths(paymentAmount.DurationInMonths);
                }

                context.SaveChanges();
                HttpContext.Session.Remove("AdvertisementId");

                return RedirectToAction("AdvertisementList");
            }


            // ===============================
            // 🏢 MEMBERSHIP PAYMENT LOGIC
            // ===============================
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company != null)
                company.MembershipId = membershipid;

            context.SaveChanges();

            return !isRenewal
                ? RedirectToAction("PaymentPendingApproval")
                : RedirectToAction("MembershipPlans");
        }


        private void SetupPaymentViewBag(int membershipid)
        {
            var paymentAmounts = context.PaymentAmounts
                .Where(p => p.MembershipId == membershipid && p.EntityType == HttpContext.Session.GetString("Role"))
                .ToList();

            ViewBag.Duration = paymentAmounts.Select(pa => new
            {
                pa.PaymentAmountId,
                pa.PaymentType,
                pa.Amount
            }).ToList();

            ViewBag.MembershipId = membershipid;
            ViewBag.PaymentMethod = new SelectList(context.PaymentMethods.Where(pm => pm.IsActive == true).ToList(), "PaymentMethodId", "MethodName");
        }

        [Authorize(Roles = "Company")]
        public IActionResult PaymentPendingApproval()
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("MembershipPlans");
            if (company.MembershipId == 1)
                return RedirectToAction("PendingApproval");


            var latestPayment = GetLatestPayment(userId.Value, company.MembershipId);

            // Approved → dashboard
            if (latestPayment != null &&
                (latestPayment.PaymentStatus == "Approved" || latestPayment.PaymentStatus == "Paid"))
                return RedirectToAction("Index");

            // No pending payment → membership selection
            if (latestPayment == null || latestPayment.PaymentStatus != "Pending")
                return RedirectToAction("MembershipPlans");

            ViewBag.PaymentDate = latestPayment.PaymentDate;
            ViewBag.TransactionId = latestPayment.TransactionId;
            ViewBag.Amount = latestPayment.PaymentAmount?.Amount;

            return View();
        }



        public IActionResult Service()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }

            var companyId = company.CompanyId;

            return View(new CompanyServiceVM
            {
                CompanyServices = context.CompanyServices
                    .Include(x => x.Service)
                    .Where(x => x.CompanyId == companyId)
                    .ToList(),

                AvailableServices = context.Services
                    .Where(x => x.IsForCompany && x.IsActive)
                    .ToList()
              
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Service(CompanyServiceVM vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var companyId = context.Companies
                .Where(x => x.UserId == userId)
                .Select(x => x.CompanyId)
                .First();

            bool exists = context.CompanyServices.Any(x =>
                x.CompanyId == companyId &&
                x.ServiceId == vm.Input.SelectedServiceId); // use Input.SelectedServiceId

            if (exists)
            {
                ModelState.AddModelError("", "Service already added");
                return RedirectToAction("Service");
            }

            context.CompanyServices.Add(new CompanyService
            {
                CompanyId = companyId,
                ServiceId = vm.Input.SelectedServiceId
            });

            context.SaveChanges();
            TempData["added"] = "Service added successfully";
            return RedirectToAction("Service");
        }



        public IActionResult DelSer(int id)
        {
            var row = context.CompanyServices.Find(id);
            if (row != null)
            {
                context.CompanyServices.Remove(row);
                context.SaveChanges();
            }
            return RedirectToAction("Service");
        }


        [Authorize(Roles = "Company")]
        public IActionResult Feedback()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }

            if (company.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            var feedbacks = context.CompanyFeedbacks
                .Where(f => f.CompanyId == company.CompanyId)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var totalFeedbacks = feedbacks.Count;
            var averageRating = feedbacks.Any() ? Math.Round(feedbacks.Average(f => f.Rating), 1) : 0;

            // Calculate rating distribution
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
        [Authorize(Roles = "Company")]
        public IActionResult ContactRequests(string statusFilter = "", DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }

            if (company.RegisterationStatus != "Approved")
                return RedirectToAction("PendingApproval");

            var query = context.ContactRequests
                .Where(r => r.TargetType == "Company" && r.TargetId == company.CompanyId);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                query = query.Where(r => r.Status == statusFilter);

            if (dateFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.CreatedAt <= dateTo.Value.AddDays(1));

            return View(query.OrderByDescending(r => r.CreatedAt).ToList());
        }


        [HttpPost]
        [Authorize(Roles = "Company")]
        public IActionResult MarkAsViewed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);

            if (company == null) return Json(new { success = false });

            var request = context.ContactRequests
                .FirstOrDefault(r => r.ContactRequestId == id &&
                                   r.TargetType == "Company" &&
                                   r.TargetId == company.CompanyId);

            if (request != null && request.Status == "New")
            {
                request.Status = "Viewed";
                context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }



        [HttpPost]
        [Authorize(Roles = "Company")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);

            if (company == null) return Json(new { success = false });

            var request = context.ContactRequests
                .FirstOrDefault(r => r.ContactRequestId == id &&
                                   r.TargetType == "Company" &&
                                   r.TargetId == company.CompanyId);

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

        [Authorize(Roles = "Company")]
        public IActionResult AdvertisementList()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            // Authorization checks
            if (!IsAuthorizedForAdvertisement(company, userId.Value))
                return RedirectToAction("Index");

            var advertisements = context.Advertisements
                .Include(a => a.Payment)
                .Where(a => a.AdvertiserType == "Company" && a.AdvertiserId == company.CompanyId)
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
                    ActiveAdCount = context.Advertisements.Count(x => x.AdvertiserType == "Company" && 
                                                                    x.AdvertiserId == company.CompanyId && 
                                                                    x.IsActive && 
                                                                    x.StartDate <= DateTime.Now && 
                                                                    x.EndDate >= DateTime.Now)
                })
                .ToList();

            ViewBag.MaxActiveAds = GetMaxActiveAds(company.MembershipId);
            ViewBag.MembershipId = company.MembershipId;

            return View(advertisements);
        }

        [Authorize(Roles = "Company")]
        public IActionResult CreateAdvertisement()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            if (!IsAuthorizedForAdvertisement(company, userId.Value))
                return RedirectToAction("Index");

            // Check active ad limit
            if (!CanCreateAdvertisement(company.CompanyId, company.MembershipId))
            {
                TempData["Error"] = $"You have reached the maximum limit of {GetMaxActiveAds(company.MembershipId)} active advertisements.";
                return RedirectToAction("AdvertisementList");
            }

            var userMembershipId = company.MembershipId;
            ViewBag.PaymentAmounts = context.PaymentAmounts
                .Where(p => p.EntityType == "Advertiser" && p.MembershipId == userMembershipId)
                .ToList();

            ViewBag.PaymentMethods = context.PaymentMethods.Where(P=>P.IsActive == true).ToList();

            return View();
        }

        [HttpPost]       //USAMA CODE
        [Authorize(Roles = "Company")]
        public IActionResult CreateAdvertisement(AdvertisementCreateVM model, IFormFile adImage, IFormFile paymentScreenshot, int paymentAmountId, int paymentMethodId, string transactionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            if (!IsAuthorizedForAdvertisement(company, userId.Value))
                return RedirectToAction("Index");

            // Validate image upload
            if (adImage == null || adImage.Length == 0)
            {
                ModelState.AddModelError("", "Advertisement image is required.");
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }

            // Check active ad limit
            if (!CanCreateAdvertisement(company.CompanyId, company.MembershipId))
            {
                TempData["Error"] = $"You have reached the maximum limit of {GetMaxActiveAds(company.MembershipId)} active advertisements.";
                return RedirectToAction("AdvertisementList");
            }

            // Basic validation for ad creation
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError("", "Title and Description are required.");
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Start date cannot be in the past.");
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }

            // Validate payment details
            if (paymentAmountId == 0 || paymentMethodId == 0 || string.IsNullOrWhiteSpace(transactionId))
            {
                ModelState.AddModelError("", "Payment details are required.");
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }

            // Validate payment screenshot
            if (paymentScreenshot == null || paymentScreenshot.Length == 0)
            {
                ModelState.AddModelError("", "Payment screenshot is required.");
                SetupAdvertisementViewBag(company.MembershipId);
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
                                         pa.MembershipId == company.MembershipId);

                if (paymentAmount == null)
                {
                    ModelState.AddModelError("", "Invalid payment amount selected.");
                    SetupAdvertisementViewBag(company.MembershipId);
                    return View(model);
                }

                // Create advertisement
                var advertisement = new Advertisement
                {
                    AdvertiserType = "Company",
                    AdvertiserId = company.CompanyId,
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
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while creating the advertisement. Please try again.");
                SetupAdvertisementViewBag(company.MembershipId);
                return View(model);
            }
        }

        [Authorize(Roles = "Company")]
        public IActionResult EditAdvertisement(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            if (!IsAuthorizedForAdvertisement(company, userId.Value))
                return RedirectToAction("Index");

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == id && 
                                   a.AdvertiserType == "Company" && 
                                   a.AdvertiserId == company.CompanyId);

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
        [Authorize(Roles = "Company")]
        public IActionResult EditAdvertisement(AdvertisementEditVM model, IFormFile? newImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return RedirectToAction("ProfileRegister", "Company");

            if (!IsAuthorizedForAdvertisement(company, userId.Value))
                return RedirectToAction("Index");

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == model.AdvertisementId && 
                                   a.AdvertiserType == "Company" && 
                                   a.AdvertiserId == company.CompanyId);

            if (advertisement == null)
                return NotFound();

            // Check if edit is allowed (before StartDate)
            if (advertisement.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Cannot edit advertisement after start date. Only active/inactive toggle is allowed.";
                return RedirectToAction("AdvertisementList");
            }

            if (!ModelState.IsValid)
                return View(model);

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
        [Authorize(Roles = "Company")]
        public IActionResult ToggleAdvertisementStatus(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Unauthorized" });

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return Json(new { success = false, message = "Company not found" });

            var advertisement = context.Advertisements
                .FirstOrDefault(a => a.AdvertisementId == id &&
                                    a.AdvertiserType == "Company" &&
                                    a.AdvertiserId == company.CompanyId);

            if (advertisement == null)
                return Json(new { success = false, message = "Advertisement not found" });

            // Only allow toggle if StartDate has passed or payment is made
            if (advertisement.StartDate > DateTime.Now && advertisement.PaymentId == null)
                return Json(new { success = false, message = "Cannot toggle status before start date or without payment" });

            // Check max active ads if activating
            if (!advertisement.IsActive) // About to activate
            {
                int maxActive = GetMaxActiveAds(company.MembershipId); // same helper as for drivers
                if (maxActive != -1) // -1 means unlimited
                {
                    var currentActive = context.Advertisements.Count(a =>
                        a.AdvertiserType == "Company" &&
                        a.AdvertiserId == company.CompanyId &&
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
        [Authorize(Roles = "Company")]
        public IActionResult DeleteAdvertisement(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Unauthorized" });

            var company = context.Companies.FirstOrDefault(c => c.UserId == userId.Value);
            if (company == null)
                return Json(new { success = false, message = "Company not found" });

            var advertisement = context.Advertisements
                .Include(a => a.Payment)
                .FirstOrDefault(a => a.AdvertisementId == id && 
                                   a.AdvertiserType == "Company" && 
                                   a.AdvertiserId == company.CompanyId);

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
        private bool IsAuthorizedForAdvertisement(Company company, int userId)
        {
            if (company.RegisterationStatus != "Approved")
                return false;

            // 🔒 BLOCK MembershipId = 1 from accessing AdvertisementList
            if (company.MembershipId == 1)
                return false; // Free membership cannot access advertisements

            var state = GetSubscriptionState(userId);
            return state == "Active";
        }

        private bool CanCreateAdvertisement(int companyId, int membershipId)
        {
            var maxActiveAds = GetMaxActiveAds(membershipId);
            if (maxActiveAds == -1) // Unlimited
                return true;

            var currentActiveAds = context.Advertisements.Count(a => 
                a.AdvertiserType == "Company" && 
                a.AdvertiserId == companyId && 
                a.IsActive && 
                a.StartDate <= DateTime.Now && 
                a.EndDate >= DateTime.Now);

            return currentActiveAds < maxActiveAds;
        }

        private int GetMaxActiveAds(int membershipId)
        {
            return membershipId switch
            {
                2 => 2,  // Max 2 active ads
                3 => -1, // Unlimited ads
                _ => 0   // No ads for other memberships
            };
        }

        private void SetupAdvertisementViewBag(int membershipId)
        {
            ViewBag.PaymentAmounts = context.PaymentAmounts
                .Where(p => p.EntityType == "Advertiser" && p.MembershipId == membershipId)
                .ToList();

            ViewBag.PaymentMethods = context.PaymentMethods.Where(p=>p.IsActive == true).ToList();
        }

        // ===============================
        // PROFILE COMPLETE
        // ===============================

        public IActionResult Profile()
        {  
             var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            return View();
        }
        public IActionResult ViewProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            return View(company);
        }
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies.FirstOrDefault(c => c.UserId == userId);
            if (company == null)
            {
                return NotFound();
            }
           
            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var cv = new CompanyValidate
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                Address = company.Address,
                Email = company.Email,
                ContactPerson = company.ContactPerson,
                CityId = company.CityId,
                Description = company.Description,
                Designation = company.Designation,
                FaxNumber = company.FaxNumber,
                Telephone = company.Telephone,


            };
            ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
            ViewBag.Designation = new SelectList(new List<string>
            {
                "Manager",
                "Owner",
                "Director",
                "CEO"
            });
            return View(cv);
        }
        [HttpPost]
        public IActionResult EditProfile(CompanyValidate cv)
        {
            var existprofile = context.Companies.Find(cv.CompanyId);
            if (existprofile == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.City = new SelectList(context.Cities, "CityId", "CityName");
                ViewBag.Designation = new SelectList(new List<string>
            {
                "Manager",
                "Owner",
                "Director",
                "CEO"
            });
                return View(cv);
            }
            existprofile.CompanyName = cv.CompanyName;
            existprofile.ContactPerson = cv.ContactPerson;
            existprofile.Address = cv.Address;
            existprofile.FaxNumber = cv.FaxNumber;
            existprofile.CityId = cv.CityId;
            existprofile.Description = cv.Description;
            existprofile.Designation = cv.Designation;
            context.Companies.Update(existprofile);
            context.SaveChanges();
            TempData["added"] = "Profile Updated Successfully!";
            return RedirectToAction("Profile");
        }

        public IActionResult PaymentPage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
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
        public IActionResult ViewPayment(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var payment = context.Payments.Include(p => p.PaymentAmount).ThenInclude(x => x.Membership).Include(p => p.PaymentMethod).Include(p => p.User).ThenInclude(u => u.Companies).FirstOrDefault(p => p.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // ===============================
        // VACANCY COMPLETE
        // ===============================
        public IActionResult VacancyPage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            var maxVacancies = GetMaxActiveVacancies(company.MembershipId);
            var activeVacancies = context.CompanyVacancies.Count(v =>
                v.CompanyId == company.CompanyId &&
                v.IsActive);

            ViewBag.MaxVacancies = maxVacancies;
            ViewBag.ActiveVacancies = activeVacancies;

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var userid = HttpContext.Session.GetInt32("UserId");

            var companyid = context.Companies.Where(u => u.UserId == userid).Select(u => u.CompanyId).FirstOrDefault();

            ViewBag.totalVacancy = context.CompanyVacancies.Where(v => v.CompanyId == companyid).Count();
            ViewBag.RejectedVacancy = context.CompanyVacancies.Where(v => v.CompanyId == companyid && v.ApprovalStatus == "Rejected").Count();
            ViewBag.ApprovedVacancy = context.CompanyVacancies.Where(v => v.CompanyId == companyid && v.ApprovalStatus == "Approved").Count();
            ViewBag.PendingVacancy = context.CompanyVacancies.Where(v => v.CompanyId == companyid && v.ApprovalStatus == "Pending").Count();
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var vacancy = context.CompanyVacancies.Where(c=>c.CompanyId == companyid).OrderByDescending(a => a.VacancyId).ToList();
            return View(vacancy);
        }
        [HttpGet]
        public IActionResult CreateVacancy()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            if (!CanCreateVacancy(company.CompanyId, company.MembershipId))
            {
                TempData["Error"] =
                    $"You can create only {GetMaxActiveVacancies(company.MembershipId)} active vacancies.";
                return RedirectToAction("VacancyPage");
            }

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }


            var companyId = context.Companies.Where(u => u.UserId == userId).Select(u => u.CompanyId).FirstOrDefault();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var model = new CompanyVacancyValidate
            {
                CompanyId = companyId,
            };

            ViewBag.jobtype = new SelectList(new List<string>
            {
                "Full Time",
                "Part Time",
                "Contract Based"
            });

            return View(model);
        }
        [HttpPost]
        public IActionResult CreateVacancy(CompanyVacancyValidate cvv)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.jobtype = new SelectList(new List<string>
            {
                "Full Time",
                "Part Time",
                "Contract Based"
                });
                return View(cvv);
            }
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");


            }
            var company = context.Companies
              .Include(c => c.City).Include(c => c.Membership)
              .FirstOrDefault(c => c.UserId == userId);
            var companyId = context.Companies.Where(u => u.UserId == userId).Select(u => u.CompanyId).FirstOrDefault();

            if (!CanCreateVacancy(companyId, company.MembershipId))
            {
                TempData["Error"] =
                    $"You can create only {GetMaxActiveVacancies(company.MembershipId)} active vacancies.";
                return RedirectToAction("VacancyPage");
            }

            var vacancy = new CompanyVacancy
            {
                CompanyId = companyId,
                JobTitle = cvv.JobTitle,
                JobDescription = cvv.JobDescription,
                JobType = cvv.JobType,
                RequiredExperience = cvv.RequiredExperience,
                Location = cvv.Location,
                Salary = cvv.Salary,
                IsActive = true
            };
            context.CompanyVacancies.Add(vacancy);
            context.SaveChanges();
            TempData["add"] = "Vacancy Created Successfully!";
            return RedirectToAction("VacancyPage");

        }
        public IActionResult ViewVacancy(int id)
        {
            var viewvacancy = context.CompanyVacancies.Include(v => v.Company).FirstOrDefault(v => v.VacancyId == id);
            if (viewvacancy == null)
            {
                return NotFound();
            }

            return View(viewvacancy);
        }

        public IActionResult EditVacancy(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var userid = HttpContext.Session.GetInt32("UserId");
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var companyId = context.Companies.Where(u => u.UserId == userid).Select(u => u.CompanyId).FirstOrDefault();
            if (companyId == null)
            {
                return NotFound();
            }
            var vacancyexist = context.CompanyVacancies.Where(v => v.CompanyId == companyId).FirstOrDefault(v => v.VacancyId == id);
            if (vacancyexist == null)
            {
                return NotFound();
            }
            var model = new CompanyVacancyValidate
            {
                VacancyId = vacancyexist.VacancyId,
                JobTitle = vacancyexist.JobTitle,
                JobType = vacancyexist.JobType,
                JobDescription = vacancyexist.JobDescription,
                Salary = vacancyexist.Salary,
                RequiredExperience = vacancyexist.RequiredExperience,
                Location = vacancyexist.Location
            };
            ViewBag.jobtype = new SelectList(new List<string>
            {
                "Full Time",
                "Part Time",
                "Contract Based"
                });
            return View(model);

        }
        [HttpPost]
        public IActionResult EditVacancy(CompanyVacancyValidate cvv, int id)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var userid = HttpContext.Session.GetInt32("UserId");
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var companyId = context.Companies.Where(u => u.UserId == userid).Select(u => u.CompanyId).FirstOrDefault();
            if (companyId == null)
            {
                return NotFound();
            }
            var vacancyexist = context.CompanyVacancies.Where(v => v.CompanyId == companyId).FirstOrDefault(v => v.VacancyId == id);
            if (vacancyexist == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.jobtype = new SelectList(new List<string>
            {
                "Full Time",
                "Part Time",
                "Contract Based"
                });
                return View(cvv);
            }
            vacancyexist.CompanyId = companyId;
            vacancyexist.JobTitle = cvv.JobTitle;
            vacancyexist.JobType = cvv.JobType;
            vacancyexist.JobDescription = cvv.JobDescription;
            vacancyexist.RequiredExperience = cvv.RequiredExperience;
            vacancyexist.Location = cvv.Location;
            vacancyexist.Salary = cvv.Salary;
            vacancyexist.ApprovalStatus = "Pending";
            context.CompanyVacancies.Update(vacancyexist);
            context.SaveChanges();
            TempData["add"] = "Vacancy Updated Successfully!";
            return RedirectToAction("VacancyPage", "Company");


        }
        public IActionResult DeleteVacancy(int id)
        {
            var userid = HttpContext.Session.GetInt32("UserId");
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var companyId = context.Companies.Where(u => u.UserId == userid).Select(u => u.CompanyId).FirstOrDefault();
            if (companyId == null)
            {
                return NotFound();
            }
            var vacancydel = context.CompanyVacancies.Where(v => v.CompanyId == companyId).FirstOrDefault(v => v.VacancyId == id);
            if (vacancydel == null)
            {
                return NotFound();
            }
            context.CompanyVacancies.Remove(vacancydel);
            context.SaveChanges();
            TempData["delete"] = "Vacancy Deleted Successfully!";
            return RedirectToAction("VacancyPage", "Company");

        }

        // Add this method to your CompanyController
        public IActionResult DownloadApplicationFile(string filePath, bool download = true)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            // Check if file exists
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction("ViewVacancyApp", new { id = Request.Query["returnId"] });
            }

            // Get file info
            var fileInfo = new FileInfo(fullPath);
            var fileName = fileInfo.Name;
            var contentType = GetContentType(fileInfo.Extension);

            if (download)
            {
                return PhysicalFile(fullPath, contentType, fileName);
            }
            else
            {
                // For viewing in browser (optional)
                return PhysicalFile(fullPath, contentType);
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        // Optional: Add AJAX status update endpoint
        [HttpPost]
        public IActionResult UpdateApplicationStatus(int id, string status)
        {
            try
            {
                var vacancyApp = context.VacancyApplications.FirstOrDefault(a => a.ApplicationId == id);
                if (vacancyApp == null)
                {
                    return Json(new { success = false, message = "Application not found" });
                }

                vacancyApp.Status = status;
                context.SaveChanges();

                return Json(new { success = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        public IActionResult VacancyAppPage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
          
         
            var userid = HttpContext.Session.GetInt32("UserId");
            if (userid == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var companyId = context.Companies.Where(u => u.UserId == userid).Select(u => u.CompanyId).FirstOrDefault();
            if (companyId == null)
            {
                return NotFound();
            }

            var app = context.VacancyApplications.Include(a => a.Vacancy).Where(a => a.Vacancy.CompanyId == companyId).OrderByDescending(a => a.CreatedAt).ToList();
            return View(app);

        }

        public IActionResult ViewVacancyApp(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var company = context.Companies
                .Include(c => c.City).Include(c => c.Membership)
                .FirstOrDefault(c => c.UserId == userId);

            // 🔒 FREE MEMBERSHIP FIRST
            if (company.MembershipId == 1)
            {
                if (company.RegisterationStatus != "Approved")
                    return RedirectToAction("PendingApproval");
            }
            else
            {
                var state = GetSubscriptionState(userId.Value);

                if (state == "Pending")
                    return RedirectToAction("PaymentPendingApproval");

                if (state == "Expired")
                    return RedirectToAction("SubscriptionExpired");

                if (state == "None")
                    return RedirectToAction("MembershipPlans");
            }
            var vacancyapp = context.VacancyApplications.Include(a => a.Vacancy).FirstOrDefault(a => a.ApplicationId == id);
            if (vacancyapp == null)
            {
                return NotFound();
            }
            ViewBag.status = new SelectList(new List<string>
            {
                "Rejected",
                "Hired",
                "Applied",
                "Reviewed",
                
                });
            return View(vacancyapp);


        }
        [HttpPost]
        public IActionResult ViewVacancyApp(int id, string Status)
        {
            var vacancyApp = context.VacancyApplications.FirstOrDefault(a => a.ApplicationId == id);
            if (vacancyApp == null)
            {
                return NotFound();
            }
            vacancyApp.Status = Status;
            context.SaveChanges();
            TempData["update"] = "Status updated successfully";
            return RedirectToAction("VacancyAppPage", "Company");
        }


        private int GetMaxActiveVacancies(int membershipId)
        {
            return membershipId switch
            {
                2 => 2,   // Basic
                3 => -1,  // Unlimited
                _ => 0
            };
        }

        private bool CanCreateVacancy(int companyId, int membershipId)
        {
            var max = GetMaxActiveVacancies(membershipId);
            if (max == -1) return true;

            var activeCount = context.CompanyVacancies.Count(v =>
                v.CompanyId == companyId &&
                v.IsActive);

            return activeCount < max;
        }

    }
}