using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class IndividualEditViewModel : BaseViewModel
    {
        private readonly IIndividualService _individualService;
        private readonly IndividualDto? _originalIndividual;
        private readonly Window _window;

        [ObservableProperty]
        private IndividualDto _individual;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public IndividualEditViewModel(
            IIndividualService individualService,
            IndividualDto? individualToEdit,
            Window window)
        {
            _individualService = individualService;
            _originalIndividual = individualToEdit;
            _window = window;

            if (_originalIndividual != null)
            {
                _individual = new IndividualDto
                {
                    Id = _originalIndividual.Id,
                    LastName = _originalIndividual.LastName,
                    FirstName = _originalIndividual.FirstName,
                    MiddleName = _originalIndividual.MiddleName,
                    BirthDate = _originalIndividual.BirthDate,
                    BirthPlace = _originalIndividual.BirthPlace,
                    Gender = _originalIndividual.Gender,
                    Citizenship = _originalIndividual.Citizenship,
                    RegistrationAddress = _originalIndividual.RegistrationAddress,
                    ActualAddress = _originalIndividual.ActualAddress,
                    Phone = _originalIndividual.Phone,
                    Email = _originalIndividual.Email,
                    PassportSeries = _originalIndividual.PassportSeries,
                    PassportNumber = _originalIndividual.PassportNumber,
                    PassportIssueDate = _originalIndividual.PassportIssueDate,
                    PassportIssuedBy = _originalIndividual.PassportIssuedBy,
                    PassportDepartmentCode = _originalIndividual.PassportDepartmentCode,
                    INN = _originalIndividual.INN,
                    SNILS = _originalIndividual.SNILS,
                    Note = _originalIndividual.Note,
                    IsArchived = _originalIndividual.IsArchived
                };
                Title = "Редактирование физического лица";
                IsEditMode = true;
            }
            else
            {
                _individual = new IndividualDto();
                Title = "Добавление физического лица";
                IsEditMode = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Individual.LastName))
                {
                    MessageBox.Show(_window, "Введите фамилию", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Individual.FirstName))
                {
                    MessageBox.Show(_window, "Введите имя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка ИНН
                if (!string.IsNullOrWhiteSpace(Individual.INN))
                {
                    if (Individual.INN.Length != 12)
                    {
                        MessageBox.Show(_window, "ИНН физического лица должен содержать 12 цифр", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!await _individualService.IsINNUniqueAsync(Individual.INN, Individual.Id > 0 ? Individual.Id : null))
                    {
                        MessageBox.Show(_window, $"Физическое лицо с ИНН {Individual.INN} уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Проверка СНИЛС
                if (!string.IsNullOrWhiteSpace(Individual.SNILS))
                {
                    if (Individual.SNILS.Length != 14) // Формат XXX-XXX-XXX XX
                    {
                        MessageBox.Show(_window, "СНИЛС должен быть в формате XXX-XXX-XXX XX", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!await _individualService.IsSNILSUniqueAsync(Individual.SNILS, Individual.Id > 0 ? Individual.Id : null))
                    {
                        MessageBox.Show(_window, $"Физическое лицо с СНИЛС {Individual.SNILS} уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (Individual.Id > 0)
                {
                    // Редактирование
                    await _individualService.UpdateIndividualAsync(Individual);
                    MessageBox.Show(_window, "Физическое лицо успешно обновлено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _individualService.CreateIndividualAsync(Individual);
                    MessageBox.Show(_window, "Физическое лицо успешно добавлено", "Успех",
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