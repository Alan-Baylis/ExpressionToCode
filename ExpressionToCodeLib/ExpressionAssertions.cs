﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionToCodeLib
{

    /// <summary>
    /// Intended to be used as a static import; i.e. via "using static ExpressionToCodeLib.ExpressionAssertions;"
    /// </summary>
    public static class ExpressionAssertions
    {
        /// <summary>
        /// Evaluates an assertion and throws an exception the assertion it returns false or throws an exception.
        /// The exception includes the code of the assertion annotated with runtime values for its sub-expressions.
        /// </summary>
        public static void Assert(Expression<Func<bool>> assertion, string msg = null)
        {
            ExpressionToCodeConfiguration.CurrentConfiguration.Assert(assertion, msg);
        }
    }
}
