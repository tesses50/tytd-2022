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
    internal class LockObj
    {

    }
    public class TYTDErrorEventArgs : EventArgs
    {
        public TYTDErrorEventArgs(VideoId? id,Exception exception)
        {
            Id=id;
            Exception=exception;
            PrintError =true;
        }
        
        public VideoId? Id {get;set;}

        public Exception Exception {get;set;}

        public bool PrintError {get;set;}
    }
    public partial class TYTDStorage 
    {
       
        protected virtual LoggerProperties ReadLoggerProperties()
        {
              string data=ReadAllTextAsync("config/tytdprop.json").GetAwaiter().GetResult();
                return JsonConvert.DeserializeObject<LoggerProperties>(data);
        }
        protected virtual bool LoggerPropertiesExists
        {
            get{
            return FileExists("config/tytdprop.json");
            }
        }

        public event EventHandler<TYTDErrorEventArgs> Error;
        internal  LoggerProperties Properties {get;set;}
           public LoggerProperties GetProperties()
        {
          
            if(LoggerPropertiesExists)
            {
                return ReadLoggerProperties();
            }else{
                LoggerProperties prop=new LoggerProperties();
                prop.AddDateInLog=true;
                prop.LogVideoIds=true;
                prop.PrintErrors =false;
                prop.PrintVideoIds=true;
                prop.UseLogs=true;
                prop.SubscriptionInterval=TimeSpan.FromHours(1);
                prop.AlwaysDownloadChannel = false;
                prop.AllowExport=false;
               
                return prop;
            }
        }
        public LoggerProperties GetLoggerProperties()
        {
            if(Properties == null)
            {
                Properties=GetProperties();
            }
            
            return  Properties;
        }
        internal static LockObj o=new LockObj();
        
        private Logger _log=null;
        public Logger GetLogger()
        {
            lock(o){   
                if(_log == null)
                {
                    _log = new Logger(this);
                }
           
                return _log;
            }
        }
    }
    public class LoggerProperties
    {
        public bool AllowExport {get;set;}
        public bool AlwaysDownloadChannel {get;set;}
        public TimeSpan SubscriptionInterval {get;set;}

        public bool UseLogs {get;set;}

        public bool PrintVideoIds {get;set;}

        public bool PrintErrors {get;set;}

        public bool LogVideoIds {get;set;}


        public bool AddDateInLog {get;set;}

        
    }
    public class Logger
    {
        
        private string _filename;
        private TYTDStorage _storage;
        
        
        
    internal Logger(TYTDStorage storage)
        {
            
            _storage=storage;
            
            string dateTime = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            _filename = $"config/logs/{dateTime}.log";
        }
        private void WriteStdErr(string message)
        {
            if(TYTDStorage.UseConsole){
                var col=Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ForegroundColor = col;
            }else{
                _storage.ConsoleWriter.WriteLine($"ERROR: {message}");
            }
        }
        private void WriteStd(string message,bool error)
        {
            if(error)
                WriteStdErr(message);
            else{
                if(TYTDStorage.UseConsole){
                var col=Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = col;
            }else{
                _storage.ConsoleWriter.WriteLine(message);
            }
            }
                
        }
        public async Task WriteAsync(string message,bool writeToConsole=false,bool isError=false,bool log=true)
        {
            await Task.Run(()=>{
                Write(message,writeToConsole,isError,log);       
            });
        }
        public void Write(string message,bool writeToConsole=false, bool isError=false,bool log=true)
        {
            
            if(writeToConsole) WriteStd(message,isError);
            if(!log || !_storage.GetLoggerProperties().UseLogs) return;

           // DateTime time = DateTime.Now;
        var msg= new StringBuilder();
        if(_storage.GetLoggerProperties().AddDateInLog)
        {
            var dat=DateTime.Now;
            msg.AppendLine($"{dat.ToShortDateString()} at {dat.ToShortTimeString()}:");
        }
        msg.AppendLine(message);
         lock(TYTDStorage.o){   
            try{
            using(var strm =  _storage.OpenOrCreateAsync(_filename).GetAwaiter().GetResult())
            {
                if(!strm.CanSeek) return;

                strm.Seek(0,SeekOrigin.End);
                using(var sw = new StreamWriter(strm))
                {
                    sw.WriteLine(msg.ToString());
                }
            } }
            catch(Exception ex)
            {
                _=ex;
            }
            //mtx.ReleaseMutex();
            }
        }
        public async Task WriteAsync(Exception ex)
        {
            await WriteAsync(ex,null);
        }
        public async Task WriteAsync(Exception ex,VideoId? id)
        {
            TYTDErrorEventArgs args=new TYTDErrorEventArgs(id,ex);
            _storage.ThrowError(args);
            
            await WriteAsync($"Exception Catched:\n{ex.ToString()}",_storage.GetLoggerProperties().PrintErrors && args.PrintError,true);
            
        }
        public async Task WriteAsync(SavedVideo video)
        {
            await WriteAsync($"Download: {video.Title} with Id: {video.Id}",_storage.GetLoggerProperties().PrintVideoIds,false,_storage.GetLoggerProperties().LogVideoIds);
        }

    }

}