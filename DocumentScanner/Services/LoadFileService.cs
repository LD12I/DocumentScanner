using Docnet.Core;
using Docnet.Core.Readers;
using DocumentScanner.Model;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DocumentScanner.Services
{
    public class LoadFileService
    {
        private AppWindow _appWindow;
        public LoadFileService(AppWindow appWindow) 
        {
            _appWindow = appWindow;
        }

        public async Task<FileDatas> openFilePicker()
        {
            var openPicker = new FileOpenPicker(_appWindow.Id)
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                CommitButtonText = "Choose selected files",
                FileTypeFilter = { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" },
                ViewMode = PickerViewMode.List,
            };

            var result = await openPicker.PickSingleFileAsync() ;
            FileDatas datas = new FileDatas();
            if (result is not null)
            {
                if(Path.GetExtension(result.Path) == ".pdf")
                {
                    IDocReader docReader = DocLib.Instance.GetDocReader(result.Path, new Docnet.Core.Models.PageDimensions(1200, 1600));

                    IDocReader docReaderHQ = DocLib.Instance.GetDocReader(result.Path, new Docnet.Core.Models.PageDimensions(3000, 4000));

                    datas.docReader = docReader;
                    datas.docReaderHQ = docReaderHQ;
                    datas.file = await StorageFile.GetFileFromPathAsync(result.Path);
                    datas.FileExtension = ".pdf";
                    
                }
                else
                {
                    datas.FileExtension = Path.GetExtension(result.Path);
                    datas.docReader = null;
                    datas.docReaderHQ = null;
                    datas.file = await StorageFile.GetFileFromPathAsync(result.Path);
                }


                return datas;
            }
            else
            {
                Debug.WriteLine("File Selection cancelled.");
                
            }
            return null;
        }
    }
}
