using System;
using System.Linq;

namespace Git4e
{
    public class HexHashToTextConverter : IHashToTextConverter
    {
        public string ConvertHashToText(byte[] hash) => BitConverter.ToString(hash).Replace("-", "");

        //see: https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array/321404
        public byte[] ConvertTextToHash(string hashText) =>
            Enumerable.Range(0, hashText.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hashText.Substring(x, 2), 16))
            .ToArray();
    }
}
