using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Square.Models;

namespace SquareHackathonWPF.Views.Forms;


/// <summary>
/// Interaction logic for AddIemVariationForm.xaml
/// </summary>
public partial class AddItemVariationWindow : Window
{
    internal bool   IsEditing       { get; init; } = false;
    internal bool   OkButtonClicked { get; private set; }
    internal string ItemId          { get; init; } = "";

    internal string      InitialVariationId     { get; init; } = "";
    internal string      InitialVariationName   { get; init; } = "";
    internal PricingType InitialPricingType     { get; init; } = PricingType.Fixed;
    internal string      InitialPricingValue    { get; init; } = "";
    internal string      InitialPricingCurrency { get; init; } = "";

    public AddItemVariationWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Loaded += delegate {
            if (!IsEditing) return;

            VariationIdTextBox.Text = InitialVariationId;
            VariationNameTextBox.Text = InitialVariationName;
            switch (InitialPricingType) {
                case PricingType.Fixed:
                    PricingValueTextBox.Text = InitialPricingValue;
                    PricingCurrencyTextBox.Text = InitialPricingCurrency;
                    PricingTypeComboBox.SelectedIndex = 0;
                    break;
                case PricingType.Variable:
                    PricingTypeComboBox.SelectedIndex = 1;
                    break;
                default:
                    throw new InvalidOperationException("Invalid pricing type");
            }
        };

        OkButton.Click += delegate {
            OkButtonClicked = true;
            Close();
        };
        Closing += OnFormClosing;
    }

    private void OnFormClosing(object? _, CancelEventArgs args)
    {
        if (OkButtonClicked)
            args.Cancel = !InputsAreValid();
    }

    private bool InputsAreValid()
    {
        if (string.IsNullOrWhiteSpace(VariationNameTextBox.Text)) {
            WarningTextBlock.Text = "Variation name cannot be empty";
            return false;
        }

        switch (PricingTypeComboBox.SelectedIndex) {
            case -1 or > 1:
                WarningTextBlock.Text = "Pricing type must be selected";
                return false;
            case 0 when string.IsNullOrWhiteSpace(PricingValueTextBox.Text):
                WarningTextBlock.Text = "Pricing value cannot be empty";
                return false;
            case 0 when string.IsNullOrWhiteSpace(PricingCurrencyTextBox.Text):
                WarningTextBlock.Text = "Currency cannot be empty";
                return false;
            default:
                // Too much work to validate the pricing value and currency. Let the API do it.
                return true;
        }
    }

    private void PricingTypeSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (PricingValueTextBox is null) return;

        switch (PricingTypeComboBox.SelectedIndex) {
            case 0:
                PricingValueTextBox.Visibility = Visibility.Visible;
                PricingCurrencyTextBox.Visibility = Visibility.Visible;
                break;
            default:
                PricingValueTextBox.Visibility = Visibility.Hidden;
                PricingCurrencyTextBox.Visibility = Visibility.Hidden;
                break;
        }
    }

    internal void GetVariation(out CatalogItemVariation variation, out CatalogObject variationAsCatalogObject)
    {
        var variationBuilder = new CatalogItemVariation.Builder()
            .ItemId(ItemId)
            .Name(VariationNameTextBox.Text);

        variationBuilder.PricingType(PricingTypeComboBox.SelectedIndex switch
        {
            0 => "FIXED_PRICING",
            1 => "VARIABLE_PRICING",
            _ => throw new InvalidOperationException("Invalid pricing type")
        });

        switch (PricingTypeComboBox.SelectedIndex) {
            case 0 when long.TryParse(PricingValueTextBox.Text, out var price):
                variationBuilder.PriceMoney(new Money.Builder()
                    .Amount(price)
                    .Currency(PricingCurrencyTextBox.Text)
                    .Build());
                break;
            case 0:
                WarningTextBlock.Text = "Invalid price. Price must be an integer multiple of lowest unit of currency.";
                break;
        }

        variation = variationBuilder.Build();

        variationAsCatalogObject = new CatalogObject.Builder("ITEM_VARIATION", VariationIdTextBox.Text.TrimStart('#'))
            .ItemVariationData(variation)
            .Build();
    }
}
public enum PricingType
{
    Fixed,
    Variable
}