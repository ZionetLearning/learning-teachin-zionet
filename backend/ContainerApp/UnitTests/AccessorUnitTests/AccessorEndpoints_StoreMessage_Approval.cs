// File: AccessorEndpoints_StoreMessage_Approval.cs
using System.Reflection;
using Newtonsoft.Json;
using ApprovalTests;
using ApprovalTests.Namers;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class AccessorEndpoints_StoreMessage_Approval
{
    private static Task<IResult> InvokeStoreMessageAsync(ChatMessage m, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
              .GetMethod("StoreMessageAsync", BindingFlags.NonPublic | BindingFlags.Static)!
              .Invoke(null, new object[] { m, s, l })!;

    //[Fact]
    //public async Task StoreMessage_CreatedAtRoute_BodyAndHeaders_Are_Approved()
    //{
    //    ApprovalSetup.Init();

    //    var svc = new Mock<IAccessorService>(MockBehavior.Strict);
    //    var log = new Mock<ILogger<AccessorService>>();
    //    ChatMessage? saved = null;

    //    svc.Setup(s => s.AddMessageAsync(It.IsAny<ChatMessage>()))
    //       .Callback<ChatMessage>(m => saved = m)
    //       .Returns(Task.CompletedTask);

    //    var threadId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    //    var msg = new ChatMessage
    //    {
    //        ThreadId = threadId,
    //        Content = "Hello, how are you?",
    //        Role = MessageRole.User
    //    };

    //    var result = await InvokeStoreMessageAsync(msg, svc.Object, log.Object);
    //    var created = Assert.IsType<CreatedAtRoute<ChatMessage>>(result);

    //    // Build a normalized snapshot object (no flakiness)
    //    var snapshot = new
    //    {
    //        routeName = created.RouteName,   // should be "GetChatHistory"
    //        routeValues = created.RouteValues?.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString()),
    //        message = new
    //        {
    //            Id = "GUID",
    //            ThreadId = created.Value.ThreadId,
    //            UserId = created.Value.UserId,
    //            Role = created.Value.Role.ToString(),
    //            Content = created.Value.Content,
    //            Timestamp = "2020-01-01T00:00:00Z"
    //        }
    //    };

    //    NamerFactory.AdditionalInformation = "StoreMessage_CreatedAtRoute";
    //    Approvals.VerifyJson(JsonConvert.SerializeObject(snapshot, Formatting.Indented));
    //    NamerFactory.AdditionalInformation = null;

    //    svc.VerifyAll();
    //    Assert.NotNull(saved); // service was called with server-side mutated message
    //}
}
