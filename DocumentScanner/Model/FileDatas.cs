using Docnet.Core.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DocumentScanner.Model
{
    public class FileDatas
    {
        public StorageFile file { get; set; }
        public IDocReader docReader { get; set; }
        public IDocReader docReaderHQ { get; set; }
        public string FileExtension { get; set; }
    }
}
