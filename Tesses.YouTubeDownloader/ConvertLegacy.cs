using Newtonsoft.Json;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using System.Threading;
using YoutubeExplode.Exceptions;
using System.Collections.Generic;

namespace Tesses.YouTubeDownloader
{
   public abstract partial class TYTDStorage
   {
       public LegacyConverter Legacy {get{return new LegacyConverter(this);}}
   }

   public class LegacyConverter
   {
       TYTDStorage storage1;
       internal LegacyConverter(TYTDStorage storage)
       {
           storage1=storage;
       }

       private async Task MoveThumbnailsFromLegacy(string id)
       {
                          /*
                    640x480= sddefault.jpg
                    120x90= default.jpg
                    320x180 = mqdefault.jpg
                    480x360= hqdefault.jpg
                    1920x1080=maxresdefault.jpg
            
               */
               storage1.CreateDirectoryIfNotExist($"Thumbnails/{id}");
               if(await storage1.FileExistsAsync($"Thumbnails/Legacy/640x480/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/sddefault.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/640x480/{id}.jpg",$"Thumbnails/{id}/sddefault.jpg");
               }
               if(await storage1.FileExistsAsync($"Thumbnails/Legacy/120x90/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/default.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/120x90/{id}.jpg",$"Thumbnails/{id}/default.jpg");
               }
               if(await storage1.FileExistsAsync($"Thumbnails/Legacy/320x180/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/mqdefault.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/320x180/{id}.jpg",$"Thumbnails/{id}/mqdefault.jpg");
               }
               if(await storage1.FileExistsAsync($"Thumbnails/Legacy/480x360/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/hqdefault.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/480x360/{id}.jpg",$"Thumbnails/{id}/hqdefault.jpg");
               }
               if(await storage1.FileExistsAsync($"Thumbnails/Legacy/1920x1080/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/maxresdefault.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/1920x1080/{id}.jpg",$"Thumbnails/{id}/maxresdefault.jpg");
               }else  if(await storage1.FileExistsAsync($"Thumbnails/Legacy/1280x720/{id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{id}/maxresdefault.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/1280x720/{id}.jpg",$"Thumbnails/{id}/maxresdefault.jpg");
               }

       }
        
       public async Task ConvertFromLegacy(IProgress<string> w =null,bool eraseEmptyDirs=false)
       {
           DateTime time=DateTime.Now;
           w.Print("TYTD Legacy -> TYTD Modern");
           w.Print("Version Conversion is starting");
           storage1.CreateDirectoryIfNotExist("NotConverted");
           storage1.CreateDirectoryIfNotExist("Converted");
           storage1.CreateDirectoryIfNotExist("Channel");
           storage1.CreateDirectoryIfNotExist("Audio");
            storage1.CreateDirectoryIfNotExist("VideoOnly");
               storage1.CreateDirectoryIfNotExist("AudioOnly");
               storage1.CreateDirectoryIfNotExist("PreMuxed");
               storage1.CreateDirectoryIfNotExist("Muxed");
                storage1.CreateDirectoryIfNotExist("Info");
                 storage1.CreateDirectoryIfNotExist("Thumbnails");
                 storage1.MoveDirectory("Thumbnails","_Thumbnails");
                 storage1.CreateDirectoryIfNotExist("Thumbnails");
                 storage1.MoveDirectory("_Thumbnails","Thumbnails/Legacy");
           List<SavedVideo> videos=new List<SavedVideo>();
           await foreach(var v in storage1.GetLegacyVideosAsync())
           {
               w.Print($"{v.Title}");
               
               // NotConverted/videoId.mp4
              videos.Add(v.ToSavedVideo());
              

               if(await storage1.FileExistsAsync($"NotConverted/{v.Id}.mp4"))
               {
                   if(!await storage1.FileExistsAsync($"PreMuxed/{v.Id}.mp4"))
                   {
                       storage1.RenameFile($"NotConverted/{v.Id}.mp4",$"PreMuxed/{v.Id}.mp4");
                   }
               }
                if(await storage1.FileExistsAsync($"Audio/{v.Id}.mp4"))
               {
                   if(!await storage1.FileExistsAsync($"AudioOnly/{v.Id}.mp4"))
                   {
                       storage1.RenameFile($"Audio/{v.Id}.mp4",$"AudioOnly/{v.Id}.mp4");
                   }
               }
                if(await storage1.FileExistsAsync($"Converted/{v.Id}.mp4"))
               {
                   if(!await storage1.FileExistsAsync($"Muxed/{v.Id}.mkv"))
                   {
                       storage1.RenameFile($"Converted/{v.Id}.mp4",$"Muxed/{v.Id}.mkv");
                   }
               }
                if(await storage1.FileExistsAsync($"Converted/{v.Id}-vidonly.bkp"))
               {
                   if(!await storage1.FileExistsAsync($"VideoOnly/{v.Id}.mp4"))
                   {
                       storage1.RenameFile($"Converted/{v.Id}-vidonly.bkp",$"VideoOnly/{v.Id}.mp4");
                   }
               }
               

               await MoveThumbnailsFromLegacy(v.Id);


               
           }
            await foreach(var c in storage1.GetChannelsAsync())
            {
                if(await storage1.FileExistsAsync($"Thumbnails/Legacy/900x900/{c.Id}.jpg") && !await storage1.FileExistsAsync($"Thumbnails/{c.Id}/900x900.jpg"))
               {
                   storage1.RenameFile($"Thumbnails/Legacy/900x900/{c.Id}.jpg",$"Thumbnails/{c.Id}/900x900.jpg");
               }
            }

           int count = videos.Count;
          foreach(var v in videos)
          {
              await storage1.WriteVideoInfoAsync(v);
                      
          }
          
           TimeSpan span = DateTime.Now - time;

           w.Print($"Version Conversion Has Finished with {count} Videos in {span.ToString()}");
       }
   }
}