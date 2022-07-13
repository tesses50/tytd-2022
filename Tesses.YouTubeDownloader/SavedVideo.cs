using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;

namespace Tesses.YouTubeDownloader
{
    public class VideoDownloadProgress
    {
       
        public SavedVideo Saved { get; set; }

        public Resolution Resolution {get;set;}
        public long Length { get; set; }
       
        public int Progress { get; set; }
        public double ProgressRaw { get; set; }
    }
    public class SavedVideoLegacy
    {
        
        
        public string Id {get;set;}
        public string Title {get;set;}
        public string AuthorChannelId {get;set;}

        public string AuthorTitle {get;set;}

        public string Description {get;set;}

        public string[] Keywords {get;set;}

        public long Likes {get;set;}

        public long Dislikes {get;set;}

        public long Views {get;set;}

        public double Duration {get;set;}

        public string UploadDate {get;set;}

        public List<(int,int,string)> Thumbnails {get;set;}

        public SavedVideo ToSavedVideo()
        {
            SavedVideo video=new SavedVideo();
            video.Id=Id;
            video.Keywords=Keywords;
            video.LegacyVideo=true;
            video.Likes=Likes;
            video.Dislikes=Dislikes;
            video.Duration=TimeSpan.FromSeconds(Duration);
            video.Title=Title;
            video.AuthorChannelId=AuthorChannelId;
            video.AuthorTitle=AuthorTitle;
            video.Description=Description;
            video.UploadDate = DateTime.Parse(UploadDate);
            return video;
        }
    }
     public class SavedVideo
    {
        public SavedVideo()
        {
            TYTDTag="";
            Id = "";
            Title = "";
            AuthorChannelId = "";
            AuthorTitle = "";
            Description = "";
            Keywords = new string[0];
            Likes = 0;
            Dislikes = 0;
            Views = 0;
            Duration = TimeSpan.Zero;
            UploadDate=new DateTime(1992,8,20);
            AddDate=DateTime.Now;
            LegacyVideo=false;
            DownloadFrom="YouTube";
            VideoFrozen=false;
        }

        public SavedVideo(Video video)
        {
            TYTDTag=TYTDStorage.TYTDTag;
            Id=video.Id;
            Title = video.Title;
            AuthorChannelId = video.Author.ChannelId;
            AuthorTitle = video.Author.ChannelTitle;
            Description = video.Description;
            Keywords=video.Keywords.ToArray();
            Likes=video.Engagement.LikeCount;
            Dislikes = video.Engagement.DislikeCount;
            Views = video.Engagement.ViewCount;
            Duration = video.Duration.Value;
            UploadDate = video.UploadDate.DateTime;
            AddDate=DateTime.Now;
            LegacyVideo=false;
            DownloadFrom="YouTube";
            VideoFrozen=false;
        }

         public string TYTDTag {get;set;}
        public bool LegacyVideo {get;set;}
          public bool VideoFrozen {get;set;}
        
        public string DownloadFrom {get;set;}
        public SavedVideoLegacy ToLegacy()
        {
        
            SavedVideoLegacy legacy=new SavedVideoLegacy();
            legacy.Thumbnails=new List<(int, int, string)>();
            legacy.Thumbnails.Add((120,90,$"https://s.ytimg.com/vi/{Id}/default.jpg"));
            legacy.Thumbnails.Add((480,360,$"https://s.ytimg.com/vi/{Id}/hqdefault.jpg"));
            legacy.Thumbnails.Add((320,180,$"https://s.ytimg.com/vi/{Id}/mqdefault.jpg"));

            legacy.AuthorChannelId=AuthorChannelId;
            legacy.AuthorTitle=AuthorTitle;
            legacy.Description=Description;
            legacy.Dislikes=Dislikes;
            legacy.Duration=Duration.TotalSeconds;
            legacy.Id=Id;
            legacy.Likes=Likes;
            legacy.Title=Title;
            legacy.UploadDate=UploadDate.ToString();
            legacy.Views=Views;
            
            
            return legacy;
        }
        public Video ToVideo()
        {
            List<Thumbnail> thumbnails=new List<Thumbnail>();
            thumbnails.Add(new Thumbnail($"https://s.ytimg.com/vi/{Id}/default.jpg",new YoutubeExplode.Common.Resolution(120,90)));
            thumbnails.Add(new Thumbnail($"https://s.ytimg.com/vi/{Id}/hqdefault.jpg",new YoutubeExplode.Common.Resolution(480,360)));
            thumbnails.Add(new Thumbnail($"https://s.ytimg.com/vi/{Id}/mqdefault.jpg",new YoutubeExplode.Common.Resolution(320,180)));
            
            return new Video(Id,Title,new YoutubeExplode.Common.Author(AuthorChannelId,AuthorTitle),new DateTimeOffset(UploadDate),Description,Duration,thumbnails,Keywords.ToList(),new Engagement(Views,Likes,Dislikes));
        }
        private static bool TimeSpanTryParse(string time,out TimeSpan ts)
        {
            ts=TimeSpan.Zero;
            if(string.IsNullOrWhiteSpace(time))
            {
                return false;
            }

            int i=time.Count((e)=>{return e==':';});
            if(i == 0)
            {
                int someNumber;
                if(int.TryParse(time,out someNumber))
                {
                    ts=TimeSpan.FromSeconds(someNumber);
                    return true;
                }

            }
            if(i == 1)
            {
                string t="00:" + time;
                if(TimeSpan.TryParse(t,out ts))
                {
                    return true;
                }
            }

            return TimeSpan.TryParse(time,out ts);
        }

        public List<Chapter> GetChapters()
        {
            List<Chapter> chapters = new List<Chapter>();
            //a line should start with time then be followed by info
            bool found_0 = false;
            foreach (var line in Description.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] splitLine = line.Split(new char[] { ' ' }, 2);
                if (splitLine.Length == 2)
                {
                    if (found_0)
                    {
                        TimeSpan time;
                        
                        if (TimeSpanTryParse(splitLine[0], out time))
                        {
                            var ls=chapters.LastOrDefault();
                            if(ls !=null)
                            {
                                ls.Length = time - ls.Offset;
                            }
                            chapters.Add(new Chapter(time, splitLine[1]));
                        }
                        
                    }
                    else
                    if (splitLine[0] == "0:00" || splitLine[0] == "00:00:00" || splitLine[0] == "0:00:00")
                    {
                        found_0 = true;
                        chapters.Add(new Chapter(TimeSpan.FromSeconds(0), splitLine[1]));
                    }
                }
            }
            var ls2=chapters.LastOrDefault();
                            if(ls2 !=null)
                            {
                                ls2.Length = Duration - ls2.Offset;
                            }
            return chapters;
        }
        public async Task<bool> VideoExistsAsync(TYTDBase baseCtl,Resolution res=Resolution.PreMuxed)
        {
             string resDir = TYTDManager.ResolutionToDirectory(res);
             string path=$"{resDir}/{Id}.mp4";

            return await baseCtl.FileExistsAsync(path);
        }
        public DateTime AddDate {get;set;}
         public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public string[] Keywords { get; set; }
        public string Id { get; set; }
        public string AuthorTitle { get; set; }
        public string AuthorChannelId { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public long Views { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }

        public async Task DownloadThumbnails(TYTDStorage manager)
        {
            await manager.DownloadThumbnails(Id);
        }
       
        public override string ToString()
        {
            StringBuilder b=new StringBuilder();
            b.AppendLine($"Title: {Title}");
            b.AppendLine($"AuthorTitle: {AuthorTitle}");
            DateTime date=UploadDate;
            
                 b.AppendLine($"Upload Date: {date.ToShortDateString()}");
            
            b.AppendLine($"Likes: {Likes}, Dislikes: {Dislikes}, Views: {Views}");
            b.AppendLine($"Duration: {Duration.ToString()}");
            b.AppendLine($"Tags: {string.Join(", ",Keywords)}");
            b.AppendLine("Description:");
            b.AppendLine(Description);
            return b.ToString();
        }
    }
      public class SavedPlaylist
    {
        public SavedPlaylist()
        {
            TYTDTag="";
            Title = "";
            AuthorChannelId="";
            AuthorTitle="";
            Id="";
            Description="";
            Videos=new List<string>();
        }
        public SavedPlaylist(Playlist playlist,List<IVideo> videos)
        {
            TYTDTag=TYTDStorage.TYTDTag;
            Title = playlist.Title;
            AuthorChannelId = playlist.Author.ChannelId;
            AuthorTitle=playlist.Author.ChannelTitle;
            Description =playlist.Description;
            Id=playlist.Id;
            Videos = videos.Select<IVideo,string>((e)=>{return e.Id;}).ToList();
        }
        
        public Playlist ToPlaylist()
        {
            List<Thumbnail> thumbnails=new List<Thumbnail>();
            return new Playlist(Id,Title,new Author(AuthorChannelId,AuthorTitle),Description,thumbnails);
        }

        public List<(VideoId Id,Resolution Resolution)> ToPersonalPlaylist(Resolution res=Resolution.PreMuxed)
        {
            List<(VideoId Id,Resolution Resolution)> items=new List<(VideoId Id, Resolution Resolution)>();
            if(Videos !=null)
            {
                foreach(var vid in Videos)
                {
                    items.Add((vid,res));
                }
            }
            return items;
        }
        
        public async IAsyncEnumerable<SavedVideo> GetVideosAsync(TYTDBase baseCls)
        {
            if(Videos !=null)
            {
                foreach(var item in Videos)
                { 
                    if(await baseCls.FileExistsAsync($"Info/{item}.json"))
                    {
                        yield return await baseCls.GetVideoInfoAsync(item);
                    }
                }
            }
        }
        public List<string> Videos { get; set; }
        public string AuthorTitle { get; set; }
        public string AuthorChannelId { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
         public string TYTDTag {get;set;}
    }
    public class SavedChannel
    {
        public SavedChannel(Channel c)
        {
            Id=c.Id;
            Title=c.Title;
            TYTDTag=TYTDStorage.TYTDTag;
        }
        public SavedChannel()
        {
            Id="";
            Title="";
            TYTDTag="";
        }
        public async IAsyncEnumerable<SavedVideo> GetVideosAsync(TYTDBase baseCls)
        {
            
            
                await foreach(var item in baseCls.GetVideosAsync())
                { 
                    if(Id.Equals(item.AuthorChannelId,StringComparison.Ordinal))
                    {
                        yield return await Task.FromResult( item);
                    }
                }
            
        }
        public async IAsyncEnumerable<SavedPlaylist> GetPlaylistsAsync(TYTDBase baseCls)
        {
                await foreach(var item in baseCls.GetPlaylistsAsync())
                { 
                    if(Id.Equals(item.AuthorChannelId,StringComparison.Ordinal))
                    {
                        yield return await Task.FromResult( item);
                    }
                }
        }
        public string Id { get; set; }
        public string Title { get; set; }
         public string TYTDTag {get;set;}
    }
}