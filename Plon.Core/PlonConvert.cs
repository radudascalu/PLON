namespace Plon.Core
{
    public static class PlonConvert
    {
        public static string Serialize<T>(T value)
        {
            return Serialization.Serializer.serialize(value);
        }

        public static T Deserialize<T>(string serialized)
        {
            return Serialization.Deserializer.deserialize<T>(serialized);
        }
    }
}
