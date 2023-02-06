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
    public struct VertexInfo
    {
        [JsonInclude]
        public readonly int x;
        [JsonInclude]
        public readonly int y;
        [JsonInclude]
        public readonly Graph.Vertex.Type type;

        public VertexInfo(int x, int y, Graph.Vertex.Type type)
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

    public Dictionary<(int, int), int> EdgeCounts => _graph.EdgeCounts().ToDictionary(kv => (_vertexToIdMap[kv.Key.src], _vertexToIdMap[kv.Key.dst]), kv => kv.Value);

    public IEnumerable<int> Vertices() => _graph.LookupVertices().Select(guid => _vertexToIdMap[guid]);
    public VertexInfo Vertices(int id)
    {
        Graph.Vertex v = _idToVertexMap[id];
        return new VertexInfo((int) v.x, (int) v.y, v.type);
    }

    public bool AddVertex(int id, double x, double y, string type)
    {
        Guid vertexGuid = Guid.NewGuid();
        var v = new Graph.Vertex(vertexGuid, x, y, StringToGraphVertexType(type.ToLower()));
        _vertexToIdMap.Add(v.id, id);
        _idToVertexMap.Add(id, v);

        return _graph.AddVertex(v);
    }

    public bool RemoveVertex(int id)
    {
        Graph.Vertex v = _idToVertexMap[id];

        if (_graph.RemoveVertex(v))
        {
            _vertexToIdMap.Remove(v.id);
            _idToVertexMap.Remove(id);
            return true;
        }

        return false;
    }

    public bool AddEdge(int id1, int id2)
    {
        if (_idToVertexMap.TryGetValue(id1, out Graph.Vertex v1)
            && _idToVertexMap.TryGetValue(id2, out Graph.Vertex v2))
        {
            return _graph.AddEdge(new Graph.Edge(v1.id, v2.id));
        }

        return false;
    }

    public bool RemoveEdge(int id1, int id2)
    {
        if (_idToVertexMap.TryGetValue(id1, out Graph.Vertex v1)
            && _idToVertexMap.TryGetValue(id2, out Graph.Vertex v2))
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
