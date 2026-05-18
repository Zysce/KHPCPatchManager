using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KHPCPatchManager.OpenKH.Xe.BinaryMapper;

public class MappingConfiguration
{
    public Dictionary<Type, Dictionary<string, Func<object, int>>> MemberMappings { get; set; }

    public Dictionary<Type, MappingDefinition> Mappings { get; set; }

    public static MappingConfiguration DefaultConfiguration() =>
        DefaultConfiguration(Encoding.UTF8);

    public static MappingConfiguration DefaultConfiguration(Encoding encoding, bool isBigEndian = false) => new MappingConfiguration
    {
        Mappings = isBigEndian ? BinaryMapper.Mappings.BigEndianMapping(encoding) : BinaryMapper.Mappings.DefaultMapping(encoding),
        MemberMappings = new Dictionary<Type, Dictionary<string, Func<object, int>>>()
    };
}

