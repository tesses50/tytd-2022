# Video Streams Info
This is Json Object (With these Key/Value Pairs)<br>
See [this](StreamInfo.md) if you havent already

| Name    | Description       |  Type |
|--------|-------|------|
| AudioCodec | The audio codec of stream ex "mp4a.40.2" | String |  
| FrameRate | The video framerate ex 24 | Int32 |
| MaxHeight | Dont use this | Int32 |  
| QualityLabel | Video quality label ex "720p" | String |  
| VideoWidth | Video Width ex 1280 | Int32 |  
| VideoHeight | Video Width ex 720 | Int32 |  
| HasVideo | Stream Has Video | Boolean |  
| HasAudio | Stream Has Audio | Boolean |  
| Size     | FileSize in bytes | Int64 |  
| Container | This is what {Container} means ex "mp4" | String |  
| VideoCodec | The video codec of stream ex "avc1.64001F" |  
| Bitrate | Stream's bitrate | Int64 |  
| Url | Direct YouTube Url (usually googlevideo.com) | String |  
