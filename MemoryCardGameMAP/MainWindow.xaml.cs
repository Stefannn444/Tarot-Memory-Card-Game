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
        private UserService _userService;


        public MainWindow()
        {
            InitializeComponent();

            // Create services
            _userService = new UserService();

            // Create main view model
            _mainViewModel = new MainViewModel();

            // Set up login view model
            var loginViewModel = new LoginViewModel(_userService, OnUserLoggedIn);
            _mainViewModel.CurrentViewModel = loginViewModel;

            // Set the DataContext
            DataContext = _mainViewModel;
        }

        private void OnUserLoggedIn(User user)
        {
            // Navigate to game view with the selected user
            var gameViewModel = new GameViewModel(user, _userService, ReturnToLogin);
            _mainViewModel.CurrentViewModel = gameViewModel;
        }

        private void ReturnToLogin()
        {
            // Navigate back to login screen
            var loginViewModel = new LoginViewModel(_userService, OnUserLoggedIn);
            _mainViewModel.CurrentViewModel = loginViewModel;
        }
    }
}