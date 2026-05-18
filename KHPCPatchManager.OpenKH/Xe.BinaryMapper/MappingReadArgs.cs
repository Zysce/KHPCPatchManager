using System.IO;

namespace KHPCPatchManager.OpenKH.Xe.BinaryMapper;

public class MappingReadArgs
{
    public BinaryReader Reader { get; set; }

    public DataAttribute DataAttribute { get; set; }

    public int Count { get; set; }

    public byte BitData { get; set; }

    public int BitIndex { get; set; }
}
