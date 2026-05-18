using System.Collections.Generic;
using System.IO;

namespace OpenKh.Egs;

internal class TM2
{
    public List<int> Offsets = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public TM2(byte[] AssetData, int AssetOffset = 0)
    {
        using MemoryStream ms = new MemoryStream(AssetData);

        int magic = ms.ReadInt32();
        if (magic != 843925844 && AssetOffset == 0) //TIM2
        {
            Invalid = true;
            return;
        }

        ms.ReadInt16(); //format
        int texCount = ms.ReadInt16();
        ms.ReadInt64(); //unused
        int totalsize = 0;

        for (int i = 0; i < texCount; i++)
        {
            ms.Seek(0x10 + totalsize, SeekOrigin.Begin);
            totalsize += ms.ReadInt32();

            if (i == 0 && totalsize == 0 && texCount > 1)
            {
                Invalid = true;
                return;
            }

            ms.ReadInt32(); //Clut size
            ms.ReadInt32(); //Image size
            int header = ms.ReadInt16(); //header size
            ms.Seek((header - 0x10) + 0x2, SeekOrigin.Current);
            int imageOffset = ((int)ms.Position);

            TextureCount += 1;
            AssetCount++;
            Offsets.Add(AssetOffset + 0x10 + imageOffset + 0x20000000);
        }
    }
}
