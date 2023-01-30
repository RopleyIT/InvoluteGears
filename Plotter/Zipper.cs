using System.IO;
using System.IO.Compression;

namespace Plotter;

public static class Zipper
{
    /// <summary>
    /// Use the ZIP deflation algorithms to compress a string
    /// into a memory stream. The stream is rewound before being
    /// returned so that the caller can read the stream.
    /// </summary>
    /// <param name="name">The name of the 'file' within the
    /// zipped output stream</param>
    /// <param name="content">The string to be compressed as
    /// the file in the stream</param>
    /// <returns>The memory stream containing the zipped
    /// content. The stream has been rewound to the
    /// beginning ready to be read.</returns>

    public static Stream ZipStringToStream(string name, string content)
    {
        MemoryStream ms = new();
        using ZipArchive archive = new(ms, ZipArchiveMode.Create, true);
        ZipArchiveEntry memberFile = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var entryStream = memberFile.Open();
        using StreamWriter sw = new(entryStream);
        sw.Write(content);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}
