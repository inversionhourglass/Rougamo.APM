using Newtonsoft.Json;
using Rougamo.APM.Serialization;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// </summary>
    public static class RougamoSkyJsonSerializeExtensions
    {
        /// <summary>
        /// change default parameter and return value serializer from <see cref="ToStringSerializer"/> to <see cref="Rougamo.APM.Serialization.JsonSerializer"/>
        /// </summary>
        public static IServiceCollection AddRougamoJsonSerializer(this IServiceCollection services, Action<JsonSerializerSettings> settingAction = null)
        {
            services.AddOptions<JsonSerializerSettings>(Rougamo.APM.Serialization.JsonSerializer.OPTIONS_NAME).Configure(settings =>
            {
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
                settings.NullValueHandling = NullValueHandling.Ignore;

                settingAction?.Invoke(settings);
            });
            services.AddSingleton<ISerializer, Rougamo.APM.Serialization.JsonSerializer>();

            return services;
        }
    }
}
