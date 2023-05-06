using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using SquareHackathonWPF.ViewModels;
using SquareHackathonWPF.Views.Forms;

namespace SquareHackathonWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.Maximized;

        DataContext = ViewModel;
    }

    private void RecordButtonClick(object sender, RoutedEventArgs e) { }

    private void AddItemButtonClick(object sender, RoutedEventArgs e)
    {
        var window = new AddItemWindow();
        window.ShowDialog();
    }
}