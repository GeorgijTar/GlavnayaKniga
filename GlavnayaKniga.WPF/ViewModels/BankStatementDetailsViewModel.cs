using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using GlavnayaKniga.WPF.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class BankStatementDetailsViewModel : BaseViewModel
    {
        private readonly IBankStatementService _statementService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IEntryService _entryService;
        private readonly IAccountService _accountService;
        private readonly IRepository<TransactionBasis> _basisRepository;
        private readonly int _statementId;
        private readonly Window _window;

        [ObservableProperty]
        private BankStatementDto? _statement;

        [ObservableProperty]
        private ObservableCollection<BankStatementDocumentDto> _documents;

        [ObservableProperty]
        private BankStatementDocumentDto? _selectedDocument;

        [ObservableProperty]
        private BankAccountDto? _bankAccount;

        public BankStatementDetailsViewModel(
            IBankStatementService statementService,
            IBankAccountService bankAccountService,
            IEntryService entryService,
            IAccountService accountService,
            IRepository<TransactionBasis> basisRepository,
            int statementId,
            Window window)
        {
            _statementService = statementService;
            _bankAccountService = bankAccountService;
            _entryService = entryService;
            _accountService = accountService;
            _basisRepository = basisRepository;
            _statementId = statementId;
            _window = window;

            _documents = new ObservableCollection<BankStatementDocumentDto>();

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка деталей выписки...";

                Statement = await _statementService.GetStatementByIdAsync(_statementId);

                if (Statement != null)
                {
                    // Загружаем банковский счет
                    if (Statement.BankAccountId.HasValue)
                    {
                        BankAccount = await _bankAccountService.GetBankAccountByIdAsync(Statement.BankAccountId.Value);
                    }

                    Documents.Clear();
                    foreach (var doc in Statement.Documents.OrderBy(d => d.Date))
                    {
                        Documents.Add(doc);
                    }

                    StatusMessage = $"Загружено документов: {Documents.Count}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show(_window, $"Ошибка загрузки деталей выписки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateEntryForDocumentAsync(BankStatementDocumentDto? document)
        {
            try
            {
                // Проверяем, что документ передан
                if (document == null)
                {
                    // Если документ не передан через параметр, используем SelectedDocument
                    document = SelectedDocument;
                }

                if (document == null)
                {
                    MessageBox.Show(_window, "Выберите документ для создания проводки", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (document.EntryId.HasValue)
                {
                    var result = MessageBox.Show(_window,
                        "По этому документу уже создана проводка. Хотите создать еще одну?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                        return;
                }

                if (BankAccount == null)
                {
                    MessageBox.Show(_window, "Не найден банковский счет для выписки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var window = new BankStatementEntryWindow();
                var viewModel = new BankStatementEntryViewModel(
                    _entryService,
                    _accountService,
                    _basisRepository,
                    _statementService,
                    document,  // Передаем конкретный документ
                    BankAccount,
                    window);

                window.DataContext = viewModel;
                window.Owner = _window;

                var dialogResult = window.ShowDialog();

                if (dialogResult == true)
                {
                    // Обновляем данные
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка создания проводки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ViewEntryAsync(BankStatementDocumentDto? document)
        {
            try
            {
                if (document == null)
                {
                    document = SelectedDocument;
                }

                if (document?.EntryId == null)
                {
                    MessageBox.Show(_window, "По этому документу еще не создана проводка", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // TODO: Открыть просмотр проводки
                MessageBox.Show(_window, $"Проводка ID: {document.EntryId}\n\nФункция просмотра проводки будет добавлена позже.",
                    "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_window, $"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Close()
        {
            _window.Close();
        }
    }
}