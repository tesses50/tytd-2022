# Server

Get Requests
  
| Url     | Description       |  
|--------|-------|  
| http://localhost:3252/api/v2/Progress | [Video Progress](VideoProgress.md)      |  
| http://localhost:3252/api/v2/QueueList       |  [Queue List](QueueList.md)    |  
| http://localhost:3252/api/AddItem/{UrlOrId} |  Download Video, Playlist, Channel or UserName v1 (Same as http://localhost:3252/api/AddItemRes/1/{UrlOrId}) (this is valid on previous downloader) (I prefer this one) |  
| http://localhost:3252/api/AddItemRes/{Resolution}/{UrlOrId} | Download Video, Playlist, Channel or UserName using [Resolution](Resolution.md)  (uses the number) v1 (this is valid on previous downloader) (I prefer this one) |    
| http://localhost:3252/api/v2/AddVideo?v={Id}&res=PreMuxed | Download Video v2 using [Resolution](Resolution.md) (uses  the Name on queryparm res) (I Prefer v1)   |  
| http://localhost:3252/api/v2/AddPlaylist?v={Id}&res=PreMuxed | Download Playlist v2 using [Resolution](Resolution.md) (uses  the Name on queryparm res) (I Prefer v1)   |  
| http://localhost:3252/api/v2/AddChannel?v={Id}&res=PreMuxed | Download Channel v2 using [Resolution](Resolution.md) (uses  the Name on queryparm res) (I Prefer v1)   |  
| http://localhost:3252/api/v2/AddUser?v={Id}&res=PreMuxed | Download User v2 using [Resolution](Resolution.md) (uses  the Name on queryparm res) (I Prefer v1)   |  
| http://localhost:3252/api/v2/AddItem?v={Id}&res=PreMuxed | Download Video, Playlist Channel or User v2 using [Resolution](Resolution.md) (uses  the Name on queryparm res) (I Prefer v1)   |  
| http://localhost:3252/api/v2/subscribe?id={ChannelId}&getinfo=true\|false&conf=NotifyAndDownload | Subscribe to YouTuber (ChannelId), See [Bell](Bell.md) for conf queryparm |  
| http://localhost:3252/api/v2/subscribe?id={UserName}&conf=NotifyAndDownload | Subscribe to YouTuber (UserName), See [Bell](Bell.md) for conf queryparm |  
| http://localhost:3252/api/v2/resubscribe?id={ChannelId}&conf=Download | Change Bell for YouTuber, See [Bell](Bell.md) for conf queryparm |  
| http://localhost:3252/api/v2/unsubscribe?id={ChannelId} | Unsubscribe from YouTuber |  
| http://localhost:3252/api/v2/subscriptions | Get Subscriptions, Is a json array of [Subscription](Subscription.md) |  