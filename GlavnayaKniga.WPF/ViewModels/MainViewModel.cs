using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace GlavnayaKniga.WPF.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private BaseViewModel? _currentViewModel;

        public ICommand ShowAccountsCommand { get; }
        public ICommand ShowEntriesCommand { get; }
        public ICommand ShowReportsCommand { get; }
        public ICommand ShowBankAccountsCommand { get; }
        public ICommand ShowBankStatementImportCommand { get; }
        public ICommand ShowCounterpartiesCommand { get; }
        public ICommand ShowNomenclatureCommand { get; }
        public ICommand ShowAssetTypesCommand { get; } 
        public ICommand ShowAssetsCommand { get; }
        public ICommand ShowStorageLocationsCommand { get; }
        public ICommand ShowReceiptsCommand { get; }
        public ICommand ShowIndividualsCommand { get; }
        public ICommand ShowPositionsCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowDepartmentsCommand { get; }
        public ICommand ShowUnitsOfMeasureCommand { get; }


        public MainViewModel(
            AccountsViewModel accountsViewModel,
            EntriesViewModel entriesViewModel,
            ReportsViewModel reportsViewModel,
            BankAccountsViewModel bankAccountsViewModel,
            BankStatementImportViewModel bankStatementImportViewModel,
            CounterpartiesViewModel counterpartiesViewModel,
             NomenclatureViewModel nomenclatureViewModel,
             AssetTypeViewModel assetTypeViewModel, 
            AssetsViewModel assetsViewModel,
            StorageLocationsViewModel storageLocationsViewModel,
            ReceiptsViewModel receiptsViewModel,
            IndividualsViewModel individualsViewModel,
            PositionsViewModel positionsViewModel,
            EmployeesViewModel employeesViewModel,
            DepartmentsViewModel departmentsViewModel,
            UnitsOfMeasureViewModel unitsOfMeasureViewModel)
        {
            ShowAccountsCommand = new RelayCommand(() =>
                CurrentViewModel = accountsViewModel);

            ShowEntriesCommand = new RelayCommand(() =>
                CurrentViewModel = entriesViewModel);

            ShowReportsCommand = new RelayCommand(() =>
                CurrentViewModel = reportsViewModel);

            ShowBankAccountsCommand = new RelayCommand(() =>
                CurrentViewModel = bankAccountsViewModel);

            ShowBankStatementImportCommand = new RelayCommand(() =>
                CurrentViewModel = bankStatementImportViewModel);

            ShowCounterpartiesCommand = new RelayCommand(() => 
               CurrentViewModel = counterpartiesViewModel);

            ShowNomenclatureCommand = new RelayCommand(() => 
                CurrentViewModel = nomenclatureViewModel);

            ShowAssetTypesCommand = new RelayCommand(() => 
                CurrentViewModel = assetTypeViewModel);

            ShowAssetsCommand = new RelayCommand(() => 
                CurrentViewModel = assetsViewModel);

            ShowStorageLocationsCommand = new RelayCommand(() => 
                CurrentViewModel = storageLocationsViewModel);

            ShowReceiptsCommand = new RelayCommand(() => 
                CurrentViewModel = receiptsViewModel);

            ShowIndividualsCommand = new RelayCommand(() =>
               CurrentViewModel = individualsViewModel);

            ShowPositionsCommand = new RelayCommand(() =>
                CurrentViewModel = positionsViewModel);

            ShowEmployeesCommand = new RelayCommand(() =>
                CurrentViewModel = employeesViewModel);

            ShowDepartmentsCommand = new RelayCommand(() => 
            CurrentViewModel = departmentsViewModel);

            ShowUnitsOfMeasureCommand = new RelayCommand(() => 
            CurrentViewModel = unitsOfMeasureViewModel);

            // По умолчанию показываем счета
            CurrentViewModel = accountsViewModel;
        }
    }
}