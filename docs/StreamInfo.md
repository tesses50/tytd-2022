# Video Streams Info
This is Json Object (With these Key/Value Pairs)<br>
File Path is StreamInfo/{Id}.json<br>
This is required to get extension for all resolutions except "Mux" which uses mkv<br>
these extensions are not prepended with .<br>
hince all of the resolutions use the same class use [this](StreamInfo2.md) for json structure for all three with type "Object"
| Name    | Description       |  Type |
|--------|-------|------|
| Expires | When the url's expire (6 hours after adding video) ex 2022-05-07T01:48:23.982827-05:00 | DateTime(String) |  
| VideoOnly | This stream go into "VideoOnly" folder, no audio | Object |  
| Audio | This stream go into "AudioOnly" folder, no video | Object |  
| Muxed | This stream go into "PreMuxed" folder, both video and audio (no conversion required) | Object |  