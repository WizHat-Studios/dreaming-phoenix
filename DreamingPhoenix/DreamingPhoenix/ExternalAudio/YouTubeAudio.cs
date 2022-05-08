using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WizHat.DreamingPhoenix.Data;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WizHat.DreamingPhoenix.ExternalAudio
{
    public class YouTubeAudio
    {
        public event EventHandler<DoubleEventArgs> OnDownloadProgress;
        public event EventHandler<StringEventArgs> OnGatheredInformations;

        private static string BasePath
        {
            get
            {
                if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "youtube")))
                    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "youtube"));

                return Path.Combine(AppContext.BaseDirectory, "youtube");
            }
        }
        private static string LocalFFMPEG
        {
            get { return Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"); }
        }

        public static bool FFMPEGExists()
        {
            if (File.Exists(LocalFFMPEG))
                return true;

            if (CommandLinePathResolver.TryGetFullPathForCommand("ffmpeg") != null)
                return true;

            return false;
        }

        public async Task<bool> DownloadAudio(string url, bool showYouTubeErrors = true)
        {
            if (!FFMPEGExists())
                return false;

            IProgress<double> downloadProgess = new Progress<double>(progress => OnDownloadProgress?.Invoke(this, new(progress)));
            YoutubeClient youtube = new();
            Video youtubeVideo;

            try
            {
                
                youtubeVideo = await youtube.Videos.GetAsync(url);
            }
            catch
            {
                return false;
            }

            string downloadedPath = Path.Combine(BasePath, $"{ReplaceInvalidChars(youtubeVideo.Title.Trim())}.mp3");

            OnGatheredInformations?.Invoke(this, new(downloadedPath));

            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(youtubeVideo.Id);
            AudioOnlyStreamInfo audioStreamInfo = (AudioOnlyStreamInfo)streamManifest.GetAudioStreams().First(audioStream => audioStream.GetType() == typeof(AudioOnlyStreamInfo) && audioStream.AudioCodec == "mp4a.40.5");
            IStreamInfo[] streamInfos = new IStreamInfo[] { audioStreamInfo };

            ConversionRequestBuilder conversionRequest = new(downloadedPath);
            conversionRequest.SetPreset(ConversionPreset.Slow);

            if (File.Exists(LocalFFMPEG))
                conversionRequest.SetFFmpegPath(LocalFFMPEG);

            await youtube.Videos.DownloadAsync(streamInfos, conversionRequest.Build(), downloadProgess);

            if (AppModel.Instance.AudioManager.IsWrongSampleRate(downloadedPath))
                AppModel.Instance.AudioManager.ConvertToSampleRate(ref downloadedPath);

            return true;
        }

        private static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
