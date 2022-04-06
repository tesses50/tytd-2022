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
    internal class TYTDBaseFileReader : Stream
    {
        //TYTDBase baseCtl;
        Stream baseStrm;
        long len;
        private TYTDBaseFileReader(long leng)
        {
            len=leng;
        }
        public static async Task<Stream> GetStream(TYTDBase baseCtl,string path)
        {
            var basect=new TYTDBaseFileReader(await baseCtl.GetLengthAsync(path));

            basect.baseStrm = await baseCtl.OpenReadAsync(path);
            return basect;
        }

        public override bool CanRead => baseStrm.CanRead;

        public override bool CanSeek => baseStrm.CanSeek;

        public override bool CanWrite => false;

        public override long Length => len;

        public override long Position { get => baseStrm.Position; set => baseStrm.Position=value; }

        public override void Flush()
        {
            baseStrm.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return baseStrm.Read(buffer,offset,count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStrm.Seek(offset,origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await baseStrm.ReadAsync(buffer,offset,count,cancellationToken);
        }

        public override void Close()
        {
            baseStrm.Close();
        }
    }
      public abstract class TYTDBase
    {
        

        public async IAsyncEnumerable<(VideoId Id,Resolution Resolution)> GetPersonalPlaylistContentsAsync(string playlist)
        {
            var ls=JsonConvert.DeserializeObject<List<(string Id,Resolution Resolution)>>(await ReadAllTextAsync($"PersonalPlaylist/{playlist}.json"));
            foreach(var item in ls)
            {
                yield return await Task.FromResult(item);
            }
        }
        public async IAsyncEnumerable<string> GetPersonalPlaylistsAsync()
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
        
       
        public virtual async Task<long> GetLengthAsync(string path)
        {
            if(!await FileExistsAsync(path)) return 0;
            using(var f = await OpenReadAsync(path))
            {
                return f.Length;
            }
        }
             public bool FileExists(string path)
        {
            return FileExistsAsync(path).GetAwaiter().GetResult();
        }

        public async IAsyncEnumerable<string> GetVideoIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Info"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }

        public async Task<SavedVideo> GetVideoInfoAsync(VideoId id)
        {
            
            return JsonConvert.DeserializeObject<SavedVideo>(await ReadAllTextAsync($"Info/{id}.json"));
        }

        public async IAsyncEnumerable<SavedVideo> GetVideosAsync()
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
        public async IAsyncEnumerable<SavedVideoLegacy> GetLegacyVideosAsync()
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
        public async Task<SavedVideoLegacy> GetLegacyVideoInfoAsync(VideoId id)
        {
             return JsonConvert.DeserializeObject<SavedVideoLegacy>(await ReadAllTextAsync($"Info/{id}.json"));
        }
          public async IAsyncEnumerable<SavedPlaylist> GetPlaylistsAsync()
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
        {
            byte[] data=new byte[await GetLengthAsync(path)];
            using(var strm = await OpenReadAsync(path))
            {
                await strm.ReadAsync(data,0,data.Length,token);
                if(token.IsCancellationRequested)
                {
                    return new byte[0];
                }
            }
            return data;
        }
        public async IAsyncEnumerable<string> GetPlaylistIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Playlist"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }
         public async IAsyncEnumerable<string> GetChannelIdsAsync()
        {
            await foreach(var item in EnumerateFilesAsync("Channel"))
            {
                if(Path.GetExtension(item).Equals(".json",StringComparison.Ordinal))
                {
                    yield return Path.GetFileNameWithoutExtension(item);
                }
            }
        }
        public async Task<SavedChannel> GetChannelInfoAsync(ChannelId id)
        {
            return JsonConvert.DeserializeObject<SavedChannel>(await ReadAllTextAsync($"Channel/{id}.json"));
        }
        public async IAsyncEnumerable<SavedChannel> GetChannelsAsync()
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

        public async Task<SavedPlaylist> GetPlaylistInfoAsync(PlaylistId id)
        {
            return JsonConvert.DeserializeObject<SavedPlaylist>(await ReadAllTextAsync($"Playlist/{id}.json"));
        }

        
        public async Task<string> ReadAllTextAsync(string file)
        {
            using(var s = await OpenReadAsync(file))
            {
                using(var sr = new StreamReader(s))
                {
                   return await sr.ReadToEndAsync();
                }
            }
        }

        public bool DirectoryExists(string path)
        {
            return DirectoryExistsAsync(path).GetAwaiter().GetResult();
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            var e = EnumerateFilesAsync(path).GetAsyncEnumerator();
            while(e.MoveNextAsync().GetAwaiter().GetResult())
            {
                yield return e.Current;
            }
        }
        public IEnumerable<string> EnumerateDirectories(string path)
        {
            var e = EnumerateDirectoriesAsync(path).GetAsyncEnumerator();
            while(e.MoveNextAsync().GetAwaiter().GetResult())
            {
                yield return e.Current;
            }
        }
        public async Task<Stream> OpenReadAsyncWithLength(string path)
        {
            return await TYTDBaseFileReader.GetStream(this,path);
        }
        public abstract Task<Stream> OpenReadAsync(string path);

        public abstract Task<bool> FileExistsAsync(string path);

        public abstract Task<bool> DirectoryExistsAsync(string path);

        public abstract IAsyncEnumerable<string> EnumerateFilesAsync(string path);

        public abstract IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path);

    }
      public static class TYTDManager
    {
        /// <summary>
        /// Add Video, Playlist, Channel Or Username
        /// </summary>
        /// <param name="url">Video, Playlist, Channel Or UserName Url Or Id</param>
        /// <param name="resolution">Video Resolution</param>
        
        public static async Task AddItemAsync(this IDownloader downloader,string url,Resolution resolution=Resolution.PreMuxed)
        {
            Console.WriteLine(url);
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
        /// <summary>
        /// Replace Personal Playlist
        /// </summary>
        /// <param name="name">Name of playlist</param>
        /// <param name="items">Videos to set in playlist</param>
        /// <returns></returns>
        public static async Task ReplacePersonalPlaylistAsync(this IWritable writable,string name,IEnumerable<(VideoId Id,Resolution Resolution)> items)
        {
             List<(string Id,Resolution Resolution)> items0=new List<(string Id, Resolution Resolution)>();
          
             items0.AddRange(items.Select<(VideoId Id,Resolution Resolution),(string Id,Resolution Resolution)>((e)=>{
                return (e.Id.Value,e.Resolution);
            }) );
            await writable.WriteAllTextAsync($"PersonalPlaylist/{name}.json",JsonConvert.SerializeObject(items0));
            
        }
        /// <summary>
        /// Append to PersonalPlaylist
        /// </summary>
        /// <param name="name">Name of playlist</param>
        /// <param name="items">Videos to add in playlist</param>
        public static async Task AddToPersonalPlaylistAsync(this IWritable writable, string name, IEnumerable<(VideoId Id, Resolution Resolution)> items)
        {
            
            List<(string Id,Resolution Resolution)> items0=new List<(string Id, Resolution Resolution)>();
            await foreach(var item in writable.GetPersonalPlaylistContentsAsync(name))
            {
                items0.Add(item);
            }
            items0.AddRange(items.Select<(VideoId Id,Resolution Resolution),(string Id,Resolution Resolution)>((e)=>{
                return (e.Id.Value,e.Resolution);
            }) );
            await writable.WriteAllTextAsync($"PersonalPlaylist/{name}.json",JsonConvert.SerializeObject(items0));
            
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
        public static async Task<bool> CopyVideoToFileAsync(VideoId id,TYTDBase src,Stream destFile,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
              string resDir = ResolutionToDirectory(res);
             string path=$"{resDir}/{id.Value}.mp4";
                bool ret=false;
                double len=await src.GetLengthAsync(path);
                if(await src.FileExistsAsync(path))
                {
                    using(var srcFile = await src.OpenReadAsync(path))
                    {
                    
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
        public static async Task CopyVideoToFileAsync(VideoId id,TYTDBase src,string dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
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

        public static async Task CopyVideoAsync(VideoId id,TYTDBase src,TYTDStorage dest,Resolution res=Resolution.PreMuxed,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
               string resDir = ResolutionToDirectory(res);
        
             string path=$"{resDir}/{id.Value}.mp4";
            string infoFile = $"Info/{id.Value}.json";
           
            
            if(await src.FileExistsAsync(infoFile))
            {
                if(!await dest.FileExistsAsync(infoFile))
                {
                    string f=await src.ReadAllTextAsync(infoFile);
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
                double len=await src.GetLengthAsync(path);
                dest.CreateDirectory(resDir);
                using(var srcFile = await src.OpenReadAsync(path))
                {
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
    public interface IWritable
    {
        public IAsyncEnumerable<(VideoId Id,Resolution Resolution)> GetPersonalPlaylistContentsAsync(string name);
        public Task WriteAllTextAsync(string path,string data);
    }
}