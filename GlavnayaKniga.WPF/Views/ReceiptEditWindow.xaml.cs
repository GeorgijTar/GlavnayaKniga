using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GlavnayaKniga.Application.DTOs;

namespace GlavnayaKniga.WPF.Views
{
    public partial class ReceiptEditWindow : Window
    {
        public ReceiptEditWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var item = e.Row.Item as ReceiptItemDto;
                if (item == null) return;

                // Даем время на обновление binding
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (DataContext is ViewModels.ReceiptEditViewModel viewModel)
                    {
                        viewModel.RecalculateRow(item);
                        viewModel.RenumberLines();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // Обновляем DataGrid_AddingNewItem для установки фокуса на новую строку
        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (DataContext is ViewModels.ReceiptEditViewModel viewModel)
            {
                var newItem = new ReceiptItemDto
                {
                    ReceiptId = viewModel.Receipt.Id,
                    Quantity = 1,
                    VatRate = viewModel.SelectedVatRate,
                    LineNumber = viewModel.Items.Count + 1
                };

                e.NewItem = newItem;

                // Добавляем новый элемент в коллекцию вручную после создания
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    viewModel.Items.Add(newItem);
                    viewModel.SelectedItem = newItem;

                    // Находим и очищаем контрол поиска в новой строке
                    var dataGrid = sender as DataGrid;
                    if (dataGrid != null)
                    {
                        dataGrid.ScrollIntoView(newItem);
                        dataGrid.CurrentCell = new DataGridCellInfo(newItem, dataGrid.Columns[1]); // Колонка номенклатуры
                        dataGrid.BeginEdit();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            var item = comboBox.DataContext as ReceiptItemDto;
            if (item == null) return;

            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (DataContext is ViewModels.ReceiptEditViewModel viewModel)
                {
                    viewModel.RecalculateRow(item);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var item = textBox.DataContext as ReceiptItemDto;
            if (item == null) return;

            if (DataContext is ViewModels.ReceiptEditViewModel viewModel)
            {
                viewModel.RecalculateRow(item);
            }
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            // Можно использовать для дополнительной логики при смене ячейки
        }

        // Обработчики для контрола поиска номенклатуры
        private void NomenclatureSearch_Loaded(object sender, RoutedEventArgs e)
        {
            var searchControl = sender as Controls.NomenclatureSearchControl;
            if (searchControl != null)
            {
                // Очищаем контрол при загрузке для новой строки
                searchControl.Clear();
                searchControl.SetFocus();
            }
        }

        private void NomenclatureSearch_ItemSelected(object sender, NomenclatureDto nomenclature)
        {
            var searchControl = sender as Controls.NomenclatureSearchControl;
            if (searchControl == null) return;

            // Получаем родительский DataGrid
            var dataGrid = FindParent<DataGrid>(searchControl);
            if (dataGrid != null)
            {
                // Завершаем редактирование и переходим к следующей ячейке
                dataGrid.CommitEdit(DataGridEditingUnit.Row, true);

                // Перемещаем фокус на следующую колонку
                var currentCell = dataGrid.CurrentCell;
                var nextColumnIndex = currentCell.Column.DisplayIndex + 1;
                if (nextColumnIndex < dataGrid.Columns.Count)
                {
                    var nextColumn = dataGrid.Columns[nextColumnIndex];
                    var currentItem = dataGrid.CurrentItem;
                    if (currentItem != null)
                    {
                        dataGrid.CurrentCell = new DataGridCellInfo(currentItem, nextColumn);
                        dataGrid.BeginEdit();
                    }
                }
            }
        }

        // Обработчик для контрола поиска контрагента
        private void ContractorSearch_ItemSelected(object sender, CounterpartyDto counterparty)
        {
            // Дополнительная логика при выборе контрагента
            if (DataContext is ViewModels.ReceiptEditViewModel viewModel)
            {
                // Вызываем CalculateTotals для обновления итогов
                viewModel.CalculateTotals();
            }
        }

        // Вспомогательный метод для поиска родительского элемента
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }
    }
}