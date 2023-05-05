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

namespace SquareHackathonWPF.Views.Forms
{
    public enum PricingType
    {
        Fixed,
        Variable
    }

    /// <summary>
    /// Interaction logic for AddIemVariationForm.xaml
    /// </summary>
    public partial class AddItemVariationForm : Window
    {
        internal bool        IsEditing       { get; init; } = false;
        internal string      ItemId          { get; set; } = "";
        internal string      VariationId     { get; set; } = "";
        internal string      VariationName   { get; set; } = "";
        internal PricingType PricingType     { get; set; }
        internal string      PricingValue    { get; set; } = "";
        internal string      PricingCurrency { get; set; } = "";

        public AddItemVariationForm()
        {
            InitializeComponent();

            Closing += OnFormClosing;
        }

        private void OnFormClosing(object? _, CancelEventArgs args) => args.Cancel = !InputsAreValid();

        private bool InputsAreValid()
        {
            if (string.IsNullOrWhiteSpace(ItemId)) {
                WarningTextBlock.Text = "Item ID cannot be empty";
                return false;
            }

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

        private void PricingTypeSelected(object sender, RoutedEventArgs e)
        {
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

            variationAsCatalogObject = new CatalogObject.Builder("ITEM_VARIATION", VariationIdTextBox.Text)
                .ItemVariationData(variation)
                .Build();
        }
    }
}
