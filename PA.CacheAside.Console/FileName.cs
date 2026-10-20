//using Microsoft.Extensions.Caching.Hybrid;
//using System.Buffers;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Text.Json.Serialization.Metadata;

//namespace PA.CacheAside.Console;



//public class AotHybridCacheSerializerFactory : IHybridCacheSerializerFactory
//{
//    private readonly JsonSerializerContext _context;

//    public AotHybridCacheSerializerFactory(JsonSerializerContext context)
//    {
//        _context = context;
//    }

//    public bool TryCreateSerializer<T>(out IHybridCacheSerializer<T>? serializer)
//    {
//        var typeInfo = _context.GetTypeInfo(typeof(T));

//        if (typeInfo is JsonTypeInfo<T> typedInfo)
//        {
//            serializer = new AotHybridCacheSerializer<T>(typedInfo);
//            return true;
//        }

//        serializer = null;
//        return false;
//    }

//    private class AotHybridCacheSerializer<T> : IHybridCacheSerializer<T>
//    {
//        private readonly JsonTypeInfo<T> _jsonTypeInfo;

//        public AotHybridCacheSerializer(JsonTypeInfo<T> jsonTypeInfo)
//        {
//            _jsonTypeInfo = jsonTypeInfo;
//        }

//        public T Deserialize(ReadOnlySequence<byte> source)
//        {
//            var reader = new Utf8JsonReader(source);
//            return JsonSerializer.Deserialize<T>(ref reader, _jsonTypeInfo)!;
//        }

//        public void Serialize(T value, IBufferWriter<byte> target)
//        {
//            using var writer = new Utf8JsonWriter(target);
//            JsonSerializer.Serialize(writer, value, _jsonTypeInfo);
//        }
//    }
//}
