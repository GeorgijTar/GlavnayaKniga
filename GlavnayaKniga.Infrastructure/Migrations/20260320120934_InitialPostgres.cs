using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GlavnayaKniga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_code = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    is_synthetic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_accounts_accounts_parent_id",
                        column: x => x.parent_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "counterpartys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    full_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    short_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    i_n_n = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    k_p_p = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    o_g_r_n = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    legal_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actual_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterpartys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "individuals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    birth_place = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    citizenship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    registration_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actual_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    passport_series = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    passport_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    passport_issue_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    passport_issued_by = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    passport_department_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    i_n_n = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    s_n_i_l_s = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_individuals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    short_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    education_requirements = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: true),
                    base_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transaction_bases",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_bases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units_of_measure",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    short_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    international_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units_of_measure", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    b_i_k = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    correspondent_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subaccount_id = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    open_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    close_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_accounts_accounts_subaccount_id",
                        column: x => x.subaccount_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    registration_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inventory_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    asset_type_id = table.Column<int>(type: "integer", nullable: false),
                    year_of_manufacture = table.Column<int>(type: "integer", nullable: true),
                    model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    commissioning_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decommissioning_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    initial_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    residual_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    responsible_person_id = table.Column<int>(type: "integer", nullable: true),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.id);
                    table.ForeignKey(
                        name: "FK_assets_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assets_asset_types_asset_type_id",
                        column: x => x.asset_type_id,
                        principalTable: "asset_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assets_counterpartys_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalTable: "counterpartys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "counterparty_bank_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    counterparty_id = table.Column<int>(type: "integer", nullable: false),
                    account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    b_i_k = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    correspondent_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_counterparty_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_counterparty_bank_accounts_counterpartys_counterparty_id",
                        column: x => x.counterparty_id,
                        principalTable: "counterpartys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "receipts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accounting_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    contractor_id = table.Column<int>(type: "integer", nullable: false),
                    credit_account_id = table.Column<int>(type: "integer", nullable: false),
                    vat_calculation_method = table.Column<int>(type: "integer", nullable: false),
                    contract_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contract_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    basis = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_vat_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_amount_with_vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_u_p_d = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    invoice_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    posted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipts", x => x.id);
                    table.ForeignKey(
                        name: "FK_receipts_accounts_credit_account_id",
                        column: x => x.credit_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receipts_counterpartys_contractor_id",
                        column: x => x.contractor_id,
                        principalTable: "counterpartys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bank_statements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    bank_account_id = table.Column<int>(type: "integer", nullable: true),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    imported_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_statements", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_statements_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    debit_account_id = table.Column<int>(type: "integer", nullable: false),
                    credit_account_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    basis_id = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    receipt_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_entries_accounts_credit_account_id",
                        column: x => x.credit_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entries_accounts_debit_account_id",
                        column: x => x.debit_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entries_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entries_transaction_bases_basis_id",
                        column: x => x.basis_id,
                        principalTable: "transaction_bases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bank_statement_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_statement_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    payer_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    payer_i_n_n = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payer_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    payer_b_i_k = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    recipient_account = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    recipient_i_n_n = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recipient_b_i_k = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    payment_purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    payment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: true),
                    received_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    withdrawn_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    entry_id = table.Column<int>(type: "integer", nullable: true),
                    payer_counterparty_id = table.Column<int>(type: "integer", nullable: true),
                    recipient_counterparty_id = table.Column<int>(type: "integer", nullable: true),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_statement_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_statement_documents_bank_statements_bank_statement_id",
                        column: x => x.bank_statement_id,
                        principalTable: "bank_statements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bank_statement_documents_counterpartys_payer_counterparty_id",
                        column: x => x.payer_counterparty_id,
                        principalTable: "counterpartys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bank_statement_documents_counterpartys_recipient_counterpar~",
                        column: x => x.recipient_counterparty_id,
                        principalTable: "counterpartys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bank_statement_documents_entries_entry_id",
                        column: x => x.entry_id,
                        principalTable: "entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    full_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    head_employee_id = table.Column<int>(type: "integer", nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentId",
                        column: x => x.parent_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    individual_id = table.Column<int>(type: "integer", nullable: false),
                    personnel_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    current_position_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hire_order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hire_order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dismissal_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dismissal_order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    dismissal_order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dismissal_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    work_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    work_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    manager_id = table.Column<int>(type: "integer", nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employees_employees_manager_id",
                        column: x => x.manager_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_individuals_individual_id",
                        column: x => x.individual_id,
                        principalTable: "individuals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_positions_current_position_id",
                        column: x => x.current_position_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employment_historys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    position_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    change_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_historys", x => x.id);
                    table.ForeignKey(
                        name: "FK_employment_historys_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employment_historys_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    responsible_employee_id = table.Column<int>(type: "integer", nullable: true),
                    area = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    temperature_regime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_storage_locations_employees_responsible_employee_id",
                        column: x => x.responsible_employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_storage_locations_storage_locations_parent_id",
                        column: x => x.parent_id,
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nomenclatures",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    full_name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    article = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    unit_id = table.Column<int>(type: "integer", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    default_vat_account_id = table.Column<int>(type: "integer", nullable: false),
                    storage_location_id = table.Column<int>(type: "integer", nullable: true),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    current_stock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    min_stock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    max_stock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nomenclatures", x => x.id);
                    table.ForeignKey(
                        name: "FK_nomenclatures_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nomenclatures_accounts_default_vat_account_id",
                        column: x => x.default_vat_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nomenclatures_storage_locations_storage_location_id",
                        column: x => x.storage_location_id,
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_nomenclatures_units_of_measure_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "receipt_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_id = table.Column<int>(type: "integer", nullable: false),
                    nomenclature_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vat_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    vat_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    amount_with_vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    debit_account_id = table.Column<int>(type: "integer", nullable: false),
                    vat_account_id = table.Column<int>(type: "integer", nullable: false),
                    storage_location_id = table.Column<int>(type: "integer", nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    line_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receipt_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_receipt_items_accounts_debit_account_id",
                        column: x => x.debit_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receipt_items_accounts_vat_account_id",
                        column: x => x.vat_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receipt_items_nomenclatures_nomenclature_id",
                        column: x => x.nomenclature_id,
                        principalTable: "nomenclatures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_receipt_items_receipts_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "receipts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_receipt_items_storage_locations_storage_location_id",
                        column: x => x.storage_location_id,
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_code",
                table: "accounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_code_is_archived",
                table: "accounts",
                columns: new[] { "code", "is_archived" },
                unique: true,
                filter: "\"is_archived\" = false");

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_full_code",
                table: "accounts",
                column: "full_code");

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_is_archived",
                table: "accounts",
                column: "is_archived");

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_is_synthetic",
                table: "accounts",
                column: "is_synthetic");

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_parent_id",
                table: "accounts",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "i_x_accounts_type",
                table: "accounts",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "i_x_asset_types_name",
                table: "asset_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_assets_account_id",
                table: "assets",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assets_asset_type_id",
                table: "assets",
                column: "asset_type_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assets_inventory_number",
                table: "assets",
                column: "inventory_number",
                unique: true,
                filter: "\"inventory_number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "i_x_assets_registration_number",
                table: "assets",
                column: "registration_number");

            migrationBuilder.CreateIndex(
                name: "i_x_assets_responsible_person_id",
                table: "assets",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "i_x_assets_serial_number",
                table: "assets",
                column: "serial_number");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_accounts_account_number",
                table: "bank_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_bank_accounts_subaccount_id",
                table: "bank_accounts",
                column: "subaccount_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_bank_statement_id",
                table: "bank_statement_documents",
                column: "bank_statement_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_date",
                table: "bank_statement_documents",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_entry_id",
                table: "bank_statement_documents",
                column: "entry_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_hash",
                table: "bank_statement_documents",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_payer_account_recipient_account",
                table: "bank_statement_documents",
                columns: new[] { "payer_account", "recipient_account" });

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_payer_counterparty_id",
                table: "bank_statement_documents",
                column: "payer_counterparty_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statement_documents_recipient_counterparty_id",
                table: "bank_statement_documents",
                column: "recipient_counterparty_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statements_account_number_start_date_end_date",
                table: "bank_statements",
                columns: new[] { "account_number", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statements_bank_account_id",
                table: "bank_statements",
                column: "bank_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_bank_statements_status",
                table: "bank_statements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_counterparty_bank_accounts_account_number",
                table: "counterparty_bank_accounts",
                column: "account_number");

            migrationBuilder.CreateIndex(
                name: "i_x_counterparty_bank_accounts_counterparty_id",
                table: "counterparty_bank_accounts",
                column: "counterparty_id");

            migrationBuilder.CreateIndex(
                name: "i_x_counterpartys_i_n_n",
                table: "counterpartys",
                column: "i_n_n",
                unique: true,
                filter: "\"i_n_n\" IS NOT NULL AND \"i_n_n\" != ''");

            migrationBuilder.CreateIndex(
                name: "i_x_counterpartys_short_name_is_archived",
                table: "counterpartys",
                columns: new[] { "short_name", "is_archived" });

            migrationBuilder.CreateIndex(
                name: "i_x_departments_code",
                table: "departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_departments_head_employee_id",
                table: "departments",
                column: "head_employee_id");

            migrationBuilder.CreateIndex(
                name: "i_x_departments_name",
                table: "departments",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "i_x_departments_parent_id",
                table: "departments",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_current_position_id",
                table: "employees",
                column: "current_position_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_department_id",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_dismissal_date",
                table: "employees",
                column: "dismissal_date");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_hire_date",
                table: "employees",
                column: "hire_date");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_individual_id",
                table: "employees",
                column: "individual_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_manager_id",
                table: "employees",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employees_personnel_number",
                table: "employees",
                column: "personnel_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_employees_status",
                table: "employees",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_employment_historys_change_type",
                table: "employment_historys",
                column: "change_type");

            migrationBuilder.CreateIndex(
                name: "i_x_employment_historys_employee_id",
                table: "employment_historys",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employment_historys_end_date",
                table: "employment_historys",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "i_x_employment_historys_position_id",
                table: "employment_historys",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "i_x_employment_historys_start_date",
                table: "employment_historys",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "i_x_entries_basis_id",
                table: "entries",
                column: "basis_id");

            migrationBuilder.CreateIndex(
                name: "i_x_entries_credit_account_id",
                table: "entries",
                column: "credit_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_entries_date",
                table: "entries",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "i_x_entries_debit_account_id",
                table: "entries",
                column: "debit_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_entries_receipt_id",
                table: "entries",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "i_x_individuals_i_n_n",
                table: "individuals",
                column: "i_n_n",
                unique: true,
                filter: "\"i_n_n\" IS NOT NULL AND \"i_n_n\" != ''");

            migrationBuilder.CreateIndex(
                name: "i_x_individuals_last_name_first_name_middle_name_birth_date",
                table: "individuals",
                columns: new[] { "last_name", "first_name", "middle_name", "birth_date" });

            migrationBuilder.CreateIndex(
                name: "i_x_individuals_s_n_i_l_s",
                table: "individuals",
                column: "s_n_i_l_s",
                unique: true,
                filter: "\"s_n_i_l_s\" IS NOT NULL AND \"s_n_i_l_s\" != ''");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_account_id",
                table: "nomenclatures",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_article",
                table: "nomenclatures",
                column: "article",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_barcode",
                table: "nomenclatures",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_default_vat_account_id",
                table: "nomenclatures",
                column: "default_vat_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_name",
                table: "nomenclatures",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_storage_location_id",
                table: "nomenclatures",
                column: "storage_location_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_type",
                table: "nomenclatures",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "i_x_nomenclatures_unit_id",
                table: "nomenclatures",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "i_x_positions_name",
                table: "positions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_receipt_items_debit_account_id",
                table: "receipt_items",
                column: "debit_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipt_items_nomenclature_id",
                table: "receipt_items",
                column: "nomenclature_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipt_items_receipt_id",
                table: "receipt_items",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipt_items_storage_location_id",
                table: "receipt_items",
                column: "storage_location_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipt_items_vat_account_id",
                table: "receipt_items",
                column: "vat_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_accounting_date",
                table: "receipts",
                column: "accounting_date");

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_contractor_id",
                table: "receipts",
                column: "contractor_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_credit_account_id",
                table: "receipts",
                column: "credit_account_id");

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_date",
                table: "receipts",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_number",
                table: "receipts",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_receipts_status",
                table: "receipts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "i_x_storage_locations_code",
                table: "storage_locations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_storage_locations_name",
                table: "storage_locations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "i_x_storage_locations_parent_id",
                table: "storage_locations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "i_x_storage_locations_responsible_employee_id",
                table: "storage_locations",
                column: "responsible_employee_id");

            migrationBuilder.CreateIndex(
                name: "i_x_storage_locations_type",
                table: "storage_locations",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "i_x_transaction_bases_name",
                table: "transaction_bases",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_units_of_measure_code",
                table: "units_of_measure",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_units_of_measure_short_name",
                table: "units_of_measure",
                column: "short_name");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "departments",
                column: "head_employee_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_HeadEmployeeId",
                table: "departments");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "bank_statement_documents");

            migrationBuilder.DropTable(
                name: "counterparty_bank_accounts");

            migrationBuilder.DropTable(
                name: "employment_historys");

            migrationBuilder.DropTable(
                name: "receipt_items");

            migrationBuilder.DropTable(
                name: "asset_types");

            migrationBuilder.DropTable(
                name: "bank_statements");

            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "nomenclatures");

            migrationBuilder.DropTable(
                name: "bank_accounts");

            migrationBuilder.DropTable(
                name: "receipts");

            migrationBuilder.DropTable(
                name: "transaction_bases");

            migrationBuilder.DropTable(
                name: "storage_locations");

            migrationBuilder.DropTable(
                name: "units_of_measure");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "counterpartys");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "individuals");

            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
