using Tesses.YouTubeDownloader;
using Tesses.YouTubeDownloader.Server;
using Tesses.WebServer;
namespace Tesses.YouTubeDownloader.Net6
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory(new HttpClient());
            TYTDServer server=new TYTDServer(currentDirectory);
            server.RootServer.Server=new StaticServer("WebSite");
            HttpServerListener listener=new HttpServerListener(server.InnerServer);
            currentDirectory.StartLoop();
            TYTDStorage.FFmpeg ="/usr/bin/ffmpeg";
            Console.WriteLine("Almost Ready to Listen");
            await listener.ListenAsync(CancellationToken.None);
        }
    }
}