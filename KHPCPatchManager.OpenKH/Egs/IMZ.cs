using System.Collections.Generic;
using System.IO;

namespace KHPCPatchManager.OpenKH.Egs;

internal class IMZ
{
    public List<int> Offsets = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public IMZ(byte[] AssetData, int AssetOffset = 0)
    {
        using MemoryStream ms = new MemoryStream(AssetData);

        int magic = ms.ReadInt32();
        if (magic != 1514622281 && AssetOffset == 0)
        { //IMGZ
            Invalid = true;
            return;
        }

        ms.ReadInt64(); //unknown

        TextureCount = ms.ReadInt32();
        AssetCount = TextureCount;
        for (int i = 0; i < TextureCount; i++)
        {
            ms.Seek(0x10 + (i * 0x8), SeekOrigin.Begin);
            int IMDoffset = ms.ReadInt32(); //Offset for IMGD data
            ms.Seek(IMDoffset, SeekOrigin.Begin);

            magic = ms.ReadInt32();
            if (magic == 1145523529) //IMGD
            {
                ms.ReadInt32(); //always 256
                int ImageOffset = ms.ReadInt32(); //offset for image data
                Offsets.Add(AssetOffset + IMDoffset + ImageOffset + 0x20000000);
            }
        }
    }
}
