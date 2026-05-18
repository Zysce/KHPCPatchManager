using System.Collections.Generic;
using System.IO;

namespace KHPCPatchManager.OpenKH.Egs;

internal class IMD
{
    public List<int> Offsets = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public IMD(byte[] AssetData, int AssetOffset = 0)
    {
        using MemoryStream ms = new MemoryStream(AssetData);

        var magic = ms.ReadInt32();
        if (magic != 1145523529 && AssetOffset == 0) //IMGD
        {
            Invalid = true;
            return;
        }

        TextureCount = 1; //IMDs are always single images
        AssetCount = 1;
        ms.ReadInt32(); //always 256(?)
        int IMDoffset = ms.ReadInt32(); //offset for image data
        Offsets.Add(AssetOffset + IMDoffset + 0x20000000);
    }
}
