# Video Streams Info
This is Json Object (With these Key/Value Pairs)<br>
See [this](StreamInfo.md) if you havent already

| Name    | Description       |  Type |
|--------|-------|------|
|  | When the url's expire (6 hours after adding video) ex 2022-05-07T01:48:23.982827-05:00 | DateTime(String) |  
| VideoOnly | This stream go into "VideoOnly" folder, no audio | Object |  
| Audio | This stream go into "AudioOnly" folder, no video | Object |  
| Muxed | This stream go into "PreMuxed" folder, both video and audio (no conversion required) | Object |  