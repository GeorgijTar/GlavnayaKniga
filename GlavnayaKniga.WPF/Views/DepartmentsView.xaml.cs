using System.Windows;
using System.Windows.Controls;
using GlavnayaKniga.Application.DTOs;

namespace GlavnayaKniga.WPF.Views
{
    public partial class DepartmentsView : UserControl
    {
        public DepartmentsView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.DepartmentsViewModel viewModel)
            {
                viewModel.SelectedDepartment = e.NewValue as DepartmentDto;
            }
        }
    }
}