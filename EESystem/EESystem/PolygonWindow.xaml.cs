﻿using EESystem.Model;
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
    /// Interaction logic for PolygonWindow.xaml
    /// </summary>
    public partial class PolygonWindow : Window
    {
        public double PolygonStrokeThickness;
        public bool IsValid = true;
        public ColorsEnum PolygonBackground;

        public PolygonWindow()
        {
            InitializeComponent();
        }

        private void Done(object sender, RoutedEventArgs e)
        {
            IsValid = true;

            if (!double.TryParse(StrokeThickness.Text, out PolygonStrokeThickness))
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
                        PolygonBackground = ColorsEnum.BLACK;
                        break;
                    case "Red":
                        PolygonBackground = ColorsEnum.RED;
                        break;
                    case "Green":
                        PolygonBackground = ColorsEnum.GREEN;
                        break;
                    case "Blue":
                        PolygonBackground = ColorsEnum.BLUE;
                        break;
                    case "Yellow":
                        PolygonBackground = ColorsEnum.YELLOW;
                        break;
                    default:
                        PolygonBackground = ColorsEnum.NONE;
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
