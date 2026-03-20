using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GlavnayaKniga.WPF.Views
{
    public partial class EntryEditWindow : Window
    {
        public EntryEditWindow()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, запятую и точку
            Regex regex = new Regex(@"^[0-9,.]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}