using MemoryCardGameMAP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryCardGameMAP.Models
{
    public class User: ViewModelBase
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        private int _gamesPlayed;
        public int GamesPlayed
        {
            get => _gamesPlayed;
            set
            {
                _gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int _gamesWon;
        public int GamesWon
        {
            get => _gamesWon;
            set
            {
                _gamesWon = value;
                OnPropertyChanged();
            }
        }

        public User()
        {
            GamesPlayed = 0;
            GamesWon = 0;
        }
    }
}
