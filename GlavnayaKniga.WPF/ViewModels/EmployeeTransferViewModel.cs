using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class EmployeeTransferViewModel : BaseViewModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPositionService _positionService;
        private readonly int _employeeId;
        private readonly Window _window;

        [ObservableProperty]
        private ObservableCollection<PositionDto> _positions;

        [ObservableProperty]
        private PositionDto? _selectedPosition;

        [ObservableProperty]
        private DateTime _transferDate;

        [ObservableProperty]
        private string _orderNumber = string.Empty;

        [ObservableProperty]
        private string _title;

        public EmployeeTransferViewModel(
            IEmployeeService employeeService,
            IPositionService positionService,
            int employeeId,
            Window window)
        {
            _employeeService = employeeService;
            _positionService = positionService;
            _employeeId = employeeId;
            _window = window;

            _positions = new ObservableCollection<PositionDto>();
            TransferDate = DateTime.Today;
            Title = "Перевод сотрудника на другую должность";

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка должностей...";

                var positions = await _positionService.GetAllPositionsAsync(false);
                Positions.Clear();
                foreach (var position in positions.OrderBy(p => p.Name))
                {
                    Positions.Add(position);
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

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                if (SelectedPosition == null)
                {
                    MessageBox.Show(_window, "Выберите должность для перевода", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(OrderNumber))
                {
                    MessageBox.Show(_window, "Введите номер приказа о переводе", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _employeeService.TransferEmployeeAsync(
                    _employeeId, SelectedPosition.Id, TransferDate, OrderNumber);

                if (result != null)
                {
                    MessageBox.Show(_window, "Сотрудник успешно переведен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _window.DialogResult = true;
                    _window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при переводе: {ex.Message}", "Ошибка",
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