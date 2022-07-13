



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
namespace Tesses.YouTubeDownloader
{
    public partial class TYTDStorage 
    {
        /// <summary>
        ///Get video queue (Used by server)
        /// </summary>
        public IReadOnlyList<(SavedVideo Video,Resolution Resolution)> GetQueueList()
        {
            return QueueList;
        }
        /// <summary>
        /// Get progress (Used by server)
        /// </summary>
        public SavedVideoProgress GetProgress()
        {
            return Progress;
        }
        List<Subscription> Subscriptions {get;set;}
        List<(SavedVideo Video, Resolution Resolution)> QueueList = new List<(SavedVideo Video, Resolution Resolution)>();
        List<IMediaContext> Temporary =new List<IMediaContext>();

        public void WaitTillMediaContentQueueEmpty()
        {
            while(true)
            {
                lock(Temporary)
                {
                    if(Temporary.Count <= 0) return;
                }
                Thread.Sleep(100);
            }
        }
        
        private async Task QueueLoop(CancellationToken token)
        {

           try{
               
               Subscriptions=new List<Subscription>();
               await foreach(var sub in GetSubscriptionsAsync())
               {
                   Subscriptions.Add(sub);
               }
           }catch(Exception ex)
           {
               await GetLogger().WriteAsync(ex);
           }
            while(!token.IsCancellationRequested)
            { try{
               
                IMediaContext context;
                lock(Temporary)
                {
                    if(Temporary.Count > 0)
                    {
                        context=Temporary[0];
                        Temporary.RemoveAt(0);
                    }else{
                        context=null;
                    }

                }
                if(context != null)
                {
                    await context.FillQueue(this,QueueList);                   
                } 
                 } catch(Exception ex)
                {
                    //did this so app can keep running
                    await GetLogger().WriteAsync(ex);
                }
            }
          
        }
    }
}