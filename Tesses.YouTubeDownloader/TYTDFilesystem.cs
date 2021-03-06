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
using System.IO;
using System.Globalization;

namespace Tesses.YouTubeDownloader
{
    /// <summary>
    /// Abstracted current directory
    /// </summary>
    public class TYTDCurrentDirectory : TYTDStorage
    {
        public TYTDCurrentDirectory(HttpClient clt) : base(clt)
        {
        }
        public TYTDCurrentDirectory() : base()
        {
        }
        public override Task<(string Path, bool Delete)> GetRealUrlOrPathAsync(string path)
        {
            return Task.FromResult((Path.Combine(Environment.CurrentDirectory,path),false));
        }
        public override async Task<bool> MuxVideosAsync(SavedVideo video, string videoSrc, string audioSrc, string videoDest, IProgress<double> progress = null, CancellationToken token = default)
        {
             if(string.IsNullOrWhiteSpace(FFmpeg))
            {
                return false;
            }
             Func<string,TimeSpan> get_percent=(e)=>{
                if(string.IsNullOrWhiteSpace(e))
                    return TimeSpan.Zero;

                int index=e.IndexOf("time=");
                if(index < 0)
                    return TimeSpan.Zero;
                
                int spaceIndex=e.IndexOf(' ',index+5);
                string j=e.Substring(index+5,spaceIndex-(index+5));
                
                double val;
                if(double.TryParse(j,out val))
                    return TimeSpan.FromSeconds(val);

                return TimeSpan.ParseExact(j, "c", CultureInfo.InvariantCulture);
                
                //return TimeSpan.Zero;
            };
            Directory.CreateDirectory("TYTD_TEMP");
            bool ret;

             ret=await this.Convert(video,videoSrc,audioSrc,videoDest,token,new Progress<string>((e)=>{
                    // time=\"\"
                    if(progress!=null)
                    {
                        var progr=get_percent(e).TotalSeconds / video.Duration.TotalSeconds;
                        progress.Report(progr / 4 + 0.50);
                    }
                }),FFmpeg);

                Directory.Delete("TYTD_TEMP",true);
                return ret;
        }
        public override async Task<Stream> CreateAsync(string path)
        {
            return await Task.FromResult(File.Create(path));
        }

        public override void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
            return await Task.FromResult(Directory.Exists(path));
        }

        public override async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                foreach(var item in Directory.EnumerateDirectories(Environment.CurrentDirectory))
                {
                      yield return await Task.FromResult(Path.GetFileName(item));
                }
            }else{
            foreach(var dir in Directory.EnumerateDirectories(path))
            {
                yield return await Task.FromResult(Path.GetFileName(dir));
            }
            }
        }

        public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
             if(string.IsNullOrWhiteSpace(path))
            {
                foreach(var item in Directory.EnumerateFiles(Environment.CurrentDirectory))
                {
                      yield return await Task.FromResult(Path.GetFileName(item));
                }
            }else{
             foreach(var file in Directory.EnumerateFiles(path))
            {
                yield return await Task.FromResult(Path.GetFileName(file));
            }
            }
        }

        public override async Task<bool> FileExistsAsync(string path)
        {
            return await Task.FromResult(File.Exists(path));
        }

        public override async Task<Stream> OpenOrCreateAsync(string path)
        {
            if(File.Exists(path))
            {
                return await Task.FromResult(File.OpenWrite(path));
            }
            return await CreateAsync(path);
        }

        public override async Task<Stream> OpenReadAsync(string path)
        {
            return await Task.FromResult(File.OpenRead(path));
        }

        public override void RenameFile(string src, string dest)
        {
            File.Move(src,dest);
        }

        public override void MoveDirectory(string src, string dest)
        {
           Directory.Move(src,dest);
        }

        public override void DeleteFile(string file)
        {
            File.Delete(file);
        }

        public override void DeleteDirectory(string dir, bool recursive = false)
        {
            Directory.Delete(dir,recursive);
        }
    }
    /// <summary>
    /// Abstracted directory from path
    /// </summary>
    public class TYTDPathDirectory : TYTDStorage
    {
        public TYTDPathDirectory(string path,HttpClient clt) : base(clt)
        {
            _path=path;
        }
        public TYTDPathDirectory(string path) : base()
        {
            _path=path;
        }
        string _path;
        public string GetPath(string internalPath)
        {
           return Path.Combine(_path,internalPath.TrimStart('/'));
        }
        public override Task<(string Path, bool Delete)> GetRealUrlOrPathAsync(string path)
        {
            return Task.FromResult((GetPath(path),false));
        }
        public override async Task<bool> MuxVideosAsync(SavedVideo video, string videoSrc, string audioSrc, string videoDest, IProgress<double> progress = null, CancellationToken token = default)
        {
             if(string.IsNullOrWhiteSpace(FFmpeg))
            {
                return false;
            }
             Func<string,TimeSpan> get_percent=(e)=>{
                if(string.IsNullOrWhiteSpace(e))
                    return TimeSpan.Zero;

                int index=e.IndexOf("time=");
                if(index < 0)
                    return TimeSpan.Zero;
                
                int spaceIndex=e.IndexOf(' ',index+5);
                string j=e.Substring(index+5,spaceIndex-(index+5));
                
                double val;
                if(double.TryParse(j,out val))
                    return TimeSpan.FromSeconds(val);

                return TimeSpan.ParseExact(j, "c", CultureInfo.InvariantCulture);
                
                //return TimeSpan.Zero;
            };
            Directory.CreateDirectory(GetPath("TYTD_TEMP"));
            bool ret;

             ret=await this.Convert(video,GetPath(videoSrc),GetPath(audioSrc),GetPath(videoDest),token,new Progress<string>((e)=>{
                    // time=\"\"
                    if(progress!=null)
                    {
                        var progr=get_percent(e).TotalSeconds / video.Duration.TotalSeconds;
                        progress.Report(progr / 4 + 0.50);
                    }
                }),FFmpeg);

                Directory.Delete(GetPath("TYTD_TEMP"),true);
                return ret;
        }
        public override async Task<Stream> CreateAsync(string path)
        {
            return await Task.FromResult(File.Create(GetPath(path)));
        }

        public override void CreateDirectory(string path)
        {
            Directory.CreateDirectory(GetPath(path));
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
            return await Task.FromResult(Directory.Exists(GetPath(path)));
        }

        public override async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
             if(string.IsNullOrWhiteSpace(path))
            {
                foreach(var item in Directory.EnumerateDirectories(_path))
                {
                      yield return await Task.FromResult(Path.GetFileName(item));
                }
            }else{
            foreach(var dir in Directory.EnumerateDirectories(GetPath(path)))
            {
                yield return await Task.FromResult(Path.GetFileName(dir));
            }
            }
        }

        public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
             if(string.IsNullOrWhiteSpace(path))
            {
                foreach(var item in Directory.EnumerateFiles(_path))
                {
                      yield return await Task.FromResult(Path.GetFileName(item));
                }
            }else{
             foreach(var file in Directory.EnumerateFiles(GetPath(path)))
            {
                yield return await Task.FromResult(Path.GetFileName(file));
            }
            }
        }

        public override async Task<bool> FileExistsAsync(string path)
        {
            return await Task.FromResult(File.Exists(GetPath(path)));
        }

        public override async Task<Stream> OpenOrCreateAsync(string path)
        {
            if(File.Exists(path))
            {
                return await Task.FromResult(File.OpenWrite(GetPath(path)));
            }
            return await CreateAsync(path);
        }

        public override async Task<Stream> OpenReadAsync(string path)
        {
            return await Task.FromResult(File.OpenRead(GetPath(path)));
        }

        public override void RenameFile(string src, string dest)
        {
            File.Move(GetPath(src),GetPath(dest));
        }

        public override void MoveDirectory(string src, string dest)
        {
           Directory.Move(GetPath(src),GetPath(dest));
        }

        public override void DeleteFile(string file)
        {
            File.Delete(GetPath(file));
        }

        public override void DeleteDirectory(string dir, bool recursive = false)
        {
            Directory.Delete(GetPath(dir),recursive);
        }
    }
}
