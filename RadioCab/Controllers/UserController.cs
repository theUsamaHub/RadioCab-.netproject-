using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadioCab.Models;
using RadioCab.Models.ViewModels;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database; 

namespace RadioCab.Controllers
{
    public class UserController : Controller
    {
        private readonly RadioCabContext context;
        private readonly ILogger<UserController> _logger;

        public UserController(RadioCabContext context)
        {
            this.context = context;
        }

        // GET: User/Index (Home Page)
        public IActionResult Index()
        {
            // Get total stats
            ViewBag.TotalCompanies = context.Companies
                .Count(c => c.RegisterationStatus == "Approved");

            ViewBag.TotalDrivers = context.Drivers
                .Count(d => d.RegisterationStatus == "Approved");

            ViewBag.TotalCities = context.Cities.Count();

            ViewBag.TotalServices = context.Services
                .Count(s => s.IsActive);

            // Get platform services
            var platformServices = context.PlatformServices
                .Where(p => p.isActive == true && p.ApprovalStatus == "Approved")
                .Take(6)
                .ToList();
            ViewBag.PlatformServices = platformServices;

            // Get ALL payment amounts for pricing table
            var allPayments = context.PaymentAmounts
                .Where(p => p.IsActive)
                .ToList();
            ViewBag.AllPayments = allPayments;

            // Get featured companies (approved and premium members first)
            var featuredCompanies = context.Companies
                .Include(c => c.City)
                .Include(c => c.Membership)
                .Where(c => c.RegisterationStatus == "Approved")
                .OrderByDescending(c => c.MembershipId) // Premium first (assuming higher ID = higher plan)
                .ThenByDescending(c => c.CompanyId)
                .Take(6)
                .ToList();
            ViewBag.FeaturedCompanies = featuredCompanies;

            // Get featured drivers (approved and premium members first)
            var featuredDrivers = context.Drivers
                .Include(d => d.City)
                .Include(d => d.Membership)
                .Where(d => d.RegisterationStatus == "Approved")
                .OrderByDescending(d => d.MembershipId) // Premium first
                .ThenByDescending(d => d.DriverId)
                .Take(6)
                .ToList();
            ViewBag.FeaturedDrivers = featuredDrivers;

            // Get active advertisements
            var advertisements = context.Advertisements
                .Where(a => a.IsActive &&
                           a.StartDate <= DateTime.Now &&
                           a.EndDate >= DateTime.Now &&
                           a.ApprovalStatus == "Approved")
                .OrderByDescending(a => a.CreatedAt)
                .Take(4)
                .ToList();
            ViewBag.Advertisements = advertisements;

            // Get FAQs
            var faqs = context.Faqs
                .OrderBy(f => f.FaqId)
                .Take(5)
                .ToList();
            ViewBag.FAQs = faqs;

            return View();
        }

        // GET: User/Companies
        public IActionResult Companies()
        {
            var currentDate = DateTime.Now;

            // Get all approved companies
            var allCompanies = context.Companies
                .Include(c => c.City)
                .Include(c => c.Membership)
                .Include(c => c.User)
                    .ThenInclude(u => u.Payments)
                .Where(c => c.RegisterationStatus == "Approved")
                .OrderByDescending(d => d.MembershipId) // Premium first
                .ThenByDescending(d => d.CompanyId)
                .AsEnumerable() // Switch to client-side for complex queries
                .ToList();

            // Create ViewModel list with calculated properties
            var companyVMs = new List<CompanyVM>();
            var cityIds = new HashSet<int>();

            foreach (var company in allCompanies)
            {
                // Check if company has valid payment OR is free membership
                bool hasValidPaymentOrFree = false;

                // Option 1: Free membership (MembershipId = 1) - no payment required
                if (company.MembershipId == 1)
                {
                    hasValidPaymentOrFree = true;
                }
                // Option 2: Paid membership - check for valid payment
                else
                {
                    var latestPayment = company.User?.Payments
                        .Where(p => (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Company") && // Check both possible purposes
                                   (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") && // Check both statuses
                                   p.ExpiryDate >= currentDate)
                        .OrderByDescending(p => p.ExpiryDate)
                        .FirstOrDefault();

                    hasValidPaymentOrFree = latestPayment != null;
                }

                if (hasValidPaymentOrFree)
                {
                    // Calculate average rating
                    var averageRating = context.CompanyFeedbacks
                        .Where(f => f.CompanyId == company.CompanyId)
                        .Average(f => (double?)f.Rating) ?? 0;

                    // Get services count
                    var servicesCount = context.CompanyServices
                        .Count(cs => cs.CompanyId == company.CompanyId && cs.isActive == true);

                    // Get feedback count
                    var feedbackCount = context.CompanyFeedbacks
                        .Count(f => f.CompanyId == company.CompanyId);

                    companyVMs.Add(new CompanyVM
                    {
                        Company = company,
                        AverageRating = averageRating,
                        ServicesCount = servicesCount,
                        FeedbackCount = feedbackCount
                    });

                    cityIds.Add(company.CityId);
                }
            }

            // Pass counts for stats
            ViewBag.TotalCompanies = companyVMs.Count;
            ViewBag.TotalCities = cityIds.Count;

            // Calculate overall average rating
            ViewBag.AverageRating = companyVMs.Any() ?
                companyVMs.Average(c => c.AverageRating) : 0;

            return View(companyVMs);
        }


        // GET: User/CompanyDetails
        public IActionResult CompanyDetails(int id)
    {
            var company = context.Companies
           .Include(c => c.City)
           .Include(c => c.Membership)
           .Include(c => c.CompanyServices)
               .ThenInclude(cs => cs.Service)
           .Include(c => c.CompanyFeedbacks)
           .FirstOrDefault(c => c.CompanyId == id && c.RegisterationStatus == "Approved");

            if (company == null)
            {
                return NotFound();
            }

            // Check if company has valid payment OR is free membership
            var currentDate = DateTime.Now;

            // Option 1: Free membership (MembershipId = 1) - no payment required
            if (company.MembershipId == 1)
            {
                // Free membership - always valid
            }
            // Option 2: Paid membership - check for valid payment
            else
            {
                var hasValidPayment = context.Payments
                    .Any(p => p.UserId == company.UserId &&
                             (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Company") && // Both purposes
                             (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") &&        // Both statuses
                             p.ExpiryDate >= currentDate);

                if (!hasValidPayment)
                {
                    return NotFound("Company subscription has expired.");
                }
            }

            // Continue with the rest of your detail page logic...

            // Calculate average rating
            ViewBag.AverageRating = company.CompanyFeedbacks.Any() ?
            company.CompanyFeedbacks.Average(f => f.Rating) : 0;

        // Get rating distribution
        ViewBag.RatingDistribution = Enumerable.Range(1, 5)
            .Select(rating => new
            {
                Rating = rating,
                Count = company.CompanyFeedbacks.Count(f => f.Rating == rating),
                Percentage = company.CompanyFeedbacks.Any() ?
                    (company.CompanyFeedbacks.Count(f => f.Rating == rating) * 100.0 / company.CompanyFeedbacks.Count()) : 0
            })
            .ToList();

        // Prepare view models for forms
        ViewBag.ContactRequestVM = new ContactRequestVM
        {
            TargetType = "Company",
            TargetId = id
        };

        ViewBag.FeedbackVM = new CompanyFeedbackVM
        {
            CompanyId = id
        };

        return View(company);
    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitContactRequest(ContactRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                // Redirect based on target type
                return model.TargetType switch
                {
                    "Company" => RedirectToAction("CompanyDetails", new { id = model.TargetId }),
                    "Driver" => RedirectToAction("DriverDetails", new { id = model.TargetId }),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            var contactRequest = new ContactRequest
            {
                TargetType = model.TargetType,
                TargetId = model.TargetId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Message = model.Message,
                Status = "New",
                CreatedAt = DateTime.Now
            };

            context.ContactRequests.Add(contactRequest);
            context.SaveChanges();

            // Redirect based on target type
            return model.TargetType switch
            {
                "Company" => RedirectToAction("CompanyDetails", new { id = model.TargetId }),
                "Driver" => RedirectToAction("DriverDetails", new { id = model.TargetId }),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitCompanyFeedback(CompanyFeedbackVM model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("CompanyDetails", new { id = model.CompanyId });

            var feedback = new CompanyFeedback
            {
                CompanyId = model.CompanyId,
                Rating = model.Rating,
                Name = model.Name,
                Email = model.Email,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            context.CompanyFeedbacks.Add(feedback);
            context.SaveChanges();

            return RedirectToAction("CompanyDetails", new { id = model.CompanyId });
        }

        // Update the Drivers action:
        public IActionResult Drivers()
        {
            var currentDate = DateTime.Now;

            // Get all approved drivers
            var allDrivers = context.Drivers
                .Include(d => d.City)
                .Include(d => d.Membership)
                .Include(d => d.User)
                    .ThenInclude(u => u.Payments)
                .Where(d => d.RegisterationStatus == "Approved")
                .OrderByDescending(d => d.MembershipId) // Premium first
                .ThenByDescending(d => d.DriverId)
                .AsEnumerable()
                .ToList();

            // Create ViewModel list with calculated properties
            var driverVMs = new List<DriverVM>();
            var cityIds = new HashSet<int>();

            foreach (var driver in allDrivers)
            {
                // Check if driver has valid payment OR is free membership
                bool hasValidPaymentOrFree = false;

                // Option 1: Free membership (MembershipId = 1) - no payment required
                if (driver.MembershipId == 1)
                {
                    hasValidPaymentOrFree = true;
                }
                // Option 2: Paid membership - check for valid payment
                else
                {
                    var latestPayment = driver.User?.Payments
                        .Where(p => (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Driver") &&
                                   (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") &&
                                   p.ExpiryDate >= currentDate)
                        .OrderByDescending(p => p.ExpiryDate)
                        .FirstOrDefault();

                    hasValidPaymentOrFree = latestPayment != null;
                }

                if (hasValidPaymentOrFree)
                {
                    // Calculate average rating
                    var averageRating = context.DriverFeedbacks
                        .Where(f => f.DriverId == driver.DriverId)
                        .Average(f => (double?)f.Rating) ?? 0;

                    // Get feedback count
                    var feedbackCount = context.DriverFeedbacks
                        .Count(f => f.DriverId == driver.DriverId);

                    // Get services count
                    var servicesCount = context.DriverServices
                        .Count(ds => ds.DriverId == driver.DriverId && ds.IsActive == true);

                    driverVMs.Add(new DriverVM
                    {
                        Driver = driver,
                        AverageRating = averageRating,
                        FeedbackCount = feedbackCount,
                        ServicesCount = servicesCount // Add this
                    });

                    cityIds.Add(driver.CityId);
                }
            }

            // Pass counts for stats
            ViewBag.TotalDrivers = driverVMs.Count;
            ViewBag.TotalCities = cityIds.Count;

            // Calculate overall average rating
            ViewBag.AverageRating = driverVMs.Any() ?
                driverVMs.Average(c => c.AverageRating) : 0;

            return View(driverVMs);
        }
        // GET: User/DriverDetails
        public IActionResult DriverDetails(int id)
        {
            var driver = context.Drivers
      .Include(d => d.City)
      .Include(d => d.Membership)
      .Include(d => d.DriverServices)  // Ensure this is included
          .ThenInclude(ds => ds.Service)
      .Include(d => d.DriverFeedbacks)
      .FirstOrDefault(d => d.DriverId == id && d.RegisterationStatus == "Approved");
            if (driver == null)
            {
                return NotFound();
            }

            // Check if driver has valid payment OR is free membership
            var currentDate = DateTime.Now;

            // Option 1: Free membership (MembershipId = 1) - no payment required
            if (driver.MembershipId == 1)
            {
                // Free membership - always valid
            }
            // Option 2: Paid membership - check for valid payment
            else
            {
                var hasValidPayment = context.Payments
                    .Any(p => p.UserId == driver.UserId &&
                             (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Driver") &&
                             (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") &&
                             p.ExpiryDate >= currentDate);

                if (!hasValidPayment)
                {
                    return NotFound("Driver subscription has expired.");
                }
            }

            // Calculate average rating
            ViewBag.AverageRating = driver.DriverFeedbacks.Any() ?
                driver.DriverFeedbacks.Average(f => f.Rating) : 0;

            // Get rating distribution
            ViewBag.RatingDistribution = Enumerable.Range(1, 5)
                .Select(rating => new
                {
                    Rating = rating,
                    Count = driver.DriverFeedbacks.Count(f => f.Rating == rating),
                    Percentage = driver.DriverFeedbacks.Any() ?
                        (driver.DriverFeedbacks.Count(f => f.Rating == rating) * 100.0 / driver.DriverFeedbacks.Count()) : 0
                })
                .ToList();

            // Prepare view models for forms
            ViewBag.ContactRequestVM = new ContactRequestVM
            {
                TargetType = "Driver",
                TargetId = id
            };

            ViewBag.FeedbackVM = new DriverFeedbackVM
            {
                DriverId = id
            };

            return View(driver);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDriverFeedback(DriverFeedbackVM model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("DriverDetails", new { id = model.DriverId });

            var feedback = new DriverFeedback
            {
                DriverId = model.DriverId,
                Rating = model.Rating,
                Name = model.Name,
                Email = model.Email,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            context.DriverFeedbacks.Add(feedback);
            context.SaveChanges();

            return RedirectToAction("DriverDetails", new { id = model.DriverId });
        }
        // GET: User/Popular
        public IActionResult Popular()
        {
            // Get popular companies (with highest ratings/most services)
            var popularCompanies = context.Companies
                .Include(c => c.City)
                .Include(c => c.CompanyFeedbacks)
                .Where(c => c.RegisterationStatus == "Approved")
                .Select(c => new
                {
                    Company = c,
                    AverageRating = c.CompanyFeedbacks.Any() ? c.CompanyFeedbacks.Average(f => f.Rating) : 0,
                    FeedbackCount = c.CompanyFeedbacks.Count()
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.FeedbackCount)
                .Take(10)
                .ToList();

            // Get popular drivers (with highest ratings/most services)
            var popularDrivers = context.Drivers
                .Include(d => d.City)
                .Include(d => d.DriverFeedbacks)
                .Include(d => d.DriverServices)
                .Where(d => d.RegisterationStatus == "Approved")
                .Select(d => new
                {
                    Driver = d,
                    AverageRating = d.DriverFeedbacks.Any() ? d.DriverFeedbacks.Average(f => f.Rating) : 0,
                    FeedbackCount = d.DriverFeedbacks.Count(),
                    ServiceCount = d.DriverServices.Count()
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ServiceCount)
                .Take(10)
                .ToList();

            // Get popular services
            var popularServices = context.Services
                .Include(s => s.DriverServices)
                .Include(s => s.CompanyServices)
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    Service = s,
                    DriverCount = s.DriverServices.Count(),
                    CompanyCount = s.CompanyServices.Count(),
                    TotalUsage = s.DriverServices.Count() + s.CompanyServices.Count()
                })
                .OrderByDescending(x => x.TotalUsage)
                .Take(15)
                .ToList();

            ViewBag.PopularCompanies = popularCompanies;
            ViewBag.PopularDrivers = popularDrivers;
            ViewBag.PopularServices = popularServices;

            return View();
        }
        // GET: User/Services
        public IActionResult Services()
        {
            // Get regular services (transport services)
            var services = context.Services
                .Include(s => s.DriverServices)
                .Include(s => s.CompanyServices)
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToList();

            // Get platform services
            var platformServices = context.PlatformServices
                .Where(p => p.isActive == true && p.ApprovalStatus == "Approved")
                .OrderBy(p => p.ServiceName)
                .ToList();

            // Get counts for filters - CORRECTED LOGIC
            ViewBag.ForDriverCount = services.Count(s => s.IsForDriver && !s.IsForCompany);
            ViewBag.ForCompanyCount = services.Count(s => !s.IsForDriver && s.IsForCompany);
            ViewBag.ForBothCount = services.Count(s => s.IsForDriver && s.IsForCompany);
            ViewBag.PlatformServicesCount = platformServices.Count();

            ViewBag.PlatformServices = platformServices;

            return View(services);
        }

        // GET: User/Vacancies
        // GET: User/Vacancies
        public IActionResult Vacancies()
        {
            var currentDate = DateTime.Now;

            // Get active vacancies from companies with active payments in a single query
            var vacancies = context.CompanyVacancies
                .Include(v => v.Company)
                    .ThenInclude(c => c.City)
                .Include(v => v.Company)
                    .ThenInclude(c => c.User)
                        .ThenInclude(u => u.Payments)
                .Include(v => v.VacancyApplications)
                .Where(v => v.IsActive && v.ApprovalStatus == "Approved")
                .AsEnumerable() // Switch to client-side for complex evaluation
                .Where(v =>
                {
                    var company = v.Company;

                    // Free membership companies cannot post vacancies
                    if (company.MembershipId == 1)
                        return false;

                    // Check if company has valid payment
                    var hasValidPayment = company.User?.Payments?
                        .Any(p => (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Company") &&
                                 (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") &&
                                 p.ExpiryDate >= currentDate) ?? false;

                    return hasValidPayment;
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToList();

            // Create ViewModel list
            var vacancyVMs = vacancies.Select(v => new VacancyVM
            {
                Vacancy = v,
                ApplicationCount = v.VacancyApplications.Count,
                IsEligibleToApply = true
            }).ToList();

            // Pass counts for stats
            ViewBag.TotalVacancies = vacancyVMs.Count;
            ViewBag.TotalCompanies = vacancies.Select(v => v.CompanyId).Distinct().Count();
            ViewBag.TotalCities = vacancies.Select(v => v.Company.CityId).Distinct().Count();
            ViewBag.TotalApplications = vacancies.Sum(v => v.VacancyApplications.Count);

            // Get unique job types for filter
            ViewBag.JobTypes = vacancies
                .Select(v => v.JobType)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return View(vacancyVMs);
        }
        // GET: User/VacancyDetails/{id}
        public IActionResult VacancyDetails(int id)
        {
            var vacancy = context.CompanyVacancies
                .Include(v => v.Company)
                    .ThenInclude(c => c.City)
                .Include(v => v.VacancyApplications)
                .FirstOrDefault(v => v.VacancyId == id &&
                                   v.IsActive &&
                                   v.ApprovalStatus == "Approved");

            if (vacancy == null)
            {
                return NotFound();
            }

            // Check if company has active payment
            var currentDate = DateTime.Now;
            var companyHasPayment = context.Payments
                .Any(p => p.UserId == vacancy.Company.UserId &&
                         (p.PaymentPurpose == "Membership" || p.PaymentPurpose == "Company") &&
                         (p.PaymentStatus == "Paid" || p.PaymentStatus == "Approved") &&
                         p.ExpiryDate >= currentDate);

            if (!companyHasPayment && vacancy.Company.MembershipId != 1)
            {
                return NotFound("Company subscription has expired.");
            }

            ViewBag.ApplicationCount = vacancy.VacancyApplications.Count;

            // Prepare view model for application form
            ViewBag.ApplicationVM = new VacancyApplicationVM
            {
                VacancyId = id
            };

            return View(vacancy);
        }

        // POST: User/ApplyForVacancy
        // POST: User/ApplyForVacancy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForVacancy(VacancyApplicationVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
            }

            // Check if vacancy exists and is active
            var vacancy = await context.CompanyVacancies
                .FirstOrDefaultAsync(v => v.VacancyId == model.VacancyId &&
                                        v.IsActive &&
                                        v.ApprovalStatus == "Approved");

            if (vacancy == null)
            {
                TempData["ErrorMessage"] = "Vacancy not found or is no longer active.";
                return RedirectToAction("Vacancies");
            }

            // Handle CV file upload
            string cvFilePath = null;
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                // Validate file size (max 5MB)
                if (model.CvFile.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "CV file size must be less than 5MB.";
                    return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var cvExtension = Path.GetExtension(model.CvFile.FileName).ToLower();
                if (!allowedExtensions.Contains(cvExtension))
                {
                    TempData["ErrorMessage"] = "CV file must be PDF, DOC, or DOCX format.";
                    return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
                }

                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine("wwwroot", "Uploads", "vacancyapplications");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique filename
                var cvFileName = $"{Guid.NewGuid()}{cvExtension}";
                cvFilePath = Path.Combine("Uploads", "vacancyapplications", cvFileName);

                // Save file
                var fullPath = Path.Combine("wwwroot", cvFilePath);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.CvFile.CopyToAsync(stream);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "CV file is required.";
                return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
            }

            // Handle Cover Letter file upload (optional)
            string coverLetterFilePath = null;
            if (model.CoverLetterFile != null && model.CoverLetterFile.Length > 0)
            {
                // Validate file size (max 5MB)
                if (model.CoverLetterFile.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Cover letter file size must be less than 5MB.";
                    return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var coverLetterExtension = Path.GetExtension(model.CoverLetterFile.FileName).ToLower();
                if (!allowedExtensions.Contains(coverLetterExtension))
                {
                    TempData["ErrorMessage"] = "Cover letter must be PDF, DOC, DOCX, or TXT format.";
                    return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
                }

                // Generate unique filename for cover letter
                var coverLetterFileName = $"cover_{Guid.NewGuid()}{coverLetterExtension}";
                coverLetterFilePath = Path.Combine("Uploads", "vacancyapplications", coverLetterFileName);

                // Save file
                var fullPath = Path.Combine("wwwroot", coverLetterFilePath);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.CoverLetterFile.CopyToAsync(stream);
                }
            }

            // Create application
            var application = new VacancyApplication
            {
                VacancyId = model.VacancyId,
                ApplicantName = model.ApplicantName,
                Email = model.Email,
                MobileNo = model.MobileNo,
                CvFile = cvFilePath,
                CoverLetter = coverLetterFilePath, // Now stores file path instead of text
                Status = "Applied",
                CreatedAt = DateTime.Now
            };

            context.VacancyApplications.Add(application);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your application has been submitted successfully!";
            return RedirectToAction("VacancyDetails", new { id = model.VacancyId });
        }

        // GET: User/Advertisements
        public IActionResult Advertisements(string sortBy = "newest")
        {
            var currentDate = DateTime.Now;

            // Get active advertisements with valid payments
            var activeAds = context.Advertisements
                .Include(a => a.Payment)
                .Where(a => a.IsActive &&
                           a.ApprovalStatus == "Approved" &&
                           a.StartDate <= currentDate &&
                           a.EndDate >= currentDate)
                .AsEnumerable()
                .Where(a =>
                {
                    // Check if payment is valid
                    var payment = a.Payment;
                    return payment != null &&
                           payment.PaymentPurpose == "Advertisement" &&
                           payment.PaymentStatus == "Paid" &&
                           payment.ExpiryDate >= currentDate;
                })
                .ToList();

            // Create ViewModel list with advertiser details
            var adVMs = new List<AdvertisementVM>();

            foreach (var ad in activeAds)
            {
                AdvertisementVM vm = null;

                if (ad.AdvertiserType == "Company")
                {
                    var company = context.Companies
                        .Include(c => c.City)
                        .Include(c => c.CompanyFeedbacks)
                        .FirstOrDefault(c => c.CompanyId == ad.AdvertiserId);

                    if (company != null && company.RegisterationStatus == "Approved")
                    {
                        vm = new AdvertisementVM
                        {
                            AdvertisementId = ad.AdvertisementId,
                            AdvertiserType = ad.AdvertiserType,
                            AdvertiserId = ad.AdvertiserId,
                            Title = ad.Title,
                            Description = ad.Description,
                            AdImage = ad.AdImage,
                            StartDate = ad.StartDate,
                            EndDate = ad.EndDate,
                            AdvertiserName = company.CompanyName,
                            CityName = company.City?.CityName,
                            ProfileImage = company.CompanyLogo,
                            Rating = company.CompanyFeedbacks.Any() ?
                                company.CompanyFeedbacks.Average(f => f.Rating) : 0,
                            ReviewCount = company.CompanyFeedbacks.Count,
                            //DaysRemaining = (ad.EndDate - currentDate).Days
                        };
                    }
                }
                else if (ad.AdvertiserType == "Driver")
                {
                    var driver = context.Drivers
                        .Include(d => d.City)
                        .Include(d => d.DriverFeedbacks)
                        .FirstOrDefault(d => d.DriverId == ad.AdvertiserId);

                    if (driver != null && driver.RegisterationStatus == "Approved")
                    {
                        vm = new AdvertisementVM
                        {
                            AdvertisementId = ad.AdvertisementId,
                            AdvertiserType = ad.AdvertiserType,
                            AdvertiserId = ad.AdvertiserId,
                            Title = ad.Title,
                            Description = ad.Description,
                            AdImage = ad.AdImage,
                            StartDate = ad.StartDate,
                            EndDate = ad.EndDate,
                            AdvertiserName = driver.DriverName,
                            CityName = driver.City?.CityName,
                            ProfileImage = driver.DriverPhoto,
                            Rating = driver.DriverFeedbacks.Any() ?
                                driver.DriverFeedbacks.Average(f => f.Rating) : 0,
                            ReviewCount = driver.DriverFeedbacks.Count,
                            //DaysRemaining = (ad.EndDate - currentDate).Days
                        };
                    }
                }

                if (vm != null)
                {
                    adVMs.Add(vm);
                }
            }

            // Apply sorting
            switch (sortBy.ToLower())
            {
                case "newest":
                    adVMs = adVMs.OrderByDescending(a => a.StartDate).ToList();
                    break;
                case "oldest":
                    adVMs = adVMs.OrderBy(a => a.StartDate).ToList();
                    break;
                case "rating":
                    adVMs = adVMs.OrderByDescending(a => a.Rating).ToList();
                    break;
                case "expiring":
                    adVMs = adVMs.OrderBy(a => a.DaysRemaining).ToList();
                    break;
                default:
                    adVMs = adVMs.OrderByDescending(a => a.StartDate).ToList();
                    break;
            }

            // Group by advertiser type for filters
            ViewBag.TotalAds = adVMs.Count;
            ViewBag.CompanyAds = adVMs.Count(a => a.AdvertiserType == "Company");
            ViewBag.DriverAds = adVMs.Count(a => a.AdvertiserType == "Driver");
            ViewBag.ActiveAds = adVMs.Count(a => a.IsActive);
            ViewBag.SelectedSort = sortBy;

            return View(adVMs);
        }
    }
}
