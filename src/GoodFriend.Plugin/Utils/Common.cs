namespace GoodFriend.Utils;

using System.Diagnostics;


public static class Common
{
    /// <summary> Opens the given link in the default application. </summary>
    public static void OpenLink(string link)
    {
        Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
    }
}