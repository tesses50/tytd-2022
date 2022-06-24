To use this tytd for javascript

=====models and enums=====
subscription:
    Id: ChannelId
    BellInfo: see subscription-conf

subscriptions:
    array of subscription

resolutions:
    0: Mux
    1: PreMuxed
    2: AudioOnly
    3: VideoOnly

subscription-conf:
    DoNothing
    GetInfo
    Notify
    Download
    NotifyAndDownload (This is default)

queuelist:
    an array containing
    Item1: see savedvideo
    Item2: see resolutions

savedvideoprogress:
     Progress: progress as 0-100
     ProgressRaw: progress as 0.0-1.0
     Length: this is the length of video in bytes (Is wrong for Mux)
     Video: see saved video

savedvideo:
    Id: Video Id    
    Title: Video Title
    AuthorChannelId: YouTube Channel Id for Video
    AuthorChannelTitle: YouTube Channel Title for Video
    Description: Video Description
    Keywords: YouTube Tags (this is an array)
    Likes: YouTube Video Likes
    Dislikes: YouTube Video Dislikes (I know they removed the ability but this can be used with return youtube dislikes)
    Views: YouTube Views
    Duration: Video Duration expressed as "00:03:48" could be "00:03:48.420420"
    UploadDate: Video Upload Date expressed as "2015-07-22T19:00:00-05:00"
    AddDate: date when added to downloader expressed as "2022-04-30T02:10:22.4359564-05:00"

savedplaylist:
    Id: Playlist Id
    Title: Playlist Title
    AuthorChannelId: YouTube Channel Id for Playlist
    AuthorChannelTitle: YouTube Channel Title for Playlist
    Description: Playlist Description
    Videos: an array of Video Ids (the videos in the playlist)

savedchannel:
    Id: Channel Id
    Title: Channel Title
    

=====methods=====

constructor example:

    var tytd=new TYTD("http://192.168.0.142:3252/",1);


to download a video, playlist, channel or user:
   
    tytd.downloadItem("https://youtube.com/watch?v=il9nqWw9W3Y");
    tytd.downloadItem("https://youtube.com/watch?v=il9nqWw9W3Y",0); //for Mux

to get video progress:
    
    tytd.progress(function(e){
       //see  savedvideoprogress
    });

to get queue:
    tytd.queuelist(function(e){
        //see queuelist
    });

to get videos:
    tytd.getvideos(function(e){
        //this will be fired for each video
        //see savedvideo
        //to get title
        e.Title
    });

to get playlists:
    tytd.getplaylists(function(e){
        //this will be fired for each playlist
        //see savedplaylist
        //to get title
        e.Title
    });

to get channels:
     tytd.getchannels(function(e){
        //this will be fired for each channel
        //see savedchannel
        //to get title
        e.Title
     });

to get video info for id:
    tytd.getvideoinfo("il9nqWw9W3Y",function(e){
        //see savedvideo
        //to get title
        e.Title //should be "Demi Lovato - Cool For The Summer (Official Video)" for this specific id
    });

to get playlist info for id:
    tytd.getplaylistinfo("PLa1F2ddGya_-UvuAqHAksYnB0qL9yWDO6",function(e)           
    {
        e.Title //should be "Blender Fundamentals 2.8" for this specific id
    });

to get channel info for id:
    tytd.getchannelinfo("UCnyB9MYKRkSFK3IIB32CoVw",function(e)
    {
        e.Title //should be "DemiLovatoVEVO" for this specific id
    });

to get subscriptions:
    tytd.getsubscriptions(function(e){
        //see subscriptions
    });

to subscribe (ChannelId):
    You Can replace NotifyAndDownload with anything from subscription-conf
    
    if you want to get info about channel:
    tytd.subscribe("UCnyB9MYKRkSFK3IIB32CoVw",true,"NotifyAndDownload");
    if you dont want to get info about channel:
    tytd.subscribe("UCnyB9MYKRkSFK3IIB32CoVw",false,"NotifyAndDownload");

to subscribe (username):
     You Can replace NotifyAndDownload with anything from subscription-conf

    tytd.subscribe("DemiLovatoVEVO","NotifyAndDownload");
    
to unsubscribe:
    tytd.unsubscribe("UCnyB9MYKRkSFK3IIB32CoVw");

to change bell (subscription-conf):
    You Can replace Download with anything from subscription-conf
    tytd.resubscribe("UCnyB9MYKRkSFK3IIB32CoVw","Download");

to enumerate directories:
    tytd.getdirectories("SomeDir/SomeSubDir",function(e)
    {
        //if the path was "SomeDir/SomeSubDir/john"
        //it would be "john"
        //this is an array
    });

to enumerate files:
    tytd.getfiles("SomeDir/SomeSubDir",function(e)
    {
        //if the path was "SomeDir/SomeSubDir/john.txt"
        //it would be "john.txt"
        //this is an array
    });

file exists:
    fileexists("SomeFile.txt",function(){
        //SomeFile.txt exists
    },function(){
        //SomeFile.txt doesnt exist
    });

directory exists:
    directoryexists("SomeDir",function(){
        //SomeDir exists
    },function(){
        //SomeDir doesnt exist
    });


