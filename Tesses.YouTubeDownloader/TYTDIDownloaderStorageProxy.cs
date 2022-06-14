using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
using System.IO;
using System.Globalization;

namespace Tesses.YouTubeDownloader
{
    public class TYTDDownloaderStorageProxy : IStorage
    {
        public IDownloader Downloader {get;set;}

        private ITYTDBase _base=null;
        public ITYTDBase Storage {get {return _base;} set{_base=value;
        var v = value as IStorage;
        if(v != null)
         SetStorage(v);}}

        private void SetStorage(IStorage storage)
        {
            if(_storage !=null)
            {
                _storage.Bell -= _EVT_BELL;
                _storage.BeforeSaveInfo -= _EVT_BSI;
                _storage.VideoFinished -= _EVT_VFIN;
                _storage.VideoProgress -= _EVT_VPROG;
                _storage.VideoStarted -= _EVT_VSTAR;
            }
            _storage=storage;
            if(storage != null)
            {
                _storage.Bell += _EVT_BELL;
                _storage.BeforeSaveInfo += _EVT_BSI;
                _storage.VideoFinished += _EVT_VFIN;
                _storage.VideoProgress += _EVT_VPROG;
                _storage.VideoStarted += _EVT_VSTAR;
            }

        }
        private void _EVT_VSTAR(object sender,VideoStartedEventArgs evt)
        {
            VideoStarted?.Invoke(this,evt);
        }
        private void _EVT_VPROG(object sender,VideoProgressEventArgs evt)
        {
            VideoProgress?.Invoke(this,evt);
        }
        private void _EVT_VFIN(object sender,VideoFinishedEventArgs evt)
        {
            VideoFinished?.Invoke(this,evt);
        }
        private void _EVT_BSI(object sender,BeforeSaveInfoEventArgs evt)
        {
            BeforeSaveInfo?.Invoke(this,evt);
        }
        private void _EVT_BELL(object sender,BellEventArgs evt)
        {
            Bell?.Invoke(this,evt);
        }
        IStorage _storage=null;

        public LegacyConverter Legacy  {
            get{
                LegacyConverter conv=null;
                StorageAsStorage((e)=>{
                    conv=e.Legacy;
                });
                return conv;
            }
        }

        public bool CanDownload { get {
             bool dl=false;
                StorageAsStorage((e)=>{
                    dl=e.CanDownload;
                });
                return dl;
        }  set {StorageAsStorage((e)=>{e.CanDownload=value;});} }
        public IExtensionContext ExtensionContext { get {IExtensionContext ctx=null;StorageAsStorage((e)=>{ctx=e.ExtensionContext;});return ctx;} set {StorageAsStorage((e)=>{e.ExtensionContext=value;});} }
        public HttpClient HttpClient 
        { 
            get
            {
                HttpClient clt=null;
                StorageAsStorage((e)=>{
                    clt=e.HttpClient;
                });
                return clt;
            }
            set
            {
                StorageAsStorage((e)=>{
                    e.HttpClient=value;
                });
            }
        }
        public YoutubeClient YoutubeClient
        {
            get
            {
                YoutubeClient clt=null;
                StorageAsStorage((e)=>{
                    clt=e.YoutubeClient;
                });
                return clt;
            }
            set
            {
                StorageAsStorage((e)=>{
                    e.YoutubeClient=value;
                });
            }
        }

        public event EventHandler<BellEventArgs> Bell;
        public event EventHandler<VideoStartedEventArgs> VideoStarted;
        public event EventHandler<BeforeSaveInfoEventArgs> BeforeSaveInfo;
        public event EventHandler<VideoProgressEventArgs> VideoProgress;
        public event EventHandler<VideoFinishedEventArgs> VideoFinished;

       

        public async Task StorageAsStorageAsync(Func<IStorage,Task> callback)
        {
            var store = Storage as IStorage;
            if(store != null && callback != null) await callback(store);
        }
        public void StorageAsStorage(Action<IStorage> callback)
        {
             var store = Storage as IStorage;
            if(store != null && callback != null)  callback(store);
        }
        public async Task StorageAsWritableAsync(Func<IWritable,Task> callback)
        {
             var store = Storage as IWritable;
            if(store != null && callback != null) await callback(store);
        }


        public DownloaderMigration Migration
        {
            get{
                return new DownloaderMigration(this);
            }
        }

        public async Task<Stream> OpenOrCreateAsync(string path)
        {
            Stream strm=Stream.Null;
            await StorageAsStorageAsync(async(e)=>{
                strm=await e.OpenOrCreateAsync(path);
            });
            return strm;
        }

        public void RenameFile(string src, string dest)
        {
             StorageAsStorage((e)=>{
                e.RenameFile(src,dest);
            });
           
        }

        public async Task<Stream> CreateAsync(string path)
        {
            Stream strm=Stream.Null;
            await StorageAsStorageAsync(async(e)=>{
                strm=await e.CreateAsync(path);
            });
            return strm;
        }

        public void CreateDirectory(string path)
        {
            StorageAsStorage((e)=>{
                e.CreateDirectory(path);
            });
        }

        public void MoveDirectory(string src, string dest)
        {
            StorageAsStorage((e)=>{e.MoveDirectory(src,dest);});
        }

        public void DeleteFile(string file)
        {
            StorageAsStorage((e)=>{e.DeleteFile(file);});
        }

        public void DeleteDirectory(string dir, bool recursive = false)
        {
            StorageAsStorage((e)=>{e.DeleteDirectory(dir,recursive);});
        }

        public async Task<Stream> OpenReadAsync(string path)
        {
            return await Storage.OpenReadAsync(path);
        }

        public async Task<bool> FileExistsAsync(string path)
        {
            return await Storage.FileExistsAsync(path);
        }

        public async Task<bool> DirectoryExistsAsync(string path)
        {
            return await Storage.DirectoryExistsAsync(path);
        }

        public async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
            await foreach(var item in Storage.EnumerateFilesAsync(path))
            {
                yield return item;
            }
        }

        public async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
            await foreach(var item in Storage.EnumerateDirectoriesAsync(path))
            {
                yield return item;
            }
        }

        public async Task WriteAllTextAsync(string path, string data)
        {
            await StorageAsWritableAsync(async(e)=>{await e.WriteAllTextAsync(path,data);});
        }

        public async Task<bool> MuxVideosAsync(SavedVideo video, string videoSrc, string audioSrc, string videoDest, IProgress<double> progress = null, CancellationToken token = default)
        {
            bool res=false;
            await StorageAsStorageAsync(async (e)=>{
                res=await e.MuxVideosAsync(video,videoSrc,audioSrc,videoDest,progress,token);
            });
            return res;
        }

        public async Task<bool> Continue(string path)
        {
            bool res=false;
            await StorageAsStorageAsync(async (e)=>{
                res=await e.Continue(path);
            });
            return res;
        }

        public async Task WriteVideoInfoAsync(SavedVideo channel)
        {
            await StorageAsStorageAsync(async (e)=>{
               await e.WriteVideoInfoAsync(channel);
            });
        }

        public async Task WritePlaylistInfoAsync(SavedPlaylist channel)
        {
             await StorageAsStorageAsync(async (e)=>{
                await e.WritePlaylistInfoAsync(channel);
            });
        }

        public async Task WriteChannelInfoAsync(SavedChannel channel)
        {
             await StorageAsStorageAsync(async (e)=>{
                await e.WriteChannelInfoAsync(channel);
            });
        }

        public void CreateDirectoryIfNotExist(string path)
        {
             StorageAsStorage((e)=>{
               e.CreateDirectoryIfNotExist(path);
            });
        }

        public Logger GetLogger()
        {
            Logger logger=null;
             StorageAsStorage((e)=>{
                logger=e.GetLogger();
            });
            return logger;
        }

        public LoggerProperties GetLoggerProperties()
        {
            LoggerProperties properties=null;
             StorageAsStorage((e)=>{
              properties=e.GetLoggerProperties();
            });
            return properties;
        }

        public async Task<bool> DownloadVideoOnlyAsync(SavedVideo video, CancellationToken token, IProgress<double> progress, bool report = true)
        {
            bool res=false;
             await StorageAsStorageAsync(async(e)=>{
             res=  await e.DownloadVideoOnlyAsync(video,token,progress,report);
            });
            return res;
        }

        public async Task MoveLegacyStreams(SavedVideo video, BestStreams streams)
        {
            await StorageAsStorageAsync(async(e)=>{
                await e.MoveLegacyStreams(video,streams);
            });
        }

        public async IAsyncEnumerable<(VideoId Id, Resolution Resolution)> GetPersonalPlaylistContentsAsync(string name)
        {
            IAsyncEnumerable<(VideoId Id,Resolution Resolution)> items=null;
            StorageAsStorage((e)=>{
                items=e.GetPersonalPlaylistContentsAsync(name);

            });
            if(items == null) yield break;
            await foreach(var item in items)
            {
                yield return await Task.FromResult(item);
            }
        }

        public async Task AddVideoAsync(VideoId id, Resolution resolution = Resolution.PreMuxed)
        {
            await Downloader.AddVideoAsync(id,resolution);
        }

        public async Task AddPlaylistAsync(PlaylistId id, Resolution resolution = Resolution.PreMuxed)
        {
            await Downloader.AddPlaylistAsync(id,resolution);
        }

        public async Task AddChannelAsync(ChannelId id, Resolution resolution = Resolution.PreMuxed)
        {
            await Downloader.AddChannelAsync(id,resolution);
        }

        public async Task AddUserAsync(UserName userName, Resolution resolution = Resolution.PreMuxed)
        {
            await Downloader.AddUserAsync(userName,resolution);
        }

        public IReadOnlyList<(SavedVideo Video, Resolution Resolution)> GetQueueList()
        {
            return Downloader.GetQueueList();
        }

        public SavedVideoProgress GetProgress()
        {
            return Downloader.GetProgress();
        }

        public async IAsyncEnumerable<Subscription> GetSubscriptionsAsync()
        {
            IAsyncEnumerable<Subscription> subscriptions=null;
            StorageAsStorage((e)=>{
               subscriptions= e.GetSubscriptionsAsync();
            });
            if(subscriptions == null) yield break;
            await foreach(var item in subscriptions)
            {
                yield return await Task.FromResult(item);
            }
            
        }

        public async Task UnsubscribeAsync(ChannelId id)
        {
            await StorageAsStorageAsync(async(e)=>{
                await e.UnsubscribeAsync(id);
            });
        }

        public async Task SubscribeAsync(ChannelId id, bool downloadChannelInfo = false, ChannelBellInfo bellInfo = ChannelBellInfo.NotifyAndDownload)
        {
            await StorageAsStorageAsync(async(e)=>{
                await e.SubscribeAsync(id,downloadChannelInfo,bellInfo);
            });
        }

        public async Task SubscribeAsync(UserName name, ChannelBellInfo info = ChannelBellInfo.NotifyAndDownload)
        {
            
            await StorageAsStorageAsync(async(e)=>{
                await e.SubscribeAsync(name,info);
            });
        }

        public async Task ResubscribeAsync(ChannelId id, ChannelBellInfo info = ChannelBellInfo.NotifyAndDownload)
        {
            
            await StorageAsStorageAsync(async(e)=>{
                await e.ResubscribeAsync(id,info);
            });
        }

        public async IAsyncEnumerable<string> GetPersonalPlaylistsAsync()
        {
             await foreach(var item in  Storage.GetPersonalPlaylistsAsync())
             {
                 yield return await Task.FromResult(item);
             }
           
        }

        public async Task<(string Path, bool Delete)> GetRealUrlOrPathAsync(string path)
        {
            return await Storage.GetRealUrlOrPathAsync(path);
                     
        }

        public async Task<long> GetLengthAsync(string path)
        {
           return await Storage.GetLengthAsync(path);
            
        }

        public bool FileExists(string path)
        {
            return Storage.FileExists(path);
        }

        public async IAsyncEnumerable<string> GetVideoIdsAsync()
        {
            await foreach(var id in Storage.GetVideoIdsAsync())
            {
                yield return await Task.FromResult(id);
            }
        }

        public async Task<SavedVideo> GetVideoInfoAsync(VideoId id)
        {
            return await Storage.GetVideoInfoAsync(id);
        }

        public async IAsyncEnumerable<SavedVideo> GetVideosAsync()
        {
            await foreach(var vid in Storage.GetVideosAsync())
            {
                yield return await Task.FromResult(vid);
            }
        }

        public async IAsyncEnumerable<SavedVideoLegacy> GetLegacyVideosAsync()
        {
            await foreach(var item in Storage.GetLegacyVideosAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public async Task<SavedVideoLegacy> GetLegacyVideoInfoAsync(VideoId id)
        {
            return await Storage.GetLegacyVideoInfoAsync(id);
        }

        public async IAsyncEnumerable<SavedPlaylist> GetPlaylistsAsync()
        {
            await foreach(var item in Storage.GetPlaylistsAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken token = default)
        {
            return await Storage.ReadAllBytesAsync(path,token);
        }

        public async IAsyncEnumerable<string> GetPlaylistIdsAsync()
        {
            await foreach(var item in Storage.GetPlaylistIdsAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public async IAsyncEnumerable<string> GetChannelIdsAsync()
        {
            await foreach(var item in Storage.GetChannelIdsAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public async IAsyncEnumerable<VideoId> GetYouTubeExplodeVideoIdsAsync()
        {
            await foreach(var item in Storage.GetYouTubeExplodeVideoIdsAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public async Task<SavedChannel> GetChannelInfoAsync(ChannelId id)
        {
            return await Storage.GetChannelInfoAsync(id);
        }

        public async IAsyncEnumerable<SavedChannel> GetChannelsAsync()
        {
            await foreach(var item in Storage.GetChannelsAsync())
            {
                yield return await Task.FromResult(item);
            }
        }

        public bool PlaylistInfoExists(PlaylistId id)
        {
            return Storage.PlaylistInfoExists(id);
        }

        public bool VideoInfoExists(VideoId id)
        {
           return Storage.VideoInfoExists(id);
        }

        public bool ChannelInfoExists(ChannelId id)
        {
            return Storage.ChannelInfoExists(id);
        }

        public async Task<SavedPlaylist> GetPlaylistInfoAsync(PlaylistId id)
        {
            return await Storage.GetPlaylistInfoAsync(id);
        }

        public Task<string> ReadAllTextAsync(string file)
        {
            return Storage.ReadAllTextAsync(file);
        }

        public bool DirectoryExists(string path)
        {
            return Storage.DirectoryExists(path);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Storage.EnumerateFiles(path);
        }

        public IEnumerable<string> EnumerateDirectories(string path)
        {
            return Storage.EnumerateDirectories(path);
        }

        public async Task<Stream> OpenReadAsyncWithLength(string path)
        {
           return await Storage.OpenReadAsyncWithLength(path);
        }

        public IReadOnlyList<Subscription> GetLoadedSubscriptions()
        {
            IReadOnlyList<Subscription> subs=new List<Subscription>();
            StorageAsStorage((e)=>{
                subs=e.GetLoadedSubscriptions();
            });
            return subs;
        }

        public void Unsubscribe(ChannelId id)
        {
            StorageAsStorage((e)=>{
                e.Unsubscribe(id);
            });
        }
    }

    public class DownloaderMigration
    {
        private TYTDDownloaderStorageProxy proxy;

        public DownloaderMigration(TYTDDownloaderStorageProxy proxy)
        {
            this.proxy = proxy;
        }

          public async Task DownloaderAsBaseAsync(Func<ITYTDBase,Task> callback)
        {
            var dl = proxy.Downloader as ITYTDBase;
            if(dl != null && callback !=null) await callback(dl);
        }
         public void DownloaderAsBase(Action<ITYTDBase> callback)
        {
            var dl = proxy.Downloader as ITYTDBase;
            if(dl != null && callback !=null) callback(dl);
        }
        public async Task CopyVideoAsync(VideoId id,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
            await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyVideoAsync(id,e,f,res,progress,token,copyThumbnails);
                });
            });
        }
        public async Task CopyAllVideosAsync(Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
             await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyAllVideosAsync(e,f,res,progress,token,copyThumbnails);
                });
            });
        
        }
        public async Task CopyPlaylistAsync(PlaylistId id,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=true)
        {
             await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyPlaylistAsync(id,e,f,res,progress,token,copyThumbnails,copyContents);
                });
            });
        }
        public async Task CopyChannelAsync(ChannelId id,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=false)
        {
             await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyChannelAsync(id,e,f,res,progress,token,copyThumbnails,copyContents);
                });
            });
        }
        public async Task CopyAllPlaylistsAsync(Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=true)
        {
             await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyAllPlaylistsAsync(e,f,res,progress,token,copyThumbnails,copyContents);
                });
            });
        }
        public async Task CopyAllChannelsAsync(Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=false)
        {
             await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyAllChannelsAsync(e,f,res,progress,token,copyThumbnails,copyContents);
                });
            });
        }
        public async Task CopyEverythingAsync(Resolution res,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
              await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyEverythingAsync(e,f,res,progress,token,copyThumbnails);
                });
            });
        }
          public async Task CopyEverythingAsync(IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
              await DownloaderAsBaseAsync(async(e)=>{
                await proxy.StorageAsStorageAsync(async(f)=>{
                await TYTDManager.CopyEverythingAsync(e,f,progress,token,copyThumbnails);
                });
            });
        }
    }
}