using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RadioCab.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__City__F2D21B76EE74DE0E", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "Faq",
                columns: table => new
                {
                    FaqId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Question = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Faq__9C741C4378C6ECFA", x => x.FaqId);
                });

            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    FeatureId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FeatureKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FeatureName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feature__82230BC9828E6353", x => x.FeatureId);
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    MembershipId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembershipName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Membersh__92A786795904E66B", x => x.MembershipId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    PaymentMethodId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MethodName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__DC31C1D3E8758A66", x => x.PaymentMethodId);
                });

            migrationBuilder.CreateTable(
                name: "PlatformServices",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceDescription = table.Column<string>(type: "text", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Approved"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Platform__C51BB00AF1088673", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceDescription = table.Column<string>(type: "text", nullable: true),
                    IsForDriver = table.Column<bool>(type: "boolean", nullable: false),
                    IsForCompany = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Services__C51BB00A84AA8E3A", x => x.ServiceId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC984B90A8", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    MobileNo = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    FeedbackType = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    AdminRemarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Feedback__6A4BEDD66C664ECF", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedback_City",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId");
                });

            migrationBuilder.CreateTable(
                name: "MembershipFeature",
                columns: table => new
                {
                    MembershipFeatureId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembershipId = table.Column<int>(type: "integer", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: false),
                    MaxAmount = table.Column<int>(type: "integer", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Membersh__CF4CA4D3E13D3438", x => x.MembershipFeatureId);
                    table.ForeignKey(
                        name: "FK__Membershi__Featu__71D1E811",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "FeatureId");
                    table.ForeignKey(
                        name: "FK__Membershi__Membe__70DDC3D8",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "MembershipId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentAmount",
                columns: table => new
                {
                    PaymentAmountId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembershipId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DurationInMonths = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentA__D1A1FC47E5AAC7ED", x => x.PaymentAmountId);
                    table.ForeignKey(
                        name: "FK_PaymentAmount_Membership",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "MembershipId");
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FaxNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CompanyLogo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FbrCertificate = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    BusinessLicense = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MembershipId = table.Column<int>(type: "integer", nullable: false),
                    RegisterationStatus = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Company__2D971CACCA6DF4AA", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_Company_City",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId");
                    table.ForeignKey(
                        name: "FK_Company_Membership",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "MembershipId");
                    table.ForeignKey(
                        name: "FK_Company_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "ContactRequest",
                columns: table => new
                {
                    ContactRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "New"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ContactR__96BC305FE008A6DE", x => x.ContactRequestId);
                    table.ForeignKey(
                        name: "FK_ContactRequest_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Driver",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DriverName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Cnic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Experience = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DriverPhoto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DrivingLicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DrivingLicenseFile = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    VehicleInfo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MembershipId = table.Column<int>(type: "integer", nullable: false),
                    RegisterationStatus = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Driver__F1B1CD04B20C3A57", x => x.DriverId);
                    table.ForeignKey(
                        name: "FK_Driver_City",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "CityId");
                    table.ForeignKey(
                        name: "FK_Driver_Membership",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "MembershipId");
                    table.ForeignKey(
                        name: "FK_Driver_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PaymentAmountId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending"),
                    TransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    PaymentScreenshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PaymentPurpose = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payment__9B556A3832F375B9", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payment_Amount",
                        column: x => x.PaymentAmountId,
                        principalTable: "PaymentAmount",
                        principalColumn: "PaymentAmountId");
                    table.ForeignKey(
                        name: "FK_Payment_Method",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethod",
                        principalColumn: "PaymentMethodId");
                    table.ForeignKey(
                        name: "FK_Payment_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "CompanyFeedback",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CompanyF__6A4BEDD6CEF586DF", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_CompanyFeedback_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId");
                });

            migrationBuilder.CreateTable(
                name: "CompanyServices",
                columns: table => new
                {
                    CompanyServiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    ServiceDescription = table.Column<string>(type: "text", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CompanyS__44FB59A09AB73129", x => x.CompanyServiceId);
                    table.ForeignKey(
                        name: "FK_CompanyServices_Service",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId");
                    table.ForeignKey(
                        name: "FK_company_services_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyVacancy",
                columns: table => new
                {
                    VacancyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    JobDescription = table.Column<string>(type: "text", nullable: false),
                    JobType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiredExperience = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Salary = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Pending"),
                    AdminRemarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CompanyV__6456763F89BC3FE9", x => x.VacancyId);
                    table.ForeignKey(
                        name: "FK_CompanyVacancy_Company",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DriverFeedback",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DriverFe__6A4BEDD6FDF8474F", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_DriverFeedback_Driver",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "DriverId");
                });

            migrationBuilder.CreateTable(
                name: "DriverServices",
                columns: table => new
                {
                    DriverServiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DriverSe__29A2842642F447D9", x => x.DriverServiceId);
                    table.ForeignKey(
                        name: "FK_DriverServices_Driver",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "DriverId");
                    table.ForeignKey(
                        name: "FK_DriverServices_Service",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId");
                });

            migrationBuilder.CreateTable(
                name: "Advertisement",
                columns: table => new
                {
                    AdvertisementId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvertiserType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdvertiserId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AdImage = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PaymentId = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Advertis__C4C7F4CDABFE821A", x => x.AdvertisementId);
                    table.ForeignKey(
                        name: "FK_Advertisement_Payment",
                        column: x => x.PaymentId,
                        principalTable: "Payment",
                        principalColumn: "PaymentId");
                });

            migrationBuilder.CreateTable(
                name: "VacancyApplication",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VacancyId = table.Column<int>(type: "integer", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MobileNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CvFile = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CoverLetter = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Applied"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VacancyA__C93A4C99C86FBC7F", x => x.ApplicationId);
                    table.ForeignKey(
                        name: "FK_VacancyApplication_Vacancy",
                        column: x => x.VacancyId,
                        principalTable: "CompanyVacancy",
                        principalColumn: "VacancyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advertisement_PaymentId",
                table: "Advertisement",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "UQ__City__2CC2CDB8172FEFF0",
                table: "City",
                column: "ZipCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_CityId",
                table: "Company",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_MembershipId",
                table: "Company",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_UserId",
                table: "Company",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFeedback_CompanyId",
                table: "CompanyFeedback",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyServices_CompanyId",
                table: "CompanyServices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyServices_ServiceId",
                table: "CompanyServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyVacancy_CompanyId",
                table: "CompanyVacancy",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequest_UserId",
                table: "ContactRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_CityId",
                table: "Driver",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_MembershipId",
                table: "Driver",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Driver_UserId",
                table: "Driver",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverFeedback_DriverId",
                table: "DriverFeedback",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverServices_ServiceId",
                table: "DriverServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "UQ_Driver_Service",
                table: "DriverServices",
                columns: new[] { "DriverId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Feature__FCEFA3304746D133",
                table: "Feature",
                column: "FeatureKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_CityId",
                table: "Feedback",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipFeature_FeatureId",
                table: "MembershipFeature",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "UQ__Membersh__1A85B6C4EED665E2",
                table: "MembershipFeature",
                columns: new[] { "MembershipId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentAmountId",
                table: "Payment",
                column: "PaymentAmountId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentMethodId",
                table: "Payment",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_PaymentAmount",
                table: "PaymentAmount",
                columns: new[] { "MembershipId", "EntityType", "PaymentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__PaymentM__218CFB1759D59EC7",
                table: "PaymentMethod",
                column: "MethodName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Services__A42B5F9973C4124C",
                table: "Services",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053443CCF8B5",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacancyApplication_VacancyId",
                table: "VacancyApplication",
                column: "VacancyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advertisement");

            migrationBuilder.DropTable(
                name: "CompanyFeedback");

            migrationBuilder.DropTable(
                name: "CompanyServices");

            migrationBuilder.DropTable(
                name: "ContactRequest");

            migrationBuilder.DropTable(
                name: "DriverFeedback");

            migrationBuilder.DropTable(
                name: "DriverServices");

            migrationBuilder.DropTable(
                name: "Faq");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "MembershipFeature");

            migrationBuilder.DropTable(
                name: "PlatformServices");

            migrationBuilder.DropTable(
                name: "VacancyApplication");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "Driver");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropTable(
                name: "CompanyVacancy");

            migrationBuilder.DropTable(
                name: "PaymentAmount");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
