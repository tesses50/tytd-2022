using System;
using Tesses.YouTubeDownloader.ExtensionLoader;
using Tesses.WebServer;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Tesses.YouTubeDownloader.ExampleExtension
{
    public class ExampleExtensionClass : Extension
    {
        string efn;
        public bool Enabled {get {return Storage.FileExists(efn);} set{
            if(value)
            {
                if(!Storage.FileExists(efn))
                {
                    Storage.WriteAllTextAsync(efn,"enabled").Wait();
                }
            }else{
                if(Storage.FileExists(efn))
                {
                    Storage.DeleteFile(efn);
                }
            }
        }}
        public bool FirstLoad()
        {
            string init=this.ExtensionStorage("init");
            bool first = !this.Storage.FileExists(init);
            if(first)
            {
                this.Storage.WriteAllTextAsync(init,"loaded").Wait();
            }
            return first;
        }
        public override void OnStart()
        {
            if(FirstLoad())
            {
                Enabled=true;
            }
            this.Storage.BeforeSaveInfo += BeforeSaveInfo;
            efn=this.ExtensionStorage("enabled");
            RouteServer svr=new RouteServer();
            svr.Add("/",Index);
            svr.Add("/setting.cgi",Setting);
        }
        public async Task Index(ServerContext ctx)
        {
            string enabledStr = Enabled ? " checked" : "";
            string index=$"<!DOCTYPE html><html lang=\"en\"><head> <meta charset=\"UTF-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Return YouTube Dislikes Addon for TYTD</title></head><body><h1>Return YouTube Dislikes Addon for TYTD</h1><form action=\"./setting.cgi\" method=\"GET\"><label for=\"enabled\">Enabled: </label><input type=\"checkbox\" name=\"enabled\" id=\"enabled\" {enabledStr}><br><input type=\"submit\" value=\"Save\"></form>This extension uses <a href=\"//returnyoutubedislike.com\">Return YouTube Dislikes</a></body></html>";
            await ctx.SendTextAsync(index);
        }
        public async Task Setting(ServerContext ctx)
        {
            Enabled=ctx.QueryParams.ContainsKey("enabled");

           await ctx.SendTextAsync(
                $"<html><head><titleYou Will Be Redirected in 5 Sec</title><meta http-equiv=\"Refresh\" content=\"5; url='./'\" /></head><body><h1>You Will Be Redirected in 5 Sec</h1></body></html>\n"
            );
        }
        public override string Name => "returnyoutubedislike.com";
        private async void BeforeSaveInfo(object sender, BeforeSaveInfoEventArgs e)
        {
            if(Enabled)
            e.VideoInfo.Dislikes=JsonConvert.DeserializeObject<Dislike>(await this.Storage.HttpClient.GetStringAsync($"https://returnyoutubedislikeapi.com/votes?videoId={e.VideoInfo.Id}")).dislikes;
        }
    }

    public class Dislike
    {
        public long dislikes {get;set;}
    }
}
