using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMP
{
    public static class ExtensionStatic
    {
        static readonly string UNKNOWN = "../Icons/exit.png";

        static Dictionary<string, string> _dict = new Dictionary<string, string>
        {
            {".jpg", "../Icons/picture.png"},
            {".png", "../Icons/picture.png"},
            {".bmp", "../Icons/picture.png"},
            {".gif", "../Icons/picture.png"},
            {".mp3", "../Icons/music.png"},
            {".wav", "../Icons/music.png"},
            {".wma", "../Icons/music.png"},
            {".ogg", "../Icons/music.png"},
            {".avi", "../Icons/movie.png"},
            {".mpg", "../Icons/movie.png"},
            {".mov", "../Icons/movie.png"},
            {".asf", "../Icons/movie.png"},
            {".mkv", "../Icons/movie.png"}
        };

        public static string GetIconsFromExtension(string ext)
        {
            string result;

            if (_dict.TryGetValue(ext, out result))
            {
                return result;
            }
            return UNKNOWN;
        }
    }
}
