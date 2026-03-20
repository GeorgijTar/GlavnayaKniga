using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Domain.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class BankStatementEntryViewModel : BaseViewModel
    {
        private readonly IEntryService _entryService;
        private readonly IAccountService _accountService;
        private readonly IRepository<TransactionBasis> _basisRepository;
        private readonly IBankStatementService _statementService; // Добавляем
        private readonly BankStatementDocumentDto _document;
        private readonly BankAccountDto _bankAccount;
        private readonly Window _window;

        [ObservableProperty]
        private EntryDto _entry;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private AccountDto? _selectedDebitAccount;

        [ObservableProperty]
        private AccountDto? _selectedCreditAccount;

        [ObservableProperty]
        private ObservableCollection<TransactionBasis> _bases;

        [ObservableProperty]
        private TransactionBasis? _selectedBasis;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isIncoming;

        public BankStatementEntryViewModel(
            IEntryService entryService,
            IAccountService accountService,
            IRepository<TransactionBasis> basisRepository,
            IBankStatementService statementService, // Добавляем параметр
            BankStatementDocumentDto document,
            BankAccountDto bankAccount,
            Window window)
        {
            _entryService = entryService;
            _accountService = accountService;
            _basisRepository = basisRepository;
            _statementService = statementService; // Инициализируем
            _document = document;
            _bankAccount = bankAccount;
            _window = window;

            _accounts = new ObservableCollection<AccountDto>();
            _bases = new ObservableCollection<TransactionBasis>();

            _isIncoming = document.IsIncoming;

            _entry = new EntryDto
            {
                Date = document.Date,
                Amount = document.Amount,
                Note = $"Банковская выписка: {document.DocumentType} №{document.Number} от {document.Date:d}. {document.PaymentPurpose}"
            };

            Title = $"Создание проводки по документу №{document.Number} от {document.Date:d}";

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка данных...";

                // Загружаем счета (только неархивные)
                var accounts = await _accountService.GetAllAccountsAsync(false);
                Accounts.Clear();
                foreach (var account in accounts.OrderBy(a => a.Code))
                {
                    Accounts.Add(account);
                }

                // Загружаем основания
                var bases = await _basisRepository.GetAllAsync();
                Bases.Clear();
                foreach (var basis in bases.OrderBy(b => b.Name))
                {
                    Bases.Add(basis);
                }
                SelectedBasis = Bases.FirstOrDefault();

                // Автоматически подставляем счета в зависимости от направления
                if (IsIncoming)
                {
                    // Входящий платеж: Дт - банковский счет, Кт - ? (счет контрагента)
                    SelectedDebitAccount = Accounts.FirstOrDefault(a => a.Id == _bankAccount.SubaccountId);
                    // Для кредита пытаемся найти счет 60 (расчеты с поставщиками)
                    SelectedCreditAccount = Accounts.FirstOrDefault(a => a.Code == "60" || a.Code.StartsWith("60."));
                }
                else
                {
                    // Исходящий платеж: Дт - ? (счет контрагента), Кт - банковский счет
                    SelectedCreditAccount = Accounts.FirstOrDefault(a => a.Id == _bankAccount.SubaccountId);
                    // Для дебета пытаемся найти счет 60 (расчеты с поставщиками)
                    SelectedDebitAccount = Accounts.FirstOrDefault(a => a.Code == "60" || a.Code.StartsWith("60."));
                }

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show(_window, $"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedDebitAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Entry.DebitAccountId = value.Id;
                Entry.DebitAccountCode = value.Code;
                Entry.DebitAccountName = value.Name;
            }
        }

        partial void OnSelectedCreditAccountChanged(AccountDto? value)
        {
            if (value != null)
            {
                Entry.CreditAccountId = value.Id;
                Entry.CreditAccountCode = value.Code;
                Entry.CreditAccountName = value.Name;
            }
        }

        partial void OnSelectedBasisChanged(TransactionBasis? value)
        {
            if (value != null)
            {
                Entry.BasisId = value.Id;
                Entry.BasisName = value.Name;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (SelectedDebitAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет дебета", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedCreditAccount == null)
                {
                    MessageBox.Show(_window, "Выберите счет кредита", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedDebitAccount.Id == SelectedCreditAccount.Id)
                {
                    MessageBox.Show(_window, "Счета дебета и кредита должны быть разными", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedBasis == null)
                {
                    MessageBox.Show(_window, "Выберите основание проводки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Entry.Amount <= 0)
                {
                    MessageBox.Show(_window, "Сумма должна быть положительной", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаем проводку
                var createdEntry = await _entryService.CreateEntryAsync(Entry);

                // Обновляем документ выписки - проставляем EntryId
                await _statementService.UpdateDocumentEntryIdAsync(_document.Id, createdEntry.Id);

                Debug.WriteLine($"Создана проводка ID={createdEntry.Id} для документа ID={_document.Id}");

                MessageBox.Show(_window, "Проводка успешно создана", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        [RelayCommand]
        private void SwapAccounts()
        {
            var temp = SelectedDebitAccount;
            SelectedDebitAccount = SelectedCreditAccount;
            SelectedCreditAccount = temp;
        }
    }
}