using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Interfaces
{
    public interface IReceiptService
    {
        // CRUD для документов
        Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync(bool includeDraft = true, bool includePosted = true);
        Task<ReceiptDto?> GetReceiptByIdAsync(int id);
        Task<ReceiptDto> CreateReceiptAsync(ReceiptDto receiptDto);
        Task<ReceiptDto> UpdateReceiptAsync(ReceiptDto receiptDto);
        Task<bool> DeleteReceiptAsync(int id);

        // Управление статусами
        Task<ReceiptDto> PostReceiptAsync(int id, int defaultBasisId);
        Task<ReceiptDto> UnpostReceiptAsync(int id);

        // Работа со строками
        Task<ReceiptItemDto> AddItemAsync(ReceiptItemDto itemDto);
        Task<ReceiptItemDto> UpdateItemAsync(ReceiptItemDto itemDto);
        Task<bool> DeleteItemAsync(int itemId);
        Task<IEnumerable<ReceiptItemDto>> GetItemsAsync(int receiptId);

        // Получение остатков
        Task<IEnumerable<NomenclatureStockDto>> GetNomenclatureStocksAsync(int? nomenclatureId = null, int? storageLocationId = null);
        Task<NomenclatureStockDto?> GetNomenclatureStockAsync(int nomenclatureId, int? storageLocationId = null);

        // Вспомогательные методы
        Task<string> GenerateDocumentNumberAsync(DateTime date);
        Task<decimal> CalculateTotalAmountAsync(IEnumerable<ReceiptItemDto> items);
    }
}