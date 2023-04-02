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
    /// Interaction logic for EllipseWindow.xaml
    /// </summary>
    public partial class EllipseWindow : Window
    {
        public double EllipseWidth;
        public double EllipseHeight;
        public double EllipseStrokeThickness;
        public bool IsValid = true;
        public ColorsEnum EllipseBackground;
        public bool EllipseAddText;
        public ColorsEnum TextForeground;
        public string TextMessage;

        public EllipseWindow()
        {
            EllipseAddText = false;
            InitializeComponent();
            IsValid = true;
        }

        private void Done(object sender, RoutedEventArgs e)
        {
            IsValid = true;

            if(!double.TryParse(XRadius.Text, out EllipseWidth))
                IsValid = false;
            if(!double.TryParse(YRadius.Text, out EllipseHeight))
                IsValid = false;
            if(!double.TryParse(StrokeThickness.Text, out EllipseStrokeThickness))
                IsValid = false;

            ComboBoxItem typeItem = (ComboBoxItem)ColorsCb.SelectedItem;
            if(typeItem == null)
            {
                IsValid = false;
            }
            else
            {
                string value = typeItem.Content.ToString();
                switch (value)
                {
                    case "Black":
                        EllipseBackground = ColorsEnum.BLACK;
                        break;
                    case "Red":
                        EllipseBackground = ColorsEnum.RED;
                        break;
                    case "Green":
                        EllipseBackground = ColorsEnum.GREEN;
                        break;
                    case "Blue":
                        EllipseBackground = ColorsEnum.BLUE;
                        break;
                    case "Yellow":
                        EllipseBackground = ColorsEnum.YELLOW;
                        break;
                    default:
                        EllipseBackground = ColorsEnum.NONE;
                        IsValid = false;
                        break;
                }

            }

            EllipseAddText = TextChb.IsChecked ?? false;
            if (EllipseAddText)
            {
                TextMessage = TextInput.Text;

                ComboBoxItem foreground = (ComboBoxItem)ForegroundCb.SelectedItem;
                if (foreground == null)
                {
                    IsValid = false;
                }
                else
                {
                    string value = foreground.Content.ToString();
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
            }

            if (IsValid)
                this.Close();

            else
            {
                ErrorMsg.Visibility = Visibility.Visible;
            }
        }

        public void SetValues(double width, double height, double strokeThickness, ColorsEnum background, 
            string text, bool isChecked)
        {
            XRadius.Text = width.ToString();
            YRadius.Text = height.ToString();
            StrokeThickness.Text = strokeThickness.ToString();
            TextInput.Text = text;
            TextChb.IsChecked = isChecked;
        }
    }
}
