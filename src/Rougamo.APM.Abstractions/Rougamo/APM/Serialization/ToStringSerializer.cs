namespace Rougamo.APM.Serialization
{
    /// <summary>
    /// call ToString to serialize object
    /// </summary>
    public class ToStringSerializer : ISerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Serialize(object obj)
        {
            return obj.ToString();
        }
    }
}
