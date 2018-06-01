﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;

namespace ExpressionToCodeTest {
    public class ValueTupleTests {
        [Fact]
        public void ExpressionWithValueTupleEqualsCanCompile() {
            var tuple = (1, 3);
            var tuple2 = (1, "123".Length);

            Expression<Func<int>> ok1 = () => tuple.Item1;
            Expression<Func<int>> ok2 = () => tuple.GetHashCode();
            Expression<Func<Tuple<int, int>>> ok3 = () => tuple.ToTuple();
            Expression<Func<bool>> ok4 = () => Equals(tuple, tuple2);
            Expression<Func<int>> ok5 = () => Comparer<(int, int)>.Default.Compare(tuple, tuple2);
            ok1.Compile()();
            ok2.Compile()();
            ok3.Compile()();
            ok4.Compile()();
            ok5.Compile()();

            Expression<Func<bool>> err1 = () => tuple.Equals(tuple2);//crash
            Expression<Func<int>> err2 = () => tuple.CompareTo(tuple2);//crash
        }

        [Fact]
        public void FastExpressionCompileValueTupleEqualsWorks() {
            var tuple = (1, 3);
            (int, int Length) tuple2 = (1, "123".Length);
            var expr = FastExpressionCompiler.ExpressionCompiler.Compile(() => tuple.Equals(tuple2));
            Assert.True(expr());
        }

        [Fact]
        public void AssertingOnValueTupleEqualsWorks() {
            var tuple = (1, 3);
            var tuple2 = (1, "123".Length);
            Expression<Func<bool>> expr = () => tuple.Equals(tuple2);
            Assert.True(expr.Compile()());
            PAssert.That(() => tuple.Equals(tuple2));
        }
    }
}
