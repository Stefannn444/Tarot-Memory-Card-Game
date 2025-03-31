using MemoryCardGameMAP.Common;
using MemoryCardGameMAP.Models;
using MemoryCardGameMAP.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using MemoryCardGameMAP.Common;
using MemoryCardGameMAP.Models;
using MemoryCardGameMAP.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace MemoryCardGameMAP.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UserService _userService;
        private readonly Action<User> _onLoginSuccessful;

        // Collection of existing users
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        // Selected user for login
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                UpdateCommandsCanExecute();
            }
        }

        // New user creation properties
        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set
            {
                _newUsername = value;
                OnPropertyChanged();
                UpdateCommandsCanExecute();
            }
        }

        private string _selectedImagePath;
        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedImagePreview));
                UpdateCommandsCanExecute();
            }
        }

        public BitmapImage SelectedImagePreview
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedImagePath))
                    return null;

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(SelectedImagePath, UriKind.RelativeOrAbsolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private List<string> _availableImages;
        private int _currentImageIndex = 0;

        public string CurrentImagePath
        {
            get => _availableImages != null && _availableImages.Count > 0 ?
                   _availableImages[_currentImageIndex] : null;
        }

       
     

        // Commands
        public RelayCommand CreateUserCommand { get; private set; }
        public RelayCommand DeleteUserCommand { get; private set; }
        public RelayCommand BrowseImageCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand NextImageCommand { get; private set; }
        public RelayCommand PreviousImageCommand { get; private set; }

        public LoginViewModel(UserService userService, Action<User> onLoginSuccessful)
        {
            _userService = userService;
            _onLoginSuccessful = onLoginSuccessful;

            LoadAvailableImages();

            // Initialize commands
            NextImageCommand = new RelayCommand(param => NextImage(), param => CanNavigateImages());
            PreviousImageCommand = new RelayCommand(param => PreviousImage(), param => CanNavigateImages());
            CreateUserCommand = new RelayCommand(param=>CreateUser(), param=>CanCreateUser());
            DeleteUserCommand = new RelayCommand(param=>DeleteUser(), param=>CanDeleteUser());
            BrowseImageCommand = new RelayCommand(param=>BrowseImage());
            PlayCommand = new RelayCommand(param=>Play(), param=>CanPlay());

            // Load existing users
            LoadUsers();

            if (_availableImages.Count > 0)
                SelectedImagePath = _availableImages[0];
        }

        private void LoadAvailableImages()
        {
            _availableImages = new List<string>();
            try
            {
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AvatarImages");
                if (Directory.Exists(imagesDirectory))
                {
                    foreach (string file in Directory.GetFiles(imagesDirectory, "*.png"))
                    {
                        _availableImages.Add(file);
                    }
                    foreach (string file in Directory.GetFiles(imagesDirectory, "*.jpg"))
                    {
                        _availableImages.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log error
                System.Diagnostics.Debug.WriteLine($"Error loading images: {ex.Message}");
            }
        }
        private bool CanNavigateImages()
        {
            return _availableImages != null && _availableImages.Count > 1;
        }

        private void NextImage()
        {
            if (_availableImages.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex + 1) % _availableImages.Count;
                SelectedImagePath = _availableImages[_currentImageIndex];
            }
        }

        private void PreviousImage()
        {
            if (_availableImages.Count > 0)
            {
                _currentImageIndex = (_currentImageIndex - 1 + _availableImages.Count) % _availableImages.Count;
                SelectedImagePath = _availableImages[_currentImageIndex];
            }
        }
        private void LoadUsers()
        {
            var users = _userService.GetAllUsers();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private bool CanCreateUser()
        {
            return !string.IsNullOrWhiteSpace(NewUsername) &&
                   !string.IsNullOrWhiteSpace(SelectedImagePath) &&
                   !Users.Any(u => u.Username.Equals(NewUsername, StringComparison.OrdinalIgnoreCase));
        }

        private void CreateUser()
        {
            var newUser = new User
            {
                Username = NewUsername,
                ImagePath = SelectedImagePath
            };

            _userService.AddUser(newUser);
            Users.Add(newUser);

            // Reset form
            NewUsername = string.Empty;
            SelectedImagePath = string.Empty;
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }

        private void DeleteUser()
        {
            _userService.DeleteUser(SelectedUser.Username);
            Users.Remove(SelectedUser);
            SelectedUser = null;
        }

        /* private void BrowseImage()
         {
             var dialog = new OpenFileDialog
             {
                 Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif",
                 Title = "Select user image"
             };

             if (dialog.ShowDialog() == true)
             {
                 // Store relative path if possible
                 string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                 string filePath = dialog.FileName;

                 // Try to make path relative if file is within app directory
                 if (filePath.StartsWith(projectDirectory))
                 {
                     filePath = filePath.Substring(projectDirectory.Length);
                 }

                 SelectedImagePath = filePath;
             }
         }*/
        private void BrowseImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Select user image"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Create images directory if it doesn't exist
                    string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    // Generate unique filename
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dialog.FileName)}";
                    string destinationPath = Path.Combine(imagesDirectory, fileName);

                    // Copy the selected image to our app's directory
                    File.Copy(dialog.FileName, destinationPath);

                    // Store the relative path
                    SelectedImagePath = Path.Combine("UserImages", fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanPlay()
        {
            return SelectedUser != null;
        }

        private void Play()
        {
            _onLoginSuccessful?.Invoke(SelectedUser);
        }

        private void UpdateCommandsCanExecute()
        {
            CreateUserCommand.RaiseCanExecuteChanged();
            DeleteUserCommand.RaiseCanExecuteChanged();
            PlayCommand.RaiseCanExecuteChanged();
        }
    }
}
