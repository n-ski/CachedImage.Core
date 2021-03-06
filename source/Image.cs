﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

// Thank you, Jeroen van Langen - http://stackoverflow.com/a/5175424/218882 and Ivan Leonenko - http://stackoverflow.com/a/12638859/218882

namespace CachedImage.Core
{
    /// <summary>
    /// Represents a control that is a wrapper on System.Windows.Controls.Image for enabling filesystem-based caching
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

        public BitmapImage FailedImage
        {
            get => (BitmapImage)GetValue(FailedImageProperty);
            set => SetValue(FailedImageProperty, value);
        }

        public int DecodePixelWidth
        {
            get => (int)GetValue(DecodePixelWidthProperty);
            set => SetValue(DecodePixelWidthProperty, value);
        }

        public int DecodePixelHeight
        {
            get => (int)GetValue(DecodePixelHeightProperty);
            set => SetValue(DecodePixelHeightProperty, value);
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
            get => (BitmapCreateOptions)GetValue(CreateOptionsProperty);
            set => SetValue(CreateOptionsProperty, value);
        }


        
        private static void FailedImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        { }

        private static async void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var url = (string)e.NewValue;
            var cachedImage = (Image)obj;
            
            cachedImage.Source = await LoadImageAsync(url, cachedImage);
            cachedImage.IsLoading = false;
        }
        
        private static async Task<BitmapImage> LoadImageAsync(string url, Image img)
        {
            if (string.IsNullOrEmpty(url))
            {
                return img.FailedImage;
            }

            var bitmapImage = new BitmapImage();
            img.IsLoading = true;

            switch (FileCache.AppCacheMode)
            {
                case FileCache.CacheMode.WinINet:
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = img.CreateOptions;

                    if (img.DecodePixelHeight > 0)
                    {
                        bitmapImage.DecodePixelHeight = img.DecodePixelHeight;
                    }
                    if (img.DecodePixelWidth > 0)
                    {
                        bitmapImage.DecodePixelWidth = img.DecodePixelWidth;
                    }

                    bitmapImage.UriSource = new Uri(url);
                    // Enable IE-like cache policy.
                    bitmapImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                    bitmapImage.EndInit();
                    if (bitmapImage.CanFreeze)
                    {
                        bitmapImage.Freeze();
                    }
                    return bitmapImage;

                case FileCache.CacheMode.Dedicated:
                    try
                    {
                        var memoryStream = await FileCache.HitAsync(url);
                        if (memoryStream == null)
                        {
                            Debug.WriteLine("No hit for URL " + url);
                            img.IsLoading = false;
                            return img.FailedImage;
                        }

                        bitmapImage.BeginInit();
                        bitmapImage.CreateOptions = img.CreateOptions;

                        if (img.DecodePixelHeight > 0)
                        {
                            bitmapImage.DecodePixelHeight = img.DecodePixelHeight;
                        }
                        if (img.DecodePixelWidth > 0)
                        {
                            bitmapImage.DecodePixelWidth = img.DecodePixelWidth;
                        }

                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        if (bitmapImage.CanFreeze)
                        {
                            bitmapImage.Freeze();
                        }
                        return bitmapImage;
                    }
                    catch (Exception)
                    {
                        // ignored, in case the downloaded file is a broken or not an image.
                        Debug.WriteLine($"Image with url {url} failed to load.");
                        return img.FailedImage;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        
        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register(nameof(ImageUrl),
            typeof(string), typeof(Image), new PropertyMetadata("", ImageUrlPropertyChanged));
        
        public static readonly DependencyProperty FailedImageProperty = DependencyProperty.Register(nameof(FailedImage),
            typeof(BitmapImage), typeof(Image), new PropertyMetadata(null, FailedImageUrlPropertyChanged));

        public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.Register(nameof(CreateOptions),
            typeof(BitmapCreateOptions), typeof(Image));


        public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register(nameof(DecodePixelWidth),
            typeof(int), typeof(Image), new PropertyMetadata(-1, FailedImageUrlPropertyChanged));

        public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register(nameof(DecodePixelHeight),
            typeof(int), typeof(Image), new PropertyMetadata(-1, FailedImageUrlPropertyChanged));



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}