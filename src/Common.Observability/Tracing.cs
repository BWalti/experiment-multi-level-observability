using System.Diagnostics;

namespace Common.Observability;

public static class Tracing
{
    private static ActivitySource _activitySource = new ("unknown");

    public static ActivitySource Source
    {
        get => _activitySource;
        internal set => _activitySource = value;
    }
}