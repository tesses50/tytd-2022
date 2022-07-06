using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
namespace Tesses.YouTubeDownloader.Tools.Common
{
    public static class SymlinkGenerator
    {
        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
        private static void createHardLink(string destPath,string srcPath)
        {if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateHardLink(destPath,srcPath,IntPtr.Zero);
                }else{
            using(var p=new Process())
            {
                
                p.StartInfo.FileName = "ln";
                p.StartInfo.ArgumentList.Add(srcPath);
                p.StartInfo.ArgumentList.Add(destPath);
                
                if(p.Start()){ p.WaitForExit();}
            }
        }
    }
        public static async Task GenerateHardLinks(TYTDStorage storage,string dest="GoodFileNames",Resolution res=Resolution.PreMuxed,bool verbose=false)
        {
            Directory.CreateDirectory(dest);
                 await foreach(var item in storage.GetVideosAsync())
            {
                if(await item.VideoExistsAsync(storage,res))
                {
                   var (path,delete)= await storage.GetRealUrlOrPathAsync(await BestStreams.GetPathResolution(storage,item,res));
                   string? ext=Path.GetExtension(path);
                  
                     string defaultExt = res == Resolution.Mux ? ".mkv" : ".mp4"; 
                     if(string.IsNullOrWhiteSpace(ext))
                   {
                       ext=defaultExt;
                   }
                      string destPathMkv=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}{defaultExt}");
                   string destPath=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}{ext}");

                   if(File.Exists(destPathMkv) && destPathMkv != destPath)
                   {
                       File.Delete(destPathMkv);
                       createHardLink(destPath,path);
                       if(verbose)
                       Console.WriteLine($"Changed: {item.Title} {defaultExt} -> {ext}");
                   }
                   if(!File.Exists(destPath))
                   {
                       createHardLink(destPath,path);
                       if(verbose)
                       Console.WriteLine(item.Title);
                   }
                }
            }
        
        }
        public static async Task GenerateSymlinks(TYTDBase storage,string dest="GoodFileNames",Resolution res=Resolution.PreMuxed,bool verbose=false)
        {
            Directory.CreateDirectory(dest);
            await foreach(var item in storage.GetVideosAsync())
            {
                if(await item.VideoExistsAsync(storage,res))
                {
                   var (path,delete)= await storage.GetRealUrlOrPathAsync(await BestStreams.GetPathResolution(storage,item,res));
                   string? ext=Path.GetExtension(path);
                  
                     string defaultExt = res == Resolution.Mux ? ".mkv" : ".mp4"; 
                     if(string.IsNullOrWhiteSpace(ext))
                   {
                       ext=defaultExt;
                   }
                      string destPathMkv=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}{defaultExt}");
                   string destPath=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}{ext}");

                   if(File.Exists(destPathMkv) && destPathMkv != destPath)
                   {
                       File.Delete(destPathMkv);
                       File.CreateSymbolicLink(destPath,path);
                       if(verbose)
                       Console.WriteLine($"Changed: {item.Title} {defaultExt} -> {ext}");
                   }
                   if(!File.Exists(destPath))
                   {
                       File.CreateSymbolicLink(destPath,path);
                       if(verbose)
                       Console.WriteLine(item.Title);
                   }
                }
            }
        }
    }
}