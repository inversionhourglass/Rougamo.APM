# Rougamo.APM

APMȫ��Application Performance Management��
���ڳ�����APM��[Pinpoint](https://github.com/pinpoint-apm/pinpoint), [Zipkin](https://github.com/openzipkin/zipkin),
[SkyWalking](https://github.com/apache/skywalking), [CAT](https://github.com/dianping/cat), [jaeger](https://github.com/jaegertracing/jaeger)
�ȣ���Щ���е�APMһ�㶼�Դ�IO�����agent��ʹ����Щagent���ǿ��Կ��ٵĽ�Ӧ�ý��벢��¼IO��ʱ������ʱ�����ǿ��ܻ�о���������IO���벢���ܺܺ�
�ķ����ͽ�����⣬��ʱ���ǿ��ܻ��ֶ������һЩ��㣬�����ֲ���������ʽ���ҷ����ģ�Ϊ�˱���������⣬���ǻ��뵽ʹ��AOP������㣬��
[Rougamo](https://github.com/inversionhourglass/Rougamo)��Ϊһ����̬AOP�������ȻҲ�ܺܺõ������һ����

����Ŀ���������κ�APM�ľ���ʵ�֣���Ҫ��Ϊ����ʹ��[Rougamo](https://github.com/inversionhourglass/Rougamo)����APM����֯�����Ŀ�ṩһЩ����ʵ��
��[����֯��](#����֯��)�ķ�ʽ�������APMʵ�ֿ����Ʋ�[Rougamo.OpenTelemetry(0.1.1�ѷ���)](https://github.com/inversionhourglass/Rougamo.OpenTelemetry)��
[Rougamo.SkyWalking(������)](https://github.com/inversionhourglass/Rougamo.Skywalking)

## Rougamo.APM.Abstractions
### ����Attribute
`Rougamo.APM.Abstractions`�ж�����һЩ����Attribute���������Ա��ⲻͬ��APMʵ���н����ظ��Ķ��壬�����ṩ�˻�ȡ��Щ����Attribute��ʵ��

|Attribute|Target|��ͼ|
|:--:|:--:|:--|
|SpanAttribute|����|����֯��ı��Attribute������˸�Attribute�ķ������¼һ��Span��Ĭ�ϻ��¼�����Ĳ���ֵ�ͷ���ֵ|
|PureSpanAttribute|����|����֯��ı��Attribute������˸�Attribute�ķ������¼һ��Span��Ĭ��<font color=red>����</font>��¼�����Ĳ���ֵ�ͷ���ֵ|
|ApmIgnoreAttribute|����,����ֵ|��`SpanAttribute`���ʹ�ã�����ָ�������ͷ���ֵ�ļ�¼|
|ApmRecordAttribute|����,����ֵ|��`PureSpanAttribute`���ʹ�ã���¼ָ�������ͷ���ֵ|
|ApmExceptionAnnounceAttribute|����|���������δ������쳣��Ĭ�ϸ��쳣ֻ���¼һ�Σ�������δ������쳣�ķ�����Span��״̬��������Ϊʧ�ܣ�ʹ�ø�Attribute��ǿ�Ƽ�¼�쳣��Ϣ����ʹ���쳣�Ѿ���¼�����������ʾ��|

```csharp
// ����ApmExceptionAnnounce���õ�ʾ��
// �����ʾ���У�����M3����δ������쳣��M3�ϵ�Span���¼�쳣��Ϣ�����Լ���״̬����Ϊʧ�ܣ�
// M2����M3ͬʱҲû�в������쳣�������ڸ��쳣�Ѿ���M3��¼������M2��Spanֻ�ὲ�Լ���״̬����Ϊʧ�ܣ��������¼�쳣��Ϣ��
// M1����M2ͬʱҲû�в������쳣�������������ApmExceptionAnnounceAttribute�����Գ��˽�״̬����Ϊʧ���⣬�����¼�쳣��Ϣ

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

### ����֯��
`Rougamo.APM.Abstractions`�ж���������Attribute����ЩAttribute��������ʹ��`Rougamo`���д���֯�룬������һ�ֱ�ǣ�
��Ҫͨ����Щ��ǽ���AOP֯�룬������Ҫ����`Rougamo`��[����֯��](https://github.com/inversionhourglass/Rougamo/blob/master/README.md#attribute%E4%BB%A3%E7%90%86%E7%BB%87%E5%85%A5moproxyattribute)
�����ˣ�������������ͨ������֯��[Rougamo.OpenTelemetry](https://github.com/inversionhourglass/Rougamo.OpenTelemetry)
���룬�ɵ�`Rougamo.OpenTelemetry`��Ŀ�в鿴[����ʵ��](https://github.com/inversionhourglass/Rougamo.OpenTelemetry/blob/master/test/sample/Rougamo.OpenTelemetryJaegerTest.AspNetCore/Utils/RandomUtils.cs)
```csharp
[assembly: MoProxy(typeof(SpanAttribute), typeof(OtelAttribute))]
[assembly: MoProxy(typeof(PureSpanAttribute), typeof(PureOtelAttribute))]

// ��֯��OtelAttribute��ʵ�ִ���
[Span]
void M1()
{
}
// ��֯��PureOtelAttribute��ʵ�ִ���
[PureSpan]
void M2()
{
}
```

## Rougamo.APM.JsonSerialization
�ڼ�¼�����ͷ���ֵʱ��Ҫ�Ƚ������л�Ϊ�ַ�����Ĭ��ʹ�õ���[ToStringSerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs)��
ʵ����ֱ�ӵ��ö����`ToString`�����������Լ�ʵ��[ISerializer](https://github.com/inversionhourglass/Rougamo.APM/blob/master/src/Rougamo.APM.Abstractions/Rougamo/APM/Serialization/ToStringSerializer.cs)
�����ݾ���APMʵ�ֵ�Ҫ���滻��Ĭ�ϵ�`ToStringSerializer`��һ��ͨ��`IServiceCollection`ע�ᵥ���ķ�ʽ�滻����`Rougamo.APM.JsonSerialization`��Ŀ�����ṩ��Json�����л�ʵ�֣�
��Ŀʹ��`Newtonsoft.Json`�������л���ͨ��`services.AddRougamoJsonSerializer()`����ע�ᡣ
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddRougamoJsonSerializer(); // ʹ��Ĭ�����л�����

        services.AddRougamoJsonSerializer(settings =>
        {
            // �޸�Newtonsoft.Json�����л�����
        });

        // ...
    }
}
```