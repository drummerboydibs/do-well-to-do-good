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

    // ---- ParseUserMeta ----

    [Fact]
    public void ParseUserMeta_ReadsBothTimestamps()
    {
        var json = "{\"created_at\":\"2024-01-05T08:30:00Z\",\"last_sign_in_at\":\"2026-06-12T15:45:00Z\"}";

        var (createdAt, lastSignIn) = AuthService.ParseUserMeta(json);

        Assert.Equal(DateTimeOffset.Parse("2024-01-05T08:30:00Z"), createdAt);
        Assert.Equal(DateTimeOffset.Parse("2026-06-12T15:45:00Z"), lastSignIn);
    }

    [Fact]
    public void ParseUserMeta_PreservesUtcInstant_RegardlessOfMachineTimeZone()
    {
        // GoTrue returns UTC ("Z"); the parsed instant must equal that UTC moment
        // no matter what the test host's local zone is (the UI does ToLocalTime()).
        var (createdAt, _) = AuthService.ParseUserMeta("{\"created_at\":\"2024-01-05T08:30:00Z\"}");

        Assert.NotNull(createdAt);
        Assert.Equal(new DateTimeOffset(2024, 1, 5, 8, 30, 0, TimeSpan.Zero), createdAt!.Value.ToUniversalTime());
    }

    [Fact]
    public void ParseUserMeta_MissingFields_AreNull()
    {
        var (createdAt, lastSignIn) = AuthService.ParseUserMeta("{\"email\":\"jane@example.com\"}");

        Assert.Null(createdAt);
        Assert.Null(lastSignIn);
    }

    [Fact]
    public void ParseUserMeta_PartialFields_FillWhatItCan()
    {
        var (createdAt, lastSignIn) = AuthService.ParseUserMeta("{\"created_at\":\"2024-01-05T08:30:00Z\"}");

        Assert.NotNull(createdAt);
        Assert.Null(lastSignIn);
    }

    [Theory]
    [InlineData("{\"created_at\":\"not-a-date\"}")]
    [InlineData("{\"created_at\":null}")]
    [InlineData("{\"created_at\":12345}")]
    public void ParseUserMeta_UnparseableTimestamp_IsNull(string json)
    {
        var (createdAt, _) = AuthService.ParseUserMeta(json);

        Assert.Null(createdAt);
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("")]
    [InlineData("[1,2,3]")]
    public void ParseUserMeta_MalformedBody_ReturnsNulls(string json)
    {
        var (createdAt, lastSignIn) = AuthService.ParseUserMeta(json);

        Assert.Null(createdAt);
        Assert.Null(lastSignIn);
    }
}
