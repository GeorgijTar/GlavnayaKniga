using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class CounterpartyEditViewModel : BaseViewModel
    {
        private readonly ICounterpartyService _counterpartyService;
        private readonly IBikService _bikService;
        private readonly ICheckoService _checkoService;
        private readonly CounterpartyDto? _originalCounterparty;
        private readonly Window _window;

        [ObservableProperty]
        private CounterpartyDto _counterparty;

        [ObservableProperty]
        private ObservableCollection<string> _counterpartyTypes;

        [ObservableProperty]
        private string _selectedType;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private ObservableCollection<CounterpartyBankAccountDto> _bankAccounts;

        [ObservableProperty]
        private CounterpartyBankAccountDto? _selectedBankAccount;

        public CounterpartyEditViewModel(
            ICounterpartyService counterpartyService,
            IBikService bikService,
            ICheckoService checkoService,
            CounterpartyDto? counterpartyToEdit,
            Window window)
        {
            _counterpartyService = counterpartyService;
            _bikService = bikService;
            _checkoService = checkoService ?? throw new ArgumentNullException(nameof(checkoService));
            _originalCounterparty = counterpartyToEdit;
            _window = window;

            _counterpartyTypes = new ObservableCollection<string>
            {
                "Юридическое лицо",
                "Индивидуальный предприниматель",
                "Физическое лицо"
            };

            _bankAccounts = new ObservableCollection<CounterpartyBankAccountDto>();

            if (_originalCounterparty != null)
            {
                _counterparty = new CounterpartyDto
                {
                    Id = _originalCounterparty.Id,
                    FullName = _originalCounterparty.FullName,
                    ShortName = _originalCounterparty.ShortName,
                    INN = _originalCounterparty.INN,
                    KPP = _originalCounterparty.KPP,
                    OGRN = _originalCounterparty.OGRN,
                    Type = _originalCounterparty.Type,
                    LegalAddress = _originalCounterparty.LegalAddress,
                    ActualAddress = _originalCounterparty.ActualAddress,
                    Phone = _originalCounterparty.Phone,
                    Email = _originalCounterparty.Email,
                    ContactPerson = _originalCounterparty.ContactPerson,
                    Note = _originalCounterparty.Note,
                    IsArchived = _originalCounterparty.IsArchived
                };
                _selectedType = _originalCounterparty.Type;
                Title = "Редактирование контрагента";
                IsEditMode = true;

                // Загружаем банковские счета
                _ = LoadBankAccountsAsync();
            }
            else
            {
                _counterparty = new CounterpartyDto
                {
                    Type = "Юридическое лицо"
                };
                _selectedType = "Юридическое лицо";
                Title = "Добавление контрагента";
                IsEditMode = false;
            }
        }

        private async Task LoadBankAccountsAsync()
        {
            if (_originalCounterparty == null) return;

            try
            {
                var accounts = await _counterpartyService.GetBankAccountsAsync(_originalCounterparty.Id);
                BankAccounts.Clear();
                foreach (var account in accounts)
                {
                    BankAccounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка загрузки банковских счетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedTypeChanged(string value)
        {
            Counterparty.Type = value;
        }

        /// <summary>
        /// Загрузка данных контрагента по ИНН из Checko
        /// </summary>
        [RelayCommand]
        private async Task LoadFromCheckoAsync()
        {
            if (string.IsNullOrWhiteSpace(Counterparty.INN))
            {
                MessageBox.Show(_window, "Введите ИНН для поиска", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Простая валидация ИНН
            if (Counterparty.INN.Length != 10 && Counterparty.INN.Length != 12)
            {
                MessageBox.Show(_window, "ИНН должен содержать 10 или 12 цифр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Поиск информации о контрагенте...";

                // Проверяем, что сервис инициализирован
                if (_checkoService == null)
                {
                    MessageBox.Show(_window, "Сервис проверки контрагентов не инициализирован", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var data = await _checkoService.GetCounterpartyDataAsync(Counterparty.INN);

                if (data == null)
                {
                    MessageBox.Show(_window, "Контрагент с таким ИНН не найден", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Заполняем поля в зависимости от типа
                if (data is CheckoCompanyData company)
                {
                    FillFromCompanyData(company);
                }
                else if (data is CheckoEntrepreneurData entrepreneur)
                {
                    FillFromEntrepreneurData(entrepreneur);
                }

                MessageBox.Show(_window, "Данные успешно загружены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        private void FillFromCompanyData(CheckoCompanyData company)
        {
            Counterparty.FullName = company.FullName ?? Counterparty.FullName;
            Counterparty.ShortName = company.ShortName ?? Counterparty.ShortName;
            Counterparty.INN = company.INN ?? Counterparty.INN;
            Counterparty.KPP = company.KPP;
            Counterparty.OGRN = company.OGRN;
            Counterparty.LegalAddress = company.LegalAddress?.Address;

            // Определяем тип
            SelectedType = "Юридическое лицо";

            // Обновляем UI
            OnPropertyChanged(nameof(Counterparty));
        }

        private void FillFromEntrepreneurData(CheckoEntrepreneurData entrepreneur)
        {
            Counterparty.FullName = entrepreneur.FullName ?? Counterparty.FullName;
            Counterparty.ShortName = entrepreneur.FullName ?? Counterparty.ShortName;
            Counterparty.INN = entrepreneur.INN ?? Counterparty.INN;
            Counterparty.OGRN = entrepreneur.OGRNIP;

            // Определяем тип
            SelectedType = "Индивидуальный предприниматель";

            // Обновляем UI
            OnPropertyChanged(nameof(Counterparty));
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Counterparty.FullName))
                {
                    MessageBox.Show(_window, "Введите полное наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Counterparty.ShortName))
                {
                    MessageBox.Show(_window, "Введите краткое наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка ИНН
                if (!string.IsNullOrWhiteSpace(Counterparty.INN))
                {
                    if (Counterparty.INN.Length != 10 && Counterparty.INN.Length != 12)
                    {
                        MessageBox.Show(_window, "ИНН должен содержать 10 или 12 цифр", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!await _counterpartyService.IsINNUniqueAsync(Counterparty.INN, Counterparty.Id > 0 ? Counterparty.Id : null))
                    {
                        MessageBox.Show(_window, $"Контрагент с ИНН {Counterparty.INN} уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (Counterparty.Id > 0)
                {
                    // Редактирование
                    await _counterpartyService.UpdateCounterpartyAsync(Counterparty);
                    MessageBox.Show(_window, "Контрагент успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _counterpartyService.CreateCounterpartyAsync(Counterparty);
                    MessageBox.Show(_window, "Контрагент успешно добавлен", "Успех",
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

        [RelayCommand]
        private void AddBankAccount()
        {
            var window = new CounterpartyBankAccountWindow();
            var viewModel = new CounterpartyBankAccountViewModel(
                _counterpartyService,
                _bikService,
                Counterparty.Id,
                null,
                window);

            window.DataContext = viewModel;
            window.Owner = _window;

            var result = window.ShowDialog();
            if (result == true)
            {
                _ = LoadBankAccountsAsync();
            }
        }

        [RelayCommand]
        private void EditBankAccount()
        {
            if (SelectedBankAccount == null)
            {
                MessageBox.Show(_window, "Выберите банковский счет для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new CounterpartyBankAccountWindow();
            var viewModel = new CounterpartyBankAccountViewModel(
                _counterpartyService,
                _bikService,
                Counterparty.Id,
                SelectedBankAccount,
                window);

            window.DataContext = viewModel;
            window.Owner = _window;

            var result = window.ShowDialog();
            if (result == true)
            {
                _ = LoadBankAccountsAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteBankAccountAsync()
        {
            if (SelectedBankAccount == null)
            {
                MessageBox.Show(_window, "Выберите банковский счет для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(_window,
                $"Вы действительно хотите удалить банковский счет {SelectedBankAccount.AccountNumber}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    var success = await _counterpartyService.DeleteBankAccountAsync(SelectedBankAccount.Id);
                    if (success)
                    {
                        await LoadBankAccountsAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_window, $"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}