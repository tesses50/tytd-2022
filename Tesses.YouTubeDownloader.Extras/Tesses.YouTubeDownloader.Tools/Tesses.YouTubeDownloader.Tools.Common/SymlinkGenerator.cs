using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using YoutubeExplode.Videos;
using Newtonsoft.Json;

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
                p.StartInfo.UseShellExecute=false;
                p.StartInfo.WorkingDirectory="/";
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
                try{
                var v=await BestStreams.GetPathResolution(storage,item,res);
                if(storage.FileExists(v))
                {
                    
                   var (path,delete)= await storage.GetRealUrlOrPathAsync(v);
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
            }catch(Exception ex)
            {
                Console.WriteLine($"ERROR: {item.Id}");
                Console.WriteLine(ex.ToString());
            }
            }
        
        }
        public static async Task<bool> VideoExistsAsync(ITYTDBase b,SavedVideo id)
        {
            var res=await BestStreams.GetPathResolution(b,id,Resolution.PreMuxed);
            if(b.FileExists(res)) return true;
            await BestStreams.GetPathResolution(b,id,Resolution.Mux);
            if(b.FileExists(res)) return true;
            await BestStreams.GetPathResolution(b,id,Resolution.VideoOnly);
            if(b.FileExists(res)) return true;
            await BestStreams.GetPathResolution(b,id,Resolution.AudioOnly);
            if(b.FileExists(res)) return true;

            return false;
        }
        public static async Task GenerateMeta(List<string> items,TYTDStorage storage,string dest="GoodInfos",bool verbose=false)
        {
            items.Add("Videos");
            if(verbose)
            {
                Console.WriteLine("[Generating Meta]");
            }
             Directory.CreateDirectory(dest);
            await foreach(var item in storage.GetVideosAsync())
            {
                if(await VideoExistsAsync(storage,item))
                {
                     string destPathJson=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}.json");
                     string destPathText=Path.Combine(dest,$"{item.Title.GetSafeFileName()}-{item.Id}.txt");
                     if(!File.Exists(destPathJson))
                     File.WriteAllText(destPathJson,JsonConvert.SerializeObject(item,Formatting.Indented));
                     if(!File.Exists(destPathText)){                     using(var f=new StreamWriter(destPathText))
                     {
                        f.WriteLine($"Title: {item.Title}");
                        f.WriteLine($"Id: {item.Id}");
                        f.WriteLine($"AuthorTitle: {item.AuthorTitle}");
                        f.WriteLine($"AuthorChannelId: {item.AuthorChannelId}");
                         f.WriteLine($"Tags: {string.Join(", ",item.Keywords)}");
                      
                         f.WriteLine($"Views: {item.Views}");
                        f.WriteLine($"Likes: {item.Likes}");
                        f.WriteLine($"Dislikes: {item.Dislikes}");
                       
                        f.WriteLine($"Duration: {item.Duration.ToString()}");
                         f.WriteLine($"Upload Date: {item.UploadDate}");
                          f.WriteLine($"Add Date: {item.AddDate.ToShortDateString()}");
                      
                          f.WriteLine($"DownloadFrom: {item.DownloadFrom}");
                        f.WriteLine($"Legacy Video: {item.LegacyVideo}");
                         f.WriteLine($"Downloader Tag: {item.TYTDTag}");
                        f.WriteLine($"Video Frozen: {item.VideoFrozen}");
                       
                       f.WriteLine("Description:");
                        f.WriteLine($"{item.Description}");
                     }}
                    items.Add($"{item.Title}-{item.Id}");
                    if(verbose)
                    {
                        Console.WriteLine($"[META] {item.Title}");
                    }
                }
            }
        }
        public static async Task GenerateSymlinks(TYTDBase storage,string dest="GoodFileNames",Resolution res=Resolution.PreMuxed,bool verbose=false)
        {
            Directory.CreateDirectory(dest);
            await foreach(var item in storage.GetVideosAsync())
            {
               var v=await BestStreams.GetPathResolution(storage,item,res);
                if(storage.FileExists(v))
                {
                   var (path,delete)= await storage.GetRealUrlOrPathAsync(v);
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