namespace GraphBackend.Domain.Exceptions;

public class StatusBasedException : Exception
{
    public int StatusCode { get; private set; }
    public StatusBasedException(string? msg, int statusCode) : base(msg)
    {
        StatusCode = statusCode;
    }
}
    
public class BadRequest400Exception : StatusBasedException { public BadRequest400Exception(string? msg) : base(msg, 400) { } }
public class Forbidden403Exception : StatusBasedException { public Forbidden403Exception(string? msg) : base(msg, 403) { } }
public class NotFound404Exception : StatusBasedException { public NotFound404Exception(string? msg) : base(msg, 404) { } }
public class ImATeapot418Exception : StatusBasedException { public ImATeapot418Exception(string? msg) : base(msg, 418) { } }
public class InternalError500Exception : StatusBasedException { public InternalError500Exception(string? msg) : base(msg, 500) { } }

public enum LockedExceptionTypes
{
    DisciplineOtherPredicatesExist = 1,
    DisciplineExistsInStudyLoadsV2 = 2,
    NotificationChannelIsSystem = 3
}
public class Locked423Exception : StatusBasedException
{
    public LockedExceptionTypes Type { get; }
    public List<LockedExceptionTypes> ConfirmedWarnings { get; }

    private Locked423Exception(string? msg, LockedExceptionTypes type, List<LockedExceptionTypes> confirmedWarnings) : base(msg, 423)
    {
        Type = type;
        ConfirmedWarnings = confirmedWarnings;
    }

    public static void ThrowIfNotConfirmed(LockedExceptionTypes type, string msg, List<LockedExceptionTypes>? confirmedWarnings)
    {
        if (confirmedWarnings is null || !confirmedWarnings.Contains(type))
            throw new Locked423Exception(msg, type, confirmedWarnings ?? []);
    }
}