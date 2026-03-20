using Microsoft.EntityFrameworkCore;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Infrastructure.Configurations;
using System.Text;

namespace GlavnayaKniga.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet остаются без изменений
        public DbSet<Account> Accounts { get; set; }
        public DbSet<TransactionBasis> TransactionBases { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankStatement> BankStatements { get; set; }
        public DbSet<BankStatementDocument> BankStatementDocuments { get; set; }
        public DbSet<Counterparty> Counterpartys { get; set; }
        public DbSet<CounterpartyBankAccount> CounterpartyBankAccounts { get; set; }
        public DbSet<Nomenclature> Nomenclatures { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<StorageLocation> StorageLocations { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }
        public DbSet<Individual> Individuals { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmploymentHistory> EmploymentHistorys { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Применяем все конфигурации (остаются без изменений)
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new EntryConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionBasisConfiguration());
            modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new BankStatementConfiguration());
            modelBuilder.ApplyConfiguration(new BankStatementDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new CounterpartyConfiguration());
            modelBuilder.ApplyConfiguration(new CounterpartyBankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new NomenclatureConfiguration());
            modelBuilder.ApplyConfiguration(new AssetTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AssetConfiguration());
            modelBuilder.ApplyConfiguration(new StorageLocationConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
            modelBuilder.ApplyConfiguration(new ReceiptItemConfiguration());
            modelBuilder.ApplyConfiguration(new IndividualConfiguration());
            modelBuilder.ApplyConfiguration(new PositionConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
            modelBuilder.ApplyConfiguration(new UnitOfMeasureConfiguration());

            // Добавляем специфичные настройки для PostgreSQL
            if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                // Настройка для PostgreSQL: использование timestamp with time zone
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                    foreach (var property in properties)
                    {
                        property.SetColumnType("timestamp with time zone");
                    }
                }
            }
            // Для PostgreSQL: устанавливаем snake_case именование
            if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                // Конвертируем все имена в snake_case
                foreach (var entity in modelBuilder.Model.GetEntityTypes())
                {
                    // Таблицы
                    entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

                    // Колонки
                    foreach (var property in entity.GetProperties())
                    {
                        property.SetColumnName(property.GetColumnName().ToSnakeCase());
                    }

                    // Индексы
                    foreach (var index in entity.GetIndexes())
                    {
                        index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}