using GlavnayaKniga.Application.Configuration;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Application.Services;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Infrastructure.Data;
using GlavnayaKniga.Infrastructure.Repositories;
using GlavnayaKniga.WPF.ViewModels;
using GlavnayaKniga.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF
{
    public partial class App : System.Windows.Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Регистрируем провайдера кодировок для поддержки windows-1251
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Конфигурация
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Выбор провайдера базы данных
            var databaseProvider = configuration["DatabaseProvider"];

            // Регистрация DbContext с выбором провайдера
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                if (databaseProvider == "Postgres")
                {
                    var connectionString = configuration.GetConnectionString("PostgresConnection");
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly("GlavnayaKniga.Infrastructure");
                        // Таймаут для команд (по умолчанию 30 сек, увеличиваем для миграций)
                        npgsqlOptions.CommandTimeout(60);
                    });
                }
                else
                {
                    var connectionString = configuration.GetConnectionString("SqliteConnection");
                    options.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly("GlavnayaKniga.Infrastructure");
                    });
                }

                // Включаем логирование для отладки
                #if DEBUG
                options.LogTo(Console.WriteLine, LogLevel.Information);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                #endif
            });
            // Конфигурация для Checko
            services.Configure<CheckoConfig>(configuration.GetSection("CheckoApi"));

            // Репозитории
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // HTTP клиенты
            services.AddHttpClient(); // Это зарегистрирует IHttpClientFactory


            services.AddHttpClient<ICheckoService, CheckoService>();

            // Или регистрируем HttpClient напрямую
            services.AddScoped<HttpClient>(sp =>
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("User-Agent", "GlavnayaKniga");
                return client;
            });


            // Сервисы
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEntryService, EntryService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IWordExportService, WordExportService>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<IBankStatementParser, BankStatementParser>();
            services.AddScoped<IBankStatementService, BankStatementService>();
            services.AddScoped<IBikService, BikService>();
            services.AddScoped<ICounterpartyService, CounterpartyService>();
            services.AddScoped<INomenclatureService, NomenclatureService>();
            services.AddScoped<IAssetTypeService, AssetTypeService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IStorageLocationService, StorageLocationService>();
            services.AddScoped<IReceiptService, ReceiptService>();
            services.AddScoped<IIndividualService, IndividualService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

            // Регистрируем CheckoService вручную с правильными зависимостями
            services.AddScoped<ICheckoService>(serviceProvider =>
            {
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);

                var config = serviceProvider.GetRequiredService<IOptions<CheckoConfig>>();

                // Проверяем, что ключ не пустой
                if (string.IsNullOrEmpty(config.Value.ApiKey))
                {
                    throw new InvalidOperationException("API ключ Checko не найден в конфигурации");
                }

                return new CheckoService(httpClient, config);
            });


            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<AccountsViewModel>();
            services.AddTransient<EntriesViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<BankAccountsViewModel>();
            services.AddTransient<BankStatementImportViewModel>();
            services.AddTransient<BankStatementDetailsViewModel>();
            services.AddTransient<BankStatementEntryViewModel>();
            services.AddTransient<CounterpartiesViewModel>(); 
            services.AddTransient<CounterpartyEditViewModel>();
            services.AddTransient<CounterpartyBankAccountViewModel>();
            services.AddTransient<NomenclatureViewModel>();
            services.AddTransient<NomenclatureEditViewModel>();
            services.AddTransient<AssetTypeViewModel>();
            services.AddTransient<AssetTypeEditViewModel>();
            services.AddTransient<AssetsViewModel>();
            services.AddTransient<AssetEditViewModel>();
            services.AddTransient<StorageLocationsViewModel>();
            services.AddTransient<StorageLocationEditViewModel>();
            services.AddTransient<ReceiptsViewModel>();
            services.AddTransient<ReceiptEditViewModel>();
            services.AddTransient<IndividualsViewModel>();
            services.AddTransient<PositionsViewModel>();
            services.AddTransient<EmployeesViewModel>();
            services.AddTransient<IndividualEditViewModel>();
            services.AddTransient<PositionEditViewModel>();
            services.AddTransient<EmployeeEditViewModel>();
            services.AddTransient<EmployeeDismissViewModel>();
            services.AddTransient<EmployeeTransferViewModel>();
            services.AddTransient<EmploymentHistoryViewModel>();
            services.AddTransient<DepartmentsViewModel>();
            services.AddTransient<DepartmentEditViewModel>();
            services.AddTransient<UnitOfMeasureEditViewModel>();
            services.AddTransient<UnitsOfMeasureViewModel>();
            services.AddTransient<ReceiptViewViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<AccountsView>();
            services.AddTransient<EntriesView>();
            services.AddTransient<ReportsView>();
            services.AddTransient<BankAccountsView>();
            services.AddTransient<BankStatementImportView>();
            services.AddTransient<BankStatementEntryWindow>();
            services.AddTransient<CounterpartiesView>(); 
            services.AddTransient<CounterpartyEditWindow>();
            services.AddTransient<CounterpartyBankAccountWindow>();
            services.AddTransient<NomenclatureView>();
            services.AddTransient<NomenclatureEditWindow>();
            services.AddTransient<AssetTypesView>();
            services.AddTransient<AssetTypesWindow>();
            services.AddTransient<AssetTypeEditWindow>();
            services.AddTransient<AssetsView>();
            services.AddTransient<AssetEditWindow>();
            services.AddTransient<StorageLocationsView>();
            services.AddTransient<StorageLocationEditWindow>();
            services.AddTransient<ReceiptsView>();
            services.AddTransient<ReceiptEditWindow>();
            services.AddTransient<IndividualsView>();
            services.AddTransient<PositionsView>();
            services.AddTransient<EmployeesView>();
            services.AddTransient<IndividualEditWindow>();
            services.AddTransient<PositionEditWindow>();
            services.AddTransient<EmployeeEditWindow>();
            services.AddTransient<EmployeeDismissWindow>();
            services.AddTransient<EmployeeTransferWindow>();
            services.AddTransient<EmploymentHistoryWindow>();
            services.AddTransient<DepartmentsView>();
            services.AddTransient<DepartmentEditWindow>();
            services.AddTransient<UnitOfMeasureEditWindow>();
            services.AddTransient<UnitsOfMeasureView>();
            services.AddTransient<ReceiptViewWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Применяем миграции
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Применяем миграции (создаем базу данных, если её нет)
                await dbContext.Database.MigrateAsync();

                // Заполняем начальными данными
                await DataSeeder.SeedAsync(dbContext);
               

                // Создаем и показываем главное окно
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}\n\n{ex.StackTrace}",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}