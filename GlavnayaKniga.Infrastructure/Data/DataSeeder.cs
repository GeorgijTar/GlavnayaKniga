using DocumentFormat.OpenXml.InkML;
using GlavnayaKniga.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            try
            {
                // Проверяем, есть ли уже данные (работает для обеих БД)
                if (await context.TransactionBases.AnyAsync() || await context.Accounts.AnyAsync())
                {
                    return; // Данные уже есть
                }

                // Добавляем основания проводок
                await SeedTransactionBasesAsync(context);

                // Добавляем стандартные счета
                await SeedAccountsAsync(context);

                // Добавляем единицы измерения
                await SeedUnitsOfMeasureAsync(context);

                await DataSeeder.SeedAssetTypesAsync(context);

                await DataSeeder.SeedStorageLocationsAsync(context);

                //// Добавляем тестовые проводки
                //await SeedTestEntriesAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при заполнении тестовыми данными: {ex.Message}");
                throw;
            }
        }

        // Метод для очистки (опционально)
        public static async Task ClearAllDataAsync(AppDbContext context)
        {
            // Порядок важен: сначала удаляем зависимые записи
            context.ReceiptItems.RemoveRange(await context.ReceiptItems.ToListAsync());
            context.Receipts.RemoveRange(await context.Receipts.ToListAsync());
            context.Entries.RemoveRange(await context.Entries.ToListAsync());
            context.BankStatementDocuments.RemoveRange(await context.BankStatementDocuments.ToListAsync());
            context.BankStatements.RemoveRange(await context.BankStatements.ToListAsync());
            context.EmploymentHistorys.RemoveRange(await context.EmploymentHistorys.ToListAsync());
            context.Employees.RemoveRange(await context.Employees.ToListAsync());
            context.Individuals.RemoveRange(await context.Individuals.ToListAsync());
            context.Positions.RemoveRange(await context.Positions.ToListAsync());
            context.Departments.RemoveRange(await context.Departments.ToListAsync());
            context.Nomenclatures.RemoveRange(await context.Nomenclatures.ToListAsync());
            context.StorageLocations.RemoveRange(await context.StorageLocations.ToListAsync());
            context.Assets.RemoveRange(await context.Assets.ToListAsync());
            context.AssetTypes.RemoveRange(await context.AssetTypes.ToListAsync());
            context.CounterpartyBankAccounts.RemoveRange(await context.CounterpartyBankAccounts.ToListAsync());
            context.Counterpartys.RemoveRange(await context.Counterpartys.ToListAsync());
            context.BankAccounts.RemoveRange(await context.BankAccounts.ToListAsync());
            context.Accounts.RemoveRange(await context.Accounts.ToListAsync());
            context.TransactionBases.RemoveRange(await context.TransactionBases.ToListAsync());
            context.UnitsOfMeasure.RemoveRange(await context.UnitsOfMeasure.ToListAsync());

            await context.SaveChangesAsync();
        }



        private static async Task SeedTransactionBasesAsync(AppDbContext context)
        {
            var bases = new[]
            {
                new TransactionBasis { Name = "Журнал-ордер № 1" },
                new TransactionBasis { Name = "Журнал-ордер № 2" },
                new TransactionBasis { Name = "Журнал-ордер № 6" },
                new TransactionBasis { Name = "Журнал-ордер № 7" },
                new TransactionBasis { Name = "Журнал-ордер № 8" },
                new TransactionBasis { Name = "Журнал-ордер № 10" },
                new TransactionBasis { Name = "Журнал-ордер № 11" },
                new TransactionBasis { Name = "Журнал-ордер № 13" },
                new TransactionBasis { Name = "Бухгалтерская справка" }
            };

            await context.TransactionBases.AddRangeAsync(bases);
            await context.SaveChangesAsync();
        }

        private static async Task SeedAccountsAsync(AppDbContext context)
        {
            var accounts = new List<Account>();

            // Раздел I. Внеоборотные активы
            accounts.Add(new Account { Code = "01", Name = "Основные средства", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "02", Name = "Амортизация основных средств", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "03", Name = "Доходные вложения в материальные ценности", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "04", Name = "Нематериальные активы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "05", Name = "Амортизация нематериальных активов", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "07", Name = "Оборудование к установке", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "08", Name = "Вложения во внеоборотные активы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "09", Name = "Отложенные налоговые активы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел II. Производственные запасы
            accounts.Add(new Account { Code = "10", Name = "Материалы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "11", Name = "Животные на выращивании и откорме", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "14", Name = "Резервы под снижение стоимости материальных ценностей", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "15", Name = "Заготовление и приобретение материальных ценностей", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "16", Name = "Отклонение в стоимости материальных ценностей", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "19", Name = "НДС по приобретенным ценностям", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел III. Затраты на производство
            accounts.Add(new Account { Code = "20", Name = "Основное производство", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "21", Name = "Полуфабрикаты собственного производства", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "23", Name = "Вспомогательные производства", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "25", Name = "Общепроизводственные расходы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "26", Name = "Общехозяйственные расходы", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "28", Name = "Брак в производстве", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "29", Name = "Обслуживающие производства и хозяйства", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел IV. Готовая продукция и товары
            accounts.Add(new Account { Code = "40", Name = "Выпуск продукции (работ, услуг)", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "41", Name = "Товары", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "42", Name = "Торговая наценка", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "43", Name = "Готовая продукция", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "44", Name = "Расходы на продажу", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "45", Name = "Товары отгруженные", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "46", Name = "Выполненные этапы по незавершенным работам", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел V. Денежные средства
            accounts.Add(new Account { Code = "50", Name = "Касса", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "51", Name = "Расчетные счета", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "52", Name = "Валютные счета", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "55", Name = "Специальные счета в банках", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "57", Name = "Переводы в пути", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "58", Name = "Финансовые вложения", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "59", Name = "Резервы под обесценение финансовых вложений", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел VI. Расчеты
            accounts.Add(new Account { Code = "60", Name = "Расчеты с поставщиками и подрядчиками", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "62", Name = "Расчеты с покупателями и заказчиками", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "63", Name = "Резервы по сомнительным долгам", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "66", Name = "Расчеты по краткосрочным кредитам и займам", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "67", Name = "Расчеты по долгосрочным кредитам и займам", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "68", Name = "Расчеты по налогам и сборам", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "69", Name = "Расчеты по социальному страхованию и обеспечению", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "70", Name = "Расчеты с персоналом по оплате труда", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "71", Name = "Расчеты с подотчетными лицами", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "73", Name = "Расчеты с персоналом по прочим операциям", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "75", Name = "Расчеты с учредителями", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "76", Name = "Расчеты с разными дебиторами и кредиторами", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "77", Name = "Отложенные налоговые обязательства", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "79", Name = "Внутрихозяйственные расчеты", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел VII. Капитал
            accounts.Add(new Account { Code = "80", Name = "Уставный капитал", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "81", Name = "Собственные акции (доли)", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "82", Name = "Резервный капитал", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "83", Name = "Добавочный капитал", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "84", Name = "Нераспределенная прибыль (непокрытый убыток)", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "86", Name = "Целевое финансирование", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            // Раздел VIII. Финансовые результаты
            accounts.Add(new Account { Code = "90", Name = "Продажи", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "91", Name = "Прочие доходы и расходы", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "94", Name = "Недостачи и потери от порчи ценностей", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "96", Name = "Резервы предстоящих расходов", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "97", Name = "Расходы будущих периодов", Type = AccountType.Active, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "98", Name = "Доходы будущих периодов", Type = AccountType.Passive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });
            accounts.Add(new Account { Code = "99", Name = "Прибыли и убытки", Type = AccountType.ActivePassive, IsSynthetic = true, CreatedAt = DateTime.UtcNow });

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();

            // Добавляем тестовые субсчета
            await SeedTestSubaccountsAsync(context);
        }

        private static async Task SeedTestSubaccountsAsync(AppDbContext context)
        {
            var accounts = await context.Accounts.ToListAsync();

            // Субсчета для счета 10 (Материалы)
            var account10 = accounts.First(a => a.Code == "10");

            var subaccounts10 = new[]
            {
                new Account { Code = "10.1", Name = "Сырье и материалы", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.2", Name = "Покупные полуфабрикаты и комплектующие", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.3", Name = "Топливо", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.4", Name = "Тара и тарные материалы", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.5", Name = "Запасные части", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.6", Name = "Прочие материалы", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "10.9", Name = "Инвентарь и хозяйственные принадлежности", Type = AccountType.Active, ParentId = account10.Id, CreatedAt = DateTime.UtcNow }
            };

            await context.Accounts.AddRangeAsync(subaccounts10);
            await context.SaveChangesAsync();

            // Субсчета для счета 60 (Расчеты с поставщиками)
            var account60 = accounts.First(a => a.Code == "60");

            var subaccounts60 = new[]
            {
                new Account { Code = "60.1", Name = "Расчеты с поставщиками", Type = AccountType.ActivePassive, ParentId = account60.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "60.2", Name = "Расчеты по авансам выданным", Type = AccountType.Active, ParentId = account60.Id, CreatedAt = DateTime.UtcNow }
            };

            await context.Accounts.AddRangeAsync(subaccounts60);
            await context.SaveChangesAsync();

            // Субсчета для счета 51 (Расчетные счета)
            var account51 = accounts.First(a => a.Code == "51");

            var subaccounts51 = new[]
            {
                new Account { Code = "51.1", Name = "Расчетный счет в ВТБ", Type = AccountType.Active, ParentId = account51.Id, CreatedAt = DateTime.UtcNow },
                new Account { Code = "51.2", Name = "Расчетный счет в РСХБ", Type = AccountType.Active, ParentId = account51.Id, CreatedAt = DateTime.UtcNow }
            };

            await context.Accounts.AddRangeAsync(subaccounts51);
            await context.SaveChangesAsync();

            // Обновляем FullCode для всех счетов
            await UpdateFullCodesAsync(context);
        }

        private static async Task UpdateFullCodesAsync(AppDbContext context)
        {
            var accounts = await context.Accounts.ToListAsync();

            foreach (var account in accounts)
            {
                if (account.ParentId.HasValue)
                {
                    var parent = accounts.FirstOrDefault(a => a.Id == account.ParentId.Value);
                    if (parent != null)
                    {
                        account.FullCode = $"{parent.FullCode ?? parent.Code}.{account.Code}";
                    }
                }
                else
                {
                    account.FullCode = account.Code;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedTestEntriesAsync(AppDbContext context)
        {
            var accounts = await context.Accounts.ToListAsync();
            var bases = await context.TransactionBases.ToListAsync();
            var basis = bases.First();

            var account51_2 = accounts.First(a => a.Code == "51.2");
            var account60_1 = accounts.First(a => a.Code == "60.1");
            var account60_2 = accounts.First(a => a.Code == "60.2");
            var account10_1 = accounts.First(a => a.Code == "10.1");
            var account10_5 = accounts.First(a => a.Code == "10.5");

            // Тестовые проводки из примера
            var entries = new[]
            {
                new Entry
                {
                    Date = new DateTime(2026, 3, 1),
                    DebitAccountId = account60_2.Id,
                    CreditAccountId = account51_2.Id,
                    Amount = 52000.00m,
                    Basis = basis,
                    Note = "Предоплата контрагенту",
                    CreatedAt = DateTime.UtcNow
                },
                new Entry
                {
                    Date = new DateTime(2026, 3, 1),
                    DebitAccountId = account60_1.Id,
                    CreditAccountId = account60_2.Id,
                    Amount = 50000.00m,
                    Basis = basis,
                    Note = "Зачет аванса на сумму поступивших материалов",
                    CreatedAt = DateTime.UtcNow
                },
                new Entry
                {
                    Date = new DateTime(2026, 3, 1),
                    DebitAccountId = account10_1.Id,
                    CreditAccountId = account60_1.Id,
                    Amount = 50000.00m,
                    Basis = basis,
                    Note = "Поступление материалов от контрагента",
                    CreatedAt = DateTime.UtcNow
                },
                new Entry
                {
                    Date = new DateTime(2026, 3, 2),
                    DebitAccountId = account51_2.Id,
                    CreditAccountId = account60_2.Id,
                    Amount = 2000.00m,
                    Basis = basis,
                    Note = "Возврат от контрагента излишне уплаченной суммы",
                    CreatedAt = DateTime.UtcNow
                },
                new Entry
                {
                    Date = new DateTime(2026, 3, 3),
                    DebitAccountId = account10_5.Id,
                    CreditAccountId = account60_1.Id,
                    Amount = 6000.00m,
                    Basis = basis,
                    Note = "Поступили материалы от поставщика 2",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Entries.AddRangeAsync(entries);
            await context.SaveChangesAsync();
        }


        /// <summary>
        /// Заполнение начальных типов объектов
        /// </summary>
        public static async Task SeedAssetTypesAsync(AppDbContext context)
        {
            if (!await context.AssetTypes.AnyAsync())
            {
                var assetTypes = new[]
                {
                    new AssetType { Name = "Трактора", Description = "Колесные и гусеничные трактора", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Комбайны", Description = "Зерноуборочные, кормоуборочные комбайны", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Сеялки", Description = "Сеялки зерновые, пропашные", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Плуги", Description = "Плуги лемешные, оборотные", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Дискаторы", Description = "Дисковые бороны, лущильники", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Щелерезы", Description = "Глубокорыхлители, щелеватели", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Культиваторы", Description = "Культиваторы сплошной обработки, пропашные", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Опрыскиватели", Description = "Опрыскиватели полевые", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Разбрасыватели", Description = "Разбрасыватели удобрений", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Автомобили", Description = "Грузовые и легковые автомобили", CreatedAt = DateTime.UtcNow },
                    new AssetType { Name = "Прочее оборудование", Description = "Прочее сельскохозяйственное оборудование", CreatedAt = DateTime.UtcNow }
                };

                await context.AssetTypes.AddRangeAsync(assetTypes);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Заполнение начальных мест хранения
        /// </summary>
        public static async Task SeedStorageLocationsAsync(AppDbContext context)
        {
            if (!await context.StorageLocations.AnyAsync())
            {
                // Создаем склады
                var warehouse1 = new StorageLocation
                {
                    Code = "СКЛ-001",
                    Name = "Центральный склад",
                    Type = StorageLocationType.Warehouse,
                    Description = "Основной склад готовой продукции",
                    Address = "ул. Ленина, д. 1",
                    Area = 1000,
                    Capacity = 5000,
                    TemperatureRegime = "Отапливаемый",
                    CreatedAt = DateTime.UtcNow
                };

                var warehouse2 = new StorageLocation
                {
                    Code = "СКЛ-002",
                    Name = "Склад ГСМ",
                    Type = StorageLocationType.Warehouse,
                    Description = "Склад горюче-смазочных материалов",
                    Address = "ул. Промышленная, д. 5",
                    Area = 500,
                    Capacity = 2000,
                    TemperatureRegime = "Неотапливаемый",
                    CreatedAt = DateTime.UtcNow
                };

                var outdoor = new StorageLocation
                {
                    Code = "ПЛ-001",
                    Name = "Открытая площадка",
                    Type = StorageLocationType.Outdoor,
                    Description = "Площадка для хранения техники",
                    Address = "ул. Полевая, д. 10",
                    Area = 5000,
                    CreatedAt = DateTime.UtcNow
                };

                await context.StorageLocations.AddRangeAsync(warehouse1, warehouse2, outdoor);
                await context.SaveChangesAsync();

                // Создаем участки и ячейки внутри центрального склада
                var section1 = new StorageLocation
                {
                    Code = "СКЛ-001-01",
                    Name = "Участок А",
                    Type = StorageLocationType.Section,
                    ParentId = warehouse1.Id,
                    Description = "Участок хранения зерна",
                    Area = 400,
                    Capacity = 2000,
                    TemperatureRegime = "Отапливаемый",
                    CreatedAt = DateTime.UtcNow
                };

                var section2 = new StorageLocation
                {
                    Code = "СКЛ-001-02",
                    Name = "Участок Б",
                    Type = StorageLocationType.Section,
                    ParentId = warehouse1.Id,
                    Description = "Участок хранения удобрений",
                    Area = 300,
                    Capacity = 1500,
                    TemperatureRegime = "Отапливаемый",
                    CreatedAt = DateTime.UtcNow
                };

                var rack1 = new StorageLocation
                {
                    Code = "СКЛ-001-01-СТ-01",
                    Name = "Стеллаж №1",
                    Type = StorageLocationType.Rack,
                    ParentId = section1.Id,
                    Description = "Стеллаж для мешков",
                    Capacity = 500,
                    CreatedAt = DateTime.UtcNow
                };

                await context.StorageLocations.AddRangeAsync(section1, section2, rack1);
                try
                {
                     await context.SaveChangesAsync();
                }
                catch (Exception ex) 
                { return; }

             

                // Создаем ячейки в стеллаже
                for (int i = 1; i <= 10; i++)
                {
                    var cell = new StorageLocation
                    {
                        Code = $"СКЛ-001-01-СТ-01-ЯЧ-{i:D2}",
                        Name = $"Ячейка {i}",
                        Type = StorageLocationType.Cell,
                        ParentId = rack1.Id,
                        Description = $"Ячейка для хранения, ряд {i}",
                        Capacity = 50,
                        CreatedAt = DateTime.UtcNow
                    };
                    await context.StorageLocations.AddAsync(cell);
                }

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Заполнение единиц измерения по ОКЕИ
        /// </summary>
        private static async Task SeedUnitsOfMeasureAsync(AppDbContext context)
        {
            if (!await context.UnitsOfMeasure.AnyAsync())
            {
                var units = new[]
                {
                    new UnitOfMeasure { Code = "796", ShortName = "шт", FullName = "Штука", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "166", ShortName = "кг", FullName = "Килограмм", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "163", ShortName = "г", FullName = "Грамм", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "168", ShortName = "т", FullName = "Тонна", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "112", ShortName = "л", FullName = "Литр", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "006", ShortName = "м", FullName = "Метр", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "055", ShortName = "м²", FullName = "Квадратный метр", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "113", ShortName = "м³", FullName = "Кубический метр", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "778", ShortName = "уп", FullName = "Упаковка", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "839", ShortName = "компл", FullName = "Комплект", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "356", ShortName = "ч", FullName = "Час", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "359", ShortName = "дн", FullName = "День", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "362", ShortName = "мес", FullName = "Месяц", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "876", ShortName = "усл", FullName = "Условная единица", CreatedAt = DateTime.UtcNow },
                    new UnitOfMeasure { Code = "000", ShortName = "др", FullName = "Другое", CreatedAt = DateTime.UtcNow }
                };

                await context.UnitsOfMeasure.AddRangeAsync(units);
                await context.SaveChangesAsync();
            }
        }
    }
}