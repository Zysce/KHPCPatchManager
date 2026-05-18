using System.Collections.Generic;
using System.IO;

namespace KHPCPatchManager.OpenKH.Egs;

internal class SDasset
{
    public List<int> Offsets = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int AssetCount = 0;
    public int TextureCount = 0;
    public bool Invalid = true;

    public SDasset(string name, byte[] originalAssetData, bool remasterpathtrue)
    {
        dynamic asset = null;
        switch (Path.GetExtension(name), remasterpathtrue)
        {
            case (".2dd", true):
            case (".2ld", true):
            case (".bar", true):
            case (".bin", true):
            case (".mag", true):
            case (".map", true):
            case (".mdlx", true):
                asset = new BAR(originalAssetData);
                break;
            case (".imd", true):
                asset = new IMD(originalAssetData);
                break;
            case (".imz", true):
                asset = new IMZ(originalAssetData);
                break;
            case (".pax", true):
                asset = new PAX(originalAssetData);
                break;
            case (".tm2", true):
                asset = new TM2(originalAssetData);
                break;
        }
        switch (".a" + (Path.GetExtension(name)), remasterpathtrue)
        {
            case (".a.fm", true):
            case (".a.fr", true):
            case (".a.gr", true):
            case (".a.it", true):
            case (".a.sp", true):
            case (".a.us", true):
            case (".a.uk", true):
            case (".a.jp", true):
                asset = new BAR(originalAssetData);
                break;
        }

        if (asset != null && !asset.Invalid)
        {
            Offsets = asset.Offsets;
            TextureCount = asset.TextureCount;
            AssetCount = asset.AssetCount;
            NamesAudio = asset.NamesAudio;
            Invalid = false;
        }
    }
}
