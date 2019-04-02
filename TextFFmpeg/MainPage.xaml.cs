using FFmpegInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace TextFFmpeg
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private FFmpegInteropMSS FFmpegMSS;
        private async void OpenLocalFile(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            filePicker.FileTypeFilter.Add("*");

            // Show file picker so user can select a file
            StorageFile file = await filePicker.PickSingleFileAsync();

            if (file != null)
            {
                mediaElement.Stop();

                // Open StorageFile as IRandomAccessStream to be passed to FFmpegInteropMSS
                IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

                try
                {
                    // Read toggle switches states and use them to setup FFmpeg MSS
                    bool forceDecodeAudio = toggleSwitchAudioDecode.IsOn;
                    bool forceDecodeVideo = toggleSwitchVideoDecode.IsOn;

                    // Instantiate FFmpegInteropMSS using the opened local file stream
#pragma warning disable CS0618 // 类型或成员已过时
                    FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, forceDecodeAudio, forceDecodeVideo);
#pragma warning restore CS0618 // 类型或成员已过时
                    MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                    if (mss != null)
                    {
                        // Pass MediaStreamSource to Media Element
                        mediaElement.SetMediaStreamSource(mss);

                        // Close control panel after file open
                        Splitter.IsPaneOpen = false;
                    }
                    else
                    {
                        DisplayErrorMessage("Cannot open media");
                    }
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage(ex.Message);
                }
            }
        }

        private void URIBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            String uri = textBox.Text;

            // Only respond when the text box is not empty and after Enter key is pressed
            if (e.Key == Windows.System.VirtualKey.Enter && !String.IsNullOrWhiteSpace(uri))
            {
                // Mark event as handled to prevent duplicate event to re-triggered
                e.Handled = true;

                try
                {
                    // Read toggle switches states and use them to setup FFmpeg MSS
                    bool forceDecodeAudio = toggleSwitchAudioDecode.IsOn;
                    bool forceDecodeVideo = toggleSwitchVideoDecode.IsOn;

                    // Set FFmpeg specific options. List of options can be found in https://www.ffmpeg.org/ffmpeg-protocols.html
                    PropertySet options = new PropertySet();

                    // Below are some sample options that you can set to configure RTSP streaming
                    // options.Add("rtsp_flags", "prefer_tcp");
                    // options.Add("stimeout", 100000);

                    // Instantiate FFmpegInteropMSS using the URI
                    mediaElement.Stop();
#pragma warning disable CS0618 // 类型或成员已过时
                    FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromUri(uri, forceDecodeAudio, forceDecodeVideo, options);
#pragma warning restore CS0618 // 类型或成员已过时
                    if (FFmpegMSS != null)
                    {
                        MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                        if (mss != null)
                        {
                            // Pass MediaStreamSource to Media Element
                            mediaElement.SetMediaStreamSource(mss);

                            // Close control panel after opening media
                            Splitter.IsPaneOpen = false;
                        }
                        else
                        {
                            DisplayErrorMessage("Cannot open media");
                        }
                    }
                    else
                    {
                        DisplayErrorMessage("Cannot open media");
                    }
                }
                catch (Exception ex)
                {
                    DisplayErrorMessage(ex.Message);
                }
            }
        }

        private void MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            DisplayErrorMessage(e.ErrorMessage);
        }

        private async void DisplayErrorMessage(string message)
        {
            // Display error message
            var errorDialog = new MessageDialog(message);
            var x = await errorDialog.ShowAsync();
        }
    }
}
