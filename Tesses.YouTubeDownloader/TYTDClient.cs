using Newtonsoft.Json;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using System.Threading;
using YoutubeExplode.Exceptions;
using System.Collections.Generic;
using System.IO;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using System.Net.Http;

namespace Tesses.YouTubeDownloader
{
    public class TYTDClient : TYTDBase,IDownloader
    {
        public TYTDClient(string url)
        {
            client=new HttpClient();
            client.BaseAddress=new Uri(url);
        }
       
        public TYTDClient(HttpClient clt,string url)
        {
            client = clt;
            client.BaseAddress=new Uri(url);
        }
        public TYTDClient(HttpClient clt, Uri uri)
        {
            client=clt;
            client.BaseAddress=uri;
        }
        public TYTDClient(Uri uri)
        {
            client=new HttpClient();
            client.BaseAddress=uri;
        }
        HttpClient client;
        public async Task AddChannelAsync(ChannelId id, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetAsync($"/api/v2/AddChannel?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddPlaylistAsync(PlaylistId id, Resolution resolution = Resolution.PreMuxed)
        {
           try{
            await client.GetAsync($"/api/v2/AddPlaylist?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddUserAsync(UserName userName, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetAsync($"/api/v2/AddUser?v={userName.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddVideoAsync(VideoId id, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetAsync($"/api/v2/AddVideo?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
           try{
            string v=await client.GetStringAsync($"/api/Storage/DirectoryExists/{path}");
            return v=="true";
            }catch(Exception ex)
            {
                _=ex;
            }
            return false;
        }

        public async override IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
            List<string> items=null;
             try{
            string v=await client.GetStringAsync($"/api/Storage/GetDirectory/{path}");
            items=JsonConvert.DeserializeObject<List<string>>(v);
            }catch(Exception ex)
            {
                _=ex;
            }

            if(items==null)
            {
                yield break;
            }else{
                foreach(var item in items)
                {
                    yield return await Task.FromResult(item);
                }
            }
        }

        public async override IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
            List<string> items=null;
             try{
            string v=await client.GetStringAsync($"/api/Storage/GetFiles/{path}");
            items=JsonConvert.DeserializeObject<List<string>>(v);
            }catch(Exception ex)
            {
                _=ex;
            }

            if(items==null)
            {
                yield break;
            }else{
                foreach(var item in items)
                {
                    yield return await Task.FromResult(item);
                }
            }
        }

        public async override Task<bool> FileExistsAsync(string path)
        {
           try{
            string v=await client.GetStringAsync($"/api/Storage/FileExists/{path}");
            return v=="true";
            }catch(Exception ex)
            {
                _=ex;
            }
            return false;
        }
        private async Task<IReadOnlyList<(SavedVideo Video, Resolution Resolution)>> GetQueueListAsync()
        {
               
             try{
            string v=await client.GetStringAsync("/api/v2/QueueList");
            return JsonConvert.DeserializeObject<List<(SavedVideo Video,Resolution res)>>(v);
            }catch(Exception ex)
            {
                _=ex;
            }

           return new List<(SavedVideo video,Resolution resolution)>();
        }
        private async Task<SavedVideoProgress> GetProgressAsync()
        {
               
             try{
            string v=await client.GetStringAsync("/api/v2/Progress");
            return JsonConvert.DeserializeObject<SavedVideoProgress>(v);
            }catch(Exception ex)
            {
                _=ex;
            }

           return null;
        }
        public SavedVideoProgress GetProgress()
        {
            return GetProgressAsync().GetAwaiter().GetResult();
        }
        
        public IReadOnlyList<(SavedVideo Video, Resolution Resolution)> GetQueueList()
        {
            return GetQueueListAsync().GetAwaiter().GetResult();
        }

        public override async Task<long> GetLengthAsync(string path)
        {
            try{
                var item=await client.GetAsync($"/api/Storage/File/{path}");
                return item.Content.Headers.ContentLength.GetValueOrDefault();
            }catch(Exception ex)
            {
                _=ex;
            }

            return 0;
        }
        
        public async override Task<Stream> OpenReadAsync(string path)
        {
               
             try{
            Stream v=await client.GetStreamAsync($"/api/Storage/File/{path}");
            return v;
            }catch(Exception ex)
            {
                _=ex;
            }

           return Stream.Null;
        }

     
    }
}