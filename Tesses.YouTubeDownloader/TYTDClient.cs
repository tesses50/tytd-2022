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
using System.Net;
using Espresso3389.HttpStream;

namespace Tesses.YouTubeDownloader
{
    public class TYTDClient : TYTDBase,IDownloader
    {
        string url;
        public TYTDClient(string url)
        {
            client=new HttpClient();
            this.url = url.TrimEnd('/') + '/';
        }
       
        public TYTDClient(HttpClient clt,string url)
        {
            client = clt;
            this.url = url.TrimEnd('/') + '/';
        }
        public TYTDClient(HttpClient clt, Uri uri)
        {
            client=clt;
           this.url = url.ToString().TrimEnd('/') + '/';
        }
        public TYTDClient(Uri uri)
        {
            client=new HttpClient();
            this.url = url.ToString().TrimEnd('/') + '/';
        }
        HttpClient client;
        public async Task AddChannelAsync(ChannelId id, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetStringAsync($"{url}api/v2/AddChannel?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddPlaylistAsync(PlaylistId id, Resolution resolution = Resolution.PreMuxed)
        {
           try{
            await client.GetStringAsync($"{url}api/v2/AddPlaylist?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddUserAsync(UserName userName, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetStringAsync($"{url}api/v2/AddUser?v={userName.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task AddVideoAsync(VideoId id, Resolution resolution = Resolution.PreMuxed)
        {
            try{
            await client.GetStringAsync($"{url}api/v2/AddVideo?v={id.Value}&res={resolution.ToString()}");
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
           try{
            string v=await client.GetStringAsync($"{url}api/Storage/DirectoryExists/{path}");
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
            string v=await client.GetStringAsync($"{url}api/Storage/GetDirectory/{path}");
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
        public async IAsyncEnumerable<Subscription> GetSubscriptionsAsync()
        {
            string v="[]";
            try{
                   v=await client.GetStringAsync("{url}api/v2/subscriptions");
               }catch(Exception ex)
            {
                _=ex;
            }    
            foreach(var item in JsonConvert.DeserializeObject<List<Subscription>>(v))
            {
                      yield return await Task.FromResult(item);
            }
            
           
        }
        public async Task UnsubscribeAsync(ChannelId id)
        {
            try{
                  string v=await client.GetStringAsync($"{url}api/v2/unsubscribe?id={id.Value}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
        public async Task SubscribeAsync(ChannelId id,bool downloadChannelInfo=false,ChannelBellInfo bellInfo = ChannelBellInfo.NotifyAndDownload)
        {
              try{
                  string dlcid=downloadChannelInfo ? "true" : "false";
                  string v=await client.GetStringAsync($"{url}api/v2/subscribe?id={id.Value}&conf={bellInfo.ToString()}&getinfo={dlcid}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
        public async Task SubscribeAsync(UserName name,ChannelBellInfo info=ChannelBellInfo.NotifyAndDownload)
        {
             try{
                
                  
                  string v=await client.GetStringAsync($"{url}api/v2/subscribe?id={ WebUtility.UrlEncode(name.Value)}&conf={info.ToString()}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
        public async Task ResubscribeAsync(ChannelId id,ChannelBellInfo info=ChannelBellInfo.NotifyAndDownload)
        {
             try{
                
                  
                  string v=await client.GetStringAsync($"{url}api/v2/resubscribe?id={id.Value}&conf={info.ToString()}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
        public async override IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
            List<string> items=null;
             try{
            string v=await client.GetStringAsync($"{url}api/Storage/GetFiles/{path}");
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
            string v=await client.GetStringAsync($"{url}api/Storage/FileExists/{path}");
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
            string v=await client.GetStringAsync($"{url}api/v2/QueueList");
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
            string v=await client.GetStringAsync($"{url}api/v2/Progress");
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

        public async override Task<Stream> OpenReadAsync(string path)
        {
            
             try{
        
             HttpStream v=new HttpStream(new Uri($"{url}api/Storage/File/{path}"),new MemoryStream(),true,32 * 1024,null,client);
             
            return await Task.FromResult(v);
            }catch(Exception ex)
            {
                _=ex;
            }

           return await Task.FromResult(Stream.Null);
        }

       
    }
}