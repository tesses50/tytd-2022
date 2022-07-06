using System.Text;

namespace Tesses.YouTubeDownloader.Tools.Common
{
    public static class StringUtils
    {
        public static string GetSafeFileName(this string filename)
        {
            StringBuilder b=new StringBuilder(filename);
            foreach(var badChr in "\\\"\'/?*<>|:")
            {
                b.Replace(badChr.ToString(),"");
            }
            if(b.Length == 0) return "file";
            return b.ToString();
        }
    }
}