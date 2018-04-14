using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Text.RegularExpressions;

namespace Genau.EagerLinq.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var linqMethods = GetLinqExtensionMethods();

            var codeDom = CodeDomProvider.CreateProvider("CSharp");
            
            var compileUnit = new CodeCompileUnit();
            var ns = new CodeNamespace("System");

            ns.Imports.AddRange(new[] {
                new CodeNamespaceImport("Genau"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.Linq")
            });

            var @class = new CodeTypeDeclaration("EagerEnumerable");
            @class.IsClass = true;
            @class.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract;
            
            @class.Members.AddRange(
                GetLinqExtensionMethods()
                    .Select(ConvertMethod)
                    .ToArray());
            
            ns.Types.Add(@class);

            compileUnit.Namespaces.Add(ns);

            using(var writer = new StreamWriter("../Genau.EagerLinq/EagerEnumerable.CodeGen.cs")) {
                codeDom.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions { });
            }
        }

        static IEnumerable<MethodInfo> GetLinqExtensionMethods()
            => typeof(Enumerable).GetMethods()
                .Where(m => m.IsPublic)
                .Where(m => m.IsEnumerableExtension())
                .Where(m => m.ReturnType.IsEnumerable())
                .Where(m => !_linqBlacklist.Contains(m.Name));

        static ISet<string> _linqBlacklist = new HashSet<string>(new[] { "TakeLast", "SkipLast" });


        static CodeMemberMethod ConvertMethod(MethodInfo linqMethod) {
            var m = new CodeMemberMethod {
                Name = linqMethod.Name,
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
            };

            var typeParams = linqMethod.GetGenericArguments()
                                .Select(t => new CodeTypeParameter(t.Name))
                                .ToArray();

            m.TypeParameters.AddRange(typeParams);

            m.Parameters.AddRange(linqMethod.GetParameters()
                                    .Select((p, i) => {
                                        switch(i) {
                                            case 0: 
                                                return new CodeParameterDeclarationExpression(p.ParameterType.MapToEagerEnumerable().AsReference(withThis: true), p.Name);
                                            default: 
                                                return new CodeParameterDeclarationExpression(p.ParameterType.AsReference(), p.Name);
                                        }
                                    })
                                    .ToArray());

            m.ReturnType = linqMethod.ReturnType.MapToEagerEnumerable().AsReference();

            m.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeTypeReferenceExpression(typeof(EagerEnumerable)),
                            "From"
                        ),
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(linqMethod.DeclaringType),
                                linqMethod.Name,
                                linqMethod.GetGenericArguments().Select(a => a.AsReference()).ToArray()
                            ),
                            linqMethod.GetParameters().Select(p => new CodeVariableReferenceExpression(p.Name)).ToArray()
                        )
                    )
                )
            );

            return m;
        }

    }


    public static class Extensions 
    {

        public static Type MapToEagerEnumerable(this Type type) {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                return typeof(IEagerEnumerable<>).MakeGenericType(type.GetGenericArguments().First());
            }
            else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>)) {
                return typeof(IEagerOrderedEnumerable<>).MakeGenericType(type.GetGenericArguments().First());
            }

            return type;
        }


        public static CodeTypeReference AsReference(this Type type, bool withThis = false)
            => new CodeTypeReference($"{(withThis ? "this " : "")}{type.BuildName()}");

        public static bool IsEnumerableExtension(this MethodInfo method)
            => method.IsStatic
                && (method.GetParameters().FirstOrDefault()?.ParameterType.IsEnumerable() 
                        ?? false);
            
        public static bool IsEnumerableLike(this Type type)
            => type.IsEnumerable() 
                || type.GetInterfaces().Any(i => i.IsEnumerable());

        public static bool IsEnumerable(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        public static string BuildName(this Type type)
            => type.IsGenericType
                ? $"{type.Name.Split('`').First()}<{string.Join(", ", type.GetGenericArguments().Select(BuildName))}>"
                : type.Name;

    }
}
