using MemoryCardGameMAP.Common;
using MemoryCardGameMAP.Models;
using MemoryCardGameMAP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace MemoryCardGameMAP.ViewModels
{
    class DialogViewModel : ViewModelBase
    {
        public void CreateParameters(List<string> labels)
        {
            Height = (labels.Count + 3) * 40;

            foreach (var label in labels)
            {
                Parameters.Add(new DialogParameter()
                {
                    ParamText = label,
                    Height = 20,
                });
            }
        }

        public List<double> GetValues()
        {
            var values = new List<double>();

            foreach (var parameter in Parameters)
            {
                string text = parameter.InputText;
                if (text == null || text.Trim().Length == 0 || IsNumeric(text) == false)
                    values.Add(0);
                else
                    values.Add(double.Parse(text));
            }

            return values;
        }

        private bool IsNumeric(string text)
        {
            return double.TryParse(text, out _);
        }

        #region Properties and commands
        public double Height { get; set; }


        public ObservableCollection<DialogParameter> Parameters { get; } =
            new ObservableCollection<DialogParameter>();

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(p =>
                    {
                        var window = Application.Current.Windows.OfType<DialogWindow>().SingleOrDefault(w => w.IsActive);
                        window?.Close();
                    });

                return _closeCommand;
            }
        }
        #endregion
    }
}
