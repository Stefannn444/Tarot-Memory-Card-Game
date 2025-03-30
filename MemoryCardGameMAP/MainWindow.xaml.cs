using MemoryCardGameMAP.Models;
using MemoryCardGameMAP.Services;
using MemoryCardGameMAP.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemoryCardGameMAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Create services
            var userService = new UserService();

            // Create main view model
            _mainViewModel = new MainViewModel();

            // Set up login view model
            var loginViewModel = new LoginViewModel(userService, OnUserLoggedIn);
            _mainViewModel.CurrentViewModel = loginViewModel;

            // Set the DataContext
            DataContext = _mainViewModel;
        }

        private void OnUserLoggedIn(User user)
        {
            // TODO: Navigate to game view with the selected user
            MessageBox.Show($"User {user.Username} logged in successfully!");

            // Here you would set _mainViewModel.CurrentViewModel to your game view model
        }
    }
}