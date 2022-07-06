using Tesses.YouTubeDownloader;
using Tesses.YouTubeDownloader.Server;
using Tesses.WebServer;
using Newtonsoft.Json;

var config=TYTDConfiguration.Load();
Environment.CurrentDirectory=config.LocalFiles;
var c=new HttpClient();
TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory(c);
TYTDClient client=new TYTDClient(c,config.Url);

TYTDDownloaderStorageProxy proxy=new TYTDDownloaderStorageProxy();
proxy.Storage = currentDirectory;
proxy.Downloader=client;

TYTDServer server=new TYTDServer(proxy);
server.RootServer.Server=new StaticServer("WebSite");
currentDirectory.CanDownload=false;
HttpServerListener listener=new HttpServerListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,3252),server.InnerServer);
currentDirectory.StartLoop();
TYTDStorage.FFmpeg ="/usr/bin/ffmpeg";
Console.WriteLine("Almost Ready to Listen");
await listener.ListenAsync(CancellationToken.None);