using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zio.FileSystems;
using Zio;
using System.Net.Http;

namespace Tesses.YouTubeDownloader.Zio
{
    public class ZioStorage : TYTDStorage
    {
        IFileSystem fileSystem;
        public ZioStorage(IFileSystem system,HttpClient clt) : base(clt)
        {
            fileSystem=system;
        }
        public ZioStorage(IFileSystem system) : base()
        {
            fileSystem =system;
        }
        public override async Task<Stream> CreateAsync(string path)
        {
            return await Task.FromResult( fileSystem.CreateFile(MakeAbsolute(path)));
        }

        public override void CreateDirectory(string path)
        {
            fileSystem.CreateDirectory(MakeAbsolute(path));
        }

        public override async Task<bool> DirectoryExistsAsync(string path)
        {
            return await Task.FromResult(fileSystem.DirectoryExists(MakeAbsolute(path)));
        }

        public override async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
        {
            foreach(var items in fileSystem.EnumerateDirectories(MakeAbsolute(path)))
            {
                yield return await Task.FromResult(items.GetName());
            }
        }

        public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
        {
            
            foreach(var items in fileSystem.EnumerateFiles(MakeAbsolute(path)))
            {
                yield return await Task.FromResult(items.GetName());
            }
        }

        public override async Task<bool> FileExistsAsync(string path)
        {
            return await Task.FromResult(fileSystem.FileExists(MakeAbsolute(path)));
        }

        public override async Task<Stream> OpenOrCreateAsync(string path)
        {
            return await Task.FromResult( fileSystem.OpenFile(MakeAbsolute(path),FileMode.OpenOrCreate,FileAccess.Write,FileShare.None));
        }
        private UPath MakeAbsolute(string j)
        {
            return UPath.Root + j;
        }
        public override async Task<Stream> OpenReadAsync(string path)
        {
            return await Task.FromResult(fileSystem.OpenFile(MakeAbsolute(path),FileMode.Open,FileAccess.Read));
        }

        public override void RenameFile(string src, string dest)
        {
            fileSystem.MoveFile(MakeAbsolute(src),MakeAbsolute(dest));
        }

        public override void MoveDirectory(string src, string dest)
        {
            fileSystem.MoveDirectory(MakeAbsolute(src),MakeAbsolute(dest));
        }

        public override void DeleteFile(string file)
        {
            fileSystem.DeleteFile(MakeAbsolute(file));
        }

        public override void DeleteDirectory(string dir, bool recursive = false)
        {
            fileSystem.DeleteDirectory(MakeAbsolute(dir),recursive);
        }
    }
}
