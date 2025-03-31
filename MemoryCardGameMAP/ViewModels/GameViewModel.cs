using MemoryCardGameMAP.Common;
using MemoryCardGameMAP.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Windows;
using MemoryCardGameMAP.Services;
using System.IO;
using MemoryCardGameMAP.Views;
using System.Text;

namespace MemoryCardGameMAP.ViewModels
{
    //TODO: clear th ebaord when time s up
    //TODO: BUG cu o carte rasucita cand dau save
    //TODO: categoriile
    //TODO: aspectul?
    //TODO: ABOUT
    //todo: timp custom
    //todo: cand dau open la un joc, pot sa l termin de mai multe ori, si, astfel, farmez winuri
    public class GameViewModel : ViewModelBase
    {
        private readonly ViewModelBase _viewModelBase;

        private readonly User _currentUser;
        private readonly UserService _userService;
        private readonly DispatcherTimer _gameTimer;
        private readonly Action _onReturnToLogin;

        // Game properties
        private ObservableCollection<CardViewModel> _cards = new ObservableCollection<CardViewModel>();
        private int _timeRemaining;
        private int _pairsMatched;
        private int _pairsTotal;
        private CardViewModel _firstSelectedCard;
        private bool _canSelectCard = true;

        public ObservableCollection<CardViewModel> Cards => _cards;

        private int _rows = 4; // Default values
        private int _columns = 4;

        public int Rows
        {
            get => _rows;
            set
            {
                _rows = value;
                OnPropertyChanged();
            }
        }

        public int Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                OnPropertyChanged();
            }
        }

        public int TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public int PairsMatched
        {
            get => _pairsMatched;
            set
            {
                _pairsMatched = value;
                OnPropertyChanged();
            }
        }

        private string _selectedCategory = "Tarot"; // default option
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }


        //TODO unique selection
        private bool _isStandardMode = true; // Default to standard 4x4
        public bool IsStandardMode
        {
            get => _isStandardMode;
            set
            {
               
                if (_isStandardMode != value)
                {
                    _isStandardMode = value;
                    if (value) IsCustomMode = false;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCustomMode = false;
        public bool IsCustomMode
        {
            get => _isCustomMode;
            set
            {
                
                if (_isCustomMode != value)
                {
                    _isCustomMode = value;
                    if (value) IsStandardMode = false;
                    OnPropertyChanged();
                }
            }
        }

        public int PairsTotal => _pairsTotal;

        // Commands
        public RelayCommand NewGameCommand { get; private set; }
        public RelayCommand OpenGameCommand { get; private set; }
        public RelayCommand SaveGameCommand { get; private set; }
        public RelayCommand ShowStatisticsCommand { get; private set; }
        public RelayCommand ExitCommand { get; private set; }
        public RelayCommand SetCategoryCommand { get; private set; }
        public RelayCommand SetGameModeCommand { get; private set; }


        public GameViewModel(User currentUser, UserService userService, Action onReturnToLogin)
        {
            _currentUser = currentUser;
            _userService = userService;
            _onReturnToLogin = onReturnToLogin;

            // Initialize timer
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += OnTimerTick;

            // Initialize commands
            NewGameCommand = new RelayCommand(param => StartNewGame());
            OpenGameCommand = new RelayCommand(param => OpenGame());
            SaveGameCommand = new RelayCommand(param => SaveGame());
            ShowStatisticsCommand = new RelayCommand(param => ShowStatistics());
            ExitCommand = new RelayCommand(param => Exit());
            SetCategoryCommand = new RelayCommand(param => SetCategory(param as string));
            SetGameModeCommand = new RelayCommand(param => SetGameMode(param as string));

        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TimeRemaining--;

            if (TimeRemaining <= 0)
            {
                _gameTimer.Stop();
                MessageBox.Show("Time's up! Game over.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetGameMode(string mode)
        {
            if(mode=="Standard")
            {
                IsStandardMode = true;
                IsCustomMode = false;
                Rows = 4;
                Columns = 4;
            }
            else if (mode == "Custom")
            {
                IsCustomMode = true;
                IsStandardMode = false;

                List<string> labels = new List<string>
                {
                    "Rows (2-6): ",
                    "Columns (2-6): "
                };
                DialogWindow window = new DialogWindow(_viewModelBase, labels);
                window.ShowDialog();
                List<double> parameters=window.GetValues();
                Rows = (int)parameters[0]; 
                Columns = (int)parameters[1];
                if(Rows>6||Rows<2||Columns>6||Columns<2
                    || (Rows * Columns) % 2 == 1)
                {
                    MessageBox.Show("Total number of cards must be even, rows and columns are between 2 and 6!");
                    Rows = 4;
                    Columns = 4;
                    IsStandardMode = true;
                    IsCustomMode=false;
                }
            }
        }

        private void SetCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                SelectedCategory = category;
            }
        }
        public void StartNewGame()
        {
            string category = SelectedCategory;
            _cards.Clear();
            PairsMatched = 0;

            _pairsTotal = (Rows*Columns)/2;
            TimeRemaining = 60 + (_pairsTotal * 10);

            // Load images for category
            string[] imagePaths = LoadImagesForCategory(category);

            // Create pairs and add to collection
            Random rnd = new Random();
            
            var shuffledImages=imagePaths.OrderBy(x=>rnd.Next()).ToArray();

            for (int i = 0; i < _pairsTotal; i++)
            {
                string imagePath = shuffledImages[i % shuffledImages.Length];

                // Create two cards with the same image
                var card1 = new CardViewModel(imagePath, i);
                var card2 = new CardViewModel(imagePath, i);

                card1.CardClicked += OnCardClicked;
                card2.CardClicked += OnCardClicked;

                _cards.Add(card1);
                _cards.Add(card2);
            }

            // Shuffle cards
            ShuffleCardPlacement();

            // Start timer
            _gameTimer.Start();
            _currentUser.GamesPlayed++;
            _userService.UpdateUser(_currentUser);
        }

        private string[] LoadImagesForCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                MessageBox.Show("No category specified. Loading default images.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new string[0];
            }

            // Define the path to the category folder
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cards", category);

            if (!Directory.Exists(basePath))
            {
                MessageBox.Show($"Category '{category}' not found. Please check your Cards folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new string[0];
            }

            // Load all image files from the category folder (assuming PNG and JPG formats)
            string[] imageFiles = Directory.GetFiles(basePath, "*.png")
                                           //.Concat(Directory.GetFiles(basePath, "*.jpg"))
                                           .ToArray();

            if (imageFiles.Length == 0)
            {
                MessageBox.Show($"No images found in the '{category}' category.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new string[0];
            }

            return imageFiles;
        }

        private void ShuffleCardPlacement()
        {
            var list = _cards.ToList();
            Random rnd = new Random();

            var shuffled = list.OrderBy(x => rnd.Next()).ToList();

            _cards.Clear();
            foreach (var card in shuffled)
            {
                _cards.Add(card);
            }
        }

        private void OnCardClicked(CardViewModel card)
        {
            if (!_canSelectCard || card.IsMatched || card.IsFaceUp)
                return;

            // Flip card
            card.IsFaceUp = true;

            if (_firstSelectedCard == null)
            {
                // First card of pair
                _firstSelectedCard = card;
            }
            else
            {
                // Second card of pair
                _canSelectCard = false;

                // Check if cards match
                if (_firstSelectedCard.PairId == card.PairId)
                {
                    // Match found
                    _firstSelectedCard.IsMatched = true;
                    card.IsMatched = true;
                    PairsMatched++;

                    if (PairsMatched >= PairsTotal)
                    {
                        _gameTimer.Stop();
                        _currentUser.GamesWon++;
                        _userService.UpdateUser(_currentUser);
                        MessageBox.Show("Congratulations! You've matched all pairs!", "Game Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    }

                    _firstSelectedCard = null;
                    _canSelectCard = true;
                }
                else
                {
                    // No match
                    var firstCard = _firstSelectedCard;

                    // Delay before flipping back
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    timer.Tick += (sender, e) =>
                    {
                        timer.Stop();
                        firstCard.IsFaceUp = false;
                        card.IsFaceUp = false;
                        _firstSelectedCard = null;
                        _canSelectCard = true;
                    };
                    timer.Start();
                }
            }
        }

        private void OpenGame()
        {
            try
            {
                // Check if saved game exists
                string saveFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedGames", $"{_currentUser.Username}.json");
                if (!File.Exists(saveFile))
                {
                    MessageBox.Show("No saved game found.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Load saved game
                string json = File.ReadAllText(saveFile);
                var savedGame = System.Text.Json.JsonSerializer.Deserialize<SavedGame>(json);

                // Stop current game if running
                _gameTimer.Stop();

                // Restore game state
                SelectedCategory = savedGame.Category;
                Rows = savedGame.Rows;
                Columns = savedGame.Columns;
                TimeRemaining = savedGame.TimeRemaining;
                _pairsTotal = (Rows * Columns) / 2;

                // Clear current cards
                _cards.Clear();
                foreach (var item in savedGame.Cards)
                {
                    var card = new CardViewModel(item.ImagePath, item.PairId);
                    card.IsFaceUp = item.IsFaceUp;
                    card.IsMatched = item.IsMatched;
                    card.CardClicked += OnCardClicked;
                    _cards.Add(card);
                }

                // Count matched pairs
                PairsMatched = _cards.Count(c => c.IsMatched) / 2;

                // Reset selection state
                _firstSelectedCard = null;
                _canSelectCard = true;

                // Resume timer
                _gameTimer.Start();

                MessageBox.Show("Game loaded successfully!", "Open Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGame()
        {
            try
            {
                // Create game state to save
                var gameState = new SavedGame
                {
                    Category = SelectedCategory,
                    Rows = Rows,
                    Columns = Columns,
                    TimeRemaining = TimeRemaining,
                    ElapsedTime = 120 - TimeRemaining, // Assuming 120 was initial time
                    SavedDate = DateTime.Now,
                    Cards = _cards.Select(c => new SavedCard
                    {
                        ImagePath = c.ImagePath,
                        PairId = c.PairId,
                        IsFaceUp = c.IsFaceUp,
                        IsMatched = c.IsMatched
                    }).ToList()
                };

                // Pause the timer
                _gameTimer.Stop();

                // Create directory if it doesn't exist
                string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedGames");
                Directory.CreateDirectory(saveDir);

                // Save to user-specific file
                string saveFile = Path.Combine(saveDir, $"{_currentUser.Username}.json");
                string json = System.Text.Json.JsonSerializer.Serialize(gameState);
                File.WriteAllText(saveFile, json);

                MessageBox.Show("Game saved successfully!", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _gameTimer.Start(); // Restart timer if save failed
            }

        }

        private void ShowStatistics()
        {
            var allUsers = _userService.GetAllUsers();

            // Create a formatted string with all users' statistics
            StringBuilder statsBuilder = new StringBuilder("User Statistics:\n\n");

            foreach (var user in allUsers)
            {
                statsBuilder.AppendLine($"Username: {user.Username}");
                statsBuilder.AppendLine($"Games Played: {user.GamesPlayed}");
                statsBuilder.AppendLine($"Games Won: {user.GamesWon}");
                statsBuilder.AppendLine();
            }

            MessageBox.Show(statsBuilder.ToString(), "Game Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit()
        {
            _gameTimer.Stop();
            _onReturnToLogin?.Invoke();
        }
    }

    public class CardViewModel : ViewModelBase
    {
        public event Action<CardViewModel> CardClicked;

        private string _imagePath;
        private int _pairId;
        private bool _isFaceUp;
        private bool _isMatched;

        public string ImagePath => _imagePath;
        public int PairId => _pairId;

        public bool IsFaceUp
        {
            get => _isFaceUp;
            set
            {
                _isFaceUp = value;
                OnPropertyChanged();
            }
        }

        public bool IsMatched
        {
            get => _isMatched;
            set
            {
                _isMatched = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ClickCommand { get; private set; }

        public CardViewModel(string imagePath, int pairId)
        {
            _imagePath = imagePath;
            _pairId = pairId;

            ClickCommand = new RelayCommand(param => OnClick());
        }

        private void OnClick()
        {
            CardClicked?.Invoke(this);
        }
    }
}