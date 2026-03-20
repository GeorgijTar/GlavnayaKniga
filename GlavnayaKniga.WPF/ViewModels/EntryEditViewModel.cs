using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.Domain.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class EntryEditViewModel : BaseViewModel
    {
        private readonly IEntryService _entryService;
        private readonly IAccountService _accountService;
        private readonly IRepository<TransactionBasis> _basisRepository;
        private readonly EntryDto? _originalEntry;
        private readonly Window _window;

        [ObservableProperty]
        private EntryDto _entry;

        [ObservableProperty]
        private ObservableCollection<AccountDto> _accounts;

        [ObservableProperty]
        private ObservableCollection<TransactionBasis> _bases;

        [ObservableProperty]
        private AccountDto? _selectedDebitAccount;

        [ObservableProperty]
        private AccountDto? _selectedCreditAccount;

        [ObservableProperty]
        private TransactionBasis? _selectedBasis;

        [ObservableProperty]
        private string _title;

        public EntryEditViewModel(
            IEntryService entryService,
            IAccountService accountService,
            IRepository<TransactionBasis> basisRepository,
            EntryDto? entryToEdit,
            Window window)
        {
            _entryService = entryService;
            _accountService = accountService;
            _basisRepository = basisRepository;
            _originalEntry = entryToEdit;
            _window = window;

            _accounts = new ObservableCollection<AccountDto>();
            _bases = new ObservableCollection<TransactionBasis>();

            if (_originalEntry != null)
            {
                _entry = new EntryDto
                {
                    Id = _originalEntry.Id,
                    Date = _originalEntry.Date,
                    DebitAccountId = _originalEntry.DebitAccountId,
                    CreditAccountId = _originalEntry.CreditAccountId,
                    Amount = _originalEntry.Amount,
                    BasisId = _originalEntry.BasisId,
                    Note = _originalEntry.Note
                };
                Title = "Редактирование проводки";
            }
            else
            {
                _entry = new EntryDto
                {
                    Date = DateTime.Today,
                    Amount = 0
                };
                Title = "Новая проводка";
            }

            _ = LoadDataAsync();
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

                // Устанавливаем выбранные значения
                if (_originalEntry != null)
                {
                    SelectedDebitAccount = Accounts.FirstOrDefault(a => a.Id == _originalEntry.DebitAccountId);
                    SelectedCreditAccount = Accounts.FirstOrDefault(a => a.Id == _originalEntry.CreditAccountId);
                    SelectedBasis = Bases.FirstOrDefault(b => b.Id == _originalEntry.BasisId);
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

                IsBusy = true;
                StatusMessage = "Сохранение...";

                if (Entry.Id > 0)
                {
                    // Редактирование
                    await _entryService.UpdateEntryAsync(Entry);
                    MessageBox.Show(_window, "Проводка успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _entryService.CreateEntryAsync(Entry);
                    MessageBox.Show(_window, "Проводка успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

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
    }
}