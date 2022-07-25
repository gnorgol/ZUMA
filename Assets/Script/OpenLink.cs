using UnityEngine;
using System.Runtime.InteropServices;
public class OpenLink : MonoBehaviour
{
    public static void OpenUrl(string url)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        OpenTab(url);
#endif
#if UNITY_EDITOR && !UNITY_WEBGL
        Application.OpenURL(url);
#endif
    }
    [DllImport("__Internal")]
    private static extern void OpenTab(string url);
}
