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

        Loaded += async delegate
        {
            // Retrieve the inventory and update the UI
            var inventory = await ViewModel.RetrieveInventory();
            if (inventory == null) return;
            foreach (var itemAsCatalogObject in inventory)
                AddItem(itemAsCatalogObject);
        };

        RecordButtonWaveImage.Source = MainWindowViewModel.ConvertToImageSource(ViewModel.Image);

        DataContext = ViewModel;
    }

    #region Methods
    internal void AddItem(CatalogObject item)
    {
        // Edit button
        var editButton = new Button {
            Width = 36,
            Background = Brushes.Transparent,
            BorderThickness = new (0),
            Content = new TextBlock {
                TextDecorations = TextDecorations.Underline,
                Foreground = Brushes.DeepSkyBlue,
                Width = 28,
                Text = "Edit."
            },
            Margin = new(3)
        };
        editButton.Click += ClickEditButton;

        // Item ID Block
        var itemIdTextBlock = new TextBlock {
            Tag = "VariationId",
            Text = item.Id,
            Margin = new (3),
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        // Item Name Block
        var itemData = item.ItemData;

        var itemNameTextBlock = new TextBlock {
            Tag = "VariationName",
            Text = itemData.Name,
            Margin = new(3)
        };

        // Item Description Block
        var itemDescriptionTextBlock = new TextBlock {
            Tag = "VariationDescription",
            Text = itemData.Description,
            Margin = new(3)
        };

        // Item Price Block
        var itemPriceTextBlock = new TextBlock {
            Tag = "VariationPricing",
            TextAlignment = TextAlignment.Right,
            Foreground = Brushes.MediumSeaGreen,
            Text = itemData.PriceRangeAsString(),
            Margin = new(3)
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

        ViewModel.Items.Add(item);
    }

    private void RecordButtonClick(object sender, RoutedEventArgs e)
    {
        var recordButton = (Button) sender;
        var buttonShape = (Rectangle) recordButton.Content;

        if (ViewModel.IsRecording) {
            ViewModel.StopRecording();
            buttonShape.RadiusX = buttonShape.RadiusY = 12;
            return;
        }
        ViewModel.StartRecording();
        buttonShape.RadiusX = buttonShape.RadiusY = 0;
    }

    private void AddItemButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new UpsertItemWindow();
        
        window.UpsertingItem += (o, item) => AddItem(item);

        window.ShowDialog();
    }

    private void ClickEditButton(object sender, RoutedEventArgs e)
    {
        var button = (Button) sender;
        var row = Grid.GetRow(button);

        // Get the elements in the row of the button with the given item
        var gridElements = InventoryGrid.Children.Cast<UIElement>();
        T? GetElement<T>(int x, int y) where T : UIElement
        // ReSharper disable once PossibleMultipleEnumeration
            => (T?) gridElements.First(c => Grid.GetRow(c) == x && Grid.GetColumn(c) == y);
        var itemNameBlock = GetElement<TextBlock>(row, 2)!;
        var descriptionBlock = GetElement<TextBlock>(row, 3)!;
        var priceBlock = GetElement<TextBlock>(row, 4)!;

        var selectedItem = ViewModel.Items[row];
        var window = new UpsertItemWindow((CatalogObject) selectedItem);

        window.UpsertingItem += (o, item) => {
            var itemData = item.ItemData;

            itemNameBlock.Text = itemData.Name;
            descriptionBlock.Text = itemData.Description;
            priceBlock.Text = itemData.PriceRangeAsString();

            ViewModel.UpdateItem(item);
        };

        window.ShowDialog();
    }
    #endregion
}