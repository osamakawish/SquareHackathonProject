using Avalonia.Controls;
using Square.Exceptions;
using System;
using System.Linq;
using Avalonia;
using Square;
using Square.Http.Client;
using NAudio.Wave;
using SquareHackathonAvalonia.ViewModels;

namespace SquareHackathonAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += async delegate {
            var window = await MainWindowViewModel.ShowPayments();
            await window.ShowDialog(this);
        };
    }
}