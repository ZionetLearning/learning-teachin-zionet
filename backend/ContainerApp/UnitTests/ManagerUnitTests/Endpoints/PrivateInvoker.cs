using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace ManagerUnitTests.Endpoints;

internal static class PrivateInvoker
{
    public static async Task<IResult> InvokePrivateEndpointAsync(
        Type endpointsType,
        string methodName,
        params object?[] suppliedArgs)
    {
        var mi = endpointsType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
                 ?? throw new MissingMethodException(endpointsType.FullName, methodName);

        var parameters = mi.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var pType = parameters[i].ParameterType;

            if (i < suppliedArgs.Length && suppliedArgs[i] is not null && pType.IsInstanceOfType(suppliedArgs[i]))
            {
                args[i] = suppliedArgs[i];
                continue;
            }

            if (pType == typeof(CancellationToken))
            {
                args[i] = CancellationToken.None;
                continue;
            }

            if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(ILogger<>))
            {
                var t1 = pType.GetGenericArguments()[0];
                var nullLoggerGeneric = typeof(NullLogger<>).MakeGenericType(t1);

                var prop = nullLoggerGeneric.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                object? instance = prop?.GetValue(null);

                if (instance is null)
                {
                    var field = nullLoggerGeneric.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                    instance = field?.GetValue(null);
                }

                if (instance is null)
                    throw new InvalidOperationException($"NullLogger<{t1.FullName}>.Instance not found.");

                args[i] = instance;
                continue;
            }

            args[i] = i < suppliedArgs.Length ? suppliedArgs[i] :
                      pType.IsValueType ? Activator.CreateInstance(pType)! : null;
        }

        var resultObj = mi.Invoke(null, args);

        if (resultObj is IResult r) return r;
        if (resultObj is Task<IResult> tr) return await tr;

        if (resultObj is Task t)
        {
            await t.ConfigureAwait(false);
            var resProp = t.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (resProp?.GetValue(t) is IResult rx) return rx;
        }

        throw new InvalidOperationException($"Method {endpointsType.Name}.{methodName} didn't return IResult/Task<IResult>.");
    }
}
