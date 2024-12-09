# Rougamo.APM

中文 | [English](https://github.com/inversionhourglass/Rougamo.APM/blob/master/README_en.md)

APM全称Application Performance Management，
现在常见的APM有[Pinpoint](https://github.com/pinpoint-apm/pinpoint), [Zipkin](https://github.com/openzipkin/zipkin),
[SkyWalking](https://github.com/apache/skywalking), [CAT](https://github.com/dianping/cat), [jaeger](https://github.com/jaegertracing/jaeger)
等，这些流行的APM一般都自带IO层面的agent，使用这些agent我们可以快速的将应用接入并记录IO耗时，但有时候我们可能会感觉到仅仅是IO接入并不能很好
的分析和解决问题，这时我们可能会手动的添加一些埋点，而这种操作是侵入式的且繁琐的，为了避免这个问题，我们会想到使用AOP进行埋点，而
[Rougamo](https://github.com/inversionhourglass/Rougamo)作为一个静态AOP组件，自然也能很好的完成这一任务。

该项目并不包含任何APM的具体实现，主要是为所有使用[Rougamo](https://github.com/inversionhourglass/Rougamo)进行APM代码织入的项目提供一些基础实现
和[代理织入](#代理织入)的方式，具体的APM实现可以移步[Rougamo.OpenTelemetry(0.1.1已发布)](https://github.com/inversionhourglass/Rougamo.OpenTelemetry)和
[Rougamo.SkyWalking(开发中)](https://github.com/inversionhourglass/Rougamo.Skywalking)

## Rougamo.APM.Abstractions

### 公共Attribute

`Rougamo.APM.Abstractions`中定义了一些公共Attribute，这样可以避免不同的APM实现中进行重复的定义，并且提供了获取这些公共Attribute的实现

|Attribute|Target|意图|
|:--:|:--:|:--|
|SpanAttribute|方法|代理织入的标记Attribute，标记了该Attribute的方法会记录一个Span，默认会记录方法的参数值和返回值|
|PureSpanAttribute|方法|代理织入的标记Attribute，标记了该Attribute的方法会记录一个Span，默认<font color=red>不会</font>记录方法的参数值和返回值|
|ApmIgnoreAttribute|参数,返回值|与`SpanAttribute`结合使用，忽略指定参数和返回值的记录|
|ApmRecordAttribute|参数,返回值|与`PureSpanAttribute`结合使用，记录指定参数和返回值|
|ApmExceptionAnnounceAttribute|方法|如果方法有未处理的异常，默认该异常只会记录一次，但所有未处理该异常的方法的Span的状态都会设置为失败，使用该Attribute会强制记录异常信息，即使该异常已经记录过，详见下面示例|

```csharp
// 关于ApmExceptionAnnounce作用的示例
// 下面的示例中，由于M3中有未处理的异常，M3上的Span会记录异常信息并将自己的状态设置为失败，
// M2调用M3同时也没有捕获处理异常，但由于该异常已经由M3记录，所以M2的Span只会讲自己的状态设置为失败，并不会记录异常信息，
// M1调用M2同时也没有捕获处理异常，但由于添加了ApmExceptionAnnounceAttribute，所以除了将状态设置为失败外，还会记录异常信息

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

### 代理织入

`Rougamo.APM.Abstractions`中定义了数个Attribute，这些Attribute本身并不会使用`Rougamo`进行代码织入，它们是一种标记，
而要通过这些标记进行AOP织入，我们需要借助`Rougamo`的[代理织入](https://github.com/inversionhourglass/Rougamo/blob/master/README.md#attribute%E4%BB%A3%E7%90%86%E7%BB%87%E5%85%A5moproxyattribute)
功能了，比如下面我们通过代理织入[Rougamo.OpenTelemetry](https://github.com/inversionhourglass/Rougamo.OpenTelemetry)
代码，可到`Rougamo.OpenTelemetry`项目中查看[完整实例](https://github.com/inversionhourglass/Rougamo.OpenTelemetry/blob/master/test/sample/Rougamo.OpenTelemetryJaegerTest.AspNetCore/Utils/RandomUtils.cs)
```csharp
[assembly: MoProxy(typeof(SpanAttribute), typeof(OtelAttribute))]
[assembly: MoProxy(typeof(PureSpanAttribute), typeof(PureOtelAttribute))]

// 将织入OtelAttribute的实现代码
[Span]
void M1()
{
}
// 将织入PureOtelAttribute的实现代码
[PureSpan]
void M2()
{
}
```

## Rougamo.APM.JsonSerialization

在记录参数和返回值时需要先将其序列化为字符串，默认使用的是[ToStringSerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs)，
实现是直接调用对象的`ToString`方法，可以自己实现[ISerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs)
并根据具体APM实现的要求替换掉默认的`ToStringSerializer`（一般通过`IServiceCollection`注册单例的方式替换）。`Rougamo.APM.JsonSerialization`项目就是提供了Json的序列化实现，
项目使用`Newtonsoft.Json`进行序列化，通过`services.AddRougamoJsonSerializer()`进行注册。
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddRougamoJsonSerializer(); // 使用默认序列化配置

        services.AddRougamoJsonSerializer(settings =>
        {
            // 修改Newtonsoft.Json的序列化配置
        });

        // ...
    }
}
```