using System;
using System.Linq.Expressions;
using ExpressionToCodeLib.Internal;

namespace ExpressionToCodeLib
{
    public static class ExpressionTreeAssertion
    {
        public static void Assert(this ExpressionToCodeConfiguration config, Expression<Func<bool>> assertion, string msg = null)
        {
            var compiled = config.Value.ExpressionCompiler.Compile(assertion);
            bool ok;
            try {
                ok = compiled();
            } catch (Exception e) {
                throw Err(config, assertion, msg ?? "failed with exception", e);
            }
            if (!ok) {
                throw Err(config, assertion, msg ?? "failed", null);
            }
        }

        static Exception Err(ExpressionToCodeConfiguration config, Expression<Func<bool>> assertion, string msg, Exception innerException)
            => UnitTestingFailure
            .AssertionExceptionFactory(
                config.Value.CodeAnnotator.AnnotateExpressionTree(config, assertion.Body, msg, true),
                innerException);
    }
}
