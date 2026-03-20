using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IRepository<Receipt> _receiptRepository;
        private readonly IRepository<ReceiptItem> _itemRepository;
        private readonly IRepository<Entry> _entryRepository;
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Counterparty> _counterpartyRepository;
        private readonly IRepository<Nomenclature> _nomenclatureRepository;
        private readonly IRepository<StorageLocation> _storageLocationRepository;
        private readonly IRepository<TransactionBasis> _basisRepository;
        private readonly IAccountService _accountService;
        private readonly INomenclatureService _nomenclatureService;

        private readonly IRepository<UnitOfMeasure> _unitRepository;

        public ReceiptService(
            IRepository<Receipt> receiptRepository,
            IRepository<ReceiptItem> itemRepository,
            IRepository<Entry> entryRepository,
            IRepository<Account> accountRepository,
            IRepository<Counterparty> counterpartyRepository,
            IRepository<Nomenclature> nomenclatureRepository,
            IRepository<StorageLocation> storageLocationRepository,
            IRepository<TransactionBasis> basisRepository,
            IAccountService accountService,
            INomenclatureService nomenclatureService,
            IRepository<UnitOfMeasure> unitRepository)
        {
            _receiptRepository = receiptRepository;
            _itemRepository = itemRepository;
            _entryRepository = entryRepository;
            _accountRepository = accountRepository;
            _counterpartyRepository = counterpartyRepository;
            _nomenclatureRepository = nomenclatureRepository;
            _storageLocationRepository = storageLocationRepository;
            _basisRepository = basisRepository;
            _accountService = accountService;
            _nomenclatureService = nomenclatureService;
            _unitRepository = unitRepository;
        }

        public async Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync(bool includeDraft = true, bool includePosted = true)
        {
            var receipts = await _receiptRepository.GetAllAsync();

            var filtered = receipts.Where(r =>
                (includeDraft && r.Status == ReceiptStatus.Draft) ||
                (includePosted && r.Status == ReceiptStatus.Posted));

            var result = new List<ReceiptDto>();
            foreach (var receipt in filtered.OrderByDescending(r => r.Date).ThenByDescending(r => r.Number))
            {
                var dto = await MapToDto(receipt);
                result.Add(dto);
            }

            return result;
        }

        public async Task<ReceiptDto?> GetReceiptByIdAsync(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            return receipt != null ? await MapToDto(receipt) : null;
        }

        public async Task<ReceiptDto> CreateReceiptAsync(ReceiptDto receiptDto)
        {
            // Генерируем номер документа, если не указан
            if (string.IsNullOrWhiteSpace(receiptDto.Number))
            {
                receiptDto.Number = await GenerateDocumentNumberAsync(receiptDto.Date);
            }

            // Проверяем уникальность номера
            var existing = await _receiptRepository.FindAsync(r => r.Number == receiptDto.Number);
            if (existing.Any())
            {
                throw new InvalidOperationException($"Документ с номером {receiptDto.Number} уже существует");
            }

            // Проверяем существование контрагента
            var contractor = await _counterpartyRepository.GetByIdAsync(receiptDto.ContractorId);
            if (contractor == null)
            {
                throw new InvalidOperationException($"Контрагент с ID {receiptDto.ContractorId} не найден");
            }

            // Проверяем существование счета кредита
            var creditAccount = await _accountRepository.GetByIdAsync(receiptDto.CreditAccountId);
            if (creditAccount == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {receiptDto.CreditAccountId} не найден");
            }

            // Если это УПД и не указан номер с/ф, используем номер документа
            if (receiptDto.IsUPD && string.IsNullOrWhiteSpace(receiptDto.InvoiceNumber))
            {
                receiptDto.InvoiceNumber = receiptDto.Number;
                receiptDto.InvoiceDate = receiptDto.Date;
            }

            var receipt = new Receipt
            {
                Number = receiptDto.Number,
                Date = receiptDto.Date,
                AccountingDate = receiptDto.AccountingDate,
                ContractorId = receiptDto.ContractorId,
                CreditAccountId = receiptDto.CreditAccountId,
                VatCalculationMethod = receiptDto.VatCalculationMethod == "AbovePrice"
                    ? VatCalculationMethod.AbovePrice
                    : VatCalculationMethod.IncludedInPrice,
                ContractNumber = receiptDto.ContractNumber,
                ContractDate = receiptDto.ContractDate,
                Basis = receiptDto.Basis,
                TotalAmount = 0,
                TotalVatAmount = 0,
                TotalAmountWithVat = 0,
                IsUPD = receiptDto.IsUPD,
                InvoiceNumber = receiptDto.InvoiceNumber,
                InvoiceDate = receiptDto.InvoiceDate,
                Status = ReceiptStatus.Draft,
                Note = receiptDto.Note,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Environment.UserName
            };

            var created = await _receiptRepository.AddAsync(receipt);
            return await MapToDto(created);
        }

        public async Task<ReceiptDto> UpdateReceiptAsync(ReceiptDto receiptDto)
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptDto.Id);
            if (receipt == null)
            {
                throw new InvalidOperationException($"Документ с ID {receiptDto.Id} не найден");
            }

            if (receipt.Status != ReceiptStatus.Draft)
            {
                throw new InvalidOperationException("Нельзя редактировать проведенный документ");
            }

            // Проверяем уникальность номера (если он изменился)
            if (receipt.Number != receiptDto.Number)
            {
                var existing = await _receiptRepository.FindAsync(r => r.Number == receiptDto.Number);
                if (existing.Any())
                {
                    throw new InvalidOperationException($"Документ с номером {receiptDto.Number} уже существует");
                }
            }

            // Проверяем существование контрагента
            var contractor = await _counterpartyRepository.GetByIdAsync(receiptDto.ContractorId);
            if (contractor == null)
            {
                throw new InvalidOperationException($"Контрагент с ID {receiptDto.ContractorId} не найден");
            }

            // Проверяем существование счета кредита
            var creditAccount = await _accountRepository.GetByIdAsync(receiptDto.CreditAccountId);
            if (creditAccount == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {receiptDto.CreditAccountId} не найден");
            }

            receipt.Number = receiptDto.Number;
            receipt.Date = receiptDto.Date;
            receipt.AccountingDate = receiptDto.AccountingDate;
            receipt.ContractorId = receiptDto.ContractorId;
            receipt.CreditAccountId = receiptDto.CreditAccountId;
            receipt.VatCalculationMethod = receiptDto.VatCalculationMethod == "AbovePrice"
                ? VatCalculationMethod.AbovePrice
                : VatCalculationMethod.IncludedInPrice;
            receipt.ContractNumber = receiptDto.ContractNumber;
            receipt.ContractDate = receiptDto.ContractDate;
            receipt.Basis = receiptDto.Basis;
            receipt.IsUPD = receiptDto.IsUPD;
            receipt.InvoiceNumber = receiptDto.InvoiceNumber;
            receipt.InvoiceDate = receiptDto.InvoiceDate;
            receipt.Note = receiptDto.Note;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _receiptRepository.UpdateAsync(receipt);
            return await MapToDto(receipt);
        }

        public async Task<bool> DeleteReceiptAsync(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null) return false;

            if (receipt.Status == ReceiptStatus.Posted)
            {
                // Если документ проведен, сначала удаляем проводки
                await UnpostReceiptAsync(id);
            }

            await _receiptRepository.DeleteAsync(receipt);
            return true;
        }

        public async Task<ReceiptDto> PostReceiptAsync(int id, int defaultBasisId)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null)
            {
                throw new InvalidOperationException($"Документ с ID {id} не найден");
            }

            if (receipt.Status == ReceiptStatus.Posted)
            {
                throw new InvalidOperationException("Документ уже проведен");
            }

            var items = await _itemRepository.FindAsync(i => i.ReceiptId == id);
            if (!items.Any())
            {
                throw new InvalidOperationException("Нельзя провести документ без строк");
            }

            // Получаем основание для проводок
            var basis = await _basisRepository.GetByIdAsync(defaultBasisId);
            if (basis == null)
            {
                throw new InvalidOperationException("Основание для проводок не найдено");
            }

            // Создаем проводки
            var entries = new List<Entry>();

            foreach (var item in items)
            {
                // Дебет счета учета номенклатуры (материалы, товары и т.д.), Кредит счета поставщика
                var entry = new Entry
                {
                    Date = receipt.AccountingDate,
                    DebitAccountId = item.DebitAccountId,
                    CreditAccountId = receipt.CreditAccountId,
                    Amount = item.Amount,
                    BasisId = defaultBasisId,
                    Note = $"Поступление: {receipt.Number} от {receipt.Date:d}, {item.Nomenclature?.Name}",
                    CreatedAt = DateTime.UtcNow
                };
                entries.Add(entry);

                // Если есть НДС, создаем дополнительную проводку
                if (item.VatAmount.HasValue && item.VatAmount.Value > 0)
                {
                    // Проверяем, что счет учета НДС указан
                    if (item.VatAccountId == 0)
                    {
                        throw new InvalidOperationException($"В строке {item.LineNumber} не указан счет учета НДС");
                    }

                    var vatEntry = new Entry
                    {
                        Date = receipt.AccountingDate,
                        DebitAccountId = item.VatAccountId, // Используем счет НДС из строки документа
                        CreditAccountId = receipt.CreditAccountId,
                        Amount = item.VatAmount.Value,
                        BasisId = defaultBasisId,
                        Note = $"НДС по поступлению: {receipt.Number} от {receipt.Date:d}, {item.Nomenclature?.Name}",
                        CreatedAt = DateTime.UtcNow
                    };
                    entries.Add(vatEntry);
                }

                // Обновляем остатки номенклатуры
                if (item.NomenclatureId > 0)
                {
                    await _nomenclatureService.UpdateStockAsync(item.NomenclatureId, item.Quantity);
                }
            }

            foreach (var entry in entries)
            {
                await _entryRepository.AddAsync(entry);
            }

            receipt.Status = ReceiptStatus.Posted;
            receipt.PostedAt = DateTime.UtcNow;
            receipt.PostedBy = Environment.UserName;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _receiptRepository.UpdateAsync(receipt);

            return await MapToDto(receipt);
        }

        public async Task<ReceiptDto> UnpostReceiptAsync(int id)
        {
            var receipt = await _receiptRepository.GetByIdAsync(id);
            if (receipt == null)
            {
                throw new InvalidOperationException($"Документ с ID {id} не найден");
            }

            if (receipt.Status != ReceiptStatus.Posted)
            {
                throw new InvalidOperationException("Документ не проведен");
            }

            // Получаем все проводки, связанные с документом по номеру и дате
            var entries = await _entryRepository.FindAsync(e =>
                (e.Note != null && e.Note.Contains($"Поступление: {receipt.Number}")) ||
                (e.Note != null && e.Note.Contains($"НДС по поступлению: {receipt.Number}")));

            // Возвращаем остатки
            var items = await _itemRepository.FindAsync(i => i.ReceiptId == id);
            foreach (var item in items)
            {
                if (item.NomenclatureId > 0)
                {
                    await _nomenclatureService.UpdateStockAsync(item.NomenclatureId, -item.Quantity);
                }
            }

            // Удаляем все проводки
            foreach (var entry in entries)
            {
                await _entryRepository.DeleteAsync(entry);
            }

            receipt.Status = ReceiptStatus.Draft;
            receipt.PostedAt = null;
            receipt.PostedBy = null;
            receipt.UpdatedAt = DateTime.UtcNow;

            await _receiptRepository.UpdateAsync(receipt);

            return await MapToDto(receipt);
        }

        public async Task<ReceiptItemDto> AddItemAsync(ReceiptItemDto itemDto)
        {
            var receipt = await _receiptRepository.GetByIdAsync(itemDto.ReceiptId);
            if (receipt == null)
            {
                throw new InvalidOperationException($"Документ с ID {itemDto.ReceiptId} не найден");
            }

            if (receipt.Status != ReceiptStatus.Draft)
            {
                throw new InvalidOperationException("Нельзя добавлять строки в проведенный документ");
            }

            var nomenclature = await _nomenclatureRepository.GetByIdAsync(itemDto.NomenclatureId);
            if (nomenclature == null)
            {
                throw new InvalidOperationException($"Номенклатура с ID {itemDto.NomenclatureId} не найдена");
            }

            var debitAccount = await _accountRepository.GetByIdAsync(itemDto.DebitAccountId);
            if (debitAccount == null)
            {
                throw new InvalidOperationException($"Счет учета с ID {itemDto.DebitAccountId} не найден");
            }

            // Проверяем счет учета НДС
            var vatAccount = await _accountRepository.GetByIdAsync(itemDto.VatAccountId);
            if (vatAccount == null)
            {
                throw new InvalidOperationException($"Счет учета НДС с ID {itemDto.VatAccountId} не найден");
            }

            // Проверяем место хранения (если указано)
            if (itemDto.StorageLocationId.HasValue)
            {
                var storage = await _storageLocationRepository.GetByIdAsync(itemDto.StorageLocationId.Value);
                if (storage == null)
                {
                    throw new InvalidOperationException($"Место хранения с ID {itemDto.StorageLocationId} не найдено");
                }
            }

            // Рассчитываем суммы в зависимости от способа расчета НДС из шапки документа
            decimal baseAmount = itemDto.Quantity * itemDto.Price;
            decimal amount = baseAmount;
            decimal? vatAmount = null;
            decimal? amountWithVat = null;

            if (itemDto.VatRate.HasValue && itemDto.VatRate.Value > 0)
            {
                if (receipt.VatCalculationMethod == VatCalculationMethod.AbovePrice)
                {
                    // НДС сверху цены
                    vatAmount = baseAmount * itemDto.VatRate.Value / 100;
                    amountWithVat = baseAmount + vatAmount;
                    amount = baseAmount;
                }
                else
                {
                    // НДС включен в цену
                    amountWithVat = baseAmount;
                    vatAmount = baseAmount * itemDto.VatRate.Value / (100 + itemDto.VatRate.Value);
                    amount = amountWithVat.Value - vatAmount.Value;
                }
            }
            else
            {
                amountWithVat = baseAmount;
                amount = baseAmount;
            }

            // Получаем следующий номер строки
            var existingItems = await _itemRepository.FindAsync(i => i.ReceiptId == itemDto.ReceiptId);
            int lineNumber = existingItems.Any() ? existingItems.Max(i => i.LineNumber) + 1 : 1;

            var item = new ReceiptItem
            {
                ReceiptId = itemDto.ReceiptId,
                NomenclatureId = itemDto.NomenclatureId,
                Quantity = itemDto.Quantity,
                Price = itemDto.Price,
                Amount = amount,
                VatRate = itemDto.VatRate,
                VatAmount = vatAmount,
                AmountWithVat = amountWithVat,
                DebitAccountId = itemDto.DebitAccountId,
                VatAccountId = itemDto.VatAccountId,
                StorageLocationId = itemDto.StorageLocationId,
                Note = itemDto.Note,
                LineNumber = lineNumber
            };

            var created = await _itemRepository.AddAsync(item);

            // Обновляем суммы документа
            await UpdateReceiptTotals(itemDto.ReceiptId);

            return await MapItemToDto(created);
        }

        public async Task<ReceiptItemDto> UpdateItemAsync(ReceiptItemDto itemDto)
        {
            var item = await _itemRepository.GetByIdAsync(itemDto.Id);
            if (item == null)
            {
                throw new InvalidOperationException($"Строка документа с ID {itemDto.Id} не найдена");
            }

            var receipt = await _receiptRepository.GetByIdAsync(item.ReceiptId);
            if (receipt?.Status != ReceiptStatus.Draft)
            {
                throw new InvalidOperationException("Нельзя редактировать строки проведенного документа");
            }

            // Проверяем счет учета НДС
            var vatAccount = await _accountRepository.GetByIdAsync(itemDto.VatAccountId);
            if (vatAccount == null)
            {
                throw new InvalidOperationException($"Счет учета НДС с ID {itemDto.VatAccountId} не найден");
            }

            // Пересчитываем суммы с учетом способа НДС из шапки
            decimal baseAmount = itemDto.Quantity * itemDto.Price;
            decimal amount = baseAmount;
            decimal? vatAmount = null;
            decimal? amountWithVat = null;

            if (itemDto.VatRate.HasValue && itemDto.VatRate.Value > 0)
            {
                if (receipt.VatCalculationMethod == VatCalculationMethod.AbovePrice)
                {
                    vatAmount = baseAmount * itemDto.VatRate.Value / 100;
                    amountWithVat = baseAmount + vatAmount;
                    amount = baseAmount;
                }
                else
                {
                    amountWithVat = baseAmount;
                    vatAmount = baseAmount * itemDto.VatRate.Value / (100 + itemDto.VatRate.Value);
                    amount = amountWithVat.Value - vatAmount.Value;
                }
            }
            else
            {
                amountWithVat = baseAmount;
                amount = baseAmount;
            }

            item.Quantity = itemDto.Quantity;
            item.Price = itemDto.Price;
            item.Amount = amount;
            item.VatRate = itemDto.VatRate;
            item.VatAmount = vatAmount;
            item.AmountWithVat = amountWithVat;
            item.DebitAccountId = itemDto.DebitAccountId;
            item.VatAccountId = itemDto.VatAccountId;
            item.StorageLocationId = itemDto.StorageLocationId;
            item.Note = itemDto.Note;

            await _itemRepository.UpdateAsync(item);

            // Обновляем суммы документа
            await UpdateReceiptTotals(item.ReceiptId);

            return await MapItemToDto(item);
        }


        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var item = await _itemRepository.GetByIdAsync(itemId);
            if (item == null) return false;

            var receipt = await _receiptRepository.GetByIdAsync(item.ReceiptId);
            if (receipt?.Status != ReceiptStatus.Draft)
            {
                throw new InvalidOperationException("Нельзя удалять строки проведенного документа");
            }

            int receiptId = item.ReceiptId;
            await _itemRepository.DeleteAsync(item);

            // Обновляем суммы документа
            await UpdateReceiptTotals(receiptId);

            return true;
        }

        public async Task<IEnumerable<ReceiptItemDto>> GetItemsAsync(int receiptId)
        {
            var items = await _itemRepository.FindAsync(i => i.ReceiptId == receiptId);
            var result = new List<ReceiptItemDto>();

            foreach (var item in items.OrderBy(i => i.LineNumber))
            {
                var dto = await MapItemToDto(item);
                result.Add(dto);
            }

            return result;
        }

        public async Task<string> GenerateDocumentNumberAsync(DateTime date)
        {
            var year = date.Year.ToString().Substring(2);
            var month = date.Month.ToString("00");

            // Получаем последний документ за этот месяц
            var receipts = await _receiptRepository.FindAsync(r =>
                r.Date.Year == date.Year && r.Date.Month == date.Month);

            int lastNumber = receipts.Count() + 1;

            return $"ПН-{year}{month}-{lastNumber:D4}";
        }

        public async Task<decimal> CalculateTotalAmountAsync(IEnumerable<ReceiptItemDto> items)
        {
            return items.Sum(i => i.Amount);
        }

        public async Task<IEnumerable<NomenclatureStockDto>> GetNomenclatureStocksAsync(int? nomenclatureId = null, int? storageLocationId = null)
        {
            var nomenclatures = await _nomenclatureRepository.GetAllAsync();
            if (nomenclatureId.HasValue)
            {
                nomenclatures = nomenclatures.Where(n => n.Id == nomenclatureId.Value);
            }

            var result = new List<NomenclatureStockDto>();

            foreach (var nomenclature in nomenclatures)
            {
                // Получаем все движения по номенклатуре
                var receipts = await _receiptRepository.FindAsync(r => r.Status == ReceiptStatus.Posted);
                var receiptIds = receipts.Select(r => r.Id);

                var items = await _itemRepository.FindAsync(i =>
                    receiptIds.Contains(i.ReceiptId) &&
                    i.NomenclatureId == nomenclature.Id);

                if (storageLocationId.HasValue)
                {
                    items = items.Where(i => i.StorageLocationId == storageLocationId);
                }

                // Группируем по местам хранения
                var groupedByStorage = items.GroupBy(i => i.StorageLocationId);

                foreach (var group in groupedByStorage)
                {
                    var storageLocation = group.Key.HasValue
                        ? await _storageLocationRepository.GetByIdAsync(group.Key.Value)
                        : null;

                    var stock = new NomenclatureStockDto
                    {
                        NomenclatureId = nomenclature.Id,
                        NomenclatureName = nomenclature.Name,
                        NomenclatureArticle = nomenclature.Article ?? "",
                        Unit = nomenclature.Unit.ToString(),
                        StorageLocationId = group.Key,
                        StorageLocationName = storageLocation?.Name,
                        CurrentStock = group.Sum(i => i.Quantity),
                        ReservedStock = 0,
                        MinStock = nomenclature.MinStock,
                        MaxStock = nomenclature.MaxStock,
                        AveragePrice = group.Any() ? group.Average(i => i.Price) : 0,
                        LastMovementDate = group.Any() ? group.Max(i => i.Receipt?.AccountingDate ?? DateTime.MinValue) : DateTime.MinValue,
                        LastMovementType = "Поступление"
                    };

                    result.Add(stock);
                }
            }

            return result;
        }

        public async Task<NomenclatureStockDto?> GetNomenclatureStockAsync(int nomenclatureId, int? storageLocationId = null)
        {
            var stocks = await GetNomenclatureStocksAsync(nomenclatureId, storageLocationId);
            return stocks.FirstOrDefault();
        }

        private async Task UpdateReceiptTotals(int receiptId)
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptId);
            if (receipt == null) return;

            var items = await _itemRepository.FindAsync(i => i.ReceiptId == receiptId);

            receipt.TotalAmount = items.Sum(i => i.Amount);
            receipt.TotalVatAmount = items.Sum(i => i.VatAmount ?? 0);
            receipt.TotalAmountWithVat = items.Sum(i => i.AmountWithVat ?? i.Amount);

            await _receiptRepository.UpdateAsync(receipt);
        }

        private async Task<ReceiptDto> MapToDto(Receipt receipt)
        {
            var contractor = await _counterpartyRepository.GetByIdAsync(receipt.ContractorId);
            var creditAccount = await _accountRepository.GetByIdAsync(receipt.CreditAccountId);

            var items = await _itemRepository.FindAsync(i => i.ReceiptId == receipt.Id);

            return new ReceiptDto
            {
                Id = receipt.Id,
                Number = receipt.Number,
                Date = receipt.Date,
                AccountingDate = receipt.AccountingDate,
                ContractorId = receipt.ContractorId,
                ContractorName = contractor?.ShortName,
                ContractorINN = contractor?.INN,
                CreditAccountId = receipt.CreditAccountId,
                CreditAccountCode = creditAccount?.Code,
                CreditAccountName = creditAccount?.Name,
                VatCalculationMethod = receipt.VatCalculationMethod.ToString(),
                ContractNumber = receipt.ContractNumber,
                ContractDate = receipt.ContractDate,
                Basis = receipt.Basis,
                TotalAmount = receipt.TotalAmount,
                TotalVatAmount = receipt.TotalVatAmount,
                TotalAmountWithVat = receipt.TotalAmountWithVat,
                IsUPD = receipt.IsUPD,
                InvoiceNumber = receipt.InvoiceNumber,
                InvoiceDate = receipt.InvoiceDate,
                Status = receipt.Status.ToString(),
                Note = receipt.Note,
                CreatedAt = receipt.CreatedAt,
                UpdatedAt = receipt.UpdatedAt,
                PostedAt = receipt.PostedAt,
                CreatedBy = receipt.CreatedBy,
                PostedBy = receipt.PostedBy,
                Items = (await Task.WhenAll(items.Select(i => MapItemToDto(i)))).ToList()
            };
        }

        private async Task<ReceiptItemDto> MapItemToDto(ReceiptItem item)
        {
            var nomenclature = await _nomenclatureRepository.GetByIdAsync(item.NomenclatureId);
            var debitAccount = await _accountRepository.GetByIdAsync(item.DebitAccountId);
            var vatAccount = await _accountRepository.GetByIdAsync(item.VatAccountId);
            
            var storageLocation = item.StorageLocationId.HasValue
                ? await _storageLocationRepository.GetByIdAsync(item.StorageLocationId.Value)
                : null;

            // Загружаем единицу измерения
            string? unitName = null;
            if (nomenclature != null)
            {
                var unit = await _unitRepository.GetByIdAsync(nomenclature.UnitId);
                unitName = unit?.ShortName;
            }

            return new ReceiptItemDto
            {
                Id = item.Id,
                ReceiptId = item.ReceiptId,
                NomenclatureId = item.NomenclatureId,
                NomenclatureName = nomenclature?.Name,
                NomenclatureArticle = nomenclature?.Article,
                NomenclatureUnit = unitName,
                Quantity = item.Quantity,
                Price = item.Price,
                Amount = item.Amount,
                VatRate = item.VatRate,
                VatAmount = item.VatAmount,
                AmountWithVat = item.AmountWithVat,
                DebitAccountId = item.DebitAccountId,
                DebitAccountCode = debitAccount?.Code,
                DebitAccountName = debitAccount?.Name,
                VatAccountId = item.VatAccountId,
                VatAccountCode = vatAccount?.Code,
                VatAccountName = vatAccount?.Name,
                StorageLocationId = item.StorageLocationId,
                StorageLocationName = storageLocation?.Name,
                Note = item.Note,
                LineNumber = item.LineNumber
            };
        }
    }
}