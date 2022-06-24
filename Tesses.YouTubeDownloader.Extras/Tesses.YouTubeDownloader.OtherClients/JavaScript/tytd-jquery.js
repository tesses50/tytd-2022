class TYTD
{
    constructor(server,defaultres)
    {
        this.server=server;
        this.defaultres=defaultres;
    }

    get defaultRes()
    {
        return this.defaultres;
    }
    set defaultRes(value)
    {
        this.defaultres=value;
    }

    get serverUrl()
    {
        return this.server;
    }

    downloadItem(url,res)
    {
            var callurl=new URL(`api/AddItemRes/${res}/${url}`,this.server).href;
            $.ajax(callurl,function(e){});
    }
    downloadItem(url)
    {
        this.downloadItem(url,this.defaultres);
    }

    progress(p)
    {
        var callurl=new URL("api/v2/Progress",this.server).href;
        $.ajax(callurl,function(e){
            p(JSON.parse(e));
        });
    }
    queuelist(ql)
    {
        var callurl=new URL("api/v2/QueueList",this.server).href;
        $.ajax(callurl,function(e){
            ql(JSON.parse(e));
        });
    }
    getvideos(vid)
    {
        this.getvideoinfofiles(function(e){
            //an array
            e.forEach(element => {
                this.getvideoinfofile(element,vid);
            });
        });
    }
    getplaylists(pl)
    {
        this.getplaylistinfofiles(function(e){
            //an array
            e.forEach(element => {
                this.getplaylistinfofile(element,pl);
            });
        });
    }
    getchannels(chan)
    {
        this.getchannelinfofiles(function(e){
            e.forEach(element =>{
                this.getchannelinfofile(element,chan);
            });
        });
    }
    getchannelinfo(id,info)
    {
        this.getchannelinfofile(`${id}.json`,info);
    }
    getvideoinfo(id,info)
    {
        this.getvideoinfofile(`${id}.json`,info);
    }
    getvideoinfofile(filename,info)
    {
        var callurl=new URL(`api/Storage/File/Info/${filename}`,this.server).href;
        $.ajax(callurl,function(e){
            info(JSON.parse(e));
        });
    }
    getchannelinfofile(filename,info)
    {
        var callurl=new URL(`api/Storage/File/Channel/${filename}`,this.server).href;
        $.ajax(callurl,function(e){
            info(JSON.parse(e));
        });
    }
    getvideoinfofiles(vid)
    {
        var callurl=new URL("api/Storage/GetFiles/Info",this.server).href;
        $.ajax(callurl,function(e){
            vid(JSON.parse(e));
        });
    }
    getplaylistinfo(info)
    {
        this.getplaylistinfofile(`${id}.json`,info);
    }
    getplaylistinfofile(info)
    {
        var callurl=new URL(`api/Storage/File/Playlist/${filename}`,this.server).href;
        $.ajax(callurl,function(e){
            info(JSON.parse(e));
        });
    }
    getplaylistinfofiles(pl)
    {
        var callurl = new URL("api/Storage/GetFiles/Playlist",this.server).href;
        $.ajax(callurl,function(e){
            pl(JSON.parse(e));
        });
    }
    getchannelinfofiles(chan)
    {
        var callurl = new URL("api/Storage/GetFiles/Channel",this.server).href;
        $.ajax(callurl,function(e){
            chan(JSON.parse(e));
        });
    }
    getsubscriptions(subs)
    {
        var callurl = new URL("api/v2/subscriptions",this.server).href;
        $.ajax(callurl,function(e){
            subs(JSON.parse(e));
        });
    }
    subscribe(cid,getinfo,conf)
    {
        var ginfo = getinfo == true ? "true" : "false";
        var callurl = new URL(`api/v2/subscribe?id=${encodeURIComponent(cid)}&conf=${conf}&getinfo=${getinfo}`,this.server).href;
        $.ajax(callurl,function(e){
           
        });
    }
    subscribe(name,conf)
    {
        var callurl = new URL(`api/v2/subscribe?id=${encodeURIComponent(name)}&conf=${conf}`,this.server).href;
        $.ajax(callurl,function(e){
           
        });
    }
    unsubscribe(cid)
    {
        var callurl = new URL(`api/v2/unsubscribe?id=${encodeURIComponent(cid)}`,this.server).href;
        $.ajax(callurl,function(e){
           
        });
    }
    resubscribe(cid,conf)
    {
        var callurl = new URL(`api/v2/resubscribe?id=${encodeURIComponent(cid)}&conf=${conf}`,this.server).href;
        $.ajax(callurl,function(e){
           
        });
    }

    getfiles(path,ls)
    {
        var callurl=new URL(`api/Storage/GetFiles/${path}`,this.server).href;
        $.ajax(callurl,function(e){
            ls(JSON.parse(e));
        });
    }
    getdirectories(path,ls)
    {
        var callurl=new URL(`api/Storage/GetDirectories/${path}`,this.server).href;
        $.ajax(callurl,function(e){
            ls(JSON.parse(e));
        });
    }
    fileexists(path,exists,doesntexist)
    {
        var callurl=new URL(`api/Storage/FileExists/${path}`,this.server).href;
        $.ajax(callurl,function(e){
            if(e==="true")
            {
                exists();
            }else{
                doesntexist();
            }
        });
    }
    directoryexists(path,exists,doesntexist)
    {
        var callurl=new URL(`api/Storage/DirectoryExists/${path}`,this.server).href;
        $.ajax(callurl,function(e){
            if(e==="true")
            {
                exists();
            }else{
                doesntexist();
            }
        });
    }
    
}