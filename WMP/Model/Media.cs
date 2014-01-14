using System;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using WMP;

namespace WMP.Model
{

    public enum MediaFilter
    { TITLE, ARTIST, YEARS, ALBUM };
    public enum t_MediaType
    { AUDIO, VIDEO, PICTURE, NONE };
    
    [Serializable]
    public class Media
    {

        protected bool titleFilter(string filter)
        {
            if (filter == null || filter.Length <= 0)
                return (true);
            if (Title != null)
                return ((Title.Contains(filter.ToLower())) ? true : false);
            return ((FileName.Contains(filter.ToLower())) ? true : false);
        }
        virtual protected bool albumFilter(string filter) { return (false); }
        virtual protected bool artistFilter(string filter) { return (false); }
        virtual protected bool yearsFilter(string filter) { return (false); }

        protected Dictionary<WMP.Model.MediaFilter, Delegate> _dict; 

        public Media()
        {
           _dict = new Dictionary<WMP.Model.MediaFilter, Delegate>
            {
                {MediaFilter.TITLE, new Func<string, bool>(titleFilter)},
                {MediaFilter.ARTIST, new Func<string, bool>(artistFilter)},
                {MediaFilter.ALBUM, new Func<string, bool>(albumFilter)},
                {MediaFilter.YEARS, new Func<string, bool>(yearsFilter)}
            };
            MediaType = t_MediaType.NONE;
        }

        static private Func<string, TagLib.File, Media>[] createMedia =
        {
            _createAudioMedia,
            _createVideoMedia,
            _createPictureMedia,
        };

        virtual public bool isDisplayable(string filter, WMP.Model.MediaFilter type)
        {
            return ((bool)(_dict[type].DynamicInvoke(filter)));
        }
     
        public string FileName { get; set; }
        [XmlIgnore]
        public bool isStopped { get; set; }
        [XmlIgnore]
        public int Duration { get; set; }
        [XmlIgnore]
        public bool isPlaying { get; set; }
        [XmlIgnore]
        public bool IsSelected { get; set; }
        [XmlIgnore]
        public string Icon { get; set; }
        [XmlIgnore]
        public string Title { get; set; }
        [XmlIgnore]
        public t_MediaType MediaType { get; set; }

        static private Media _createAudioMedia(string fileName, TagLib.File file)
        {
            AudioMedia media = new AudioMedia();

            media.MediaType = t_MediaType.AUDIO;
            media.Artist = file.Tag.FirstPerformer;
            media.Album = file.Tag.Album;
            media.Year = file.Tag.Year;
            media.Title = file.Tag.Title != "" ? file.Tag.Title : Path.GetFileNameWithoutExtension(fileName);
            foreach (TagLib.ICodec codec in file.Properties.Codecs)
            {
                TagLib.IAudioCodec acodec = codec as TagLib.IAudioCodec;
                if (acodec != null && (acodec.MediaTypes & TagLib.MediaTypes.Audio) != TagLib.MediaTypes.None)
                    media.Bitrate = acodec.AudioBitrate;
                break;
            }
            return media;
        }

        static private Media _createVideoMedia(string fileName, TagLib.File file)
        {
            VideoMedia media = new VideoMedia();

            media.MediaType = t_MediaType.VIDEO;
            media.Year = file.Tag.Year;
            media.Title = file.Tag.Title != "" ? file.Tag.Title : Path.GetFileNameWithoutExtension(fileName);
            foreach (TagLib.ICodec codec in file.Properties.Codecs)
            {
                TagLib.IAudioCodec acodec = codec as TagLib.IAudioCodec;
                TagLib.IVideoCodec vcodec = codec as TagLib.IVideoCodec;
                if (acodec != null && (acodec.MediaTypes & TagLib.MediaTypes.Audio) != TagLib.MediaTypes.None)
                    media.Bitrate = acodec.AudioBitrate;
                if (vcodec != null && (vcodec.MediaTypes & TagLib.MediaTypes.Video) != TagLib.MediaTypes.None)
                {
                    media.Width = vcodec.VideoWidth;
                    media.Height = vcodec.VideoHeight;
                }
                break;
            }
            return media;
        }

        static private Media _createPictureMedia(string fileName, TagLib.File file)
        {
            PictureMedia media = new PictureMedia();
            Image picture = Image.FromFile(fileName);

            media.MediaType = t_MediaType.PICTURE;
            media.Width = picture.Width;
            media.Height = picture.Height;
            media.Title = Path.GetFileNameWithoutExtension(fileName);
            return media;
        }

        static public Media CreateMedia(bool isPlaying, string fileName, bool isStopped, string icon)
        {
            t_MediaType type = ExtensionStatic.GetTypeFromExtension(Path.GetExtension(fileName));
            Media media;
            TagLib.File file;

            Console.WriteLine(fileName);
            try
            {
                file = TagLib.File.Create(fileName);
            }
            catch (Exception)
            {
                return new Media() { isPlaying = isPlaying, FileName = fileName, isStopped = isStopped,
                                     Icon = icon, MediaType = t_MediaType.NONE};
            }

            if (type != t_MediaType.NONE)
                media = createMedia[(int)type](fileName, file);
            else
            {
                media = new Media();
                media.MediaType = t_MediaType.NONE;
                media.Title = Path.GetFileNameWithoutExtension(fileName);
            }
            media.isPlaying = isPlaying;
            media.FileName = fileName;
            media.isStopped = isStopped;
            media.Icon = icon;
            return media;
        }
    }
}
