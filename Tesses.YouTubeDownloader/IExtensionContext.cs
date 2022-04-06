using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Tesses.YouTubeDownloader
{
    public interface IExtensionContext
    {
        List<IExtension> Extensions {get;}
    }

    public interface IExtension
    {
        void OnEnd();
        string Name {get;}
        Task VideoStarted(SavedVideo video,Resolution resolution,long length);

        Task VideoProgress(SavedVideo video,double progress);
        Task VideoFinished(SavedVideo video,Resolution resolution);
    }
}