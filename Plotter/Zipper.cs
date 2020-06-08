using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Plotter
{
    public static class Zipper
    {
        public static Stream ZipStringToStream(string name, string content)
        {
            MemoryStream ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                var memberFile = archive.CreateEntry(name, CompressionLevel.Optimal);
                using (var entryStream = memberFile.Open())
                using (var sw = new StreamWriter(entryStream))
                    sw.Write(content);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
