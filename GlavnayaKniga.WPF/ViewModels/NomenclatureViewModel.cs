using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class NomenclatureViewModel : BaseViewModel
    {
        private readonly INomenclatureService _nomenclatureService;
        private readonly IAccountService _accountService;
        private readonly IStorageLocationService _storageLocationService;
        private readonly IUnitOfMeasureService _unitService;

        [ObservableProperty]
        private ObservableCollection<NomenclatureDto> _nomenclatures;

        [ObservableProperty]
        private ObservableCollection<NomenclatureDto> _filteredNomenclatures;

        [ObservableProperty]
        private NomenclatureDto? _selectedNomenclature;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private ObservableCollection<string> _typeFilters;

        [ObservableProperty]
        private string? _selectedTypeFilter;

        public NomenclatureViewModel(
            INomenclatureService nomenclatureService,
            IAccountService accountService,             
            IStorageLocationService storageLocationService,
            IUnitOfMeasureService unitService)
        {
            _nomenclatureService = nomenclatureService;
            _accountService = accountService;
            _storageLocationService = storageLocationService;
            _unitService = unitService;
            _nomenclatures = new ObservableCollection<NomenclatureDto>();
            _filteredNomenclatures = new ObservableCollection<NomenclatureDto>();

            _typeFilters = new ObservableCollection<string>
            {
                "Все типы",
                "Материалы",
                "Инвентарь",
                "Удобрения",
                "СЗР",
                "Семена",
                "Топливо",
                "Запчасти",
                "Оборудование",
                "Прочее"
            };

            _selectedTypeFilter = "Все типы";

            LoadDataAsync();
            _storageLocationService = storageLocationService;
            _unitService = unitService;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка номенклатуры...";

                var items = await _nomenclatureService.GetAllNomenclatureAsync(ShowArchived);

                Nomenclatures.Clear();
                foreach (var item in items.OrderBy(n => n.Name))
                {
                    Nomenclatures.Add(item);
                }

                ApplyFilter();

                StatusMessage = $"Загружено позиций: {Nomenclatures.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки номенклатуры: {ex.Message}", "Ошибка",
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

        partial void OnSelectedTypeFilterChanged(string? value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = Nomenclatures.AsEnumerable();

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(n =>
                    n.Name.ToLower().Contains(searchLower) ||
                    (n.Article != null && n.Article.ToLower().Contains(searchLower)) ||
                    (n.Barcode != null && n.Barcode.Contains(SearchText)));
            }

            // Фильтр по типу
            if (!string.IsNullOrWhiteSpace(SelectedTypeFilter) && SelectedTypeFilter != "Все типы")
            {
                filtered = filtered.Where(n => n.TypeDisplay == SelectedTypeFilter);
            }

            FilteredNomenclatures.Clear();
            foreach (var item in filtered)
            {
                FilteredNomenclatures.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddNomenclatureAsync()
        {
            try
            {
                var window = new NomenclatureEditWindow();
                var viewModel = new NomenclatureEditViewModel(
                    _nomenclatureService,
                    _accountService,
                    _storageLocationService,
                    _unitService,
                    null,
                    window);

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
        private async Task EditNomenclatureAsync()
        {
            try
            {
                if (SelectedNomenclature == null)
                {
                    MessageBox.Show("Выберите позицию для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var itemToEdit = await _nomenclatureService.GetNomenclatureByIdAsync(SelectedNomenclature.Id);
                if (itemToEdit == null)
                {
                    MessageBox.Show("Позиция не найдена", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new NomenclatureEditWindow();
                var viewModel = new NomenclatureEditViewModel(
                    _nomenclatureService,
                    _accountService,
                    _storageLocationService,
                    _unitService,
                    itemToEdit,
                    window);

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
        private async Task ArchiveNomenclatureAsync()
        {
            try
            {
                if (SelectedNomenclature == null)
                {
                    MessageBox.Show("Выберите позицию для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedNomenclature.IsArchived)
                {
                    MessageBox.Show("Позиция уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать позицию {SelectedNomenclature.Name}?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация...";

                    var success = await _nomenclatureService.ArchiveNomenclatureAsync(SelectedNomenclature.Id);

                    if (success)
                    {
                        StatusMessage = "Позиция успешно архивирована";
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
        private async Task UnarchiveNomenclatureAsync()
        {
            try
            {
                if (SelectedNomenclature == null)
                {
                    MessageBox.Show("Выберите позицию для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedNomenclature.IsArchived)
                {
                    MessageBox.Show("Позиция не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать позицию {SelectedNomenclature.Name}?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация...";

                    var success = await _nomenclatureService.UnarchiveNomenclatureAsync(SelectedNomenclature.Id);

                    if (success)
                    {
                        StatusMessage = "Позиция успешно разархивирована";
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