namespace PennyWise.Core;

/// <summary>Small result type so services can report failures without exceptions.</summary>
public readonly record struct Result(bool Success, string? ErrorMessage)
{
    public static Result Ok() => new(true, null);

    public static Result Fail(string message) => new(false, message);
}
