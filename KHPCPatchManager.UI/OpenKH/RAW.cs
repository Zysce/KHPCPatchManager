using System.Collections.Generic;
using System.IO;
using System;

namespace OpenKh.Egs;

internal class RAW
{
    public List<int> Offsets = new List<int>();
    public List<string> NamesAudio = new List<string>();
    public int TextureCount = 0;
    public int AssetCount = 0;
    public bool Invalid = false;

    public RAW(byte[] AssetData, int AssetOffset = 0)
    {
        using MemoryStream ms = new MemoryStream(AssetData);

        int magic = ms.ReadInt32();
        if (magic != 0 && AssetOffset == 0) //0x00000000
        {
            Invalid = true;
            return;
        }

        ms.Seek(0x0c, SeekOrigin.Begin);
        TextureCount += ms.ReadInt32();
        AssetCount = TextureCount;

        ms.Seek(0x18, SeekOrigin.Begin);
        int GsinfoOff = ms.ReadInt32();
        int dataOffset = ms.ReadInt32();

        int diff = 0;
        for (int i = 0; i < TextureCount; i++)
        {
            int offset = AssetOffset + dataOffset + diff + (i * 0x10) + 0x20000000;
            Offsets.Add(offset);

            ms.Seek(GsinfoOff + 0x70 + (i * 0xA0), SeekOrigin.Begin);
            long Tex0Reg = ms.ReadInt64();

            uint PSM = (uint)(Tex0Reg >> 20) & 0x3fu;
            int width = (ushort)(1u << ((int)(Tex0Reg >> 26) & 0x0F));
            int height = (ushort)(1u << ((int)(Tex0Reg >> 30) & 0x0F));

            int bpp;
            bool div = false;
            switch (PSM)
            {
                case 0:
                case 1:
                case 27:
                case 26:
                case 48:
                case 49:
                    bpp = 4;
                    break;
                case 2:
                case 10:
                case 50:
                case 58:
                    bpp = 2;
                    break;
                case 19:
                case 44: //unsure about this one
                    bpp = 1;
                    break;
                case 20:
                    bpp = 2;
                    div = true;
                    break;
                default:
                    bpp = 1;
                    div = false;
                    Console.WriteLine("Warning: Unknown Pixel Storage Mode! PSM = " + PSM);
                    break;
            }
            if (!div)
                diff += (width * height) * bpp;
            else
                diff += (width * height) / bpp;
        }

        int index = Helpers.IndexOfByteArray(AssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), 0);

        while (index > -1)
        {
            ms.Seek(index + 0x0a, SeekOrigin.Begin);
            int imageToApplyTo = (int)ms.ReadInt16();

            ms.Seek(0x1c, SeekOrigin.Current);
            int texaOffset = ms.ReadInt32();
            int offset = index + texaOffset + 0x08 + (imageToApplyTo * 0x10) + 0x20000000;
            Offsets.Add(AssetOffset + offset);

            TextureCount++;
            AssetCount++;
            index = Helpers.IndexOfByteArray(AssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), index + 1);
        }
    }
}
