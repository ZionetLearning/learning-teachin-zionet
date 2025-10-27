using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

[Collection("Per-test user collection")]
public class UserAvatarIntegrationTests(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UsersTestBase(perUserFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    // минимальная валидная PNG 1x1 (прозрачная)
    private static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Yk7G4sAAAAASUVORK5CYII=");

    private const string ContentTypePng = "image/png";


    [Fact(DisplayName = "Avatar: POST upload-url returns SAS and blobPath")]
    public async Task UploadUrl_Should_Return_Sas_And_Path()
    {
        var user = await CreateUserAsync();
        var url = $"/users-manager/user/{user.UserId}/avatar/upload-url";

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
        var uploadResp = await Client.PostAsJsonAsync($"/users-manager/user/{user.UserId}/avatar/upload-url", uploadReq);
        uploadResp.ShouldBeOk();

        var uploadDto = await ReadAsJsonAsync<JsonElement>(uploadResp);
        var uploadUrl = uploadDto.GetProperty("uploadUrl").GetString()!;
        var blobPath = uploadDto.GetProperty("blobPath").GetString()!;

        // 2) PUT в Azurite напрямую по SAS
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
        var confirmResp = await Client.PostAsJsonAsync($"/users-manager/user/{user.UserId}/avatar/confirm", confirmReq);
        confirmResp.ShouldBeOk();

        var getUserResp = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        getUserResp.ShouldBeOk();
        var userData = await ReadAsJsonAsync<UserData>(getUserResp);
        userData!.AvatarPath.Should().Be(blobPath);
        userData.AvatarContentType.Should().Be(ContentTypePng);

        // 👇 запоминаем время установки
        var setAt = userData.AvatarUpdatedAtUtc;
        setAt.Should().NotBeNull();

        // 4) read-url
        var readUrlResp = await Client.GetAsync($"/users-manager/user/{user.UserId}/avatar/url");
        readUrlResp.ShouldBeOk();
        var readUrl = await readUrlResp.Content.ReadAsStringAsync();
        readUrl.Should().NotBeNullOrWhiteSpace();

        // Попробуем GET картинку по read-url (для SAS sp=r или публичной ссылки это 200)
        using (var raw = new HttpClient())
        {
            var img = await raw.GetAsync(readUrl.Trim('"')); // если пришло как JSON-строка
            img.StatusCode.Should().Be(HttpStatusCode.OK);
            // размер не обязателен, но должен быть > 0
            (await img.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
        }

        // 5) delete
        var del = await Client.DeleteAsync($"/users-manager/user/{user.UserId}/avatar");
        del.ShouldBeOk();

        // повторный delete — идемпотентно
        var del2 = await Client.DeleteAsync($"/users-manager/user/{user.UserId}/avatar");
        del2.ShouldBeOk();

        // и поля в user должны очиститься
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
        var resp = await Client.PostAsJsonAsync($"/users-manager/user/{user.UserId}/avatar/confirm", confirmReq);

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

        // Пытаемся для B получить upload-url под токеном A → 403
        var url = $"/users-manager/user/{userB.UserId}/avatar/upload-url";
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)Png1x1.Length };

        var resp = await Client.PostAsJsonAsync(url, body);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects unsupported content-type")]
    public async Task UploadUrl_Should_Reject_Unsupported_ContentType()
    {
        var user = await CreateUserAsync();

        var body = new { contentType = "image/gif", sizeBytes = (long?)1234 };
        var resp = await Client.PostAsJsonAsync($"/users-manager/user/{user.UserId}/avatar/upload-url", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Avatar: upload-url rejects size > MaxBytes")]
    public async Task UploadUrl_Should_Reject_Too_Large()
    {
        var user = await CreateUserAsync();

        // заведомо больше лимита (2 МБ)
        var body = new { contentType = ContentTypePng, sizeBytes = (long?)(30 * 1024 * 1024) };
        var resp = await Client.PostAsJsonAsync($"/users-manager/user/{user.UserId}/avatar/upload-url", body);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
