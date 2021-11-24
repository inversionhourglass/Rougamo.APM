using System;

namespace Rougamo.APM
{
    /// <summary>
    /// ignore parameter value record
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class ApmIgnoreAttribute : Attribute
    {
    }
}
