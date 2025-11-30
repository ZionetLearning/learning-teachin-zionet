using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models.Notification;
using IntegrationTests.Tests.Auth;
using Manager.Models.Emails;
using Manager.Models.Users;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using IntegrationTests.Models.Emails;

namespace IntegrationTests.Tests.Emails;

[Collection("IntegrationTests")]
public class EmailIntegrationTests(
    HttpClientFixture clientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRTestFixture
) : AuthTestBase(clientFixture, outputHelper, signalRTestFixture)
{
    [Fact(DisplayName = "Full email flow works: fetch recipients -> generate draft -> send email")]
    public async Task FullEmailFlow_ShouldSucceed()
    {
        // Register a custom user for this test
        var email = "zionet@mail.com";
        var password = "123456";
        var name = "test_learning";
        var userId = Guid.NewGuid();


        var res = await Client.PostAsJsonAsync(UserRoutes.UserBase, new CreateUserRequest
        {
            UserId = userId,
            Email = email,
            Password = password,
            FirstName = name,
            LastName = name + "_last",
            Role = Role.Teacher.ToString()
        });
        res.EnsureSuccessStatusCode();


        var loginResponse = await LoginAsync(email, password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var accessToken = await ExtractAccessToken(loginResponse);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


        // Fetch recipient emails
        var getRecipientsResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, ApiRoutes.GetEmailRecipients(name)));
        getRecipientsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var recipientPayload = await getRecipientsResponse.Content.ReadFromJsonAsync<RecipientsResult>();
        recipientPayload.Should().NotBeNull();
        recipientPayload!.Emails.Should().Contain(email);



        // Request draft generation
        var draftRequest = new AiGeneratedEmailRequest
        {
            Subject = "Reminder: Meeting Tomorrow",
            Purpose = "meeting_reminder",
            PreferredLanguageCode = SupportedLanguage.en
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.EmailDraft)
        {
            Content = JsonContent.Create(draftRequest)
        };
        var draftResponse = await Client.SendAsync(request);
        draftResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await EnsureSignalRStartedAsync();
        SignalRFixture.ClearReceivedMessages();


        var received = await WaitForEventAsync(
            e => e.EventType == EventType.EmailDraftGenerated,
            TimeSpan.FromSeconds(15)
        );
        received.Should().NotBeNull("Expected a SignalR notification for email draft generation");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var draftPayload = JsonSerializer.Deserialize<EmailDraftResponse>(
            received.Event.Payload.GetRawText(),
            options
        );

        draftPayload.Should().NotBeNull("Expected a deserializable EmailDraftResponse from SignalR");
        draftPayload.Subject.Should().NotBeNullOrWhiteSpace("The draft body should be generated");
        draftPayload.Body.Should().NotBeNullOrWhiteSpace("The draft body should be generated");

        /*
         This part Manual test:
         Need to switch to valid email address
        */

        //email = "shaharamr1041@gmail.com";

        //var sendRequest = new SendEmailRequest
        //{
        //    RecipientEmail = email,
        //    Subject = "Reminder: Meeting Tomorrow",
        //    Body = "<p>Don't forget about our meeting scheduled for tomorrow at 10am.</p>"
        //};

        //var sendResponse = await ClientFixture.Client.PostAsJsonAsync(ApiRoutes.SendEmail, sendRequest);
        //sendResponse.StatusCode.Should().Be(HttpStatusCode.OK);


        // Delete the test user
        await ClientFixture.LoginAsync(Role.Admin);

        var deleteResponse = await Client.DeleteAsync(ApiRoutes.UserById(userId));
        deleteResponse.ShouldBeOk();
    }

}
