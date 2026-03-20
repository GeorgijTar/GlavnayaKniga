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
using System.Windows.Shapes;

namespace GlavnayaKniga.WPF.Views
{
    /// <summary>
    /// Логика взаимодействия для StorageLocationEditWindow.xaml
    /// </summary>
    public partial class StorageLocationEditWindow : Window
    {
        public StorageLocationEditWindow()
        {
            InitializeComponent();
        }

        private void EmployeeSearch_ItemSelected(object sender, EmployeeDto employee)
        {
            // Дополнительная логика при выборе сотрудника
            if (DataContext is ViewModels.StorageLocationEditViewModel viewModel)
            {
                // Можно добавить дополнительную логику при необходимости
            }
        }
    }
}
