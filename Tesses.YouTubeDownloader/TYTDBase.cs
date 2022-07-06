using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
namespace Tesses.YouTubeDownloader
{
  
      public abstract class TYTDBase : ITYTDBase
    {
        
        public virtual bool PersonalPlaylistExists(string name)
        {
            return FileExists($"PersonalPlaylist/{name}.json");
        }
        public virtual async IAsyncEnumerable<ListContentItem> GetPersonalPlaylistContentsAsync(string playlist)
        {
            if(!PersonalPlaylistExists(playlist)) yield break;
            var ls=JsonConvert.DeserializeObject<List<ListContentItem>>(await ReadAllTextAsync($"PersonalPlaylist/{playlist}.json"));
            foreach(var item in ls)
            {
                yield return await Task.FromResult(item);
            }
        }
        public virtual async IAsyncEnumerable<string> GetPersonalPlaylistsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("PersonalPlaylist"))
            {
                if(Path.GetExtension(item) == ".json")
                {
                    yield return await Task.FromResult(Path.GetFileNameWithoutExtension(item));
                }
            }
        }
        public virtual async Task<(String Path,bool Delete)> GetRealUrlOrPathAsync(string path)
        {
            string tmpFile = Path.GetTempFileName();
            File.Delete(tmpFile);
            tmpFile=tmpFile + Path.GetExtension(path);
            using(var f = File.Create(tmpFile))
            {
                using(var src=await OpenReadAsync(path))
                {
                    await TYTDManager.CopyStream(src,f);
                }
            }
            
            return (tmpFile,true);
        } 
        
       
             public bool FileExists(string path)
        {
            return FileExistsAsync(path).GetAwaiter().GetResult();
        }

        public virtual async IAsyncEnumerable<string> GetVideoIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Info"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }

        public virtual async Task<SavedVideo> GetVideoInfoAsync(VideoId id)
        {
            
            return JsonConvert.DeserializeObject<SavedVideo>(await ReadAllTextAsync($"Info/{id}.json"));
        }

        public virtual async IAsyncEnumerable<SavedVideo> GetVideosAsync()
        {
            await foreach(var item in GetVideoIdsAsync())
            {
                var item0=await GetVideoInfoAsync(item);
                if(item0 != null)
                {
                    yield return item0;
                }
            }
        }
        public virtual async IAsyncEnumerable<SavedVideoLegacy> GetLegacyVideosAsync()
        {
            await foreach(var item in GetVideoIdsAsync())
            {
                var item0 =await GetLegacyVideoInfoAsync(item);
                 if(item0 != null)
                {
                    yield return item0;
                }
            }
        }
        public virtual async Task<SavedVideoLegacy> GetLegacyVideoInfoAsync(VideoId id)
        {
             return JsonConvert.DeserializeObject<SavedVideoLegacy>(await ReadAllTextAsync($"Info/{id}.json"));
        }
          public virtual async IAsyncEnumerable<SavedPlaylist> GetPlaylistsAsync()
        {
            await foreach(var item in GetPlaylistIdsAsync())
            {
                var item0=await GetPlaylistInfoAsync(item);
                if(item0 != null)
                {
                    yield return item0;
                }
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string path,CancellationToken token=default(CancellationToken))
        {using(var strm = await OpenReadAsync(path))
            {
            byte[] data=new byte[strm.Length];
            
                await strm.ReadAsync(data,0,data.Length,token);
                if(token.IsCancellationRequested)
                {
                    return new byte[0];
                }
                return data;
            }
            
        }
        public virtual async IAsyncEnumerable<string> GetPlaylistIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Playlist"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }
         public virtual async IAsyncEnumerable<string> GetChannelIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Channel"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }
        public virtual async IAsyncEnumerable<VideoId> GetYouTubeExplodeVideoIdsAsync()
        {
            await foreach(var item in GetVideoIdsAsync())
            {
                VideoId? id= VideoId.TryParse(item);
                if(id.HasValue) yield return id.Value;
            }
        }
        public virtual async Task<SavedChannel> GetChannelInfoAsync(ChannelId id)
        {
            return JsonConvert.DeserializeObject<SavedChannel>(await ReadAllTextAsync($"Channel/{id}.json"));
        }
        public virtual async IAsyncEnumerable<SavedChannel> GetChannelsAsync()
        {
            await foreach(var item in GetChannelIdsAsync())
            {
                var item0=await GetChannelInfoAsync(item);
                if(item0 != null)
                {
                    yield return item0;
                }
            }
        }

        public virtual async IAsyncEnumerable<SavedVideo> GetDownloadsAsync()
        {
            await foreach(var item in GetDownloadUrlsAsync())
            {
                yield return await GetDownloadInfoAsync(item);
            }
        }
        public virtual async IAsyncEnumerable<string> GetDownloadUrlsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("FileInfo"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return B64.Base64UrlDecodes(Path.GetFileNameWithoutExtension(item));
                }
            }
        }
        public virtual async Task<BestStreamInfo.BestStreamsSerialized> GetBestStreamInfoAsync(VideoId id)
        {
            return JsonConvert.DeserializeObject<BestStreamInfo.BestStreamsSerialized>(await ReadAllTextAsync($"StreamInfo/{id.Value}.json"));
        }
        public virtual bool BestStreamInfoExists(VideoId id)
        {
            return FileExists($"StreamInfo/{id.Value}.json");
        }
        public virtual async Task<SavedVideo> GetDownloadInfoAsync(string url)
        {
            string enc=$"FileInfo/{B64.Base64UrlEncodes(url)}.json";
            return JsonConvert.DeserializeObject<SavedVideo>(await ReadAllTextAsync(enc));

        }
        public virtual bool DownloadExists(string url)
        {
            string enc=$"FileInfo/{B64.Base64UrlEncodes(url)}.json";
            return FileExists(enc);
        }
        public virtual bool PlaylistInfoExists(PlaylistId id)
        {
            return FileExists($"Playlist/{id}.json");
        }
        public virtual bool VideoInfoExists(VideoId id)
        {
            return FileExists($"Info/{id}.json");
        }
        public virtual bool ChannelInfoExists(ChannelId id)
        {
            return FileExists($"Channel/{id}.json");
        }
        public virtual async Task<SavedPlaylist> GetPlaylistInfoAsync(PlaylistId id)
        {
            return JsonConvert.DeserializeObject<SavedPlaylist>(await ReadAllTextAsync($"Playlist/{id}.json"));
        }
 
        public virtual async Task<string> ReadAllTextAsync(string file)
        {
            using(var s = await OpenReadAsync(file))
            {
                using(var sr = new StreamReader(s))
                {
                   return await sr.ReadToEndAsync();
                }
            }
        }

        public virtual bool DirectoryExists(string path)
        {
            return DirectoryExistsAsync(path).GetAwaiter().GetResult();
        }

        public virtual IEnumerable<string> EnumerateFiles(string path)
        {
            var e = EnumerateFilesAsync(path).GetAsyncEnumerator();
            while(e.MoveNextAsync().GetAwaiter().GetResult())
            {
                yield return e.Current;
            }
        }
        public virtual IEnumerable<string> EnumerateDirectories(string path)
        {
            var e = EnumerateDirectoriesAsync(path).GetAsyncEnumerator();
            while(e.MoveNextAsync().GetAwaiter().GetResult())
            {
                yield return e.Current;
            }
        }
     
        public abstract Task<Stream> OpenReadAsync(string path);

        public abstract Task<bool> FileExistsAsync(string path);

        public abstract Task<bool> DirectoryExistsAsync(string path);

        public abstract IAsyncEnumerable<string> EnumerateFilesAsync(string path);

        public abstract IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path);

    }

    public class ListContentItem
    {
        public ListContentItem()
        {
            Id="";
            Resolution=Resolution.PreMuxed;
        }
        public ListContentItem(VideoId id)
        {
            Id=id.Value;
            Resolution=Resolution.PreMuxed;
        }
        public ListContentItem(string id)
        {
            Id=id;
            Resolution =Resolution.PreMuxed;
        }
        public ListContentItem(string id,Resolution resolution)
        {
            Id=id;
            Resolution=resolution;
        }
        public ListContentItem(VideoId id,Resolution resolution)
        {
            Id=id.Value;
            Resolution=resolution;
        }
        public string Id {get;set;}
        public Resolution Resolution {get;set;}

        public static implicit operator ListContentItem(VideoId id)
        {
            return new ListContentItem(id.Value,  Resolution.PreMuxed);
        }
        public static implicit operator VideoId?(ListContentItem item)
        {
            return VideoId.TryParse(item.Id);
        }

        public static implicit operator ListContentItem((VideoId Id,Resolution Resolution) item)
        {
            return new ListContentItem (item.Id,item.Resolution);
        }
        public static implicit operator (VideoId Id,Resolution Resolution)(ListContentItem item)
        {
            return (VideoId.Parse(item.Id),item.Resolution);
        }
    }

    public static class TYTDManager
    {
        /// <summary>
        /// Add Video, Playlist, Channel Or Username
        /// </summary>
        /// <param name="url">Video, Playlist, Channel Or UserName Url Or Id</param>
        /// <param name="resolution">Video Resolution</param>
         public static async Task<List<(VideoId Id,Resolution Resolution)>> ToPersonalPlaylist(this IAsyncEnumerable<VideoId> list,Resolution res)
        {
            List<(VideoId Id,Resolution Resolution)> items=new List<(VideoId Id, Resolution Resolution)>();
            await foreach(var item in list)
            {
                items.Add((item,res));
            }
            return items;
        }
        public static async Task AddItemAsync(this IDownloader downloader,string url,Resolution resolution=Resolution.PreMuxed)
        {
            
             VideoId? vid = VideoId.TryParse(url);
            PlaylistId? pid = PlaylistId.TryParse(url);
            ChannelId? cid = ChannelId.TryParse(url);
            UserName? user = UserName.TryParse(url);

            if (url.Length == 11)
            {
                if (vid.HasValue)
                {
                    await downloader.AddVideoAsync(vid.Value, resolution); //shall we download video
                }
            }
            else
            {
                if (pid.HasValue)
                {
                    await downloader.AddPlaylistAsync(pid.Value, resolution);
                }
                else if (vid.HasValue)
                {
                    
                    await downloader.AddVideoAsync(vid.Value, resolution);
                }
                else if (cid.HasValue)
                {
                    await downloader.AddChannelAsync(cid.Value, resolution);
                }
                else if (user.HasValue)
                {
                    await downloader.AddUserAsync(user.Value, resolution);
                }
            }

        }

    

        internal static void Print(this IProgress<string> prog,string text)
        {
            if(prog !=null)
            {
                prog.Report(text);
            }
        }
         private static string[] resStr = new string[] {"Muxed","PreMuxed","AudioOnly","VideoOnly"};
       
        /// <summary>
        /// Convert DirectoryName to Resolution
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>Video Resolution</returns>
        public static Resolution DirectoryToResolution(string folder)
        {
           int e= Array.IndexOf(resStr,folder);
            if(e == -1) {return Resolution.NoDownload;}

            return (Resolution)e;
        }
        public static string ResolutionToDirectory(Resolution res)
        {
            return resStr[(int)res];
        }
        public static async Task<bool> CopyVideoToFileAsync(VideoId id,ITYTDBase src,Stream destFile,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
             string infoFile=$"Info/{id.Value}.json";
             string path="";
             if(await src.FileExistsAsync(infoFile)){
              string f=await src.ReadAllTextAsync(infoFile);
                SavedVideo video = JsonConvert.DeserializeObject<SavedVideo>(f);
                path=await BestStreams.GetPathResolution(src,video,res);
             }else{
                 return false;
             }
             if(string.IsNullOrWhiteSpace(path)) return false;

                bool ret=false;
                   if(await src.FileExistsAsync(path))
                {
                    using(var srcFile = await src.OpenReadAsync(path))
                    {
                double len=srcFile.Length;
             
                    
                           ret= await CopyStream(srcFile,destFile,new Progress<long>((e)=>{
                            if(progress !=null)
                            {
                                progress.Report(e/len);
                            }
                            }),token);
                    
                }
                }
                return ret;
        }
        public static async Task CopyVideoToFileAsync(VideoId id,ITYTDBase src,string dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
            bool delete=false;
            using(var f = File.Create(dest))
            {
                delete=!await CopyVideoToFileAsync(id,src,f,res,progress,token);
            }
            if(delete)
            {
                File.Delete(dest);
            }
        }
        
        public static async Task<bool> CopyStream(Stream src,Stream dest,IProgress<long> progress=null,CancellationToken token=default(CancellationToken))
        {
            
            try{
            byte[] array=new byte[1024];
            int read=0;
            long totrd=0;
            do{
                read=await src.ReadAsync(array,0,array.Length,token);
                if(token.IsCancellationRequested)
                {
                    return false;
                }
                totrd += read;
                await dest.WriteAsync(array,0,read,token);
                if(token.IsCancellationRequested)
                {
                    return false;
                }
                if(progress!=null)
                {
                    progress.Report(totrd);
                }
            }while(read> 0);

            
            }catch(Exception ex)
            {
                _=ex;
                return false;
            }

            return true;
        }
        public static async Task CopyThumbnailsAsync(string id,ITYTDBase src,IStorage dest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
            
            if(!src.DirectoryExists("Thumbnails") || !src.DirectoryExists($"Thumbnails/{id}")) return;
            await dest.CopyDirectoryFrom(src,$"Thumbnails/{id}",$"Thumbnails/{id}",progress,token);
        }

        public static async Task CopyDirectoryFrom(this IStorage _dest,ITYTDBase _src,string src,string dest,IProgress<double> progress,CancellationToken token=default(CancellationToken))
        {

            List<string> dirs=new List<string>();

            await foreach(var dir in _src.EnumerateDirectoriesAsync(src))
            {
                dirs.Add(dir);
                _dest.CreateDirectoryIfNotExist($"{dest.TrimEnd('/')}/{dir.TrimStart('/')}");
            }
            List<string> files=new List<string>();
            await foreach(var file in _src.EnumerateFilesAsync(src))
            {
                files.Add(file);
            }
            int total = dirs.Count + files.Count;
            int i=0;
            double segLen = 1.0 / total;
            foreach(var item in dirs)
            {
                 if(token.IsCancellationRequested) return;
                await CopyDirectoryFrom(_dest,_src,$"{src.TrimEnd('/')}/{item.TrimStart('/')}",$"{dest.TrimEnd('/')}/{item.TrimStart('/')}",new Progress<double>(e=>{
                    double percent = e / total;
                    percent += segLen * i;
                    if(percent >= 1) percent=1;
                    if(percent <= 0) percent= 0;
                    progress?.Report(percent);
                }),token);
                i++;
            }
             if(token.IsCancellationRequested) return;
            foreach(var item in files)
            {
                 if(token.IsCancellationRequested) return;
                 await CopyFileFrom(_dest,_src,$"{src.TrimEnd('/')}/{item.TrimStart('/')}",$"{dest.TrimEnd('/')}/{item.TrimStart('/')}",new Progress<double>(e=>{
                    double percent = e / total;
                    percent += segLen * i;
                    if(percent >= 1) percent=1;
                    if(percent <= 0) percent= 0;
                    progress?.Report(percent);
                }),token);
                i++;
            }
        }
        public static async Task CopyFileFrom(this IStorage _dest,ITYTDBase _src,string src,string dest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
            
            using(var srcFile = await _src.OpenReadAsync(src))
                {double len=srcFile.Length;
                    bool deleteFile=false;
                    using(var destFile = await _dest.CreateAsync(dest))
                    {
                       deleteFile=!await CopyStream(srcFile,destFile,new Progress<long>((e)=>{
                            if(progress !=null)
                            {
                                progress.Report(e/len);
                            }
                        }),token);
                    }
                    //dest.DeleteFile(path);
                }
        }

        
        public static async Task CopyVideoAsync(VideoId id,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
            await CopyVideoAsync(id,src,dest,res,progress,token,true);
        }
        public static async Task CopyChannelContentsAsync(ChannelId id,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
            List<(VideoId Id, Resolution Resolution)> items=new List<(VideoId Id, Resolution Resolution)>();
            await foreach(var v in src.GetVideosAsync())
            {
                if(v.AuthorChannelId == id.Value)
                {
                    VideoId? id2=VideoId.TryParse(v.Id);
                    if(id2.HasValue)
                    {
                        items.Add((id2.Value,res));
                    }
                }
            }
            await CopyPersonalPlaylistContentsAsync(items,src,dest,progress,token,copyThumbnails);
        }
        public static async Task CopyChannelAsync(ChannelId id,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=false)
        {
            if(!src.ChannelInfoExists(id)) return;
            var channel = await src.GetChannelInfoAsync(id);
            await dest.WriteChannelInfoAsync(channel);
            if(copyThumbnails)
                await CopyThumbnailsAsync(id,src,dest,progress,default(CancellationToken));

            if(copyContents)
                await CopyChannelContentsAsync(id,src,dest,res,progress,token,copyThumbnails);
        }
        public static async Task CopyEverythingAsync(ITYTDBase src,IStorage dest,Resolution res,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
            await CopyAllPlaylistsAsync(src,dest,res,null,token,copyThumbnails,false);
            if(token.IsCancellationRequested) return;
            await CopyAllChannelsAsync(src,dest,res,null,token,true);
            if(token.IsCancellationRequested) return;
            await CopyAllVideosAsync(src,dest,res,progress,token,copyThumbnails);
        }
        public static async Task CopyEverythingAsync(ITYTDBase src,IStorage dest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
            await CopyEverythingAsync(src,dest,Resolution.Mux,new Progress<double>(e=>{
                double percent=e / 4;
                progress?.Report(percent);
            }));
             await CopyEverythingAsync(src,dest,Resolution.PreMuxed,new Progress<double>(e=>{
                double percent=e / 4;
                percent+=0.25;
                progress?.Report(percent);
            }));
             await CopyEverythingAsync(src,dest,Resolution.AudioOnly,new Progress<double>(e=>{
                double percent=e / 4;
                percent+=0.50;
                progress?.Report(percent);
            }));
             await CopyEverythingAsync(src,dest,Resolution.VideoOnly,new Progress<double>(e=>{
                double percent=e / 4;
                percent+=0.75;
                progress?.Report(percent);
            }));
        }
        public static async Task CopyAllVideosAsync(ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
            await CopyPersonalPlaylistContentsAsync(await src.GetYouTubeExplodeVideoIdsAsync().ToPersonalPlaylist(res),src,dest,progress,token,copyThumbnails);
        }
        public static async Task CopyAllPlaylistsAsync(ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=true)
        {
            List<SavedPlaylist> pl=new List<SavedPlaylist>();
            await foreach(var playlist in src.GetPlaylistsAsync())
            {
                pl.Add(playlist);
            }
             int total=pl.Count;
                int i = 0;
                double segLen = 1.0 / total;
                foreach(var item in pl)
                {
                    if(token.IsCancellationRequested) return;
                    await CopyPlaylistAsync(item.Id,src,dest,res,new Progress<double>(e=>{
                        double percent = e / total;
                    percent += segLen * i;
                    if(percent >= 1) percent=1;
                    if(percent <= 0) percent= 0;
                    progress?.Report(percent);
                    }),token,copyThumbnails,copyContents);
                    i++;
                }
        }
        public static async Task CopyAllChannelsAsync(ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true,bool copyContents=false)
        {
            //copying all channels is equivalent to copying all videos except we copy all channels info also
            await foreach(var item in src.GetChannelsAsync())
            {
                await dest.WriteChannelInfoAsync(item);
            }
            if(copyContents)
            await CopyAllVideosAsync(src,dest,res,progress,token,copyThumbnails);
        }
        public static async Task CopyPersonalPlaylistContentsAsync(List<(VideoId Id,Resolution Resolution)> list,ITYTDBase src,IStorage dest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken), bool copyThumbnails=true)
        {
            int total=list.Count;
                int i = 0;
                double segLen = 1.0 / total;

                foreach(var item in list)
                {
                    if(token.IsCancellationRequested) return;
                    await CopyVideoAsync(item.Id,src,dest,item.Resolution,new Progress<double>(e=>{
                        double percent = e / total;
                    percent += segLen * i;
                    if(percent >= 1) percent=1;
                    if(percent <= 0) percent= 0;
                    progress?.Report(percent);
                    }),token,copyThumbnails);
                    i++;
                }
        }
        public static async Task CopyPlaylistContentsAsync(SavedPlaylist list,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken), bool copyThumbnails=true)
        {
           
            await CopyPersonalPlaylistContentsAsync(list.ToPersonalPlaylist(res),src,dest,progress,token,copyThumbnails);
        }
        public static async Task CopyPlaylistAsync(PlaylistId id,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token = default(CancellationToken),bool copyThumbnails=true,bool copyContents=true)
        {
            //copy playlist and videos
            if(!src.PlaylistInfoExists(id)) return;
            var playlist=await src.GetPlaylistInfoAsync(id);
            await dest.WritePlaylistInfoAsync(playlist);
            if(copyContents)
            await CopyPlaylistContentsAsync(playlist,src,dest,res,progress,token,copyThumbnails);
        }
        public static async Task CopyVideoAsync(VideoId id,ITYTDBase src,IStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken),bool copyThumbnails=true)
        {
                if(copyThumbnails)
                {
                    await CopyThumbnailsAsync(id.Value,src,dest,progress,token);
                }
            string resDir = ResolutionToDirectory(res);
                  
            string infoFile = $"Info/{id}.json";

            string streamInfo = $"StreamInfo/{id}.json";
           string path="";
            if(src.VideoInfoExists(id))
            {
              
                SavedVideo video = await src.GetVideoInfoAsync(id);
                path=await BestStreams.GetPathResolution(src,video,res);
                if(!dest.VideoInfoExists(id))
                {
                    await dest.WriteVideoInfoAsync(video);
                }
            }else{
                return;
            }            
            if(string.IsNullOrWhiteSpace(path)) return;
            if(await src.FileExistsAsync(streamInfo))
            {
                if(!await dest.FileExistsAsync(streamInfo))
                {
                    string f = await src.ReadAllTextAsync(streamInfo);
                    await dest.WriteAllTextAsync(infoFile,f);
                }
            }
            if(token.IsCancellationRequested)
            {
                return;
            }
            if(await src.FileExistsAsync(path))
            {
                if(!await dest.FileExistsAsync(path))
                {
                    return;
                }
                
                dest.CreateDirectory(resDir);
                using(var srcFile = await src.OpenReadAsync(path))
                {
                    double len=srcFile.Length;
                    bool deleteFile=false;
                    using(var destFile = await dest.CreateAsync(path))
                    {
                       deleteFile=!await CopyStream(srcFile,destFile,new Progress<long>((e)=>{
                            if(progress !=null)
                            {
                                progress.Report(e/len);
                            }
                        }),token);
                    }
                    //dest.DeleteFile(path);
                }
            }
        }
    }
    public class Chapter
    {
        public Chapter(TimeSpan ts, string name)
        {
            Offset = ts;
            ChapterName = name;
        }
        public TimeSpan Length {get;set;}
        public TimeSpan Offset { get; set; }
        public string ChapterName { get; set; }

        public override string ToString()
        {
            
            return $"[{Offset}-{Offset.Add(Length)}] {ChapterName}";
        }
    }
    public interface IPersonalPlaylistGet
    {
           IAsyncEnumerable<ListContentItem> GetPersonalPlaylistContentsAsync(string name);
           bool PersonalPlaylistExists(string name);
    }
    public interface IPersonalPlaylistSet : IPersonalPlaylistGet
    {
          Task AddToPersonalPlaylistAsync(string name, IEnumerable<ListContentItem> items);
          
          Task ReplacePersonalPlaylistAsync(string name,IEnumerable<ListContentItem> items);

          Task RemoveItemFromPersonalPlaylistAsync(string name,VideoId id);

          Task SetResolutionForItemInPersonalPlaylistAsync(string name,VideoId id,Resolution resolution);
          

    }
    public interface IWritable : IPersonalPlaylistGet, IPersonalPlaylistSet
    {
             public Task WriteAllTextAsync(string path,string data);
    }       


}