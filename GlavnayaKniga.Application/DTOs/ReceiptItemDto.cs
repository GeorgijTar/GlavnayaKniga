using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GlavnayaKniga.Application.DTOs
{
    public class ReceiptItemDto : INotifyPropertyChanged
    {
        private int _id;
        private int _receiptId;
        private int _nomenclatureId;
        private string? _nomenclatureName;
        private string? _nomenclatureArticle;
        private string? _nomenclatureUnit;
        private decimal _quantity;
        private decimal _price;
        private decimal _amount;
        private decimal? _vatRate;
        private decimal? _vatAmount;
        private decimal? _amountWithVat;
        private int _debitAccountId;
        private string? _debitAccountCode;
        private string? _debitAccountName;
        private int _vatAccountId;
        private string? _vatAccountCode;
        private string? _vatAccountName;
        private int? _storageLocationId;
        private string? _storageLocationName;
        private string? _note;
        private int _lineNumber;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public int ReceiptId
        {
            get => _receiptId;
            set { _receiptId = value; OnPropertyChanged(); }
        }

        public int NomenclatureId
        {
            get => _nomenclatureId;
            set { _nomenclatureId = value; OnPropertyChanged(); }
        }

        public string? NomenclatureName
        {
            get => _nomenclatureName;
            set { _nomenclatureName = value; OnPropertyChanged(); }
        }

        public string? NomenclatureArticle
        {
            get => _nomenclatureArticle;
            set { _nomenclatureArticle = value; OnPropertyChanged(); }
        }

        public string? NomenclatureUnit
        {
            get => _nomenclatureUnit;
            set { _nomenclatureUnit = value; OnPropertyChanged(); }
        }

        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        public decimal? VatRate
        {
            get => _vatRate;
            set { _vatRate = value; OnPropertyChanged(); }
        }

        public decimal? VatAmount
        {
            get => _vatAmount;
            set { _vatAmount = value; OnPropertyChanged(); }
        }

        public decimal? AmountWithVat
        {
            get => _amountWithVat;
            set { _amountWithVat = value; OnPropertyChanged(); }
        }

        public int DebitAccountId
        {
            get => _debitAccountId;
            set { _debitAccountId = value; OnPropertyChanged(); }
        }

        public string? DebitAccountCode
        {
            get => _debitAccountCode;
            set { _debitAccountCode = value; OnPropertyChanged(); }
        }

        public string? DebitAccountName
        {
            get => _debitAccountName;
            set { _debitAccountName = value; OnPropertyChanged(); }
        }

        public int VatAccountId
        {
            get => _vatAccountId;
            set { _vatAccountId = value; OnPropertyChanged(); }
        }

        public string? VatAccountCode
        {
            get => _vatAccountCode;
            set { _vatAccountCode = value; OnPropertyChanged(); }
        }

        public string? VatAccountName
        {
            get => _vatAccountName;
            set { _vatAccountName = value; OnPropertyChanged(); }
        }

        public int? StorageLocationId
        {
            get => _storageLocationId;
            set { _storageLocationId = value; OnPropertyChanged(); }
        }

        public string? StorageLocationName
        {
            get => _storageLocationName;
            set { _storageLocationName = value; OnPropertyChanged(); }
        }

        public string? Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public int LineNumber
        {
            get => _lineNumber;
            set { _lineNumber = value; OnPropertyChanged(); }
        }

        // Вычисляемые свойства
        public string DisplayName => $"{NomenclatureName} ({NomenclatureArticle})";
        public string DisplayQuantity => $"{Quantity:N3} {NomenclatureUnit}";
        public string DisplayAmount => Amount.ToString("N2");
        public string DisplayAmountWithVat => AmountWithVat?.ToString("N2") ?? Amount.ToString("N2");
        public string DisplayVatRate => VatRate?.ToString("N0") ?? "без НДС";
        public bool HasVat => VatRate.HasValue && VatRate.Value > 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}