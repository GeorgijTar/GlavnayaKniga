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
    public partial class NomenclatureSearchControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<NomenclatureDto>), typeof(NomenclatureSearchControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(NomenclatureDto), typeof(NomenclatureSearchControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(NomenclatureSearchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(NomenclatureSearchControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(NomenclatureSearchControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(NomenclatureSearchControl),
                new PropertyMetadata(null));

        private List<NomenclatureDto> _allItems = new();
        private bool _ignoreSelectionChange;
        private bool _ignoreTextChange;

        public IEnumerable<NomenclatureDto> ItemsSource
        {
            get => (IEnumerable<NomenclatureDto>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public NomenclatureDto SelectedItem
        {
            get => (NomenclatureDto)GetValue(SelectedItemProperty);
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

        public event EventHandler<NomenclatureDto> ItemSelected;

        public NomenclatureSearchControl()
        {
            InitializeComponent();

            ResultsListBox.ItemsSource = SearchResults;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NomenclatureSearchControl)d;
            control._allItems = (e.NewValue as IEnumerable<NomenclatureDto>)?.ToList() ?? new();

            if (control.SelectedItem != null)
            {
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = control.SelectedItem.DisplayName ?? "";
                control._ignoreTextChange = false;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NomenclatureSearchControl)d;
            if (!control._ignoreSelectionChange)
            {
                var item = e.NewValue as NomenclatureDto;
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = item?.DisplayName ?? "";
                control._ignoreTextChange = false;
                control.IsPopupOpen = false;
            }
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NomenclatureSearchControl)d;
            if (!control._ignoreTextChange)
            {
                control.SearchTextBox.Text = e.NewValue as string ?? "";
            }
        }

        private void UpdateSearchResults()
        {
            SearchResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || SearchTextBox.Text.Length < 2)
            {
                IsPopupOpen = false;
                return;
            }

            var searchText = SearchTextBox.Text.ToLower();
            var results = _allItems.Where(n =>
                (n.Name != null && n.Name.ToLower().Contains(searchText)) ||
                (n.Article != null && n.Article.ToLower().Contains(searchText)) ||
                (n.FullName != null && n.FullName.ToLower().Contains(searchText)) ||
                (n.Barcode != null && n.Barcode.Contains(SearchTextBox.Text)))
                .Take(20)
                .ToList();

            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            IsPopupOpen = SearchResults.Any();
        }

        public ObservableCollection<NomenclatureDto> SearchResults { get; } = new();

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_ignoreTextChange)
            {
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
                        SelectItem(ResultsListBox.SelectedItem as NomenclatureDto);
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
                SelectedItem = ResultsListBox.SelectedItem as NomenclatureDto;
                _ignoreSelectionChange = false;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem != null)
            {
                SelectItem(ResultsListBox.SelectedItem as NomenclatureDto);
            }
        }

        private void SelectItem(NomenclatureDto item)
        {
            if (item != null)
            {
                _ignoreSelectionChange = true;
                _ignoreTextChange = true;
                SelectedItem = item;
                SearchTextBox.Text = item.DisplayName ?? "";
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
                SelectItem(ResultsListBox.SelectedItem as NomenclatureDto);
        });

        // Добавляем публичный метод для очистки контрола
        public void Clear()
        {
            _ignoreTextChange = true;
            _ignoreSelectionChange = true;
            SearchTextBox.Text = string.Empty;
            SetCurrentValue(SearchTextProperty, string.Empty);
            SelectedItem = null;
            SearchResults.Clear();
            IsPopupOpen = false;
            _ignoreTextChange = false;
            _ignoreSelectionChange = false;
        }

        // Добавляем обработчик для потери фокуса при добавлении новой строки
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            // Не закрываем попап сразу, даем время на клик
        }

        // Добавляем метод для установки фокуса
        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}