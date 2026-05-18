using OpenKh.Utils;
using System;
using System.IO;

namespace OpenKh.Egs;

internal class AssetConfig
{
    public bool SortOrder = true;
    public bool UpdateAssetCountFromOriginal = true;
    public bool UpdateAssetCountFromRemastered = true;
    public int ForceAssetCount = -1;

    public AssetConfig(string remasteredAssetsFolder)
    {
        string config = Path.Combine(remasteredAssetsFolder, "assets.config");

        if (ZipManager.FileExists(config))
        {
            string[] options = ZipManager.FileReadAllLines(config);
            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i].ToLower().Replace(" ", "");
                if (option.StartsWith("#")) continue;
                switch (option)
                {
                    case "sortorder=false":
                        SortOrder = false;
                        break;
                    case "updateassetcountfromoriginal=false":
                        UpdateAssetCountFromOriginal = false;
                        break;
                    case "updateassetcountfromremastered=false":
                        UpdateAssetCountFromRemastered = false;
                        break;
                }
                if (option.Contains("forceassetcount="))
                {
                    try
                    {
                        ForceAssetCount = Int32.Parse(option.Replace("forceassetcount=", ""));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Incorrect number format for ForceAssetCount: " + e.ToString());
                    }
                }
                if (option.Contains("forceassetorder="))
                {
                    string[] assets = option.Replace("forceassetcount=", "").Split(',');
                    for (int j = 0; j < assets.Length; j++)
                    {
                    }
                }
            }
        }
    }
}
