using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DiscUtils;
using Tesses.YouTubeDownloader;
namespace Tesses.YouTubeDownloader.DiscUtils
{
    public class DiscUtilsFileSystem : TYTDStorage
    {
        IFileSystem fileSystem;
        string parent_path;
        public DiscUtilsFileSystem(IFileSystem fs) : base()
        {
            fileSystem=fs;
            parent_path="\\";
        }

        public DiscUtilsFileSystem(IFileSystem fileSystem,string path)
        {
            parent_path=path.Replace('/','\\');
        }

        public DiscUtilsFileSystem(IFileSystem fs,HttpClient clt) : base(clt)
        {
            fileSystem=fs;
            parent_path="\\";
        }
        public DiscUtilsFileSystem(IFileSystem fs,HttpClient clt,string path) : base(clt)
        {
            fileSystem=fs;
            parent_path=path.Replace('/','\\');
        }

        public override async Task<Stream> CreateAsync(string path)
        {
            return await Task.FromResult(fileSystem.OpenFile(ConvertToDiscUtils(path),FileMode.Create,FileAccess.Write));
        }

        public override void CreateDirectory(string path)
        {
            fileSystem.CreateDirectory(ConvertToDiscUtils(path));
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
           return await Task.FromResult( fileSystem.DirectoryExists(ConvertToDiscUtils(path)));
        }

        public override async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
            foreach(var item in fileSystem.GetDirectories(path))
            {
                yield return await Task.FromResult(GetFileName(item));
            }
        }

        public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
             foreach(var item in fileSystem.GetFiles(path))
            {
                yield return await Task.FromResult(GetFileName(item));
            }
        }

        public override async Task<bool> FileExistsAsync(string path)
        {
             return await Task.FromResult( fileSystem.FileExists(ConvertToDiscUtils(path)));
        }

        public override async Task<Stream> OpenOrCreateAsync(string path)
        {
            return await Task.FromResult(fileSystem.OpenFile(ConvertToDiscUtils(path),FileMode.OpenOrCreate,FileAccess.Write));
        }

        public override async Task<Stream> OpenReadAsync(string path)
        {
            return await Task.FromResult(fileSystem.OpenFile(ConvertToDiscUtils(path),FileMode.Open,FileAccess.Read));
        }

        public override void RenameFile(string src, string dest)
        {
            fileSystem.MoveFile(ConvertToDiscUtils(src),ConvertToDiscUtils(dest));
        }
        public override async Task<long> GetLengthAsync(string path)
        {
           return await Task.FromResult(fileSystem.GetFileLength(ConvertToDiscUtils(path)));
        }

        private string ConvertToDiscUtils(string path)
        {
            return parent_path.TrimEnd('\\') + '\\' + path.Replace('/','\\');
        }
        private string GetFileName(string path)
        {
            return Path.GetFileName(path.Replace('\\',Path.DirectorySeparatorChar));
        }

        public override void MoveDirectory(string src, string dest)
        {
            fileSystem.MoveDirectory(ConvertToDiscUtils(src),ConvertToDiscUtils(dest));
        }

        public override void DeleteFile(string file)
        {
            fileSystem.DeleteFile(ConvertToDiscUtils(file));
        }

        public override void DeleteDirectory(string dir, bool recursive = false)
        {
            fileSystem.DeleteDirectory(dir,recursive);
        }
    }
}
