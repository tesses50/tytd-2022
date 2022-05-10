# Tesses.YouTubeDownloader

[Server Endpoints](docs/Server.md)  
<br>
[Classes and Enums](docs/JsonAndEnum.md)  

# What this is known to work on
  - Modern Linux/Windows/Mac (or any thing that can run .NET Standard 2.0+)
   - Works on my Wii with [wii-linux-ngx](https://www.github.com/neagix/wii-linux-ngx) and [this guide](https://tesses.net/apps/tytd/2022/wii.php) (if it 404s its not complete yet)

# To Use It as a server
    using Tesses.YouTubeDownloader.Server;
    using Tesses.YouTubeDownloader;
    ...
    TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory(new HttpClient());
    TYTDServer server=new TYTDServer(currentDirectory);
    server.RootServer.Server=new StaticServer("WebSite");
    HttpServerListener listener=new HttpServerListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,3252),server.InnerServer);
    currentDirectory.StartLoop();
    TYTDStorage.FFmpeg ="/usr/bin/ffmpeg";
    Console.WriteLine("Almost Ready to Listen");
    await listener.ListenAsync(CancellationToken.None);

Then:

     dotnet add package Tesses.YouTubeDownloader.Server
     
