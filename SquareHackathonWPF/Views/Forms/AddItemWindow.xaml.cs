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

namespace SquareHackathonWPF.Views.Forms
{
    /// <summary>
    /// Interaction logic for AddItemWindow.xaml
    /// </summary>
    public partial class AddItemWindow : Window
    {
        public AddItemWindow()
        {
            InitializeComponent();
        }

        private void AddVariationButtonClick(object sender, RoutedEventArgs e)
        {
            var idForm = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new(3),
                MaxLines = 1,
                MinWidth = 128,
                Background = Brushes.LightGray,
                Margin = new(3),
                Text = "Id.",
                Tag = "VariationId"
            };

            var nameForm = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new(3),
                MaxLines = 1,
                MinWidth = 128,
                Background = Brushes.LightGray,
                Margin = new(3),
                Text = "Name.",
                Tag = "VariationName"
            };

            var stackPanel = new StackPanel { Children = { idForm, nameForm } };

            FormPanel.Children.Add(stackPanel);
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            // try to add the item

            // if it fails, show the error message, ideally the same one produced by the square api
        }
    }
}
