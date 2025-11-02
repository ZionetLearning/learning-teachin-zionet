using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
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
        var user = await CreateUserAsync();
        await ClientFixture.LoginAsync(Role.Admin); 
        var url = $"{ApiRoutes.AvatarUploadUrl(user.UserId)}";

        var req = new
        {
            contentType = ContentTypePng,
            sizeBytes = (long?)Png1x1.Length
        };

        var resp = await Client.PostAsJsonAsync(url, req);
        resp.ShouldBeOk();

        var dto = await ReadAsJsonAsync<JsonElement>(resp);

        var uploadUrl = dto.GetProperty("uploadUrl").GetString();
        var blobPath = dto.GetProperty("blobPath").GetString();

        uploadUrl.Should().NotBeNullOrWhiteSpace();
        blobPath.Should().NotBeNullOrWhiteSpace();
        blobPath!.Should().Contain(user.UserId.ToString("D"));
        blobPath.Should().Contain("avatar_v");
        uploadUrl!.Should().Contain(blobPath);
    }

    [Fact(DisplayName = "Avatar: Full flow upload → confirm → read-url → delete")]
    public async Task FullFlow_Upload_Confirm_ReadUrl_Delete_Should_Succeed()
    {
        var user = await CreateUserAsync();

        // 1) upload-url
        var uploadReq = new { contentType = ContentTypePng, sizeBytes = (long?)Png1x1.Length };
        var uploadResp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(user.UserId)}", uploadReq);
        uploadResp.ShouldBeOk();

        var uploadDto = await ReadAsJsonAsync<JsonElement>(uploadResp);
        var uploadUrl = uploadDto.GetProperty("uploadUrl").GetString()!;
        var blobPath = uploadDto.GetProperty("blobPath").GetString()!;

        // 2) PUT in Azurite
        using (var raw = new HttpClient())
        {
            using var content = new ByteArrayContent(Png1x1);
            content.Headers.ContentType = new MediaTypeHeaderValue(ContentTypePng);
            content.Headers.Add("x-ms-blob-type", "BlockBlob");

            var put = await raw.PutAsync(uploadUrl, content);
            put.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // 3) confirm
        var confirmReq = new { blobPath = blobPath, contentType = ContentTypePng };
        var confirmResp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarConfirm(user.UserId)}", confirmReq);
        confirmResp.ShouldBeOk();

        var getUserResp = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        getUserResp.ShouldBeOk();
        var userData = await ReadAsJsonAsync<UserData>(getUserResp);
        userData!.AvatarPath.Should().Be(blobPath);
        userData.AvatarContentType.Should().Be(ContentTypePng);

        var setAt = userData.AvatarUpdatedAtUtc;
        setAt.Should().NotBeNull();

        // 4) read-url
        var readUrlResp = await Client.GetAsync($"{ApiRoutes.AvatarReadUrl(user.UserId)}");
        readUrlResp.ShouldBeOk();
        var readUrl = await readUrlResp.Content.ReadAsStringAsync();
        readUrl.Should().NotBeNullOrWhiteSpace();

        using (var raw = new HttpClient())
        {
            var img = await raw.GetAsync(readUrl.Trim('"'));
            img.StatusCode.Should().Be(HttpStatusCode.OK);
            (await img.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
        }

        // 5) delete
        var del = await Client.DeleteAsync($"{ApiRoutes.AvatarDelete(user.UserId)}");
        del.ShouldBeOk();

        var del2 = await Client.DeleteAsync($"{ApiRoutes.AvatarDelete(user.UserId)}");
        del2.ShouldBeOk();

        var afterDelResp = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        afterDelResp.ShouldBeOk();
        var afterDel = await ReadAsJsonAsync<UserData>(afterDelResp);
        afterDel!.AvatarPath.Should().BeNull();
        afterDel.AvatarContentType.Should().BeNull();
        afterDel.AvatarUpdatedAtUtc!.Value.ToUniversalTime()
            .Should().BeOnOrAfter(setAt!.Value.ToUniversalTime());
    }

    [Fact(DisplayName = "Avatar: confirm should reject wrong blobPath prefix")]
    public async Task Confirm_Should_Reject_Invalid_BlobPath_Prefix()
    {
        var user = await CreateUserAsync();

        var confirmReq = new { blobPath = $"some-other-user/avatar_v123.png", contentType = ContentTypePng };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarConfirm(user.UserId)}", confirmReq);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Avatar: upload-url forbids for other user (SelfOrAdmin)")]
    public async Task UploadUrl_Should_Forbidden_For_Other_User()
    {
        // current token = logged-in user A
        var userA = await CreateUserAsync();
        var userB = TestDataHelper.CreateUser(role: "student");
        var createB = await Client.PostAsJsonAsync(ApiRoutes.User, userB);
        createB.ShouldBeCreated();

        // get B  upload-url with token A 
        var url = $"{ApiRoutes.AvatarUploadUrl(userB.UserId)}";
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)Png1x1.Length };

        var resp = await Client.PostAsJsonAsync(url, body);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects unsupported content-type")]
    public async Task UploadUrl_Should_Reject_Unsupported_ContentType()
    {
        var user = await CreateUserAsync();

        var body = new { contentType = "image/gif", sizeBytes = (long?)1234 };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(user.UserId)}", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects size > MaxBytes")]
    public async Task UploadUrl_Should_Reject_Too_Large()
    {
        var user = await CreateUserAsync();

        // > 10 MB
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)(30 * 1024 * 1024) };
        var resp = await Client.PostAsJsonAsync($"{ApiRoutes.AvatarUploadUrl(user.UserId)}", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
