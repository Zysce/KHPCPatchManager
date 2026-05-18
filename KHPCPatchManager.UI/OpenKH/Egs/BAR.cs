using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Egs;

internal class BAR
{
    public List<int> Offsets = new List<int>();
    public List<int> OffsetsTIM = new List<int>();
    public List<int> OffsetsPAX = new List<int>();
    public List<int> OffsetsTM2 = new List<int>();
    public List<int> OffsetsAudio = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public BAR(byte[] AssetData, int AssetOffset = 0)
    {
        dynamic subasset;

        using MemoryStream ms = new MemoryStream(AssetData);

        int type;
        int offset;
        int subsize;
        string magic;
        byte[] subfile;

        if (AssetOffset == 0)
        {
            magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
            if (magic != "BAR") //BAR
            {
                Invalid = true;
                return;
            }
            ms.ReadBytes(1);
        }
        else
        {
            ms.ReadInt32(); //magic
        }

        int count = ms.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            ms.Seek(0x10 + (i * 0x10), SeekOrigin.Begin);

            type = ms.ReadInt32(); //subasset type
            ms.ReadInt32(); //subasset name
            offset = ms.ReadInt32(); //subasset offset
            subsize = ms.ReadInt32(); //subasset size

            ms.Seek(offset, SeekOrigin.Begin);

            switch (type)
            {
                case 7: //RAW Image
                    int rawmagic = ms.ReadInt32();
                    if (rawmagic == 0)
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new RAW(subfile, offset);

                        TextureCount += subasset.TextureCount;
                        AssetCount += subasset.TextureCount;
                        OffsetsTIM.AddRange(subasset.Offsets);
                    }
                    break;
                case 10: //TIM2
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                    if (magic == "TIM2")
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new TM2(subfile, offset);

                        TextureCount += subasset.TextureCount;
                        AssetCount += subasset.TextureCount;
                        OffsetsTM2.AddRange(subasset.Offsets);
                    }
                    break;
                case 18: //PAX
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
                    if (magic == "PAX")
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new PAX(subfile, offset);

                        TextureCount += subasset.TextureCount;
                        AssetCount += subasset.TextureCount;
                        OffsetsPAX.AddRange(subasset.Offsets);
                    }
                    break;
                case 24: //IMD
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                    if (magic == "IMGD")
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new IMD(subfile, offset);

                        TextureCount += subasset.TextureCount;
                        AssetCount += subasset.TextureCount;
                        Offsets.AddRange(subasset.Offsets);
                    }
                    break;
                case 29: //IMZ                           
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                    if (magic == "IMGZ")
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new IMZ(subfile, offset);

                        TextureCount += subasset.TextureCount;
                        AssetCount += subasset.TextureCount;
                        Offsets.AddRange(subasset.Offsets);
                    }
                    break;
                case 31: //Sound Effects
                case 34: //Voice Audio
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(6));
                    if (magic == "ORIGIN")
                    {
                        ms.ReadBytes(10);
                        string name = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(32)).TrimEnd('\0');
                        NamesAudio.Add(name);

                        AssetCount += 1;
                        OffsetsAudio.Add(-1);
                    }
                    break;
                case 36: //raw bitmap
                    TextureCount += 1;
                    AssetCount++;
                    Offsets.Add(offset + 0x20000000);
                    break;
                case 46: //BAR
                    magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
                    if (magic == "BAR")
                    {
                        ms.Seek(offset, SeekOrigin.Begin);
                        subfile = ms.ReadBytes(subsize);
                        subasset = new BAR(subfile, offset);

                        AssetCount += subasset.AssetCount;
                        Offsets.AddRange(subasset.Offsets);
                    }
                    break;
            }
        }

        OffsetsTIM.AddRange(OffsetsPAX);
        OffsetsTIM.AddRange(OffsetsTM2);
        OffsetsTIM.AddRange(Offsets);
        OffsetsTIM.AddRange(OffsetsAudio);
        Offsets = OffsetsTIM;

        if (TextureCount == 0)
        {
            Invalid = true;
            return;
        }
    }
}
