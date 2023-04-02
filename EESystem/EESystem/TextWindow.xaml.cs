using EESystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EESystem
{
    /// <summary>
    /// Interaction logic for TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    {
        public string TextMessage;
        public double TextFontSize;
        public bool IsValid = true;
        public ColorsEnum TextForeground;

        public TextWindow()
        {
            InitializeComponent();
        }

        private void Done(object sender, RoutedEventArgs e)
        {
            IsValid = true;

            TextMessage = Text.Text;
            if (String.IsNullOrEmpty(TextMessage))
                IsValid = false;

            if (!double.TryParse(FontSize.Text, out TextFontSize))
                IsValid = false;

            ComboBoxItem typeItem = (ComboBoxItem)ColorsCb.SelectedItem;
            if (typeItem == null)
            {
                IsValid = false;
            }
            else
            {
                string value = typeItem.Content.ToString();
                switch (value)
                {
                    case "Black":
                        TextForeground = ColorsEnum.BLACK;
                        break;
                    case "Red":
                        TextForeground = ColorsEnum.RED;
                        break;
                    case "Green":
                        TextForeground = ColorsEnum.GREEN;
                        break;
                    case "Blue":
                        TextForeground = ColorsEnum.BLUE;
                        break;
                    case "Yellow":
                        TextForeground = ColorsEnum.YELLOW;
                        break;
                    default:
                        TextForeground = ColorsEnum.NONE;
                        IsValid = false;
                        break;
                }

            }

            if (IsValid)
                this.Close();

            else
            {
                ErrorMsg.Visibility = Visibility.Visible;
            }
        }
    }
}
