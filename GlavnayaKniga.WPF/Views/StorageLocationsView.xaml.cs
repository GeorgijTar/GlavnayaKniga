using GlavnayaKniga.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlavnayaKniga.WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для StorageLocationsView.xaml
    /// </summary>
    public partial class StorageLocationsView : UserControl
    {
        public StorageLocationsView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.StorageLocationsViewModel viewModel)
            {
                viewModel.SelectedLocation = e.NewValue as StorageLocationDto;
            }
        }
    }
}
