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
    public partial class IndividualSearchControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<IndividualDto>), typeof(IndividualSearchControl),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(IndividualDto), typeof(IndividualSearchControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(IndividualSearchControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

        public static readonly DependencyProperty IsPopupOpenProperty =
            DependencyProperty.Register("IsPopupOpen", typeof(bool), typeof(IndividualSearchControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(IndividualSearchControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EditCommandProperty =
            DependencyProperty.Register("EditCommand", typeof(ICommand), typeof(IndividualSearchControl),
                new PropertyMetadata(null));

        private List<IndividualDto> _allItems = new();
        private bool _ignoreSelectionChange;
        private bool _ignoreTextChange;

        public IEnumerable<IndividualDto> ItemsSource
        {
            get => (IEnumerable<IndividualDto>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public IndividualDto SelectedItem
        {
            get => (IndividualDto)GetValue(SelectedItemProperty);
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

        public event EventHandler<IndividualDto> ItemSelected;

        public IndividualSearchControl()
        {
            InitializeComponent();

            ResultsListBox.ItemsSource = SearchResults;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IndividualSearchControl)d;
            control._allItems = (e.NewValue as IEnumerable<IndividualDto>)?.ToList() ?? new();

            if (control.SelectedItem != null)
            {
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = control.SelectedItem.ShortName ?? "";
                control._ignoreTextChange = false;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IndividualSearchControl)d;
            if (!control._ignoreSelectionChange)
            {
                var item = e.NewValue as IndividualDto;
                control._ignoreTextChange = true;
                control.SearchTextBox.Text = item?.ShortName ?? "";
                control._ignoreTextChange = false;
                control.IsPopupOpen = false;
            }
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IndividualSearchControl)d;
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
            var results = _allItems.Where(i =>
                (i.LastName != null && i.LastName.ToLower().Contains(searchText)) ||
                (i.FirstName != null && i.FirstName.ToLower().Contains(searchText)) ||
                (i.MiddleName != null && i.MiddleName.ToLower().Contains(searchText)) ||
                (i.INN != null && i.INN.Contains(SearchTextBox.Text)) ||
                (i.SNILS != null && i.SNILS.Contains(SearchTextBox.Text)) ||
                (i.Phone != null && i.Phone.Contains(SearchTextBox.Text)) ||
                (i.Email != null && i.Email.ToLower().Contains(searchText)))
                .Take(20)
                .ToList();

            foreach (var item in results)
            {
                SearchResults.Add(item);
            }

            IsPopupOpen = SearchResults.Any();
        }

        public ObservableCollection<IndividualDto> SearchResults { get; } = new();

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
                        SelectItem(ResultsListBox.SelectedItem as IndividualDto);
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
                SelectedItem = ResultsListBox.SelectedItem as IndividualDto;
                _ignoreSelectionChange = false;
            }
        }

        private void ResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsListBox.SelectedItem != null)
            {
                SelectItem(ResultsListBox.SelectedItem as IndividualDto);
            }
        }

        private void SelectItem(IndividualDto item)
        {
            if (item != null)
            {
                _ignoreSelectionChange = true;
                _ignoreTextChange = true;
                SelectedItem = item;
                SearchTextBox.Text = item.ShortName ?? "";
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
                SelectItem(ResultsListBox.SelectedItem as IndividualDto);
        });
    }
}