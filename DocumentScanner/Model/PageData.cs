using CommunityToolkit.Mvvm.Input;
using DocumentScanner.ViewModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentScanner.Model
{
    public class PageData : viewModelBase
    {
        private bool _benabled;
        private SolidColorBrush _color;

        public SoftwareBitmapSource PageSource { get; set; }
        public int PageNumber { get; set; }
        
        public bool Benabled
        {
            get => _benabled;
            set
            {
                _benabled = value;
                OnPropertyChanged(nameof(Benabled));
            }
        }
        public RelayCommand SelectedPage {  get; set; }
        public SolidColorBrush Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }
}
