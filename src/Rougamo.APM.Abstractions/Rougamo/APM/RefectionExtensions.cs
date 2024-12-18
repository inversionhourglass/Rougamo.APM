﻿using Rougamo.APM.Serialization;
using Rougamo.Context;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rougamo.APM
{
    /// <summary>
    /// </summary>
    public static class RefectionExtensions
    {
        private const int ARGS_FLAG_COUNT = 48;
        private const long ARGS_FLAG_MASK = 0X7FFFFFFFFFFFFFFF >> (63 - ARGS_FLAG_COUNT);
        private const int RETURN_POSITION = 63;
        private const int THROWS_POSITION = 62;
        /*
         * is record return value
         * |
         * |                   |-------------------48bits record args--------------------|
         * 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000
         *  |
         *  |
         * is force exception throws
         */
        private static ConcurrentDictionary<MethodBase, long> _Ignores = new ConcurrentDictionary<MethodBase, long>();

        private static ConcurrentDictionary<MethodBase, long> _Records = new ConcurrentDictionary<MethodBase, long>();

        /// <summary>
        /// Get ignores arguments and return value which marked by <see cref="ApmIgnoreAttribute"/>
        /// </summary>
        public static long GetApmIgnores(this MethodBase method)
        {
            return _Ignores.GetOrAdd(method, GetIgnores);
        }

        private static long GetIgnores(MethodBase method)
        {
            var ignores = 0L;
            var parameters = method.GetParameters();
            var length = parameters.Length > ARGS_FLAG_COUNT ? ARGS_FLAG_COUNT : parameters.Length;
            for (var i = 0; i < length; i++)
            {
                if(parameters[i].HasAttribute<ApmIgnoreAttribute>())
                {
                    ignores |= 1L << i;
                }
            }
            if(method.HasAttribute<ApmExceptionAnnounceAttribute>())
            {
                ignores |= 1L << THROWS_POSITION;
            }
            if(method is MethodInfo methodInfo)
            {
                if(methodInfo.ReturnParameter.HasAttribute<ApmIgnoreAttribute>())
                {
                    ignores |= 1L << RETURN_POSITION;
                }
            }

            return ignores;
        }

        /// <summary>
        /// Get record arguments and return value which marked by <see cref="ApmRecordAttribute"/>
        /// </summary>
        public static long GetApmRecords(this MethodBase method)
        {
            return _Records.GetOrAdd(method, GetRecords);
        }

        private static long GetRecords(MethodBase method)
        {
            var records = 0L;
            var parameters = method.GetParameters();
            var length = parameters.Length > ARGS_FLAG_COUNT ? ARGS_FLAG_COUNT : parameters.Length;
            for (var i = 0; i < length; i++)
            {
                if (parameters[i].HasAttribute<ApmRecordAttribute>())
                {
                    records |= 1L << i;
                }
            }
            if (method.HasAttribute<ApmExceptionAnnounceAttribute>())
            {
                records |= 1L << THROWS_POSITION;
            }
            if (method is MethodInfo methodInfo)
            {
                if (methodInfo.ReturnParameter.HasAttribute<ApmRecordAttribute>())
                {
                    records |= 1L << RETURN_POSITION;
                }
            }

            return records;
        }

        /// <summary>
        /// Get method parameter recording string, return null if no parameter need to record
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetMethodParameters(this MethodContext context, ISerializer serializer, bool recordByDefault)
        {
            return recordByDefault ? GetMethodParametersByIgnore(context, serializer) : GetMethodParametersByRecord(context, serializer);
        }

        /// <summary>
        /// Get method return value string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetMethodReturnValue(this MethodContext context, ISerializer serializer, bool recordByDefault)
        {
            return recordByDefault ? GetMethodReturnValueByIgnore(context, serializer) : GetMethodReturnValueByRecord(context, serializer);
        }

        /// <summary>
        /// Is mute exception for apm if it has been recorded
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMuteExceptionForApm(this MethodContext context, bool recordByDefault)
        {
            return recordByDefault ? IsMuteExceptionForApmByIgnores(context) : IsMuteExceptionForApmByRecord(context);
        }

        /// <summary>
        /// Get method parameter recording string by <see cref="ApmIgnoreAttribute"/>, return null if no parameter need to record
        /// </summary>
        public static string? GetMethodParametersByIgnore(this MethodContext context, ISerializer serializer)
        {
            var ignores = context.Method.GetApmIgnores() & ARGS_FLAG_MASK;
            var count = context.Arguments.Length;
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                if((ignores & 1) != 1)
                {
                    builder.Append($"arg{i}={serializer.Serialize(context.Arguments[i])}&");
                }
                ignores >>= 1;
            }
            return builder.Length == 0 ? null : builder.ToString();
        }

        /// <summary>
        /// Get method return value string by <see cref="ApmIgnoreAttribute"/>, return null if not record return value
        /// </summary>
        public static string? GetMethodReturnValueByIgnore(this MethodContext context, ISerializer serializer)
        {
            if (context.ReturnValue == null) return null;

            var ignores = context.Method.GetApmIgnores();
            return ignores >= 0 ? serializer.Serialize(context.ReturnValue) : null;
        }

        /// <summary>
        /// Get method parameter recording string by <see cref="ApmRecordAttribute"/>, return null if no parameter need to record
        /// </summary>
        public static string? GetMethodParametersByRecord(this MethodContext context, ISerializer serializer)
        {
            var records = context.Method.GetApmRecords() & ARGS_FLAG_MASK;
            var count = context.Arguments.Length;
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                if((records & 1) == 1)
                {
                    builder.Append($"arg{i}={serializer.Serialize(context.Arguments[i])}&");
                }
                records >>= 1;
            }
            return builder.Length == 0 ? null : builder.ToString();
        }

        /// <summary>
        /// Get method return value string by <see cref="ApmRecordAttribute"/>, return null if not record return value
        /// </summary>
        public static string? GetMethodReturnValueByRecord(this MethodContext context, ISerializer serializer)
        {
            if (context.ReturnValue == null) return null;

            var records = context.Method.GetApmRecords();
            return records >= 0 ? null : serializer.Serialize(context.ReturnValue);
        }

        /// <summary>
        /// Is mute exception for apm if it has been recorded
        /// </summary>
        public static bool IsMuteExceptionForApmByIgnores(this MethodContext context)
        {
            var throws = context.Method.GetApmIgnores() & (1L << THROWS_POSITION);
            return throws == 0;
        }

        /// <summary>
        /// Is mute exception for apm if it has been recorded
        /// </summary>
        public static bool IsMuteExceptionForApmByRecord(this MethodContext context)
        {
            var throws = context.Method.GetApmRecords() & (1L << THROWS_POSITION);
            return throws == 0;
        }

        private static bool HasAttribute<TAttribute>(this MethodBase method, bool inherit = true)
        {
            var attributes = method.GetCustomAttributes(typeof(TAttribute), inherit);
            return attributes.Length != 0;
        }

        private static bool HasAttribute<TAttribute>(this ParameterInfo parameter, bool inherit = true)
        {
            var attributes = parameter.GetCustomAttributes(typeof(TAttribute), inherit);
            return attributes.Length != 0;
        }
    }
}
