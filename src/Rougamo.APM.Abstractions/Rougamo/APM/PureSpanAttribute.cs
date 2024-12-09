using System;

namespace Rougamo.APM
{
    /// <summary>
    /// Span annotation, not record parameter and return value by default
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PureSpanAttribute : Attribute
    {
    }
}
