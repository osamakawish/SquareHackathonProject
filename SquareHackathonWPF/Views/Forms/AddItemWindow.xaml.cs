﻿using Square.Models;
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
public partial class AddItemWindow
{
    private string              IdempotencyKey     { get; }      = Guid.NewGuid().ToString();
    private string              ItemId             { get; set; } = "";
    private CatalogItem.Builder CatalogItemBuilder { get; }      = new();
    private List<ItemVariation> Variations         { get; }      = new();

    internal event EventHandler<Item>? AddingItem;

    public AddItemWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        ImplementTextBoxEvents();
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

    private void AddVariation(ItemVariation variation)
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
        var idBlock = new TextBlock { Text = $"#{variation.AsCatalogObject.Id}", Tag = "VariationId" };
        var nameBlock = new TextBlock { Text = variation.Variation.Name, Tag = "VariationName" };

        // Pricing box
        var pricingBlock = new TextBlock
        {
            Text = ItemVariation.PriceToString(variation.Variation),
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

    private async void OkButtonClick(object sender, RoutedEventArgs args)
    {
        // Check if the textbox inputs are valid, otherwise return
        if (!ValidatedTextBoxInputs()) return;

        // update item ids for variations
        foreach (var variation in Variations)
            variation.Variation = variation.Variation.ToBuilder().ItemId(ItemId).Build();

        // try to add the item
        var item = Item.FromBuilder(ItemId, CatalogItemBuilder);
        var request = new UpsertCatalogObjectRequest(IdempotencyKey, item.AsCatalogObject);
        try {
            await App.Client.CatalogApi.UpsertCatalogObjectAsync(request);
            Closed += delegate { AddingItem?.Invoke(this, item); };
            
            // Close the window if it succeeds
            Close();
        }
        catch (ApiException e) {
            // if it fails, show the error message, ideally the same one produced by the square api.
            ErrorBlock.Text = $"Failed to send request:\n{e.Message}";
        }
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
            idBlock.Text = $"#{variation.AsCatalogObject.Id}";
            nameBlock.Text = variation.Variation.Name;
            pricingBlock.Text = $"{variation.Variation.PriceMoney.Amount} ({variation.Variation.PriceMoney.Currency})";
        };

        variationWindow.ShowDialog();
    }
    #endregion
}