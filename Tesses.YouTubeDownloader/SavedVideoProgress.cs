namespace Tesses.YouTubeDownloader
{
    public class SavedVideoProgress
    {
        public SavedVideo Video {get;set;}
        public double ProgressRaw {get;set;}
        public int Progress {get;set;}
        public long Length {get;set;}
    }
}