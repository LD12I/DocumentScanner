using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;

namespace DocumentScanner.Services
{
    public class OCRService
    {
        private TesseractEngine _engine;

        public void InitEngine()
        {
            _engine = new TesseractEngine(@"./TesseractConfig","eng",EngineMode.Default);
        }

        public string ReadText(Bitmap bitmap)
        {
            string tempFile = Path.GetTempFileName() + ".png";

            bitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);

            using var img = Pix.LoadFromFile(tempFile);
            using var page = _engine.Process(img, PageSegMode.Auto);

            File.Delete(tempFile);

            return page.GetText();
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
