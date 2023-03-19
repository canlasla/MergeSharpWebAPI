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
        public readonly string category;
        [JsonInclude]
        public readonly int key;
        [JsonInclude]
        public readonly string loc;

        public VertexInfo(int key, int x, int y, Graph.Vertex.Category category)
        {
            this.key = key;
            this.loc = $"{x} {y}";
            this.category = category.ToString().ToLower();
        }
    }


    public readonly struct EdgeInfo
    {
        [JsonInclude]
        public readonly int from;
        [JsonInclude]
        public readonly string fromPort;
        [JsonInclude]
        public readonly int to;
        [JsonInclude]
        public readonly string toPort;
        [JsonInclude]
        public readonly int key;
        public EdgeInfo(int srcKey, string fromPort, int dstKey, string toPort, int key)
        {
            this.from = srcKey;
            this.fromPort = fromPort;
            this.to = dstKey;
            this.toPort = toPort;
            this.key = key;
        }
    }

    // Json object for the frontend
    public readonly struct GraphInfo
    {
        [JsonInclude]
        public readonly IEnumerable<VertexInfo> vertices;
        [JsonInclude]
        public readonly IEnumerable<EdgeInfo> edges;

        public GraphInfo(IEnumerable<VertexInfo> vertices, IEnumerable<EdgeInfo> edges)
        {
            this.vertices = vertices;
            this.edges = edges;
        }
    }

    public GraphInfo GetGraph()
    {
        GraphInfo graph = new(Vertices, Edges());
        return graph;
    }

    // TODO: Complete method that returns all the edges into an IEnumerable<EdgeInfo>:
    public IEnumerable<EdgeInfo> Edges()
    {
        List<EdgeInfo> edges = new();
        int idx = -1;
        foreach (KeyValuePair<(int, int), int> kv in EdgeCounts)
        {
            // for kv.value:
            for (int i = 0; i < kv.Value; i++)
            {
                int srcKey = kv.Key.Item1;
                int dstKey = kv.Key.Item2;
                string toPort = "in1";
                int key = idx;
                edges.Add(new EdgeInfo(srcKey, "out", dstKey, toPort, key));
                idx--;
            }
        }
        return edges;
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
            return new VertexInfo(key, vertex.x, vertex.y, vertex.category);
        }
    );
    public VertexInfo Vertex(int key)
    {
        if (_keyToVertexMap.TryGetValue(key, out Graph.Vertex v))
        {
            return new VertexInfo(key, v.x, v.y, v.category);
        }

        throw new KeyNotFoundException();
    }

    public bool AddVertex(int key, int x, int y, string scategory)
    {
        if (_keyToVertexMap.ContainsKey(key))
        {
            return false;
        }

        if (Enum.TryParse(scategory, true, out Graph.Vertex.Category category))
        {
            int retries = 3; // arbitrary amount of retries

            while (retries > 0)
            {
                Guid vertexGuid = Guid.NewGuid();
                var v = new Graph.Vertex(vertexGuid, x, y, category);

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

    public void ApplySynchronizedUpdate(byte[] encodedMsg)
    {
        GraphMsg decodedMsg = new();
        decodedMsg.Decode(encodedMsg);
        _graph.ApplySynchronizedUpdate(decodedMsg);

        _vertexGuidToKeyMap.Clear();
        _keyToVertexMap.Clear();

        int i = 0;
        foreach (Graph.Vertex vertex in _graph.Vertices)
        {
            _vertexGuidToKeyMap.Add(vertex.guid, i);
            _keyToVertexMap.Add(i, vertex);
            i++;
        }
    }

    // return the encoded message because the caller should not know anything about Graph
    // ie the params and return val should not have any graph stuff in it
    // that's why _graph is private
    public byte[] GetLastSynchronizedUpdate() => _graph.GetLastSynchronizedUpdate().Encode();
}
