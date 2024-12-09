using System;

namespace Rougamo.APM
{
    /// <summary>
    /// Span annotation, record parameter and return value by default
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SpanAttribute : Attribute
    {
    }
}
