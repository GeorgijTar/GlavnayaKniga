using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GlavnayaKniga.Application.DTOs;

namespace GlavnayaKniga.WPF.Controls
{
    public partial class CounterpartySearchControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<CounterpartyDto>), typeof(CounterpartySearchControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(CounterpartyDto), typeof(CounterpartySearchControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(CounterpartySearchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(CounterpartySearchControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(CounterpartySearchControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(CounterpartySearchControl),
                new PropertyMetadata(null));

        private List<CounterpartyDto> _allItems = new();
        private bool _ignoreSelectionChange;
        private bool _ignoreTextChange;

        public IEnumerable<CounterpartyDto> ItemsSource
        {
            get => (IEnumerable<CounterpartyDto>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public CounterpartyDto SelectedItem
        {
            get => (CounterpartyDto)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public bool IsPopupOpen
        {
            get => (bool)GetValue(IsPopupOpenProperty);
            set => SetValue(IsPopupOpenProperty, value);
        }

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public ICommand EditCommand
        {
            get => (ICommand)GetValue(EditCommandProperty);
            set => SetValue(EditCommandProperty, value);
        }

        public event EventHandler<CounterpartyDto> ItemSelected;

        public CounterpartySearchControl()
        {
            InitializeComponent();

            // Устанавливаем DataContext для привязок внутри контрола
            ResultsListBox.ItemsSource = SearchResults;

            // Подписываемся на изменение текста
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CounterpartySearchControl)d;
            control._allItems = (e.NewValue as IEnumerable<CounterpartyDto>)?.ToList() ?? new();

            // Если есть выбранный элемент, обновляем текст поиска
            if (control.SelectedItem != null)
            {
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = control.SelectedItem.ShortName ?? control.SelectedItem.FullName ?? "";
                control._ignoreTextChange = false;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CounterpartySearchControl)d;
            if (!control._ignoreSelectionChange)
            {
                var item = e.NewValue as CounterpartyDto;
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = item?.ShortName ?? item?.FullName ?? "";
                control._ignoreTextChange = false;
                control.IsPopupOpen = false;
            }
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CounterpartySearchControl)d;
            if (!control._ignoreTextChange)
            {
                control.SearchTextBox.Text = e.NewValue as string ?? "";
            }
        }

        private void UpdateSearchResults()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || SearchTextBox.Text.Length < 1)
            {
                IsPopupOpen = false;
                return;
            }

            var searchText = SearchTextBox.Text.ToLower();
            var results = _allItems.Where(c =>
                (c.ShortName != null && c.ShortName.ToLower().Contains(searchText)) ||
                (c.FullName != null && c.FullName.ToLower().Contains(searchText)) ||
                (c.INN != null && c.INN.Contains(SearchTextBox.Text)) ||
                (c.KPP != null && c.KPP.Contains(SearchTextBox.Text)))
                .Take(20)
                .ToList();

            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            // Открываем попап, если есть результаты
            IsPopupOpen = SearchResults.Any();
        }

        public ObservableCollection<CounterpartyDto> SearchResults { get; } = new();

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_ignoreTextChange)
            {
                // Обновляем SearchText
                SetCurrentValue(SearchTextProperty, SearchTextBox.Text);
                UpdateSearchResults();
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (ResultsListBox.Items.Count > 0)
                    {
                        ResultsListBox.SelectedIndex = 0;
                        ResultsListBox.ScrollIntoView(ResultsListBox.SelectedItem);
                        e.Handled = true;
                    }
                    break;

                case Key.Up:
                    if (ResultsListBox.SelectedIndex > 0)
                    {
                        ResultsListBox.SelectedIndex--;
                        ResultsListBox.ScrollIntoView(ResultsListBox.SelectedItem);
                        e.Handled = true;
                    }
                    break;

                case Key.Enter:
                    if (ResultsListBox.SelectedItem != null)
                    {
                        SelectItem(ResultsListBox.SelectedItem as CounterpartyDto);
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    IsPopupOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!ResultsListBox.IsKeyboardFocusWithin && !SearchTextBox.IsKeyboardFocusWithin)
                {
                    IsPopupOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsListBox.SelectedItem != null && !_ignoreSelectionChange)
            {
                _ignoreSelectionChange = true;
                SelectedItem = ResultsListBox.SelectedItem as CounterpartyDto;
                _ignoreSelectionChange = false;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem != null)
            {
                SelectItem(ResultsListBox.SelectedItem as CounterpartyDto);
            }
        }

        private void SelectItem(CounterpartyDto item)
        {
            if (item != null)
            {
                _ignoreSelectionChange = true;
                _ignoreTextChange = true;
                SelectedItem = item;
                SearchTextBox.Text = item.ShortName ?? item.FullName ?? "";
                SetCurrentValue(SearchTextProperty, SearchTextBox.Text);
                _ignoreTextChange = false;
                _ignoreSelectionChange = false;
                IsPopupOpen = false;
                ItemSelected?.Invoke(this, item);
            }
        }

        private ICommand _showDropdownCommand;
        public ICommand ShowDropdownCommand => _showDropdownCommand ??= new RelayCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                IsPopupOpen = true;
        });

        private ICommand _hideDropdownCommand;
        public ICommand HideDropdownCommand => _hideDropdownCommand ??= new RelayCommand(() =>
            IsPopupOpen = false);

        private ICommand _selectCurrentCommand;
        public ICommand SelectCurrentCommand => _selectCurrentCommand ??= new RelayCommand(() =>
        {
            if (ResultsListBox.SelectedItem != null)
                SelectItem(ResultsListBox.SelectedItem as CounterpartyDto);
        });
    }

   
}