using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class PositionEditViewModel : BaseViewModel
    {
        private readonly IPositionService _positionService;
        private readonly PositionDto? _originalPosition;
        private readonly Window _window;

        [ObservableProperty]
        private PositionDto _position;

        [ObservableProperty]
        private ObservableCollection<string> _categories;

        [ObservableProperty]
        private string _selectedCategory;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public PositionEditViewModel(
            IPositionService positionService,
            PositionDto? positionToEdit,
            Window window)
        {
            _positionService = positionService;
            _originalPosition = positionToEdit;
            _window = window;

            _categories = new ObservableCollection<string>
            {
                "Руководитель",
                "Специалист",
                "Рабочий",
                "Прочее"
            };

            if (_originalPosition != null)
            {
                _position = new PositionDto
                {
                    Id = _originalPosition.Id,
                    Name = _originalPosition.Name,
                    ShortName = _originalPosition.ShortName,
                    Category = _originalPosition.Category,
                    Description = _originalPosition.Description,
                    EducationRequirements = _originalPosition.EducationRequirements,
                    ExperienceYears = _originalPosition.ExperienceYears,
                    BaseSalary = _originalPosition.BaseSalary,
                    IsArchived = _originalPosition.IsArchived
                };
                _selectedCategory = _originalPosition.CategoryDisplay;
                Title = "Редактирование должности";
                IsEditMode = true;
            }
            else
            {
                _position = new PositionDto
                {
                    Category = "Specialist"
                };
                _selectedCategory = "Специалист";
                Title = "Добавление должности";
                IsEditMode = false;
            }
        }

        partial void OnSelectedCategoryChanged(string value)
        {
            Position.Category = value switch
            {
                "Руководитель" => "Manager",
                "Специалист" => "Specialist",
                "Рабочий" => "Worker",
                _ => "Other"
            };
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                IsBusy = true;

                // Валидация
                if (string.IsNullOrWhiteSpace(Position.Name))
                {
                    MessageBox.Show(_window, "Введите наименование должности", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности наименования
                if (!await _positionService.IsNameUniqueAsync(Position.Name, Position.Id > 0 ? Position.Id : null))
                {
                    MessageBox.Show(_window, $"Должность с наименованием '{Position.Name}' уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Position.Id > 0)
                {
                    // Редактирование
                    await _positionService.UpdatePositionAsync(Position);
                    MessageBox.Show(_window, "Должность успешно обновлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _positionService.CreatePositionAsync(Position);
                    MessageBox.Show(_window, "Должность успешно добавлена", "Успех",
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