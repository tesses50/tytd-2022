using Tesses.YouTubeDownloader;
using Tesses.YouTubeDownloader.Server;
using Tesses.WebServer;
namespace Tesses.YouTubeDownloader.Net6
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if(args.Contains("--docker"))
            {

                Environment.CurrentDirectory="/data";
            }
            TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory(new HttpClient());
            TYTDServer server=new TYTDServer(currentDirectory);
            server.RootServer.Server=new StaticServer("WebSite");
            currentDirectory.CanDownload=true;
            HttpServerListener listener=new HttpServerListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,3252),server.InnerServer);
            currentDirectory.StartLoop();
            TYTDStorage.FFmpeg ="/usr/bin/ffmpeg";
            Console.WriteLine("Almost Ready to Listen");
            await listener.ListenAsync(CancellationToken.None);
           
        }
    }
}
