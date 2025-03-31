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
using MemoryCardGameMAP.Common;
using MemoryCardGameMAP.ViewModels;

namespace MemoryCardGameMAP.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        private readonly DialogViewModel _dialogVM;

        public DialogWindow(ViewModelBase mainVM, List<string> labels)
        {
            InitializeComponent();

            _dialogVM = new DialogViewModel();

            _dialogVM.CreateParameters(labels);

            DataContext = _dialogVM;
        }

        public List<double> GetValues()
        {
            return _dialogVM.GetValues();
        }
    }
}
