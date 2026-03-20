using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class EmployeeDismissViewModel : BaseViewModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly int _employeeId;
        private readonly Window _window;

        [ObservableProperty]
        private DateTime _dismissalDate;

        [ObservableProperty]
        private string _orderNumber = string.Empty;

        [ObservableProperty]
        private string _reason = string.Empty;

        [ObservableProperty]
        private string _title;

        public EmployeeDismissViewModel(
            IEmployeeService employeeService,
            int employeeId,
            Window window)
        {
            _employeeService = employeeService;
            _employeeId = employeeId;
            _window = window;

            DismissalDate = DateTime.Today;
            Title = "Увольнение сотрудника";
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(OrderNumber))
                {
                    MessageBox.Show(_window, "Введите номер приказа об увольнении", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Reason))
                {
                    MessageBox.Show(_window, "Введите причину увольнения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _employeeService.DismissEmployeeAsync(
                    _employeeId, DismissalDate, OrderNumber, Reason);

                if (result != null)
                {
                    MessageBox.Show(_window, "Сотрудник успешно уволен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _window.DialogResult = true;
                    _window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка при увольнении: {ex.Message}", "Ошибка",
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