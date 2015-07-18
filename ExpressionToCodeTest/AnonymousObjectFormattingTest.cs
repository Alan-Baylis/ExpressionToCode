﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;

namespace ExpressionToCodeTest
{
    public class AnonymousObjectFormattingTest
    {
        [Fact]
        public void AnonymousObjectsRenderAsCode()
        {
            Assert.Equal("\nnew {\n  A = 1,\n  Foo = \"Bar\",\n}", ObjectToCode.ComplexObjectToPseudoCode(new { A = 1, Foo = "Bar", }));
        }

        [Fact]
        public void AnonymousObjectsInArray()
        {
            Assert.Equal("new[] {\nnew {\n  Val = 3,\n}, \nnew {\n  Val = 42,\n}}", ObjectToCode.ComplexObjectToPseudoCode(new[] { new { Val = 3, }, new { Val = 42 } }));
        }

        [Fact]
        public void EnumerableInAnonymousObject()
        {
            Assert.Equal("\nnew {\n  Nums = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, ...},\n}", ObjectToCode.ComplexObjectToPseudoCode(new { Nums = Enumerable.Range(1, 13) }));
        }
        [Fact]
        public void EnumInAnonymousObject()
        {
            Assert.Equal("\nnew {\n  Enum = ConsoleKey.A,\n}", ObjectToCode.ComplexObjectToPseudoCode(new { Enum = ConsoleKey.A}));
        }
    }
}
