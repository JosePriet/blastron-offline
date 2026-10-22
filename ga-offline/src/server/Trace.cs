using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SynCore;
using SynCore.Data;
using UnityEngine;

namespace GAOffline
{
    // Boot diagnostics. AbstractManagers.Awake is patched to call Install() first. Every Unity log message is copied to
    // logcat (tag GAOffline, via android.util.Log) and to ga_offline/log.txt, and every 5 s a heartbeat records the
    // scene and which managers have not started yet, so a stalled boot shows where it stopped.
    public static class Trace
    {
        static bool installed;
        static StreamWriter file;
        static AndroidJavaClass droidLog;

        public static void Install()
        {
            if (installed) return;
            installed = true;
            try { droidLog = new AndroidJavaClass("android.util.Log"); } catch (Exception) { }
            try
            {
                file = new StreamWriter(Path.Combine(LocalServer.Dir, "log.txt"), true);
                file.AutoFlush = true;
            }
            catch (Exception) { }
            Application.RegisterLogCallback(OnLog);
            var go = new GameObject("GAOfflineTrace");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Beat>();
            Write("I", "boot " + Api.Version + ", level " + Application.loadedLevelName + ", data " + Application.persistentDataPath);
        }

        static string last;
        static int repeats;

        static void OnLog(string message, string stack, LogType type)
        {
            string key = type + message + stack;
            if (key == last) { repeats++; return; } // per-frame exceptions: one copy, then a count
            if (repeats > 0) Write("I", "(previous message repeated " + repeats + " more times)");
            last = key;
            repeats = 0;
            if (message != null && message.StartsWith("[GAOffline]", StringComparison.Ordinal) && type == LogType.Log) { Write("I", message); return; }
            Write(type == LogType.Log ? "I" : type == LogType.Warning ? "W" : "E",
                  type + ": " + message + (type == LogType.Exception || type == LogType.Error ? "\n" + stack : ""));
        }

        public static void Write(string level, string line)
        {
            try { if (droidLog != null) droidLog.CallStatic<int>(level == "E" ? "e" : level == "W" ? "w" : "i", "GAOffline", line); } catch (Exception) { }
            try { if (file != null) file.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + level + " " + line); } catch (Exception) { }
        }

        class Beat : MonoBehaviour
        {
            static readonly FieldInfo All = typeof(AbstractManagers).GetField("allManagers", BindingFlags.Instance | BindingFlags.NonPublic);
            static readonly FieldInfo Inst = typeof(AbstractManagers).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            float next;
            int beats;

            void Update()
            {
                if (Time.realtimeSinceStartup < next || beats >= 60) return;
                next = Time.realtimeSinceStartup + 5f;
                beats++;
                var waiting = new List<string>();
                try
                {
                    var inst = Inst == null ? null : Inst.GetValue(null);
                    var list = inst == null ? null : All.GetValue(inst) as List<IManager>;
                    if (list != null)
                        foreach (var m in list)
                            if (m.Status != TManagerStatus.Started) waiting.Add(m.GetType().Name + "=" + m.Status);
                }
                catch (Exception e) { waiting.Add("? " + e.Message); }
                Write("I", "beat t=" + (int)Time.realtimeSinceStartup + " frame=" + Time.frameCount + " level=" + Application.loadedLevelName
                           + " ready=" + AbstractManagers.IsReady + " not started: " + string.Join(", ", waiting.ToArray()));
            }
        }
    }
}
