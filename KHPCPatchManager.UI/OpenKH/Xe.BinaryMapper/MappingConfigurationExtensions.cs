using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Xe.BinaryMapper;

public static class MappingConfigurationExtensions
{
    public static MappingConfiguration ForType<T>(
        this MappingConfiguration configuration,
        Func<MappingReadArgs, object> reader,
        Action<MappingWriteArgs> writer) =>
        ForType(configuration, typeof(T), reader, writer);

    public static MappingConfiguration ForType(
        this MappingConfiguration configuration, Type type,
        Func<MappingReadArgs, object> reader,
        Action<MappingWriteArgs> writer)
    {
        if (configuration.Mappings == null)
            configuration.Mappings = new Dictionary<Type, MappingDefinition>();

        configuration.Mappings[type] = new MappingDefinition
        {
            Reader = reader,
            Writer = writer
        };
        return configuration;
    }

    public static MappingConfiguration UseMemberForLength<T>(
        this MappingConfiguration configuration, string memberName,
        Func<T, string, int> getLengthFunc)
        where T : class
    {
        var classType = typeof(T);
        var classMappings = new Dictionary<string, Func<object, int>>();
        if (!configuration.MemberMappings.TryGetValue(classType, out classMappings))
        {
            classMappings = new Dictionary<string, Func<object, int>>();
            configuration.MemberMappings.Add(classType, classMappings);
        }

        classMappings[memberName] = o => getLengthFunc((T)o, memberName);
        return configuration;
    }

    public static IBinaryMapping Build(this MappingConfiguration configuration) =>
        new RealBinaryMapping(configuration);
}
