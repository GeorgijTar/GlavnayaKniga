using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Domain.Entities;
using System.Windows;
using System.Windows.Controls;

namespace GlavnayaKniga.WPF.Views
{
    public partial class AccountsView : UserControl
    {
        public AccountsView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.AccountsViewModel viewModel)
            {
                viewModel.SelectedAccount = e.NewValue as AccountDto;
            }
        }
    }
}