using System;
using System.Collections.Generic;
using System.IO;

namespace KHPCPatchManager.OpenKH.Egs;

internal class PAX
{
    public List<int> Offsets = new List<int>();
    public SortedDictionary<int, int> TempOffsets = new SortedDictionary<int, int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public PAX(byte[] AssetData, int AssetOffset = 0)
    {
        using MemoryStream ms = new MemoryStream(AssetData);

        var magic = ms.ReadInt32();
        if (magic != 1599619408 && AssetOffset == 0) //PAX_
        {
            Invalid = true;
            return;
        }

        ms.ReadInt64(); //we just skip these 8 bytes. unsure what they are for.

        var Dpxoffset = ms.ReadInt32();
        ms.Seek(Dpxoffset + 0xC, SeekOrigin.Begin);

        var Unk1Count = ms.ReadInt32(); //unsure what this block of data is for. we seem to not need it though.
        ms.Seek(Unk1Count * 0x20, SeekOrigin.Current); //so skip it to get to the part we actually need.

        var DpdCount = ms.ReadInt32();
        var DpdOffsets = ((int)ms.Position); //the DPDs are what have our textures so save the position of this area.

        for (int d = 0; d < DpdCount; d++)
        {
            ms.Seek(DpdOffsets + (d * 0x4), SeekOrigin.Begin);

            var DpdOffset = ms.ReadInt32();
            ms.Seek(Dpxoffset + DpdOffset, SeekOrigin.Begin);

            ms.ReadInt32(); //unknown

            var Unk2Count = ms.ReadInt32(); //don't know this block of data, so skip it to get what me need
            ms.Seek(Unk2Count * 0x4, SeekOrigin.Current);

            var DpdTexCount = ms.ReadInt32(); //finally found the texture offsets
            var DpdTexOffsets = ((int)ms.Position); //save this position

            for (int t = 0; t < DpdTexCount; t++)
            {
                ms.Seek(DpdTexOffsets + (t * 0x4), SeekOrigin.Begin);
                var DpdTexOffset = ms.ReadInt32();

                ms.Seek(Dpxoffset + DpdOffset + DpdTexOffset, SeekOrigin.Begin);
                int value1 = ms.ReadInt32(); //use this as a key in  the dictionary
                ms.ReadInt32();
                int value2 = ms.ReadInt32(); //this value seems to define if a texture is new

                if (value2 == 0)
                {
                    TextureCount++;
                    AssetCount++;
                    int finaloffset = AssetOffset + Dpxoffset + DpdOffset + (DpdTexOffset + 0x20) + 0x20000000;

                    //check to see if our key already exists
                    if (!TempOffsets.ContainsKey(value1))
                    {
                        //if it doesn't then add it as normal
                        TempOffsets.Add(value1, finaloffset);
                    }
                    else
                    {
                        //if it does then we need to increase the offset by 1 for the original value
                        TempOffsets[value1] += 1;
                        //then use that new value + 1 as our new offset for the duplicate key then add t to our key so that it can actually be added.
                        TempOffsets.Add(value1 + t, (TempOffsets[value1] + 1));
                    }
                }
            }
            //Add our current list of offsets from the dpd to our main ffsets list
            Offsets.AddRange(TempOffsets.Values);
            //then clear the temp list for the next dpdF
            TempOffsets.Clear();
        }
    }
}
