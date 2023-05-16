using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DynamicData;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using NAudio.WaveFormRenderer;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;
using SquareHackathonWPF.Models.SquareApi;

namespace SquareHackathonWPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // ReSharper disable once StringLiteralTypo
    private SpeechConfig SpeechConfig { get; } = SpeechConfig.FromSubscription(App.GetAzureKey1(), region: "eastus");
    private AudioConfig AudioConfig { get; }= AudioConfig.FromWavFileInput("path/to/your/audio/file.wav");
    public MainWindowViewModel(MainWindow mainWindow) => Window = mainWindow;

    internal Image Image { get; set; } = new Bitmap(1, 1);
    public   WaveIn?             WaveIn { get; set; }

    /// <summary>
    /// A list of items in the inventory.
    /// </summary>
    /// <remarks><b>Please ensure the type of the catalog object is item before adding to the list.</b></remarks>
    internal List<CatalogObject> Items { get; } = new();

    internal bool IsRecording { get; set; }
    public MainWindow Window { get; set; }

    #region Test Methods for Square API
    internal async Task<string> ShowPayments()
    {
        string messageBoxContent;

        // Generate the text for the dialog box.
        try
        {
            // Retrieve the list of all payments.
            var result = await App.Client.PaymentsApi.ListPaymentsAsync();

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
        var catalogApi = App.Client.CatalogApi;

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

    internal async void AddNewItemTest()
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
            .Currency("CAD")
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
            var result = await App.Client.CatalogApi.UpsertCatalogObjectAsync(body: body);
        }
        catch (ApiException e)
        {
            Console.WriteLine("Failed to make the request");
            Console.WriteLine($"Response Code: {e.ResponseCode}");
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
    #endregion

    internal async void RecordSpeech()
    {
        using var recognizer = new SpeechRecognizer(SpeechConfig, AudioConfig);
        // Configure additional recognizer settings if needed

        // Start speech recognition
        var result = await recognizer.RecognizeOnceAsync();

        // Process the recognition result
        if (result.Reason != ResultReason.RecognizedSpeech) return;

        var transcribedText = result.Text;
        // Handle the transcribed text as needed
        Window.CaptionBlock.Text = transcribedText;
    }

    internal void StartRecording()
    {
        WaveIn = new() { DeviceNumber = 0 }; // Change this to the appropriate device number
        WaveIn.WaveFormat = new (44100, WaveIn.GetCapabilities(WaveIn.DeviceNumber).Channels);
        WaveIn.DataAvailable += OnDataAvailable;
        WaveIn.StartRecording();
    }

    internal void StopRecording()
    {
        if (WaveIn == null) return;
        WaveIn.StopRecording();
        WaveIn.Dispose();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var audioData = e.Buffer;
        
        var waveBuffer = new WaveBuffer(audioData);
        
        var rmsPeakProvider = new RmsPeakProvider(200);

        var rendererSettings = new StandardWaveFormRendererSettings {
            Width = 640,
            TopHeight = 32,
            BottomHeight = 0
        };

        var renderer = new WaveFormRenderer();
        WaveStream waveStream = new RawSourceWaveStream(new MemoryStream(waveBuffer.ByteBuffer), WaveIn?.WaveFormat);
        
        Image = renderer.Render(waveStream, rmsPeakProvider, rendererSettings);
    }


    internal static void GetCapabilities()
    {
        var deviceCount = WaveIn.DeviceCount;

        for (var i = 0; i < deviceCount; i++) {
            var capabilities = WaveIn.GetCapabilities(i);
            Console.WriteLine($"Device {i}: {capabilities.ProductName}, {capabilities.Channels} channels");
        }
    }

    public async Task<IList<CatalogObject>?> RetrieveInventory()
    {
        try {
            var result = await App.Client.CatalogApi.ListCatalogAsync(types: "ITEM");
            return result.Objects;
        }
        catch (ApiException e) {
            MessageBox.Show("Failed to make the request\n" +
                            $"Response Code: {e.ResponseCode}" +
                            $"Exception: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Updates the item in the list of items, <see cref="Items"/>, by replacing the item with the same ID.
    /// </summary>
    /// <param name="item"></param>
    internal void UpdateItem(CatalogObject item)
        => Items[Items.FindIndex(it => it.Id == item.Id)] = item;

    public static ImageSource ConvertToImageSource(Image drawingImage)
    {
        using var memoryStream = new MemoryStream();
        // Save the System.Drawing.Image to a memory stream in PNG format
        drawingImage.Save(memoryStream, ImageFormat.Png);

        // Create a BitmapImage and set the memory stream as its source
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
        bitmapImage.EndInit();

        // Create a PngBitmapEncoder and add the BitmapImage as a frame
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

        // Create a new MemoryStream for the encoded image data
        var encodedStream = new MemoryStream();
        encoder.Save(encodedStream);

        // Create a BitmapImage from the encoded image data
        var finalBitmapImage = new BitmapImage();
        finalBitmapImage.BeginInit();
        finalBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        finalBitmapImage.StreamSource = new MemoryStream(encodedStream.ToArray());
        finalBitmapImage.EndInit();

        return finalBitmapImage;
    }

}