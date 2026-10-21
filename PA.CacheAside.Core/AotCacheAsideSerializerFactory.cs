using Microsoft.Extensions.Caching.Hybrid;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PA.CacheAside.Core;

public class AotCacheAsideSerializerFactory(AotCacheAsideJsonContext context) : IHybridCacheSerializerFactory
{
    public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)
    {
        var typeInfo = context.GetTypeInfo(typeof(T));

        if (typeInfo is JsonTypeInfo<T> typedInfo)
        {
            serializer = new AotCacheAsideSerializer<T>(typedInfo);
            return true;
        }

        serializer = null;
        return false;
    }

    private class AotCacheAsideSerializer<T>(JsonTypeInfo<T> jsonTypeInfo) : IHybridCacheSerializer<T>
    {
        public T Deserialize(ReadOnlySequence<byte> source)
        {
            var reader = new Utf8JsonReader(source);
            return JsonSerializer.Deserialize<T>(ref reader, jsonTypeInfo)!;
        }

        public void Serialize(T value, IBufferWriter<byte> target)
        {
            using var writer = new Utf8JsonWriter(target);
            JsonSerializer.Serialize(writer, value, jsonTypeInfo);
        }
    }
}
