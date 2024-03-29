﻿using Square.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SquareHackathonWPF.Views.Forms;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace SquareHackathonWPF.Models.SquareApi;

internal interface ISquareApiCatalogObject
{
    string Id { get; }
    CatalogObject AsCatalogObject { get; }
}

internal record ItemVariation(string Id, CatalogItemVariation Variation) : ISquareApiCatalogObject
{
    private string _id = Id;
    public string Id {
        get => _id;
        set {
            AsCatalogObject = AsCatalogObject.ToBuilder().Id($"#{value.TrimStart('#')}").Build();
            _id = value;
        }
    }

    public CatalogItemVariation Variation { get; set; } = Variation;

    // Id != itemId here. They're two separate variables.
    public static ItemVariation FromBuilder(string itemId, CatalogItemVariation.Builder builder)
        => new(itemId, builder.ItemId(itemId).Build());

    public CatalogObject AsCatalogObject { get; private set; }
        = new(type: "ITEM_VARIATION", id: $"#{Id.TrimStart('#')}", itemVariationData: Variation);

    public static string PriceToString(CatalogItemVariation variation) => variation.PricingType switch {
        "FIXED_PRICING" => $"{variation.PriceMoney.Amount} ({variation.PriceMoney.Currency})",
        "VARIABLE_PRICING" => "Price Varies",
        _ => throw new InvalidOperationException("Invalid pricing type")
    };

    public void SetPricingType(PricingType pricingType)
    {
        Variation = Variation.ToBuilder().PricingType(
            pricingType switch {
                PricingType.Fixed => "FIXED_PRICING",
                PricingType.Variable => "VARIABLE_PRICING",
                _ => throw new InvalidOperationException("Invalid pricing type") })
            .Build();
    }
}

public class Item
{
    private readonly CatalogObject _item;
    public Item (CatalogObject item)
    {
        if (item.Type != "ITEM")
            throw new ArgumentException("CatalogObject must be of type ITEM", nameof(item));
        _item = item;
    }

    public static explicit operator CatalogObject(Item item) => item._item;
    public static implicit operator Item(CatalogObject item) => new(item);

    public string PriceRangeAsString() => PriceRangeAsString(_item.ItemData);

    public static string PriceRangeAsString(CatalogItem catalogItem)
    {
        var variations = catalogItem.Variations.Select(v => v.ItemVariationData).ToList();

        return variations switch
        {
            { Count: <= 0 } => throw new InvalidOperationException("Item has no variations"),
            { Count: 1 } => ItemVariation.PriceToString(variations[0]),
            [_, _, ..] when variations.Any(v => v.PricingType == "VARIABLE_PRICING") => "Price Varies",
            _ => $"{variations.Min(v => v.PriceMoney.Amount)} - {variations.Max(v => v.PriceMoney.Amount)}" +
                 $" ({variations[0].PriceMoney.Currency})"
        };
    }

    public static string ParseId(string idInTextBlock) => idInTextBlock.TrimStart('#');
}

public static class SquareApiExt
{
    public static string PriceRangeAsString(this CatalogItem catalogItem)
    {
        var variations = catalogItem.Variations.Select(v => v.ItemVariationData).ToList();

        return variations switch
        {
            { Count: <= 0 } => throw new InvalidOperationException("Item has no variations"),
            { Count: 1 } => ItemVariation.PriceToString(variations[0]),
            [_, _, ..] when variations.Any(v => v.PricingType == "VARIABLE_PRICING") => "Price Varies",
            _ => $"{variations.Min(v => v.PriceMoney.Amount)} - {variations.Max(v => v.PriceMoney.Amount)}" +
                 $" ({variations[0].PriceMoney.Currency})"
        };
    }
}