using System;

namespace Rougamo.APM
{
    /// <summary>
    /// Ignore parameter value record, working with <see cref="SpanAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ApmRecordAttribute : Attribute
    {
    }
}
