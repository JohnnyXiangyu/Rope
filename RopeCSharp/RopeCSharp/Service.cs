using Rope.Abstractions.CSharpAttributes;
using Rope.Abstractions.Reflection;
using RopeCSharp.Exceptions;
using System.Reflection;

namespace RopeCSharp;
public class Service
{
    public static DataBase LoadAssembly(string path)
    {
        Assembly assembly = Assembly.LoadFrom(path);
        return LoadAssembly(assembly);
    }

    public static DataBase LoadAssembly(Assembly assembly)
    {
        // build a set of supported types
        HashSet<string> allowedLiteralTypes = [
            typeof(string).Name,
            typeof(int).Name,
            typeof(long).Name,
            typeof(float).Name,
            typeof(double).Name,
            typeof(bool).Name
        ];
        Dictionary<string, DataConstructor> typeDict = assembly
            .GetTypes()
            .Where(type => type.CustomAttributes.Any(attr => attr.AttributeType == typeof(ValueTypeAttribute)))
            .Select(t => (t.Name, GetCustomType(t)))
            .ToDictionary();

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
                                .Select(methodParameter =>
                                    new Parameter()
                                    {
                                        Name = methodParameter.Name,
                                        DataConstructor = GetDataConstructorForParam(methodParameter, allowedLiteralTypes, typeDict)
                                    })
                                .ToArray()
                        })
                    .ToArray()
                })
            .Select(context => (context.Name, context)).ToDictionary();

        return new DataBase() { ContextTypes = contextDict, Constructors = typeDict };
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

    private static DataConstructor GetCustomType(Type type)
    {
        ConstructorInfo primeCtor;
        ConstructorInfo[] ctors = type.GetConstructors();
        if (ctors.Length == 1)
        {
            primeCtor = ctors[0];
        }
        else
        {
            primeCtor = ctors.First(ctor => ctor.CustomAttributes.Any(attr => attr.AttributeType == typeof(GraphConstructorAttribute)));
        }

        LiteralType[] literalParams = primeCtor.GetParameters().Select(param => GetLiteralType(param.ParameterType)).ToArray();
        DataConstructor valueType = new()
        {
            Name = type.Name,
            IsArray = false,
            Params = literalParams
        };

        return valueType;
    }

    private static DataConstructor GetArrayType(Type type)
    {
        LiteralType baseType = GetLiteralType(type.GetElementType());
        return new DataConstructor()
        {
            Name = type.Name,
            Params = [baseType],
            IsArray = true
        };
    }

    private static DataConstructor GetDataConstructorForParam(ParameterInfo methodParameter, HashSet<string> allowedLiteralTypes, Dictionary<string, DataConstructor> typeDict)
    {
        if (allowedLiteralTypes.Contains(methodParameter.ParameterType.Name))
        {
            LiteralType literal = GetLiteralType(methodParameter.ParameterType);
            return new DataConstructor()
            {
                Name = methodParameter.ParameterType.Name,
                IsArray = false,
                Params = [literal]
            };
        }
        else if (methodParameter.ParameterType.IsArray)
        {
            return GetArrayType(methodParameter.ParameterType);
        }
        else if (typeDict.TryGetValue(methodParameter.ParameterType.Name, out DataConstructor? ctype))
        {
            return ctype;
        }
        else
        {
            throw new UnsupportedTypeException();
        }
    }
}
