using Newtonsoft.Json;

public class TYTDConfiguration
{
    public TYTDConfiguration()
    {
        Url = "http://127.0.0.1:3252/";
        LocalFiles=Environment.CurrentDirectory;
    }
    public string Url {get;set;}

    public string LocalFiles {get;set;}

    public static TYTDConfiguration Load()
    {
        if(!File.Exists("proxy.json")) return new TYTDConfiguration();
        var res= JsonConvert.DeserializeObject<TYTDConfiguration>(File.ReadAllText("proxy.json"));
        if(res != null)
        {
            return res;
        }
        return new TYTDConfiguration();
    }
}