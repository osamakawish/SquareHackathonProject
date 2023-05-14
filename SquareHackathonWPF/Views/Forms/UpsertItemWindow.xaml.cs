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
using Square.Exceptions;
using SquareHackathonWPF.Models.SquareApi;
using SquareHackathonWPF.ViewModels;

namespace SquareHackathonWPF.Views.Forms;

/// <summary>
/// Interaction logic for AddItemWindow.xaml
/// </summary>
public partial class UpsertItemWindow
{
    internal bool                IsEdit             { get; init; } = false;
    private  CatalogObject       ItemObject         { get; }       = new("Item", "");
    private  string              ItemId             { get; set; }  = "";
    private  CatalogItem.Builder CatalogItemBuilder { get; }       = new();
    private  List<CatalogObject> Variations         { get; }       = new();

    private static string IdempotencyKey => Guid.NewGuid().ToString();

    internal event EventHandler<CatalogObject>? UpsertingItem;

    public UpsertItemWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        AddVariation(new (
            id: "#Main",
            type: "ITEM_VARIATION",
            itemVariationData: new (
                name: "Main",
                pricingType: "FIXED_PRICING",
                priceMoney: new(100, "CAD"))));

        ImplementTextBoxEvents();
    }

    /// <summary>
    /// The window's constructor to be used when editing an item.
    /// </summary>
    internal UpsertItemWindow(CatalogObject item)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        ItemObject = item;
        CatalogItemBuilder = ItemObject.ItemData.ToBuilder();
        
        var itemId = item.Id;
        var itemName = item.ItemData.Name;
        var itemDescription = item.ItemData.Description;
        var variations = item.ItemData.Variations;

        IsEdit = true;
        ItemId = itemId; ItemIdTextBox.Text = itemId; ItemIdTextBox.IsEnabled = false;
        ItemNameTextBox.Text = itemName;
        DescriptionTextBox.Text = itemDescription;

        variations?.ToList().ForEach(AddVariation);
        ShowIds();

        ImplementTextBoxEvents();
    }

    private void ShowIds()
    {
        var messageBoxText = $"Item id: {ItemId}\n" +
                             $"Variation Item Ids: {string.Join(", ", Variations.Select(v => v.ItemVariationData.ItemId))}\n" +
                             $"Variation Ids: {string.Join(", ", Variations.Select(v => v.Id))}";
        MessageBox.Show(messageBoxText);
    }

    #region Item Variations
    private void AddVariationButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new AddItemVariationWindow { ItemId = ItemIdTextBox.Text };

        window.Closed += delegate {
            if (!window.OkButtonClicked) return;

            var variation = window.GetVariation();
            AddVariation(variation);
        };

        window.ShowDialog();
    }

    /// <summary>
    /// Adds the variation to the variations panel in the UI and the list of variations (<see cref="Variations"/>).
    /// </summary>
    /// <param name="variation"></param>
    private void AddVariation(CatalogObject variation)
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
            },
            Margin = new (3)
        };
        editButton.Click += ClickEditButton;

        // Id and Name boxes
        var idBlock = new TextBlock {
            Text = $"#{variation.Id.TrimStart('#')}",
            Tag = "VariationId",
            Margin = new(3)
        };
        var nameBlock = new TextBlock {
            Text = variation.ItemVariationData.Name,
            Tag = "VariationName",
            Margin = new(3)
        };

        // Pricing box
        var pricingBlock = new TextBlock
        {
            Text = ItemVariation.PriceToString(variation.ItemVariationData),
            Width = 80, Foreground = Brushes.MediumSeaGreen,
            Tag = "VariationPricing",
            TextAlignment = TextAlignment.Right,
            Margin = new(3)
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

        variation = variation.ToBuilder().Id(variation.Id).Build();
        Variations.Add(variation);
    }
    #endregion

    #region Remaining Item Details
    private void ImplementTextBoxEvents()
    {
        ItemIdTextBox.TextChanged += delegate {
            if (ItemIdTextBox.Text.Length == 0) return;
            ItemId = ItemIdTextBox.Text;
        };

        ItemNameTextBox.TextChanged += delegate {
            if (ItemNameTextBox.Text.Length == 0) return;
            CatalogItemBuilder.Name(ItemNameTextBox.Text);
        };

        DescriptionTextBox.TextChanged += delegate {
            if (DescriptionTextBox.Text.Length == 0) return;
            CatalogItemBuilder.Description(DescriptionTextBox.Text);
        };
    }

    // TODO: Debug here.
    private async void OkButtonClick(object sender, RoutedEventArgs args)
    {
        // Check if the textbox inputs are valid, otherwise return
        if (!ValidatedTextBoxInputs()) return;

        // Update item ids for variations if editing
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var v in Variations) {
            var catalogItemVariation = v.ItemVariationData.ToBuilder()
                .ItemId(ItemId)
                .Build();
            var variation = v.ToBuilder()
                .ItemVariationData(catalogItemVariation)
                .Build();
            variation.ToBuilder().Id(ItemId).Build();
        }

        // try to add the item
        CatalogItemBuilder.Variations(Variations.Select(v => v).ToList());
        var item = ItemObject.ToBuilder()
            .ItemData(CatalogItemBuilder.Build())
            .Build();

        var messageBoxText = $"Item id: {item.Id}\n" +
                             $"Variation Item Ids: {string.Join(", ", Variations.Select(v => v.ItemVariationData.ItemId))}\n" +
                             $"Variation Ids: {string.Join(", ", Variations.Select(v => v.Id))}";
        MessageBox.Show(messageBoxText);
        Clipboard.SetText(messageBoxText);

        // Make the API call
        var request = new UpsertCatalogObjectRequest(idempotencyKey: IdempotencyKey, mObject: item);
        try {
            await App.Client.CatalogApi.UpsertCatalogObjectAsync(request);
            Closed += delegate { UpsertingItem?.Invoke(this, item); };
            
            Close();
        }
        catch (ApiException e) {
            var errors = e.Errors;
            var message = errors.Aggregate("", (current, ex) => current + $"({e.ResponseCode}) {ex.Category}: {ex.Detail}\n");

            ErrorBlock.Text = $"{message}";
            Clipboard.SetText(message);
        }
    }

    private long? GetVersion(object item)
    {
        return null;
    }

    private bool ValidatedTextBoxInputs()
    {
        // Update error block if there are any errors
        if (ItemIdTextBox.Text.Length == 0) {
            ErrorBlock.Text = "Item ID cannot be empty.";
            return false;
        }
        if (ItemNameTextBox.Text.Length == 0) {
            ErrorBlock.Text = "Item name cannot be empty.";
            return true;
        }
        // ReSharper disable once InvertIf
        if (Variations.Count == 0) {
            ErrorBlock.Text = "Item must have at least one variation.";
            return false;
        }

        return true;
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

            var variation = variationWindow.GetVariation();
            var variationData = variation.ItemVariationData;

            idBlock.Text = $"#{variation.Id}";
            nameBlock.Text = variationData.Name;
            pricingBlock.Text = $"{variationData.PriceMoney.Amount} ({variationData.PriceMoney.Currency})";
        };

        if (IsEdit) variationWindow.VariationIdTextBox.IsEnabled = false;

        variationWindow.ShowDialog();
    }
    #endregion
}