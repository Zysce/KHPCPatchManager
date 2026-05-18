using System.IO;

namespace Xe.BinaryMapper;

public class MappingWriteArgs
{
    public BinaryWriter Writer { get; set; }

    public object Item { get; set; }

    public DataAttribute DataAttribute { get; set; }

    public int Count { get; set; }

    public byte BitData { get; set; }

    public int BitIndex { get; set; }
}
