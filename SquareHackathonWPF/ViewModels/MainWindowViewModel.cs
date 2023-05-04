﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;

namespace SquareHackathonWPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public  WaveIn?       WaveIn { get; set; }

    // Modify code in the future to choose between sandbox and production modes.
    private SquareClient Client { get; } = new SquareClient.Builder()
        .Environment(Square.Environment.Sandbox)
        .AccessToken(App.GetSquareAccessToken())
        .Build();

    internal async Task<string> ShowPayments()
    {
        string messageBoxContent;

        // Generate the text for the dialog box.
        try
        {
            // Retrieve the list of all payments.
            var result = await Client.PaymentsApi.ListPaymentsAsync();

            if (result.Payments != null)
            {
                // Display the individual payments in the message box.
                var paymentsList = result.Payments
                    .Select(payment => $"[Payment ID: {payment.Id}, " +
                                       $"Amount: {payment.AmountMoney.Amount}, " +
                                       $"Status: {payment.Status}]");
                messageBoxContent = string.Join("\n", paymentsList);
            }
            else
            {
                // Display a message if no payments are found.
                messageBoxContent = "No payments found";
            }
        }
        catch (ApiException e)
        {
            // Display an error message if the request fails.
            var errorMessage = "";

            errorMessage += $"Failed to make the request\n";
            errorMessage += $"Response Code: {e.ResponseCode}\n";
            errorMessage += $"Exception: {e.Message}\n";

            messageBoxContent = errorMessage;
        }

        return messageBoxContent;
    }

    private void ShowInventory()
    {
        // Create a new instance of the CatalogApi client
        var catalogApi = Client.CatalogApi;

        // Use the ListCatalog method to retrieve the items in your inventory
        var items = catalogApi.ListCatalogAsync(types: "item" ).Result.Objects;

        // Loop through the list of items and do something with each item
        foreach (var item in items)
        {
            // Do something with the item, such as display its name or price
            Console.WriteLine($"Item name: {item.ItemData.Name}");
            //Console.WriteLine($"Item price: {item.ItemData.Variations.First().PriceMoney.Amount / 100.0m}");
        }
    }

    private async void AddNewItem()
    {
        var cocoaVariationSmall = new CatalogItemVariation.Builder()
            .ItemId("#Cocoa")
            .Name("Small")
            .PricingType("VARIABLE_PRICING")
            .Build();

        var catalogObjectSmall = new CatalogObject.Builder(type: "ITEM_VARIATION", id: "#Small")
            .ItemVariationData(cocoaVariationSmall)
            .Build();

        var priceMoney = new Money.Builder()
            .Amount(400L)
            .Currency("USD")
            .Build();

        var cocoaVariationLarge = new CatalogItemVariation.Builder()
            .ItemId("#Cocoa")
            .Name("Large")
            .PricingType("FIXED_PRICING")
            .PriceMoney(priceMoney)
            .Build();

        var catalogObjectLarge = new CatalogObject.Builder(type: "ITEM_VARIATION", id: "#Large")
            .ItemVariationData(cocoaVariationLarge)
            .Build();

        var variationsAsCatalogObjects = new List<CatalogObject> {
            catalogObjectSmall,
            catalogObjectLarge
        };

        var itemData = new CatalogItem.Builder()
            .Name("Cocoa")
            .Description("Hot Chocolate")
            .Abbreviation("Ch")
            .Variations(variationsAsCatalogObjects)
            .Build();

        var @object = new CatalogObject.Builder(type: "ITEM", id: "#Cocoa")
            .ItemData(itemData)
            .Build();

        var body = new UpsertCatalogObjectRequest.Builder(idempotencyKey: "af3d1afc-7212-4300-b463-0bfc5314a5ae", mObject: @object)
            .Build();

        try
        {
            var result = await Client.CatalogApi.UpsertCatalogObjectAsync(body: body);
        }
        catch (ApiException e)
        {
            Console.WriteLine("Failed to make the request");
            Console.WriteLine($"Response Code: {e.ResponseCode}");
            Console.WriteLine($"Exception: {e.Message}");
        }
    }

    private void StartRecording()
    {
        WaveIn = new() { DeviceNumber = 0 }; // Change this to the appropriate device number
        WaveIn.WaveFormat = new (44100, WaveIn.GetCapabilities(WaveIn.DeviceNumber).Channels);
        WaveIn.DataAvailable += WaveIn_DataAvailable;
        WaveIn.StartRecording();
    }

    private void StopRecording()
    {
        if (WaveIn == null) return;
        WaveIn.StopRecording();
        WaveIn.Dispose();
    }

    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        // Do something with the audio data, such as write it to a file
    }

    internal static void GetCapibilities()
    {
        var deviceCount = WaveIn.DeviceCount;

        for (var i = 0; i < deviceCount; i++) {
            var capabilities = WaveIn.GetCapabilities(i);
            Console.WriteLine($"Device {i}: {capabilities.ProductName}, {capabilities.Channels} channels");
        }
    }
}