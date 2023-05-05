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
    }

    private void AddVariationButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new AddItemVariationForm { ItemId = ItemIdTextBox.Text };
        window.ShowDialog();

        window.Closed += delegate {
            window.GetVariation(out var variation, out var variationAsCatalogObject);
            AddVariation(variation, variationAsCatalogObject);
        };
    }

    private void AddVariation(CatalogItemVariation variation, CatalogObject variationAsCatalogObject)
    {
        var style = new Style(typeof(TextBlock)) {
            Setters = {
                new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(PaddingProperty, new Thickness(3)),
                new Setter(WidthProperty, 112),
                new Setter(ForegroundProperty, Brushes.LightGray),
                new Setter(MarginProperty, new Thickness(3))
            }
        };

        var button = new Button
        {
            Width = 36,
            Background = Brushes.Transparent,
            BorderThickness = new (0),
            Content = new TextBlock
            {
                TextDecorations = TextDecorations.Underline,
                Foreground = Brushes.DeepSkyBlue,
                Width = 32,
                Text = "Edit."
            }
        };

        var idBlock = new TextBox { Text = $"#{variationAsCatalogObject.Id}", Tag = "VariationId" };

        var nameBlock = new TextBox { Text = variation.Name, Tag = "VariationName" };

        var pricingBlock = new TextBox
        {
            Text = $"{variation.PriceMoney.Amount} ({variation.PriceMoney.Currency})",
            Width = 96, Foreground = Brushes.MediumSeaGreen,
            Tag = "VariationPricing"
        };

        var stackPanel = new StackPanel
        {
            Resources = new() { { typeof(TextBlock), style } },
            Orientation = Orientation.Horizontal,
            Children = { button, idBlock, nameBlock, pricingBlock }
        };

        VariationsStackPanel.Children.Add(stackPanel);
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
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

        AddItemVariationForm variationForm;

        if (pricingBlock.Text == "Price Varies") {
            variationForm = new() {
                IsEditing = true,
                ItemId = ItemIdTextBox.Text,
                InitialVariationId = idBlock.Text,
                InitialVariationName = nameBlock.Text,
                InitialPricingType = PricingType.Variable
            };
        }
        else {
            var pricing = pricingBlock.Text.Split(' ');

            variationForm = new() {
                IsEditing = true,
                ItemId = ItemIdTextBox.Text,
                InitialVariationId = idBlock.Text,
                InitialVariationName = nameBlock.Text,
                InitialPricingType = PricingType.Variable,
                InitialPricingValue = long.Parse(pricing[0]).ToString(),
                InitialPricingCurrency = pricing[1].Trim('(', ')')
            };
        }

        variationForm = new AddItemVariationForm {
            IsEditing = true,
            ItemId = ItemIdTextBox.Text,
            InitialVariationId = idBlock.Text,
            InitialVariationName = nameBlock.Text,
        };

        variationForm.Closed += delegate {
            variationForm.GetVariation(out var variation, out var variationAsCatalogObject);
            idBlock.Text = $"#{variationAsCatalogObject.Id}";
            nameBlock.Text = variation.Name;
            pricingBlock.Text = $"{variation.PriceMoney.Amount} ({variation.PriceMoney.Currency})";
        };

        variationForm.ShowDialog();
    }
}