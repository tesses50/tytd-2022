using System;
namespace Tesses.YouTubeDownloader
{
    public class ConsoleWriteEventArgs : EventArgs
    {
        ///<summary>
        ///Use Console.Write(e.Text); not Console.WriteLine(e.Text);
        ///</summary>
        public string Text {get;set;}

        internal ConsoleWriteEventArgs(string e)
        {
            Text=e;
        }
    }
}