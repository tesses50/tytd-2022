using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tesses.WebServer;
using System.Net;
using YoutubeExplode.Videos;
using System.Linq;
using System.IO;
using System.Text;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
using Tesses.Extensions;
using YoutubeExplode.Videos.Streams;

namespace Tesses.Extensions
{
    public static class Extensions
    {
        public static string Substring(this string value,string str)
        {
            return value.Substring(str.Length);
        }
        public static async Task RedirectBackAsync(this ServerContext ctx)
        {
            await ctx.SendTextAsync("<script>history.back()</script>\n");
        }
    }
}

namespace Tesses.YouTubeDownloader.Server
{
 using Tesses.YouTubeDownloader;

public class Progress
{
    public long Length {get;set;}
    public double Percent {get;set;}
    
    public SavedVideo Video {get;set;}

    public bool StartEvent {get;set;}
    public bool StopEvent {get;set;}
}
internal static class B64
{
    public static string Base64UrlEncodes(string arg)
    {
        return Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(arg));
    }

    public static string Base64Encode(byte[] arg)
    {
        return Convert.ToBase64String(arg);
    }
    public static byte[] Base64Decode(string arg)
    {
        return Convert.FromBase64String(arg);
    }

    public static string Base64Encodes(string arg)
    {
        return Base64Encode(System.Text.Encoding.UTF8.GetBytes(arg));
    }

    public  static string Base64UrlEncode(byte[] arg)
    {
        string s = Convert.ToBase64String(arg); // Regular base64 encoder
        s = s.Split('=')[0]; // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
        return s;
    }
    public static string Base64Decodes(string arg)
    {
        return System.Text.Encoding.UTF8.GetString(Base64Decode(arg));
    }
    public static string Base64UrlDecodes(string arg)
    {
        return System.Text.Encoding.UTF8.GetString(Base64UrlDecode(arg));
    }
    public static byte[] Base64UrlDecode(string arg)
    {
        string s = arg;
        s = s.Replace('-', '+'); // 62nd char of encoding
        s = s.Replace('_', '/'); // 63rd char of encoding
        switch (s.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2: s += "=="; break; // Two pad chars
            case 3: s += "="; break; // One pad char
            default: throw new System.Exception(
              "Illegal base64url string!");
        }
        return Convert.FromBase64String(s); // Standard base64 decoder
    }

}

    internal class ApiV1Server : Tesses.WebServer.Server
    {
        IDownloader downloader1;

        public ApiV1Server(IDownloader dl)
        {
            downloader1=dl;
        }

        public override async Task GetAsync(ServerContext ctx)
        {
            string path=ctx.UrlAndQuery;

            if(path.StartsWith("/AddPlaylistRes/"))
            {     string id_res=path.Substring(16);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                         await downloader1.AddPlaylistAsync(id_res_split[1],(Resolution)num);
                     }
                 }
            }
             if(path.StartsWith("/AddChannelRes/"))
            {     string id_res=path.Substring(15);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                         await downloader1.AddChannelAsync(id_res_split[1],(Resolution)num);
                     }
                 }
            }
            
            if(path.StartsWith("/AddChannel/"))
            {
                 await downloader1.AddChannelAsync(path.Substring(12),Resolution.PreMuxed);
                     
            }
            if(path.StartsWith("/AddPlaylist/"))
            {
                 await downloader1.AddPlaylistAsync(path.Substring(13),Resolution.PreMuxed);
                     
            }
            if(path.StartsWith("/AddVideoRes/"))
            {     string id_res=path.Substring(13);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                         await downloader1.AddVideoAsync(id_res_split[1],(Resolution)num);
                     }
                 }
            }

            if(path.StartsWith("/AddVideo/"))
            {
                      //string id_res=path.Substring(12);
                 //string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 //if(id_res_split.Length ==2)
                 //{
                     
                         await downloader1.AddVideoAsync(path.Substring(10),Resolution.PreMuxed);
                     
                // }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
            if(path.StartsWith("/AddItemRes/"))
            {
                       string id_res=path.Substring(12);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                         await downloader1.AddItemAsync(id_res_split[1],(Resolution)num);
                     }
                 }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
             if(path.StartsWith("/AddItem/"))
            {
                       //string id_res=path.Substring(12);
                 //string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 //if(id_res_split.Length ==2)
                 //{
                     
                         await downloader1.AddItemAsync(path.Substring(9),Resolution.PreMuxed);
                     
                // }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
            if(path.StartsWith("/AddUserRes/"))
            {
                       string id_res=path.Substring(12);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                         await downloader1.AddUserAsync(id_res_split[1],(Resolution)num);
                     }
                 }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
             if(path.StartsWith("/AddUser/"))
            {
                       //string id_res=path.Substring(12);
                 //string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 //if(id_res_split.Length ==2)
                 //{
                     
                         await downloader1.AddUserAsync(path.Substring(9),Resolution.PreMuxed);
                     
                // }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
            if(path.StartsWith("/AddFile/"))
            {
                       //string id_res=path.Substring(12);
                 //string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 //if(id_res_split.Length ==2)
                 //{
                     
                         await downloader1.AddFileAsync(path.Substring(9));
                     
                // }
                // await ctx.SendTextAsync(
               // $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            //);
            
            }
           await ctx.RedirectBackAsync();
        }
    }
    internal class ApiStorage : Tesses.WebServer.Server
    {
        ITYTDBase baseCtl;
        public ApiStorage(ITYTDBase baseCtl)
        {
            this.baseCtl=baseCtl;

        }
        public static System.Net.Mime.ContentDisposition GetVideoContentDisposition(string name)
        {
            var cd = new System.Net.Mime.ContentDisposition();
            string filename = GetVideoName(name);
            cd.FileName = filename;
            cd.DispositionType =  System.Net.Mime.DispositionTypeNames.Inline;
            
            return cd;
        }
        public static string GetVideoName(string name)
        {
            
            

            string asAscii = Encoding.ASCII.GetString(
            Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding(
                    Encoding.ASCII.EncodingName,
                        new EncoderReplacementFallback(string.Empty),
                        new DecoderExceptionFallback()
                    ),
                    Encoding.UTF8.GetBytes(name)
                )
            );
            return asAscii;
        }
     
        public override async Task GetAsync(ServerContext ctx)
        {
            string path=ctx.UrlAndQuery;
            /*if(path.StartsWith("/File/NotConverted/"))
            {
                // redirect to new
                // /File/NotConverted/xxxxxxxxxxx.mp4
                string idmp4=WebUtility.UrlDecode(path.Substring(19));
                if(idmp4.Length == 15)
                {
                    string id=Path.GetFileNameWithoutExtension(idmp4);
                    string path2 = $"Info/{id}.json";
                    
                if(!await baseCtl.FileExistsAsync(path2))
                 {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                 } var data= await baseCtl.ReadAllTextAsync(path2);
                var data2=JsonConvert.DeserializeObject<SavedVideo>(data);
              var loc=  await BestStreams.GetPathResolution(baseCtl,data2,Resolution.PreMuxed);
                
                if(!await baseCtl.FileExistsAsync(loc))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                using(var s = await baseCtl.OpenReadAsyncWithLength(loc))
                {
                    await ctx.SendStreamAsync(s,HeyRed.Mime.MimeTypesMap.GetMimeType(loc));
                }

                }
            }
            else if(path.StartsWith("/File/Converted/"))
            {
                // redirect to new
                // /File/NotConverted/xxxxxxxxxxx.mp4
                string idmp4=WebUtility.UrlDecode(path.Substring(16));
                if(idmp4.Length == 15)
                {
                    string id=Path.GetFileNameWithoutExtension(idmp4);
                    string path2 = $"Info/{id}.json";
                    
                if(!await baseCtl.FileExistsAsync(path2))
                 {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                 } var data= await baseCtl.ReadAllTextAsync(path2);
                var data2=JsonConvert.DeserializeObject<SavedVideo>(data);
              var loc=  await BestStreams.GetPathResolution(baseCtl,data2,Resolution.Mux);
                
                if(!await baseCtl.FileExistsAsync(loc))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                using(var s = await baseCtl.OpenReadAsyncWithLength(loc))
                {
                    await ctx.SendStreamAsync(s,HeyRed.Mime.MimeTypesMap.GetMimeType(loc));
                }

                }
            }
           else if(path.StartsWith("/File/Audio/"))
            {
                // redirect to new
                // /File/NotConverted/xxxxxxxxxxx.mp4
                string idmp4=WebUtility.UrlDecode(path.Substring(12));
                if(idmp4.Length == 15)
                {
                    string id=Path.GetFileNameWithoutExtension(idmp4);
                    string path2 = $"Info/{id}.json";
                    
                if(!await baseCtl.FileExistsAsync(path2))
                 {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                 } var data= await baseCtl.ReadAllTextAsync(path2);
                var data2=JsonConvert.DeserializeObject<SavedVideo>(data);
              var loc=  await BestStreams.GetPathResolution(baseCtl,data2,Resolution.AudioOnly);
                
                if(!await baseCtl.FileExistsAsync(loc))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                using(var s = await baseCtl.OpenReadAsyncWithLength(loc))
                {
                    await ctx.SendStreamAsync(s,HeyRed.Mime.MimeTypesMap.GetMimeType(loc));
                }

                }
            }
            else if(path.StartsWith("/File/Info/"))
            {
                 string idjson=WebUtility.UrlDecode(path.Substring(11));
                 string path2 = $"Info/{idjson}";
                 if(!await baseCtl.FileExistsAsync(path2))
                 {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                 }
                 var data= await baseCtl.ReadAllTextAsync(path2);
                var data2=JsonConvert.DeserializeObject<SavedVideo>(data);
                await ctx.SendJsonAsync(data2.ToLegacy());
            }
            else*/
            if(path.StartsWith("/File/FileInfo/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/File/FileInfo/")));
                string url = B64.Base64UrlDecodes(file);
                if(baseCtl.DownloadExists(url))
                {
                    var obj=await baseCtl.GetDownloadInfoAsync(url);
                   await ctx.SendJsonAsync(obj);
                }else{
                    ctx.StatusCode = 404;
                    ctx.NetworkStream.Close();
                }
            }
            else if(path.StartsWith("/File/Info/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/File/Info/")));
                VideoId? id =VideoId.TryParse(file);
                if(id.HasValue && baseCtl.VideoInfoExists(id.Value))
                {
                   var obj=await baseCtl.GetVideoInfoAsync(id.Value);
                   await ctx.SendJsonAsync(obj);
                }else{
                    ctx.StatusCode = 404;
                    ctx.NetworkStream.Close();
                }
            }else if(path.StartsWith("/File/Playlist/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/File/Playlist/")));
                PlaylistId? id =PlaylistId.TryParse(file);
                if(id.HasValue && baseCtl.PlaylistInfoExists(id.Value))
                {
                   var obj=await baseCtl.GetPlaylistInfoAsync(id.Value);
                   await ctx.SendJsonAsync(obj);
                }else{
                    ctx.StatusCode = 404;
                    ctx.NetworkStream.Close();
                }
            }else if(path.StartsWith("/File/Channel/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/File/Channel/")));
                ChannelId? id =ChannelId.TryParse(file);
                if(id.HasValue && baseCtl.ChannelInfoExists(id.Value))
                {
                    
                   var obj=await baseCtl.GetChannelInfoAsync(id.Value);
                   await ctx.SendJsonAsync(obj);
                }else{
                    ctx.StatusCode = 404;
                    ctx.NetworkStream.Close();
                }
            }else if(path.StartsWith("/File/StreamInfo/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/File/Info/")));
                VideoId? id =VideoId.TryParse(file);
                if(id.HasValue && baseCtl.BestStreamInfoExists(id.Value))
                {
                   var obj=await baseCtl.GetBestStreamInfoAsync(id.Value);
                   await ctx.SendJsonAsync(obj);
                }else{
                    ctx.StatusCode = 404;
                    ctx.NetworkStream.Close();
                }
            }
            else if(path.StartsWith("/File/"))
            {
                string file=WebUtility.UrlDecode(path.Substring(6));
                if(!await baseCtl.FileExistsAsync(file))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                using(var s = await baseCtl.OpenReadAsync(file))
                {
                    await ctx.SendStreamAsync(s);
                }
            }/*else if(path.StartsWith("/File-v2/"))
            {
                string file=WebUtility.UrlDecode(path.Substring(9));
                if(!await baseCtl.FileExistsAsync(file))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                 using(var s = await baseCtl.OpenReadAsyncWithLength(file))
                {
                    await ctx.SendStreamAsync(s);
                }
            }*/
            else if(path.StartsWith("/GetFiles/FileInfo") || path.StartsWith("/GetFiles/FileInfo/"))
            {
                List<string> urls=new List<string>();
                await foreach(var url in baseCtl.GetDownloadUrlsAsync())
                {
                    urls.Add($"{B64.Base64UrlEncodes(url)}.json");
                }
                 await ctx.SendJsonAsync(urls);
            }
            else if(path.StartsWith("/GetFiles/Info") || path.StartsWith("/GetFiles/Info/") || path.StartsWith("/GetFiles/StreamInfo") || path.StartsWith("/GetFiles/StreamInfo/"))
            {
                bool containsStrmInfo=path.Contains("StreamInfo");
                List<string> items=new List<string>();
                await foreach(var vid in baseCtl.GetVideoIdsAsync())
                {
                    var vid2=VideoId.TryParse(vid);
                    
                    if(!containsStrmInfo || (vid2.HasValue && baseCtl.BestStreamInfoExists(vid2.Value))){
                        items.Add($"{vid}.json");
                    }
                }
                await ctx.SendJsonAsync(items);
            }else if(path.StartsWith("/GetFiles/Playlist") || path.StartsWith("/GetFiles/Playlist/"))
            {
                
                List<string> items=new List<string>();
                await foreach(var vid in baseCtl.GetPlaylistIdsAsync())
                {
                    items.Add($"{vid}.json");
                }
                await ctx.SendJsonAsync(items);
            }else if(path.StartsWith("/GetFiles/Channel") || path.StartsWith("/GetFiles/Channel/"))
            {
                List<string> items=new List<string>();
                await foreach(var vid in baseCtl.GetChannelIdsAsync())
                {
                    items.Add($"{vid}.json");
                }
                await ctx.SendJsonAsync(items);
            }
            else if(path.StartsWith("/GetFiles/"))
            {
              await ctx.SendJsonAsync(baseCtl.EnumerateFiles( WebUtility.UrlDecode(path.Substring(10))).ToList());
            }
            else if(path.StartsWith("/GetDirectories/"))
            {
                 await ctx.SendJsonAsync(baseCtl.EnumerateDirectories( WebUtility.UrlDecode(path.Substring(16))).ToList());
            }
             else if(path.StartsWith("/FileExists/StreamInfo/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/FileExists/StreamInfo/")));
                VideoId? id =VideoId.TryParse(file);
                if(id.HasValue && baseCtl.BestStreamInfoExists(id.Value))
                {
                    await ctx.SendTextAsync( "true","text/plain");
                }else{
                    await ctx.SendTextAsync( "false","text/plain");
                }
            }
            else if(path.StartsWith("/FileExists/FileInfo/"))
            {
                 string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/FileExists/StreamInfo/")));
                string url = B64.Base64Decodes(file);
                
                if(baseCtl.DownloadExists(url))
                {
                    await ctx.SendTextAsync( "true","text/plain");
                }else{
                    await ctx.SendTextAsync( "false","text/plain");
                }
            }
            else if(path.StartsWith("/FileExists/Info/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/FileExists/Info/")));
                VideoId? id =VideoId.TryParse(file);
                if(id.HasValue && baseCtl.VideoInfoExists(id.Value))
                {
                    await ctx.SendTextAsync( "true","text/plain");
                }else{
                    await ctx.SendTextAsync( "false","text/plain");
                }
            } else if(path.StartsWith("/FileExists/Playlist/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/FileExists/Playlist/")));
                PlaylistId? id =PlaylistId.TryParse(file);
                if(id.HasValue && baseCtl.PlaylistInfoExists(id.Value))
                {
                    await ctx.SendTextAsync( "true","text/plain");
                }else{
                    await ctx.SendTextAsync( "false","text/plain");
                }
            }else if(path.StartsWith("/FileExists/Channel/"))
            {
                string file = Path.GetFileNameWithoutExtension(WebUtility.UrlDecode(path.Substring("/FileExists/Channel/")));
                ChannelId? id =ChannelId.TryParse(file);
                if(id.HasValue && baseCtl.ChannelInfoExists(id.Value))
                {
                    await ctx.SendTextAsync( "true","text/plain");
                }else{
                    await ctx.SendTextAsync( "false","text/plain");
                }
            }
            else if(path.StartsWith("/FileExists/"))
            {
                await ctx.SendTextAsync(baseCtl.FileExists(WebUtility.UrlDecode(path.Substring(12))) ? "true" : "false","text/plain");
            }else if(path.StartsWith("/DirectoryExists/"))
            {
                 await ctx.SendTextAsync(baseCtl.DirectoryExists(WebUtility.UrlDecode(path.Substring(17))) ? "true" : "false","text/plain");
            }else if(path.StartsWith("/Download/"))
            {
                string url = path.Substring("/Download/");
                if(baseCtl.DownloadExists(url) && baseCtl.DownloadFileExists(url))
                {
                            var v = await baseCtl.GetDownloadInfoAsync(url);
                           string header=GetVideoContentDisposition(v.Title).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using(var strm = await baseCtl.OpenReadAsync(baseCtl.GetDownloadFile(url)))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(v.Title));
                            }
                }
            }
            else if(path.StartsWith("/Video/"))
            {
                
                string id=path.Substring(7);
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue){
                    if(baseCtl.VideoInfoExists(id1.Value))
                    {
                        //Console.WriteLine("Id exists");
                        SavedVideo v = await baseCtl.GetVideoInfoAsync(id1.Value);
                        
                      string path0=  await BestStreams.GetPathResolution(baseCtl,v,Resolution.PreMuxed);
                      
                       if(!string.IsNullOrWhiteSpace(path0))
                       {
                           
                           //Console.WriteLine("F is not null");
                           string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using(var strm = await baseCtl.OpenReadAsync(path0))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                       }
                    }
                }

            }
            else if(path.StartsWith("/VideoRes/"))
            {
                 string id_res=path.Substring(10);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;

                       
                        
                       

                     VideoId? id1=VideoId.TryParse(id_res_split[1]);
                if(id1.HasValue){
                    if(baseCtl.VideoInfoExists(id1.Value))
                    {
                        //Console.WriteLine("Id exists");
                        SavedVideo v = await baseCtl.GetVideoInfoAsync(id1.Value);
                        
                      string path0=  await BestStreams.GetPathResolution(baseCtl,v,(Resolution)num);
                      
                       if(!string.IsNullOrWhiteSpace(path0))
                       {
                           
                           //Console.WriteLine("F is not null");
                           string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using(var strm = await baseCtl.OpenReadAsync(path0))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                       }
                    }
                }
                     }
                 }
            }
            else if(path.StartsWith("/Watch/"))
            {
                
                string id=path.Substring(7);
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue){
                    int i=0;
                    alt:
                    if(i>= 10)
                    {
                        ctx.StatusCode=500;
                        
                        return;
                    }
                    if(baseCtl.VideoInfoExists(id1.Value))
                    {

                        //Console.WriteLine("Id exists");
                        SavedVideo v = await baseCtl.GetVideoInfoAsync(id1.Value);
                     var res=   await BestStreamInfo.GetBestStreams(baseCtl,id1.Value);
                      string path0=  await BestStreams.GetPathResolution(baseCtl,v,Resolution.PreMuxed);
                      
                       if(!string.IsNullOrWhiteSpace(path0) && baseCtl.VideoInfoExists(id1.Value))
                       {
                           
                           //Console.WriteLine("F is not null");
                           string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using(var strm = await baseCtl.OpenReadAsync(path0))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                       }else{
                           //stream to browser
                           string url=res.MuxedStreamInfo.Url;
                            var b = baseCtl as TYTDStorage;
                            if(b != null)
                            {
                               
                                string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using( var strm=await b.YoutubeClient.Videos.Streams.GetAsync(res.MuxedStreamInfo))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                            }else{
                                 ctx.StatusCode=500;
                                return;
                            }
                       }
                    }else{
                         var b = baseCtl as TYTDStorage;
                            if(b != null)
                            {
                                var videoInfo=await b.YoutubeClient.Videos.GetAsync(id1.Value);
                                await b.WriteVideoInfoAsync(new SavedVideo(videoInfo));
                            }else{
                                 ctx.StatusCode=500;
                                return;
                            }
                        i++;
                        goto alt;
                    }
                    
                }

            }
            else if(path.StartsWith("/WatchRes/"))
            {
                  string id_res=path.Substring(10);
                 string[] id_res_split = id_res.Split(new char[] {'/'},2,StringSplitOptions.RemoveEmptyEntries);
                 if(id_res_split.Length ==2)
                 {
                     int num;
                     if(int.TryParse(id_res_split[0],out num))
                     {
                         if(num < 0) num=1;
                         if(num > 3) num=1;
                     VideoId? id1=VideoId.TryParse(id_res_split[1]);

                if(id1.HasValue){
                    int i=0;
                    alt:
                    if(i>= 10)
                    {
                        ctx.StatusCode=500;
                        
                        return;
                    }
                    if(baseCtl.VideoInfoExists(id1.Value))
                    {

                        //Console.WriteLine("Id exists");
                        SavedVideo v = await baseCtl.GetVideoInfoAsync(id1.Value);
                     var res=   await BestStreamInfo.GetBestStreams(baseCtl,id1.Value);
                      string path0=  await BestStreams.GetPathResolution(baseCtl,v,(Resolution)num);
                      
                       if(!string.IsNullOrWhiteSpace(path0) && baseCtl.VideoInfoExists(id1.Value))
                       {
                           
                           //Console.WriteLine("F is not null");
                           string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            using(var strm = await baseCtl.OpenReadAsync(path0))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                       }else{
                           //stream to browser
                           
                            var b = baseCtl as TYTDStorage;
                            if(b != null)
                            {
                               
                                string filename = $"{v.Title}-{Path.GetFileName(path0)}";
                           string header=GetVideoContentDisposition(filename).ToString();
                           ctx.ResponseHeaders.Add("Content-Disposition",header);
                            IStreamInfo info=res.MuxedStreamInfo;
                            if(num == 2)
                            {
                                info = res.AudioOnlyStreamInfo;
                            }else if(num == 3){
                                info = res.VideoOnlyStreamInfo;
                            }

                            using( var strm=await b.YoutubeClient.Videos.Streams.GetAsync(info))
                            {
                                await ctx.SendStreamAsync(strm,HeyRed.Mime.MimeTypesMap.GetMimeType(filename));
                            }
                            }else{
                                 ctx.StatusCode=500;
                                return;
                            }
                       }
                    }else{
                         var b = baseCtl as TYTDStorage;
                            if(b != null)
                            {
                                var videoInfo=await b.YoutubeClient.Videos.GetAsync(id1.Value);
                                await b.WriteVideoInfoAsync(new SavedVideo(videoInfo));
                            }else{
                                 ctx.StatusCode=500;
                                return;
                            }
                        i++;
                        goto alt;
                    }
                    
                }

                }
                 }
            }
            else{
                await NotFoundServer.ServerNull.GetAsync(ctx);
            }
        }
    }
    internal class ApiV2Server : RouteServer
    {
        IDownloader Downloader;
        public ApiV2Server(IDownloader downloader)
        {
                this.Downloader=downloader;
                AddBoth("/event",Event);
                AddBoth("/CancelDownload",Cancel);
                AddBoth("/Search",Search);
                AddBoth("/AddItem",AddItem);
                AddBoth("/AddChannel",AddChannel);
                AddBoth("/AddUser",AddUser);
                AddBoth("/AddPlaylist",AddPlaylist);
                AddBoth("/AddVideo",AddVideo);
                AddBoth("/AddFile",AddFile);
                AddBoth("/Progress",ProgressFunc);
                AddBoth("/QueueList",QueueList);
                AddBoth("/subscribe",Subscribe);
                AddBoth("/resubscribe",Resubscribe);
                AddBoth("/unsubscribe",Unsubscribe);
                AddBoth("/subscriptions",Subscriptions);
                AddBoth("/Subscribe",Subscribe);
                AddBoth("/Resubscribe",Resubscribe);
                AddBoth("/Unsubscribe",Unsubscribe);
                AddBoth("/Subscriptions",Subscriptions);
                AddBoth("/AddToList",AddToList);
                AddBoth("/DeleteFromList",DeleteFromList);
                Add("/ReplaceList",ReplaceList,"POST");
                AddBoth("/DeleteList",DeleteList);
                AddBoth("/SetResolutionInList",SetResolutionInList);

                Add("/export/everything.json",Everything_Export,"GET");
                Add("/export/videos.json",VideosExport,"GET");
                Add("/export/playlists.json",PlaylistsExport,"GET");
                Add("/export/channels.json",ChannelsExport,"GET");
                Add("/export/filedownloads.json",FilesExport,"GET");
                Add("/export/subscriptions.json",SubscriptionsExport,"GET");
                Add("/export/personal_lists.json",PersonalListsExport,"GET");
                
                /*
                public async Task AddToPersonalPlaylistAsync(string name, IEnumerable<(VideoId Id, Resolution Resolution)> items)
        {
            throw new NotImplementedException();
        }

        public async Task ReplacePersonalPlaylistAsync(string name, IEnumerable<(VideoId Id, Resolution Resolution)> items)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveItemFromPersonalPlaylistAsync(string name, VideoId id)
        {
            throw new NotImplementedException();
        }

        public async Task SetResolutionForItemInPersonalPlaylistAsync(string name, VideoId id, Resolution resolution)
        {
            throw new NotImplementedException();
        }*/
        }
        public async Task Event(ServerContext ctx)
        {
              IStorage storage=Downloader as IStorage;
                if(storage != null){
                
                var _p=Downloader.GetProgress();
                    long len = _p.Length;
                    bool first=true;
                    
                    SendEvents evts=new SendEvents();
                    storage.VideoStarted += (sender,e)=>
                    {
                        len=e.EstimatedLength;
                        Progress p=new Progress();
                        p.StartEvent=true;
                        p.StopEvent=false;
                        p.Length=e.EstimatedLength;
                        p.Percent=0;
                       
                        p.Video=e.VideoInfo;
                         evts.SendEvent(p);
                    };
                    storage.VideoProgress += (sender,e)=>{
                        Progress p=new Progress();
                        p.StartEvent=false;
                        p.StopEvent=false;
                        p.Length=len;
                        p.Percent=e.Progress;
                        if(first)
                            p.Video=e.VideoInfo;
                        
                        first=false;
                         evts.SendEvent(p);
                    };
                    storage.VideoFinished +=(sender,e)=>{
                        Progress p=new Progress();
                        p.StartEvent=false;
                        p.StopEvent=true;
                        p.Length=len;
                        p.Percent=1;
                        p.Video=e.VideoInfo;
                        evts.SendEvent(p);
                    };
                    ctx.ServerSentEvents(evts);
                }else{
                    await ctx.SendTextAsync("Error no IStorage");
                }
                    
        }
        private async Task Cancel(ServerContext ctx)
        {
            bool restart=false;
            string restartString="false";
            if(ctx.QueryParams.TryGetFirst("restart",out restartString))
            {
                if(!bool.TryParse(restartString,out restart))
                {
                    restart=false;
                }
            }
            Downloader.CancelDownload(restart);
            await ctx.RedirectBackAsync();
        }

        private async Task Search(ServerContext ctx)
        {
            var dl = Downloader as IStorage;
            string q;
            if(ctx.QueryParams.TryGetFirst("q",out q))
            {
                bool getInfoBool=false;
                string getInfo;
                if(ctx.QueryParams.TryGetFirst("getinfo",out getInfo))
                {
                    if(!bool.TryParse(getInfo,out getInfoBool)) getInfoBool=false;
                }
                List<SearchResult> results=new List<SearchResult>();
                await foreach(var vid in dl.SearchYouTubeAsync(q,getInfoBool))
                {
                    results.Add(vid);
                }
                if(getInfoBool)
                {
                    dl.WaitTillMediaContentQueueEmpty();
                }
                await ctx.SendJsonAsync(results);
            }
        }

        private void AddBoth(string url,HttpActionAsync action)
        {
            Add(url,action);
            Add(url,async(evt)=>{
                evt.ParseBody();
                await action(evt);
            },"POST");
        }
        public async Task DeleteList(ServerContext ctx)
        {
            //this is for personal playlists
              string name;
            if(ctx.QueryParams.TryGetFirst("name",out name)){
                Downloader.DeletePersonalPlaylist(name);
               
                //Downloader.AddToPersonalPlaylistAsync(name);

            }
            await ctx.RedirectBackAsync();
        }
           public async Task ReplaceList(ServerContext ctx)
        {
            
            //this is for personal playlists
            string name;
            if(ctx.QueryParams.TryGetFirst("name",out name)){
                string jsonData;
                List<ListContentItem> itemList;
                if(ctx.QueryParams.TryGetFirst("data",out jsonData))
                {
                     itemList = JsonConvert.DeserializeObject<List<ListContentItem>>(jsonData);
                    
                     await Downloader.ReplacePersonalPlaylistAsync(name,itemList);

          
                }
               
                //Downloader.AddToPersonalPlaylistAsync(name);

            }
             await ctx.RedirectBackAsync();
        }
        public async Task Everything_Export(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportEverythingAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task VideosExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportVideosAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task PlaylistsExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportPlaylistsAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task ChannelsExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportChannelsAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task FilesExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportDownloadsAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task SubscriptionsExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportSubscriptionsAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task PersonalListsExport(ServerContext ctx)
        {
            var storage = Downloader as TYTDStorage;
            if(storage != null)
            {
                if(storage.GetLoggerProperties().AllowExport)
                {
                    TYTDExporter exporter=new TYTDExporter(storage);
                    var res=await exporter.ExportPersonalPlaylistsAsync();
                    await ctx.SendJsonAsync(res);
                }else{
                    ctx.StatusCode=403;
                    await ctx.SendTextAsync("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Can't Export, Access Denied</title></head><body><h1>Can't Export, Access Denied</h1><p>Call the TYTD adminstrator if you are not the administrator to edit the following</p><hr><p>In file: <i><b>config/tytdprop.json</b></i>, unless overriden in code<br>Change <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">false</font> with <font color=\"#ce9178\">&quot;AllowExport&quot;</font>:<font color=\"#569cd6\">true</font></p></body></html>");
                }
            }
        }
        public async Task AddToList(ServerContext ctx)
        {
            
            //this is for personal playlists
            string name;
            if(ctx.QueryParams.TryGetFirst("name",out name)){
                string jsonData;
                List<ListContentItem> itemList;
                if(ctx.Method == "POST" && ctx.QueryParams.TryGetFirst("data",out jsonData))
                {
                     itemList = JsonConvert.DeserializeObject<List<ListContentItem>>(jsonData);
                    
                }else{
                    itemList=new List<ListContentItem>();
                    string id;
            
            if(ctx.QueryParams.TryGetFirst("v",out id))
            {
               
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue)
                { 
                    itemList.Add(new ListContentItem(id1,resolution));
                   
                }
            }                        
          
                }
                await Downloader.AddToPersonalPlaylistAsync(name,itemList);

                //Downloader.AddToPersonalPlaylistAsync(name);

            }
             await ctx.RedirectBackAsync();
        }
        public async Task DeleteFromList(ServerContext ctx)
        {
            //this is for personal playlists
            string name;
            if(ctx.QueryParams.TryGetFirst("name",out name)){
            string id;
            
            if(ctx.QueryParams.TryGetFirst("v",out id))
            {
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue)
                { 
                    await Downloader.RemoveItemFromPersonalPlaylistAsync(name,id1.Value);
                }
            }
            }
           await ctx.RedirectBackAsync();
        }
         public async Task SetResolutionInList(ServerContext ctx)
        {
            //this is for personal playlists
            string name;
            if(ctx.QueryParams.TryGetFirst("name",out name)){
            string id;
            
            if(ctx.QueryParams.TryGetFirst("v",out id))
            {
                 Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue)
                { 
                    await Downloader.SetResolutionForItemInPersonalPlaylistAsync(name,id1.Value,resolution);
                }
            }
            }
            await ctx.RedirectBackAsync();
        }
        public async Task Subscriptions(ServerContext ctx)
        {
            IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
               
                        
                var sub=storage.GetLoadedSubscriptions();
                await ctx.SendJsonAsync(sub);
                     
                 
            }
           await ctx.RedirectBackAsync();
        }
        public async Task Resubscribe(ServerContext ctx)
                {
            IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
                string id;

                 if(ctx.QueryParams.TryGetFirst("id",out id))
                 {
                    
                     string confstr;
                     ChannelBellInfo conf=ChannelBellInfo.NotifyAndDownload;
                    if(ctx.QueryParams.TryGetFirst("conf",out confstr))
                    {
                        if(!Enum.TryParse<ChannelBellInfo>(confstr,out conf))
                        {
                            conf = ChannelBellInfo.NotifyAndDownload;
                        }
                    }
                    
                     ChannelId? cid=ChannelId.TryParse(id);

                     if(cid.HasValue)
                     {
                        
                         await storage.ResubscribeAsync(cid.Value,conf);
                     }
                 }
            }
           await ctx.RedirectBackAsync();
        }

        public async Task Unsubscribe(ServerContext ctx)
        {
        IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
                string id;

                 if(ctx.QueryParams.TryGetFirst("id",out id))
                 {
                    
                    
                    
                     ChannelId? cid=ChannelId.TryParse(id);

                     if(cid.HasValue)
                     {
                        
                         storage.Unsubscribe(cid.Value);
                         
                     }
                 }
            }
            await ctx.RedirectBackAsync();
        }
        public async Task Subscribe(ServerContext ctx)
        {
            IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
                string id;

                 if(ctx.QueryParams.TryGetFirst("id",out id))
                 {
                     string getinfostr;
                     bool getinfo=true;
                     if(ctx.QueryParams.TryGetFirst("getinfo",out getinfostr))
                     {
                         if(getinfostr == "false")
                         {
                             getinfo=false;
                         }
                     }
                     string confstr;
                     ChannelBellInfo conf=ChannelBellInfo.NotifyAndDownload;
                    if(ctx.QueryParams.TryGetFirst("conf",out confstr))
                    {
                        if(!Enum.TryParse<ChannelBellInfo>(confstr,out conf))
                        {
                            conf = ChannelBellInfo.NotifyAndDownload;
                        }
                    }
                    
                     ChannelId? cid=ChannelId.TryParse(id);

                     if(cid.HasValue)
                     {
                        
                         await storage.SubscribeAsync(cid.Value,getinfo,conf);
                     }else{
                        UserName? uname=UserName.TryParse(id);
                        await storage.SubscribeAsync(uname.Value,conf);

                     }
                 }
            }
            await ctx.RedirectBackAsync();
        }
       
        public async Task QueueList(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetQueueList());
        }
        public async Task ProgressFunc(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetProgress());
        }
        public async Task AddFile(ServerContext ctx)
        {
            string url;
            string downloadStr;
            bool download=true;
            if(ctx.QueryParams.TryGetFirst("url",out url))
            {
                if(ctx.QueryParams.TryGetFirst("download",out downloadStr))
                {
                    bool dl;
                    if(bool.TryParse(downloadStr,out dl))
                    {
                        download=dl;
                    }
                }

                await Downloader.AddFileAsync(url,download);
                await ctx.RedirectBackAsync();
            }
        }
         public async Task AddVideo(ServerContext ctx)
        {
            string id;
            
            if(ctx.QueryParams.TryGetFirst("v",out id))
            {
               
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue)
                { 
                    
                    await Downloader.AddVideoAsync(id1.Value,resolution);
                }
            }                        
           await ctx.RedirectBackAsync();
        }    
        public async Task AddItem(ServerContext ctx)
        {
            string id;
            if(ctx.QueryParams.TryGetFirst("v",out id))
            {
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
             
                    await Downloader.AddItemAsync(id,resolution);
                
            }                        
            await ctx.RedirectBackAsync();
        }    
        public async Task AddUser(ServerContext ctx)
        {
            string id;
            if(ctx.QueryParams.TryGetFirst("id",out id))
            {
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                UserName? id1=UserName.TryParse(id);
                if(id1.HasValue)
                {
                    await Downloader.AddUserAsync(id1.Value,resolution);
                }
            }                        
            await ctx.RedirectBackAsync();
        }    
   
        public async Task AddChannel(ServerContext ctx)
        {
            string id;
            if(ctx.QueryParams.TryGetFirst("id",out id))
            {
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                ChannelId? id1=ChannelId.TryParse(id);
                if(id1.HasValue)
                {
                    await Downloader.AddChannelAsync(id1.Value,resolution);
                }
            }                        
            await ctx.RedirectBackAsync();
        }    
   
            public async Task AddPlaylist(ServerContext ctx)
        {
            string id;
            if(ctx.QueryParams.TryGetFirst("id",out id))
            {
                Resolution resolution=Resolution.PreMuxed;
                string res;
                if(ctx.QueryParams.TryGetFirst("res",out res))
                {
                    if(!Enum.TryParse<Resolution>(res,out resolution))
                    {
                        resolution=Resolution.PreMuxed;
                    }
                }
                PlaylistId? id1=PlaylistId.TryParse(id);
                if(id1.HasValue)
                {
                    await Downloader.AddPlaylistAsync(id1.Value,resolution);
                }
            }                        
            await ctx.RedirectBackAsync();
        }    
   
    }
    public class TYTDServer 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseCtl">TYTD context</param>
        public TYTDServer(ITYTDBase baseCtl)
        {
            ExtensionsServer=new ChangeableServer();
            RootServer=new ChangeableServer();
             MountableServer mountableServer=new MountableServer(RootServer);
            IDownloader downloader=baseCtl as IDownloader;
           
            if(downloader != null)
            {
                mountableServer.Mount("/api/",new ApiV1Server(downloader));
                mountableServer.Mount("/api/v2/",new ApiV2Server(downloader));
            }
             mountableServer.Mount("/api/v2/Extensions/",ExtensionsServer);
             mountableServer.Mount("/api/Storage/",new ApiStorage(baseCtl));
            
            InnerServer=mountableServer;
        }
        /// <summary>
        /// To provide to tesses.webserver
        /// </summary>
        
        public IServer InnerServer {get; private set;}
        /// <summary>
        /// Set by Nuget Package Tesses.YouTubeDownloader.ExtensionLoader
        /// </summary>
        
        public ChangeableServer ExtensionsServer {get; private set;}
        /// <summary>
        /// An optional static website (recomendeded)
        /// </summary>
        
        public ChangeableServer RootServer {get;private set;}
       
        
     }
}
