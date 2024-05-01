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
        ContextType[] contextTypesRef = contextTypes.Select(contextType =>
        {
            var methods = contextType.GetMethods()
                .Where(method => method.CustomAttributes
                .Any(attr => attr.AttributeType == typeof(ContextActionAttribute)))
                .Where(method => !method.IsStatic);

            ContextAction[] actions = methods.Select(contextMethod =>
            {
                ParameterInfo[] parameters = contextMethod.GetParameters();
                DataConstructor[] values = parameters.Select(methodParameter =>
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
                    
                }).ToArray();

                return new ContextAction() { Name = contextMethod.Name, Params = values };
            }).ToArray();

            return new ContextType() { Name = contextType.Name, ModuleReq = contextType.Namespace, Actions = actions };
        }).ToArray();

        Dictionary<string, ContextType> contextDict = [];
        foreach (ContextType context in contextTypesRef)
        {
            contextDict.Add(context.Name, context);
        }

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
}
