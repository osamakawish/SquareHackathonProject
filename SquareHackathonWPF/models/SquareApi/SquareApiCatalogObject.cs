using Square.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquareHackathonWPF.Models.SquareApi;

internal interface ISquareApiCatalogObject
{
    string Id { get; }
    CatalogObject AsCatalogObject { get; }
}

internal record ItemVariation(string Id, CatalogItemVariation Variation) : ISquareApiCatalogObject
{
    public CatalogObject AsCatalogObject { get; } = new(type: "ITEM_VARIATION", id: Id, itemVariationData: Variation);
}

internal record Item(string Id, CatalogItem CatalogItem) : ISquareApiCatalogObject
{
    public CatalogObject AsCatalogObject { get; } = new(type: "ITEM", id: Id, itemData: CatalogItem);
}