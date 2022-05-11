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
              string path=$"Channel/{Id.Value}.json";
              if(await storage.Continue(path))
              {
                  channel=await DownloadThumbnails(storage,await storage.YoutubeClient.Channels.GetAsync(Id.Value));
                  //channel=new SavedChannel(i);
                  await storage.WriteAllTextAsync(path,JsonConvert.SerializeObject(channel));
                  return channel;
              }else{
                  var j=JsonConvert.DeserializeObject<SavedChannel>(await storage.ReadAllTextAsync(path));
                  return j;
              }
            }else{
                 var c=await storage.YoutubeClient.Channels.GetByUserAsync(name1);
                channel=await DownloadThumbnails(storage,c);
                string path=$"Channel/{c.Id.Value}.json";
                if(await storage.Continue(path))
                {
                     await storage.WriteAllTextAsync(path,JsonConvert.SerializeObject(channel));

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
    
            string path=$"Playlist/{Id}.json"; 
            List<IVideo> videos=new List<IVideo>();
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
            await storage.WriteAllTextAsync(path,JsonConvert.SerializeObject(p));
            if(Resolution == Resolution.NoDownload) return;
            foreach(var item in videos)
            {
                VideoMediaContext context=new VideoMediaContext(item.Id,Resolution);
                await context.FillQueue(storage,Queue);
            }

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
            string path=$"Info/{Id}.json";
            SavedVideo video;
            if(await storage.Continue(path))
            {
                try{
                    video = new SavedVideo(await storage.YoutubeClient.Videos.GetAsync(Id));
                    
                    storage.SendBeforeSaveInfo(video);
                    await storage.WriteAllTextAsync(path,JsonConvert.SerializeObject(video));
                    await video.DownloadThumbnails(storage);
                }catch(Exception ex)
                {
                    _=ex;
                    return;
                }
                
            }else{
                video = JsonConvert.DeserializeObject<SavedVideo>(await storage.ReadAllTextAsync(path));
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