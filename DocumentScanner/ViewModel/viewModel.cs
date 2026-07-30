using CommunityToolkit.Mvvm.Input;
using Docnet.Core;
using Docnet.Core.Readers;
using DocumentScanner.Model;
using DocumentScanner.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace DocumentScanner.ViewModel
{
    public class viewModel : viewModelBase
    {
        #region PrivateValuables
        private string _allPage;
        private string _selectedPages;
        private string _fileName;
        private string _oCRresult;
        private IDocReader docReader;
        private AppWindow _appWindow;
        private LoadFileService _lfs;
        private FileDatas _data;
        private OCRService _ocrS;
        private ImageTransformationService _iTS;
        private int from;
        private int to;
        private bool _selectPageISChecked;
        private float _minZoomFactor;
        #endregion

        #region PublicProps
        public string AllPage
        {
            get { return _allPage; }
            set
            {
                if (_allPage != value)
                {
                    _allPage = value;
                    OnPropertyChanged(nameof(AllPage));
                }
            }
        }
        public float ZoomFactor
        {
            get { return _minZoomFactor; }
            set
            {
                if (_minZoomFactor != value)
                {
                    _minZoomFactor = value;
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }
        public string SelectedPages
        {
            get { return _selectedPages; }
            set
            {
                if (_selectedPages != value)
                {
                    _selectedPages = value;
                    OnPropertyChanged(nameof(SelectedPages));
                }
            }
        }
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }
        public string OCRresult
        {
            get { return _oCRresult; }
            set
            {
                if (_oCRresult != value)
                {
                    _oCRresult = value;
                    OnPropertyChanged(nameof(OCRresult));
                }
            }
        }
        public bool SelectPageISChecked
        {
            get { return _selectPageISChecked; }
            set
            {
                if (_selectPageISChecked != value)
                {
                    _selectPageISChecked = value;
                    OnPropertyChanged(nameof(SelectPageISChecked));
                }
            }
        }
        #endregion

        #region RelayCommands
        public RelayCommand OpenFile_Click { get; set; }
        public RelayCommand CloseFile_Click { get; set; }
        public RelayCommand Scanfile_Click { get; set; }
        public RelayCommand SelectPages_Click { get; set; }
        public RelayCommand ClearOCRResponse_Click { get; set; }
        public RelayCommand ClearSelection_Click { get; set; }
        #endregion

        #region ObservbC
        public ObservableCollection<PageData> Pages { get; set; }
        #endregion


        public viewModel(AppWindow appWindow)
        {
            _appWindow = appWindow;
            Init();
        }

        async Task Init()
        {
            AllPage = "There is no data to show";
            SelectedPages = "";
            FileName = "Select a document first";
            OpenFile_Click = new RelayCommand(openFilePicker);
            CloseFile_Click = new RelayCommand(DiscardCurrentFile);
            Scanfile_Click = new RelayCommand(Scanfile);
            SelectPages_Click = new RelayCommand(SelectPageRange);
            ClearOCRResponse_Click = new RelayCommand(ClearOCRResponse);
            ClearSelection_Click = new RelayCommand(ClearSelection);
            Pages = new ObservableCollection<PageData>();
            _lfs = new LoadFileService(_appWindow);
            _iTS = new ImageTransformationService();
            _ocrS = new OCRService();
            _ocrS.InitEngine();
        }

        private void ClearSelection()
        {
            from = -1;
            to = -1;
            SelectedPages = "Selected pages: All";
            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));
            }
        }

        private async void openFilePicker()
        {
            _data = await _lfs.openFilePicker();
            await CreatePagesforUI();
        }

        private void ClearOCRResponse()
        {
            OCRresult = "";
        }

        private void DiscardCurrentFile()
        {
            FileName = "Select a document first";
            AllPage = "There is no data to show";
            SelectedPages = "";
            Pages.Clear();
            OCRresult = "";
            if(_data != null)
                _data = null;
        }

        async Task CreatePagesforUI()
        {
            FileName = _data.file.Name;
            SelectedPages = "Selected pages: All";
            AllPage = "Document page count: 1";

            from = -1;
            to = -1;

            if(_data.FileExtension == ".pdf")
            {
                AllPage = "Document page count: " + _data.docReader.GetPageCount();
                for (int i = 0; i < _data.docReader.GetPageCount(); i++)
                {
                    using var page = _data.docReader.GetPageReader(i);

                    var bytes = page.GetImage();

                    var image = _iTS.CreateImageSourceAsync(bytes, page.GetPageWidth(), page.GetPageHeight());

                    PageData data = new PageData
                    {
                        PageSource = await image,
                        PageNumber = i + 1,
                        Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)),
                        Benabled = false,
                    };

                    data.SelectedPage = new RelayCommand(() =>
                    {
                        if (from == -1)
                        {
                            from = data.PageNumber - 1;
                            int temp = from + 1;
                            data.Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 252, 191));
                            SelectedPages = "Selected pages: " + temp + " -";
                        }
                        else if (to == -1)
                        {
                            to = data.PageNumber;
                            int temp = from + 1;
                            data.Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(100, 164, 227, 255));
                            SelectedPages = "Selected pages: " + temp + " - " + to;
                        }

                        for (int i = from + 1; i < to; i++)
                        {
                            Pages[i].Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(100, 164, 227, 255));
                        }

                        if (from != -1 && to != -1)
                        {
                            SelectPageISChecked = false;
                            for (int i = 0; i < Pages.Count; i++)
                            {
                                Pages[i].Benabled = false;

                            }
                        }
                    });

                    Pages.Add(data);
                }
            }
            else
            {
                BitmapImage bitmapImage = new BitmapImage();

                using var stream = await _data.file.OpenReadAsync();

                await bitmapImage.SetSourceAsync(stream);

                PageData data = new PageData
                {
                    PageSource = await _iTS.CreateImageSourceAsync(_data.file),
                    PageNumber = 1,
                    Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)),
                    Benabled = false,
                };

                Pages.Add(data);
            }

            
        }

        private async void Scanfile()
        {
            if(_data.FileExtension == ".pdf")
            {
                if (from == -1 || to == -1)
                {
                    from = 0;
                    to = _data.docReader.GetPageCount();
                }

                for (int i = from; i < to; i++)
                {
                    using var page = _data.docReaderHQ.GetPageReader(i);

                    var bytes = page.GetImage();


                    Bitmap bitmap = _iTS.CreateBitmap(bytes, page.GetPageWidth(), page.GetPageHeight());

                    string text = _ocrS.ReadText(bitmap);


                    //await _iTS.SaveBitmapAsync(bitmap);
                    OCRresult += text + "\n";
                }
            }
            else
            {
                Bitmap bitmap = new Bitmap(_data.file.Path);
                string text = _ocrS.ReadText(bitmap);
                OCRresult += text + "\n";
            }
            
        }

        private void SelectPageRange()
        {
            from = -1;
            to = -1;
            
            if (SelectPageISChecked)
            {
                SelectedPages = "Selected pages: All";
                for (int i = 0; i < Pages.Count; i++)
                {
                    Pages[i].Benabled = true;
                    Pages[i].Color = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));
                }
            }
            else
            {
                for (int i = 0; i < Pages.Count; i++)
                {
                    Pages[i].Benabled = false;
                }

            }
        }
    }
}
