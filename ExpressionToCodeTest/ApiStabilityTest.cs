﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;
using Xunit;

namespace ExpressionToCodeTest
{
    public class ApiStabilityTest
    {
        [Fact]
        public void PublicApi()
        {
            var publicTypes = typeof(ExpressionToCode).Assembly.GetTypes()
                .Where(IsPublic)
                .Where(type => !type.Namespace.Contains("Unstable"))
                .OrderByDescending(type => type.IsEnum)
                .ThenByDescending(type => type.IsInterface)
                .ThenBy(type => type.FullName);

            ApprovalTest.Verify(PrettyPrintTypes(publicTypes));
        }

        [Fact]
        public void UnstableApi()
        {
            var unstableTypes = typeof(ExpressionToCode).Assembly.GetTypes()
                .Where(IsPublic)
                .Where(type => type.Namespace.Contains("Unstable"))
                .OrderByDescending(type => type.IsEnum)
                .ThenByDescending(type => type.IsInterface)
                .ThenBy(type => type.FullName);

            ApprovalTest.Verify(PrettyPrintTypes(unstableTypes));
        }

        static string PrettyPrintTypes(IEnumerable<Type> types) => string.Join("", types.Select(PrettyPrintTypeDescription));
        static string PrettyPrintTypeDescription(Type o) => PrettyPrintTypeHeader(o) + "\n" + PrettyPrintTypeContents(o);

        static string PrettyPrintTypeContents(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .OrderBy(mi => mi.MetadataToken)
                    .Where(mi => mi.DeclaringType.Assembly != typeof(object).Assembly) //exclude noise
                ;

            var methodBlock = string.Join("", methods.Select(mi => PrettyPrintMethod(mi) + "\n"));

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(mi => mi.DeclaringType.Assembly != typeof(object).Assembly) //exclude noise
                ;

            var fieldBlock = string.Join("", fields.Select(fi => PrettyPrintField(fi) + "\n"));

            return fieldBlock + methodBlock + "\n";
        }

        static string PrettyPrintTypeHeader(Type type)
        {
            var prefix = TypePrefix(type);

            var baseType = type.BaseType == typeof(object) ? null : type.BaseType;
            var allInterfaces = type.GetInterfaces();
            var interfaces = baseType == null ? allInterfaces : allInterfaces.Except(baseType.GetInterfaces());
            var inheritanceTypes = new[] { baseType }.OfType<Type>().Concat(interfaces);
            var suffix = !inheritanceTypes.Any() || type.IsEnum ? "" : " : " + string.Join(", ", inheritanceTypes.Select(ObjectToCode.ToCSharpFriendlyTypeName));

            var name = ObjectToCode.ToCSharpFriendlyTypeName(type);

            return prefix + " " + name + suffix;
        }

        static string TypePrefix(Type type)
        {
            if (type.IsEnum) {
                return "enum";
            } else if (type.IsValueType) {
                return "struct";
            } else if (type.IsInterface) {
                return "interface";
            } else {
                return "class";
            }
        }

        static string PrettyPrintMethod(MethodInfo mi)
        {
            var fakeTarget = mi.IsStatic ? "TYPE" : "inst";

            return "    " + ObjectToCode.ToCSharpFriendlyTypeName(mi.ReturnType) + " " + fakeTarget +
                "." + mi.Name
                + PrettyPrintGenericArguments(mi)
                + PrettyPrintParameterList(mi);
        }

        static object PrettyPrintField(FieldInfo fi) => "    "
            + (fi.IsLiteral ? "const " : (fi.IsStatic ? "static " : "") + (fi.IsInitOnly ? "readonly " : ""))
            + ObjectToCode.ToCSharpFriendlyTypeName(fi.FieldType)
            + " " + fi.Name
            + (fi.IsLiteral ? " = " + ObjectToCode.ComplexObjectToPseudoCode(fi.GetRawConstantValue()) : "");

        static string PrettyPrintParameterList(MethodInfo mi)
        {
            return "(" + string.Join(
                ", ",
                mi.GetParameters()
                    .Select(
                        pi =>
                            ObjectToCode.ToCSharpFriendlyTypeName(pi.ParameterType) + " " + pi.Name)) + ")";
        }

        static string PrettyPrintGenericArguments(MethodInfo mi)
        {
            if (!mi.IsGenericMethodDefinition) {
                return "";
            }
            return "<"
                + string.Join(", ", mi.GetGenericArguments().Select(ObjectToCode.ToCSharpFriendlyTypeName))
                + ">";
        }

        static bool IsPublic(Type type) => type.IsPublic || type.IsNestedPublic && IsPublic(type.DeclaringType);
    }
}
