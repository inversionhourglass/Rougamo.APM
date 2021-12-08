using System;

namespace Rougamo.APM
{
    /// <summary>
    /// do not mute the exception even it has been recorded
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApmExceptionAnnounceAttribute : Attribute
    {
    }
}
