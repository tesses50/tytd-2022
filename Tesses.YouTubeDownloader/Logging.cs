



using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
using System.Text;

namespace Tesses.YouTubeDownloader
{
    public partial class TYTDStorage 
    {
        private Mutex mtx0=new Mutex();
        private Logger _log=null;
        public Logger GetLogger()
        {
            mtx0.WaitOne();
            if(_log == null)
            {
                _log = new Logger(this);
            }
            mtx0.ReleaseMutex();
            return _log;
        }
    }
    public class LoggerProperties
    {
        public bool UseLogs {get;set;}

        public bool PrintVideoIds {get;set;}

        public bool PrintErrors {get;set;}

        public bool LogVideoIds {get;set;}


        public bool AddDateInLog {get;set;}
    }
    public class Logger
    {
        Mutex mtx=new Mutex();
        private string _filename;
        private TYTDStorage _storage;
        
        public LoggerProperties Properties {get;set;}

        private LoggerProperties GetProperties(TYTDStorage storage)
        {
            if(storage.FileExists("config/logger.json"))
            {
                string data=storage.ReadAllTextAsync("config/lggger.json").GetAwaiter().GetResult();
                return JsonConvert.DeserializeObject<LoggerProperties>(data);
            }else{
                LoggerProperties prop=new LoggerProperties();
                prop.AddDateInLog=true;
                prop.LogVideoIds=true;
                prop.PrintErrors =false;
                prop.PrintVideoIds=true;
                prop.UseLogs=true;
                return prop;
            }
        }
        internal Logger(TYTDStorage storage)
        {
            Properties = GetProperties(storage);
            _storage=storage;
            
            string dateTime = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            _filename = $"config/logs/{dateTime}.log";
        }
        private void WriteStdErr(string message)
        {
            var col=Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = col;
        }
        private void WriteStd(string message,bool error)
        {
            if(error)
                WriteStdErr(message);
            else
                Console.WriteLine(message);
        }
        public async Task WriteAsync(string message,bool writeToConsole=false, bool isError=false,bool log=true)
        {
            
            if(writeToConsole) WriteStd(message,isError);
            if(!log || !Properties.UseLogs) return;

           // DateTime time = DateTime.Now;
        var msg= new StringBuilder();
        if(Properties.AddDateInLog)
        {
            var dat=DateTime.Now;
            msg.AppendLine($"{dat.ToShortDateString()} at {dat.ToShortTimeString()}:");
        }
        msg.AppendLine(message);
            mtx.WaitOne();
            using(var strm = await _storage.OpenOrCreateAsync(_filename))
            {
                if(!strm.CanSeek) return;

                strm.Seek(0,SeekOrigin.End);
                using(var sw = new StreamWriter(strm))
                {
                    await sw.WriteLineAsync(msg.ToString());
                }
            } 
            mtx.ReleaseMutex();
        }

        public async Task WriteAsync(Exception ex)
        {
            await WriteAsync($"Exception Catched:\n{ex.ToString()}",Properties.PrintErrors,true);
        }
        public async Task WriteAsync(SavedVideo video)
        {
            await WriteAsync($"Download: {video.Title} with Id: {video.Id}",Properties.PrintVideoIds,false,Properties.LogVideoIds);
        }

    }

}