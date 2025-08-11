using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Dapr.Client;

namespace Manager.UnitTests.TestHelpers;

public static class DaprTestHelpers
{
    /// <summary>
    /// Creates a Dapr InvocationException that looks like a 404 from the Accessor app,
    /// without taking a hard dependency on internal types that can move between versions.
    /// </summary>
    public static InvocationException NotFoundInvocation(
        string appId = "accessor",
        string methodName = "task/any")
    {
        // Find the internal wrapper type in the Dapr.Client assembly
        var daprAsm = typeof(DaprClient).Assembly;
        var wrapperType =
            daprAsm.GetType("Dapr.Client.Http.Client.HttpResponseMessageWrapper") ??
            daprAsm.GetType("Dapr.Client.Http.HttpResponseMessageWrapper")
            ?? throw new InvalidOperationException("Could not find HttpResponseMessageWrapper in Dapr.Client assembly.");

        // Construct the wrapper: ctor(HttpResponseMessage, string)
        var http = new HttpResponseMessage(HttpStatusCode.NotFound);
        var wrapper = Activator.CreateInstance(wrapperType, http, "404")
                      ?? throw new InvalidOperationException("Failed to create HttpResponseMessageWrapper instance.");

        // Find a public InvocationException ctor that accepts the wrapper type as any parameter.
        var invType = typeof(InvocationException);
        var ctor = invType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Any(p => p.ParameterType == wrapperType))
            ?? throw new InvalidOperationException("Suitable InvocationException constructor not found.");

        // Build argument list based on the selected ctor signature.
        var parms = ctor.GetParameters();
        var args = new object?[parms.Length];

        for (int i = 0; i < parms.Length; i++)
        {
            var p = parms[i];
            if (p.ParameterType == wrapperType)
                args[i] = wrapper;
            else if (p.ParameterType == typeof(string))
                args[i] = i == 0 ? methodName : appId; // best-effort fill for (methodName, appId, wrapper)
            else if (typeof(Exception).IsAssignableFrom(p.ParameterType))
                args[i] = new HttpRequestException("404 Not Found");
            else
                args[i] = null;
        }

        return (InvocationException)ctor.Invoke(args);
    }
}