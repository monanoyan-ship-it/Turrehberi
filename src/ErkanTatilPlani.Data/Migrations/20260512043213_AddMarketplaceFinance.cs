using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactSurname",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Companies",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalCompanyTitle",
                table: "Companies",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MarketplaceEnabled",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingErrorCode",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OnboardingErrorMessage",
                table: "Companies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PayoutDelayDays",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformCommissionRate",
                table: "Companies",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 12m);

            migrationBuilder.AddColumn<int>(
                name: "SellerLegalTypeId",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "SellerOnboardingStatusId",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubMerchantExternalId",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubMerchantKey",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxOffice",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReservationId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ConversationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BuyerIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SellerReceivableAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommissionRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IyziCommissionRateAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IyziCommissionFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CallbackReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedById = table.Column<int>(type: "integer", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BankReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayoutBatches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayoutBatches_Visitors_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceRefunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentTransactionId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    RequestedById = table.Column<int>(type: "integer", nullable: true),
                    ProcessedById = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProviderRefundId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderPaymentTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceRefunds_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceRefunds_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceRefunds_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceRefunds_Visitors_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketplaceRefunds_Visitors_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentTransactionId = table.Column<int>(type: "integer", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ProviderPaymentTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderTransactionStatus = table.Column<int>(type: "integer", nullable: false),
                    SubMerchantKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalSubMerchantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubMerchantPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubMerchantPayoutRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SubMerchantPayoutAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MerchantPayoutAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IyziCommissionRateAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IyziCommissionFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BlockageRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BlockageRateAmountMerchant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BlockageRateAmountSubMerchant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BlockageResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLineItems_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentLineItems_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentLineItems_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentTransactionId = table.Column<int>(type: "integer", nullable: true),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    PayoutBatchId = table.Column<int>(type: "integer", nullable: true),
                    EntryTypeId = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SettledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceLedgerEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketplaceLedgerEntries_PaymentTransactions_PaymentTransac~",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketplaceLedgerEntries_PayoutBatches_PayoutBatchId",
                        column: x => x.PayoutBatchId,
                        principalTable: "PayoutBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketplaceLedgerEntries_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ContactName", "ContactSurname", "Iban", "LegalCompanyTitle", "MarketplaceEnabled", "OnboardedAt", "OnboardingErrorCode", "OnboardingErrorMessage", "PayoutDelayDays", "PlatformCommissionRate", "SellerLegalTypeId", "SellerOnboardingStatusId", "SubMerchantExternalId", "SubMerchantKey", "TaxOffice" },
                values: new object[] { "", "", "", "", false, null, "", "", 7, 12m, 2, 0, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ContactName", "ContactSurname", "Iban", "LegalCompanyTitle", "MarketplaceEnabled", "OnboardedAt", "OnboardingErrorCode", "OnboardingErrorMessage", "PayoutDelayDays", "PlatformCommissionRate", "SellerLegalTypeId", "SellerOnboardingStatusId", "SubMerchantExternalId", "SubMerchantKey", "TaxOffice" },
                values: new object[] { "", "", "", "", false, null, "", "", 7, 12m, 2, 0, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ContactName", "ContactSurname", "Iban", "LegalCompanyTitle", "MarketplaceEnabled", "OnboardedAt", "OnboardingErrorCode", "OnboardingErrorMessage", "PayoutDelayDays", "PlatformCommissionRate", "SellerLegalTypeId", "SellerOnboardingStatusId", "SubMerchantExternalId", "SubMerchantKey", "TaxOffice" },
                values: new object[] { "", "", "", "", false, null, "", "", 7, 12m, 2, 0, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ContactName", "ContactSurname", "Iban", "LegalCompanyTitle", "MarketplaceEnabled", "OnboardedAt", "OnboardingErrorCode", "OnboardingErrorMessage", "PayoutDelayDays", "PlatformCommissionRate", "SellerLegalTypeId", "SellerOnboardingStatusId", "SubMerchantExternalId", "SubMerchantKey", "TaxOffice" },
                values: new object[] { "", "", "", "", false, null, "", "", 7, 12m, 2, 0, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ContactName", "ContactSurname", "Iban", "LegalCompanyTitle", "MarketplaceEnabled", "OnboardedAt", "OnboardingErrorCode", "OnboardingErrorMessage", "PayoutDelayDays", "PlatformCommissionRate", "SellerLegalTypeId", "SellerOnboardingStatusId", "SubMerchantExternalId", "SubMerchantKey", "TaxOffice" },
                values: new object[] { "", "", "", "", false, null, "", "", 7, 12m, 2, 0, "", "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SubMerchantExternalId",
                table: "Companies",
                column: "SubMerchantExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_SubMerchantKey",
                table: "Companies",
                column: "SubMerchantKey");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_AvailableAt",
                table: "MarketplaceLedgerEntries",
                column: "AvailableAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_CompanyId",
                table: "MarketplaceLedgerEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_PaymentTransactionId",
                table: "MarketplaceLedgerEntries",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_PayoutBatchId",
                table: "MarketplaceLedgerEntries",
                column: "PayoutBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_ReservationId",
                table: "MarketplaceLedgerEntries",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceLedgerEntries_StatusId",
                table: "MarketplaceLedgerEntries",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_CompanyId",
                table: "MarketplaceRefunds",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_PaymentTransactionId",
                table: "MarketplaceRefunds",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_ProcessedById",
                table: "MarketplaceRefunds",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_RequestedById",
                table: "MarketplaceRefunds",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_ReservationId",
                table: "MarketplaceRefunds",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceRefunds_StatusId",
                table: "MarketplaceRefunds",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLineItems_CompanyId",
                table: "PaymentLineItems",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLineItems_PaymentTransactionId",
                table: "PaymentLineItems",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLineItems_ProviderPaymentTransactionId",
                table: "PaymentLineItems",
                column: "ProviderPaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLineItems_ReservationId",
                table: "PaymentLineItems",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CompanyId",
                table: "PaymentTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ConversationId",
                table: "PaymentTransactions",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentToken",
                table: "PaymentTransactions",
                column: "PaymentToken");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReservationId",
                table: "PaymentTransactions",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_VisitorId",
                table: "PaymentTransactions",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_ApprovedById",
                table: "PayoutBatches",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_BatchNumber",
                table: "PayoutBatches",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_CompanyId",
                table: "PayoutBatches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBatches_StatusId",
                table: "PayoutBatches",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceLedgerEntries");

            migrationBuilder.DropTable(
                name: "MarketplaceRefunds");

            migrationBuilder.DropTable(
                name: "PaymentLineItems");

            migrationBuilder.DropTable(
                name: "PayoutBatches");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Companies_SubMerchantExternalId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_SubMerchantKey",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ContactSurname",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LegalCompanyTitle",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MarketplaceEnabled",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "OnboardedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "OnboardingErrorCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "OnboardingErrorMessage",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PayoutDelayDays",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PlatformCommissionRate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SellerLegalTypeId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SellerOnboardingStatusId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubMerchantExternalId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubMerchantKey",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxOffice",
                table: "Companies");
        }
    }
}
