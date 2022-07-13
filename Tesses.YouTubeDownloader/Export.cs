using Newtonsoft.Json;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using System.Threading;
using YoutubeExplode.Exceptions;
using System.Collections.Generic;

namespace Tesses.YouTubeDownloader
{
    public class TYTDExporter
    {
        ITYTDBase _base;
        string _tytd_tag;
        public TYTDExporter(ITYTDBase baseCtl)
        {
            _tytd_tag=TYTDStorage.TYTDTag;
            _base=baseCtl;
        }

        public async Task<EverythingExport> ExportEverythingAsync()
        {
            EverythingExport everythingExport=new EverythingExport();
            everythingExport.Videos=await ExportVideosAsync();
            everythingExport.Playlists = await ExportPlaylistsAsync();
            everythingExport.Channels = await ExportChannelsAsync();
            everythingExport.DownloadedFiles=await ExportDownloadsAsync();
            everythingExport.Subscriptions= await ExportSubscriptionsAsync();
            everythingExport.PersonalPlaylists=await ExportPersonalPlaylistsAsync();
            everythingExport.TYTDTag = _tytd_tag;
            return everythingExport;
        }
        public async Task<List<SavedVideo>> ExportVideosAsync()
        {
            List<SavedVideo> videos=new List<SavedVideo>();
            await foreach(var item in _base.GetVideosAsync())
            {
                videos.Add(item);
            }
            return videos;
        }
        public async Task<List<SavedPlaylist>> ExportPlaylistsAsync()
        {
            List<SavedPlaylist> videos=new List<SavedPlaylist>();
            await foreach(var item in _base.GetPlaylistsAsync())
            {
                videos.Add(item);
            }
            return videos;
        }
        public async Task<List<SavedChannel>> ExportChannelsAsync()
        {
            List<SavedChannel> videos=new List<SavedChannel>();
            await foreach(var item in _base.GetChannelsAsync())
            {
                videos.Add(item);
            }
            return videos;
        }
        public async Task<List<SavedVideo>> ExportDownloadsAsync()
        {
            List<SavedVideo> videos=new List<SavedVideo>();
            await foreach(var item in _base.GetDownloadsAsync())
            {
                videos.Add(item);
            }
            return videos;
        }
        public async Task<List<Subscription>> ExportSubscriptionsAsync()
        {
            List<Subscription> subs=new List<Subscription>();
           var dler = _base as IDownloader;
           if(dler != null)
           {
               await foreach(var item in dler.GetSubscriptionsAsync())
               {
                    subs.Add(item);
               }
           }
           return subs;
        }
        public async Task<List<PersonalPlaylist>> ExportPersonalPlaylistsAsync()
        {
            List<PersonalPlaylist> playlists=new List<PersonalPlaylist>();
            await foreach(var item in _base.GetPersonalPlaylistsAsync())
            {
                PersonalPlaylist personalPlaylist=new PersonalPlaylist();
                personalPlaylist.Name=item;
                personalPlaylist.Items=new List<ListContentItem>();
                await foreach(var item2 in _base.GetPersonalPlaylistContentsAsync(item))
                {
                    personalPlaylist.Items.Add(item2);
                }
                playlists.Add(personalPlaylist);
            }

            return playlists;
        }


    }

    public class EverythingExport
    {
        public string TYTDTag {get;set;}
        public List<SavedVideo> Videos {get;set;}

        public List<SavedPlaylist> Playlists {get;set;}

        public List<SavedChannel> Channels {get;set;}

        public List<SavedVideo> DownloadedFiles {get;set;}

        public List<Subscription> Subscriptions {get;set;}

        public List<PersonalPlaylist> PersonalPlaylists {get;set;}
        
    }

    public class PersonalPlaylist
    {
        public string Name {get;set;}

        public List<ListContentItem> Items {get;set;}
    }
}