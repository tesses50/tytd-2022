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

namespace Tesses.YouTubeDownloader.Server
{
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
            await ctx.SendRedirectAsync("/");
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
            await ctx.SendRedirectAsync("/");
            }
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
            else*/ if(path.StartsWith("/File/"))
            {
                string file=WebUtility.UrlDecode(path.Substring(6));
                if(!await baseCtl.FileExistsAsync(file))
                {
                    await NotFoundServer.ServerNull.GetAsync(ctx);
                    return;
                }
                using(var s = await baseCtl.OpenReadAsyncWithLength(file))
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
            else if(path.StartsWith("/GetFiles/"))
            {
              await ctx.SendJsonAsync(baseCtl.EnumerateFiles( WebUtility.UrlDecode(path.Substring(10))).ToList());
            }else if(path.StartsWith("/GetDirectories/"))
            {
                 await ctx.SendJsonAsync(baseCtl.EnumerateDirectories( WebUtility.UrlDecode(path.Substring(16))).ToList());
            }else if(path.StartsWith("/FileExists-v2/"))
            {
                await ctx.SendTextAsync(baseCtl.FileExists(WebUtility.UrlDecode(path.Substring(15))) ? "true" : "false","text/plain");
            }
            else if(path.StartsWith("/FileExists/"))
            {
                await ctx.SendTextAsync(baseCtl.FileExists(WebUtility.UrlDecode(path.Substring(12))) ? "true" : "false","text/plain");
            }else if(path.StartsWith("/DirectoryExists/"))
            {
                 await ctx.SendTextAsync(baseCtl.DirectoryExists(WebUtility.UrlDecode(path.Substring(17))) ? "true" : "false","text/plain");
            }else if(path.StartsWith("/Video/"))
            {
                
                string id=path.Substring(7);
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue){
                    if(baseCtl.FileExists($"Info/{id1.Value.Value}.json"))
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

            }else if(path.StartsWith("/VideoRes/"))
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
                    if(baseCtl.FileExists($"Info/{id1.Value.Value}.json"))
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
                
                Add("/AddItem",AddItem);
                Add("/AddChannel",AddChannel);
                Add("/AddUser",AddUser);
                Add("/AddPlaylist",AddPlaylist);
                Add("/AddVideo",AddVideo);
                Add("/Progress",ProgressFunc);
                Add("/QueueList",QueueList);
                Add("/subscribe",Subscribe);
                Add("/resubscribe",Resubscribe);
                Add("/unsubscribe",Unsubscribe);
                Add("/subscriptions",Subscriptions);
                
        }
        public async Task Subscriptions(ServerContext ctx)
        {
            IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
               
                        
                var sub=storage.GetLoadedSubscriptions();
                await ctx.SendJsonAsync(sub);
                     
                 
            }
             
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
                    
                     ChannelId? cid=ChannelId.TryParse(WebUtility.UrlDecode(id));

                     if(cid.HasValue)
                     {
                        
                         await storage.ResubscribeAsync(cid.Value,conf);
                     }
                 }
            }
             await ctx.SendTextAsync(
                $"<html><head><title>You Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
        }

        public async Task Unsubscribe(ServerContext ctx)
        {
        IStorage storage = Downloader as IStorage;
            if(storage != null)
            {
                string id;

                 if(ctx.QueryParams.TryGetFirst("id",out id))
                 {
                    
                    
                    
                     ChannelId? cid=ChannelId.TryParse(WebUtility.UrlDecode(id));

                     if(cid.HasValue)
                     {
                        
                         storage.Unsubscribe(cid.Value);
                         
                     }
                 }
            }
             await ctx.SendTextAsync(
                $"<html><head><title>You Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
                    
                     ChannelId? cid=ChannelId.TryParse(WebUtility.UrlDecode(id));

                     if(cid.HasValue)
                     {
                        
                         await storage.SubscribeAsync(cid.Value,getinfo,conf);
                     }else{
                        UserName? uname=UserName.TryParse(WebUtility.UrlDecode(id));
                        await storage.SubscribeAsync(uname.Value,conf);

                     }
                 }
            }
             await ctx.SendTextAsync(
                $"<html><head><title>You Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
        }
       
        public async Task QueueList(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetQueueList());
        }
        public async Task ProgressFunc(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetProgress());
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
            await ctx.SendTextAsync(
                $"<html><head><title>You Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
            await ctx.SendTextAsync(
                $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
            await ctx.SendTextAsync(
                $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
            await ctx.SendTextAsync(
                $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
            await ctx.SendTextAsync(
                $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='../../'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
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
