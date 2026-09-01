namespace Solari.Sdk.Tests;

/// <summary>Minimal assertion helper - this test project intentionally has zero
/// NuGet dependencies (see README) so it can build offline; think of this as
/// the handful of xunit.Assert members these tests actually use.</summary>
internal static class Assert
{
    public static void Equal<T>(T expected, T actual, string? because = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"Expected '{expected}' but got '{actual}'.{(because is null ? "" : " " + because)}");
        }
    }

    public static void True(bool condition, string? because = null)
    {
        if (!condition)
        {
            throw new Exception($"Expected condition to be true.{(because is null ? "" : " " + because)}");
        }
    }

    public static void Contains(string expectedSubstring, string actual, string? because = null)
    {
        if (!actual.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new Exception($"Expected '{actual}' to contain '{expectedSubstring}'.{(because is null ? "" : " " + because)}");
        }
    }

    public static async Task ThrowsAsync<TException>(Func<Task> action, string? because = null) where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new Exception($"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
        }

        throw new Exception($"Expected {typeof(TException).Name} but no exception was thrown.{(because is null ? "" : " " + because)}");
    }
}
