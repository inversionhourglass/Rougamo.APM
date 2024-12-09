# 5.0.0

- 降低`Rougamo.Fody`依赖版本为`1.0.1`，仅使用`MethodContext`，不需要依赖高版本。依赖低版本可以提供更好的兼容性
- 移出`Rougamo.APM.JsonSerialization`项目的多SDK版本支持，仅使用`netstandard2.0`。没有针对特定版本有特殊操作，使用`netstandard2.0`即可提供最佳兼容