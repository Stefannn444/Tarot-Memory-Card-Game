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
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;
        private UserService _userService;


        public MainWindow()
        {
            InitializeComponent();

            _userService = new UserService();

            _mainViewModel = new MainViewModel();

            var loginViewModel = new LoginViewModel(_userService, OnUserLoggedIn);
            _mainViewModel.CurrentViewModel = loginViewModel;

            DataContext = _mainViewModel;
        }

        private void OnUserLoggedIn(User user)
        {
            var gameViewModel = new GameViewModel(user, _userService, ReturnToLogin);
            _mainViewModel.CurrentViewModel = gameViewModel;
        }

        private void ReturnToLogin()
        {
            var loginViewModel = new LoginViewModel(_userService, OnUserLoggedIn);
            _mainViewModel.CurrentViewModel = loginViewModel;
        }
    }
}