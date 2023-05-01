using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Square.Exceptions;
using Square;
using Avalonia.A;

namespace SquareHackathonAvalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";
    internal static async Task<Window> ShowPayments()
    {
        // Get client from access token.
        var accessToken = App.GetSquareAccessToken();
        var client = new SquareClient.Builder()
            .Environment(Square.Environment.Sandbox)
            .AccessToken(accessToken)
            .Build();

        // Create a dialog box to display the result.
        var dialogBox = new Window
        {
            Width = 400,
            Height = 60,
            Padding = new(12)
        };

        // Generate content for the dialog box.
        try
        {
            // Call ListPayments() to get all payments.
            var result = await client.PaymentsApi.ListPaymentsAsync();

            if (result.Payments != null)
            {
                // Display the result in the dialog box.
                var paymentsList = result.Payments
                    .Select(payment => $"Payment ID: {payment.Id}, Amount: {payment.AmountMoney.Amount}, Status: {payment.Status}")
                    .ToList();
                dialogBox.Content = string.Join("\n", paymentsList);
            }
            else
            {
                // Display a message if no payments are found.
                dialogBox.Content = "No payments found";
            }
        }
        catch (ApiException e)
        {
            // Display an error message if the request fails.
            var errorMessage = "";

            errorMessage += $"Failed to make the request\n";
            errorMessage += $"Response Code: {e.ResponseCode}\n";
            errorMessage += $"Exception: {e.Message}\n";

            dialogBox.Content = errorMessage;
        }
        return dialogBox;
    }

    private async void StartRecording()
    {
        _audioRecorder = new AudioRecorder();
        await _audioRecorder.StartRecordingAsync();
        _audioRecorder.AudioDataAvailable += AudioRecorder_AudioDataAvailable;
    }

    private async void StopRecording()
    {
        await _audioRecorder.StopRecordingAsync();
        _audioRecorder.AudioDataAvailable -= AudioRecorder_AudioDataAvailable;
        _audioRecorder.Dispose();
    }

    private void AudioRecorder_AudioDataAvailable(object sender, byte[] audioData)
    {
        // Do something with the audio data, such as write it to a file
    }

}