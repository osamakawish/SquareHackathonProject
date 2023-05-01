using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;
using Square;
using Square.Exceptions;

namespace SquareHackathonWPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public WaveIn? WaveIn { get; set; }
    
    internal static async Task<string> ShowPayments()
    {
        // Get client from access token.
        var accessToken = App.GetSquareAccessToken();
        var client = new SquareClient.Builder()
            .Environment(Square.Environment.Sandbox)
            .AccessToken(accessToken)
            .Build();

        string messageBoxContent;

        // Generate content for the dialog box.
        try
        {
            // Call ListPayments() to get all payments.
            var result = await client.PaymentsApi.ListPaymentsAsync();

            if (result.Payments != null)
            {
                // Display the result in the dialog box.
                var paymentsList = result.Payments
                    .Select(payment => $"[Payment ID: {payment.Id}, " +
                                       $"Amount: {payment.AmountMoney.Amount}, " +
                                       $"Status: {payment.Status}]")
                    .ToList();
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