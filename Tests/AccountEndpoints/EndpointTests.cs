using System.Reflection;

namespace Tests.AccountEndpoint;
public class EndpointsTest
{
    private static readonly MethodInfo? IsLocalPathMethod;

    static EndpointsTest()
    {
        // Get the type from the original namespace and class
        var type = Type.GetType("FloodOnlineReportingTool.Public.Endpoints.Account.AccountEndpoints, FloodOnlineReportingTool.Public");
        if (type == null)
        {
            // Fallback: try to find by enumerating loaded assemblies if the assembly-qualified name differs
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType("FloodOnlineReportingTool.Public.Endpoints.Account.AccountEndpoints");
                if (type != null) break;
            }
        }

        IsLocalPathMethod = type?.GetMethod("IsLocalPath", BindingFlags.NonPublic | BindingFlags.Static);
        if (IsLocalPathMethod == null)
            throw new InvalidOperationException("Could not find private static method IsLocalPath on AccountEndpoints.");
    }

    [Theory]
    [MemberData(nameof(GetCases))]
    public void IsLocalPath_ReturnsExpected(string? input, bool expected)
    {
        // Invoke the private static method via reflection
        var resultObj = IsLocalPathMethod!.Invoke(null, new object?[] { input });
        Assert.IsType<bool>(resultObj);
        var actual = (bool)resultObj!;
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object?[]> GetCases()
    {
        // null and whitespace cases
        yield return new object?[] { null, false };
        yield return new object?[] { "", false };
        yield return new object?[] { "   ", false };

        // absolute URIs
        yield return new object?[] { "http://example.com", false };
        yield return new object?[] { "https://example.com/path", false };
        yield return new object?[] { "mailto:someone@example.com", false };

        // protocol-relative
        yield return new object?[] { "//example.com", false };

        // Windows absolute path (likely treated as absolute)
        yield return new object?[] { @"C:\temp\file.txt", false };

        // relative paths - expected to be accepted
        yield return new object?[] { "contacts", true };
        yield return new object?[] { "report-flooding/contacts", true };
        yield return new object?[] { "/report-flooding", true };
    }
}