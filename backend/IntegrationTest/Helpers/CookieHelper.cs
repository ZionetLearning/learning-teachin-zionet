using IntegrationTests.Constants;
public static class CookieHelper
{
    public static string? ExtractCookieFromHeaders(HttpResponseMessage response, string cookieName)
    {
        if (response.Headers.TryGetValues($"{TestConstants.SetCookie}", out var setCookieHeaders))
        {
            foreach (var header in setCookieHeaders)
            {
                if (header.StartsWith($"{cookieName}="))
                {
                    return header.Split(';')[0].Split('=')[1];
                }
            }
        }

        return null;
    }
}
