using System.Text.Json.Serialization;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Services;

// The service should act as the translator between frontend key and backend Guid
public class GraphService
{
    private readonly Graph _graph;
    private readonly Dictionary<Guid, int> _vertexGuidToKeyMap;
    private readonly Dictionary<int, Graph.Vertex> _keyToVertexMap;

    public GraphService()
    {
        _graph = new();
        _vertexGuidToKeyMap = new();
        _keyToVertexMap = new();
    }

    // Json object for the frontend
    public readonly struct VertexInfo
    {
        [JsonInclude]
        public readonly int key;
        [JsonInclude]
        public readonly double x;
        [JsonInclude]
        public readonly double y;
        [JsonInclude]
        public readonly string type;

        public VertexInfo(int key, double x, double y, Graph.Vertex.Type type)
        {
            this.key = key;
            this.x = x;
            this.y = y;
            this.type = type.ToString().ToLower();
        }
    }

    public Dictionary<(int, int), int> EdgeCounts => _graph.Edges.ToDictionary(
                                                                kv => (_vertexGuidToKeyMap[kv.Key.src], _vertexGuidToKeyMap[kv.Key.dst]),
                                                                kv => kv.Value
                                                            );

    public int EdgeCount(int srcKey, int dstKey)
    {
        if (_keyToVertexMap.TryGetValue(srcKey, out Graph.Vertex src)
                && _keyToVertexMap.TryGetValue(dstKey, out Graph.Vertex dst))
        {
            return _graph.EdgeCount(new Graph.Edge(src.guid, dst.guid));
        }

        throw new KeyNotFoundException();
    }

    public IEnumerable<VertexInfo> Vertices => _keyToVertexMap.Select(
        (kv) =>
        {
            int key = kv.Key;
            Graph.Vertex vertex = kv.Value;
            return new VertexInfo(key, vertex.x, vertex.y, vertex.type);
        }
    );
    public VertexInfo Vertex(int key)
    {
        if (_keyToVertexMap.TryGetValue(key, out Graph.Vertex v))
        {
            return new VertexInfo(key, v.x, v.y, v.type);
        }

        throw new KeyNotFoundException();
    }

    public bool AddVertex(int key, double x, double y, string stype)
    {
        if (_keyToVertexMap.ContainsKey(key))
        {
            return false;
        }

        if (Enum.TryParse(stype, true, out Graph.Vertex.Type type))
        {
            int retries = 3; // arbitrary amount of retries

            while (retries > 0)
            {
                Guid vertexGuid = Guid.NewGuid();
                var v = new Graph.Vertex(vertexGuid, x, y, type);

                if (_graph.AddVertex(v)) // fails if vertexGuid is non-unique (very small probability)
                {
                    _vertexGuidToKeyMap.Add(v.guid, key);
                    _keyToVertexMap.Add(key, v);
                    return true;
                }

                retries--;
            }
        }

        return false;
    }

    public bool RemoveVertex(int key)
    {
        if (_keyToVertexMap.TryGetValue(key, out Graph.Vertex v)
                && _graph.RemoveVertex(v))
        {
            _ = _vertexGuidToKeyMap.Remove(v.guid);
            _ = _keyToVertexMap.Remove(key);
            return true;
        }

        return false;
    }

    public bool AddEdge(int srcKey, int dstKey)
    {
        if (_keyToVertexMap.TryGetValue(srcKey, out Graph.Vertex v1)
            && _keyToVertexMap.TryGetValue(dstKey, out Graph.Vertex v2))
        {
            return _graph.AddEdge(new Graph.Edge(v1.guid, v2.guid));
        }

        return false;
    }

    public bool RemoveEdge(int srcKey, int dstKey)
    {
        if (_keyToVertexMap.TryGetValue(srcKey, out Graph.Vertex v1)
            && _keyToVertexMap.TryGetValue(dstKey, out Graph.Vertex v2))
        {
            return _graph.RemoveEdge(new Graph.Edge(v1.guid, v2.guid));
        }

        return false;
    }

    // TODO: where is this function called / who calls this function? Frontend or backend?
    // - if frontend then how would they have access to GraphMsg?
    // public void ApplySynchronizedUpdate(GraphMsg msg) => _graph.ApplySynchronizedUpdate(msg);

    // Change the above to the following:
    public void ApplySynchronizedUpdate(byte[] encodedMsg)
    {
        GraphMsg decodedMsg = new();
        decodedMsg.Decode(encodedMsg);
        // TODO: This function should return true or false if the merge was successful
        // encodedMsg may be of wrong type or the message may have been messed up in transit
        // TODO: or catch the NotSupportedException
        _graph.ApplySynchronizedUpdate(decodedMsg);

        // TODO: update _keyToVertexMap and _vertexGuidToKeyMap
    }

    // return the encoded message because the caller should not know anything about Graph
    // ie the params and return val should not have any graph stuff in it
    // that's why _graph is private
    public byte[] GetLastSynchronizedUpdate() => _graph.GetLastSynchronizedUpdate().Encode();
}
