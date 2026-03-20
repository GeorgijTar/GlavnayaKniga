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
    public partial class PositionSearchControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<PositionDto>), typeof(PositionSearchControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(PositionDto), typeof(PositionSearchControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(PositionSearchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(PositionSearchControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(PositionSearchControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(PositionSearchControl),
                new PropertyMetadata(null));

        private List<PositionDto> _allItems = new();
        private bool _ignoreSelectionChange;
        private bool _ignoreTextChange;

        public IEnumerable<PositionDto> ItemsSource
        {
            get => (IEnumerable<PositionDto>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public PositionDto SelectedItem
        {
            get => (PositionDto)GetValue(SelectedItemProperty);
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

        public event EventHandler<PositionDto> ItemSelected;

        public PositionSearchControl()
        {
            InitializeComponent();

            ResultsListBox.ItemsSource = SearchResults;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PositionSearchControl)d;
            control._allItems = (e.NewValue as IEnumerable<PositionDto>)?.ToList() ?? new();

            if (control.SelectedItem != null)
            {
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = control.SelectedItem.Name ?? "";
                control._ignoreTextChange = false;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PositionSearchControl)d;
            if (!control._ignoreSelectionChange)
            {
                var item = e.NewValue as PositionDto;
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = item?.Name ?? "";
                control._ignoreTextChange = false;
                control.IsPopupOpen = false;
            }
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PositionSearchControl)d;
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
            var results = _allItems.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(searchText)) ||
                (p.ShortName != null && p.ShortName.ToLower().Contains(searchText)) ||
                (p.CategoryDisplay != null && p.CategoryDisplay.ToLower().Contains(searchText)))
                .Take(20)
                .ToList();

            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            IsPopupOpen = SearchResults.Any();
        }

        public ObservableCollection<PositionDto> SearchResults { get; } = new();

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
                        SelectItem(ResultsListBox.SelectedItem as PositionDto);
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
                SelectedItem = ResultsListBox.SelectedItem as PositionDto;
                _ignoreSelectionChange = false;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem != null)
            {
                SelectItem(ResultsListBox.SelectedItem as PositionDto);
            }
        }

        private void SelectItem(PositionDto item)
        {
            if (item != null)
            {
                _ignoreSelectionChange = true;
                _ignoreTextChange = true;
                SelectedItem = item;
                SearchTextBox.Text = item.Name ?? "";
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
                SelectItem(ResultsListBox.SelectedItem as PositionDto);
        });
    }
}