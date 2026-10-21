#!/usr/bin/env python3
"""Source-level Windows port patches for the AssetRipper export of Blastron 2.3.0.
Usage: pc_patch.py <ExportedProject/Assets> <offline-src-dir>
Each edit asserts its anchor exists exactly once, so a changed export fails loudly."""
import os, sys, shutil, re

A = sys.argv[1]; OFF = sys.argv[2]
FP = os.path.join(A, "Plugins/Assembly-CSharp-firstpass"); CS = os.path.join(A, "Scripts/Assembly-CSharp")
ANDROID = "UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android"

def edit(path, old, new, count=1):
    p = os.path.join(A, path) if not os.path.isabs(path) else path
    s = open(p, encoding="utf-8-sig").read()
    n = s.count(old)
    assert n == count, f"{path}: expected {count} of {old!r}, found {n}"
    open(p, "w", encoding="utf-8").write(s.replace(old, new))

# --- decompiler output that does not compile ---
edit(FP + "/Poly2Tri/Triangulation/Util/FixedBitArray3.cs",
     "return System_002ECollections_002EGeneric_002EIEnumerable_003Cbool_003E_002EGetEnumerator();",
     "return ((IEnumerable<bool>)this).GetEnumerator();")
edit(FP + "/EB/Sparx/WalletManager.cs",
     "new Action(this, (IntPtr)__ldftn(WalletManager.FetchOffers))", "delegate { FetchOffers(false); }")
# EB.Action vs System.Action (System.Core) ambiguity; the game's types are EB.Action-compatible void() delegates
for f, n in [("BigArrow", 2), ("ClippedProgressBar", 2), ("RangeWeapon", 2), ("MatchmakingGameModeSelectUI", 1)]:
    p = os.path.join(CS, f + ".cs"); s = open(p, encoding="utf-8-sig").read()
    s2, k = re.subn(r"(?<![\w.<])Action(?=[ )])(?!<)", "EB.Action", s)
    assert k == n, (f, k); open(p, "w", encoding="utf-8").write(s2)
edit(CS + "/ReplicatedSequence.cs", "new Action<uint, object, string>(", "new EB.Action<uint, object, string>(")

# --- offline server (same code as the Android build) ---
# lives in Assembly-CSharp because it reads the game's item tables; firstpass reaches it by reflection
dst = os.path.join(CS, "Offline"); os.makedirs(dst, exist_ok=True)
for f in ("LocalEndPoint.cs", "OfflineCatalog.cs", "LocalProvider.cs"):
    shutil.copy(os.path.join(OFF, f), dst)
edit(CS + "/Offline/LocalProvider.cs", 'public string Name => "offline";',
     'public string Name\n\t\t{\n\t\t\tget { return "offline"; }\n\t\t}')
edit(FP + "/EB/Sparx/EndPointFactory.cs", "\t\t\tUri uri = new Uri(url);\n",
     "\t\t\t// Offline build: the Sparx servers are gone, answer locally.\n\t\t\treturn (EndPoint)Activator.CreateInstance(Type.GetType(\"EB.Sparx.LocalEndPoint, Assembly-CSharp\", true), url, options);\n\t\t\tUri uri = new Uri(url);\n")
edit(FP + "/EB/IAP/Internal/ProviderFactory.cs", "return GoogleProvider.Create(config);", "return (Provider)System.Activator.CreateInstance(System.Type.GetType(\"EB.Sparx.LocalProvider, Assembly-CSharp\", true), config);")
edit(FP + "/EB/Sparx/ObbManager.cs", "\t\tpublic Coroutine Check()\n\t\t{\n\t\t\treturn Coroutines.Run(DoCheck());",
     "\t\tpublic Coroutine Check()\n\t\t{\n\t\t\t// Content packs ship with the game; there is no OBB to fetch.\n\t\t\tIsMounted = true;\n\t\t\treturn null;")

# --- Android-only plugins: no-ops on PC ---
edit(FP + "/GooglePlayDownloader.cs", "\tpublic static bool RunningOnAndroid()\n\t{\n",
     f"\tpublic static bool RunningOnAndroid()\n\t{{\n\t\tif (!({ANDROID}))\n\t\t{{\n\t\t\treturn false;\n\t\t}}\n")
edit(FP + "/ChartboostPlugin.cs", "\t\tif (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSignature))",
     f"\t\tif (!({ANDROID}) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSignature))")
for m in ("CacheInterstitial", "ShowInterstitial(string location", "ShowMoreApps", "CacheMoreApps"):
    p = os.path.join(FP, "ChartboostPlugin.cs"); s = open(p, encoding="utf-8-sig").read()
    i = s.index("public void " + m); j = s.index("{", i) + 1
    s = s[:j] + "\n\t\tif (_chartboost_wrapper == null)\n\t\t{\n\t\t\treturn;\n\t\t}" + s[j:]
    open(p, "w", encoding="utf-8").write(s)
edit(FP + "/MobileAppTrackerPlugin.cs", "\t\tif (_initialized || string.IsNullOrEmpty(AdvertiserId)",
     f"\t\tif (!({ANDROID}) || _initialized || string.IsNullOrEmpty(AdvertiserId)")
edit(FP + "/EB/Sparx/Protect.cs", "\t\t\tif (!string.IsNullOrEmpty(_sha1))\n",
     f"\t\t\tif (!({ANDROID}))\n\t\t\t{{\n\t\t\t\t_sha1 = \"offline\";\n\t\t\t}}\n\t\t\tif (!string.IsNullOrEmpty(_sha1))\n")
edit(FP + "/EB/Sparx/Protect.cs", "\t\t\tstring chal = string.Empty;\n",
     f"\t\t\tstring chal = string.Empty;\n\t\t\tif (!({ANDROID}))\n\t\t\t{{\n\t\t\t\tcb(\"offline\");\n\t\t\t\tyield break;\n\t\t\t}}\n")
edit(FP + "/EB/Version.cs", "\t\t\tcase SystemLanguage.Chinese:\n\t\t\t{\n",
     f"\t\t\tcase SystemLanguage.Chinese:\n\t\t\t{{\n\t\t\t\tif (!({ANDROID}))\n\t\t\t\t{{\n\t\t\t\t\treturn Localizer.GetLanguageCode(Language.ChineseSimplified);\n\t\t\t\t}}\n")
edit(FP + "/EB/Version.cs", "\t\tpublic static string GetCountryCode()\n\t\t{\n",
     f"\t\tpublic static string GetCountryCode()\n\t\t{{\n\t\t\tif (!({ANDROID}))\n\t\t\t{{\n\t\t\t\treturn \"US\";\n\t\t\t}}\n")
for m in ("GetAndroidDeviceID", "GetMACAddress"):
    edit(FP + "/EB/Version.cs", f"\t\tpublic static string {m}()\n\t\t{{\n\t\t\tstring result = string.Empty;\n",
         f"\t\tpublic static string {m}()\n\t\t{{\n\t\t\tstring result = string.Empty;\n\t\t\tif (!({ANDROID}))\n\t\t\t{{\n\t\t\t\treturn UnityEngine.SystemInfo.deviceUniqueIdentifier;\n\t\t\t}}\n")
# web views (support/offer pages) need the Android activity; skip them on PC
edit(CS + "/HomeUI.cs", '\t\tAndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");\n\t\tif (androidJavaClass != null)\n',
     f'\t\tAndroidJavaClass androidJavaClass = ({ANDROID}) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;\n\t\tif (androidJavaClass != null)\n')
edit(CS + "/WebViewDialogUI.cs", '\t\tAndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");\n\t\tif (androidJavaClass != null)\n',
     f'\t\tAndroidJavaClass androidJavaClass = ({ANDROID}) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;\n\t\tif (androidJavaClass != null)\n', count=3)

# Name entry used a native Android keyboard activity. On PC, show a text box instead.
edit(CS + "/KeyboardUI.cs", "\tprivate void Awake()\n\t{\n",
     f"\tprivate bool _pcOpen;\n\n\tprivate string _pcText = string.Empty;\n\n\tprivate void Awake()\n\t{{\n\t\tif (!({ANDROID}))\n\t\t{{\n\t\t\treturn;\n\t\t}}\n")
edit(CS + "/KeyboardUI.cs", '\tpublic void DisplayKeyboard()\n\t{\n',
     f'\tpublic void DisplayKeyboard()\n\t{{\n\t\tif (_androidClass == null)\n\t\t{{\n\t\t\t_pcOpen = true;\n\t\t\treturn;\n\t\t}}\n')
edit(CS + "/KeyboardUI.cs", "\tpublic void HideKeyboard()\n\t{\n", """\tprivate void OnGUI()
\t{
\t\tif (!_pcOpen)
\t\t{
\t\t\treturn;
\t\t}
\t\tfloat w = Mathf.Min(600f, Screen.width - 40f);
\t\tRect r = new Rect((Screen.width - w) / 2f, Screen.height * 0.25f, w, 40f);
\t\tGUI.SetNextControlName("pc_keyboard");
\t\t_pcText = GUI.TextField(r, _pcText, _maxStringLength);
\t\tGUI.FocusControl("pc_keyboard");
\t\tOnKeyboardTextChangeClicked(_pcText);
\t\tbool enter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
\t\tif (enter || GUI.Button(new Rect(r.x + w - 120f, r.y + 48f, 120f, 36f), "OK"))
\t\t{
\t\t\t_pcOpen = false;
\t\t\tOnKeyboardSendClicked(_pcText);
\t\t\tOnKeyboardHidden(_pcText);
\t\t}
\t}

\tpublic void HideKeyboard()
\t{
\t\t_pcOpen = false;
""")
print("ok")

# --- shaders AssetRipper could not decompile (GLES-only programs) ---
# Replacements are hand ports of the original GLES code, in port/shaders/. Same file, so the .meta guid
# and every material reference stay intact.
SH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "shaders")
repl = {}
for f in os.listdir(SH):
    src = open(os.path.join(SH, f)).read()
    repl[src.split("\n", 1)[0].strip()] = src
done = 0
shdir = os.path.join(A, "Shader")
for f in sorted(os.listdir(shdir)):
    if not f.endswith(".shader"):
        continue
    p = os.path.join(shdir, f); s = open(p, encoding="utf-8-sig").read()
    if "DummyShaderTextExporter" not in s:
        continue
    head = s.split("\n", 1)[0].strip()
    assert head in repl, f"no replacement for {f}: {head}"
    open(p, "w", encoding="utf-8").write(repl[head]); done += 1
print("shaders replaced:", done)

# AssetRipper adds .NET Standard facades for its decompiler; Unity 4's Mono can't load them and no script needs them
for f in ("System.Runtime.CompilerServices.Unsafe.dll", "System.Runtime.InteropServices.dll"):
    for ext in ("", ".meta"):
        p = os.path.join(A, "Plugins", f + ext)
        if os.path.exists(p):
            os.remove(p)
print("ok")
