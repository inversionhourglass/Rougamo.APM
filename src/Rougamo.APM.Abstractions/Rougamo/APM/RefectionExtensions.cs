using Rougamo.APM.Serialization;
using Rougamo.Context;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Rougamo.APM
{
    /// <summary>
    /// </summary>
    public static class RefectionExtensions
    {
        private static ConcurrentDictionary<MethodBase, long> _Ignores = new ConcurrentDictionary<MethodBase, long>();

        private static ConcurrentDictionary<MethodBase, long> _Records = new ConcurrentDictionary<MethodBase, long>();

        /// <summary>
        /// get ignores arguments and return value which marked by <see cref="ApmIgnoreAttribute"/>
        /// </summary>
        public static long GetApmIgnores(this MethodBase method)
        {
            return _Ignores.GetOrAdd(method, GetIgnores);
        }

        private static long GetIgnores(MethodBase method)
        {
            var ignores = 0L;
            var parameters = method.GetParameters();
            var length = parameters.Length > 63 ? 63 : parameters.Length;
            for (var i = 0; i < length; i++)
            {
                var paraAttrs = parameters[i].GetCustomAttributes(typeof(ApmIgnoreAttribute), true);
                if (paraAttrs.Length != 0)
                {
                    ignores |= 1L << i;
                }
            }
            if(method is MethodInfo methodInfo)
            {
                var returnAttrs = methodInfo.ReturnParameter.GetCustomAttributes(typeof(ApmIgnoreAttribute), true);
                if(returnAttrs.Length != 0)
                {
                    ignores |= 1L << 63;
                }
            }

            return ignores;
        }

        /// <summary>
        /// get record arguments and return value which marked by <see cref="ApmRecordAttribute"/>
        /// </summary>
        public static long GetApmRecords(this MethodBase method)
        {
            return _Records.GetOrAdd(method, GetRecords);
        }

        private static long GetRecords(MethodBase method)
        {
            var records = 0L;
            var parameters = method.GetParameters();
            var length = parameters.Length > 63 ? 63 : parameters.Length;
            for (var i = 0; i < length; i++)
            {
                var paraAttrs = parameters[i].GetCustomAttributes(typeof(ApmRecordAttribute), true);
                if (paraAttrs.Length != 0)
                {
                    records |= 1L << i;
                }
            }
            if (method is MethodInfo methodInfo)
            {
                var returnAttrs = methodInfo.ReturnParameter.GetCustomAttributes(typeof(ApmRecordAttribute), true);
                if (returnAttrs.Length != 0)
                {
                    records |= 1L << 63;
                }
            }

            return records;
        }

        /// <summary>
        /// get method parameter recording string by <see cref="ApmIgnoreAttribute"/>
        /// </summary>
        public static string GetMethodParametersByIgnore(this MethodContext context, ISerializer serializer)
        {
            var ignores = context.Method.GetApmIgnores() & 0x7FFFFFFFFFFFFFFF;
            var count = context.Arguments.Length;
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                ignores >>= 1;
                var value = (ignores & 1) == 1 ? "***" : context.Arguments[i];
                builder.Append($"arg{i}={serializer.Serialize(value)}&");
            }
            return builder.ToString();
        }

        /// <summary>
        /// get method return value string by <see cref="ApmIgnoreAttribute"/>
        /// </summary>
        public static string GetMethodReturnValueByIgnore(this MethodContext context, ISerializer serializer)
        {
            if (context.ReturnValue == null) return null;

            var ignores = context.Method.GetApmIgnores();
            return ignores >= 0 ? "***" : serializer.Serialize(context.ReturnValue);
        }

        /// <summary>
        /// get method parameter recording string by <see cref="ApmRecordAttribute"/>
        /// </summary>
        public static string GetMethodParametersByRecord(this MethodContext context, ISerializer serializer)
        {
            var records = context.Method.GetApmRecords() & 0x7FFFFFFFFFFFFFFF;
            var count = context.Arguments.Length;
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                records >>= 1;
                var value = (records & 1) == 1 ? context.Arguments[i] : "***";
                builder.Append($"arg{i}={serializer.Serialize(value)}&");
            }
            return builder.ToString();
        }

        /// <summary>
        /// get method return value string by <see cref="ApmRecordAttribute"/>
        /// </summary>
        public static string GetMethodReturnValueByRecord(this MethodContext context, ISerializer serializer)
        {
            if (context.ReturnValue == null) return null;

            var records = context.Method.GetApmRecords();
            return records >= 0 ? serializer.Serialize(context.ReturnValue) : "***";
        }
    }
}
