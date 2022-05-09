using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using YoutubeExplode.Channels;
using YoutubeExplode.Videos;

namespace Tesses.YouTubeDownloader
{
    
    public class BellEventArgs : EventArgs
    {
        public VideoId Id {get;set;}
    }
    public class VideoProgressEventArgs : EventArgs
    {
        public SavedVideo VideoInfo {get;set;}

        public double Progress {get;set;}
    }
    public class BeforeSaveInfoEventArgs : EventArgs
    {
        public SavedVideo VideoInfo {get;set;}

    }
    public class VideoStartedEventArgs : EventArgs
    {
        public SavedVideo VideoInfo {get;set;}
        public Resolution Resolution {get;set;}
        public long EstimatedLength {get;set;}
    }

    public class VideoFinishedEventArgs : EventArgs
    {
        public SavedVideo VideoInfo {get;set;}
        public Resolution Resolution {get;set;}
    }

    public abstract partial class TYTDStorage
    {
        ///<summary>
        ///For Notifications Like Bell on YouTube
        ///</summary>

        public event EventHandler<BellEventArgs> Bell;
        public event EventHandler<VideoStartedEventArgs> VideoStarted;

        public event EventHandler<BeforeSaveInfoEventArgs> BeforeSaveInfo;

        public  event EventHandler<VideoProgressEventArgs> VideoProgress;

        public event EventHandler<VideoFinishedEventArgs> VideoFinished;

        internal void SendBeforeSaveInfo(SavedVideo video)
        {
            BeforeSaveInfo?.Invoke(this,new BeforeSaveInfoEventArgs() {VideoInfo=video});
        }
        internal void SendBell(VideoId i)
        {
            
                Bell?.Invoke(this,new BellEventArgs() {Id=i});
            
        }

    
        public DateTime LastSubscriptionTime = DateTime.MinValue;
        public  async Task HandleSubscriptions()
        {
            var date=DateTime.Now;
            var interval=GetLoggerProperties().SubscriptionInterval;
            if(date - interval < LastSubscriptionTime)
            {
                return;
            }
            foreach(var sub in Subscriptions)
            {
                await sub.LookOnYouTube();
            }
            LastSubscriptionTime=date;
        }

        public async IAsyncEnumerable<Subscription> GetSubscriptionsAsync()
        {
            
            await foreach(var item in EnumerateFilesAsync("Subscriptions"))
            {
                if(Path.GetExtension(item).Equals(".json"))
                {
                    var sub=JsonConvert.DeserializeObject<Subscription>(item);
                    sub.Base=this;
                    yield return await Task.FromResult(sub);
                }
            }
        }
        /// <summary>
        /// Subscribe to creator
        /// </summary>
        public async Task Subscribe(ChannelId id, bool downloadChannelInfo=false,ChannelBellInfo bellInfo = ChannelBellInfo.NotifyAndDownload)
        {
            if(downloadChannelInfo)
            {
                 ChannelMediaContext context=new ChannelMediaContext(id,Resolution.NoDownload);
                 var c=await context.GetChannel(this);
            }
            Subscription sub=new Subscription();
            sub.BellInfo = bellInfo;
            sub.Base=this;
            sub.Id=id.Value;
            Subscriptions.Add(sub);
            await SaveSubscription(sub);
        }
        public async Task SaveSubscription(Subscription sub)
        {
            await WriteAllTextAsync($"Subscriptions/{sub.Id}",JsonConvert.SerializeObject(sub));
        }
        public async Task Subscribe(UserName name,ChannelBellInfo bellInfo=ChannelBellInfo.NotifyAndDownload)
        {
            ChannelMediaContext context=new ChannelMediaContext(name,Resolution.NoDownload);
            var c=await context.GetChannel(this);
            await Subscribe(ChannelId.Parse(c.Id),false,bellInfo);
        }
        public void Unsubscribe(ChannelId id)
        {
           Subscription sub= Subscriptions.FirstOrDefault(e=>e.Id==id.Value);
            if(sub != null)
            {
                Subscriptions.Remove(sub);
            }
            DeleteFile($"Subscriptions/{sub.Id}.json");
        }

        public  Subscription GetSubscription(ChannelId id)
        {
            return Subscriptions.FirstOrDefault(e=>e.Id==id.Value);
        }
        
    }
    [Flags]
    public enum ChannelBellInfo
    {
        DoNothing=0,
        GetInfo=1,
        
        Notify=2,

        Download=3,

        NotifyAndDownload=Notify|Download
    }

    public class Subscription
    {
        
        [Newtonsoft.Json.JsonIgnore]
        public TYTDStorage Base {get;set;}

        [Newtonsoft.Json.JsonIgnore]
        public DateTime LastCheckTime = DateTime.Now;

        public string Id {get;set;}

        public ChannelBellInfo BellInfo {get;set;}
        
        public async Task<SavedChannel>  GetChannelInfo()
        {
            if(await Base.FileExistsAsync($"Channel/{Id}.json"))
            {
                return await Base.GetChannelInfoAsync(Id);
            }
            return null;
        }

        public  async Task LookOnYouTube()
        {
            try{
            List<(string Id,DateTime Time)> items=new List<(string Id, DateTime Time)>();
            XmlDocument doc=new XmlDocument();
            doc.LoadXml(await Base.HttpClient.GetStringAsync($"https://www.youtube.com/feeds/videos.xml?channel_id={Id}"));
            foreach(XmlNode item in doc.GetElementsByTagName("entry"))
            {
                string Id="";
                DateTime Time=LastCheckTime-TimeSpan.FromMinutes(1);
          
            foreach(XmlNode item2 in item.ChildNodes)
            {
               if(item2.Name == "yt:videoId")
               {
                   if(BellInfo.HasFlag(ChannelBellInfo.Download))
                   {
                       await Base.AddVideoAsync(item2.InnerText);
                   }

                   Id=item2.InnerText;
               }
               if(item2.Name == "published")
               {
                   Time = DateTime.Parse(item2.InnerText);
               }
           }
           items.Add((Id,Time));
           }
           if(BellInfo.HasFlag(ChannelBellInfo.Notify) )
           {
                foreach(var item in items)
                {
                    if(item.Time > LastCheckTime)
                    {
                        Base.SendBell(item.Id);
                    }
                }
           }
           LastCheckTime=DateTime.Now;
            }catch(Exception ex)
            {
                await Base.GetLogger().WriteAsync(ex);
            }
        }
    }
}