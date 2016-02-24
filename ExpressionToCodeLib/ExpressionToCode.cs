﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ExpressionToCodeLib.Unstable_v2_Api;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace ExpressionToCodeLib
{
    public static class ExpressionToCode
    {
        public static string ToCode<T, T1, T2, T3>(Expression<Func<T, T1, T2, T3>> e) => ToCode((Expression)e);
        public static string ToCode<T, T1, T2>(Expression<Func<T, T1, T2>> e) => ToCode((Expression)e);
        public static string ToCode<T, T1>(Expression<Func<T, T1>> e) => ToCode((Expression)e);
        public static string ToCode<T>(Expression<Func<T>> e) => ToCode((Expression)e);
        public static string AnnotatedToCode<T, T1, T2, T3>(Expression<Func<T, T1, T2, T3>> e) => AnnotatedToCode((Expression)e);
        public static string AnnotatedToCode<T, T1, T2>(Expression<Func<T, T1, T2>> e) => AnnotatedToCode((Expression)e);
        public static string AnnotatedToCode<T, T1>(Expression<Func<T, T1>> e) => AnnotatedToCode((Expression)e);
        public static string AnnotatedToCode<T>(Expression<Func<T>> e) => AnnotatedToCode((Expression)e);
        internal static bool ShouldIgnoreSpaceAfter(char c) => c == ' ' || c == '(';
        public static string ToCode(Expression e) => ExpressionStringify.Default.ToCode(e);
        public static string AnnotatedToCode(Expression expr) => AnnotatedToCode(expr, null, false);

        internal static string AnnotatedToCode(Expression expr, string msg, bool ignoreOutermostValue)
        {
            return (msg == null ? "" : msg + "\n") + ExpressionWithSubExpressions.Create(expr, true).ComposeToSingleString();
            var splitLine = ExpressionToStringWithValues(expr, ignoreOutermostValue);

            var exprWithStalkedValues = new StringBuilder();
            if (msg == null) {
                exprWithStalkedValues.AppendLine(splitLine.Line);
            } else if (IsMultiline(msg)) {
                exprWithStalkedValues.AppendLine(msg);
                exprWithStalkedValues.AppendLine(splitLine.Line);
            } else {
                exprWithStalkedValues.AppendLine(splitLine.Line + "  :  " + msg);
            }

            for (int nodeI = splitLine.Nodes.Length - 1; nodeI >= 0; nodeI--) {
                char[] stalkLine = new string('\u2007', splitLine.Nodes[nodeI].Location).ToCharArray(); //figure-spaces.
                for (int i = 0; i < stalkLine.Length; i++) {
                    if (splitLine.Line[i] == ' ') {
                        stalkLine[i] = ' '; //use normal spaces where the expr used normal spaces for more natural spacing.
                    }
                }

                for (int prevI = 0; prevI < nodeI; prevI++) {
                    stalkLine[splitLine.Nodes[prevI].Location] = '\u2502'; //light vertical lines
                }
                exprWithStalkedValues.AppendLine((new string(stalkLine) + splitLine.Nodes[nodeI].Value).TrimEnd());
            }

            return exprWithStalkedValues.ToString();
        }

        static bool IsMultiline(string msg)
        {
            var idxAfterNewline = msg.IndexOf('\n') + 1;
            return idxAfterNewline > 0 && idxAfterNewline < msg.Length;
        }

        struct ExpressionWithSubExpressions
        {
            public string ExpressionString;
            public SubExpressionValue[] SubExpressions;

            public struct SubExpressionValue
            {
                public string SubExpression, ValueAsString;
            }

            public static ExpressionWithSubExpressions Create(Expression e, bool ignoreOutermostValue)
            {
                var sb = new StringBuilder();
                bool ignoreInitialSpace = true;
                var node = new ExpressionToCodeImpl().ExpressionDispatch(e);
                AppendNodeToStringBuilder(sb, node, ref ignoreInitialSpace);
                var fullExprText = sb.ToString();
                var subExpressionValues = new List<SubExpressionValue>();
                FindSubExpressionValues(node, node, subExpressionValues, ignoreOutermostValue);
                return new ExpressionWithSubExpressions { ExpressionString = fullExprText, SubExpressions = subExpressionValues.ToArray() };
            }

            static void AppendNodeToStringBuilder(StringBuilder sb, StringifiedExpression node, ref bool ignoreInitialSpace)
            {
                if (node.Text != null) {
                    var trimmedText = ignoreInitialSpace ? node.Text.TrimStart() : node.Text;
                    sb.Append(trimmedText);
                    ignoreInitialSpace = node.Text != "" && ShouldIgnoreSpaceAfter(node.Text[node.Text.Length - 1]);
                } else {
                    foreach (var kid in node.Children) {
                        AppendNodeToStringBuilder(sb, kid, ref ignoreInitialSpace);
                    }
                }
            }

            static void FindSubExpressionValues(StringifiedExpression node, StringifiedExpression subExprNode, List<SubExpressionValue> subExpressionValues, bool ignoreOutermostValue)
            {
                if (!ignoreOutermostValue && node.OptionalValue != null) {
                    var sb = new StringBuilder();
                    var ignoreInitialSpace = true;
                    AppendNodeWithLimitedDepth(sb, subExprNode, ref ignoreInitialSpace, 1);
                    var subExprString = sb.ToString();
                    string valueString = ExpressionValueAsCode(node.OptionalValue);
                    subExpressionValues.Add(new SubExpressionValue { SubExpression = subExprString, ValueAsString = valueString });
                }
                foreach (var kid in node.Children) {
                    if (kid.IsConceptualChild) {
                        FindSubExpressionValues(kid, kid, subExpressionValues, false);
                    } else {
                        FindSubExpressionValues(kid, subExprNode, subExpressionValues, ignoreOutermostValue);
                    }
                }
            }

            static void AppendNodeWithLimitedDepth(StringBuilder sb, StringifiedExpression node, ref bool ignoreInitialSpace, int unfoldToDepth)
            {
                if (node.Text != null) {
                    var trimmedText = ignoreInitialSpace ? node.Text.TrimStart() : node.Text;
                    sb.Append(trimmedText);
                    ignoreInitialSpace = node.Text != "" && ShouldIgnoreSpaceAfter(node.Text[node.Text.Length - 1]);
                } else {
                    foreach (var kid in node.Children) {
                        if (kid.IsConceptualChild && unfoldToDepth == 0) {
                            sb.Append("...");
                        } else {
                            AppendNodeWithLimitedDepth(sb, kid, ref ignoreInitialSpace, unfoldToDepth - (kid.IsConceptualChild ? 1 : 0));
                        }
                    }
                }
            }

            public string ComposeToSingleString()
            {
                return ExpressionString + "\n" + string.Join("", SubExpressions.Select(sub => sub.SubExpression + ": " + sub.ValueAsString + "\n"));
            }
        }

        static SplitExpressionLine ExpressionToStringWithValues(Expression e, bool ignoreOutermostValue)
        {
            var nodeInfos = new List<SubExpressionInfo>();
            var sb = new StringBuilder();
            bool ignoreInitialSpace = true;
            var node = new ExpressionToCodeImpl().ExpressionDispatch(e);
            AppendTo(sb, nodeInfos, node, ref ignoreInitialSpace, !ignoreOutermostValue);
            nodeInfos.Add(new SubExpressionInfo { Location = sb.Length, Value = null });
            return new SplitExpressionLine { Line = sb.ToString().TrimEnd(), Nodes = nodeInfos.ToArray() };
        }

        static void AppendTo(StringBuilder sb, List<SubExpressionInfo> nodeInfos, StringifiedExpression node, ref bool ignoreInitialSpace, bool showTopExpressionValue)
        {
            if (node.Text != null) {
                var trimmedText = ignoreInitialSpace ? node.Text.TrimStart() : node.Text;
                var pos0 = sb.Length;
                sb.Append(trimmedText);
                ignoreInitialSpace = node.Text.Any() && ShouldIgnoreSpaceAfter(node.Text[node.Text.Length - 1]);
                if (showTopExpressionValue) {
                    string valueString = node.OptionalValue == null ? null : ExpressionValueAsCode(node.OptionalValue);
                    if (valueString != null) {
                        nodeInfos.Add(new SubExpressionInfo { Location = pos0 + trimmedText.Length / 2, Value = valueString });
                    }
                }
            }
            foreach (var kid in node.Children) {
                AppendTo(sb, nodeInfos, kid, ref ignoreInitialSpace, showTopExpressionValue || kid.IsConceptualChild);
            }
        }

        static string ExpressionValueAsCode(Expression expression)
        {
            try {
                Delegate lambda;
                try {
                    lambda = Expression.Lambda(expression).Compile();
                } catch (InvalidOperationException) {
                    return null;
                }

                var val = lambda.DynamicInvoke();
                try {
                    return ObjectToCode.ComplexObjectToPseudoCode(val);
                } catch (Exception e) {
                    return "stringification throws " + e.GetType().FullName;
                }
            } catch (TargetInvocationException tie) {
                return "throws " + tie.InnerException.GetType().FullName;
            }
        }

        struct SplitExpressionLine
        {
            public string Line;
            public SubExpressionInfo[] Nodes;
        }

        struct SubExpressionInfo
        {
            public int Location;
            public string Value;
        }
    }
}
