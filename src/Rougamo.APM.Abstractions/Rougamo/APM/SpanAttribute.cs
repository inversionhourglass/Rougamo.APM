using System;

namespace Rougamo.APM
{
    /// <summary>
    /// span annotation only
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SpanAttribute : Attribute
    {
    }
}
