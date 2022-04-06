using Newtonsoft.Json;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using System.Threading;
using YoutubeExplode.Exceptions;

namespace Tesses.YouTubeDownloader
{
    public class BestStreams
    {
       
        public BestStreamInfo MuxedStreamInfo {get;set;}

        public BestStreamInfo VideoOnlyStreamInfo {get;set;}

        public BestStreamInfo AudioOnlyStreamInfo {get;set;}

        public static async Task<string> GetPathResolution(TYTDBase storage,SavedVideo video,Resolution resolution=Resolution.PreMuxed)
        {
            if(video.LegacyVideo)
            {
                if(resolution == Resolution.Mux)
                    return $"{TYTDManager.ResolutionToDirectory(resolution)}/{video.Id}.mkv";

                return $"{TYTDManager.ResolutionToDirectory(resolution)}/{video.Id}.mp4";
            }else{
                 var f= await BestStreamInfo.GetBestStreams(storage,video.Id);
                 if(f ==null)
                    return "";
                    
                      string[] exts= new string[] {"mkv",f.MuxedStreamInfo.Container.Name,f.AudioOnlyStreamInfo.Container.Name,f.VideoOnlyStreamInfo.Container.Name};
                        string ext=exts[(int)resolution];

                        return $"{TYTDManager.ResolutionToDirectory(resolution)}/{video.Id}.{ext}";
            }
        }

        
    }

    
    
    public class BestStreamInfo : IStreamInfo
    {
        public static async Task<BestStreams> GetBestStreams(TYTDBase storage,VideoId id)
        {
            //Console.WriteLine("IN FUNC");
            if(storage.DirectoryExists("StreamInfo"))
            {
                //Console.WriteLine("DIR");
                if(storage.FileExists($"StreamInfo/{id.Value}.json"))
                {
                    //Console.WriteLine("STREAMS");
                    BestStreamsSerialized serialization=JsonConvert.DeserializeObject<BestStreamsSerialized>(await storage.ReadAllTextAsync($"StreamInfo/{id.Value}.json"));
                    
                        BestStreams streams=new BestStreams();
                        streams.VideoOnlyStreamInfo = new BestStreamInfo(serialization.VideoOnly);
                        streams.AudioOnlyStreamInfo = new BestStreamInfo(serialization.Audio);
                        streams.MuxedStreamInfo =new BestStreamInfo(serialization.Muxed);
                        return streams;
                    
                }
            }
            return null;
        }
        public static async Task<BestStreams> GetBestStreams(TYTDStorage storage,VideoId id,CancellationToken token=default(CancellationToken),bool expire_check=true)
        {
            if(storage.DirectoryExists("StreamInfo"))
            {
                if(storage.FileExists($"StreamInfo/{id.Value}.json"))
                {
                    BestStreamsSerialized serialization=JsonConvert.DeserializeObject<BestStreamsSerialized>(await storage.ReadAllTextAsync($"StreamInfo/{id.Value}.json"));
                    if(DateTime.Now < serialization.Expires || !expire_check)
                    {
                        BestStreams streams=new BestStreams();
                        streams.VideoOnlyStreamInfo = new BestStreamInfo(serialization.VideoOnly);
                        streams.AudioOnlyStreamInfo = new BestStreamInfo(serialization.Audio);
                        streams.MuxedStreamInfo =new BestStreamInfo(serialization.Muxed);
                        return streams;
                    }
                }
            }else{
                storage.CreateDirectory("StreamInfo");
            }
            DateTime expires=DateTime.Now.AddHours(6);
            try{
            var res=await storage.YoutubeClient.Videos.Streams.GetManifestAsync(id,token);
            
            var audioOnly=res.GetAudioOnlyStreams().GetWithHighestBitrate();
            var videoOnly=res.GetVideoOnlyStreams().GetWithHighestVideoQuality();
            var muxed = res.GetMuxedStreams().GetWithHighestVideoQuality();
            BestStreamsSerialized serialized=new BestStreamsSerialized();
            serialized.Expires=expires;
            BestStreams streams1 = new BestStreams();
            streams1.VideoOnlyStreamInfo=new BestStreamInfo();
            streams1.VideoOnlyStreamInfo.SetInfo(videoOnly);
            serialized.VideoOnly=streams1.VideoOnlyStreamInfo.Serialization;
            streams1.AudioOnlyStreamInfo=new BestStreamInfo();
            streams1.AudioOnlyStreamInfo.SetInfo(audioOnly);
            serialized.Audio=streams1.AudioOnlyStreamInfo.Serialization;
            streams1.MuxedStreamInfo =new BestStreamInfo();
            streams1.MuxedStreamInfo.SetInfo(muxed);
            serialized.Muxed = streams1.MuxedStreamInfo.Serialization;

            await storage.WriteAllTextAsync($"StreamInfo/{id.Value}.json",JsonConvert.SerializeObject(serialized));
            return streams1;
            }catch(YoutubeExplodeException ex)
            {
                _=ex;
                return null;
            }
        } 
         private class BestStreamsSerialized
        {
            public DateTime Expires {get;set;}
            public BestStreamInfoSerialization VideoOnly {get;set;}
            public BestStreamInfoSerialization Audio {get;set;}

            public BestStreamInfoSerialization Muxed {get;set;}
            
            
        }
        public BestStreamInfo()
        {

        }
        public class BestStreamVideoInfo : IVideoStreamInfo
        {
            internal BestStreamVideoInfo(BestStreamInfoSerialization ser)
            {
                Serialization=ser;
            }
            public Container Container {get {return new Container(Serialization.Container);}}
           
            
            internal BestStreamInfoSerialization Serialization;
            public VideoQuality VideoQuality {get {return new VideoQuality(Serialization.QualityLabel,Serialization.MaxHeight,Serialization.FrameRate);} internal set {Serialization.QualityLabel=value.Label; Serialization.MaxHeight=value.MaxHeight; Serialization.FrameRate=value.Framerate;}}
            public YoutubeExplode.Common.Resolution VideoResolution {get {return new YoutubeExplode.Common.Resolution(Serialization.VideoWidth,Serialization.VideoHeight);} internal set{
                Serialization.VideoWidth = value.Width;
                Serialization.VideoHeight = value.Height;
            }}
            public string Url {get {return Serialization.Url;} }
            public string VideoCodec {get {return Serialization.VideoCodec;} internal set{Serialization.VideoCodec=value;}}

              public FileSize Size {get {return new FileSize(Serialization.Size);} }

             public Bitrate Bitrate {get {return new Bitrate(Serialization.Bitrate);}}
            
        }
            public class BestStreamAudioInfo : IAudioStreamInfo
        {
            internal BestStreamAudioInfo(BestStreamInfoSerialization ser)
            {
                Serialization=ser;
            }
            public Container Container {get {return new Container(Serialization.Container);}}
           
            
            internal BestStreamInfoSerialization Serialization;
              public string Url {get {return Serialization.Url;} }
            public string AudioCodec {get {return Serialization.AudioCodec;} internal set{Serialization.AudioCodec=value;}}

              public FileSize Size {get {return new FileSize(Serialization.Size);} }

             public Bitrate Bitrate {get {return new Bitrate(Serialization.Bitrate);}}
        }
        private BestStreamInfo(BestStreamInfoSerialization ser)
        {
            Serialization=ser;
            if(HasVideo)
            {
                VideoInfo=new BestStreamVideoInfo(Serialization);
            }
            if(HasAudio)
            {
                AudioInfo=new BestStreamAudioInfo(Serialization);
            }
        }
        
        internal class BestStreamInfoSerialization
        {
            public string AudioCodec {get;set;}
            public int FrameRate {get;set;}
            public int MaxHeight {get;set;}
            public string QualityLabel {get;set;}
            public int VideoWidth {get;set;}
            public int VideoHeight {get;set;}
            public bool HasVideo {get;set;}

            public bool HasAudio {get;set;}
            public long Size {get;set;}
            public string Container {get;set;}

            public string VideoCodec {get;set;}
            public long Bitrate {get;set;}
            public string Url {get;set;} //streamUrl
         
        }
        
        internal BestStreamInfoSerialization Serialization =new BestStreamInfoSerialization();
        
        public bool HasVideo {get {return Serialization.HasVideo;} private set {Serialization.HasVideo=value;}}
        public bool HasAudio {get {return Serialization.HasAudio;} private set {Serialization.HasAudio=value;}}
    
        public FileSize Size {get {return new FileSize(Serialization.Size);} private set {Serialization.Size = value.Bytes;}}

        public Bitrate Bitrate {get {return new Bitrate(Serialization.Bitrate);} private set{Serialization.Bitrate = value.BitsPerSecond;}}
        public IStreamInfo StreamInfo {get {return _si;} set {SetInfo(value);}}
        
        private IStreamInfo _si;
        
        public string Url {get {return Serialization.Url; } private set {Serialization.Url=value;}}       
        
        public Container Container {get {return new Container(Serialization.Container);} set {Serialization.Container=value.Name;}}
        public IVideoStreamInfo VideoInfo {get;private set;}
        public IAudioStreamInfo AudioInfo {get;private set;}
        internal void SetInfo(IStreamInfo info)
        {
            Size=info.Size;
            Container = info.Container;
            Bitrate=info.Bitrate;
            Url = info.Url;
            IVideoStreamInfo vsi = info as IVideoStreamInfo;
            IAudioStreamInfo asi = info as IAudioStreamInfo;
            if(vsi!=null)
            {
                HasVideo=true;
                BestStreamVideoInfo videoInfo=new BestStreamVideoInfo(Serialization);
                videoInfo.VideoCodec=vsi.VideoCodec;
                videoInfo.VideoQuality = vsi.VideoQuality;
                videoInfo.VideoResolution = vsi.VideoResolution;
                VideoInfo =videoInfo;
            }
            if(asi != null)
            {
                HasAudio=true;
                BestStreamAudioInfo audioInfo=new BestStreamAudioInfo(Serialization);
                audioInfo.AudioCodec=asi.AudioCodec;
                AudioInfo = audioInfo;
            }
            //vsi.VideoCodec
        }
    }
}