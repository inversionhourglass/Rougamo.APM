using Rougamo.APM.Serialization;
using Rougamo.Context;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Rougamo.APM
{
    public static class RefectionExtensions
    {
        private static ConcurrentDictionary<MethodBase, long> _Ignores = new ConcurrentDictionary<MethodBase, long>();

        public static long GetIgnoreArgs(this MethodBase method)
        {
            return _Ignores.GetOrAdd(method, GetIgnores);
        }

        private static long GetIgnores(MethodBase method)
        {
            var ignores = 0L;
            var parameters = method.GetParameters();
            var methodAttrs = method.GetCustomAttributes(typeof(ApmIgnoreAttribute), true);
            var length = parameters.Length > 64 ? 64 : parameters.Length;
            if (methodAttrs.Length != 0)
            {
                for (var i = 0; i < length; i++)
                {
                    ignores |= 1L << i;
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    var paraAttrs = parameters[i].GetCustomAttributes(typeof(ApmIgnoreAttribute), true);
                    if (paraAttrs.Length != 0)
                    {
                        ignores |= 1L << i;
                    }
                }
            }

            return ignores;
        }

        public static string GetMethodParameters(this MethodContext context, ISerializer serializer)
        {
            var ignores = context.Method.GetIgnoreArgs();
            var count = context.Arguments.Length;
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                var value = i > 64 || (ignores & (1 << i)) != 0 ? "***" : context.Arguments[i];
                builder.Append($"arg{i}={serializer.Serialize(value)}&");
            }
            return builder.ToString();
        }
    }
}
