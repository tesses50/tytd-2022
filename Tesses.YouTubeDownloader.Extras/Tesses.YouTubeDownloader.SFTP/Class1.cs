using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Renci.SshNet;
using Tesses.YouTubeDownloader;
using System.Collections.Generic;

namespace Tesses.YouTubeDownloader.SFTP
{
   public class SSHFS : TYTDStorage
{
    SftpClient ftp;
    string path;
    public SSHFS(string url,string userName,string passWord)
    {
        Uri uri=new Uri(url);
        int port;
        if(uri.Port == -1)
        {
            port=22;
        }else{
            port=uri.Port;
        }
        path=uri.PathAndQuery;
        ftp=new SftpClient(uri.Host,port,userName,passWord);
        
        ftp.Connect();
        ftp.ChangeDirectory(path);
    }
    private string GetArg(string[] a,int aI)
    {
        if(aI < a.Length)
        {
            return a[aI];
        }
        return "";
    }
    public SSHFS(Uri uri)
    {
        int port;
         if(uri.Port == -1)
        {
            port=22;
        }else{
            port=uri.Port;
        }
        path=uri.PathAndQuery;
        var userPass=uri.UserInfo.Split(new char[]{':'},2,StringSplitOptions.None);
        ftp=new SftpClient(uri.Host,port,GetArg(userPass,0),GetArg(userPass,1));
        ftp.Connect();
        ftp.ChangeDirectory(path);
    }
    
    public  SSHFS(string url,string userName,params PrivateKeyFile[] keys)
    {
        Uri uri=new Uri(url);
        int port;
        if(uri.Port == -1)
        {
            port=22;
        }else{
            port=uri.Port;
        }
        path=uri.PathAndQuery;
        ftp=new SftpClient(uri.Host,port,userName,keys);
        
        ftp.Connect();
        ftp.ChangeDirectory(path);
    }
    public SSHFS(SftpClient client)
    {
        ftp=client;
        path=client.WorkingDirectory;
    }
    public override async Task<Stream> CreateAsync(string path)
    {
        return await Task.FromResult(ftp.Open($"{this.path.TrimEnd('/')}/{path.TrimStart('/')}",FileMode.Create,FileAccess.Write));
    }

    public override void CreateDirectory(string path)
    {
        ftp.CreateDirectory(path);
    }
    
    public override void DeleteDirectory(string dir, bool recursive = false)
    {
        var entries = ftp.ListDirectory(dir).ToArray();
        
        if(!recursive && entries.Length > 0)
        {
            return;
        }
        if(recursive)
        {
            foreach(var entry in entries)
            {
                if(entry.IsDirectory)
                {
                    DeleteDirectory($"{dir.TrimEnd('/')}/{entry.Name}",true);
                }else{
                    entry.Delete();
                }
            }
        }
        ftp.DeleteDirectory(dir); 
    }

    public override void DeleteFile(string file)
    {
        ftp.DeleteFile(path);
    }

    public override async Task<bool> DirectoryExistsAsync(string path)
    {
        bool value=false;
        if(ftp.Exists(path))
        {
            
            if(ftp.Get(path).IsDirectory)
            {
                value=true;
            }
        }
        return await Task.FromResult(value);
    }

    public override async IAsyncEnumerable<string> EnumerateDirectoriesAsync(string path)
    {
        foreach(var item in ftp.ListDirectory(path))
        {
            if(item.IsDirectory)
            {
                yield return await Task.FromResult(item.Name);
            }
        }
    }

    public override async IAsyncEnumerable<string> EnumerateFilesAsync(string path)
    {
        foreach(var item in ftp.ListDirectory(path))
        {
            if(item.IsRegularFile)
            {
                yield return await Task.FromResult(item.Name);
            }

        }
    }

    public override async Task<bool> FileExistsAsync(string path)
    {
        bool value=false;
        if(ftp.Exists(path))
        {
            if(ftp.Get(path).IsRegularFile)
            {
                value=true;
            }
        }
        return await Task.FromResult(value);
    }

    public override void MoveDirectory(string src, string dest)
    {
        ftp.RenameFile(src,dest);
    }

    public override async Task<Stream> OpenOrCreateAsync(string path)
    {
        return await Task.FromResult(ftp.Open($"{this.path.TrimEnd('/')}/{path.TrimStart('/')}",FileMode.OpenOrCreate,FileAccess.Write));
    }

    public override async Task<Stream> OpenReadAsync(string path)
    {
        
        return await Task.FromResult(ftp.Open($"{this.path.TrimEnd('/')}/{path.TrimStart('/')}",FileMode.Open,FileAccess.Read));
    }

    public override void RenameFile(string src, string dest)
    {
        ftp.RenameFile(src,dest);
    }
}

}
