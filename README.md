[![NuGet version](https://badge.fury.io/nu/CachedImage.Core.png)](https://badge.fury.io/nu/CachedImage.Core)

A WPF control that wraps the Image control to enable file-system based caching, ported to .NET Core 3.1 for use with .NET Core WPF applications.

### Background
If we use the native WPF `Image` control for displaying images over the HTTP protocol (by setting the `Source` to an http url), the image will be downloaded from the server every time the control is loaded. 

In its `Dedicated` mode (see `Cache Mode` below), the `Image` control present in this `CachedImage.Core` library, wraps the native `Image` control to add a local file-system based caching capability. This control creates a local copy of the image on the first time an image is downloaded; to a configurable cache folder (defaults to `<current-user/appdata/roaming>\AppName\Cache`). All the subsequent loads of the control (or the page, window or app that contains the control), will display the image from the local file-system and will not download it from the server.

In its `WinINet` mode, the `Image` control uses the Temporary Internet Files directory that IE uses for the cache.

### Cache Mode
We provide two cache mode: `WinINet` mode and `Dedicated` mode.
* `WinINet`: This is the default mode and it takes advantage of `BitmapImage.UriCachePolicy` property and uses the Temporary Internet Files directory of IE to store cached images. The image control will have the same cache policy of IE.
* `Dedicated`: Another url-based cache implementation. You can set your own cache directory. The cache will never expire unless you delete the cache folder manually.

### Installation
Using Package Manager Console:
`PM> Install-Package CachedImage.Core`

### Usage
1. Install the NuGet package named `CachedImage.Core` on the WPF project 
2. Add a namespace reference to the `CachedImage.Core` assembly on the Window/Usercontrol `xmlns:cachedImage="clr-namespace:CachedImage.Core;assembly=CachedImage.Core"` as in the example `Window` below:
  ```xml
  <Window x:Class="MyWindow1"
          xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
          xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
          xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
          xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
          xmlns:cachedImage="clr-namespace:CachedImage.Core;assembly=CachedImage.Core">
  
  </Window>
  ```
3. Use the control and set or bind the `ImageUrl` attribute:
  ```xml
      <cachedImage:Image ImageUrl="{Binding LargeImageUrl}" />
  ```
4. As it is only a wrapper, all the XAML elements that could be used with the `Image` control are valid here as well:
  ```xml
    <cachedImage:Image ImageUrl="{Binding LargeImageUrl}">
        <Image.ToolTip>This image gets cached to the file-system the first time it is downloaded</Image.ToolTip>
    </cachedImage:Image>
  ```
5. To change cache mode, set `FileCache.AppCacheMode` like this:
  ```csharp
    CachedImage.Core.FileCache.AppCacheMode = CachedImage.Core.FileCache.CacheMode.Dedicated; // The default mode is WinINet
  ```
6. To change the cache folder location of the dedicated cache mode, set the static string property named `AppCacheDirectory` of the `FileCache` class like this:
  ```csharp
    CachedImage.Core.FileCache.AppCacheDirectory = Path.Combine(
		Environment.GetFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
		"MyCustomCacheFolder");
  ```
7. Please note that the dedicated cache mode does not consider `Cache-Control` or `Expires` headers. Unless the cache folder (or specific files in it) gets deleted, the control will not fetch the file again from the server. The application could let the end-user empty the cache folder as done in the [flickr downloadr](https://github.com/flickr-downloadr/flickr-downloadr) application that uses this control.

### License

[MIT License](https://raw.github.com/floydpink/CachedImage/master/LICENSE)
