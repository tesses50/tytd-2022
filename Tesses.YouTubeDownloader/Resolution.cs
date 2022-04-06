using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Tesses.YouTubeDownloader
{
    public enum Resolution
    {
        NoDownload=-1,
        Mux=0,
        PreMuxed=1,
        AudioOnly=2,

        VideoOnly=3
    }
}