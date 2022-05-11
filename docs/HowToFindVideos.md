# How to find videos

| Name    | Url   | Url with filename in content-disposition    | Path |
| ------- | ------- | ---------- | ----- |
| Mux     | http://localhost:3252/api/Storage/File/Mux/{Id}.mkv | http://localhost:3252/api/Storage/VideoRes/0/{Id}  | Mux/{Id}.mkv | 
| PreMuxed | http://localhost:3252/api/Storage/File/PreMuxed/{Id}.{Container} | http://localhost:3252/api/Storage/VideoRes/1/{Id} | PreMuxed/{Id}.{Container} |
| AudioOnly | http://localhost:3252/api/Storage/File/AudioOnly/{Id}.{Container} | http://localhost:3252/api/Storage/VideoRes/2/{Id} | AudioOnly/{Id}.{Container} |
| 
| VideoOnly | http://localhost:3252/api/Storage/File/VideoOnly/{Id}.{Container} | http://localhost:3252/api/Storage/VideoRes/3/{Id} | VideoOnly/{Id}.{Container} |
| 

# What you should see before this
[StreamInfo](StreamInfo.md) (this is required to get {Container})<br>
[Resolution](Resolution.md) (this is recomended so you know what the resolution numbers mean)