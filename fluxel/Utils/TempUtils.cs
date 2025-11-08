using System;
using System.IO;

namespace fluxel.Utils;

public static class TempUtils
{
    public static string CopyToTemp(byte[] data, string extension)
    {
        var folder = Environment.CurrentDirectory + "/temp";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var temp = Path.GetRandomFileName();
        var path = Path.Combine(folder, $"{temp}.{extension}");

        File.WriteAllBytes(path, data);
        return path;
    }
}
