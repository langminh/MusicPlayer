using MusicPlayer.Code;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Winforms = System.Windows.Forms;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Music> listMusic = null;
        private Music currentSong = null;
        private List<Music> prevSong = null;
        private string path = "";
        private bool isMute = false;
        private double currentVolume = 100;

        private bool isPlay = false;
        private int currentPostion = 0;
        int prev = -1;

        private bool isShuffe = false;
        private bool isRepeat = false;

        public MainWindow()
        {
            InitializeComponent();

            listMusic = new List<Music>();
            prevSong = new List<Music>();

            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "file.txt"))
            {
                string text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "file.txt");
                try
                {
                    string[] temp = text.Split('_');
                    this.path = temp[1];
                    currentVolume = int.Parse(temp[3]);

                    if(currentVolume == 0)
                    {
                        isMute = true;
                        changeVolumeMute.Visibility = Visibility.Visible;
                        changeVolume.Visibility = Visibility.Collapsed;
                        Media.Volume = 0;
                        volume.Value = 0;
                    }
                    else
                    {
                        isMute = false;
                        changeVolumeMute.Visibility = Visibility.Collapsed;
                        changeVolume.Visibility = Visibility.Visible;
                        Media.Volume = currentVolume;
                        volume.Value = currentVolume;
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        listMusic = GetMusics(path);
                        if (listMusic.Count > 0)
                        {
                            listSong.ItemsSource = listMusic;

                            txtNameSongTitle.Text = listMusic[0].SongName;
                            txtDuration.Text = listMusic[0].TotalSongTime;
                            ImageBrush ib = new ImageBrush();
                            ib.ImageSource = listMusic[0].ImgSrc;
                            Img.Fill = ib;
                        }
                        else
                        {
                            string p = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", string.Empty);
                            p = p + @"\Assets\photo.jpg";
                            Bitmap b = (Bitmap)Bitmap.FromFile(p);
                            BitmapImage imgB = b.ToBitmapImage();
                            ImageBrush ib = new ImageBrush();
                            ib.ImageSource = imgB;
                            Img.Fill = ib;
                            volume.Value = 100;

                            txtNameSongTitle.Text = "";
                        }
                    }
                }
                catch { }
            }
            else
            {
                string p = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", string.Empty);
                p = p + @"\Assets\photo.jpg";
                Bitmap b = (Bitmap)Bitmap.FromFile(p);
                BitmapImage imgB = b.ToBitmapImage();
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = imgB;
                Img.Fill = ib;
                volume.Value = 100;

                txtNameSongTitle.Text = "";
            }

            
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return (BitmapImage)i;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Winforms.FolderBrowserDialog folderDialog = new Winforms.FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
            Winforms.DialogResult result = folderDialog.ShowDialog();

            if (result == Winforms.DialogResult.OK)
            {
                string path = folderDialog.SelectedPath;
                this.path = path;
                listMusic = GetMusics(path);
                listSong.ItemsSource = listMusic;
            }
            currentPostion = 1;
        }



        List<Music> GetMusics(string path)
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.mp3"); //Getting Text files
                List<Music> result = new List<Music>();

                int i = 1;
                foreach (FileInfo file in Files)
                {
                    string str = file.FullName;
                    Music music = new Music();
                    TagLib.File tagFile = TagLib.File.Create(str);
                    if (tagFile.Tag.Title == null)
                    {
                        music.SongName = file.Name;
                    }
                    else
                    {
                        music.SongName = tagFile.Tag.Title;
                    }

                    if (tagFile.Tag.Year.ToString().Equals("0"))
                    {
                        music.Year = "        ";
                    }
                    else
                    {
                        music.Year = tagFile.Tag.Year.ToString();
                    }
                    music.SongPath = file.FullName;
                    int hours = tagFile.Properties.Duration.Hours;

                    music.TotalSongTime = ((hours < 10 ? "0" + hours.ToString() : hours.ToString()) + ":") + tagFile.Properties.Duration.Minutes.ToString() + ":" + tagFile.Properties.Duration.Seconds.ToString();
                    string number = "";
                    if (i < 10)
                    {
                        number = "0" + i.ToString();
                    }
                    else
                    {
                        number = i.ToString();
                    }
                    music.Number = number;

                    if (tagFile.Tag.Pictures.Count() > 0)
                    {
                        TagLib.IPicture pic = tagFile.Tag.Pictures[0];
                        MemoryStream ms = new MemoryStream(pic.Data.Data);
                        ms.Seek(0, SeekOrigin.Begin);

                        // ImageSource for System.Windows.Controls.Image
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();

                        music.ImgSrc = bitmap;
                    }
                    else
                    {
                        string p = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", string.Empty);
                        p = p + @"\Assets\photo.jpg";
                        Bitmap b = (Bitmap)Bitmap.FromFile(p);
                        music.ImgSrc = b.ToBitmapImage();
                    }
                    result.Add(music);
                    i++;
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                //Do your stuff
                currentSong = item.Content as Music;
                currentPostion = int.Parse(currentSong.Number);
                btnPlay.Visibility = Visibility.Collapsed;
                btnPlay_Pause.Visibility = Visibility.Visible;
                slider.Value = 0;

                if (prevSong.Count > listMusic.Count)
                {
                    prevSong.RemoveAt(0);
                    if (!prevSong.Contains(currentSong))
                        prevSong.Insert(prevSong.Count, currentSong);
                }
                else
                {
                    if (!prevSong.Contains(currentSong))
                        prevSong.Add(currentSong);
                }

                currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                Play(currentSong);
            }
        }

        void Play(Music currentSong)
        {
            try
            {
                if (currentSong != null)
                {
                    Media.Source = new Uri(currentSong.SongPath);
                    txtNameSongTitle.Text = currentSong.SongName;
                    txtDuration.Text = currentSong.TotalSongTime;
                    ImageBrush imgB = new ImageBrush();
                    imgB.ImageSource = currentSong.ImgSrc;
                    Img.Fill = imgB;

                    System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(timer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    dispatcherTimer.Start();
                    Media.Play();
                    currentPostion = int.Parse(currentSong.Number);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Play and pause music
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (listMusic.Count > 0)
            {
                isPlay = !isPlay;

                if (isPlay)
                {
                    btnPlay.Visibility = Visibility.Visible;
                    btnPlay_Pause.Visibility = Visibility.Collapsed;
                    Media.Pause();
                }
                else
                {
                    btnPlay.Visibility = Visibility.Collapsed;
                    btnPlay_Pause.Visibility = Visibility.Visible;
                    Media.Play();
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            slider.Value = Media.Position.TotalSeconds;
            txtCurrent.Text = (Media.Position.Hours < 10 ? "0" + Media.Position.Hours : Media.Position.Hours.ToString()) + ":"
                + (Media.Position.Minutes < 10 ? "0" + Media.Position.Minutes : Media.Position.Minutes.ToString()) + ":"
                + (Media.Position.Seconds < 10 ? "0" + Media.Position.Seconds : Media.Position.Seconds.ToString());
        }

        /// <summary>
        /// Repeat music
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            isRepeat = !isRepeat;
        }

        /// <summary>
        /// Previuos song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (prevSong.Count > 0)
            {
                if (prev < prevSong.Count - 1)
                {
                    prev++;
                    Play(prevSong[prev]);
                    currentPostion = int.Parse(prevSong[prev].Number);
                }
                else
                {
                    Play(currentSong);
                    currentPostion = int.Parse(currentSong.Number);
                }
            }
            else
            {
                Play(currentSong);
                currentPostion = int.Parse(currentSong.Number);
            }
        }

        /// <summary>
        /// Next song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentPostion <= listMusic.Count)
            {
                if (prevSong.Count > listMusic.Count)
                {
                    prevSong.RemoveAt(0);
                    if (!prevSong.Contains(currentSong))
                        prevSong.Insert(prevSong.Count, currentSong);
                }
                else
                {
                    if(!prevSong.Contains(currentSong))
                        prevSong.Add(currentSong);
                }

                if (isShuffe)
                {
                    Random r = new Random();

                    currentPostion = r.Next(0, listMusic.Count);
                    currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                    Play(currentSong);
                }
                else
                {
                    if (currentPostion < listMusic.Count)
                    {
                        currentPostion++;
                        currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                        Play(currentSong);
                    }
                    else
                    {
                        currentPostion = 0;
                        currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                        Play(currentSong);
                    }
                }
            }
        }

        /// <summary>
        /// Shuffle song play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            isShuffe = !isShuffe;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan ts = TimeSpan.FromSeconds(e.NewValue);
            Media.Position = ts;

            txtCurrent.Text = (Media.Position.Hours < 10 ? "0" + Media.Position.Hours : Media.Position.Hours.ToString()) + ":"
                + (Media.Position.Minutes < 10 ? "0" + Media.Position.Minutes : Media.Position.Minutes.ToString()) + ":"
                + (Media.Position.Seconds < 10 ? "0" + Media.Position.Seconds : Media.Position.Seconds.ToString());
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Media.Volume = volume.Value;
            currentVolume = volume.Value;
            if (volume.Value == 0)
            {
                isMute = true;
                changeVolumeMute.Visibility = Visibility.Visible;
                changeVolume.Visibility = Visibility.Collapsed;
                Media.Volume = 0;
            }
            else
            {
                isMute = false;
                changeVolumeMute.Visibility = Visibility.Collapsed;
                changeVolume.Visibility = Visibility.Visible;
                Media.Volume = currentVolume;
            }
        }

        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (Media.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = TimeSpan.FromMilliseconds(Media.NaturalDuration.TimeSpan.TotalMilliseconds);
                slider.Maximum = ts.TotalSeconds;
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //if (Media.GetMediaState() == MediaState.Stop)
            {
                if (isRepeat)
                {
                    Play(currentSong);
                }
                else
                {
                    if (prevSong.Count > listMusic.Count)
                    {
                        prevSong.RemoveAt(0);
                        if (!prevSong.Contains(currentSong))
                            prevSong.Insert(prevSong.Count, currentSong);
                    }
                    else
                    {
                        if (!prevSong.Contains(currentSong))
                            prevSong.Add(currentSong);
                    }

                    if (currentPostion <= listMusic.Count)
                    {
                        if (isShuffe)
                        {
                            Random r = new Random();

                            currentPostion = r.Next(0, listMusic.Count);
                            currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                            Play(currentSong);
                        }
                        else
                        {
                            if (currentPostion < listMusic.Count)
                            {
                                currentPostion++;
                                currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                                Play(currentSong);
                            }
                            else
                            {
                                currentPostion = 0;
                                currentSong = listMusic.Where(x => x.Number.Equals(currentPostion < 10 ? "0" + currentPostion : currentPostion.ToString())).FirstOrDefault();
                                Play(currentSong);
                            }
                        }
                    }
                }
            }
        }

        private void changeVolumeMute_Click(object sender, RoutedEventArgs e)
        {
            isMute = !isMute;
            if (isMute || volume.Value == 0)
            {
                changeVolumeMute.Visibility = Visibility.Visible;
                changeVolume.Visibility = Visibility.Collapsed;
                Media.Volume = 0;
            }
            else if(volume.Value != 0)
            {
                changeVolumeMute.Visibility = Visibility.Collapsed;
                changeVolume.Visibility = Visibility.Visible;
                Media.Volume = currentVolume;
            }
        }

        private void changeVolume_Click(object sender, RoutedEventArgs e)
        {
            isMute = !isMute;
            if (isMute || volume.Value == 0)
            {
                changeVolumeMute.Visibility = Visibility.Visible;
                changeVolume.Visibility = Visibility.Collapsed;
                Media.Volume = 0;
            }
            else if (volume.Value != 0)
            {
                changeVolumeMute.Visibility = Visibility.Collapsed;
                changeVolume.Visibility = Visibility.Visible;
                Media.Volume = currentVolume;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            Media.Stop();

            string content = "path_ " + path + " _ volume_ " + currentVolume.ToString();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "file.txt"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "file.txt");
            }
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "file.txt", content);
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
