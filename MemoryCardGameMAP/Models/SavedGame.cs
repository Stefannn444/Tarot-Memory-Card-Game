using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryCardGameMAP.Models
{
    public class SavedGame
    {
        public string Category { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int TimeRemaining { get; set; }
        public int ElapsedTime { get; set; }
        public List<SavedCard> Cards { get; set; } = new List<SavedCard>();
        public DateTime SavedDate { get; set; }
    }

    public class SavedCard
    {
        public string ImagePath { get; set; }
        public int PairId { get; set; }
        public bool IsFaceUp { get; set; }
        public bool IsMatched { get; set; }
    }
}
