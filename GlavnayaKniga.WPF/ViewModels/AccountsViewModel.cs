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

namespace GlavnayaKniga.WPF.ViewModels;

public partial class AccountsViewModel : BaseViewModel
{
    private readonly IAccountService _accountService;
    private readonly IWordExportService _wordExportService;
    private ObservableCollection<AccountDto> _allAccounts;


    [ObservableProperty]
    private ObservableCollection<AccountDto> _accounts;

    [ObservableProperty]
    private ObservableCollection<AccountDto> _filteredAccounts;

    [ObservableProperty]
    private object? _selectedAccount;

    [ObservableProperty]
    private bool _showArchivedAccounts;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public AccountsViewModel(IAccountService accountService, IWordExportService wordExportService)
    {
        _accountService = accountService;
        _wordExportService = wordExportService;
        _allAccounts = new ObservableCollection<AccountDto>();
        _accounts = new ObservableCollection<AccountDto>();
        _filteredAccounts = new ObservableCollection<AccountDto>();

        LoadAccountsAsync();
    }

    private AccountDto? SelectedAccountDto => SelectedAccount as AccountDto;

    partial void OnShowArchivedAccountsChanged(bool value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_allAccounts == null || !_allAccounts.Any())
            return;

        try
        {
            var filtered = _allAccounts.AsEnumerable();

            // Фильтр по архивным
            if (!ShowArchivedAccounts)
            {
                filtered = FilterNonArchived(filtered);
            }

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(a =>
                    a.Code.ToLower().Contains(searchLower) ||
                    a.Name.ToLower().Contains(searchLower) ||
                    (a.FullCode != null && a.FullCode.ToLower().Contains(searchLower)));
            }

            FilteredAccounts.Clear();
            foreach (var account in filtered)
            {
                FilteredAccounts.Add(account);
            }

            StatusMessage = $"Показано счетов: {FilteredAccounts.Count} из {_allAccounts.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка фильтрации: {ex.Message}";
        }
    }

    private IEnumerable<AccountDto> FilterNonArchived(IEnumerable<AccountDto> accounts)
    {
        foreach (var account in accounts)
        {
            if (!account.IsArchived)
            {
                // Рекурсивно фильтруем детей
                var nonArchivedChildren = FilterNonArchived(account.Children).ToList();

                if (nonArchivedChildren.Any())
                {
                    var filteredAccount = new AccountDto
                    {
                        Id = account.Id,
                        Code = account.Code,
                        Name = account.Name,
                        FullCode = account.FullCode,
                        ParentId = account.ParentId,
                        ParentCode = account.ParentCode,
                        IsArchived = account.IsArchived,
                        ArchivedAt = account.ArchivedAt,
                        CreatedAt = account.CreatedAt,
                        UpdatedAt = account.UpdatedAt
                    };

                    foreach (var child in nonArchivedChildren)
                    {
                        filteredAccount.Children.Add(child);
                    }

                    yield return filteredAccount;
                }
                else if (!account.IsArchived)
                {
                    yield return account;
                }
            }
        }
    }

    [RelayCommand]
    private async Task LoadAccountsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Загрузка счетов...";

            var accounts = await _accountService.GetAccountHierarchyAsync(true); // Загружаем все, включая архивные

            _allAccounts.Clear();
            Accounts.Clear();
            FilteredAccounts.Clear();

            foreach (var account in accounts)
            {
                _allAccounts.Add(account);
                Accounts.Add(account);
            }

            ApplyFilters();

            StatusMessage = $"Загружено счетов: {_allAccounts.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
            MessageBox.Show($"Ошибка загрузки счетов: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        try
        {
            var window = new AccountEditWindow();

            var viewModel = new AccountEditViewModel(
                _accountService,
                null,
                window);

            window.DataContext = viewModel;
            window.Owner = System.Windows.Application.Current.MainWindow;

            var result = window.ShowDialog();

            if (result == true)
            {
                await LoadAccountsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task EditAccountAsync()
    {
        try
        {
            var selectedDto = SelectedAccountDto;
            if (selectedDto == null)
            {
                MessageBox.Show("Выберите счет для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectedDto.IsArchived)
            {
                MessageBox.Show("Нельзя редактировать архивный счет", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var accountToEdit = await _accountService.GetAccountByIdAsync(selectedDto.Id);
            if (accountToEdit == null)
            {
                MessageBox.Show("Счет не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var window = new AccountEditWindow();

            var viewModel = new AccountEditViewModel(
                _accountService,
                accountToEdit,
                window);

            window.DataContext = viewModel;
            window.Owner = System.Windows.Application.Current.MainWindow;

            var result = window.ShowDialog();

            if (result == true)
            {
                await LoadAccountsAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ArchiveAccountAsync()
    {
        try
        {
            var selectedDto = SelectedAccountDto;
            if (selectedDto == null)
            {
                MessageBox.Show("Выберите счет для архивации", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectedDto.IsArchived)
            {
                MessageBox.Show("Счет уже в архиве", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверяем наличие дочерних счетов
            if (selectedDto.Children != null && selectedDto.Children.Any(c => !c.IsArchived))
            {
                MessageBox.Show(
                    "Нельзя архивировать счет, у которого есть неархивные дочерние счета. " +
                    "Сначала архивируйте дочерние счета.",
                    "Ошибка архивации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите архивировать счет {selectedDto.Code} {selectedDto.Name}?\n\n" +
                $"Архивные счета нельзя использовать в новых проводках, но они сохраняются в истории.",
                "Подтверждение архивации",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Архивация счета...";

                var success = await _accountService.ArchiveAccountAsync(selectedDto.Id);

                if (success)
                {
                    StatusMessage = "Счет успешно архивирован";
                    await LoadAccountsAsync();

                    MessageBox.Show($"Счет {selectedDto.Code} успешно архивирован", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Не удалось архивировать счет";
                    MessageBox.Show("Не удалось архивировать счет", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
    private async Task UnarchiveAccountAsync()
    {
        try
        {
            var selectedDto = SelectedAccountDto;
            if (selectedDto == null)
            {
                MessageBox.Show("Выберите счет для разархивации", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!selectedDto.IsArchived)
            {
                MessageBox.Show("Счет не в архиве", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверяем родительский счет
            if (selectedDto.ParentId.HasValue)
            {
                var parent = await _accountService.GetAccountByIdAsync(selectedDto.ParentId.Value);
                if (parent != null && parent.IsArchived)
                {
                    MessageBox.Show(
                        "Нельзя разархивировать счет, так как родительский счет находится в архиве. " +
                        "Сначала разархивируйте родительский счет.",
                        "Ошибка разархивации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите разархивировать счет {selectedDto.Code} {selectedDto.Name}?",
                "Подтверждение разархивации",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Разархивация счета...";

                var success = await _accountService.UnarchiveAccountAsync(selectedDto.Id);

                if (success)
                {
                    StatusMessage = "Счет успешно разархивирован";
                    await LoadAccountsAsync();

                    MessageBox.Show($"Счет {selectedDto.Code} успешно разархивирован", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Не удалось разархивировать счет";
                    MessageBox.Show("Не удалось разархивировать счет", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
    private async Task DeleteAccountAsync()
    {
        try
        {
            var selectedDto = SelectedAccountDto;
            if (selectedDto == null)
            {
                MessageBox.Show("Выберите счет для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!selectedDto.IsArchived)
            {
                // Проверяем, использовался ли счет в проводках
                var isUsed = await _accountService.IsAccountUsedInEntriesAsync(selectedDto.Id);

                if (isUsed)
                {
                    var archiveChoice = MessageBox.Show(
                        $"Счет {selectedDto.Code} {selectedDto.Name} использовался в проводках. Его нельзя удалить.\n\n" +
                        "Хотите архивировать счет?",
                        "Информация",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (archiveChoice == MessageBoxResult.Yes)
                    {
                        await ArchiveAccountAsync();
                    }
                    return;
                }

                // Если не использовался, предлагаем удалить или архивировать
                var choice = MessageBox.Show(
                    $"Счет {selectedDto.Code} {selectedDto.Name} не использовался в проводках.\n\n" +
                    "Что вы хотите сделать?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (choice == MessageBoxResult.Yes) // Да = Архивировать
                {
                    await ArchiveAccountAsync();
                    return;
                }
                else if (choice == MessageBoxResult.No) // Нет = Удалить
                {
                    // Переходим к удалению
                }
                else // Отмена
                {
                    return;
                }
            }

            // Удаление счета (архивного или неиспользованного неархивного)
            var confirmResult = MessageBox.Show(
                $"Вы действительно хотите удалить счет {selectedDto.Code} {selectedDto.Name}?\n\n" +
                "Это действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Удаление счета...";

                var success = await _accountService.DeleteAccountAsync(selectedDto.Id);

                if (success)
                {
                    StatusMessage = "Счет успешно удален";
                    await LoadAccountsAsync();

                    MessageBox.Show($"Счет {selectedDto.Code} успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Не удалось удалить счет";
                    MessageBox.Show("Не удалось удалить счет", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
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
        await LoadAccountsAsync();
    }


    [RelayCommand]
    private async Task ExportToWordAsync()
    {
        try
        {
            // Определяем, что экспортировать: все счета или только отфильтрованные
            var accountsToExport = _allAccounts; // Экспортируем все счета

            if (accountsToExport == null || !accountsToExport.Any())
            {
                MessageBox.Show("Нет счетов для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Настройки сохранения файла
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Documents|*.docx",
                DefaultExt = "docx",
                FileName = $"План счетов_{DateTime.Now:yyyy-MM-dd}.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                StatusMessage = "Экспорт в Word...";

                // Экспортируем счета
                var filePath = await _wordExportService.SaveAccountsToWordFileAsync(
                    accountsToExport,
                    saveFileDialog.FileName,
                    "План счетов");

                StatusMessage = $"Счета экспортированы в {filePath}";

                // Спрашиваем, открыть ли файл
                var openResult = MessageBox.Show(
                    "Счета успешно экспортированы. Открыть файл?",
                    "Экспорт завершен",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (openResult == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка экспорта: {ex.Message}";
            MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Добавляем команду для экспорта только видимых (отфильтрованных) счетов
    [RelayCommand]
    private async Task ExportFilteredToWordAsync()
    {
        try
        {
            var accountsToExport = FilteredAccounts;

            if (accountsToExport == null || !accountsToExport.Any())
            {
                MessageBox.Show("Нет счетов для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Documents|*.docx",
                DefaultExt = "docx",
                FileName = $"План счетов (отфильтрованный)_{DateTime.Now:yyyy-MM-dd}.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                StatusMessage = "Экспорт в Word...";

                var filePath = await _wordExportService.SaveAccountsToWordFileAsync(
                    accountsToExport,
                    saveFileDialog.FileName,
                    "План счетов (отфильтрованный)");

                StatusMessage = $"Счета экспортированы в {filePath}";

                var openResult = MessageBox.Show(
                    "Счета успешно экспортированы. Открыть файл?",
                    "Экспорт завершен",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (openResult == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка экспорта: {ex.Message}";
            MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}