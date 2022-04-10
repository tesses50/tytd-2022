using System;
using YoutubeExplode;
using YoutubeExplode.Videos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using YoutubeExplode.Videos.Streams;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

namespace Tesses.YouTubeDownloader
{
    public abstract partial class TYTDStorage
    {
        
        private async Task DownloadLoop(CancellationToken token = default(CancellationToken))
        {
            while (!token.IsCancellationRequested)
            {
                bool hasAny;
                var (Video, Resolution) = Dequeue(out hasAny);
                if (hasAny)
                {
                    await DownloadVideoAsync(Video, Resolution, token,new Progress<double>(ReportProgress),true);
                }

            }
        }

        public readonly SavedVideoProgress Progress = new SavedVideoProgress();
        private void ReportProgress(double progress)
        {
            Progress.Progress = (int)(progress * 100);
            Progress.ProgressRaw = progress;

            if (ExtensionContext != null)
            {
                foreach (var ext in ExtensionContext.Extensions)
                {
                    ext.VideoProgress(Progress.Video, progress);
                }
            }
        }
        private void ReportStartVideo(SavedVideo video, Resolution resolution, long length)
        {
            Progress.Video = video;
            Progress.Progress = 0;
            Progress.ProgressRaw = 0;
            Progress.Length = length;

            if (ExtensionContext != null)
            {
                foreach (var item in ExtensionContext.Extensions)
                {
                    item.VideoStarted(video, resolution, length);
                }
            }
        }
        private void ReportEndVideo(SavedVideo video, Resolution resolution)
        {
            Progress.Progress = 100;
            Progress.ProgressRaw = 1;


            if (ExtensionContext != null)
            {
                foreach (var item in ExtensionContext.Extensions)
                {
                    item.VideoFinished(video, resolution);
                }
            }
        }
        public async Task DownloadNoQueue(SavedVideo info,Resolution resolution=Resolution.Mux,CancellationToken token=default(CancellationToken),IProgress<double> progress=null)
        {
          
                await DownloadVideoAsync(info,resolution,token,progress,false);
            
        }

        public async Task<SavedVideo> GetSavedVideoAsync(VideoId id)
        {
            VideoMediaContext context=new VideoMediaContext(id,Resolution.PreMuxed);
            List<(SavedVideo Video,Resolution)> s=new List<(SavedVideo Video, Resolution)>();
            await context.FillQueue(this,s);
            if(s.Count> 0)
            {
                return s.First().Video;
            }
            return null;
        }

        
        private async Task DownloadVideoAsync(SavedVideo video, Resolution resolution, CancellationToken token=default(CancellationToken),IProgress<double> progress=null,bool report=true)
        {
            switch (resolution)
            {
                case Resolution.Mux:
                    await DownloadVideoMuxedAsync(video,token,progress,report);
                    break;
                case Resolution.PreMuxed:
                    await DownloadPreMuxedVideoAsync(video, token,progress,report);
                    break;
                case Resolution.AudioOnly:
                    await DownloadAudioOnlyAsync(video,token,progress,report);
                    break;
                case Resolution.VideoOnly:
                    await DownloadVideoOnlyAsync(video,token,progress,report);
                    break;
            }

        }
        public async Task<bool> Continue(string path)
        {
   
            if (await FileExistsAsync(path))
            {
                return (await GetLengthAsync(path) == 0);
            }
            return true;
        }
         private static async Task<bool> run_process(string filename,CancellationToken token,params string[] args)
        {
            return await run_process(filename,null,token,args);
        }
        private static async Task<bool> run_process(string filename,IProgress<string> new_line,CancellationToken token, params string[] args)
        {
                using(Process process=new Process()){
                process.StartInfo.FileName = filename;
                StringBuilder builder=new StringBuilder();
                foreach(var arg in args)
                {
                    builder.Append($"\"{arg}\" ");
                }
                process.StartInfo.Arguments=builder.ToString();
                if(new_line !=null)
                {
                    process.StartInfo.UseShellExecute=false;
                    process.StartInfo.RedirectStandardError=true;
                    process.StartInfo.RedirectStandardOutput=true;
                
                }
                try{
                if(process.Start())
                {
                    if(new_line != null)
                    {
                        while(!process.HasExited)
                        {
                            if(token.IsCancellationRequested)
                            {
                                process.Kill();
                            }
                            new_line.Report(await process.StandardError.ReadLineAsync());
                        }
                    }else{
                        while(!process.HasExited)
                        {
                            if(token.IsCancellationRequested)
                            {
                                process.Kill();
                            }
                            await Task.Delay(100);
                        }
                    }
                }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _=ex;
                    return false;
                }
                return true;
                }
        }
        
        protected async Task<bool> Convert(SavedVideo video,string videoFile,string audioFile,string outFile,CancellationToken token,IProgress<string> new_line,string ffmpeg="ffmpeg")
        {
            
            Func<string,string> escape_ffmetadata_str = (e)=>{
                StringBuilder builder=new StringBuilder(e);
                builder.Replace("\r\n","\n");
                foreach(var item in new char[] {'\\','#',';','=','\n'})
                {
                    builder.Replace(item.ToString(), $"\\{item}");
                }
                return builder.ToString();
            };
            Action<List<string>,Chapter> add_chapter = (list,chapter)=>{
                list.Add("[CHAPTER]");
                list.Add("TIMEBASE=1/1");
                list.Add($"START={(int)chapter.Offset.TotalSeconds}");
                list.Add($"END={(int)(chapter.Offset.Add(chapter.Length).TotalSeconds)}");
                list.Add($"TITLE={escape_ffmetadata_str(chapter.ChapterName)}");
                list.Add("");
            };
            string txtFile=Path.Combine("TYTD_TEMP","video_info.txt");
           
            DeleteIfExists (txtFile);
            if(! await run_process(ffmpeg,token,"-y","-i",videoFile,"-f","ffmetadata",txtFile))
            {
                        return false;
            }
                List<string> entries=File.ReadAllLines(txtFile).ToList();
                if(token.IsCancellationRequested)
                    {
                        return false;
                    }
                entries.Add($"title={escape_ffmetadata_str(video.Title)}");
                    entries.Add($"artist={escape_ffmetadata_str(video.AuthorTitle)}");
                    entries.Add($"description={escape_ffmetadata_str(video.Description)}");
                    entries.Add($"comments=videoId\\={video.Id},likes\\={video.Likes},views\\={video.Views},authorChannelId\\={video.AuthorChannelId}");
                    entries.Add("");
                    foreach(var chapter in video.GetChapters())
                    {
                    //    Console.WriteLine(chapter.ChapterName);
                        add_chapter(entries,chapter);
                    }
                    File.WriteAllLines(txtFile,entries);
                 return await run_process(ffmpeg,new_line,token,"-y","-i",videoFile,"-i",txtFile,"-i",audioFile,"-map_metadata","1","-c","copy","-map","0:v","-map","2:a",outFile);
                 

                  


        }
        private async Task<bool> CopyStreamAsync(Stream src,Stream dest,long pos=0,long len=0,int bufferSize=4096,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
        {
            if(pos > 0)
            {
                src.Position=pos;
                dest.Position=pos;
            }
            double curPos = pos;
            int read;
            byte[] buffer=new byte[bufferSize];
            do{
                read=await src.ReadAsync(buffer,0,buffer.Length,token);
                if(token.IsCancellationRequested)
                {
                    return false;
                }
                curPos+=read;
                await dest.WriteAsync(buffer,0,read);
                if(progress != null)
                {
                    progress.Report(curPos / len);
                }
            }while(read>0 && !token.IsCancellationRequested);
            if(token.IsCancellationRequested)
            {
                return false;
            }
            return true;
        }
        public abstract Task<Stream> OpenOrCreateAsync(string path);
        public abstract void RenameFile(string src,string dest);
        public static string FFmpeg {get;set;}

        public virtual async Task<bool> MuxVideosAsync(SavedVideo video,string videoSrc,string audioSrc,string videoDest,IProgress<double> progress=null,CancellationToken token=default(CancellationToken))
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
           
            bool ret=false;
             string video_bin=Path.Combine("TYTD_TEMP","video.bin");
             string audio_bin=Path.Combine("TYTD_TEMP","audio.bin");
              string output_mkv=Path.Combine("TYTD_TEMP","output.mkv");
           
                Directory.CreateDirectory("TYTD_TEMP");
                //hince we are in a unknown environment we need to copy video
                DeleteIfExists(video_bin);
                DeleteIfExists(audio_bin);
                DeleteIfExists(output_mkv);
                long len=await GetLengthAsync(videoSrc);

                using(var vstrm_src=await OpenReadAsync(videoSrc))
                {
                    using(var vstrm_dest = File.Create(video_bin))
                    {
                        Console.WriteLine("Opening vstream");
                        if(!await CopyStreamAsync(vstrm_src,vstrm_dest,0,len,4096,
                            new Progress<double>((e)=>{
                                 if(progress !=null)
                                {
                                    progress.Report(e/4);
                                }
                            })
                            ,token
                        ))
                        {
                            
                            goto end;
                        }
                    }
                }
                 using(var astrm_src=await OpenReadAsync(audioSrc))
                {
                    using(var astrm_dest = File.Create(audio_bin))
                    {
                        Console.WriteLine("opening astream");
                        if(!await CopyStreamAsync(astrm_src,astrm_dest,0,len,4096,
                            new Progress<double>((e)=>{
                                if(progress !=null)
                                {
                                progress.Report(e/4+0.25);
                                }
                            })
                            ,token
                        ))
                        {
                            goto end;
                        }
                    }
                }
                 string videoFile=Path.Combine("TYTD_TEMP","video.bin");
            string audioFile=Path.Combine("TYTD_TEMP","audio.bin");
            string outFile = Path.Combine("TYTD_TEMP","output.mkv");
                ret=await Convert(video,videoFile,audioFile,outFile,token,new Progress<string>((e)=>{
                    // time=\"\"
                    if(progress!=null)
                    {
                        var progr=get_percent(e).TotalSeconds / video.Duration.TotalSeconds;
                        progress.Report(progr / 4 + 0.50);
                    }
                }),FFmpeg);
                if(ret)
                {
                    using(var mstrm_src=File.OpenRead(output_mkv))
                    {
                        using(var mstrm_dest=await CreateAsync(videoDest))
                        {
                            ret=await CopyStreamAsync(mstrm_src,mstrm_dest,0,mstrm_src.Length,4096,new Progress<double>((e)=>{
                                if(progress!=null)
                                {
                                    progress.Report(e / 4 + 0.75);
                                }
                            }),token);
                            
                        }
                    }             
                }
                end:
                Directory.Delete("TYTD_TEMP",true);
                return ret;
        }
        private async Task DownloadVideoMuxedAsync(SavedVideo video,CancellationToken token,IProgress<double> progress,bool report=true)
        {
            bool isValid=true;
            isValid=await DownloadVideoOnlyAsync(video,token,progress,report);
            if(token.IsCancellationRequested || !isValid)
            {
                return;
            }
            isValid = await DownloadAudioOnlyAsync(video,token,progress,report);
            if(token.IsCancellationRequested || !isValid)
            {
                return;
            }
             var streams=await BestStreamInfo.GetBestStreams(this,video.Id,token,false);
             if(token.IsCancellationRequested)
             {
                 return;
             }
             if(report)
            ReportStartVideo(video,Resolution.Mux,0);
             string complete = $"Muxed/{video.Id}.mkv";
             string incomplete = $"Muxed/{video.Id}incomplete.mkv";
              string complete_vidonly = $"VideoOnly/{video.Id}.{streams.VideoOnlyStreamInfo.Container}";
               string complete_audonly = $"AudioOnly/{video.Id}.{streams.AudioOnlyStreamInfo.Container}";
                                                      
            if(await Continue(complete))
            {
              
                    if(await MuxVideosAsync(video,complete_vidonly,complete_audonly,incomplete,progress,token))
                    {
                        RenameFile(incomplete,complete);
                    }
            }
            if(report)
            ReportEndVideo(video,Resolution.Mux);
        }
        private void DeleteIfExists(string path)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }
        public async Task<bool> DownloadVideoOnlyAsync(SavedVideo video,CancellationToken token,IProgress<double> progress,bool report=true)
        {
           
            bool ret=false;
            var streams = await BestStreamInfo.GetBestStreams(this, video.Id, token, false); 
            if(!can_download) return false;
            if(streams != null)
            {
                await MoveLegacyStreams(video,streams);
                string complete = $"VideoOnly/{video.Id}.{streams.VideoOnlyStreamInfo.Container}";
                string incomplete = $"VideoOnly/{video.Id}incomplete.{streams.VideoOnlyStreamInfo.Container}";

                if(await Continue(complete))
                {
                    streams = await BestStreamInfo.GetBestStreams(this,video.Id,token);
                    if(streams != null)
                    {
                        using(var strm = await YoutubeClient.Videos.Streams.GetAsync(streams.VideoOnlyStreamInfo,token))
                        {
                            if(token.IsCancellationRequested)
                            {
                                return false;
                            }
                            if(report)
                            ReportStartVideo(video, Resolution.VideoOnly,streams.VideoOnlyStreamInfo.Size.Bytes);
                            long len=await GetLengthAsync(incomplete);
                            
                            using(var dest = await OpenOrCreateAsync(incomplete))
                            {
                                ret=await CopyStreamAsync(strm,dest,len,streams.VideoOnlyStreamInfo.Size.Bytes,4096,progress,token);
                            }
                            if(ret)
                            {
                                RenameFile(incomplete,complete);
                                if(report)
                                ReportEndVideo(video, Resolution.VideoOnly);
                            }
                        }
                    }
                }else{
                        
                    ret=true;
                }
            }

             //We know its resolution 
            return ret;
        }
        private async Task MoveLegacyStream(string src,string dest)
        {
            if(src.Equals(dest))
                return;
            
            if(!await Continue(dest))
                return;
            
            if(!await FileExistsAsync(src))
                return;

            RenameFile(src,dest);

        }
        public async Task MoveLegacyStreams(SavedVideo video,BestStreams streams)
        {
            if(video.LegacyVideo)
            {
                string legacyVideoOnlyComplete = $"VideoOnly/{video.Id}.mp4";
                string legacyAudioOnlyComplete = $"AudioOnly/{video.Id}.mp4";
                 string legacyPreMuxedComplete = $"PreMuxed/{video.Id}.mp4";

                 string modernVideoOnlyComplete = $"VideoOnly/{video.Id}.{streams.VideoOnlyStreamInfo.Container}";
                 string modernAudioOnlyComplete = $"AudioOnly/{video.Id}.{streams.AudioOnlyStreamInfo.Container}";
                 string modernPreMuxedComplete = $"PreMuxed/{video.Id}.{streams.MuxedStreamInfo}";

                string legacyVideoOnlyInComplete = $"VideoOnly/{video.Id}incomplete.mp4";
                string legacyAudioOnlyInComplete = $"AudioOnly/{video.Id}incomplete.mp4";
                 string legacyPreMuxedInComplete = $"PreMuxed/{video.Id}incomplete.mp4";

                 string modernVideoOnlyInComplete = $"VideoOnly/{video.Id}incomplete.{streams.VideoOnlyStreamInfo.Container}";
                 string modernAudioOnlyInComplete = $"AudioOnly/{video.Id}incomplete.{streams.AudioOnlyStreamInfo.Container}";
                 string modernPreMuxedInComplete = $"PreMuxed/{video.Id}incomplete.{streams.MuxedStreamInfo}";

                await MoveLegacyStream(legacyVideoOnlyComplete,modernVideoOnlyComplete);
                await MoveLegacyStream(legacyAudioOnlyComplete,modernAudioOnlyComplete);
                await MoveLegacyStream(legacyPreMuxedComplete,modernPreMuxedComplete);

                 await MoveLegacyStream(legacyVideoOnlyInComplete,modernVideoOnlyInComplete);
                await MoveLegacyStream(legacyAudioOnlyInComplete,modernAudioOnlyInComplete);
                await MoveLegacyStream(legacyPreMuxedInComplete,modernPreMuxedInComplete);
                   video.LegacyVideo=false;
                            await WriteAllTextAsync($"Info/{video.Id}.json",JsonConvert.SerializeObject(video));
                      
                
            }
        }
        private async Task<bool> DownloadAudioOnlyAsync(SavedVideo video,CancellationToken token,IProgress<double> progress,bool report=true)
        {
            
            bool ret=false;
            var streams = await BestStreamInfo.GetBestStreams(this, video.Id, token, false);
            if(!can_download) return false;
            if(streams != null)
            {
                string complete = $"AudioOnly/{video.Id}.{streams.AudioOnlyStreamInfo.Container}";
                string incomplete = $"AudioOnly/{video.Id}incomplete.{streams.AudioOnlyStreamInfo.Container}";
                await MoveLegacyStreams(video,streams);
                if(await Continue(complete))
                {
                    
                    streams = await BestStreamInfo.GetBestStreams(this,video.Id,token);
                    if(streams != null)
                    {
                        
                        using(var strm = await YoutubeClient.Videos.Streams.GetAsync(streams.AudioOnlyStreamInfo,token))
                        {
                            if(token.IsCancellationRequested)
                            {
                                return false;
                            }
                            if(report)
                            ReportStartVideo(video, Resolution.AudioOnly,streams.AudioOnlyStreamInfo.Size.Bytes);
                            long len=await GetLengthAsync(incomplete);
                            
                            using(var dest = await OpenOrCreateAsync(incomplete))
                            {
                                ret=await CopyStreamAsync(strm,dest,len,streams.AudioOnlyStreamInfo.Size.Bytes,4096,progress,token);
                            }
                            if(ret)
                            {
                                RenameFile(incomplete,complete);
                                if(report)
                                ReportEndVideo(video, Resolution.AudioOnly);
                            }
                        }
                    }
                }else{
                      
                    ret=true;
                }
            }

             //We know its resolution 
            return ret;
        }
        private async Task DownloadPreMuxedVideoAsync(SavedVideo video, CancellationToken token,IProgress<double> progress,bool report=true)
        {
            var streams = await BestStreamInfo.GetBestStreams(this, video.Id, token, false);
            if(!can_download) return;
            if(streams != null)
            {
                await MoveLegacyStreams(video,streams);
                string complete = $"PreMuxed/{video.Id}.{streams.MuxedStreamInfo.Container}";
                string incomplete = $"PreMuxed/{video.Id}incomplete.{streams.MuxedStreamInfo.Container}";

                if(await Continue(complete))
                {
                   
                    streams = await BestStreamInfo.GetBestStreams(this,video.Id,token);
                    if(streams != null)
                    {
                        
                        using(var strm = await YoutubeClient.Videos.Streams.GetAsync(streams.MuxedStreamInfo,token))
                        {
                             if(token.IsCancellationRequested)
                            {
                                return;
                            }
                            if(report)
                            ReportStartVideo(video,Resolution.PreMuxed,streams.MuxedStreamInfo.Size.Bytes);
                            long len=await GetLengthAsync(incomplete);
                            bool ret;
                            using(var dest = await OpenOrCreateAsync(incomplete))
                            {
                                ret=await CopyStreamAsync(strm,dest,len,streams.MuxedStreamInfo.Size.Bytes,4096,progress,token);
                            }
                            //We know its resolution 
                            if(ret)
                            {
                                RenameFile(incomplete,complete);
                                if(report)
                                ReportEndVideo(video, Resolution.PreMuxed); 
                            }
                        }
                    }
                }
                
            }

            
        }
        private (SavedVideo video, Resolution resolution) Dequeue(out bool hasAny)
        {
            SavedVideo video;
            Resolution resolution;
            hasAny = false;
            lock (QueueList)
            {
                if (QueueList.Count > 0)
                {
                    (video, resolution) = QueueList[QueueList.Count - 1];
                    hasAny = true;
                    QueueList.RemoveAt(QueueList.Count - 1);
                }
                else
                {
                    video = null;
                    resolution = Resolution.PreMuxed;
                }
            }
            return (video, resolution);
        }
    }
}