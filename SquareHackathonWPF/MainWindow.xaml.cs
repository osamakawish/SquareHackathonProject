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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using Square.Models;
using SquareHackathonWPF.Models;
using SquareHackathonWPF.Models.SquareApi;
using SquareHackathonWPF.ViewModels;
using SquareHackathonWPF.Views.Forms;
using UIElement = System.Windows.UIElement;

namespace SquareHackathonWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once UnusedMember.Global
public partial class MainWindow
{
    private MainWindowViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.Maximized;

        DataContext = ViewModel;
    }

    internal void AddItem(Item item)
    {
        var editButton = new Button {
            Width = 36,
            Background = Brushes.Transparent,
            BorderThickness = new (0),
            Content = new TextBlock {
                TextDecorations = TextDecorations.Underline,
                Foreground = Brushes.DeepSkyBlue,
                Width = 28,
                Text = "Edit."
            }
        };
        editButton.Click += ClickEditButton;

        var itemIdTextBlock = new TextBlock {
            Tag = "VariationId",
            Text = item.Id
        };

        var itemNameTextBlock = new TextBlock {
            Tag = "VariationName",
            Text = item.CatalogItem.Name
        };

        var itemDescriptionTextBlock = new TextBlock {
            Tag = "VariationDescription",
            Text = item.CatalogItem.Description
        };

        var itemPriceTextBlock = new TextBlock {
            Tag = "VariationPricing",
            TextAlignment = TextAlignment.Right,
            Foreground = Brushes.MediumSeaGreen,
            Text = item.PriceRangeAsString()
        };

        var newRow = new RowDefinition();
        InventoryGrid.RowDefinitions.Add(newRow);

        void SetGridCell(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            InventoryGrid.Children.Add(element);
        }
        void SetGridCells(params (UIElement element, int row, int column)[] elements)
            => elements.ToList().ForEach((e => SetGridCell(e.element, e.row, e.column)));

        var row = InventoryGrid.RowDefinitions.Count - 1;

        SetGridCells(
            (editButton, row, 0),
            (itemIdTextBlock, row, 1),
            (itemNameTextBlock, row, 2),
            (itemDescriptionTextBlock, row, 3),
            (itemPriceTextBlock, row, 4)
        );
    }

    private void RecordButtonClick(object sender, RoutedEventArgs e) { }

    private void AddItemButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new AddItemWindow();

        // TODO: Add item to inventory grid if ok clicked

        window.ShowDialog();
    }

    private void ClickEditButton(object sender, RoutedEventArgs e) { }
}