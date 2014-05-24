ExpressionToCode
================
Generates valid, readable C# from an Expression Tree, for example:

{{{
  @"() => new[] { 1.0, 2.01, 3.5 }.SequenceEqual(new[] { 1.0, 2.01, 3.5 })"
== 
  ExpressionToCode.ToCode(
    () => new[] { 1.0, 2.01, 3.5 }.SequenceEqual(new[] { 1.0, 2.01, 3.5 })
  )
}}}

!ExpressionToCode also provides a clone of Groovy's [http://dontmindthelanguage.wordpress.com/2009/12/11/groovy-1-7-power-assert/ Power Assert] which includes the code of the failing assertion's expression and the values of its subexpressions.  This functionality is particularly useful in a unit testing framework such as [http://www.nunit.org/ NUnit] or [http://xunit.codeplex.com/ xUnit.NET].  When you execute the following (invalid) assertion:

{{{
PAssert.That(()=>Enumerable.Range(0,1000).ToDictionary(i=>"n"+i)["n3"].ToString() == (3.5).ToString());
}}}

The assertion fails with the following message:

{{{
PAssert.That failed for:

Enumerable.Range(0, 1000).ToDictionary(i => "n" + (object)i)["n3"].ToString() == 3.5.ToString()
             |                 |                            |         |        |        |
             |                 |                            |         |        |        "3.5"
             |                 |                            |         |        false
             |                 |                            |         "3"
             |                 |                            3
             |                 {[n0, 0], [n1, 1], [n2, 2], [n3, 3], [n4, 4], [n5, 5], [n6, 6], [n7, 7], [n8, 8], [n9, 9], ...}
             {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, ...}
}}}

!ExpressionToCode was inspired by [http://powerassert.codeplex.com/ Power Asssert.NET].  It differs from !PowerAssert.NET by support a larger portion of the lambda syntax; the aim is to generate valid C# for *all* expression trees created from lambda's.  Currently supported:

 * Supports static field and property access
 * Supports more operators, e.g. logical and bitwise negation
 * Recognizes C# indexer use (e.g. `dict["mykey"]==3`), in addition to special cases for array indexers and string indexers
 * Adds parentheses where required by operator precedence and associativity (e.g. `() => x - (a - b) + x * (a + b)` is correctly regenerated)
 * Generates valid numeric and other constant literals including escapes and suffixes where required (e.g. `1m + (decimal)Math.Sqrt(1.41)`)
 * Supports C# syntactic sugar for object initializers, object member initializers, list initializers, extension methods, anonymous types ([http://code.google.com/p/expressiontocode/issues/detail?id=12&can=1&q=anonymous 12], [http://code.google.com/p/expressiontocode/issues/detail?id=3&can=1&q=anonymous 3]), etc
 * Uses the same spacing rules Visual Studio does by default
 * Supports nested Lambdas
 * Expands generic type instances and nullable types into normal C# (e.g. `Func<int, bool>` and `int?`)
 * Recognizes references to `this` and omits the keyword where possible ([http://code.google.com/p/expressiontocode/issues/list?cursor=5&updated=5&ts=1295683070&can=1 5])  

Not yet implemented:

 * Recognize when `==` differs from `.Equals` or `.SequenceEquals`, as Power Assert.NET does (issue 2).
 * Omit implicit casts (e.g. `object.Equals((object)3, (object)4)`) - issue 4.
 * Use LINQ query syntax where required - issue 6.
 * Detect when type parameters to methods are superfluous - issue 13.
 * Detect when nested lambda parameters require type annotation - issue 14.
 * See all [http://code.google.com/p/expressiontocode/issues/list open issues].

Requires .NET 4.0 (.NET 3.5 could be supported by omitting support for newer expression types, this would require a few simple source changes).

If you have any questions, you can contact me at eamon at nerbonne dot org.

See the [Documentation documentation] and [http://code.google.com/p/expressiontocode/downloads download the library], [http://nuget.org/packages/ExpressionToCodeLib/ import it using NuGet], or checkout the source (license: Apache 2.0 or the MIT license, at your option)!  




