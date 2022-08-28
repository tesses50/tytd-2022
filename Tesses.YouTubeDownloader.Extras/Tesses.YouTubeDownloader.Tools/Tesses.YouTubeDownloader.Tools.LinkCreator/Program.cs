using Tesses.YouTubeDownloader.Tools.Common;
using Tesses.YouTubeDownloader;
using System;
using System.IO;

Resolution res=Resolution.PreMuxed;

bool verbose=false;
bool isSymlink = false;
bool isBigExport=false;
List<string> _args=new List<string>();
foreach(var arg in args)
{
    bool any=false;
    if(arg == "-h" || arg == "--help")
    {
        _args.Clear();
        break;
    }
    if( (arg.Length >= 2 &&  arg[1] != '-' && arg[0] == '-'  && arg.Contains("g") )|| arg == "--generate-export")
    {
        any=true;
        isBigExport=true;
    }
    if( (arg.Length >= 2 &&  arg[1] != '-' && arg[0] == '-'  && arg.Contains("s") )|| arg == "--symbolic")
    {
        any=true;
        isSymlink=true;
    }
    if((arg.Length >= 2 &&  arg[1] != '-' && arg[0] == '-' && arg.Contains("m") ) || arg == "--mux" )
    {
        any=true;
        res = Resolution.Mux;
    }
    if((arg.Length >= 2 &&  arg[1] != '-' && arg[0] == '-' && arg.Contains("a") ) || arg == "--audio-only")
    {
        any=true;
        res = Resolution.AudioOnly;
        
    }
    if ((arg.Length >= 2 &&  arg[1] != '-' && arg[0] == '-' && arg.Contains("V") ) || arg=="--video-only")
    {
        any=true;
        res = Resolution.VideoOnly;
        
    }
     if ((arg.Length >= 2 &&   arg[1] != '-' && arg[0] == '-' && arg.Contains("v") ) || arg=="--verbose")
    {
        any=true;
        verbose=true;
        
    }
    
        if(!any)
        _args.Add(arg);
    
}
string[] argv = _args.ToArray();
if(argv.Length < 2)
{
    string app = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

    Console.WriteLine($"usage: {app} [-smaVvg] <Working> <Destination> [<Resolution>]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -s, --symbolic              make symbolic links instead of hard links");
    Console.WriteLine("  -m, --mux                   set resolution to Mux");
    Console.WriteLine("  -a, --audio-only            set resolution to AudioOnly");
    Console.WriteLine("  -V, --video-only            set resolution to VideoOnly");
    Console.WriteLine("  -h, --help                  show this help");
    Console.WriteLine("  -v, --verbose               print video names");
    Console.WriteLine("  -g, --generate-export       Export everything to human readable format");
    Console.WriteLine();
    Console.WriteLine("Positional Arguments:");
    Console.WriteLine("  Working                     the folder containing the Info Directory for TYTD. (required)");
    Console.WriteLine("  Destination                 the folder to create links within. (required)");


}else{


Environment.CurrentDirectory=argv[0];
TYTDCurrentDirectory currentDirectory=new TYTDCurrentDirectory();
currentDirectory.CanDownload=false;

if(isSymlink){
    if(isBigExport)
    {
        string outDir = argv[1];
        string sd = Path.Combine(outDir,"PreMuxed");
        string hd = Path.Combine(outDir,"Muxed");
        string vo = Path.Combine(outDir,"VideoOnly");
        string ao = Path.Combine(outDir,"AudioOnly");
        string meta=Path.Combine(outDir,"Meta");
        
        List<string> videos=new List<string>();
        try{
              await SymlinkGenerator.GenerateSymlinks(currentDirectory,sd,Resolution.PreMuxed,verbose);
              await SymlinkGenerator.GenerateSymlinks(currentDirectory,hd,Resolution.Mux,verbose);
              await SymlinkGenerator.GenerateSymlinks(currentDirectory,vo,Resolution.VideoOnly,verbose);
              await SymlinkGenerator.GenerateSymlinks(currentDirectory,ao,Resolution.AudioOnly,verbose);
              await SymlinkGenerator.GenerateMeta(videos,currentDirectory,meta,verbose);
        }catch(Exception ex){
            _=ex;
        }
        File.WriteAllLines(Path.Combine(outDir,"videos.txt"),videos);
    }else{
        await SymlinkGenerator.GenerateSymlinks(currentDirectory,argv[1],res,verbose);
    }
}else{
    if(isBigExport)
    {
        string outDir = argv[1];
        string sd = Path.Combine(outDir,"PreMuxed");
        string hd = Path.Combine(outDir,"Muxed");
        string vo = Path.Combine(outDir,"VideoOnly");
        string ao = Path.Combine(outDir,"AudioOnly");
        string meta=Path.Combine(outDir,"Meta");
        List<string> videos=new List<string>();
         try{
              await SymlinkGenerator.GenerateHardLinks(currentDirectory,sd,Resolution.PreMuxed,verbose);
              await SymlinkGenerator.GenerateHardLinks(currentDirectory,hd,Resolution.Mux,verbose);
              await SymlinkGenerator.GenerateHardLinks(currentDirectory,vo,Resolution.VideoOnly,verbose);
              await SymlinkGenerator.GenerateHardLinks(currentDirectory,ao,Resolution.AudioOnly,verbose);
              await SymlinkGenerator.GenerateMeta(videos,currentDirectory,meta,verbose);
              File.WriteAllLines(Path.Combine(outDir,"videos.txt"),videos);
        }catch(Exception ex){
            _=ex;
        }

    }else{
        await SymlinkGenerator.GenerateHardLinks(currentDirectory,argv[1],res,verbose);
    }
}
}
