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
using System.Windows.Shapes;

namespace MemoryCardGameMAP.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Credits:\n\n" +
                "Jîtea Ștefan-Alexandru\n" +
                "stefan.jitea@student.unitbv.ro\n" +
                "10LF232\n" +
                "Informatica",
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
