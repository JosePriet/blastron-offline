#!/bin/bash
# cc.sh fp|cs : compile exported scripts the way the Unity 4 editor would (full .NET 2.0/3.5 profile, C# 4 subset)
S=/tmp/claude-0/-home-claude/31a90881-e3dc-5d7d-ad62-1e255f8ff816/scratchpad
M=$S/bl/apk/assets/bin/Data/Managed; N=$S/net20/build/.NETFramework/v2.0; N35=$S/net20/n35/build/.NETFramework/v3.5
E=${EXPORT:-$S/arout/ExportedProject/Assets}
CSC="dotnet /usr/lib/dotnet/sdk/8.0.132/Roslyn/bincore/csc.dll -nologo -nostdlib -noconfig -unsafe -target:library -langversion:4 -nowarn:0618,0108,0114,0162,0168,0219,0414,0649,0169,0109,0067,1635,0429,0252,0253,0659,0661,0660,0628"
REF="-r:$N/mscorlib.dll -r:$N/System.dll -r:$N35/System.Core.dll -r:$N/System.Xml.dll -r:$M/UnityEngine.dll -r:$M/crypto.dll -r:$E/Plugins/HOTween.dll -r:$E/Plugins/SmoothMoves_Runtime.dll"
mkdir -p $S/pc/bin
if [ "$1" = fp ]; then
  $CSC $REF -out:$S/pc/bin/Assembly-CSharp-firstpass.dll -recurse:"$E/Plugins/*.cs"
else
  $CSC $REF -r:$S/pc/bin/Assembly-CSharp-firstpass.dll -out:$S/pc/bin/Assembly-CSharp.dll -recurse:"$E/Scripts/*.cs"
fi
