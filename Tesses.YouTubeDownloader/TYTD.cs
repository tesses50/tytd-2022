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

namespace Tesses.YouTubeDownloader
{
    public abstract partial class TYTDStorage : TYTDBase, IWritable, IDownloader
    {
        private static readonly HttpClient _default = new HttpClient();
        public abstract Task<Stream> CreateAsync(string path);

        public abstract void CreateDirectory(string path);

        public TYTDStorage(HttpClient clt)
        {
            
            HttpClient=clt;
            YoutubeClient=new YoutubeClient(HttpClient);
            ExtensionContext=null;
            
        }
        public TYTDStorage()
        {
            HttpClient=_default;
             YoutubeClient=new YoutubeClient(HttpClient);
            ExtensionContext=null;
        }
        public async Task WriteAllBytesAsync(string path,byte[] data,CancellationToken token=default(CancellationToken))
        {
            using(var s=await CreateAsync(path))
            {
                await s.WriteAsync(data,0,data.Length,token);
            }
        }

        bool can_download=true;
        public bool CanDownload {get {return can_download;} set {can_download=value;}}
        
        public abstract void MoveDirectory(string src,string dest);
        public abstract void DeleteFile(string file);
        public abstract void DeleteDirectory(string dir,bool recursive=false);
        public IExtensionContext ExtensionContext {get;set;}
        public HttpClient HttpClient {get;set;}
        public YoutubeClient YoutubeClient {get;set;}

        public async Task AddPlaylistAsync(PlaylistId id,Resolution resolution=Resolution.PreMuxed)
        {
            lock(Temporary)
            {
                Temporary.Add( new PlaylistMediaContext(id,resolution));
            }
          

            await Task.FromResult(0);
        }
        public async Task AddChannelAsync(ChannelId id,Resolution resolution=Resolution.PreMuxed)
        {
            lock(Temporary)
            {
                Temporary.Add(new ChannelMediaContext(id,resolution));
            }
            await Task.FromResult(0);
        }
        public async Task AddUserAsync(UserName name,Resolution resolution=Resolution.PreMuxed)
        {
            lock(Temporary)
            {
                Temporary.Add(new ChannelMediaContext(name,resolution));
            }
            await Task.FromResult(0);
        }
        
        public async Task AddVideoAsync(VideoId videoId,Resolution res=Resolution.PreMuxed)
        {
            lock(Temporary)
            {
                Temporary.Add(new VideoMediaContext(videoId,res));
            }
            await Task.FromResult(0);
        }
        public void CreateDirectoryIfNotExist(string dir)
        {
            if(!DirectoryExists(dir))
            {
                CreateDirectory(dir);
            }
        }
        public async Task DownloadThumbnails(VideoId id)
        {
            if(!can_download) return;
            string Id=id.Value;
            string[] res=new string[] {"default.jpg","sddefault.jpg","mqdefault.jpg","hqdefault.jpg","maxresdefault.jpg"};
            CreateDirectoryIfNotExist($"Thumbnails/{Id}");
            foreach(var reso in res)
            {
                if(await Continue($"Thumbnails/{Id}/{reso}"))
                {
                    try{
                        var data=await HttpClient.GetByteArrayAsync($"https://s.ytimg.com/vi/{Id}/{reso}");
                        await WriteAllBytesAsync($"Thumbnails/{Id}/{reso}",data);
                    }catch(Exception ex)
                    {
                        _=ex;
                    }
                }
            }
        }
        public void CreateDirectories()
        {
            CreateDirectoryIfNotExist("Subscriptions");
             CreateDirectoryIfNotExist("VideoOnly");
            CreateDirectoryIfNotExist("AudioOnly");
            CreateDirectoryIfNotExist("Muxed");
            CreateDirectoryIfNotExist("PreMuxed");
            CreateDirectoryIfNotExist("Info");
            CreateDirectoryIfNotExist("Thumbnails");
        }
        public void StartLoop(CancellationToken token = default(CancellationToken))
        {
           CreateDirectories();
            Thread thread0=new Thread(()=>{
                 DownloadLoop(token).Wait();
            });
            thread0.Start();
            Thread thread1=new Thread(()=>{
                 QueueLoop(token).Wait();
            });
           thread1.Start();
        }
        public async Task WriteAllTextAsync(string path,string data)
        {
            using(var dstStrm= await CreateAsync(path))
            {
                using(var sw = new StreamWriter(dstStrm))
                {
                    await sw.WriteAsync(data);
                }
            }
        }
    }
}
