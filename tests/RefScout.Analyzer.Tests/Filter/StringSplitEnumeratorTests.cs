using System.Collections.Generic;
using RefScout.Analyzer.Filter;
using Xunit;

namespace RefScout.Analyzer.Tests.Filter;

public class StringSplitEnumeratorTests
{
    [Theory]
    [InlineData("a simple test", new[] { "a", "simple", "test" })]
    [InlineData("    a      simple       test    ", new[] { "a", "simple", "test" })]
    [InlineData("   ", new string[] { })]
    [InlineData("", new string[] { })]
    [InlineData("\" quote not closed", new[] { " quote not closed" })]
    [InlineData("\" quote is closed\"", new[] { " quote is closed" })]
    [InlineData("\" quote is closed\" bippity boopity", new[] { " quote is closed", "bippity", "boopity" })]
    [InlineData("     aaa     \" quote     is closed\" bippity boopity",
        new[] { "aaa", " quote     is closed", "bippity", "boopity" })]
    [InlineData("\"quotenospace\"test", new[] { "quotenospace", "test" })]
    [InlineData("test \"quotenospace", new[] { "test", "quotenospace" })]
    public void TestEnumeratorResult(string input, string[] expected)
    {
        var actual = EnumeratorToArray(new StringSplitEnumerator(input));
        Assert.Equal(expected, actual);
    }

    private static string[] EnumeratorToArray(StringSplitEnumerator enumerator)
    {
        var list = new List<string>();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current.ToString());
        }

        return list.ToArray();
    }
}