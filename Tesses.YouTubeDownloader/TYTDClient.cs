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

using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Utils.Extensions;
using System.Net.Http.Headers;

namespace Tesses.YouTubeDownloader
{

//From YouTubeExplode
internal static class Helpers
{
     public static async ValueTask<HttpResponseMessage> HeadAsync(
        this HttpClient http,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
        return await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
    }
     public static async ValueTask<Stream> GetStreamAsync(
        this HttpClient http,
        string requestUri,
        long? from = null,
        long? to = null,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Range = new RangeHeaderValue(from, to);

        var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStreamAsync();
    }

    public static async ValueTask<long?> TryGetContentLengthAsync(
        this HttpClient http,
        string requestUri,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.HeadAsync(requestUri, cancellationToken);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return response.Content.Headers.ContentLength;
    }
}


// Special abstraction that works around YouTube's stream throttling
// and provides seeking support.
// From YouTubeExplode
internal class SegmentedHttpStream : Stream
{
    private readonly HttpClient _http;
    private readonly string _url;
    private readonly long? _segmentSize;

    private Stream _segmentStream;
    private long _actualPosition;

    [ExcludeFromCodeCoverage]
    public override bool CanRead => true;

    [ExcludeFromCodeCoverage]
    public override bool CanSeek => true;

    [ExcludeFromCodeCoverage]
    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position { get; set; }

    public SegmentedHttpStream(HttpClient http, string url, long length, long? segmentSize)
    {
        _url = url;
        _http = http;
        Length = length;
        _segmentSize = segmentSize;
    }

    private void ResetSegmentStream()
    {
        _segmentStream?.Dispose();
        _segmentStream = null;
    }

    private async ValueTask<Stream> ResolveSegmentStreamAsync(
        CancellationToken cancellationToken = default)
    {
        if (_segmentStream != null)
            return _segmentStream;

        var from = Position;

        var to = _segmentSize != null
            ? Position + _segmentSize - 1
            : null;

        var stream = await _http.GetStreamAsync(_url, from, to, true, cancellationToken);

        return _segmentStream = stream;
    }

    public async ValueTask PreloadAsync(CancellationToken cancellationToken = default) =>
        await ResolveSegmentStreamAsync(cancellationToken);

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        while (true)
        {
            // Check if consumer changed position between reads
            if (_actualPosition != Position)
                ResetSegmentStream();

            // Check if finished reading (exit condition)
            if (Position >= Length)
                return 0;

            var stream = await ResolveSegmentStreamAsync(cancellationToken);
            var bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
            _actualPosition = Position += bytesRead;

            if (bytesRead != 0)
                return bytesRead;

            // Reached the end of the segment, try to load the next one
            ResetSegmentStream();
        }
    }

    [ExcludeFromCodeCoverage]
    public override int Read(byte[] buffer, int offset, int count) =>
        ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

    [ExcludeFromCodeCoverage]
    public override long Seek(long offset, SeekOrigin origin) => Position = origin switch
    {
        SeekOrigin.Begin => offset,
        SeekOrigin.Current => Position + offset,
        SeekOrigin.End => Length + offset,
        _ => throw new ArgumentOutOfRangeException(nameof(origin))
    };

    [ExcludeFromCodeCoverage]
    public override void Flush() =>
        throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override void SetLength(long value) =>
        throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            ResetSegmentStream();
        }
    }
}
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
        public async Task AddFileAsync(string url,bool download=true)
        {
            try{
                await client.GetStringAsync($"{url}api/v2/AddFile?url={WebUtility.UrlEncode(url)}&download={download}");
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
                var strmLen= await client.TryGetContentLengthAsync($"{url}api/Storage/File/{path}",true);
            SegmentedHttpStream v=new SegmentedHttpStream(client,$"{url}api/Storage/File/{path}",strmLen.GetValueOrDefault(),null);
            return v;
            }catch(Exception ex)
            {
                _=ex;
            }

           return Stream.Null;
        }

        public async Task AddToPersonalPlaylistAsync(string name, IEnumerable<ListContentItem> items)
        {
             Dictionary<string,string> values=new Dictionary<string, string>
            {
                { "name", name},
                { "data", JsonConvert.SerializeObject(items.ToArray())}
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync($"{url}api/v2/AddToList",content);
            var resposeStr = await response.Content.ReadAsStringAsync();
        }

        public async Task ReplacePersonalPlaylistAsync(string name, IEnumerable<ListContentItem> items)
        {
            //ReplaceList
            Dictionary<string,string> values=new Dictionary<string, string>
            {
                { "name", name},
                { "data", JsonConvert.SerializeObject(items.ToArray())}
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync($"{url}api/v2/ReplaceList",content);
            var resposeStr = await response.Content.ReadAsStringAsync();
        }

        public async Task RemoveItemFromPersonalPlaylistAsync(string name, VideoId id)
        {
             try{
                
                  
                  await client.GetStringAsync($"{url}api/v2/DeleteFromList?name={WebUtility.UrlEncode(name)}&v={id.Value}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public async Task SetResolutionForItemInPersonalPlaylistAsync(string name, VideoId id, Resolution resolution)
        {
              try{
                
                  
                  await client.GetStringAsync($"{url}api/v2/SetResolutionInList?name={WebUtility.UrlEncode(name)}&v={id.Value}&res={resolution.ToString()}");
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
        public void DeletePersonalPlaylist(string name)
        {
             try{
                
                  
                  client.GetStringAsync($"{url}api/v2/DeleteList?name={WebUtility.UrlEncode(name)}").GetAwaiter().GetResult();
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }

        public void CancelDownload(bool restart = false)
        {
             try{
                
                  
                  client.GetStringAsync($"{url}api/v2/CancelDownload?restart={restart}").GetAwaiter().GetResult();
            
            }catch(Exception ex)
            {
                _=ex;
            }
        }
    }
}