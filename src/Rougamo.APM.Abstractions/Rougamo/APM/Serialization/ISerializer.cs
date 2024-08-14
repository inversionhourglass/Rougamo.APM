namespace Rougamo.APM.Serialization
{
    /// <summary>
    /// parameter and return value serializer
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// serialize object to string
        /// </summary>
        string Serialize(object? obj);
    }
}
