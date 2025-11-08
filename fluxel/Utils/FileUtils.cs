using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace fluxel.Utils;

public static class FileUtils
{
    public static bool IsImage(this MemoryStream ms) => ms.ToArray().IsImage();

    public static bool IsImage(this byte[] input)
    {
        var jpg = new List<string> { "FF", "D8" };
        var png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
        var gif = new List<string> { "47", "49", "46", "38", "39", "61" };

        var imgTypes = new List<List<string>> { jpg, png, gif };

        var bytesIterated = new List<string>();

        for (var i = 0; i < imgTypes.MaxBy(x => x.Count)!.Count; i++)
        {
            var bit = input[i].ToString("X2");
            bytesIterated.Add(bit);

            var isImage = imgTypes.Any(img => !img.Except(bytesIterated).Any());
            if (isImage) return true;
        }

        return false;
    }

    public static bool IsGif(this byte[] input)
    {
        var gif = new List<string> { "47", "49", "46", "38", "39", "61" };

        if (input.Length < gif.Count)
            return false;

        for (var i = 0; i < gif.Count; i++)
        {
            var bit = input[i].ToString("X2");

            if (bit != gif[i])
                return false;
        }

        return true;
    }
}
