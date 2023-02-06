using System.Text.Json.Serialization;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Services;

// The service should act as the translator between frontend id and backend Guid
public class GraphService
{
    private readonly Graph _graph;
    private readonly Dictionary<Guid, int> _vertexToIdMap;
    private readonly Dictionary<int, Graph.Vertex> _idToVertexMap;

    public GraphService()
    {
        _graph = new();
        _vertexToIdMap = new();
        _idToVertexMap = new();
    }

    // Json object for the frontend
    public readonly struct VertexInfo
    {
        [JsonInclude]
        public readonly double x;
        [JsonInclude]
        public readonly double y;
        [JsonInclude]
        public readonly Graph.Vertex.Type type;

        public VertexInfo(double x, double y, Graph.Vertex.Type type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }

    private static Graph.Vertex.Type StringToGraphVertexType(string str)
    {
        return str switch
        {
            "and" => Graph.Vertex.Type.And,
            "or" => Graph.Vertex.Type.Or,
            "xor" => Graph.Vertex.Type.Xor,
            "not" => Graph.Vertex.Type.Not,
            "nand" => Graph.Vertex.Type.Nand,
            "nor" => Graph.Vertex.Type.Nor,
            "xnor" => Graph.Vertex.Type.XNor,
            _ => Graph.Vertex.Type.Invalid,
        };
    }

    public Dictionary<(int, int), int> EdgeCounts => _graph.EdgeCounts().ToDictionary(
                                                                kv => (_vertexToIdMap[kv.Key.src], _vertexToIdMap[kv.Key.dst]),
                                                                kv => kv.Value
                                                            );

    public int EdgeCount(int srcId, int dstId)
    {
        if (_idToVertexMap.TryGetValue(srcId, out Graph.Vertex src)
                && _idToVertexMap.TryGetValue(dstId, out Graph.Vertex dst))
        {
            return _graph.EdgeCount(new Graph.Edge(src.id, dst.id));
        }

        return 0;
    }

    public IEnumerable<int> Vertices => _graph.LookupVertices().Select(guid => _vertexToIdMap[guid]);
    public VertexInfo Vertex(int id)
    {
        if (_idToVertexMap.TryGetValue(id, out Graph.Vertex v))
        {
            return new VertexInfo(v.x, v.y, v.type);
        }

        // TODO: throw an exception
        return new VertexInfo();
    }

    public bool AddVertex(int id, double x, double y, string type)
    {
        if (_idToVertexMap.ContainsKey(id))
        {
            return false;
        }

        Guid vertexGuid = Guid.NewGuid();
        var v = new Graph.Vertex(vertexGuid, x, y, StringToGraphVertexType(type.ToLower()));

        _vertexToIdMap.Add(v.id, id);
        _idToVertexMap.Add(id, v);

        return _graph.AddVertex(v);
    }

    public bool RemoveVertex(int id)
    {
        if (_idToVertexMap.TryGetValue(id, out Graph.Vertex v)
                && _graph.RemoveVertex(v))
        {
            _ = _vertexToIdMap.Remove(v.id);
            _ = _idToVertexMap.Remove(id);
            return true;
        }

        return false;
    }

    public bool AddEdge(int src, int dst)
    {
        if (_idToVertexMap.TryGetValue(src, out Graph.Vertex v1)
            && _idToVertexMap.TryGetValue(dst, out Graph.Vertex v2))
        {
            return _graph.AddEdge(new Graph.Edge(v1.id, v2.id));
        }

        return false;
    }

    public bool RemoveEdge(int src, int dst)
    {
        if (_idToVertexMap.TryGetValue(src, out Graph.Vertex v1)
            && _idToVertexMap.TryGetValue(dst, out Graph.Vertex v2))
        {
            return _graph.RemoveEdge(new Graph.Edge(v1.id, v2.id));
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
    }

    // return the encoded message because the caller should not know anything about Graph
    // ie the params and return val should not have any graph stuff in it
    // that's why _graph is private
    public byte[] GetLastSynchronizedUpdate() => _graph.GetLastSynchronizedUpdate().Encode();
}
