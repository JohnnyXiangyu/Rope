using Rope.Abstractions.CSharpAttributes;
using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;
using RopeCSharp.Exceptions;
using RopeCSharp.Extensions;
using RopeCSharp.Serialization;
using System.Collections.Immutable;
using System.Reflection;

namespace RopeCSharp;
public class Service
{
    public static string SerializeScript(RopeScript script, Dictionary<string, ContextType> contextTypes)
    {
        SerializationContext serializationContext = new(contextTypes);
        script.Serialize(serializationContext);
        return serializationContext.ToString();
    }

    public static DataBase LoadAssembly(string path)
    {
        Assembly assembly = Assembly.LoadFrom(path);
        return LoadAssembly(assembly);
    }

    public static DataBase LoadAssembly(Assembly assembly)
    {
        // build a set of supported types
        Type[] allowedLiteralTypes = [
            typeof(string),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(bool)
        ];
        ImmutableHashSet<Type> allowedLiteralTypeNames = allowedLiteralTypes.ToImmutableHashSet();

        // construct a dictionary of all context information
        Dictionary<string, ContextType> contextDict = assembly
            .GetTypes()
            .Where(type => type.CustomAttributes.Any(attr => attr.AttributeType == typeof(ContextTypeAttribute)))
            .Select(contextType =>
                new ContextType()
                {
                    Name = contextType.Name,
                    ModuleReq = contextType.Namespace,
                    Actions = contextType
                    .GetMethods()
                    .Where(method => method.CustomAttributes
                    .Any(attr => attr.AttributeType == typeof(ContextActionAttribute)))
                    .Where(method => !method.IsStatic)
                    .Select(contextMethod =>
                        new ContextAction()
                        {
                            Name = contextMethod.Name,
                            Params = contextMethod
                                .GetParameters()
                                .Where(methodParameter => allowedLiteralTypeNames.Contains(methodParameter.ParameterType))
                                .Select(methodParameter =>
                                    new Parameter()
                                    {
                                        Name = methodParameter.Name,
                                        Type = GetLiteralType(methodParameter.ParameterType)
                                    })
                                .ToArray()
                        })
                    .Select(action => (action.Name, action))
                    .ToDictionary()
                })
            .Select(context => (context.Name, context))
            .ToDictionary();

        return new DataBase() { ContextTypes = contextDict };
    }

    private static LiteralType GetLiteralType(Type? type)
    {
        if (type == typeof(string))
        {
            return LiteralType.String;
        }
        else if (type == typeof(int))
        {
            return LiteralType.Int32;
        }
        else if (type == typeof(long))
        {
            return LiteralType.Int64;
        }
        else if (type == typeof(float))
        {
            return LiteralType.Float;
        }
        else if (type == typeof(double))
        {
            return LiteralType.Double;
        }
        else if (type == typeof(bool))
        {
            return LiteralType.Boolean;
        }
        else
        {
            throw new UnsupportedTypeException();
        }
    }
}
