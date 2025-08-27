using IntegrationTests.Constants;
public static class CookieHelper
{
    public static string? ExtractCookieFromHeaders(HttpResponseMessage response, string cookieName)
    {
        if (response.Headers.TryGetValues(TestConstants.SetCookie, out var setCookieHeaders))
        {
            foreach (var header in setCookieHeaders)
            {
                if (header.StartsWith($"{cookieName}="))
                {
                    var cookiePart = header.Split(';')[0];
                    var separatorIndex = cookiePart.IndexOf('=');
                    if (separatorIndex > -1)
                    {
                        return cookiePart.Substring(separatorIndex + 1);
                    }
                }
            }
        }

        return null;
    }
}

