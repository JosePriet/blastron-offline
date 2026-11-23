// The shipped firstpass assembly supplies the JSON codec, request/response types,
// EB.Dot, the IAP types and the Sparx delegate types. Replace engine-only services.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class Application { public static string persistentDataPath; }
    public static class Debug
    {
        public static readonly List<string> Messages = new List<string>();
        public static void Log(string message) => Messages.Add(message);
        public static void LogError(string message) => Messages.Add(message);
    }
}
namespace EB
{
    public static class Time { public static int Now { get; set; } }
    public static class Coroutines { public static void NextFrame(System.Action callback) => callback(); }
}
namespace EB.Sparx
{
    public class Hub
    {
        public static readonly Hub Instance = new Hub();
        public readonly Settings Config = new Settings();
        public class Settings
        {
            public bool UsePush = true;
            public readonly PingSettings GameManagerConfig = new PingSettings();
        }
        public class PingSettings { public bool PingEnabled = true; }
    }
    public abstract class EndPoint
    {
        protected EndPoint(string url, EndPointOptions options) {}
        public abstract void AddData(string key, string value);
        public abstract string GetData(string key);
        public abstract void RPC(string name, ArrayList args, Action<string, object> callback);
        public abstract int Service(Request request, Action<Response> callback);
        public abstract bool IsDone(int id);
        public abstract void Update();
    }
}
public class InventoryItem { public enum Category { Weapon, Boost } }
public static class InventoryDB
{
    public class MetaData
    {
        public bool isHidden, isDefault;
        public int softCost, hardCost, levelRequired, quantity;
        public string name;
    }
    public static readonly List<MetaData> Weapons = new List<MetaData>();
    public static List<MetaData> GetCategoryList(InventoryItem.Category category) =>
        category == InventoryItem.Category.Weapon ? Weapons : new List<MetaData>();
}
public static class CharacterDB
{
    public enum Category { kHead, kTorso, kLegs }
    public class MetaData { public string name; public bool isHidden, isDefault; public int unlockLevel; }
    public static List<MetaData> GetCategory(Category category) => new List<MetaData>();
}
