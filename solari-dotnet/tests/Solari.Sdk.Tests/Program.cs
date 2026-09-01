using System.Reflection;
using Solari.Sdk.Tests;

// This is a hand-rolled test runner, not xunit. Why: this SDK was built in a
// sandbox where outbound access to nuget.org is blocked by network policy,
// so `dotnet add package xunit` can't restore there. The Solari.Sdk library
// itself has zero external NuGet dependencies for the same reason (see its
// .csproj comments), so these tests only need what already ships in the
// .NET SDK.
//
// On a normal machine with NuGet access, swap this project back to the
// standard `dotnet new xunit` template (or nunit/mstest) - the test bodies
// in SolariClientTests.cs translate 1:1 to `[Fact]` methods; nothing here
// depends on this runner's shape.

int passed = 0;
int failed = 0;

foreach (var method in typeof(SolariClientTests)
             .GetMethods(BindingFlags.Public | BindingFlags.Static)
             .OrderBy(m => m.Name))
{
    try
    {
        method.Invoke(null, null);
        Console.WriteLine($"PASS  {method.Name}");
        passed++;
    }
    catch (TargetInvocationException tie)
    {
        Console.WriteLine($"FAIL  {method.Name}: {tie.InnerException?.Message ?? tie.Message}");
        failed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL  {method.Name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine();
Console.WriteLine($"{passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;
