using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class UnitOfMeasureEditViewModel : BaseViewModel
    {
        private readonly IUnitOfMeasureService _unitService;
        private readonly UnitOfMeasureDto? _originalUnit;
        private readonly Window _window;

        [ObservableProperty]
        private UnitOfMeasureDto _unit;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public UnitOfMeasureEditViewModel(
            IUnitOfMeasureService unitService,
            UnitOfMeasureDto? unitToEdit,
            Window window)
        {
            _unitService = unitService;
            _originalUnit = unitToEdit;
            _window = window;

            if (_originalUnit != null)
            {
                _unit = new UnitOfMeasureDto
                {
                    Id = _originalUnit.Id,
                    Code = _originalUnit.Code,
                    ShortName = _originalUnit.ShortName,
                    FullName = _originalUnit.FullName,
                    InternationalCode = _originalUnit.InternationalCode,
                    Description = _originalUnit.Description,
                    IsArchived = _originalUnit.IsArchived
                };
                Title = "Редактирование единицы измерения";
                IsEditMode = true;
            }
            else
            {
                _unit = new UnitOfMeasureDto();
                Title = "Добавление единицы измерения";
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
                if (string.IsNullOrWhiteSpace(Unit.Code))
                {
                    MessageBox.Show(_window, "Введите код единицы измерения", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Unit.ShortName))
                {
                    MessageBox.Show(_window, "Введите краткое наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Unit.FullName))
                {
                    MessageBox.Show(_window, "Введите полное наименование", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности кода
                if (!await _unitService.IsCodeUniqueAsync(Unit.Code, Unit.Id > 0 ? Unit.Id : null))
                {
                    MessageBox.Show(_window, $"Код '{Unit.Code}' уже используется", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Unit.Id > 0)
                {
                    // Редактирование
                    await _unitService.UpdateUnitAsync(Unit);
                    MessageBox.Show(_window, "Единица измерения успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _unitService.CreateUnitAsync(Unit);
                    MessageBox.Show(_window, "Единица измерения успешно добавлена", "Успех",
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