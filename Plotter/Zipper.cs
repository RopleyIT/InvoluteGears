using System.IO;
using System.IO.Compression;

namespace Plotter;

public static class Zipper
{
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
