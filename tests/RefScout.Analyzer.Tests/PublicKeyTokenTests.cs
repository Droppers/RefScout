using Xunit;

namespace RefScout.Analyzer.Tests;

public class PublicKeyTokenTests
{
    private static readonly string TokenString = "31bf3856ad364e35";
    private static readonly byte[] TokenBytes = { 49, 191, 56, 86, 173, 54, 78, 53 };

    [Fact]
    public void ToString_Test()
    {
        var token = new PublicKeyToken(TokenBytes);
        Assert.Equal(TokenString, token.ToString());
        Assert.Equal("null", PublicKeyToken.Empty.ToString());
    }

    [Fact]
    public void Equality()
    {
        var tokenA = new PublicKeyToken(TokenBytes);
        var tokenB = new PublicKeyToken(TokenBytes);
        Assert.Equal(tokenA, tokenB);
        Assert.False(tokenA.Equals(new object()));
        Assert.True(tokenA == tokenB);

        Assert.Equal(0, PublicKeyToken.Empty.GetHashCode());
        Assert.NotEqual(0, tokenB.GetHashCode());
    }
}