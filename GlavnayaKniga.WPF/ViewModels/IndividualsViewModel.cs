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
    public partial class IndividualsViewModel : BaseViewModel
    {
        private readonly IIndividualService _individualService;

        [ObservableProperty]
        private ObservableCollection<IndividualDto> _individuals;

        [ObservableProperty]
        private ObservableCollection<IndividualDto> _filteredIndividuals;

        [ObservableProperty]
        private IndividualDto? _selectedIndividual;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showArchived;

        public IndividualsViewModel(IIndividualService individualService)
        {
            _individualService = individualService;
            _individuals = new ObservableCollection<IndividualDto>();
            _filteredIndividuals = new ObservableCollection<IndividualDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка физических лиц...";

                var individuals = await _individualService.GetAllIndividualsAsync(ShowArchived);

                Individuals.Clear();
                foreach (var individual in individuals.OrderBy(i => i.LastName))
                {
                    Individuals.Add(individual);
                }

                ApplyFilter();

                StatusMessage = $"Загружено: {Individuals.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки физических лиц: {ex.Message}", "Ошибка",
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
                FilteredIndividuals.Clear();
                foreach (var item in Individuals)
                {
                    FilteredIndividuals.Add(item);
                }
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = Individuals.Where(i =>
                i.LastName.ToLower().Contains(searchLower) ||
                i.FirstName.ToLower().Contains(searchLower) ||
                (i.MiddleName != null && i.MiddleName.ToLower().Contains(searchLower)) ||
                (i.INN != null && i.INN.Contains(SearchText)) ||
                (i.Phone != null && i.Phone.Contains(SearchText)));

            FilteredIndividuals.Clear();
            foreach (var item in filtered)
            {
                FilteredIndividuals.Add(item);
            }
        }

        [RelayCommand]
        private async Task AddIndividualAsync()
        {
            try
            {
                var window = new IndividualEditWindow();
                var viewModel = new IndividualEditViewModel(_individualService, null, window);

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
        private async Task EditIndividualAsync()
        {
            try
            {
                if (SelectedIndividual == null)
                {
                    MessageBox.Show("Выберите физическое лицо для редактирования", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var individualToEdit = await _individualService.GetIndividualByIdAsync(SelectedIndividual.Id);
                if (individualToEdit == null)
                {
                    MessageBox.Show("Физическое лицо не найдено", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new IndividualEditWindow();
                var viewModel = new IndividualEditViewModel(_individualService, individualToEdit, window);

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
        private async Task ArchiveIndividualAsync()
        {
            try
            {
                if (SelectedIndividual == null)
                {
                    MessageBox.Show("Выберите физическое лицо для архивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (SelectedIndividual.IsArchived)
                {
                    MessageBox.Show("Физическое лицо уже в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите архивировать {SelectedIndividual.FullName}?",
                    "Подтверждение архивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Архивация...";

                    var success = await _individualService.ArchiveIndividualAsync(SelectedIndividual.Id);

                    if (success)
                    {
                        StatusMessage = "Физическое лицо архивировано";
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
        private async Task UnarchiveIndividualAsync()
        {
            try
            {
                if (SelectedIndividual == null)
                {
                    MessageBox.Show("Выберите физическое лицо для разархивации", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!SelectedIndividual.IsArchived)
                {
                    MessageBox.Show("Физическое лицо не в архиве", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разархивировать {SelectedIndividual.FullName}?",
                    "Подтверждение разархивации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsBusy = true;
                    StatusMessage = "Разархивация...";

                    var success = await _individualService.UnarchiveIndividualAsync(SelectedIndividual.Id);

                    if (success)
                    {
                        StatusMessage = "Физическое лицо разархивировано";
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