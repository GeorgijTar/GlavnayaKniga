using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class CounterpartiesViewModel : BaseViewModel
    {
        private readonly ICounterpartyService _counterpartyService;
        private readonly IBikService _bikService;
        private readonly ICheckoService _checkoService; // Добавляем поле

        [ObservableProperty]
        private ObservableCollection<CounterpartyDto> _counterparties;

        [ObservableProperty]
        private ObservableCollection<CounterpartyDto> _filteredCounterparties;

        [ObservableProperty]
        private CounterpartyDto? _selectedCounterparty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        public CounterpartiesViewModel(
            ICounterpartyService counterpartyService,
            IBikService bikService,
            ICheckoService checkoService) // Добавляем параметр
        {
            _counterpartyService = counterpartyService;
            _bikService = bikService;
            _checkoService = checkoService; // Инициализируем поле
            _counterparties = new ObservableCollection<CounterpartyDto>();
            _filteredCounterparties = new ObservableCollection<CounterpartyDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка контрагентов...";

                var counterparties = await _counterpartyService.GetAllCounterpartiesAsync(ShowArchived);

                Counterparties.Clear();
                foreach (var counterparty in counterparties.OrderBy(c => c.ShortName))
                {
                    Counterparties.Add(counterparty);
                }

                ApplyFilter();

                StatusMessage = $"Загружено контрагентов: {Counterparties.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки контрагентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        partial void OnShowArchivedChanged(bool value)
        {
            _ = LoadDataAsync();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCounterparties.Clear();
                foreach (var item in Counterparties)
                {
                    FilteredCounterparties.Add(item);
                }
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = Counterparties.Where(c =>
                c.ShortName.ToLower().Contains(searchLower) ||
                c.FullName.ToLower().Contains(searchLower) ||
                (c.INN != null && c.INN.Contains(searchLower)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)));

            FilteredCounterparties.Clear();
            foreach (var item in filtered)
            {
                FilteredCounterparties.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddCounterpartyAsync()
        {
            try
            {
                var window = new CounterpartyEditWindow();
                var viewModel = new CounterpartyEditViewModel(
                    _counterpartyService,  // 1. ICounterpartyService
                    _bikService,           // 2. IBikService
                    _checkoService,        // 3. ICheckoService
                    null,                  // 4. CounterpartyDto? (null для нового)
                    window);               // 5. Window

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditCounterpartyAsync()
        {
            try
            {
                if (SelectedCounterparty == null)
                {
                    MessageBox.Show("Выберите контрагента для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var counterpartyToEdit = await _counterpartyService.GetCounterpartyByIdAsync(SelectedCounterparty.Id);
                if (counterpartyToEdit == null)
                {
                    MessageBox.Show("Контрагент не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new CounterpartyEditWindow();
                var viewModel = new CounterpartyEditViewModel(
                    _counterpartyService,  // 1. ICounterpartyService
                    _bikService,           // 2. IBikService
                    _checkoService,        // 3. ICheckoService
                    counterpartyToEdit,    // 4. CounterpartyDto? (редактируемый)
                    window);               // 5. Window

                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;

                var result = window.ShowDialog();
                if (result == true)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ArchiveCounterpartyAsync()
        {
            try
            {
                if (SelectedCounterparty == null)
                {
                    MessageBox.Show("Выберите контрагента для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedCounterparty.IsArchived)
                {
                    MessageBox.Show("Контрагент уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать контрагента {SelectedCounterparty.ShortName}?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация контрагента...";

                    var success = await _counterpartyService.ArchiveCounterpartyAsync(SelectedCounterparty.Id);

                    if (success)
                    {
                        StatusMessage = "Контрагент успешно архивирован";
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при архивации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UnarchiveCounterpartyAsync()
        {
            try
            {
                if (SelectedCounterparty == null)
                {
                    MessageBox.Show("Выберите контрагента для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedCounterparty.IsArchived)
                {
                    MessageBox.Show("Контрагент не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать контрагента {SelectedCounterparty.ShortName}?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация контрагента...";

                    var success = await _counterpartyService.UnarchiveCounterpartyAsync(SelectedCounterparty.Id);

                    if (success)
                    {
                        StatusMessage = "Контрагент успешно разархивирован";
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при разархивации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }
    }
}