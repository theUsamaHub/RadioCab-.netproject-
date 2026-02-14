using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RadioCab.Models;
using System.Diagnostics;
using System.Linq;

namespace RadioCab.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IEmailService _emailService;
      
        private readonly RadioCabContext _context;

        public AdminController(RadioCabContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var dashboard = new Dashboard();

            // Get data from database
            dashboard.feedback_list = _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Take(4)
                .ToList();

            dashboard.CompanyVacancy_list = _context.CompanyVacancies
                .Where(c => c.ApprovalStatus == "Pending")
                .Take(4)
                .ToList();

            dashboard.driver_list = _context.Drivers
                .Where(x => x.RegisterationStatus == "Pending")
                .Take(2)
                .ToList();

            dashboard.company_list = _context.Companies
                .Where(x => x.RegisterationStatus == "Pending")
                .Take(2)
                .ToList();

            dashboard.Payment_list = _context.Payments
                .Include(c => c.User)
                .Include(c => c.PaymentMethod)
                .Include(x => x.PaymentAmount)
                .Where(x => x.PaymentStatus == "Pending")
                .OrderByDescending(p => p.PaymentDate)
                .Take(4)
                .ToList();

            // Get all lists for counts
            dashboard.User_list = _context.Users.ToList();
            dashboard.service_list = _context.Services.ToList();
            dashboard.PaymentMethod_list = _context.PaymentMethods.ToList();

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalCompanies = _context.Companies.Count();
            ViewBag.TotalDrivers = _context.Drivers.Count();
            ViewBag.PendingPayments = _context.Payments.Count(p => p.PaymentStatus == "Pending");
            ViewBag.PendingApprovals = _context.Companies.Count() + _context.Drivers.Count();
            ViewBag.ActiveServices = _context.Services.Count(s => s.IsActive);
            ViewBag.NewFeedbacks = _context.Feedbacks.Count();

            // Calculate total revenue from paid payments
            var paidPayments = _context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p => p.PaymentStatus == "Paid")
                .ToList();
            ViewBag.TotalRevenue = paidPayments.Sum(p => p.PaymentAmount?.Amount ?? 0);

            // Chart data - Last 6 months revenue (initialize empty if no data)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var monthlyRevenue = _context.Payments
                .Include(p => p.PaymentAmount)
                .Where(p => p.PaymentStatus == "Paid" && p.PaymentDate >= sixMonthsAgo)
                .ToList()
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Revenue = g.Sum(p => p.PaymentAmount?.Amount ?? 0)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Initialize chart data
            if (monthlyRevenue.Any())
            {
                ViewBag.RevenueChartLabels = monthlyRevenue
                    .Select(r => r.Month.ToString("MMM yy"))
                    .ToList();
                ViewBag.RevenueChartData = monthlyRevenue
                    .Select(r => (double)r.Revenue)
                    .ToList();
            }
            else
            {
                // Set default values if no data
                ViewBag.RevenueChartLabels = new List<string> { "No Data" };
                ViewBag.RevenueChartData = new List<double> { 0 };
            }

            // Payment method distribution
            var paymentMethodsData = _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.PaymentStatus == "Paid")
                .ToList()
                .GroupBy(p => p.PaymentMethod?.MethodName ?? "Unknown")
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Initialize payment method data
            if (paymentMethodsData.Any())
            {
                ViewBag.PaymentMethodLabels = paymentMethodsData
                    .Select(p => p.Method)
                    .ToList();
                ViewBag.PaymentMethodData = paymentMethodsData
                    .Select(p => p.Count)
                    .ToList();
            }
            else
            {
                // Set default values if no data
                ViewBag.PaymentMethodLabels = new List<string> { "No Data" };
                ViewBag.PaymentMethodData = new List<int> { 0 };
            }

            return View(dashboard);
        }


        // ==================== FEEDBACK MANAGEMENT ====================
        public IActionResult Feedback()
        {
            var fb = _context.Feedbacks.Include(c => c.City).Where(f => f.FeedbackType == "Compliment").ToList();
            var vm = new CDVM
            {
                feedback_list = fb,

                City_list = _context.Cities.OrderBy(c => c.CityName)
                .Select(c => new CityListVM
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    ZipCode = c.ZipCode
                }).ToList()
            };
            ViewData["Title"] = "COMPLIMENT FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Feedback methods";
            return View(vm);
        }

        public IActionResult Suggestion()
        {
            var fb = _context.Feedbacks.Include(c => c.City).Where(f => f.FeedbackType == "Suggestion").ToList();
            var vm = new CDVM
            {
                feedback_list = fb,

                City_list = _context.Cities.OrderBy(c => c.CityName)
               .Select(c => new CityListVM
               {
                   CityId = c.CityId,
                   CityName = c.CityName,
                   ZipCode = c.ZipCode
               }).ToList()
            };
            ViewData["Title"] = "SUGGESTION FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Feedback methods";
            return View(vm);
        }

        public IActionResult Complain()
        {
            var fb = _context.Feedbacks.Include(c => c.City).Where(f => f.FeedbackType == "Complaint").ToList();
            var vm = new CDVM
            {
                feedback_list = fb,

                City_list = _context.Cities.OrderBy(c => c.CityName)
               .Select(c => new CityListVM
               {
                   CityId = c.CityId,
                   CityName = c.CityName,
                   ZipCode = c.ZipCode
               }).ToList()

            };

            ViewData["Title"] = "COMPLAINT FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Driver Feedback methods";
            return View(vm);
        }

        [HttpGet]
        public IActionResult ComplainEdit(int id)
        {
            var feedback = _context.Feedbacks.FirstOrDefault(f => f.FeedbackId == id);

            if (feedback == null)
            {
                TempData["ErrorMessage"] = "Complaint not found!";
                return RedirectToAction("Complain"); // list page
            }
            ViewData["Title"] = "COMPLAINT FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Driver Feedback methods";
            return View(feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ComplainEdit(Feedback model)
        {

            var existingFeedback = _context.Feedbacks.FirstOrDefault(f => f.FeedbackId == model.FeedbackId);

            if (existingFeedback == null)
            {
                TempData["Errorcom"] = "Complaint not found!";
                return RedirectToAction("Complain");
            }

            // 🔹 Allowed status values (must match DB CHECK constraint)
            var validStatuses = new[] { "New", "Resolved", "Closed", "In Progress" };

            // Validate status
            if (!validStatuses.Contains(model.Status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Errorcom"] = "Invalid status value. Allowed: new, Resolved, Closed";
                return View(model);
            }

            // Normalize case to match DB
            existingFeedback.Status = validStatuses.First(s => s.Equals(model.Status, StringComparison.OrdinalIgnoreCase));

            // Update remarks & timestamp
            existingFeedback.AdminRemarks = model.AdminRemarks;


            _context.SaveChanges();

            // 🔹 Send Email if Resolved
            if (existingFeedback.Status == "Resolved" && !string.IsNullOrEmpty(existingFeedback.Email))
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        existingFeedback.Email,
                        "Complaint Resolved - Radio Cabs",
                        $"<h3>Your complaint has been resolved!</h3>" +
                        $"<p>Admin Remarks: {existingFeedback.AdminRemarks}</p>" +
                        $"<p>Thank you for your feedback.</p>"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email Error: " + ex.Message);
                    TempData["Errorcom"] = "Complaint updated, but email failed: " + ex.Message;
                }
            }

            TempData["Successcom"] = "Complaint updated successfully!";
            return RedirectToAction("Complain"); // list page
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFeedbackAjax(int id)
        {
            var company = _context.Feedbacks.FirstOrDefault(u => u.FeedbackId == id);
            if (company == null)
                return Json(new { success = false, message = "Not found" });

            _context.Feedbacks.Remove(company);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END FEEDBACK MANAGEMENT ====================


        // ==================== FEEDBACK COMPANY MANAGEMENT ====================

        [HttpGet]
        public IActionResult Companyfeedback()
        {
            var fb = _context.CompanyFeedbacks
                .Include(c => c.Company)
                .ToList();

            var vm = new CDVM
            {
                Companyfeedback_list = fb,
            };

            ViewData["Title"] = "COMPANY FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Company Feedback methods";
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteCompanyFeedbackAjax(int id)
        {
            try
            {
                var feedback = _context.CompanyFeedbacks.FirstOrDefault(cf => cf.FeedbackId == id);
                if (feedback == null)
                    return Json(new { success = false, message = "Feedback not found" });

                _context.CompanyFeedbacks.Remove(feedback);
                _context.SaveChanges();

                return Json(new { success = true, message = "Feedback deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting feedback: " + ex.Message });
            }
        }

        // ==================== END COMPNAY FEEDBACK MANAGEMENT ====================


        // ==================== DRIVER FEEDBACK MANAGEMENT ====================
        public IActionResult Driverfeedback()
        {           
            var vm = _context.DriverFeedbacks
                .Include(x => x.Driver) 
                .ToList();

            var model = new CDVM
            {
                Driverfeedback_list = vm
            };

            ViewData["Title"] = "DRIVER FEEDBACK MANAGEMENT";
            ViewData["Subtitle"] = "Manage all Driver Feedback methods";
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDriverfeedbackAjax(int id)
        {
            var f = _context.DriverFeedbacks.Find(id);
            if (f == null)
                return Json(new { success = false, message = "Not found" });

            _context.DriverFeedbacks.Remove(f);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END DRIVER MANAGEMENT ====================

        // ==================== City MANAGEMENT ====================
        public IActionResult City_list()
        {
           
            var cities = _context.Cities
                .OrderBy(c => c.CityName)
                .Select(c => new CityListVM
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    ZipCode = c.ZipCode
                })
                .ToList();

            var viewModel = new CityPageVM
            {
                City_list = cities
            };
            ViewData["Title"] = "Cities";
            ViewData["Subtitle"] = "Manage all cities";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult City_list(CityPageVM model)
        {
            if (!ModelState.IsValid)
            {
                model.City_list = _context.Cities
                    .Select(c => new CityListVM
                    {
                        CityId = c.CityId,
                        CityName = c.CityName,
                        ZipCode = c.ZipCode
                    })
                    .ToList();
                ViewData["Title"] = "Cities";
                ViewData["Subtitle"] = "Manage all cities";
                return View(model);
            }

            // Check if city name already exists
            bool cityExists = _context.Cities.Any(c => c.CityName == model.City_Validate.CityName);

            if (cityExists)
            {
                ModelState.AddModelError("City_Validate.CityName", "City already exists!");
                model.City_list = _context.Cities
                    .Select(c => new CityListVM
                    {
                        CityId = c.CityId,
                        CityName = c.CityName,
                        ZipCode = c.ZipCode
                    })
                    .ToList();
                return View(model);
            }

            // Check if zip code already exists (if provided)
            if (!string.IsNullOrEmpty(model.City_Validate.ZipCode))
            {
                bool zipExists = _context.Cities
                    .Any(c => c.ZipCode == model.City_Validate.ZipCode);

                if (zipExists)
                {
                    ModelState.AddModelError("City_Validate.ZipCode", "Zip code already exists!");
                    model.City_list = _context.Cities
                        .Select(c => new CityListVM
                        {
                            CityId = c.CityId,
                            CityName = c.CityName,
                            ZipCode = c.ZipCode
                        })
                        .ToList();
                    return View(model);
                }
            }

            // Create new city
            var newCity = new City
            {
                CityName = model.City_Validate.CityName,
                ZipCode = model.City_Validate.ZipCode
            };

            // Save to database
            _context.Cities.Add(newCity);
            _context.SaveChanges();

            // Redirect back to city list
            return RedirectToAction("City_list");
        }

        [HttpGet]
        public JsonResult GetCity(int id)
        {
            // Find city by ID
            var city = _context.Cities
                .Where(c => c.CityId == id)
                .Select(c => new
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    ZipCode = c.ZipCode
                })
                .FirstOrDefault();

            if (city == null)
            {
                return Json(new { success = false, message = "City not found" });
            }

            return Json(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCity([FromBody] CityFormVM model)
        {
           
            if (model == null)
            {
                return Json(new { success = false, message = "No data received" });
            }

            if (string.IsNullOrEmpty(model.CityName))
            {
                return Json(new { success = false, message = "City name is required" });
            }

            // Find the city to update
            var city = _context.Cities.Find(model.CityId);
            if (city == null)
            {
                return Json(new { success = false, message = "City not found" });
            }

            // Check for duplicate city name (excluding current city)
            bool cityNameExists = _context.Cities
                .Any(c => c.CityId != model.CityId && c.CityName == model.CityName);

            if (cityNameExists)
            {
                return Json(new { success = false, message = "City name already exists" });
            }

            // Check for duplicate zip code (if provided)
            if (!string.IsNullOrEmpty(model.ZipCode))
            {
                bool zipCodeExists = _context.Cities
                    .Any(c => c.CityId != model.CityId && c.ZipCode == model.ZipCode);

                if (zipCodeExists)
                {
                    return Json(new { success = false, message = "Zip code already exists" });
                }
            }

            // Update city information
            city.CityName = model.CityName;
            city.ZipCode = model.ZipCode;

            // Save changes
            _context.SaveChanges();

            return Json(new { success = true, message = "City updated successfully" });
        }

        public IActionResult DeleteCityAjax(int id)
        {
            var member = _context.Cities.FirstOrDefault(u => u.CityId == id);
            if (member == null)
                return Json(new { success = false, message = "City not found" });

            _context.Cities.Remove(member);
            _context.SaveChanges();

            return Json(new { success = true });

        }

        // ==================== END CITY MANAGEMENT ====================

        // ==================== PAYMENT METHOD MANAGEMENT ====================

        [HttpGet]
        public IActionResult PaymentMethodList()
        {
            var model = new PaymentMethodVM
            {
                paymentMethods_list = _context.PaymentMethods
                    .Select(p => new PaymentMethod
                    {
                        PaymentMethodId = p.PaymentMethodId,
                        MethodName = p.MethodName,
                        IsActive = p.IsActive
                    })
                    .ToList(),
                paymentMethodValidateForm = new PaymentMethodValidate()
            };
            ViewData["Title"] = "PAYMENT METHOD";
            ViewData["Subtitle"] = "Manage all payment methods";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentMethodList(PaymentMethodVM model)
        {
            // Check if model is valid
            if (!ModelState.IsValid)
            {
                model.paymentMethods_list = _context.PaymentMethods
                    .Select(p => new PaymentMethod
                    {
                        PaymentMethodId = p.PaymentMethodId,
                        MethodName = p.MethodName,
                        IsActive = p.IsActive
                    })
                    .ToList();
                ViewData["Title"] = "PAYMENT METHOD";
                ViewData["Subtitle"] = "Manage all payment methods";
                return View(model);
            }

            // Check if payment method name already exists
            bool methodExists = _context.PaymentMethods.Any(p => p.MethodName == model.paymentMethodValidateForm.MethodName);

            if (methodExists)
            {
                ModelState.AddModelError("paymentMethodValidateForm.MethodName", "Payment method already exists!");
                model.paymentMethods_list = _context.PaymentMethods
                    .Select(p => new PaymentMethod
                    {
                        PaymentMethodId = p.PaymentMethodId,
                        MethodName = p.MethodName,
                        IsActive = p.IsActive
                    })
                    .ToList();
                return View(model);
            }

            // Create new payment method - always active by default
            var newPaymentMethod = new PaymentMethod
            {
                MethodName = model.paymentMethodValidateForm.MethodName,
                IsActive = true // Default to active when creating new
            };

            // Save to database
            _context.PaymentMethods.Add(newPaymentMethod);
            _context.SaveChanges();

            // Redirect back to payment method list
            return RedirectToAction("PaymentMethodList");
        }

        [HttpGet]
        public JsonResult GetPaymentMethod(int id)
        {
            // Find payment method by ID
            var paymentMethod = _context.PaymentMethods
                .Where(p => p.PaymentMethodId == id)
                .Select(p => new
                {
                    paymentMethodId = p.PaymentMethodId,
                    methodName = p.MethodName,
                    isActive = p.IsActive
                })
                .FirstOrDefault();

            if (paymentMethod == null)
            {
                return Json(new { success = false, message = "Payment method not found" });
            }

            return Json(paymentMethod);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdatePaymentMethod([FromBody] PaymentMethodValidate model)
        {
            // Basic validation
            if (model == null)
            {
                return Json(new { success = false, message = "No data received" });
            }

            if (string.IsNullOrEmpty(model.MethodName))
            {
                return Json(new { success = false, message = "Method name is required" });
            }

            if (model.MethodName.Length < 3)
            {
                return Json(new { success = false, message = "Method name must be at least 3 characters" });
            }

            if (model.MethodName.Length > 50)
            {
                return Json(new { success = false, message = "Method name must be less than 50 characters" });
            }

            // Find the payment method to update
            var paymentMethod = _context.PaymentMethods.Find(model.PaymentMethodId);
            if (paymentMethod == null)
            {
                return Json(new { success = false, message = "Payment method not found" });
            }

            // Check for duplicate method name (excluding current payment method)
            bool methodNameExists = _context.PaymentMethods
                .Any(p => p.PaymentMethodId != model.PaymentMethodId && p.MethodName == model.MethodName);

            if (methodNameExists)
            {
                return Json(new { success = false, message = "Payment method name already exists" });
            }

            // Update payment method information
            paymentMethod.MethodName = model.MethodName;
            paymentMethod.IsActive = model.IsActive;

            // Save changes
            _context.SaveChanges();

            return Json(new { success = true, message = "Payment method updated successfully" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeletePaymentMethodAjax(int id)
        {
            var paymentMethod = _context.PaymentMethods.FirstOrDefault(p => p.PaymentMethodId == id);
            if (paymentMethod == null)
                return Json(new { success = false, message = "Payment method not found" });

            _context.PaymentMethods.Remove(paymentMethod);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END PAYMENT METHOD MANAGEMENT ====================

        // ==================== FAQS MANAGEMENT ====================

        [HttpGet]
        public IActionResult Faqs()
        {
            var model = new FaqVM
            {
                faqlist = _context.Faqs
                    .Select(f => new Faq
                    {
                        FaqId = f.FaqId,
                        Question = f.Question,
                        Answer = f.Answer
                    })
                    .ToList(),
                faqform = new FaqValidate()
            };
            ViewData["Title"] = "FAQS MANAGEMENT";
            ViewData["Subtitle"] = "Manage all FAQS";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Faqs(FaqVM model)
        {
            if (model.faqform != null)
            {
                model.faqform.Question = model.faqform.Question.Trim();
                model.faqform.Answer = model.faqform.Answer.Trim();
            }

            model.faqlist = _context.Faqs.ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.ShowForm = true;
                return View(model);
            }

            // Check for duplicate question (excluding current FAQ in edit mode)
            bool questionExists = _context.Faqs
                .Any(f => f.FaqId != model.faqform.FaqId
                       && f.Question.ToLower() == model.faqform.Question.ToLower());

            if (questionExists)
            {
                ModelState.AddModelError("faqform.Question", "This question already exists!");
                return View(model);
            }

            // EDIT MODE
            if (model.faqform.FaqId > 0)
            {
                var existingFaq = _context.Faqs.Find(model.faqform.FaqId);
                if (existingFaq != null)
                {
                    existingFaq.Question = model.faqform.Question;
                    existingFaq.Answer = model.faqform.Answer;

                    TempData["faqSuccessMessage"] = "FAQ updated successfully!";
                }
            }
            else // ADD MODE
            {
                var newFaq = new Faq
                {
                    Question = model.faqform.Question,
                    Answer = model.faqform.Answer
                };

                _context.Faqs.Add(newFaq);
                TempData["faqSuccessMessage"] = "FAQ added successfully!";
            }
            ViewData["Title"] = "FAQS MANAGEMENT";
            ViewData["Subtitle"] = "Manage all FAQS";
            _context.SaveChanges();
            return RedirectToAction("Faqs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteFaqAjax(int id)
        {
            try
            {
                var faq = _context.Faqs.FirstOrDefault(f => f.FaqId == id);
                if (faq == null)
                    return Json(new { success = false, message = "FAQ not found" });

                _context.Faqs.Remove(faq);
                _context.SaveChanges();

                return Json(new { success = true, message = "FAQ deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting FAQ: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetFaq(int id)
        {
            var faq = _context.Faqs
                .Where(f => f.FaqId == id)
                .Select(f => new
                {
                    FaqId = f.FaqId,
                    Question = f.Question,
                    Answer = f.Answer
                })
                .FirstOrDefault();

            if (faq == null)
            {
                return Json(new { success = false, message = "FAQ not found" });
            }

            return Json(faq);
        }

        // ==================== FAQS MANAGEMENT ====================


        // ==================== MEMEBERSHIP PLAN MANAGEMENT ====================
        public IActionResult Member()
        {
            var membership_list = new MembershipVM
            {
                Membership_list = _context.Memberships.ToList(),
                Membership_form = new MembershipValidate()
            };
            ViewData["Title"] = "MemberShip Plan Management";
            ViewData["Subtitle"] = "Manage membership plan add,edit, and delete";
            return View(membership_list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Member(MembershipVM model)
        {
            if (model.Membership_form.MembershipId > 0)
            {
                var existingMembership = _context.Memberships
                    .FirstOrDefault(m => m.MembershipId == model.Membership_form.MembershipId);

                if (existingMembership == null)
                {
                    ModelState.AddModelError("", "Membership not found.");
                }
                else
                {
                    // Check for duplicate name (excluding current membership)
                    var duplicateCheck = _context.Memberships
                        .FirstOrDefault(m =>
                            m.MembershipName.ToLower() == model.Membership_form.MembershipName.Trim().ToLower() &&
                            m.MembershipId != model.Membership_form.MembershipId);

                    if (duplicateCheck != null)
                    {
                        ModelState.AddModelError("Membership_form.MembershipName",
                            "Membership with this name already exists.");
                    }
                }
            }
            else // Adding new membership
            {
                var check = _context.Memberships
                    .FirstOrDefault(m => m.MembershipName.ToLower() == model.Membership_form.MembershipName.Trim().ToLower());

                if (check != null)
                {
                    ModelState.AddModelError("Membership_form.MembershipName",
                        "Membership with this name already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                var membership_list_invalid = new MembershipVM
                {
                    Membership_list = _context.Memberships.ToList(),
                    Membership_form = model.Membership_form
                };
                return View(membership_list_invalid);
            }

            if (model.Membership_form.MembershipId > 0)
            {
                // Update existing membership
                var existingMembership = _context.Memberships
                    .FirstOrDefault(m => m.MembershipId == model.Membership_form.MembershipId);

                if (existingMembership != null)
                {
                    existingMembership.MembershipName = model.Membership_form.MembershipName.Trim();
                    existingMembership.Description = model.Membership_form.Description.Trim();
                    existingMembership.IsActive = model.Membership_form.IsActive;

                    _context.Memberships.Update(existingMembership);
                    _context.SaveChanges();

                    TempData["MembershipEditMessage"] = "Membership updated successfully.";
                }
            }
            else
            {
                // Add new membership
                var newMembership = new Membership
                {
                    MembershipName = model.Membership_form.MembershipName.Trim(),
                    Description = model.Membership_form.Description.Trim(),
                    IsActive = true
                };

                _context.Memberships.Add(newMembership);
                _context.SaveChanges();

                TempData["MembershipAddMessage"] = "Membership added successfully.";
            }

            var membership_list = new MembershipVM
            {
                Membership_list = _context.Memberships.ToList(),
                Membership_form = new MembershipValidate()
            };
            ViewData["Title"] = "MemberShip Plan Management";
            ViewData["Subtitle"] = "Manage membership plan add,edit, and delete";
            return View(membership_list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMemberAjax(int id)
        {
            var member = _context.Memberships.FirstOrDefault(u => u.MembershipId == id);
            if (member == null)
                return Json(new { success = false, message = "Membership not found" });

            _context.Memberships.Remove(member);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMemberAjax([FromBody] MembershipValidate model)
        {
            var response = new { success = false, errors = new Dictionary<string, string[]>(), message = "" };

            // Validate model
            if (!ModelState.IsValid)
            {
                response = new
                {
                    success = false,
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ),
                    message = "Validation failed"
                };
                return Json(response);
            }

            // Check if membership exists
            var existingMembership = _context.Memberships
                .FirstOrDefault(m => m.MembershipId == model.MembershipId);

            if (existingMembership == null)
            {
                response = new { success = false, errors = new Dictionary<string, string[]>(), message = "Membership not found" };
                return Json(response);
            }

            // Check for duplicate name (excluding current membership)
            var duplicateCheck = _context.Memberships
                .FirstOrDefault(m =>
                    m.MembershipName.ToLower() == model.MembershipName.Trim().ToLower() &&
                    m.MembershipId != model.MembershipId);

            if (duplicateCheck != null)
            {
                ModelState.AddModelError("MembershipName", "Membership with this name already exists.");

                response = new
                {
                    success = false,
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    ),
                    message = "Duplicate membership name"
                };
                return Json(response);
            }

            try
            {
                // Update membership - FIX: Add IsActive update here
                existingMembership.MembershipName = model.MembershipName.Trim();
                existingMembership.Description = model.Description.Trim();
                existingMembership.IsActive = model.IsActive; // Add this line

                _context.Memberships.Update(existingMembership);
                _context.SaveChanges();

                response = new { success = true, errors = new Dictionary<string, string[]>(), message = "Membership updated successfully" };
                return Json(response);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                // _logger.LogError(ex, "Error updating membership");

                response = new
                {
                    success = false,
                    errors = new Dictionary<string, string[]>(),
                    message = "An error occurred while updating the membership: " + ex.Message
                };
                return Json(response);
            }
        }

        // ==================== END MEMEBERSHIP PLAN MANAGEMENT ====================


        // ==================== DRIVER MANAGEMENT ====================
        public IActionResult DriverManage(int? membershipId, int? cityId)
        {
            var query = _context.Drivers
                .Include(d => d.User).OrderByDescending(c => c.DriverId)
                .Include(d => d.City)
                .Include(d => d.Membership)
                .AsQueryable();

            // Apply filters
            if (membershipId.HasValue && membershipId.Value > 0)
            {
                query = query.Where(d => d.MembershipId == membershipId.Value);
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(d => d.CityId == cityId.Value);
            }

            var vm = new CDVM
            {
                Driver_list = query.Select(d => new DriverDetailsVM
                {
                    DriverId = d.DriverId,
                    DriverName = d.DriverName,
                    DriverPhoto = d.DriverPhoto,
                    DrivingLicenseNumber = d.DrivingLicenseNumber,
                    Address = d.Address,
                    Telephone = d.Telephone,
                    DrivingLicenseFile = d.DrivingLicenseFile,
                    VehicleInfo = d.VehicleInfo,

                    UserName = d.User.FullName,
                    UserEmail = d.User.Email,

                    CityName = d.City.CityName,
                    MembershipName = d.Membership.MembershipName,

                    RegisterationStatus = d.RegisterationStatus
                }).ToList(),

                membership_list = _context.Memberships.OrderBy(m => m.MembershipName).ToList(),
                cities = _context.Cities.OrderBy(c => c.CityName).ToList(),

                SelectedMembershipId = membershipId,
                SelectedCityId = cityId
            };
            ViewData["Title"] = "Driver Management";
            ViewData["Subtitle"] = "Manage Each drivers";
            return View(vm);
        }


        public IActionResult DeleteDriverAjax(int id)
        {
            var drive = _context.Drivers.FirstOrDefault(u => u.DriverId == id);
            if (drive == null)
                return Json(new { success = false, message = "Driver not found" });

            _context.Drivers.Remove(drive);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult DriverDetail(int id)
        {
            var vm = new CDVM();


            vm.Single_Driver = _context.Drivers.Include(d => d.User).Include(d => d.City).Include(d => d.Membership).Where(d => d.DriverId == id).Select(d => new DriverDetailsVM
            {
                DriverId = d.DriverId,
                DriverName = d.DriverName,
                Address = d.Address,
                Telephone = d.Telephone,
                DriverEmail = d.Email,
                Experience = d.Experience,
                Description = d.Description,
                DriverPhoto = d.DriverPhoto,
                DrivingLicenseNumber = d.DrivingLicenseNumber,
                DrivingLicenseFile = d.DrivingLicenseFile,
                VehicleInfo = d.VehicleInfo,


                UserId = d.User.UserID,
                UserName = d.User.FullName,
                UserEmail = d.User.Email,

                CityName = d.City.CityName,
                MembershipName = d.Membership.MembershipName,
                RegisterationStatus = d.RegisterationStatus
            }).FirstOrDefault();

            vm.DriverService_list = _context.DriverServices.Include(ds => ds.Service).Where(ds => ds.DriverId == vm.Single_Driver.DriverId && ds.IsActive == true).ToList();


            if (vm.Single_Driver == null)
                return NotFound();

            var userId = vm.Single_Driver.UserId;

            vm.Payment_list = _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentAmount)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(2)
                .ToList();

            return View(vm);
        }


        [HttpGet]
        public IActionResult EditDriver(int id)
        {
            var driver = _context.Drivers
                .Include(d => d.City)
                .Include(d => d.Membership)
                .FirstOrDefault(d => d.DriverId == id);

            if (driver == null)
            {
                TempData["ErrorMessage"] = "Driver not found.";
                return RedirectToAction("DriverManage");
            }

            var vm = new CDVM()
            {
                Single_Driver = new DriverDetailsVM
                {
                    DriverId = driver.DriverId,
                    DriverName = driver.DriverName,
                    Address = driver.Address,
                    DriverEmail = driver.Email,
                    Experience = driver.Experience,
                    Description = driver.Description,
                    DriverPhoto = driver.DriverPhoto,
                    DrivingLicenseNumber = driver.DrivingLicenseNumber,
                    VehicleInfo = driver.VehicleInfo,
                    CityName = driver.City?.CityName ?? "N/A",
                    MembershipName = driver.Membership?.MembershipName ?? "N/A",
                    RegisterationStatus = driver.RegisterationStatus
                }
            };
            ViewData["Title"] = "Driver Management";
            ViewData["Subtitle"] = "Edit driver status";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDriver(CDVM model)
        {
            if (model?.Single_Driver == null)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction("DriverManage");
            }

            try
            {
                var driver = _context.Drivers
                    .FirstOrDefault(d => d.DriverId == model.Single_Driver.DriverId);

                if (driver == null)
                {
                    TempData["ErrorMessage"] = "Driver not found.";
                    return RedirectToAction("DriverManage");
                }

                var oldStatus = driver.RegisterationStatus;
                driver.RegisterationStatus = model.Single_Driver.RegisterationStatus;

                _context.SaveChanges();

                if (!string.IsNullOrEmpty(driver.Email) && oldStatus != model.Single_Driver.RegisterationStatus)
                {
                    try
                    {
                        if (model.Single_Driver.RegisterationStatus == "Approved")
                        {
                            await _emailService.SendEmailAsync(
                                driver.Email,
                                "Driver Account Approved - Radio Cabs",
                                "<h3>Your Driver Account has been Approved!</h3>" +
                                "<p>Congratulations! Your driver account has been approved and you can now start accepting rides.</p>" +
                                "<p>Thank you for choosing Radio Cabs!</p>" +
                                "<br><p><strong>Radio Cabs Team</strong></p>"
                            );
                        }
                        else if (model.Single_Driver.RegisterationStatus == "Rejected")
                        {
                            await _emailService.SendEmailAsync(
                                driver.Email,
                                "Driver Account Rejected - Radio Cabs",
                                "<h3>Your Driver Account Application has been Rejected</h3>" +
                                "<p>We regret to inform you that your driver account application has been rejected.</p>" +
                                "<p>Please review your application details and contact support if you have any questions.</p>" +
                                "<p>You can submit a new application with corrected information.</p>" +
                                "<br><p><strong>Radio Cabs Team</strong></p>"
                            );
                        }
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the entire operation
                        Console.WriteLine($"Email Error for Driver {driver.DriverId}: {emailEx.Message}");
                        TempData["WarningMessage"] = $"Driver status updated, but email notification failed: {emailEx.Message}";
                    }
                }

                TempData["DSuccessMessage"] = $"Driver registration status updated to '{driver.RegisterationStatus}'.";
                return RedirectToAction("DriverManage");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating driver: {ex.Message}";

                // Reload driver data
                var reloadedDriver = _context.Drivers
                    .Include(d => d.City)
                    .Include(d => d.Membership)
                    .FirstOrDefault(d => d.DriverId == model.Single_Driver.DriverId);

                if (reloadedDriver != null)
                {
                    model.Single_Driver = new DriverDetailsVM
                    {
                        DriverId = reloadedDriver.DriverId,
                        DriverName = reloadedDriver.DriverName,
                        Address = reloadedDriver.Address,
                        DriverEmail = reloadedDriver.Email,
                        Experience = reloadedDriver.Experience,
                        Description = reloadedDriver.Description,
                        DriverPhoto = reloadedDriver.DriverPhoto,
                        DrivingLicenseNumber = reloadedDriver.DrivingLicenseNumber,
                        VehicleInfo = reloadedDriver.VehicleInfo,
                        CityName = reloadedDriver.City?.CityName ?? "N/A",
                        MembershipName = reloadedDriver.Membership?.MembershipName ?? "N/A",
                        RegisterationStatus = reloadedDriver.RegisterationStatus
                    };
                }
                ViewData["Title"] = "Driver Management";
                ViewData["Subtitle"] = "Edit driver status";
                return View(model);
            }
        }

        // ==================== END DRIVER  ====================


        // ==================== START PAYMENT MANAGEMENT ====================
        public IActionResult SinglePayment(int id)
        {

            var paymentlist = new CDVM()
            {
                Payment_list = _context.Payments
                .Include(p => p.PaymentMethod).Include(p => p.PaymentAmount).Include(p => p.User)
                .Where(p => p.UserId == id).OrderByDescending(p => p.PaymentDate).ToList()
            };
            ViewData["Title"] = "Payment Management";
            ViewData["Subtitle"] = "Manage each payment";
            return View(paymentlist);
        }

        public IActionResult PaymentManage()
        {
            var paymentlist = new CDVM()
            {
                Payment_list = _context.Payments
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentAmount).ThenInclude(pa => pa.Membership)
                .OrderByDescending(p => p.PaymentDate)
                .ToList()
            };
            ViewData["Title"] = "Payment Management";
            ViewData["Subtitle"] = "Manage each payment";
            return View(paymentlist);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePaymentAjax(int id)
        {
            var company = _context.Payments.FirstOrDefault(u => u.PaymentId == id);
            if (company == null)
                return Json(new { success = false, message = "Payment not found" });

            _context.Payments.Remove(company);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult EditPayment(int id)
        {
            var payment = _context.Payments
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentAmount)
                    .ThenInclude(pa => pa.Membership)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            var vm = new CDVM
            {
                Single_Payment = payment
            };
            ViewData["Title"] = "Payment Management";
            ViewData["Subtitle"] = "Manage payment status";
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditPayment(CDVM model)
        {
            if (model?.Single_Payment == null)
                return NotFound();

            var payment = _context.Payments
                .Include(p => p.User)            // User email ke liye
                .Include(p => p.PaymentAmount)   // DurationInMonths ke liye
                .FirstOrDefault(p => p.PaymentId == model.Single_Payment.PaymentId);

            if (payment == null)
                return NotFound();

            // Validate the status
            var validStatuses = new[] { "Pending", "Paid", "Rejected" };
            if (!validStatuses.Contains(model.Single_Payment.PaymentStatus))
            {
                TempData["ErrorMessage"] = "Invalid payment status.";
                return RedirectToAction(nameof(EditPayment), new { id = model.Single_Payment.PaymentId });
            }

            // Update status
            payment.PaymentStatus = model.Single_Payment.PaymentStatus;

            // Paid logic
            if (model.Single_Payment.PaymentStatus == "Paid")
            {
                payment.PaymentDate = DateTime.Now;
                payment.ExpiryDate = payment.PaymentDate.AddMonths(payment.PaymentAmount.DurationInMonths);

                try
                {
                    await _emailService.SendEmailAsync(
                        payment.User.Email,   // user email
                        "Payment Approved - Radio Cabs",
                        $"<h3>Payment Approved!</h3>" +
                        $"<p>Payment Date: {payment.PaymentDate:dd MMM yyyy}</p>" +
                        $"<p>Expiry Date: {payment.ExpiryDate:dd MMM yyyy}</p>" +
                        $"<p>Thank you for your payment!</p>"
                    );
                }
                catch (Exception ex)
                {
                    // Error log / show admin panel
                    Console.WriteLine("Email Error: " + ex.Message);
                    TempData["ErrorMessage"] = "Email failed: " + ex.Message;
                }
            }
            else if (model.Single_Payment.PaymentStatus == "Rejected")
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        payment.User.Email,
                        "Payment Failed - Radio Cabs",
                        "<p>Your payment has failed. Please try again or contact support.</p>"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email Error: " + ex.Message);
                    TempData["ErrorMessage"] = "Email failed: " + ex.Message;
                }
            }

            // Save changes
            _context.SaveChanges();
            ViewData["Title"] = "Payment Management";
            ViewData["Subtitle"] = "Manage payment status";
            TempData["SuccessMessage"] = "Payment status updated successfully!";
            return RedirectToAction("PaymentManage");
        }

        public IActionResult Invoice(int id)
        {
            var payment = _context.Payments
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentAmount)
                    .ThenInclude(pa => pa.Membership)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            var vm = new CDVM
            {
                Single_Payment = payment
            };
            ViewData["Title"] = "Payment Detail";
            ViewData["Subtitle"] = "Deatil of Transaction";
            return View(vm);
        }

        // ==================== END PAYMENT ====================


        // ==================== COMPANY MANAGEMENT ====================
        public IActionResult CompanyManage()
        {
            var company_detail = new CDVM
            {
                Company_list = _context.Companies
                    .Include(c => c.User).OrderByDescending(c => c.CompanyId)
                    .Include(c => c.City)
                    .Include(c => c.Membership)
                    .Select(c => new CompanyDetailsVM
                    {
                        CompanyId = c.CompanyId,
                        CompanyName = c.CompanyName,
                        ContactPerson = c.ContactPerson,
                        Designation = c.Designation,
                        Address = c.Address,
                        Telephone = c.Telephone,
                        FaxNumber = c.FaxNumber,

                        UserName = c.User.FullName,
                        UserEmail = c.User.Email,
                        UserPhone = c.User.Phone,

                        CityName = c.City.CityName,
                        MembershipName = c.Membership.MembershipName,

                        RegisterationStatus = c.RegisterationStatus
                    })
                    .ToList(),

                membership_list = _context.Memberships.ToList()
            };

            ViewData["Title"] = "Manage Companies";
            ViewData["Subtitle"] = "Here you manage companies";
            return View(company_detail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCompanyAjax(int id)
        {
            var company = _context.Companies.FirstOrDefault(u => u.CompanyId == id);
            if (company == null)
                return Json(new { success = false, message = "Company not found" });

            _context.Companies.Remove(company);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        public IActionResult CompanyDetail(int id)
        {
            var company = _context.Companies.Include(c => c.User).Include(c => c.City).Include(c => c.Membership).FirstOrDefault(c => c.CompanyId == id);

            if (company == null)
                return NotFound();

            var vm = new CDVM()
            {
                Single_Company = new CompanyDetailsVM
                {
                    CompanyId = company.CompanyId,
                    CompanyName = company.CompanyName,
                    Address = company.Address,
                    Telephone = company.Telephone,
                    CompanyEmail = company.Email,
                    Description = company.Description,
                    FBRCertificate = company.FbrCertificate,
                    BusinessLicense = company.BusinessLicense,
                    ContactPerson = company.ContactPerson,
                    Designation = company.Designation,
                    FaxNumber = company.FaxNumber,
                    CompanyLogo = company.CompanyLogo,

                    UserID = company.User.UserID,
                    UserName = company.User.FullName,
                    UserEmail = company.User.Email,

                    CityName = company.City.CityName,

                    MembershipName = company.Membership.MembershipName,
                    RegisterationStatus = company.RegisterationStatus

                }
            };
            ViewData["Title"] = "Company Deatil";
            ViewData["Subtitle"] = "Deatil of Company";
            return View(vm);
        }

        [HttpGet]
        public IActionResult CompanyEdit(int id)
        {
            var company = _context.Companies.Include(c => c.User).Include(c => c.City).Include(c => c.Membership).FirstOrDefault(c => c.CompanyId == id);

            if (company == null)
                return NotFound();

            var vm = new CDVM()
            {
                Single_Company = new CompanyDetailsVM
                {
                    CompanyId = company.CompanyId,
                    CompanyName = company.CompanyName,
                    Address = company.Address,
                    Telephone = company.Telephone,
                    CompanyEmail = company.Email,
                    Description = company.Description,
                    FBRCertificate = company.FbrCertificate,
                    BusinessLicense = company.BusinessLicense,
                    ContactPerson = company.ContactPerson,
                    Designation = company.Designation,
                    FaxNumber = company.FaxNumber,
                    CompanyLogo = company.CompanyLogo,

                    UserID = company.User.UserID,
                    UserName = company.User.FullName,
                    UserEmail = company.User.Email,

                    CityName = company.City.CityName,

                    MembershipName = company.Membership.MembershipName,
                    RegisterationStatus = company.RegisterationStatus

                }
            };
            ViewData["Title"] = "Edit Company Status";
            ViewData["Subtitle"] = "Here you manage edit status company";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyEdit(CDVM model)
        {
            var company = _context.Companies
                .FirstOrDefault(c => c.CompanyId == model.Single_Company.CompanyId);

            if (company == null)
                return NotFound();

            company.RegisterationStatus = model.Single_Company.RegisterationStatus;
            try { 
              var email = company.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    if (model.Single_Company.RegisterationStatus == "Approved")
                    {
                        await _emailService.SendEmailAsync(
                            email,
                            "Ad Approved - Radio Cabs",
                            "<h3>Your Company has been approved!</h3><p>Congratulations! Your Account has been approved.</p><p>Thank you.</p>"
                        );
                    }
                    else if (model.Single_Company.RegisterationStatus == "Rejected")
                    {
                        await _emailService.SendEmailAsync(
                            email,
                            "Ad Rejected - Radio Cabs",
                            "<h3>Your Company Account has been rejected</h3><p>Please review and submit again.</p>"
                        );
                    }
                    else
                    {
                        TempData["ErrorC"] = "Ad updated, but user email was not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
                TempData["ErrorC"] = "Ad updated, but email failed: " + ex.Message;
            }


            _context.SaveChanges();
            ViewData["Title"] = "Edit Company Status";
            ViewData["Subtitle"] = "Here you manage edit status company";
            TempData["CompanyMessage"] = "Company status updated successfully!";
            return RedirectToAction("CompanyManage");
        }

        // ==================== END COMPANY  ====================


        // ==================== User Start ====================
        public IActionResult UsersManage()
        {
            var user_list = new CDVM
            {
                User_list = _context.Users.Where(x=>x.Role != "Admin").ToList()
            };
            ViewData["Title"] = "User Management";
            ViewData["Subtitle"] = "Here you can manage user";
            return View(user_list);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUserAjax(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserID == id);

            if (user == null)
                return Json(new { success = false, message = "User not found" });

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        public IActionResult UserDetail(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
                return NotFound();

            var vm = new CDVM
            {
                Single_User = user
            };
            ViewData["Title"] = "User Management";
            ViewData["Subtitle"] = "Here is a detail of user";
            return View(vm);
        }

        // ==================== User END====================       

        // ==================== PLATFORM SERVICE MANAGEMENT ====================
        public IActionResult Platform()
        {
            var platform_service = new PlatformServiceVM
            {
                PlatformService_list = _context.PlatformServices.ToList(),
                Platform_form = new PlatformServiceValidate()
            };
            ViewData["Title"] = "Platform Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";
            return View(platform_service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Platform(PlatformServiceVM model)
        {
            // Check for duplicate service name (case-insensitive and trimmed)
            var existingService = _context.PlatformServices
                .FirstOrDefault(ps => ps.ServiceName.ToLower() == model.Platform_form.ServiceName.Trim().ToLower());

            if (existingService != null)
            {
                ModelState.AddModelError("Platform_form.ServiceName", "Service with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpenAddModal = true;

                // Reload the list for the view
                var platform_service_invalid = new PlatformServiceVM
                {
                    PlatformService_list = _context.PlatformServices.ToList(),
                    Platform_form = model.Platform_form
                };
                return View(platform_service_invalid);
            }

            var newService = new PlatformService
            {
                ServiceName = model.Platform_form.ServiceName.Trim(),
                ServiceDescription = model.Platform_form.ServiceDescription.Trim(),
                isActive = model.Platform_form.IsActive,

            };

            _context.PlatformServices.Add(newService);
            _context.SaveChanges();

            TempData["PlatformServiceAddMessage"] = "Platform service added successfully.";
            ViewData["Title"] = "Plateform Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";
            // Redirect to clear form state
            return RedirectToAction(nameof(Platform));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePlatformAjax(int id)
        {
            var service = _context.PlatformServices.FirstOrDefault(x => x.ServiceId == id);

            if (service == null)
                return Json(new { success = false, message = "Service not found" });

            _context.PlatformServices.Remove(service);
            _context.SaveChanges();

            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPlatform(PlatformServiceVM model)
        {
            // Get current service
            var currentService = _context.PlatformServices.Find(model.Platform_form.ServiceId);

            if (currentService == null)
            {
                return NotFound();
            }

            // Check if name is being changed
            bool nameChanged = !string.Equals(currentService.ServiceName.Trim(),
                model.Platform_form.ServiceName.Trim(),
                StringComparison.OrdinalIgnoreCase
            );

            // Only check for duplicates if name changed
            if (nameChanged)
            {
                var existingService = _context.PlatformServices
                    .FirstOrDefault(ps =>
                        ps.ServiceId != model.Platform_form.ServiceId && ps.ServiceName.ToLower() == model.Platform_form.ServiceName.Trim().ToLower());

                if (existingService != null)
                {
                    ModelState.AddModelError("Platform_form.ServiceName", "Service with this name already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpenEditModal = true;

                // Return complete view model
                var viewModel = new PlatformServiceVM
                {
                    PlatformService_list = _context.PlatformServices.ToList(),
                    Platform_form = model.Platform_form
                };

                return View("Platform", viewModel);
            }

            // Update the service
            currentService.ServiceName = model.Platform_form.ServiceName.Trim();
            currentService.ServiceDescription = model.Platform_form.ServiceDescription.Trim();
            currentService.isActive = model.Platform_form.IsActive;

            _context.SaveChanges();

            TempData["PlatformServiceEditMessage"] = "Platform service updated successfully.";
            ViewData["Title"] = "PLatform Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";

            return RedirectToAction(nameof(Platform));
        }


        public IActionResult GetServiceForEdit(int id)
        {
            var service = _context.PlatformServices.Find(id);

            if (service == null)
                return NotFound();

            var model = new PlatformServiceVM
            {
                Platform_form = new PlatformServiceValidate
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    ServiceDescription = service.ServiceDescription,
                    IsActive = (bool)service.isActive
                }
            };

            return PartialView("_EditServicePartial", model);
        }

        // ==================== PLATFORM SERVICE MANAGEMENT ====================


        // ==================== SERVICE MANAGEMENT ====================
        public IActionResult Service()
        {
            var serviceVM = new ServiceVM
            {
                service_list = _context.Services.OrderByDescending(s => s.ServiceId).ToList(),
                Service_Form = new ServiceValidate()
            };
            ViewData["Title"] = "Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";
            return View(serviceVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Service(ServiceVM model)
        {
            if (!model.Service_Form.IsForDriver && !model.Service_Form.IsForCompany)
            {
                ModelState.AddModelError("", "Service must be for Driver or Company (select at least one).");
            }

            var existingService = _context.Services
                .FirstOrDefault(s => s.ServiceName.ToLower() == model.Service_Form.ServiceName.Trim().ToLower());

            if (existingService != null)
            {
                ModelState.AddModelError("Service_Form.ServiceName", "Service with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpenAddModal = true;

                var serviceVM = new ServiceVM
                {
                    service_list = _context.Services.ToList(),
                    Service_Form = model.Service_Form
                };
                return View(serviceVM);
            }

            var newService = new Service
            {
                ServiceName = model.Service_Form.ServiceName.Trim(),
                ServiceDescription = model.Service_Form.ServiceDescription?.Trim(),
                IsForDriver = model.Service_Form.IsForDriver,
                IsForCompany = model.Service_Form.IsForCompany,
                IsActive = model.Service_Form.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Services.Add(newService);
            _context.SaveChanges();

            TempData["ServiceSuccessMessage"] = "Service added successfully.";
            ViewData["Title"] = "Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";
            return RedirectToAction(nameof(Service));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(ServiceVM model)
        {
            if (!model.Service_Form.IsForDriver && !model.Service_Form.IsForCompany)
            {
                ModelState.AddModelError("", "Service must be for Driver or Company (select at least one).");
            }

            var currentService = _context.Services.Find(model.Service_Form.ServiceId);

            if (currentService == null)
            {
                return NotFound();
            }

            // Check if name is being changed
            bool nameChanged = !string.Equals(
                currentService.ServiceName.Trim(),
                model.Service_Form.ServiceName.Trim(),
                StringComparison.OrdinalIgnoreCase
            );

            // Only check for duplicates if name changed
            if (nameChanged)
            {
                var existingService = _context.Services
                    .FirstOrDefault(s =>
                        s.ServiceId != model.Service_Form.ServiceId &&
                        s.ServiceName.ToLower() == model.Service_Form.ServiceName.Trim().ToLower());

                if (existingService != null)
                {
                    ModelState.AddModelError("Service_Form.ServiceName", "Service with this name already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OpenEditModal = true;

                var serviceVM = new ServiceVM
                {
                    service_list = _context.Services.ToList(),
                    Service_Form = model.Service_Form
                };
                return View("Service", serviceVM);
            }

            // Update the service
            currentService.ServiceName = model.Service_Form.ServiceName.Trim();
            currentService.ServiceDescription = model.Service_Form.ServiceDescription?.Trim();
            currentService.IsForDriver = model.Service_Form.IsForDriver;
            currentService.IsForCompany = model.Service_Form.IsForCompany;
            currentService.IsActive = model.Service_Form.IsActive;

            _context.SaveChanges();

            TempData["ServiceSuccessMessage"] = "Service updated successfully.";
            ViewData["Title"] = "Service Management";
            ViewData["Subtitle"] = "Here you can add, edit and delete services";
            return RedirectToAction(nameof(Service));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteServiceAjax(int id)
        {
            var service = _context.Services.FirstOrDefault(x => x.ServiceId == id);

            if (service == null)
                return Json(new { success = false, message = "Service not found" });

            _context.Services.Remove(service);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END SERVICE MANAGEMENT ====================


        // ==================== PAYMENT AMOUNT MANAGEMENT ====================
        public IActionResult PaymentAmountList()
        {
            var vm = new PaymentAmountVM
            {
                paymentAmount_list = _context.PaymentAmounts
                    .Include(p => p.Membership)
                    .OrderBy(p => p.PaymentAmountId)
                    .ToList(),

                MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList()
            };
            ViewData["Title"] = "Payment Amount Management";
            ViewData["Subtitle"] = "Here you can manage payments";
            return View(vm);
        }

        [HttpGet]
        public IActionResult PaymentAmountAdd()
        {
            var vm = new PaymentAmountVM
            {

                MembershipList = _context.Memberships
               .OrderBy(m => m.MembershipName)
               .ToList()
            };
            ViewData["Title"] = "Payment Amount Management";
            ViewData["Subtitle"] = "Here you can add payments";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentAmountAdd(PaymentAmountVM model)
        {
            // Server-side validation
            if (!ModelState.IsValid)
            {
                ViewBag.Memberships = _context.Memberships.OrderBy(m => m.MembershipName).ToList();
                return View(model);
            }


            bool isDuplicate = _context.PaymentAmounts.Any(p =>
                p.MembershipId == model.paymentvalidate_form.MembershipId &&
                p.EntityType.ToLower() == model.paymentvalidate_form.EntityType.Trim().ToLower() &&
                p.PaymentType.ToLower() == model.paymentvalidate_form.PaymentType.Trim().ToLower()
            );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "This combination of Membership, Entity Type, and Payment Type already exists.");
                model.MembershipList = _context.Memberships.OrderBy(m => m.MembershipName).ToList();

                return View(model);
            }

            var paymentAmount = new PaymentAmount
            {
                MembershipId = model.paymentvalidate_form.MembershipId,
                EntityType = model.paymentvalidate_form.EntityType.Trim().ToLower(),
                PaymentType = model.paymentvalidate_form.PaymentType.Trim().ToLower(),
                DurationInMonths = model.paymentvalidate_form.DurationInMonths,
                Amount = model.paymentvalidate_form.Amount,
                IsActive = true
            };

            _context.PaymentAmounts.Add(paymentAmount);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment amount added successfully!";
            ViewData["Title"] = "Payment Amount Management";
            ViewData["Subtitle"] = "Here you can add payments";
            return RedirectToAction("PaymentAmountList");
        }

        // GET: Edit Payment Amount
        public IActionResult PaymentAmountEdit(int id)
        {
            var paymentAmount = _context.PaymentAmounts
                .Include(p => p.Membership)
                .FirstOrDefault(p => p.PaymentAmountId == id);

            if (paymentAmount == null)
            {
                TempData["ErrorMessage"] = "Payment amount not found.";
                return RedirectToAction("PaymentAmountList");
            }

            var vm = new PaymentAmountVM
            {
                paymentvalidate_form = new PaymentAmountValidate
                {
                    PaymentAmountId = paymentAmount.PaymentAmountId,
                    MembershipId = paymentAmount.MembershipId,
                    EntityType = paymentAmount.EntityType,
                    PaymentType = paymentAmount.PaymentType,
                    DurationInMonths = paymentAmount.DurationInMonths,
                    Amount = paymentAmount.Amount,
                    IsActive = paymentAmount.IsActive
                },
                MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList()
            };
            ViewData["Title"] = "Edit Payment Amount";
            ViewData["Subtitle"] = "Here you can Edit payments";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentAmountEdit(PaymentAmountVM model)
        {
            if (!ModelState.IsValid)
            {
                model.MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList();
                return View(model);
            }

            var existingPaymentAmount = _context.PaymentAmounts
                .FirstOrDefault(p => p.PaymentAmountId == model.paymentvalidate_form.PaymentAmountId);

            if (existingPaymentAmount == null)
            {
                TempData["ErrorMessage"] = "Payment amount not found.";
                return RedirectToAction("PaymentAmountList");
            }

            // Check for duplicate (excluding current record)
            bool isDuplicate = _context.PaymentAmounts.Any(p =>
                p.PaymentAmountId != model.paymentvalidate_form.PaymentAmountId &&
                p.MembershipId == model.paymentvalidate_form.MembershipId &&
                p.EntityType.ToLower() == model.paymentvalidate_form.EntityType.Trim().ToLower() &&
                p.PaymentType.ToLower() == model.paymentvalidate_form.PaymentType.Trim().ToLower()
            );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "This combination of Membership, Entity Type, and Payment Type already exists.");
                model.MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList();
                return View(model);
            }

            // Validate amount
            if (model.paymentvalidate_form.Amount < 0)
            {
                ModelState.AddModelError("paymentvalidate_form.Amount", "Amount must be a positive number.");
                model.MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList();
                return View(model);
            }

            // Validate duration
            if (model.paymentvalidate_form.DurationInMonths < 1 || model.paymentvalidate_form.DurationInMonths > 120)
            {
                ModelState.AddModelError("paymentvalidate_form.DurationInMonths", "Duration must be between 1 and 120 months.");
                model.MembershipList = _context.Memberships
                    .OrderBy(m => m.MembershipName)
                    .ToList();
                return View(model);
            }

            // Update the existing record
            existingPaymentAmount.MembershipId = model.paymentvalidate_form.MembershipId;
            existingPaymentAmount.EntityType = model.paymentvalidate_form.EntityType.Trim().ToLower();
            existingPaymentAmount.PaymentType = model.paymentvalidate_form.PaymentType.Trim().ToLower();
            existingPaymentAmount.DurationInMonths = model.paymentvalidate_form.DurationInMonths;
            existingPaymentAmount.Amount = model.paymentvalidate_form.Amount;
            existingPaymentAmount.IsActive = model.paymentvalidate_form.IsActive;

            _context.PaymentAmounts.Update(existingPaymentAmount);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Payment amount updated successfully!";
            ViewData["Title"] = "Edit Payment Amount";
            ViewData["Subtitle"] = "Here you can Edit payments";
            return RedirectToAction("PaymentAmountList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePyamountAjax(int id)
        {
            var paymentAmount = _context.PaymentAmounts
                .Include(p => p.Membership)
                .FirstOrDefault(x => x.PaymentAmountId == id);

            if (paymentAmount == null)
                return Json(new { success = false, message = "Payment amount not found" });

            _context.PaymentAmounts.Remove(paymentAmount);
            _context.SaveChanges();

            return Json(new { success = true, message = "Payment amount deleted successfully" });
        }

        // ==================== END PAYMENT LIST MANAGEMENT ====================

        // ==================== FEATURE MANAGEMENT ====================
        public IActionResult Feature()
        {
            var feature_list = new FeatureVM
            {
                Feature_list = _context.Features.ToList(),
                FeatureForm = new FeatureValidate()
            };
            ViewData["Title"] = "Features";
            ViewData["Subtitle"] = "Here you can Add , Edit and Delete Features";
            return View(feature_list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Feature(FeatureVM model)
        {
            var key = model.FeatureForm.FeatureKey.Trim().ToLower();

            var check = _context.Features
                .FirstOrDefault(f => f.FeatureKey.ToLower() == key
                                  && f.FeatureId != model.FeatureForm.FeatureId);

            if (check != null)
            {
                ModelState.AddModelError("FeatureForm.FeatureKey", "Feature with this key already exists.");
            }

    
            model.ShowForm = true;

            if (!ModelState.IsValid)
            {
                model.Feature_list = _context.Features.ToList();
                return View(model);
            }

            // ⭐ EDIT MODE
            if (model.FeatureForm.FeatureId > 0)
            {
                var existing = _context.Features
                    .FirstOrDefault(f => f.FeatureId == model.FeatureForm.FeatureId);

                if (existing != null)
                {
                    existing.FeatureKey = model.FeatureForm.FeatureKey.Trim();
                    existing.FeatureName = model.FeatureForm.FeatureName.Trim();

                    TempData["FeatureAddMessage"] = "Feature updated successfully.";
                }
            }
            else // ⭐ ADD MODE
            {
                var newFeature = new Feature
                {
                    FeatureKey = model.FeatureForm.FeatureKey.Trim(),
                    FeatureName = model.FeatureForm.FeatureName.Trim()
                };

                _context.Features.Add(newFeature);
                TempData["FeatureAddMessage"] = "Feature added successfully.";
            }

            ViewData["Title"] = "Features";
            ViewData["Subtitle"] = "Here you can Add , Edit and Delete Features";

            _context.SaveChanges();

            return RedirectToAction(nameof(Feature));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFeatureAjax(int id)
        {
            var f = _context.Features.Find(id);
            if (f == null)
                return Json(new { success = false, message = "Not found" });

            _context.Features.Remove(f);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== FEATURE MANAGEMENT ====================


        // ==================== FEATURE VALUE MANAGEMENT ====================
        public IActionResult FeatureValue()
        {
            ViewBag.Memberships = _context.Memberships.ToList();
            ViewBag.Features = _context.Features.ToList();

            var vm = new MembershipFeatureVM
            {
                MembershipFeature_list = _context.MembershipFeatures
                    .Include(x => x.Membership)
                    .Include(x => x.Feature)
                    .ToList(),

                MembershipFeature_form = new MembershipFeatureValidate()
            };
            ViewData["Title"] = "Feature Value";
            ViewData["Subtitle"] = "Here you can Add , Edit and Delete Feature Value";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FeatureValue(MembershipFeatureVM model)
        {
            ViewBag.Memberships = _context.Memberships.ToList();
            ViewBag.Features = _context.Features.ToList();

      
            model.ShowForm = true;

            if (!ModelState.IsValid)
            {

                model.MembershipFeature_list = _context.MembershipFeatures
                    .Include(x => x.Membership)
                    .Include(x => x.Feature)
                    .ToList();
                return View(model);
            }

            // 🔎 Check UNIQUE constraint
            bool alreadyExists = _context.MembershipFeatures.Any(x =>
                x.MembershipId == model.MembershipFeature_form.MembershipId &&
                x.FeatureId == model.MembershipFeature_form.FeatureId &&
                x.MembershipFeatureId != model.MembershipFeature_form.MembershipFeatureId
            );

            if (alreadyExists)
            {
                ModelState.AddModelError("", "This feature is already assigned to this membership.");
                model.MembershipFeature_list = _context.MembershipFeatures
                    .Include(x => x.Membership)
                    .Include(x => x.Feature)
                    .ToList();
                return View(model);
            }

            // ✏️ EDIT or ➕ ADD
            if (model.MembershipFeature_form.MembershipFeatureId > 0)
            {
                var existing = _context.MembershipFeatures
                    .FirstOrDefault(x => x.MembershipFeatureId == model.MembershipFeature_form.MembershipFeatureId);

                if (existing == null)
                    return NotFound();

                existing.MembershipId = model.MembershipFeature_form.MembershipId;
                existing.FeatureId = model.MembershipFeature_form.FeatureId;
                existing.MaxAmount = model.MembershipFeature_form.MaxAmount;
                existing.IsEnabled = model.MembershipFeature_form.IsEnabled;

                TempData["Message"] = "Membership Feature updated successfully.";
            }
            else
            {
                var entity = new MembershipFeature
                {
                    MembershipId = model.MembershipFeature_form.MembershipId,
                    FeatureId = model.MembershipFeature_form.FeatureId,
                    MaxAmount = model.MembershipFeature_form.MaxAmount,
                    IsEnabled = model.MembershipFeature_form.IsEnabled
                };

                _context.MembershipFeatures.Add(entity);
                TempData["Message"] = "Membership Feature added successfully.";
            }
            ViewData["Title"] = "Feature Value";
            ViewData["Subtitle"] = "Here you can Add , Edit and Delete Feature Value";
            _context.SaveChanges();
            return RedirectToAction(nameof(FeatureValue));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMembershipFeatureAjax(int id)
        {
            var f = _context.MembershipFeatures.Find(id);
            if (f == null)
                return Json(new { success = false, message = "Not found" });

            _context.MembershipFeatures.Remove(f);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END FEATURE VALUE MANAGEMENT ====================


        // ==================== ADS MANAGEMENT ====================
        public IActionResult Ads()
        {
            var model = _context.Advertisements
                .Include(c => c.Payment)
                    .ThenInclude(p => p.User)
                .Include(c => c.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            var vm = new CDVM
            {
                ad_list = model,
                Payment_list = _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.PaymentMethod)
                    .Where(p => p.PaymentPurpose == "Advertisement")
                    .ToList()
            };
            ViewData["Title"] = "Ads Management";
            ViewData["Subtitle"] = "Manage your ads";
            return View(vm);
        }
        [HttpGet]
        public IActionResult ViewAds(int id)
        {
            var model = _context.Advertisements.Include(a => a.Payment)
              .ThenInclude(p => p.User).Include(a => a.Payment)
              .ThenInclude(p => p.PaymentMethod).Include(a => a.Payment)
              .ThenInclude(p => p.PaymentAmount)  
              .FirstOrDefault(a => a.AdvertisementId == id);


            if (model == null)
            {
                return NotFound();
            }

            var vm = new CDVM
            {
                single_ad = model
            };
            ViewData["Title"] = "Ad Deatil";
            ViewData["Subtitle"] = "View complete detail of ad";
            return View(vm);
        }

        [HttpGet]
        public IActionResult EditAd(int id)
        {
            var model = _context.Advertisements
                 .Include(a => a.Payment)
                     .ThenInclude(p => p.User)
                 .Include(a => a.Payment)
                     .ThenInclude(p => p.PaymentMethod)
                 .FirstOrDefault(a => a.AdvertisementId == id);

            if (model == null)
            {
                return NotFound();
            }

            var vm = new CDVM
            {
                single_ad = model
            };
            ViewData["Title"] = "Edit Ads Status";
            ViewData["Subtitle"] = "Admin only update status";
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAd(CDVM model)
        {
            if (model?.single_ad == null || model.single_ad.AdvertisementId == 0)
                return BadRequest("Invalid ad data.");

            var ad = _context.Advertisements
                .Include(a => a.Payment)
                    .ThenInclude(p => p.User)
                .FirstOrDefault(a => a.AdvertisementId == model.single_ad.AdvertisementId);

            if (ad == null)
                return NotFound();

            ad.ApprovalStatus = model.single_ad.ApprovalStatus;

            try
            {
                var email = ad.Payment?.User?.Email;  // SAFE access

                if (!string.IsNullOrEmpty(email))
                {
                    if (ad.ApprovalStatus == "Approved")
                    {
                        await _emailService.SendEmailAsync(
                            email,
                            "Ad Approved - Radio Cabs",
                            "<h3>Your ad has been approved!</h3><p>Congratulations! Your ad has been approved.</p><p>Thank you.</p>"
                        );
                    }
                    else if (ad.ApprovalStatus == "Rejected")
                    {
                        await _emailService.SendEmailAsync(
                            email,
                            "Ad Rejected - Radio Cabs",
                            "<h3>Your ad has been rejected</h3><p>Please review and submit again.</p>"
                        );
                    }
                }
                else
                {
                    TempData["Error"] = "Ad updated, but user email was not found.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
                TempData["Error"] = "Ad updated, but email failed: " + ex.Message;
            }

            _context.SaveChanges();
            TempData["SuccessAds"] = "Ad status updated successfully!";
            return RedirectToAction("Ads");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAdvertisement(int id)
        {
            try
            {
                var advertisement = _context.Advertisements.FirstOrDefault(a => a.AdvertisementId == id);

                if (advertisement == null)
                {
                    return Json(new { success = false, message = "Advertisement not found" });
                }

                _context.Advertisements.Remove(advertisement);
                _context.SaveChanges();

                return Json(new { success = true, message = "Advertisement deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // ==================== END ADS MANAGEMENT ====================

        // ==================== JOB VACANCUY MANAGEMENT ====================
        public IActionResult JobVacancyManagement(string jobType = "", string status = "")
        {
            var viewModel = new CDVM();

            // Get current date for expiry check
            var currentDate = DateTime.Now;

            var validCompanies = (
               from p in _context.Payments
               join c in _context.Companies on p.UserId equals c.UserId
               where p.PaymentStatus == "Paid"
                   && p.ExpiryDate > DateTime.Now
               select c.CompanyId).Distinct().ToList();



            // Get vacancies for valid companies with company data
            var query = _context.CompanyVacancies
                .Include(cv => cv.Company)
                .Where(cv => validCompanies.Contains(cv.CompanyId))
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(cv => cv.JobType == jobType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(cv => cv.ApprovalStatus== status);
            }

            viewModel.CompanyVacancy_list = query
                .OrderByDescending(cv => cv.CreatedAt)
                .ToList();

            // Populate filter dropdowns
            viewModel.JobTypes = _context.CompanyVacancies
                .Select(cv => cv.JobType)
                .Distinct()
                .OrderBy(jt => jt)
                .ToList();

            viewModel.Statuses = new List<string> { "Pending", "Approved", "Rejected" };

            // Set selected filters
            viewModel.SelectedJobType = jobType;
            viewModel.SelectedStatus = status;

            // Get company details for display
            viewModel.Company_list = _context.Companies
                .Where(c => validCompanies.Contains(c.CompanyId))
                .Select(c => new CompanyDetailsVM
                {
                    CompanyId = c.CompanyId,
                    CompanyName = c.CompanyName,
                    ContactPerson = c.ContactPerson,
                    Designation = c.Designation,
                    Telephone = c.Telephone,
                    UserID = c.UserId
                })
                .ToList();
            ViewData["Title"] = "Job Vacancy Managememt";
            ViewData["Subtitle"] = "Manage company job vacancy";
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult UpdateVacancy(int id)
        {
            var vacancy = _context.CompanyVacancies
                .Include(v => v.Company)
                .FirstOrDefault(v => v.VacancyId == id);

            if (vacancy == null)
            {
                TempData["Error"] = "Vacancy not found!";
                return RedirectToAction("JobVacancyManagement");
            }
            ViewData["Title"] = "Update Vacancy";
            ViewData["Subtitle"] = "Update Vacancy Satus";
            return View(new CDVM { Single_vacancy = vacancy });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVacancy(CDVM model)
        {
            var vacancy = _context.CompanyVacancies
                .Include(v => v.Company).ThenInclude(c => c.User)
                .FirstOrDefault(v => v.VacancyId == model.Single_vacancy.VacancyId);

            if (vacancy == null)
            {
                TempData["Error"] = "Vacancy not found!";
                return RedirectToAction("JobVacancyManagement");
            }
            vacancy.ApprovalStatus = model.Single_vacancy.ApprovalStatus;
            vacancy.AdminRemarks = model.Single_vacancy.AdminRemarks;
            try
            {
                if (vacancy.ApprovalStatus == "Approved")
                {
                    await _emailService.SendEmailAsync(
                        vacancy.Company.Email,   // adjust if different
                        "Vacancy Approved - Radio Cabs",
                        $"<h3>Your job vacancy has been approved!</h3>" +
                        $"<p>Remarks: {vacancy.AdminRemarks}</p>" +
                        $"<p>You can now receive applications.</p>"
                    );
                }
                else if (vacancy.ApprovalStatus == "Rejected")
                {
                    await _emailService.SendEmailAsync(
                        vacancy.Company.Email,   // adjust if different
                        "Vacancy Rejected - Radio Cabs",
                        $"<h3>Your job vacancy has been rejected</h3>" +
                        $"<p>Remarks: {vacancy.ApprovalStatus}</p>" +
                        $"<p>Please review and submit again.</p>"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error: " + ex.Message);
                TempData["Error"] = "Vacancy updated, but email failed: " + ex.Message;
            }

            // Save changes
            _context.SaveChanges();

            TempData["Success"] = "Vacancy status updated successfully!";
            return RedirectToAction("JobVacancyManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVacancy(int id)
        {
            var f = _context.CompanyVacancies.Find(id);
            if (f == null)
                return Json(new { success = false, message = "Not found" });

            _context.CompanyVacancies.Remove(f);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        // ==================== END JOB VACANCY MANAGEMENT ====================

        public IActionResult CheckCompanyPaymentStatus(int companyId)
        {
            var company = _context.Companies.Find(companyId);
            if (company == null)
            {
                return Json(new { hasValidPayment = false });
            }

            var hasValidPayment = _context.Payments
                .Any(p => p.UserId == company.UserId
                       && p.PaymentStatus == "Paid"
                       && p.ExpiryDate > DateTime.Now);

            return Json(new { hasValidPayment = hasValidPayment });
        }

        [HttpPost]
        public IActionResult CheckPaymentBeforeUpdate(int vacancyId)
        {
            try
            {
                var vacancy = _context.CompanyVacancies
                    .Include(v => v.Company)
                    .FirstOrDefault(v => v.VacancyId == vacancyId);

                if (vacancy == null)
                {
                    return Json(new { hasValidPayment = false });
                }

                var hasValidPayment = _context.Payments
                    .Any(p => p.UserId == vacancy.Company.UserId
                           && p.PaymentStatus == "Paid"
                           && p.ExpiryDate > DateTime.Now);

                return Json(new { hasValidPayment = hasValidPayment });
            }
            catch
            {
                return Json(new { hasValidPayment = false });
            }
        }

    }
}


