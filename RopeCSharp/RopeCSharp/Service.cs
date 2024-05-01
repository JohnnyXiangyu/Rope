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

        // build a set of supported types
        IEnumerable<Type> valueTypes = assembly.GetTypes().Where(type => type.CustomAttributes.Any(attr => attr.AttributeType == typeof(ValueTypeAttribute)));
        Dictionary<string, DataConstructor> typeDict = [];
        HashSet<string> allowedLiteralTypes = [
            typeof(string).Name,
            typeof(int).Name,
            typeof(long).Name,
            typeof(float).Name,
            typeof(double).Name,
            typeof(bool).Name
        ];
        foreach (Type type in valueTypes)
        {
            typeDict.Add(type.Name, GetCustomType(type));
        }
        
        // parse context type and actions
        IEnumerable<Type> contextTypes = assembly.GetTypes().Where(type => type.CustomAttributes.Any(attr => attr.AttributeType == typeof(ContextTypeAttribute)));
        ContextType[] contextTypesRef = contextTypes.Select(type =>
        {
            var methods = type.GetMethods()
                .Where(method => method.CustomAttributes
                .Any(attr => attr.AttributeType == typeof(ContextActionAttribute)))
                .Where(method => !method.IsStatic);

            ContextAction[] actions = methods.Select(method =>
            {
                ParameterInfo[] parameters = method.GetParameters();
                DataConstructor[] values = parameters.Select(param =>
                {
                    if (allowedLiteralTypes.Contains(param.ParameterType.Name))
                    {
                        LiteralType literal = GetLiteralType(param.ParameterType);
                        return new DataConstructor() { Name = param.ParameterType.Name, Params = [literal] };
                    }
                    else if (typeDict.TryGetValue(param.ParameterType.Name, out DataConstructor? ctype))
                    {
                        return ctype;
                    }
                    else
                    {
                        throw new UnsupportedTypeException();
                    }
                    
                }).ToArray();

                return new ContextAction() { Name = method.Name, Params = values };
            }).ToArray();

            return new ContextType() { Name = type.Name, ModuleReq = type.Namespace, Actions = actions };
        }).ToArray();

        Dictionary<string, ContextType> contextDict = [];
        foreach (ContextType context in contextTypesRef)
        {
            contextDict.Add(context.Name, context);
        }

        return new DataBase() { ContextTypes = contextDict, Constructors = typeDict };
    }

    private static LiteralType GetLiteralType(Type type)
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
            throw new ArgumentException();
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
            Params = literalParams
        };

        return valueType;
    }
}
