using System;
using System.ComponentModel;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

// Thank you, Jeroen van Langen - http://stackoverflow.com/a/5175424/218882 and Ivan Leonenko - http://stackoverflow.com/a/12638859/218882

namespace CachedImage
{
    /// <summary>
    ///     Represents a control that is a wrapper on System.Windows.Controls.Image for enabling filesystem-based caching
    /// </summary>
    public class Image : System.Windows.Controls.Image, INotifyPropertyChanged
    {
        static Image()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Image),
                new FrameworkPropertyMetadata(typeof(Image)));
        }
        
        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty);
            set => SetValue(ImageUrlProperty, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public BitmapCreateOptions CreateOptions
        {
            get => ((BitmapCreateOptions)(base.GetValue(Image.CreateOptionsProperty)));
            set => base.SetValue(Image.CreateOptionsProperty, value);
        }

        private static async void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var url = (String)e.NewValue;
            var cachedImage = (Image)obj;

            if (String.IsNullOrEmpty(url))
                return;

            cachedImage.Source = await LoadImageAsync(url, ((Image)obj));
        }
        
        private static async Task<BitmapImage> LoadImageAsync(string url, Image img)
        {
            var bitmapImage = new BitmapImage();
            img.IsLoading = true;

            switch (FileCache.AppCacheMode)
            {
                case FileCache.CacheMode.WinINet:
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = img.CreateOptions;
                    bitmapImage.UriSource = new Uri(url);
                    // Enable IE-like cache policy.
                    bitmapImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    img.IsLoading = false;
                    return bitmapImage;

                case FileCache.CacheMode.Dedicated:
                    try
                    {
                        var memoryStream = await FileCache.HitAsync(url);
                        if (memoryStream == null)
                        {
                            Console.WriteLine("No hit for URL " + url);
                            return null;
                        }

                        bitmapImage.BeginInit();
                        bitmapImage.CreateOptions = img.CreateOptions;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        img.IsLoading = false;
                        return bitmapImage;
                    }
                    catch (Exception)
                    {
                        // ignored, in case the downloaded file is a broken or not an image.
                        Console.WriteLine("Ignored Dedicated fail");
                        img.IsLoading = false;
                        return null;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl",
            typeof(string), typeof(Image), new PropertyMetadata("", ImageUrlPropertyChanged));

        public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.Register("CreateOptions",
            typeof(BitmapCreateOptions), typeof(Image));


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}