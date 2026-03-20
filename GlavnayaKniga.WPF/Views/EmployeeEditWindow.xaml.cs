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
    /// Логика взаимодействия для EmployeeEditWindow.xaml
    /// </summary>
    public partial class EmployeeEditWindow : Window
    {
        public EmployeeEditWindow()
        {
            InitializeComponent();
        }

        private void IndividualSearch_ItemSelected(object sender, IndividualDto individual)
        {
            // Дополнительная логика при выборе физического лица
            if (DataContext is ViewModels.EmployeeEditViewModel viewModel)
            {
                // Фокус переходит на следующий контрол
                PositionSearch.Focus();
            }
        }

        private void PositionSearch_ItemSelected(object sender, PositionDto position)
        {
            // Дополнительная логика при выборе должности
            if (DataContext is ViewModels.EmployeeEditViewModel viewModel)
            {
                // Фокус переходит на следующий контрол
                // Можно установить фокус на следующий элемент
            }
        }
    }
}
