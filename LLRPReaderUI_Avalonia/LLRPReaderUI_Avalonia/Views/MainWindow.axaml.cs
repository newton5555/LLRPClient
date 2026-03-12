using Avalonia.Controls;
using LLRPReaderUI_Avalonia.ViewModels;

namespace LLRPReaderUI_Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
