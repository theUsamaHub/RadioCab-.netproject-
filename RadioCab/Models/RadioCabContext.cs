using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RadioCab.Models;

public partial class RadioCabContext : DbContext
{
    public RadioCabContext()
    {
    }

    public RadioCabContext(DbContextOptions<RadioCabContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CompanyFeedback> CompanyFeedbacks { get; set; }

    public virtual DbSet<CompanyService> CompanyServices { get; set; }

    public virtual DbSet<CompanyVacancy> CompanyVacancies { get; set; }

    public virtual DbSet<ContactRequest> ContactRequests { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<DriverFeedback> DriverFeedbacks { get; set; }

    public virtual DbSet<DriverService> DriverServices { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Feature> Features { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<MembershipFeature> MembershipFeatures { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentAmount> PaymentAmounts { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PlatformService> PlatformServices { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VacancyApplication> VacancyApplications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.AdvertisementId).HasName("PK__Advertis__C4C7F4CDABFE821A");

            entity.Property(e => e.ApprovalStatus).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Payment).WithMany(p => p.Advertisements).HasConstraintName("FK_Advertisement_Payment");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__City__F2D21B76EE74DE0E");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Company__2D971CACCA6DF4AA");

            entity.Property(e => e.RegisterationStatus).HasDefaultValue("Pending");

            entity.HasOne(d => d.City).WithMany(p => p.Companies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Company_City");

            entity.HasOne(d => d.Membership).WithMany(p => p.Companies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Company_Membership");

            entity.HasOne(d => d.User).WithMany(p => p.Companies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Company_User");
        });

        modelBuilder.Entity<CompanyFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__CompanyF__6A4BEDD6CEF586DF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyFeedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CompanyFeedback_Company");
        });

        modelBuilder.Entity<CompanyService>(entity =>
        {
            entity.HasKey(e => e.CompanyServiceId).HasName("PK__CompanyS__44FB59A09AB73129");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.isActive).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyServices).HasConstraintName("FK_company_services_Company");

            entity.HasOne(d => d.Service).WithMany(p => p.CompanyServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CompanyServices_Service");
        });

        modelBuilder.Entity<CompanyVacancy>(entity =>
        {
            entity.HasKey(e => e.VacancyId).HasName("PK__CompanyV__6456763F89BC3FE9");

            entity.Property(e => e.ApprovalStatus).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithMany(p => p.CompanyVacancies).HasConstraintName("FK_CompanyVacancy_Company");
        });

        modelBuilder.Entity<ContactRequest>(entity =>
        {
            entity.HasKey(e => e.ContactRequestId).HasName("PK__ContactR__96BC305FE008A6DE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("New");

            entity.HasOne(d => d.User).WithMany(p => p.ContactRequests).HasConstraintName("FK_ContactRequest_User");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK__Driver__F1B1CD04B20C3A57");

            entity.Property(e => e.RegisterationStatus).HasDefaultValue("Pending");

            entity.HasOne(d => d.City).WithMany(p => p.Drivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Driver_City");

            entity.HasOne(d => d.Membership).WithMany(p => p.Drivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Driver_Membership");

            entity.HasOne(d => d.User).WithMany(p => p.Drivers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Driver_User");
        });

        modelBuilder.Entity<DriverFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__DriverFe__6A4BEDD6FDF8474F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverFeedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverFeedback_Driver");
        });

        modelBuilder.Entity<DriverService>(entity =>
        {
            entity.HasKey(e => e.DriverServiceId).HasName("PK__DriverSe__29A2842642F447D9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverServices_Driver");

            entity.HasOne(d => d.Service).WithMany(p => p.DriverServices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DriverServices_Service");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__Faq__9C741C4378C6ECFA");
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.FeatureId).HasName("PK__Feature__82230BC9828E6353");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD66C664ECF");

            entity.HasOne(d => d.City).WithMany(p => p.Feedbacks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedback_City");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Membersh__92A786795904E66B");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<MembershipFeature>(entity =>
        {
            entity.HasKey(e => e.MembershipFeatureId).HasName("PK__Membersh__CF4CA4D3E13D3438");

            entity.Property(e => e.IsEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.Feature).WithMany(p => p.MembershipFeatures)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Membershi__Featu__71D1E811");

            entity.HasOne(d => d.Membership).WithMany(p => p.MembershipFeatures)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Membershi__Membe__70DDC3D8");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A3832F375B9");

            entity.Property(e => e.PaymentStatus).HasDefaultValue("Pending");

            entity.HasOne(d => d.PaymentAmount).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Amount");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Method");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_User");
        });

        modelBuilder.Entity<PaymentAmount>(entity =>
        {
            entity.HasKey(e => e.PaymentAmountId).HasName("PK__PaymentA__D1A1FC47E5AAC7ED");

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Membership).WithMany(p => p.PaymentAmounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentAmount_Membership");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D3E8758A66");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<PlatformService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Platform__C51BB00AF1088673");

            entity.Property(e => e.ApprovalStatus).HasDefaultValue("Approved");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.isActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Services__C51BB00A84AA8E3A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("PK__Users__1788CCAC984B90A8");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");
        });

        modelBuilder.Entity<VacancyApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__VacancyA__C93A4C99C86FBC7F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Applied");

            entity.HasOne(d => d.Vacancy).WithMany(p => p.VacancyApplications).HasConstraintName("FK_VacancyApplication_Vacancy");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
