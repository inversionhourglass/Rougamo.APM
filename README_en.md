# Rougamo.APM

APM stands for Application Performance Management. Some common APM tools include [Pinpoint](https://github.com/pinpoint-apm/pinpoint), [Zipkin](https://github.com/openzipkin/zipkin), [SkyWalking](https://github.com/apache/skywalking), [CAT](https://github.com/dianping/cat), and [Jaeger](https://github.com/jaegertracing/jaeger). These popular APM tools typically come with built-in IO-level agents, which allow us to quickly integrate and record IO performance. However, sometimes IO integration alone might not be sufficient for thorough analysis and problem-solving. In such cases, we might need to manually add some instrumentation, which can be intrusive and cumbersome. To avoid this, we can use AOP for instrumentation, and [Rougamo](https://github.com/inversionhourglass/Rougamo), as a static AOP component, can effectively handle this task.

This project does not include any specific APM implementations but provides basic implementations and [proxy weaving](#代理织入) methods for projects using [Rougamo](https://github.com/inversionhourglass/Rougamo) for APM code weaving. For specific APM implementations, refer to [Rougamo.OpenTelemetry (0.1.1 released)](https://github.com/inversionhourglass/Rougamo.OpenTelemetry) and [Rougamo.SkyWalking (in development)](https://github.com/inversionhourglass/Rougamo.Skywalking).

## Rougamo.APM.Abstractions

### Common Attributes

`Rougamo.APM.Abstractions` defines several common attributes to avoid redundant definitions across different APM implementations and provides implementations for retrieving these attributes.

| Attribute                  | Target      | Purpose                                                                                                        |
|----------------------------|-------------|----------------------------------------------------------------------------------------------------------------|
| SpanAttribute              | Method       | Marker attribute for proxy weaving that indicates a Span will be recorded for the marked method. By default, it records method parameters and return values. |
| PureSpanAttribute          | Method       | Marker attribute for proxy weaving that indicates a Span will be recorded for the marked method but does <font color=red>not</font> record method parameters and return values by default. |
| ApmIgnoreAttribute         | Parameter, Return Value | Used with `SpanAttribute` to ignore specified parameters and return values from being recorded.              |
| ApmRecordAttribute         | Parameter, Return Value | Used with `PureSpanAttribute` to record specified parameters and return values.                                  |
| ApmExceptionAnnounceAttribute | Method    | Ensures that exceptions are recorded even if they have been logged before. By default, exceptions are recorded only once, but all Spans with unhandled exceptions are marked as failed. |

```csharp
// Example of ApmExceptionAnnounceAttribute
// In the example below, since M3 has an unhandled exception, the Span on M3 will record the exception and mark its status as failed.
// M2, which calls M3 and does not catch the exception, will only mark its Span as failed without recording the exception because M3 has already recorded it.
// M1, which calls M2 and does not catch the exception, will also mark its Span as failed and will record the exception information due to the addition of ApmExceptionAnnounceAttribute.

[ApmExceptionAnnounce]
[Span]
void M1()
{
    M2();
}
[Span]
void M2()
{
    M3();
}
[Span]
void M3()
{
    throw new NotImplementedException();
}
```

### Proxy Weaving

`Rougamo.APM.Abstractions` defines several attributes that are markers and do not directly use `Rougamo` for code weaving. To weave AOP based on these markers, we need to utilize `Rougamo`'s [proxy weaving](https://github.com/inversionhourglass/Rougamo/blob/master/README.md#attribute%E4%BB%A3%E7%90%86%E7%BB%87%E5%85%A5moproxyattribute) functionality. For example, proxy weaving with [Rougamo.OpenTelemetry](https://github.com/inversionhourglass/Rougamo.OpenTelemetry) can be done as follows; see the [full example](https://github.com/inversionhourglass/Rougamo.OpenTelemetry/blob/master/test/sample/Rougamo.OpenTelemetryJaegerTest.AspNetCore/Utils/RandomUtils.cs) in the `Rougamo.OpenTelemetry` project:

```csharp
[assembly: MoProxy(typeof(SpanAttribute), typeof(OtelAttribute))]
[assembly: MoProxy(typeof(PureSpanAttribute), typeof(PureOtelAttribute))]

// Implementation code weaving OtelAttribute
[Span]
void M1()
{
}
// Implementation code weaving PureOtelAttribute
[PureSpan]
void M2()
{
}
```

## Rougamo.APM.JsonSerialization

When recording parameters and return values, they need to be serialized into strings. By default, [ToStringSerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs) is used, which directly calls the object's `ToString` method. You can implement your own [ISerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs) and replace the default `ToStringSerializer` as per the specific requirements of your APM implementation (usually by registering a singleton in `IServiceCollection`). The `Rougamo.APM.JsonSerialization` project provides a JSON serialization implementation using `Newtonsoft.Json`, and can be registered with `services.AddRougamoJsonSerializer()`:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddRougamoJsonSerializer(); // Use default serialization configuration

        services.AddRougamoJsonSerializer(settings =>
        {
            // Modify Newtonsoft.Json serialization configuration
        });

        // ...
    }
}
```