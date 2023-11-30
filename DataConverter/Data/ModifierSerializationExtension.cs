using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace DataConverter.Data;

public static class ModifierSerializationExtension
{
    public static DefaultJsonTypeInfoResolver Exclude(this DefaultJsonTypeInfoResolver resolver, Type type, params string[] membersToExclude)
    {
        if (resolver == null || membersToExclude == null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        var membersToExcludeSet = membersToExclude.ToHashSet();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Kind == JsonTypeInfoKind.Object && type.IsAssignableFrom(typeInfo.Type)) // Or type == typeInfo.Type if you don't want to exclude from subtypes
            {
                foreach (var property in typeInfo.Properties)
                {
                    if (property.GetMemberName() is { } name && membersToExcludeSet.Contains(name))
                    {
                        property.ShouldSerialize = static (obj, value) => false;
                    }
                }
            }
        });
        return resolver;
    }

    public static string? GetMemberName(this JsonPropertyInfo property) => (property.AttributeProvider as MemberInfo)?.Name;
}
