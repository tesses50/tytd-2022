using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using YoutubeExplode.Playlists;
using YoutubeExplode.Channels;
using Newtonsoft.Json;

namespace Tesses.YouTubeDownloader
{
    public interface ITYTDBase : IPersonalPlaylistGet
    {
        
        IAsyncEnumerable<string> GetDownloadUrlsAsync();
        IAsyncEnumerable<SavedVideo> GetDownloadsAsync();
        Task<SavedVideo> GetDownloadInfoAsync(string url);
        bool DownloadExists(string path);
        Task<BestStreamInfo.BestStreamsSerialized> GetBestStreamInfoAsync(VideoId id);
        bool BestStreamInfoExists(VideoId id);
        IAsyncEnumerable<string> GetPersonalPlaylistsAsync();
        Task<(String Path,bool Delete)> GetRealUrlOrPathAsync(string path);       
        
        bool FileExists(string path);
        IAsyncEnumerable<string> GetVideoIdsAsync();
        Task<SavedVideo> GetVideoInfoAsync(VideoId id);
        IAsyncEnumerable<SavedVideo> GetVideosAsync();
       
        IAsyncEnumerable<SavedVideoLegacy> GetLegacyVideosAsync();
        Task<SavedVideoLegacy> GetLegacyVideoInfoAsync(VideoId id);
         IAsyncEnumerable<SavedPlaylist> GetPlaylistsAsync();
        

       Task<byte[]> ReadAllBytesAsync(string path,CancellationToken token=default(CancellationToken));
    
       IAsyncEnumerable<string> GetPlaylistIdsAsync();
        IAsyncEnumerable<string> GetChannelIdsAsync();
       IAsyncEnumerable<VideoId> GetYouTubeExplodeVideoIdsAsync();
        Task<SavedChannel> GetChannelInfoAsync(ChannelId id);
        IAsyncEnumerable<SavedChannel> GetChannelsAsync();

        bool PlaylistInfoExists(PlaylistId id);
        bool VideoInfoExists(VideoId id);
        bool ChannelInfoExists(ChannelId id);
        Task<SavedPlaylist> GetPlaylistInfoAsync(PlaylistId id);
        Task<string> ReadAllTextAsync(string file);
        bool DirectoryExists(string path);

        IEnumerable<string> EnumerateFiles(string path);
        IEnumerable<string> EnumerateDirectories(string path);
        
        Task<Stream> OpenReadAsync(string path);

        Task<bool> FileExistsAsync(string path);

        Task<bool> DirectoryExistsAsync(string path);

        IAsyncEnumerable<string> EnumerateFilesAsync(string path);

        IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path);

        
    }
}
