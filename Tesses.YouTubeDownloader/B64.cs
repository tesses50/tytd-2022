using System;
namespace Tesses.YouTubeDownloader
{
internal static class B64
{
    public static string Base64UrlEncodes(string arg)
    {
        return Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(arg));
    }

    public static string Base64Encode(byte[] arg)
    {
        return Convert.ToBase64String(arg);
    }
    public static byte[] Base64Decode(string arg)
    {
        return Convert.FromBase64String(arg);
    }

    public static string Base64Encodes(string arg)
    {
        return Base64Encode(System.Text.Encoding.UTF8.GetBytes(arg));
    }

    public  static string Base64UrlEncode(byte[] arg)
    {
        string s = Convert.ToBase64String(arg); // Regular base64 encoder
        s = s.Split('=')[0]; // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
        return s;
    }
    public static string Base64Decodes(string arg)
    {
        return System.Text.Encoding.UTF8.GetString(Base64Decode(arg));
    }
    public static string Base64UrlDecodes(string arg)
    {
        return System.Text.Encoding.UTF8.GetString(Base64UrlDecode(arg));
    }
    public static byte[] Base64UrlDecode(string arg)
    {
        string s = arg;
        s = s.Replace('-', '+'); // 62nd char of encoding
        s = s.Replace('_', '/'); // 63rd char of encoding
        switch (s.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2: s += "=="; break; // Two pad chars
            case 3: s += "="; break; // One pad char
            default: throw new System.Exception(
              "Illegal base64url string!");
        }
        return Convert.FromBase64String(s); // Standard base64 decoder
    }

}
}