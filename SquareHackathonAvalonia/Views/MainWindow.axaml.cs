using Avalonia.Controls;
using Square.Exceptions;
using System;

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
        try
        {
            var result = await client.PaymentsApi.ListPaymentsAsync();
        }
        catch (ApiException e)
        {
            Console.WriteLine("Failed to make the request");
            Console.WriteLine($"Response Code: {e.ResponseCode}");
            Console.WriteLine($"Exception: {e.Message}");
        }
    }
}