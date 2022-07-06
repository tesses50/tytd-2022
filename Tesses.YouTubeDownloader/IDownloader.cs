using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
namespace Tesses.YouTubeDownloader
{
    public interface IDownloader : IPersonalPlaylistSet
    {
        Task AddVideoAsync(VideoId id,Resolution resolution=Resolution.PreMuxed);
        Task AddPlaylistAsync(PlaylistId id,Resolution resolution=Resolution.PreMuxed);
        Task AddChannelAsync(ChannelId id,Resolution resolution=Resolution.PreMuxed);

        Task AddUserAsync(UserName userName,Resolution resolution=Resolution.PreMuxed);
        Task AddFileAsync(string url,bool download=true);
        IReadOnlyList<(SavedVideo Video,Resolution Resolution)> GetQueueList();
        SavedVideoProgress GetProgress();
        IAsyncEnumerable<Subscription> GetSubscriptionsAsync();
        Task UnsubscribeAsync(ChannelId id);
        Task SubscribeAsync(ChannelId id,bool downloadChannelInfo=false,ChannelBellInfo bellInfo = ChannelBellInfo.NotifyAndDownload);
        Task SubscribeAsync(UserName name,ChannelBellInfo info=ChannelBellInfo.NotifyAndDownload);
        Task ResubscribeAsync(ChannelId id,ChannelBellInfo info=ChannelBellInfo.NotifyAndDownload);
        void DeletePersonalPlaylist(string name);

    }
}