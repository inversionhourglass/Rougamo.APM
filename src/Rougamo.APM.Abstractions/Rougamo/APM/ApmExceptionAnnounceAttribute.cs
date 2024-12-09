using System;

namespace Rougamo.APM
{
    /// <summary>
    /// Do not mute the exception even it has been recorded
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApmExceptionAnnounceAttribute : Attribute
    {
    }
}
