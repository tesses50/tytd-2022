using Newtonsoft.Json;
using Tesses.YouTubeDownloader;
using YoutubeExplode.Videos;

namespace Tesses.YouTubeDownloader.YTGET
{
    public class Program
    {
        public static HttpClient Client;
        public static async Task Main(string[] args)
        {
            Client=new HttpClient();
             var vars=Environment.GetEnvironmentVariables();
            if(vars.Contains("TYTD_DIR"))
            {
                Environment.CurrentDirectory=vars["TYTD_DIR"].ToString();
            }
            Resolution resolution=Resolution.PreMuxed;
            if(args.Length ==0)
            {
                while(true)
                {
                    Console.Write("> ");
                    string res=Console.ReadLine();
                    if(!string.IsNullOrWhiteSpace(res))
                    {
                        if(res.Equals("exit"))
                        {
                            return;
                        }else if(res.Equals("clear")){
                            Console.Clear();
                        }else if(res.Equals("save")) {
                            Console.Write("Type Video Id or Url: ");
                            string idOrUrl=Console.ReadLine();
                            Console.Write("Type destination directory: ");
                            string dest=Console.ReadLine();

                        } else if(res.Equals("help"))
                        {
                            Console.WriteLine("exit: Exit the program");
                            Console.WriteLine("clear: clear screen");
                            Console.WriteLine("resmux: set the Video Resolution to Mux");
                            Console.WriteLine("respremux: set the Video Resolution to PreMuxed");
                            Console.WriteLine("resaudio: set the Video Resolution to AudioOnly");
                            Console.WriteLine("resvidonly: set the Video Resolution to VideoOnly");
                            Console.WriteLine("Url or Id: Video Id or Url");
                            Console.WriteLine();
                        }
                        else if(res.Equals("resmux"))
                        {
                            Console.WriteLine("Video Resolution has been set to Mux");
                            resolution=Resolution.Mux;
                        }else if(res.Equals("respremux"))
                        {
                             Console.WriteLine("Video Resolution has been set to PreMuxed");
                            resolution = Resolution.PreMuxed;
                        }else if(res.Equals("resaudio"))
                        {
                            Console.WriteLine("Video Resolution has been set to AudioOnly");
                            resolution = Resolution.AudioOnly;
                        }else if(res.Equals("resvidonly"))
                        {
                             Console.WriteLine("Video Resolution has been set to VideoOnly");
                            resolution = Resolution.VideoOnly;
                        }else{
                             await Download(res);
                        }

                       
                    }
                }
            }
            foreach(var arg in args)
            {
                await Download(arg);
            }
        }
        public static async Task Download(string url,Resolution resolution=Resolution.PreMuxed,CancellationToken token=default(CancellationToken))
        {
            //download video
           
            TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory();
            currentDirectory.CreateDirectories();
            VideoId? id=VideoId.TryParse(url);
            if(id.HasValue)
            {
                var savedVideo=  await currentDirectory.GetSavedVideoAsync(id.Value);
                if(savedVideo !=null)
                {
                    Console.Write($"{savedVideo.Title}: ");
                 
                //bool first=true;
                using(ProgressBar bar=new ProgressBar())
                {
                  bar.Report(0.0);
               await currentDirectory.DownloadNoQueue(savedVideo,resolution,token,new Progress<double>((p)=>{
                    bar.Report(p);
                }));
                bar.Report(0.99);
                bar.Report(1.0);
               
                }
                 Console.WriteLine();
                
                }
            }
        }
    }
}