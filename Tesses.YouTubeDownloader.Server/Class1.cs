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
        TYTDBase baseCtl;
        public ApiStorage(TYTDBase baseCtl)
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
            if(path.StartsWith("/File/"))
            {
                using(var s = await baseCtl.OpenReadAsyncWithLength( WebUtility.UrlDecode(path.Substring(6))))
                {
                    await ctx.SendStreamAsync(s);
                }
            }else if(path.StartsWith("/GetFiles/"))
            {
              await ctx.SendJsonAsync(baseCtl.EnumerateFiles( WebUtility.UrlDecode(path.Substring(10))).ToList());
            }else if(path.StartsWith("/GetDirectories/"))
            {
                 await ctx.SendJsonAsync(baseCtl.EnumerateDirectories( WebUtility.UrlDecode(path.Substring(16))).ToList());
            }else if(path.StartsWith("/FileExists/"))
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
                           string filename = Path.GetFileName(path0);
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
                           string filename = Path.GetFileName(path0);
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
                Add("/Progress",ProgressFunc);
                Add("/QueueList",QueueList);
        }
        public async Task QueueList(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetQueueList());
        }
        public async Task ProgressFunc(ServerContext ctx)
        {
            await ctx.SendJsonAsync(Downloader.GetProgress());
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
                VideoId? id1=VideoId.TryParse(id);
                if(id1.HasValue)
                {
                    await Downloader.AddItemAsync(id1,resolution);
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
        public TYTDServer(TYTDBase baseCtl)
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
