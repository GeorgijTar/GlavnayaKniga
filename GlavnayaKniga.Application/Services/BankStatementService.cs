using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class BankStatementService : IBankStatementService
    {
        private readonly IRepository<BankStatement> _statementRepository;
        private readonly IRepository<BankStatementDocument> _documentRepository;
        private readonly IRepository<Entry> _entryRepository;
        private readonly IBankStatementParser _parser;
        private readonly IBankAccountService _bankAccountService;
        private readonly IAccountService _accountService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly ICheckoService _checkoService;
        private readonly IBikService _bikService;

        public BankStatementService(
            IRepository<BankStatement> statementRepository,
            IRepository<BankStatementDocument> documentRepository,
            IRepository<Entry> entryRepository,
            IBankStatementParser parser,
            IBankAccountService bankAccountService,
            IAccountService accountService,
            ICounterpartyService counterpartyService,
            ICheckoService checkoService,
            IBikService bikService) 
        {
            _statementRepository = statementRepository;
            _documentRepository = documentRepository;
            _entryRepository = entryRepository;
            _parser = parser;
            _bankAccountService = bankAccountService;
            _accountService = accountService;
            _counterpartyService = counterpartyService;
            _checkoService = checkoService;
            _bikService = bikService; 
        }

        public async Task<BankStatementImportResult> ImportStatementAsync(string filePath)
        {
            var result = new BankStatementImportResult();

            try
            {
                // Парсим файл
                var parseResult = await _parser.ParseFileAsync(filePath);
                if (!parseResult.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = parseResult.ErrorMessage;
                    return result;
                }

                // Проверяем дубликат
                var isDuplicate = await IsDuplicateStatementAsync(
                    parseResult.AccountNumber,
                    parseResult.StartDate,
                    parseResult.EndDate);

                if (isDuplicate)
                {
                    result.Success = false;
                    result.ErrorMessage = "Выписка за данный период уже импортирована";
                    return result;
                }

                // Ищем соответствующий банковский счет
                var bankAccount = await _bankAccountService.FindBankAccountByNumberAsync(parseResult.AccountNumber);

                // Создаем запись о выписке
                var statement = new BankStatement
                {
                    FileName = parseResult.FileName,
                    StartDate = parseResult.StartDate,
                    EndDate = parseResult.EndDate,
                    AccountNumber = parseResult.AccountNumber,
                    BankAccountId = bankAccount?.Id,
                    ImportedAt = DateTime.UtcNow,
                    ImportedBy = Environment.UserName,
                    Status = StatementImportStatus.New
                };

                var createdStatement = await _statementRepository.AddAsync(statement);

                // Сохраняем документы и ищем контрагентов
                int createdCounterparties = 0;
                int createdBankAccounts = 0;
                var warnings = new List<string>();

                foreach (var docDto in parseResult.Documents)
                {
                    try
                    {
                        // Ищем или создаем контрагента-плательщика
                        int? payerId = null;
                        if (!string.IsNullOrWhiteSpace(docDto.PayerINN))
                        {
                            var (counterpartyId, isNewCounterparty, isNewBankAccount) = await FindOrCreateCounterpartyWithBankAccountAsync(
                                docDto.PayerINN,
                                docDto.PayerName ?? "",
                                docDto.PayerAccount,
                                docDto.PayerBIK,
                                true);

                            payerId = counterpartyId;
                            if (isNewCounterparty) createdCounterparties++;
                            if (isNewBankAccount) createdBankAccounts++;
                        }

                        // Ищем или создаем контрагента-получателя
                        int? recipientId = null;
                        if (!string.IsNullOrWhiteSpace(docDto.RecipientINN))
                        {
                            var (counterpartyId, isNewCounterparty, isNewBankAccount) = await FindOrCreateCounterpartyWithBankAccountAsync(
                                docDto.RecipientINN,
                                docDto.RecipientName ?? "",
                                docDto.RecipientAccount,
                                docDto.RecipientBIK,
                                false);

                            recipientId = counterpartyId;
                            if (isNewCounterparty) createdCounterparties++;
                            if (isNewBankAccount) createdBankAccounts++;
                        }

                        var document = new BankStatementDocument
                        {
                            BankStatementId = createdStatement.Id,
                            DocumentType = docDto.DocumentType,
                            Number = docDto.Number,
                            Date = docDto.Date,
                            Amount = docDto.Amount,
                            PayerAccount = docDto.PayerAccount,
                            PayerINN = docDto.PayerINN,
                            PayerName = docDto.PayerName,
                            PayerBIK = docDto.PayerBIK,
                            RecipientAccount = docDto.RecipientAccount,
                            RecipientINN = docDto.RecipientINN,
                            RecipientName = docDto.RecipientName,
                            RecipientBIK = docDto.RecipientBIK,
                            PaymentPurpose = docDto.PaymentPurpose,
                            PaymentType = docDto.PaymentType,
                            Priority = docDto.Priority,
                            ReceivedDate = docDto.ReceivedDate,
                            WithdrawnDate = docDto.WithdrawnDate,
                            PayerCounterpartyId = payerId,
                            RecipientCounterpartyId = recipientId,
                            Hash = docDto.Hash
                        };

                        await _documentRepository.AddAsync(document);
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"Ошибка обработки документа {docDto.Number} от {docDto.Date:d}: {ex.Message}");
                    }
                }

                result.Success = true;
                result.StatementId = createdStatement.Id;
                result.DocumentsCount = parseResult.Documents.Count;
                result.AccountNumber = parseResult.AccountNumber;
                result.StartDate = parseResult.StartDate;
                result.EndDate = parseResult.EndDate;
                result.BankAccountFound = bankAccount != null;
                result.BankAccountId = bankAccount?.Id;
                result.CreatedCounterpartiesCount = createdCounterparties;
                result.CreatedBankAccountsCount = createdBankAccounts;
                result.Warnings = warnings;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Ошибка импорта: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Найти или создать контрагента и его банковский счет
        /// </summary>
        private async Task<(int? CounterpartyId, bool IsNewCounterparty, bool IsNewBankAccount)> FindOrCreateCounterpartyWithBankAccountAsync(
            string inn,
            string name,
            string accountNumber,
            string? bik,
            bool isPayer)
        {
            bool isNewCounterparty = false;
            bool isNewBankAccount = false;

            try
            {
                // Проверяем, есть ли уже контрагент с таким ИНН
                var existingCounterparty = await _counterpartyService.GetCounterpartyByINNAsync(inn);

                if (existingCounterparty != null)
                {
                    Debug.WriteLine($"Найден существующий контрагент: {existingCounterparty.ShortName} (ИНН: {inn})");

                    // Проверяем, есть ли у контрагента такой банковский счет
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        var existingAccount = await _counterpartyService.GetBankAccountByNumberAsync(accountNumber, existingCounterparty.Id);

                        if (existingAccount == null)
                        {
                            // Создаем банковский счет для контрагента
                            await CreateBankAccountForCounterparty(existingCounterparty.Id, accountNumber, name, bik);
                            isNewBankAccount = true;
                            Debug.WriteLine($"Создан новый банковский счет для контрагента {existingCounterparty.ShortName}: {accountNumber}");
                        }
                    }

                    return (existingCounterparty.Id, false, isNewBankAccount);
                }

                // Создаем нового контрагента
                CounterpartyDto? newCounterparty = null;

                // Пробуем получить данные из Checko
                var checkoData = await _checkoService.GetCounterpartyDataAsync(inn);

                if (checkoData != null)
                {
                    var newCounterpartyDto = new CounterpartyDto { INN = inn };

                    if (checkoData is CheckoCompanyData company)
                    {
                        newCounterpartyDto.FullName = company.FullName ?? name;
                        newCounterpartyDto.ShortName = company.ShortName ?? (name.Length > 200 ? name.Substring(0, 200) : name);
                        newCounterpartyDto.KPP = company.KPP;
                        newCounterpartyDto.OGRN = company.OGRN;
                        newCounterpartyDto.LegalAddress = company.LegalAddress?.Address;
                        newCounterpartyDto.Type = "Юридическое лицо";
                    }
                    else if (checkoData is CheckoEntrepreneurData entrepreneur)
                    {
                        newCounterpartyDto.FullName = entrepreneur.FullName ?? name;
                        newCounterpartyDto.ShortName = entrepreneur.FullName ?? (name.Length > 200 ? name.Substring(0, 200) : name);
                        newCounterpartyDto.OGRN = entrepreneur.OGRNIP;
                        newCounterpartyDto.Type = "Индивидуальный предприниматель";
                    }

                    newCounterparty = await _counterpartyService.CreateCounterpartyAsync(newCounterpartyDto);
                    Debug.WriteLine($"Создан новый контрагент из Checko: {newCounterparty.ShortName} (ИНН: {inn})");
                    isNewCounterparty = true;
                }
                else
                {
                    // Создаем минимального контрагента
                    var shortName = name.Length > 200 ? name.Substring(0, 200) : name;

                    var newCounterpartyDto = new CounterpartyDto
                    {
                        INN = inn,
                        FullName = name,
                        ShortName = shortName,
                        Type = "Юридическое лицо"
                    };

                    newCounterparty = await _counterpartyService.CreateCounterpartyAsync(newCounterpartyDto);
                    Debug.WriteLine($"Создан новый контрагент (минимальные данные): {newCounterparty.ShortName} (ИНН: {inn})");
                    isNewCounterparty = true;
                }

                // Создаем банковский счет для нового контрагента
                if (newCounterparty != null && !string.IsNullOrWhiteSpace(accountNumber))
                {
                    await CreateBankAccountForCounterparty(newCounterparty.Id, accountNumber, name, bik);
                    isNewBankAccount = true;
                }

                return (newCounterparty?.Id, isNewCounterparty, isNewBankAccount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании контрагента и банковского счета: {ex.Message}");
                return (null, false, false);
            }
        }

        /// <summary>
        /// Создает банковский счет для контрагента
        /// </summary>
        private async Task CreateBankAccountForCounterparty(int counterpartyId, string accountNumber, string bankName, string? bik)
        {
            try
            {
                // Получаем информацию о банке по БИК, если он указан
                string? correspondentAccount = null;
                string? fullBankName = bankName;

                if (!string.IsNullOrWhiteSpace(bik))
                {
                    var bankInfo = await _bikService.GetBankInfoByBikAsync(bik);
                    if (bankInfo != null)
                    {
                        fullBankName = bankInfo.Name;
                        correspondentAccount = bankInfo.CorrespondentAccount;
                    }
                }

                var bankAccountDto = new CounterpartyBankAccountDto
                {
                    CounterpartyId = counterpartyId,
                    AccountNumber = accountNumber,
                    BankName = fullBankName ?? bankName,
                    BIK = bik ?? string.Empty,
                    CorrespondentAccount = correspondentAccount,
                    Currency = "RUB",
                    IsDefault = false, // Не делаем основным по умолчанию
                    Note = $"Автоматически создан при импорте банковской выписки"
                };

                await _counterpartyService.AddBankAccountAsync(bankAccountDto);
                Debug.WriteLine($"Создан банковский счет {accountNumber} для контрагента ID={counterpartyId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании банковского счета: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Найти или создать контрагента по ИНН и наименованию
        /// </summary>
        public async Task<int?> FindOrCreateCounterpartyAsync(string inn, string name, bool isPayer)
        {
            try
            {
                // Проверяем, есть ли уже контрагент с таким ИНН
                var existingCounterparty = await _counterpartyService.GetCounterpartyByINNAsync(inn);
                if (existingCounterparty != null)
                {
                    Debug.WriteLine($"Найден существующий контрагент: {existingCounterparty.ShortName} (ИНН: {inn})");

                    // Проверяем и добавляем банковский счет, если его нет
                    await EnsureBankAccountForCounterparty(existingCounterparty, inn, name, isPayer);

                    return existingCounterparty.Id;
                }

                // Пробуем получить данные из Checko
                var checkoData = await _checkoService.GetCounterpartyDataAsync(inn);
                CounterpartyDto? createdCounterparty = null;

                if (checkoData != null)
                {
                    // Создаем контрагента из данных Checko
                    var newCounterparty = new CounterpartyDto
                    {
                        INN = inn
                    };

                    if (checkoData is CheckoCompanyData company)
                    {
                        newCounterparty.FullName = company.FullName ?? name;
                        newCounterparty.ShortName = company.ShortName ?? (name.Length > 200 ? name.Substring(0, 200) : name);
                        newCounterparty.KPP = company.KPP;
                        newCounterparty.OGRN = company.OGRN;
                        newCounterparty.LegalAddress = company.LegalAddress?.Address;
                        newCounterparty.Type = "Юридическое лицо";
                    }
                    else if (checkoData is CheckoEntrepreneurData entrepreneur)
                    {
                        newCounterparty.FullName = entrepreneur.FullName ?? name;
                        newCounterparty.ShortName = entrepreneur.FullName ?? (name.Length > 200 ? name.Substring(0, 200) : name);
                        newCounterparty.OGRN = entrepreneur.OGRNIP;
                        newCounterparty.Type = "Индивидуальный предприниматель";
                    }

                    createdCounterparty = await _counterpartyService.CreateCounterpartyAsync(newCounterparty);
                    Debug.WriteLine($"Создан новый контрагент из Checko: {createdCounterparty.ShortName} (ИНН: {inn})");
                }
                else
                {
                    // Создаем минимального контрагента только с ИНН и наименованием
                    var shortName = name.Length > 200 ? name.Substring(0, 200) : name;

                    var newCounterparty = new CounterpartyDto
                    {
                        INN = inn,
                        FullName = name,
                        ShortName = shortName,
                        Type = "Юридическое лицо" // По умолчанию
                    };

                    createdCounterparty = await _counterpartyService.CreateCounterpartyAsync(newCounterparty);
                    Debug.WriteLine($"Создан новый контрагент (минимальные данные): {createdCounterparty.ShortName} (ИНН: {inn})");
                }

                // Добавляем банковский счет для нового контрагента
                if (createdCounterparty != null)
                {
                    await CreateBankAccountForCounterparty(createdCounterparty.Id, inn, name, isPayer);
                }

                return createdCounterparty?.Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании контрагента: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Проверяет наличие банковского счета у контрагента и создает его при необходимости
        /// </summary>
        private async Task EnsureBankAccountForCounterparty(CounterpartyDto counterparty, string inn, string name, bool isPayer)
        {
            try
            {
                // Определяем номер счета в зависимости от того, плательщик это или получатель
                // В реальном сценарии номер счета нужно получать из документа выписки
                // Этот метод будет вызываться из контекста, где доступен документ

                // Здесь мы не можем создать счет, так как у нас нет номера счета
                // Этот метод будет переопределен в вызывающем коде
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при проверке банковского счета: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает банковский счет для контрагента на основе данных из выписки
        /// </summary>
        private async Task CreateBankAccountForCounterparty(int counterpartyId, string inn, string name, bool isPayer)
        {
            // Этот метод будет вызываться из основного цикла импорта с конкретными данными документа
            // Реализация будет в методе ImportStatementAsync
        }

        public async Task UpdateDocumentCounterpartyAsync(int documentId, int counterpartyId, bool isPayer)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null) return;

            if (isPayer)
            {
                document.PayerCounterpartyId = counterpartyId;
            }
            else
            {
                document.RecipientCounterpartyId = counterpartyId;
            }

            await _documentRepository.UpdateAsync(document);
        }

        public async Task<IEnumerable<BankStatementDto>> GetAllStatementsAsync()
        {
            var statements = await _statementRepository.GetAllAsync();
            var result = new List<BankStatementDto>();

            foreach (var statement in statements.OrderByDescending(s => s.ImportedAt))
            {
                var dto = await MapToDto(statement);
                result.Add(dto);
            }

            return result;
        }

        public async Task<BankStatementDto?> GetStatementByIdAsync(int id)
        {
            var statement = await _statementRepository.GetByIdAsync(id);
            return statement != null ? await MapToDto(statement) : null;
        }

        public async Task<EntriesGenerationResult> CreateEntriesFromStatementAsync(int statementId, int defaultBasisId)
        {
            var result = new EntriesGenerationResult
            {
                Success = true,
                Warnings = new List<string>()
            };

            try
            {
                var statement = await _statementRepository.GetByIdAsync(statementId);
                if (statement == null)
                {
                    throw new InvalidOperationException($"Выписка с ID {statementId} не найдена");
                }

                if (statement.BankAccountId == null)
                {
                    throw new InvalidOperationException("Для выписки не найден соответствующий банковский счет");
                }

                var documents = await _documentRepository.FindAsync(d => d.BankStatementId == statementId);
                var documentList = documents.ToList();

                foreach (var document in documentList)
                {
                    try
                    {
                        // Пропускаем уже импортированные
                        if (document.EntryId.HasValue) continue;

                        // Создаем проводку
                        var entry = await CreateEntryFromDocument(document, statement.BankAccountId.Value, defaultBasisId);

                        document.EntryId = entry.Id;
                        await _documentRepository.UpdateAsync(document);

                        result.EntriesCreated++;
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Документ {document.Number} от {document.Date:d}: {ex.Message}");
                    }

                    result.DocumentsProcessed++;
                }

                // Обновляем статус выписки после создания проводок
                await UpdateStatementStatusAsync(statementId);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<bool> DeleteStatementAsync(int id)
        {
            var statement = await _statementRepository.GetByIdAsync(id);
            if (statement == null) return false;

            // Удаляем связанные документы (каскадно)
            await _statementRepository.DeleteAsync(statement);
            return true;
        }

        public async Task<bool> IsDuplicateStatementAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var statements = await _statementRepository.FindAsync(s =>
                s.AccountNumber == accountNumber &&
                s.StartDate == startDate &&
                s.EndDate == endDate);

            return statements.Any();
        }

        private async Task<Entry> CreateEntryFromDocument(
            BankStatementDocument document,
            int bankAccountId,
            int defaultBasisId)
        {
            var bankAccount = await _bankAccountService.GetBankAccountByIdAsync(bankAccountId);
            if (bankAccount == null)
            {
                throw new InvalidOperationException("Банковский счет не найден");
            }

            // Определяем счета дебета и кредита в зависимости от направления
            int debitAccountId, creditAccountId;

            if (document.IsIncoming)
            {
                // Входящий платеж: Дт банковский счет, Кт ? (нужно определять по контрагенту)
                debitAccountId = bankAccount.SubaccountId;
                creditAccountId = await DetermineCounterpartyAccount(document, true);
            }
            else
            {
                // Исходящий платеж: Дт ?, Кт банковский счет
                debitAccountId = await DetermineCounterpartyAccount(document, false);
                creditAccountId = bankAccount.SubaccountId;
            }

            var entry = new Entry
            {
                Date = document.Date,
                DebitAccountId = debitAccountId,
                CreditAccountId = creditAccountId,
                Amount = document.Amount,
                BasisId = defaultBasisId,
                Note = $"Банковская выписка: {document.DocumentType} №{document.Number} от {document.Date:d}. {document.PaymentPurpose}",
                CreatedAt = DateTime.UtcNow
            };

            return await _entryRepository.AddAsync(entry);
        }

        private async Task<int> DetermineCounterpartyAccount(BankStatementDocument document, bool isIncoming)
        {
            // TODO: Реализовать логику определения счета контрагента
            // По умолчанию используем счет 60 (Расчеты с поставщиками)
            var account = await _accountService.GetAccountByCodeAsync("60");
            if (account != null)
            {
                return account.Id;
            }

            // Если счета 60 нет, используем первый попавшийся активно-пассивный счет
            var accounts = await _accountService.GetAllAccountsAsync(false);
            var defaultAccount = accounts.FirstOrDefault(a => a.Type == Domain.Entities.AccountType.ActivePassive);
            if (defaultAccount != null)
            {
                return defaultAccount.Id;
            }

            throw new InvalidOperationException("Не удалось определить счет контрагента");
        }

        /// <summary>
        /// Обновить статус выписки на основе наличия проводок у документов
        /// </summary>
        public async Task UpdateStatementStatusAsync(int statementId)
        {
            var statement = await _statementRepository.GetByIdAsync(statementId);
            if (statement == null) return;

            var documents = await _documentRepository.FindAsync(d => d.BankStatementId == statementId);
            var documentList = documents.ToList();

            if (!documentList.Any())
            {
                statement.Status = StatementImportStatus.New;
            }
            else
            {
                int totalCount = documentList.Count;
                int processedCount = documentList.Count(d => d.EntryId.HasValue);

                if (processedCount == 0)
                {
                    statement.Status = StatementImportStatus.New;
                }
                else if (processedCount == totalCount)
                {
                    statement.Status = StatementImportStatus.Processed;
                }
                else
                {
                    statement.Status = StatementImportStatus.PartiallyProcessed;
                }
            }

            await _statementRepository.UpdateAsync(statement);

            Debug.WriteLine($"Обновлен статус выписки {statementId}: {statement.Status}");
        }

        /// <summary>
        /// Обновить документ выписки после создания проводки
        /// </summary>
        public async Task UpdateDocumentEntryIdAsync(int documentId, int entryId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document != null)
            {
                document.EntryId = entryId;
                await _documentRepository.UpdateAsync(document);

                Debug.WriteLine($"Обновлен документ {documentId}: установлен EntryId={entryId}");

                // Обновляем статус всей выписки
                if (document.BankStatementId > 0)
                {
                    await UpdateStatementStatusAsync(document.BankStatementId);
                }
            }
        }


        private async Task<BankStatementDto> MapToDto(BankStatement statement)
        {
            var documents = await _documentRepository.FindAsync(d => d.BankStatementId == statement.Id);
            var documentList = documents.ToList();

            var bankAccount = statement.BankAccountId.HasValue
                ? await _bankAccountService.GetBankAccountByIdAsync(statement.BankAccountId.Value)
                : null;

            var dto = new BankStatementDto
            {
                Id = statement.Id,
                FileName = statement.FileName,
                StartDate = statement.StartDate,
                EndDate = statement.EndDate,
                AccountNumber = statement.AccountNumber,
                BankAccountId = statement.BankAccountId,
                BankAccountDisplay = bankAccount?.DisplayName,
                ImportedAt = statement.ImportedAt,
                ImportedBy = statement.ImportedBy,
                Status = statement.Status.ToString(),
                DocumentsCount = documentList.Count,
                TotalIncoming = documentList.Where(d => d.IsIncoming).Sum(d => d.Amount),
                TotalOutgoing = documentList.Where(d => !d.IsIncoming).Sum(d => d.Amount)
            };

            // Заполняем документы
            foreach (var doc in documentList.OrderBy(d => d.Date))
            {
                dto.Documents.Add(new BankStatementDocumentDto
                {
                    Id = doc.Id,
                    DocumentType = doc.DocumentType,
                    Number = doc.Number,
                    Date = doc.Date,
                    Amount = doc.Amount,

                    // Плательщик
                    PayerAccount = doc.PayerAccount,
                    PayerINN = doc.PayerINN,
                    PayerName = doc.PayerName,
                    PayerBIK = doc.PayerBIK,

                    // Получатель
                    RecipientAccount = doc.RecipientAccount,
                    RecipientINN = doc.RecipientINN,
                    RecipientName = doc.RecipientName,
                    RecipientBIK = doc.RecipientBIK,

                    // Платеж
                    PaymentPurpose = doc.PaymentPurpose,
                    PaymentType = doc.PaymentType,
                    Priority = doc.Priority,

                    // Даты
                    ReceivedDate = doc.ReceivedDate,
                    WithdrawnDate = doc.WithdrawnDate,

                    // Связи
                    EntryId = doc.EntryId,
                    IsIncoming = doc.IsIncoming,

                    // Хэш
                    Hash = doc.Hash
                });
            }

            return dto;
        }
    }
}