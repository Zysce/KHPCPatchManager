using System;

namespace Xe.BinaryMapper;

public class MappingDefinition
{
    public Func<MappingReadArgs, object> Reader { get; set; }

    public Action<MappingWriteArgs> Writer { get; set; }
}
