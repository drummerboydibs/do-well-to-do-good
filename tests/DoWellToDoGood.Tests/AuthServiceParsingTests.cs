using System.Text;
using DoWellToDoGood.Services;

namespace DoWellToDoGood.Tests;

public class AuthServiceParsingTests
{
    // ---- ParseFragment ----

    [Fact]
    public void ParseFragment_ReadsMagicLinkTokens()
    {
        var p = AuthService.ParseFragment("access_token=abc&refresh_token=def&expires_in=3600");

        Assert.Equal("abc", p["access_token"]);
        Assert.Equal("def", p["refresh_token"]);
        Assert.Equal("3600", p["expires_in"]);
    }

    [Fact]
    public void ParseFragment_UrlDecodesValues()
    {
        var p = AuthService.ParseFragment("redirect=%2Fhome%2Fback&access_token=a%2Bb");

        Assert.Equal("/home/back", p["redirect"]);
        Assert.Equal("a+b", p["access_token"]);
    }

    [Fact]
    public void ParseFragment_IgnoresPairsWithoutAnEquals()
    {
        var p = AuthService.ParseFragment("garbage&access_token=ok&alsogarbage");

        Assert.True(p.ContainsKey("access_token"));
        Assert.False(p.ContainsKey("garbage"));
        Assert.False(p.ContainsKey("alsogarbage"));
        Assert.Single(p);
    }

    [Fact]
    public void ParseFragment_EmptyString_IsEmpty()
    {
        Assert.Empty(AuthService.ParseFragment(""));
    }

    // ---- ExtractError ----

    [Theory]
    [InlineData("{\"msg\":\"first\",\"message\":\"second\"}", "first")]
    [InlineData("{\"message\":\"second\"}", "second")]
    [InlineData("{\"error_description\":\"third\"}", "third")]
    [InlineData("{\"error\":\"fourth\"}", "fourth")]
    public void ExtractError_PrefersFirstKnownKey(string body, string expected)
    {
        Assert.Equal(expected, AuthService.ExtractError(body));
    }

    [Fact]
    public void ExtractError_NonJson_ReturnsNull()
    {
        Assert.Null(AuthService.ExtractError("upstream exploded"));
    }

    [Fact]
    public void ExtractError_JsonWithoutKnownKeys_ReturnsNull()
    {
        Assert.Null(AuthService.ExtractError("{\"code\":429}"));
    }

    // ---- DecodeClaims ----

    private static string B64Url(string json)
    {
        var b = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        return b.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Jwt(string payloadJson) => $"header.{B64Url(payloadJson)}.signature";

    [Fact]
    public void DecodeClaims_ExtractsEmailAndUserId()
    {
        var token = Jwt("{\"email\":\"jane@example.com\",\"sub\":\"user-123\"}");

        var (email, userId) = AuthService.DecodeClaims(token);

        Assert.Equal("jane@example.com", email);
        Assert.Equal("user-123", userId);
    }

    [Fact]
    public void DecodeClaims_MissingClaims_AreNull()
    {
        var (email, userId) = AuthService.DecodeClaims(Jwt("{\"role\":\"authenticated\"}"));

        Assert.Null(email);
        Assert.Null(userId);
    }

    [Theory]
    [InlineData("not-a-jwt")]
    [InlineData("only.two")]
    [InlineData("header.!!!notbase64!!!.sig")]
    [InlineData("")]
    public void DecodeClaims_Malformed_ReturnsNulls(string token)
    {
        var (email, userId) = AuthService.DecodeClaims(token);

        Assert.Null(email);
        Assert.Null(userId);
    }
}
