using ABI.Windows.Foundation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DocumentScanner.Services
{
    public class ImageTransformationService
    {
        public async Task<SoftwareBitmapSource> CreateImageSourceAsync(byte[] pixels,int width,int height)
        {
            var softwareBitmap = new SoftwareBitmap(
                BitmapPixelFormat.Bgra8,
                width,
                height,
                BitmapAlphaMode.Premultiplied);

            byte[] corrected = new byte[pixels.Length];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                corrected[i] = pixels[i + 2]; // B
                corrected[i + 1] = pixels[i + 1]; // G
                corrected[i + 2] = pixels[i];     // R
                corrected[i + 3] = pixels[i + 3]; // A
            }

            softwareBitmap.CopyFromBuffer(corrected.AsBuffer());

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            return source;
        }

        public async Task<SoftwareBitmapSource> CreateImageSourceAsync(StorageFile file)
        {
            using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

            SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource source = new SoftwareBitmapSource();

            await source.SetBitmapAsync(bitmap);

            return source;
        }

        public Bitmap CreateBitmap(byte[] pixels, int width, int height)
        {
            Bitmap bmp = new Bitmap(
                width,
                height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var data = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);

                Marshal.Copy(
                pixels,
                0,
                data.Scan0,
                pixels.Length);

            bmp.UnlockBits(data);

            return bmp;
        }


        public async Task SaveBitmapAsync(SoftwareBitmap bitmap)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "debug.png");

            using FileStream fileStream = File.Create(path);

            using var randomAccessStream = fileStream.AsRandomAccessStream();

            var encoder = await BitmapEncoder.CreateAsync(
                BitmapEncoder.PngEncoderId,
                randomAccessStream);

            encoder.SetSoftwareBitmap(bitmap);

            await encoder.FlushAsync();

            Debug.WriteLine(path);
        }
    }
}
