using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GAOffline
{
    // Stands in for havoc.synapse-games.com. WWWManager.Begin is patched to call in here first:
    //   Begin(url, form): API calls are answered locally and loaded from a file:// URL instead.
    //   Begin(url):       data files (assets/cards.xml, assets/tutorial1.xml) are served from this DLL.
    public static class LocalServer
    {
        const string Host = "synapse-games.com/";
        static int counter;
        static string dir;

        public static string Dir
        {
            get
            {
                if (dir == null)
                {
                    try
                    {
                        dir = Path.Combine(Application.persistentDataPath, "ga_offline");
                        Directory.CreateDirectory(dir);
                    }
                    catch (Exception)
                    {
                        dir = Path.Combine(Application.temporaryCachePath, "ga_offline");
                        Directory.CreateDirectory(dir);
                    }
                    Debug.Log("[GAOffline] data folder " + dir);
                }
                return dir;
            }
        }

        public static WWW BeginForm(WWWManager mgr, string url, WWWForm form)
        {
            if (url == null || url.IndexOf(Host, StringComparison.Ordinal) < 0) return null;
            string message = Query(url, "message");
            if (message == null) return null;
            Dictionary<string, string> args = ParseForm(form == null ? null : form.data);
            string body;
            try
            {
                body = Api.Handle(message, args);
            }
            catch (Exception e)
            {
                Debug.LogError("[GAOffline] " + message + " failed: " + e);
                body = Json.Write(Api.Base());
            }
            Debug.Log("[GAOffline] " + message + " -> " + body.Length + " bytes");
            return mgr.Begin(Serve(body, "json"));
        }

        public static string MapUrl(string url)
        {
            if (url == null) return null;
            int h = url.IndexOf(Host + "assets/", StringComparison.Ordinal);
            if (h < 0) return url;
            string name = url.Substring(h + Host.Length + "assets/".Length);
            int q = name.IndexOf('?');
            if (q >= 0) name = name.Substring(0, q);
            string text = GameData.Resource(name);
            if (text == null)
            {
                Debug.LogWarning("[GAOffline] no local copy of assets/" + name);
                return url;
            }
            Debug.Log("[GAOffline] assets/" + name + " served locally");
            return Serve(text, "xml");
        }

        static string Serve(string body, string ext)
        {
            int n = counter++ % 64;
            string path = Path.Combine(Dir, "r" + n + "." + ext);
            File.WriteAllText(path, body, new UTF8Encoding(false));
            return "file://" + path;
        }

        static string Query(string url, string key)
        {
            int q = url.IndexOf('?');
            if (q < 0) return null;
            foreach (string kv in url.Substring(q + 1).Split('&'))
            {
                int e = kv.IndexOf('=');
                if (e > 0 && kv.Substring(0, e) == key) return WWW.UnEscapeURL(kv.Substring(e + 1));
            }
            return null;
        }

        static Dictionary<string, string> ParseForm(byte[] data)
        {
            var d = new Dictionary<string, string>();
            if (data == null) return d;
            foreach (string kv in Encoding.UTF8.GetString(data).Split('&'))
            {
                int e = kv.IndexOf('=');
                if (e <= 0) continue;
                d[WWW.UnEscapeURL(kv.Substring(0, e).Replace('+', ' '))] = WWW.UnEscapeURL(kv.Substring(e + 1).Replace('+', ' '));
            }
            return d;
        }
    }
}
