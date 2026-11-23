using System;
using System.Collections;
using System.IO;
using System.Reflection;
using EB.IAP;
using EB.Sparx;

internal static class Program
{
    private static LocalEndPoint endpoint;
    private static int failures;
    private static string Account => Path.Combine(UnityEngine.Application.persistentDataPath, "offline_server.json");

    private static void Restart()
    {
        typeof(LocalEndPoint).GetField("_state", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
        endpoint = new LocalEndPoint("http://offline.invalid", default(EndPointOptions));
    }

    private static Response Call(string path, bool post = false, Hashtable data = null)
    {
        var request = new Request("http://offline.invalid" + path, post);
        if (data != null)
            foreach (DictionaryEntry entry in data) request.AddData((string)entry.Key, entry.Value);
        Response response = null;
        int id = endpoint.Service(request, result => response = result);
        endpoint.Update();
        Require(endpoint.IsDone(id) && response != null, "request did not complete");
        return response;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static int Balance(Response response)
    {
        Require(response.sucessful, "wallet operation failed");
        return EB.Dot.Integer("balance", response.hashtable, -1);
    }

    private static void Run(string name, Action check)
    {
        string directory = Path.Combine(Path.GetTempPath(), "blastron-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        UnityEngine.Application.persistentDataPath = directory;
        try
        {
            Restart();
            check();
            Console.WriteLine("PASS " + name);
        }
        catch (Exception ex)
        {
            failures++;
            Console.WriteLine("FAIL " + name + " - " + ex.Message);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void SaveRecovery()
    {
        File.WriteAllText(Account, "{\"name\":\"Legacy\",\"ds\":{\"stage\":3},\"tokens\":{\"token_tutorial\":0}}");
        Restart();
        Require(EB.Dot.Integer("stage", Call("/ds/1/").hashtable, -1) == 3, "legacy progress not loaded");
        var delta = new Hashtable { { "value", 7 } };
        Require(Balance(Call("/wallet/credit", true, delta)) == 7, "first account write");
        Require(Balance(Call("/wallet/credit", true, delta)) == 14, "second write");
        Restart();
        Require(Balance(Call("/wallet/balance")) == 14, "restart lost latest write");
        Require(EB.Dot.String("user.name", Call("/kabam/guest", true).hashtable, "") == "Legacy", "legacy name lost");
        Require(EB.Dot.Integer("0.count", Call("/gacha/getTokens").arrayList, -1) == 0, "legacy token granted again");
        File.WriteAllText(Account, "{");
        Restart();
        Require(Balance(Call("/wallet/balance")) == 7, "previous account lost to a torn primary");
        Require(Balance(Call("/wallet/credit", true, delta)) == 14, "could not save after recovery");

        // Interrupt copy-back after committing a complete new snapshot.
        File.WriteAllText(Account + ".tmp", "{\"wallet\":{\"balance\":21}}");
        File.WriteAllText(Account + ".commit", "");
        File.WriteAllText(Account, "{");
        Restart();
        Require(Balance(Call("/wallet/balance")) == 21 && !File.Exists(Account + ".commit"), "committed snapshot lost");

        File.Delete(Account + ".tmp");
        Directory.CreateDirectory(Account + ".tmp");
        Require(!Call("/wallet/credit", true, delta).sucessful, "write failure reported success");
        Require(Balance(Call("/wallet/balance")) == 21, "failed write changed the wallet");
        Directory.Delete(Account + ".tmp");
        Require(Balance(Call("/wallet/credit", true, delta)) == 28, "retry counted twice");

        File.WriteAllText(Account, "{");
        File.WriteAllText(Account + ".bak", "{");
        Restart();
        Require(!Call("/wallet/credit", true, delta).sucessful && File.ReadAllText(Account) == "{",
            "unreadable existing account was overwritten");
    }

    private static void RequestPrivacy()
    {
        UnityEngine.Debug.Messages.Clear();
        var data = new Hashtable { { "name", "audit-placeholder" } };
        Require(Call("/kabam/name", true, data).sucessful, "rename failed");
        Require(!UnityEngine.Debug.Messages.Exists(line => line.Contains("audit-placeholder")), "request body logged");
    }

    private static void PayoutRetry()
    {
        Transaction transaction = null;
        var config = new EB.IAP.Config { Verify = result => transaction = result };
        var item = new Item { productId = "offline_gears_1", developerPayload = "offline@=>@1" };
        var provider = new LocalProvider(config);
        provider.PurchaseItem(item);
        var purchase = new Hashtable { { "payoutid", 1 }, { "response-data", transaction.payload } };
        var envelope = new Hashtable { { "data", purchase } };
        Require(Balance(Call("/store/verify-payout", true, envelope)) == 25, "first payout");
        Restart();
        Require(Balance(Call("/store/verify-payout", true, envelope)) == 25, "retry credited the same purchase again");
        purchase["payoutid"] = 2;
        Require(!Call("/store/verify-payout", true, envelope).sucessful, "receipt applied to a different pack");
        purchase["payoutid"] = 1;
        new LocalProvider(config).PurchaseItem(item);
        purchase["response-data"] = transaction.payload;
        Require(Balance(Call("/store/verify-payout", true, envelope)) == 50, "second legitimate purchase lost");
    }

    private static void SaveFragments()
    {
        var data = new Hashtable { { "data", new Hashtable { { "campaign", 3 }, { "profile", 7 } } } };
        Require(Call("/ds/1", true, data).sucessful, "save failed");
        Restart();
        var read = Call("/ds/1/campaign");
        Require(read.sucessful && read.hashtable.Count == 1 && EB.Dot.Integer("campaign", read.hashtable, -1) == 3,
            "keyed read returned unrelated progress");
        Require(Call("/ds/1/").hashtable.Count == 2 && Call("/ds/1/missing").hashtable.Count == 0, "full/missing read");
        Require(!Call("/ds/2", true, data).sucessful && !Call("/ds/2/profile").sucessful, "unrelated account shared progress");
    }

    private static void VaultStartup()
    {
        var group = new Hashtable { { "group", "tutorial" } };
        var pick = new Hashtable { { "group", "tutorial" }, { "box", "tutorial" }, { "payment", "token" } };
        Require(!Call("/gacha/getSet", false, group).sucessful, "empty catalog looked ready");
        Require(!Call("/gacha/pick", true, pick).sucessful, "empty pick succeeded");
        var early = Call("/store/payouts");
        Require(EB.Dot.Integer("offers.soft.0.value", early.hashtable, -1) == 1000, "early store fallback");
        InventoryDB.Weapons.Add(new InventoryDB.MetaData { name = "Example", softCost = 600, hardCost = 2, quantity = 2 });
        var ready = Call("/store/payouts");
        Require(EB.Dot.Integer("offers.soft.0.value", ready.hashtable, -1) == 3000, "early prices persisted");
        var set = Call("/gacha/getSet", false, group);
        Require(set.sucessful && EB.Dot.Array("boxes", set.hashtable, null).Count == 1, "catalog did not recover");
        Require(Call("/gacha/pick", true, pick).sucessful, "tutorial token lost during startup");
        Require(!Call("/gacha/pick", true, pick).sucessful, "token spent twice");
    }

    private static int Main()
    {
        Run("interrupted and failed saves", SaveRecovery);
        Run("account request logging", RequestPrivacy);
        Run("free gear verification retries", PayoutRetry);
        Run("account and fragment routing", SaveFragments);
        Run("vault metadata startup and retry", VaultStartup);
        return failures == 0 ? 0 : 1;
    }
}
