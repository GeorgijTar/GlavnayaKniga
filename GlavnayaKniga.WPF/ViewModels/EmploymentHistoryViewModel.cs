using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class EmploymentHistoryViewModel : BaseViewModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly int _employeeId;
        private readonly Window _window;

        [ObservableProperty]
        private EmployeeDto? _employee;

        [ObservableProperty]
        private ObservableCollection<EmploymentHistoryDto> _history;

        [ObservableProperty]
        private EmploymentHistoryDto? _selectedHistory;

        public EmploymentHistoryViewModel(
            IEmployeeService employeeService,
            int employeeId,
            Window window)
        {
            _employeeService = employeeService;
            _employeeId = employeeId;
            _window = window;

            _history = new ObservableCollection<EmploymentHistoryDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка истории...";

                Employee = await _employeeService.GetEmployeeByIdAsync(_employeeId);

                var history = await _employeeService.GetEmploymentHistoryAsync(_employeeId);
                History.Clear();
                foreach (var item in history.OrderByDescending(h => h.StartDate))
                {
                    History.Add(item);
                }

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show(_window, $"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Close()
        {
            _window.Close();
        }
    }
}