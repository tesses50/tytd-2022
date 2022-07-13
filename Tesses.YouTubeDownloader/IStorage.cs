using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
using Newtonsoft.Json;

namespace Tesses.YouTubeDownloader
{
    public interface IStorage : IWritable, IDownloader, ITYTDBase
    { 
        void WaitTillMediaContentQueueEmpty();
        
        Task WriteBestStreamInfoAsync(VideoId id,BestStreamInfo.BestStreamsSerialized serialized);
        Task<bool> MuxVideosAsync(SavedVideo video,string videoSrc,string audioSrc,string videoDest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken));
        Task<bool> Continue(string path);
        Task WriteVideoInfoAsync(SavedVideo channel);
        Task WritePlaylistInfoAsync(SavedPlaylist channel);
        Task WriteChannelInfoAsync(SavedChannel channel);
        void CreateDirectoryIfNotExist(string path);
       LegacyConverter Legacy {get;}
       Logger GetLogger();
       LoggerProperties GetLoggerProperties();
      IReadOnlyList<Subscription> GetLoadedSubscriptions();
          event EventHandler<BellEventArgs> Bell;
        event EventHandler<VideoStartedEventArgs> VideoStarted;

        event EventHandler<BeforeSaveInfoEventArgs> BeforeSaveInfo;

        event EventHandler<VideoProgressEventArgs> VideoProgress;

        public event EventHandler<VideoFinishedEventArgs> VideoFinished;
        bool CanDownload {get;set;}
         IExtensionContext ExtensionContext {get;set;}
        HttpClient HttpClient {get;set;}
        YoutubeClient YoutubeClient {get;set;}
        Task<Stream> OpenOrCreateAsync(string path);

        void RenameFile(string src, string dest);

        Task<Stream> CreateAsync(string path);
        void Unsubscribe(ChannelId id);
        void CreateDirectory(string path);

        void MoveDirectory(string src, string dest);

        void DeleteFile(string file);

        void DeleteDirectory(string dir, bool recursive = false);
        Task<bool> DownloadVideoOnlyAsync(SavedVideo video,CancellationToken token,IProgress<double> progress,bool report=true);
        Task MoveLegacyStreams(SavedVideo video,BestStreams streams);
         
        void StartLoop(CancellationToken token=default(CancellationToken));
        event EventHandler<TYTDErrorEventArgs> Error;
    }
}