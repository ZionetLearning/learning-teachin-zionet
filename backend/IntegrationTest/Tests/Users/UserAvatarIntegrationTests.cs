using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

[Collection("Per-test user collection")]
public class UserAvatarIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UsersTestBase(httpClientFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    // min png
    private static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Yk7G4sAAAAASUVORK5CYII=");

    private const string ContentTypePng = "image/png";


    [Fact(DisplayName = "Avatar: POST upload-url returns SAS and blobPath")]
    public async Task UploadUrl_Should_Return_Sas_And_Path()
    {
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        await ClientFixture.LoginAsync(Role.Admin);
        var url = $"{ApiRoutes.AvatarUploadUrl(userInfo.UserId)}";

        var req = new
        {
            contentType = ContentTypePng,
            sizeBytes = (long?)Png1x1.Length
        };

        var resp = await Client.PostAsJsonAsync(url, req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var dto = await ReadAsJsonAsync<JsonElement>(resp);

        var uploadUrl = dto.GetProperty("uploadUrl").GetString();
        var blobPath = dto.GetProperty("blobPath").GetString();

        Assert.False(string.IsNullOrWhiteSpace(uploadUrl));
        Assert.False(string.IsNullOrWhiteSpace(blobPath));
        Assert.NotNull(blobPath);
        Assert.Contains(userInfo.UserId.ToString("D"), blobPath);
        Assert.Contains("avatar_v", blobPath);
        Assert.NotNull(uploadUrl);
        Assert.Contains(blobPath, uploadUrl);
    }

    [Fact(DisplayName = "Avatar: Full flow upload → confirm → read-url → delete")]
    public async Task FullFlow_Upload_Confirm_ReadUrl_Delete_Should_Succeed()
    {
        var user = await CreateUserAsync();

        // 1) upload-url
        var uploadReq = new { contentType = ContentTypePng, sizeBytes = (long?)Png1x1.Length };
        var uploadResp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(user.UserId)}", uploadReq);
        Assert.Equal(HttpStatusCode.OK, uploadResp.StatusCode);

        var uploadDto = await ReadAsJsonAsync<JsonElement>(uploadResp);
        var uploadUrl = uploadDto.GetProperty("uploadUrl").GetString()!;
        var blobPath = uploadDto.GetProperty("blobPath").GetString()!;

        // 2) PUT in Azurite/Azure
        using (var raw = new HttpClient())
        {
            using var content = new ByteArrayContent(Png1x1);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypePng);
            content.Headers.Add("x-ms-blob-type", "BlockBlob");

            var put = await raw.PutAsync(uploadUrl, content);
            Assert.Equal(HttpStatusCode.Created, put.StatusCode);
        }

        // 3) confirm
        var confirmReq = new { blobPath = blobPath, contentType = ContentTypePng };
        var confirmResp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarConfirm(user.UserId)}", confirmReq);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);

        // 3.1) Wait until the user reflects the avatar (eventual consistency in cloud)
        GetUserResponse? userData = null;
        var swSet = System.Diagnostics.Stopwatch.StartNew();
        var setTimeout = TimeSpan.FromSeconds(10);
        var pollDelay = TimeSpan.FromMilliseconds(200);

        while (true)
        {
            var getUserResp = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
            Assert.Equal(HttpStatusCode.OK, getUserResp.StatusCode);
            userData = await ReadAsJsonAsync<GetUserResponse>(getUserResp);

            if (userData?.AvatarPath == blobPath &&
                userData.AvatarContentType == ContentTypePng &&
                userData.AvatarUpdatedAtUtc != null)
            {
                break;
            }

            if (swSet.Elapsed > setTimeout)
            {
                // add log with blobPath and userData.AvatarPath for easier debugging
                outputHelper.WriteLine($"Timeout waiting for avatar to be set. Expected blobPath: ");
                outputHelper.WriteLine(blobPath);
                outputHelper.WriteLine($"Actual AvatarPath: ");
                outputHelper.WriteLine(userData?.AvatarPath ?? "null");
                Assert.Equal(blobPath, userData?.AvatarPath);
            }

            await Task.Delay(pollDelay);
        }

        var setAt = userData!.AvatarUpdatedAtUtc;
        Assert.NotNull(setAt);

        // 4) read-url (retry until blob is readable)
        string? readUrl = null;
        byte[]? imgBytes = null;
        var swRead = System.Diagnostics.Stopwatch.StartNew();
        var readTimeout = TimeSpan.FromSeconds(10);

        while (true)
        {
            var readUrlResp = await Client.GetAsync($"{ApiRoutes.AvatarReadUrl(user.UserId)}");
            Assert.Equal(HttpStatusCode.OK, readUrlResp.StatusCode);
            readUrl = (await readUrlResp.Content.ReadAsStringAsync()).Trim('"');
            Assert.False(string.IsNullOrWhiteSpace(readUrl));

            using var raw = new HttpClient();
            var img = await raw.GetAsync(readUrl);
            if (img.StatusCode == HttpStatusCode.OK)
            {
                var bytes = await img.Content.ReadAsByteArrayAsync();
                if (bytes.Length > 0)
                {
                    imgBytes = bytes;
                    break;
                }
            }

            if (swRead.Elapsed > readTimeout)
            {
                var final = await new HttpClient().GetAsync(readUrl);
                Assert.Equal(HttpStatusCode.OK, final.StatusCode);
                Assert.True((await final.Content.ReadAsByteArrayAsync()).Length > 0);
            }

            await Task.Delay(pollDelay);
        }

        Assert.NotNull(imgBytes);
        Assert.True(imgBytes.Length > 0);

        // 5) delete (idempotent)
        var del = await Client.DeleteAsync($"{ApiRoutes.AvatarDelete(user.UserId)}");
        Assert.Equal(HttpStatusCode.OK, del.StatusCode);

        var del2 = await Client.DeleteAsync($"{ApiRoutes.AvatarDelete(user.UserId)}");
        Assert.Equal(HttpStatusCode.OK, del2.StatusCode);

        // 5.1) Wait until fields are cleared
        var swDel = System.Diagnostics.Stopwatch.StartNew();
        var delTimeout = TimeSpan.FromSeconds(10);

        while (true)
        {
            var afterDelResp = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
            Assert.Equal(HttpStatusCode.OK, afterDelResp.StatusCode);
            var afterDel = await ReadAsJsonAsync<GetUserResponse>(afterDelResp);

            if (afterDel!.AvatarPath is null &&
                afterDel.AvatarContentType is null &&
                afterDel.AvatarUpdatedAtUtc!.Value.ToUniversalTime() >= setAt!.Value.ToUniversalTime())
            {
                break;
            }

            if (swDel.Elapsed > delTimeout)
            {
                Assert.Null(afterDel!.AvatarPath);
                Assert.Null(afterDel.AvatarContentType);
                Assert.True(afterDel.AvatarUpdatedAtUtc!.Value.ToUniversalTime() >= setAt!.Value.ToUniversalTime());
            }

            await Task.Delay(pollDelay);
        }
    }

    [Fact(DisplayName = "Avatar: confirm should reject wrong blobPath prefix")]
    public async Task Confirm_Should_Reject_Invalid_BlobPath_Prefix()
    {
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        await ClientFixture.LoginAsync(Role.Admin);

        var confirmReq = new { blobPath = $"some-other-user/avatar_v123.png", contentType = ContentTypePng };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarConfirm(userInfo.UserId)}", confirmReq);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Avatar: upload-url forbids for other user (SelfOrAdmin)")]
    public async Task UploadUrl_Should_Forbidden_For_Other_User()
    {
        // current token = logged-in user A
        var userA = await CreateUserAsync();
        var userB = TestDataHelper.CreateUser(role: "student");
        var createB = await Client.PostAsJsonAsync(ApiRoutes.User, userB);
        Assert.Equal(HttpStatusCode.Created, createB.StatusCode);

        // get B  upload-url with token A
        var url = $"{ApiRoutes.AvatarUploadUrl(userB.UserId)}";
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)Png1x1.Length };

        var resp = await Client.PostAsJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects unsupported content-type")]
    public async Task UploadUrl_Should_Reject_Unsupported_ContentType()
    {
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        await ClientFixture.LoginAsync(Role.Admin);

        var body = new { contentType = "image/gif", sizeBytes = (long?)1234 };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(userInfo.UserId)}", body);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects size > MaxBytes")]
    public async Task UploadUrl_Should_Reject_Too_Large()
    {
        var userInfo = ClientFixture.GetUserInfo(Role.Admin);

        await ClientFixture.LoginAsync(Role.Admin);

        // > 10 MB
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)(30 * 1024 * 1024) };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(userInfo.UserId)}", body);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}