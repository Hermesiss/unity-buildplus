using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Trismegistus.BuildPlus {
    public static class Extensions
    {
        public static string ReplaceUnsupportedSymbols(this string s) {
            var symbols = Path.GetInvalidFileNameChars();

            foreach (var symbol in symbols) 
                s = s.Replace(symbol, '_');

            return s;
        }

        public static string ReplaceMultiple(this string s, char symbol) {
            var regex = new Regex($"[{symbol}]" + "{2,}", RegexOptions.None);     
            s = regex.Replace(s, symbol.ToString());
            return s;
        }
    }
}
