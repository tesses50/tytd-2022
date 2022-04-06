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
    public interface IDownloader
    {
        Task AddVideoAsync(VideoId id,Resolution resolution=Resolution.PreMuxed);
        Task AddPlaylistAsync(PlaylistId id,Resolution resolution=Resolution.PreMuxed);
        Task AddChannelAsync(ChannelId id,Resolution resolution=Resolution.PreMuxed);

        Task AddUserAsync(UserName userName,Resolution resolution=Resolution.PreMuxed);

        IReadOnlyList<(SavedVideo Video,Resolution Resolution)> GetQueueList();
        SavedVideoProgress GetProgress();
    }
}