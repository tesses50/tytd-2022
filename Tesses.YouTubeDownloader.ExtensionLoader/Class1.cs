using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tesses.YouTubeDownloader;
using Tesses.WebServer;
using System.Reflection;
using System.IO;
using System.Text;
using System.Net;
using Tesses.YouTubeDownloader.Server;

namespace Tesses.YouTubeDownloader.ExtensionLoader
{
    public class Loader : IExtensionContext
    {
        internal class IndexServer : Tesses.WebServer.Server
        {
            public IndexServer(List<IExtension> exts)
            {
                Extensions=exts;
            }
            List<IExtension> Extensions;
            public override async Task GetAsync(ServerContext ctx)
            {
                StringBuilder builder=new StringBuilder("<html><head><title>Extensions</title></head><body><h1>Extensions</h1>");
                
                foreach(var ext in Extensions)
                {
                    builder.Append($"<a href=\"{WebUtility.HtmlEncode(ext.Name)}\">{WebUtility.HtmlEncode(ext.Name)}</a><br>");
                }
                builder.Append("</body></html>");
                await ctx.SendTextAsync(builder.ToString());
            }
        }
        List<IExtension> extensions = new List<IExtension>();
        public List<IExtension> Extensions => extensions;
        
        IStorage Storage;
        MountableServer Server;
        string dir;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage">Storage for TYTD</param>
        /// <param name="lookInDir">where to look for extensions</param>
        public Loader(IStorage storage,string lookInDir="config/apidll")
        {
            Directory.CreateDirectory(lookInDir);
            dir=lookInDir;
            Server= new MountableServer(new IndexServer(Extensions));
            Storage = storage;
        }

        private (IExtension Extension,IServer Server) LoadExtension(string dllPath)
        {
            //Assembly.Load()
           Assembly assembly= Assembly.LoadFrom(dllPath);
           foreach(var cls in assembly.GetTypes())
           {
               if(typeof(Extension).IsAssignableFrom(cls))
               {
                   var ext=(Extension)Activator.CreateInstance(cls);
                   ext.name=Path.GetFileNameWithoutExtension(dllPath);
                   ChangeableServer server=new ChangeableServer();
                   ext.Server=server;
                   ext.Storage=Storage;
                   ext.OnStart();
                  return (ext,server);
               }
           }
           return (null,null);
        }
        /// <summary>
        /// Load Extensions
        /// </summary>
        public void LoadExtensions()
        {
            foreach(var extdir in Directory.GetDirectories(dir))
            {
                string extname = Path.GetFileName(extdir);
                string dll = Path.Combine(extdir,$"{extname}.dll");
                if(File.Exists(dll))
                {
                    var (ext,server) =LoadExtension(dll);

                    if(ext != null && server !=null)
                    {
                        Extensions.Add(ext);
                        Server.Mount($"/{WebUtility.UrlEncode(ext.Name)}",server);
                    }
                }
            }
        }
        /// <summary>
        /// Set server to TYTDServer.ExtensionServer
        /// Will set Storage.ExtensionContext
        /// </summary>
        /// <param name="server">TYTD server</param>
        public void SetServer(TYTDServer server)
        {
            server.ExtensionsServer.Server = Server; 
            var storage=Storage;
            if(storage !=null)
            {
                storage.ExtensionContext=this;
            }
        }

    }

    
    public abstract class Extension : IExtension
    {
        /// <summary>
        /// If You Have A WebPage You Want for Extension
        /// Set Server.Server to your server
        /// </summary>
        /// <value></value>
        public ChangeableServer Server {get; internal set;}
        /// <summary>
        /// Abstract storage for Downloader
        /// Use relative paths please
        /// </summary>
        /// <value></value>
        public IStorage Storage {get; internal set;}
        /// <summary>
        /// Get extension storage dir use "Storage" to actually access files
        /// The path config/apistore/{Name} is created if it doesnt exist
        /// </summary>
        /// <param name="path">relative and absolute paths are treated the same use / for spliting paths</param>
        /// <returns>a path for file</returns>
        public string ExtensionStorage(string path)
        {
            Storage.CreateDirectoryIfNotExist($"config/apistore/{Name}");
            return $"config/apistore/{Name}/{path.TrimStart('/')}";
        }
        /// <summary>
        /// Called when extension starts
        /// </summary>
        public abstract void OnStart();
        internal string name;
        /// <summary>
        /// Name, defaults to dll folder name * (the dll is loaded based on foldername)
        /// </summary>
        /// <value></value>
        public virtual string Name {get {return name;}}
        /// <summary>
        /// Called when extension needs to end
        /// </summary>
        public virtual void OnEnd()
        {

        }
        /// <summary>
        /// Called when Video On Downloader Finishes
        /// </summary>
        /// <param name="video">Info about Video</param>
        /// <param name="resolution">Video Resolution</param>
        public virtual async Task VideoFinished(SavedVideo video, Resolution resolution)
        {
            await Task.FromResult(true);
        }
        /// <summary>
        /// Called when downloader writes some data
        /// </summary>
        /// <param name="video">Info about Video</param>
        /// <param name="progress">Video Percent 0 -> 1 (might overflow so clamp it if you need to)</param>
        /// <returns></returns>
        public virtual async Task VideoProgress(SavedVideo video, double progress)
        {
            await Task.FromResult(true);
        }
        /// <summary>
        /// Called when Video On Downloader Starts
        /// </summary>
        /// <param name="video">Info about Video</param>
        /// <param name="resolution">Video Resolution</param>
        /// <param name="length">Video File Length (can be 0 if resolution==Resolution.Mux)</param>
        public virtual async Task VideoStarted(SavedVideo video, Resolution resolution, long length)
        {
            await Task.FromResult(true);
        }
    }
}
