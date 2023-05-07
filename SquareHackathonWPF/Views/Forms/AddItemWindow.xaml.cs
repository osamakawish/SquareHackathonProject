using Square.Models;
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

namespace SquareHackathonWPF.Views.Forms;

/// <summary>
/// Interaction logic for AddItemWindow.xaml
/// </summary>
public partial class AddItemWindow
{
    public AddItemWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void AddVariationButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new AddItemVariationWindow { ItemId = ItemIdTextBox.Text };
        
        window.Closed += delegate {
            if (!window.OkButtonClicked) return;

            window.GetVariation(out var variation, out var variationAsCatalogObject);
            AddVariation(variation, variationAsCatalogObject);
        };

        window.ShowDialog();
    }

    private void AddVariation(CatalogItemVariation variation, CatalogObject variationAsCatalogObject)
    {
        // Edit button
        var editButton = new Button
        {
            Width = 36,
            Background = Brushes.Transparent,
            BorderThickness = new (0),
            Content = new TextBlock
            {
                TextDecorations = TextDecorations.Underline,
                Foreground = Brushes.DeepSkyBlue,
                Width = 28,
                Text = "Edit."
            }
        };
        editButton.Click += ClickEditButton;

        // Id and Name boxes
        var idBlock = new TextBlock { Text = $"#{variationAsCatalogObject.Id}", Tag = "VariationId" };
        var nameBlock = new TextBlock { Text = variation.Name, Tag = "VariationName" };

        // Pricing box
        var pricingBlock = new TextBlock
        {
            Text = variation switch {
                { PricingType: "FIXED_PRICING" } => $"{variation.PriceMoney.Amount} ({variation.PriceMoney.Currency})",
                { PricingType: "VARIABLE_PRICING" } => "Price Varies",
                _ => throw new InvalidOperationException("Invalid pricing type")
            },
            Width = 80, Foreground = Brushes.MediumSeaGreen,
            Tag = "VariationPricing",
            TextAlignment = TextAlignment.Right
        };

        // Finally, add the new row for the variations panel
        var stackPanel = new StackPanel
        {
            Resources = { { typeof(TextBlock), new Style(typeof(TextBlock))
            {
                Setters = {
                    new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Left),
                    new Setter(WidthProperty, 106d),
                    new Setter(ForegroundProperty, Brushes.LightGray)
                } } } },
            Children = { editButton, idBlock, nameBlock, pricingBlock }
        };
        VariationsStackPanel.Children.Add(stackPanel);
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        // update item ids for variations

        // try to add the item

        // if it fails, show the error message, ideally the same one produced by the square api.
    }

    private void ClickEditButton(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var stackPanel = (StackPanel)button.Parent;

        var idBlock = (TextBlock)stackPanel.Children[1];
        var nameBlock = (TextBlock)stackPanel.Children[2];
        var pricingBlock = (TextBlock)stackPanel.Children[3];

        AddItemVariationWindow variationWindow;

        if (pricingBlock.Text == "Price Varies") {
            variationWindow = new() {
                IsEditing = true,
                ItemId = ItemIdTextBox.Text,
                InitialVariationId = idBlock.Text.TrimStart('#'),
                InitialVariationName = nameBlock.Text,
                InitialPricingType = PricingType.Variable
            };
        }
        else {
            var pricing = pricingBlock.Text.Split(' ');

            variationWindow = new() {
                IsEditing = true,
                ItemId = ItemIdTextBox.Text,
                InitialVariationId = idBlock.Text.TrimStart('#'),
                InitialVariationName = nameBlock.Text,
                InitialPricingType = PricingType.Fixed,
                InitialPricingValue = long.Parse(pricing[0]).ToString(),
                InitialPricingCurrency = pricing[1].Trim('(', ')')
            };
        }

        variationWindow.Closed += (_, eventArgs) => {
            if (!variationWindow.OkButtonClicked) return;

            variationWindow.GetVariation(out var variation, out var variationAsCatalogObject);
            idBlock.Text = $"#{variationAsCatalogObject.Id}";
            nameBlock.Text = variation.Name;
            pricingBlock.Text = $"{variation.PriceMoney.Amount} ({variation.PriceMoney.Currency})";
        };

        variationWindow.ShowDialog();
    }
}