using Avalonia.Controls;
using Square.Exceptions;
using System;
using System.Linq;
using Avalonia;
using Square;
using Square.Http.Client;

namespace SquareHackathonAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ShowPayments();
    }

    private async void ShowPayments()
    {
        var accessToken = App.GetSquareAccessToken();
        var client = new SquareClient.Builder()
            .Environment(Square.Environment.Sandbox)
            .AccessToken(accessToken)
            .Build();

        var dialogBox = new Window
        {
            Width = 400, Height = 60, Padding = new (12)
        };

        try
        {
            var result = await client.PaymentsApi.ListPaymentsAsync();
            if (result.Payments != null)
            {
                var paymentsList = result.Payments
                    .Select(payment => $"Payment ID: {payment.Id}, Amount: {payment.AmountMoney.Amount}, Status: {payment.Status}")
                    .ToList();
                dialogBox.Content = string.Join("\n", paymentsList);
            }
            else
            {
                dialogBox.Content = "No payments found";
            }
        }
        catch (ApiException e)
        {
            var errorMessage = "";

            errorMessage += $"Failed to make the request\n";
            errorMessage += $"Response Code: {e.ResponseCode}\n";
            errorMessage += $"Exception: {e.Message}\n";

            dialogBox.Content = errorMessage;
        }
        finally
        {
            await dialogBox.ShowDialog(this);
        }
    }
}