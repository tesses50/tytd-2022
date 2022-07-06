using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Tesses.YouTubeDownloader
{
   
    internal interface IMediaContext
    {
        Task FillQueue(TYTDStorage storage,List<(SavedVideo video,Resolution resolution)> Queue);
    }

    internal class ChannelMediaContext : IMediaContext
    {
        public ChannelMediaContext(ChannelId id,Resolution resolution)
        {
            Id=id;
            Resolution=resolution;
        }
        public ChannelMediaContext(UserName name,Resolution resolution)
        {
            name1=name;
            Resolution=resolution;
        }
        Resolution Resolution;
        UserName name1;
        ChannelId? Id; //made me nullable

        public async Task<SavedChannel> GetChannel(TYTDStorage storage)
        {
            SavedChannel channel;
            if(Id.HasValue) //dont check for if(Id != null) hince I was looking for several minutes for the bug
            {
              //string path=$"Channel/{Id.Value}.json";
              if(!storage.ChannelInfoExists(Id.Value))
              {
                  try{
                  channel=await DownloadThumbnails(storage,await storage.YoutubeClient.Channels.GetAsync(Id.Value));
                  //channel=new SavedChannel(i);
                  await storage.WriteChannelInfoAsync(channel);
                  }catch(Exception ex)
                  {
                      await storage.GetLogger().WriteAsync(ex);
                      return null;
                  }
                  return channel;
              }else{
                  var j=await storage.GetChannelInfoAsync(Id.Value);
                  return j;
              }
            }else{
                 var c=await storage.YoutubeClient.Channels.GetByUserAsync(name1);
                channel=await DownloadThumbnails(storage,c);
                //string path=$"Channel/{c.Id.Value}.json";
                if(!storage.ChannelInfoExists(c.Id.Value))
                {
                     await storage.WriteChannelInfoAsync(channel);

                }
                return channel;
            }
        }
        private async Task<SavedChannel> DownloadThumbnails(TYTDStorage storage,YoutubeExplode.Channels.Channel channel)
        {
            storage.CreateDirectoryIfNotExist($"Thumbnails/{channel.Id}");
            foreach(var item in channel.Thumbnails)
            {
                try{
                     string path=$"Thumbnails/{channel.Id}/{item.Resolution.Width}x{item.Resolution.Height}.jpg";
               
                    if(await storage.Continue(path))
                    {
                        using(var f = await storage.CreateAsync(path))
                        {
                            using(var src = await storage.HttpClient.GetStreamAsync(item.Url))
                            {
                                await src.CopyToAsync(f);
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    _=ex;
                }
            }
            return new SavedChannel(channel);
        }
        public async Task FillQueue(TYTDStorage storage, List<(SavedVideo video, Resolution resolution)> Queue)
        {
            var channel=await GetChannel(storage);
            
            if(Resolution == Resolution.NoDownload) return;
            await foreach(var video in storage.YoutubeClient.Channels.GetUploadsAsync(channel.Id))
            {
                VideoMediaContext media = new VideoMediaContext(video.Id,Resolution);
                await media.FillQueue(storage,Queue);
            }
        }
    }

    internal class PlaylistMediaContext : IMediaContext
    {
        
        PlaylistId Id;
        Resolution Resolution;
        public PlaylistMediaContext(PlaylistId id,Resolution res)
        {
            Id=id;
            Resolution=res;
            
        }
        public async Task FillQueue(TYTDStorage storage, List<(SavedVideo video, Resolution resolution)> Queue)
        {
    
           // string path=$"Playlist/{Id}.json"; 
            List<IVideo> videos=new List<IVideo>();
            try{
            
            await foreach(var vid in storage.YoutubeClient.Playlists.GetVideosAsync(Id))
            {
                videos.Add(vid);
            }
            var p=new SavedPlaylist(await storage.YoutubeClient.Playlists.GetAsync(Id),videos);
            if(storage.GetLoggerProperties().AlwaysDownloadChannel)
            {
                var c=ChannelId.Parse(p.AuthorChannelId);
                ChannelMediaContext cmc=new ChannelMediaContext(c,Resolution.NoDownload);
                await cmc.GetChannel(storage);
                
            }
            await storage.WritePlaylistInfoAsync(p);
            }catch(Exception ex)
            {
                await storage.GetLogger().WriteAsync(ex);
            }
            if(Resolution == Resolution.NoDownload) return;
            foreach(var item in videos)
            {
                VideoMediaContext context=new VideoMediaContext(item.Id,Resolution);
                await context.FillQueue(storage,Queue);
            }

        }
    }
    internal class NormalDownloadMediaContext : IMediaContext
    {
        public NormalDownloadMediaContext(string url,bool download=true)
        {
            this.url=url;
            this.download=download;
        }
        bool download;
        string url;
        public async Task FillQueue(TYTDStorage storage, List<(SavedVideo video, Resolution resolution)> Queue)
        {
            
            
            SavedVideo video=new SavedVideo();
            if(storage.DownloadExists(url)){
                video = await storage.GetDownloadInfoAsync(url);
            }else{
            video.Id = url;
            
            await GetFileNameAsync(storage,video);
            }
            lock(Queue){
            Queue.Add((video,Resolution.PreMuxed));
            }
        }
        private async Task GetFileNameAsync(TYTDStorage storage,SavedVideo video)
        {
            string[] uri0=url.Split(new char[]{'?'},2,StringSplitOptions.None);
            string filename=Path.GetFileName(uri0[0]);
            System.Net.Http.HttpRequestMessage message=new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head,url);
            message.Headers.Add("Range","bytes=0-");
            
            var head=await storage.HttpClient.SendAsync(message);
            if(head.Content.Headers.ContentDisposition != null && !string.IsNullOrWhiteSpace(head.Content.Headers.ContentDisposition.FileName))
            {
                filename = head.Content.Headers.ContentDisposition.FileName;
            }
            long length = 0;
            if(head.Content.Headers.ContentLength.HasValue)
            {
                length = head.Content.Headers.ContentLength.Value;
            }
            video.Title = filename;
            var res=head.StatusCode == System.Net.HttpStatusCode.PartialContent ? "true" : "false";
            video.DownloadFrom=$"NormalDownload,Length={length},CanSeek={res}";
            video.AuthorTitle = "NotYouTube";
            video.AuthorChannelId = "TYTD_FILEDOWNLOAD";
            List<string> hdrs=new List<string>();
            foreach(var hdr in head.Content.Headers)
            {
               foreach(var item in hdr.Value){
                hdrs.Add($"{hdr.Key}: {item}");
               }
            }
            string headers=string.Join("\n",hdrs);
            video.Description=$"File Download on \"{DateTime.Now.ToShortDateString()}\" at \"{DateTime.Now.ToShortTimeString()}\"\nHeaders:\n{headers}";
            video.Likes=42;
            video.Dislikes=42;
            video.Views=42;
            video.Duration = new TimeSpan(0,0,0);
            video.Keywords =  new string[] {"FILE"};
            if(head.Headers.Date.HasValue)
            {
                video.UploadDate = head.Headers.Date.Value.DateTime;
            }

            await storage.WriteVideoInfoAsync(video);
        
        }
    }
    internal class VideoMediaContext : IMediaContext
    {
        VideoId Id;
        Resolution resolution;
       
        public VideoMediaContext(VideoId id,Resolution res)
        {
            Id=id;
            resolution=res;
            
        }
        public async Task FillQueue(TYTDStorage storage,List<(SavedVideo,Resolution)> queue)
        {
           
            SavedVideo video;
            if(!storage.VideoInfoExists(Id))
            {
                try{
                    video = new SavedVideo(await storage.YoutubeClient.Videos.GetAsync(Id));
                    
                    storage.SendBeforeSaveInfo(video);
                    await storage.WriteVideoInfoAsync(video);
                    await video.DownloadThumbnails(storage);
                }catch(Exception ex)
                {
                    
                    await storage.GetLogger().WriteAsync(ex,Id);
                    return;
                }
                
            }else{
                video = await storage.GetVideoInfoAsync(Id);
            }
            if(storage.GetLoggerProperties().AlwaysDownloadChannel)
            {
                var c=ChannelId.Parse(video.AuthorChannelId);
                ChannelMediaContext cmc=new ChannelMediaContext(c,Resolution.NoDownload);
                await cmc.GetChannel(storage);
                
            }
            if(resolution == Resolution.NoDownload) return;
            lock(queue)
            {
                queue.Add((video,resolution));
            }
        }
    }
}