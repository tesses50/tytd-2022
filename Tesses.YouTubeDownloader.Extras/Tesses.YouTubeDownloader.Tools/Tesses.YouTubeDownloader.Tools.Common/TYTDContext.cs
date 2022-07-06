using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using Tesses.YouTubeDownloader.SFTP;
namespace Tesses.YouTubeDownloader.Tools.Common
{
    public static class TYTDOpener
    {
        public static TYTDBase? GetTYTDBase(string p)
        {
            Uri? uri;
            
            if(Uri.TryCreate(p,UriKind.Absolute,out uri))
            {
                if(uri.IsFile)
                {
                    return new TYTDPathDirectory(uri.LocalPath);
                }
                if(uri.Scheme == "sftp")
                {
                    return new SSHFS(uri);
                }
                if(uri.Scheme == "http" || uri.Scheme == "https")
                {
                    return new TYTDClient(uri);
                }
            }else{
                if(!string.IsNullOrWhiteSpace(p)) return new TYTDPathDirectory(p);
            }
            return null;
        }

    }
}