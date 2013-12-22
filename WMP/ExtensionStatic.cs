using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMP.Model;

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
            {".mkv", "../Icons/movie.png"},
            {".wmv", "../Icons/movie.png"}
        };

        static Dictionary<string, t_MediaType> _types = new Dictionary<string, t_MediaType>
        {
            {".jpg", t_MediaType.PICTURE},
            {".png", t_MediaType.PICTURE},
            {".bmp", t_MediaType.PICTURE},
            {".gif", t_MediaType.PICTURE},
            {".mp3", t_MediaType.AUDIO},
            {".wav", t_MediaType.AUDIO},
            {".wma", t_MediaType.AUDIO},
            {".ogg", t_MediaType.AUDIO},
            {".avi", t_MediaType.VIDEO},
            {".mpg", t_MediaType.VIDEO},
            {".mov", t_MediaType.VIDEO},
            {".asf", t_MediaType.VIDEO},
            {".mkv", t_MediaType.VIDEO},
            {".mmv", t_MediaType.VIDEO}
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

        public static t_MediaType GetTypeFromExtension(string ext)
        {
            t_MediaType result;

            if (_types.TryGetValue(ext, out result))
            {
                return result;
            }
            return t_MediaType.NONE;
        }

    }
}
