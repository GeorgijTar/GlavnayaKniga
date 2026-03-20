using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class AssetTypeEditViewModel : BaseViewModel
    {
        private readonly IAssetTypeService _assetTypeService;
        private readonly AssetTypeDto? _originalType;
        private readonly Window _window;

        [ObservableProperty]
        private AssetTypeDto _type;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isEditMode;

        public AssetTypeEditViewModel(
            IAssetTypeService assetTypeService,
            AssetTypeDto? typeToEdit,
            Window window)
        {
            _assetTypeService = assetTypeService;
            _originalType = typeToEdit;
            _window = window;

            if (_originalType != null)
            {
                _type = new AssetTypeDto
                {
                    Id = _originalType.Id,
                    Name = _originalType.Name,
                    Description = _originalType.Description,
                    IsArchived = _originalType.IsArchived
                };
                Title = "Редактирование типа объекта";
                IsEditMode = true;
            }
            else
            {
                _type = new AssetTypeDto();
                Title = "Добавление типа объекта";
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
                if (string.IsNullOrWhiteSpace(Type.Name))
                {
                    MessageBox.Show(_window, "Введите наименование типа", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка уникальности наименования
                if (!await _assetTypeService.IsNameUniqueAsync(Type.Name, Type.Id > 0 ? Type.Id : null))
                {
                    MessageBox.Show(_window, $"Тип с наименованием '{Type.Name}' уже существует", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Type.Id > 0)
                {
                    // Редактирование
                    await _assetTypeService.UpdateAssetTypeAsync(Type);
                    MessageBox.Show(_window, "Тип успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление
                    await _assetTypeService.CreateAssetTypeAsync(Type);
                    MessageBox.Show(_window, "Тип успешно добавлен", "Успех",
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