using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;

namespace MusicPlayer.Code
{
    public class Music
    {
        public string Number { get; set; }
        public BitmapImage ImgSrc{ get; set; }
        public string SongName { get; set; }
        public string Year { get; set; }
        public string TotalSongTime { get; set; }
        public string SongPath { get; set; }
    }
}
