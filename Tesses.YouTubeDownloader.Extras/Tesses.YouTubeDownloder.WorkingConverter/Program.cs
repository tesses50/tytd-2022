using Tesses.YouTubeDownloader;
namespace Tesses.YouTubeDownloader.WorkingConverter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            TYTDStorage storage=new TYTDCurrentDirectory();
            Progress<string> progress=new Progress<string>((e)=>{Console.WriteLine(e);});
            await storage.Legacy.ConvertFromLegacy(progress);
        }
    }
}