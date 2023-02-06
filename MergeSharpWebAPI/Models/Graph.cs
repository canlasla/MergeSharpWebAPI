using MergeSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharpWebAPI.Models;

[TypeAntiEntropyProtocol(typeof(Graph))]
public class GraphMsg : PropagationMessage
{
    [JsonInclude]
    public CanilleraGraphMsg cGraphMsg { get; private set; }

    [JsonInclude]
    public Dictionary<Guid, VertexInfoMsg> vertexInfoMsgs { get; private set; }

    public GraphMsg()
    {
        cGraphMsg = new();
        vertexInfoMsgs = new();
    }

    public GraphMsg(CanilleraGraph cGraph, Dictionary<Guid, VertexInfo> vertexInfo)
    {
        cGraphMsg = (CanilleraGraphMsg) cGraph.GetLastSynchronizedUpdate();
        vertexInfoMsgs = vertexInfo.ToDictionary(kv => kv.Key, kv => (VertexInfoMsg) kv.Value.GetLastSynchronizedUpdate());
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<GraphMsg>(input);
        if (json is null)
        {
            return;
        }

        this.cGraphMsg = json.cGraphMsg;
        this.vertexInfoMsgs = json.vertexInfoMsgs;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

public class Graph : CRDT
{

    // TODO: rethink how to organize the Graph class
    // should the Edge and Vertex be in the Graph?
    // Why is the VertexInfo not in the Graph? Why is it not in the Vertex?
    public struct Edge
    {
        public Guid src { get; set; }
        public Guid dst { get; set; }
        public Edge(Guid src, Guid dst)
        {
            this.src = src;
            this.dst = dst;
        }
    }

    public struct Vertex
    {
        public enum Type
        {
            Invalid, And, Or, Xor, Not, Nand, Nor, XNor
        }

        public readonly Guid id { get; }
        public double x { get; set; }
        public double y { get; set; }
        public readonly Type type { get; }

        public Vertex(Guid id, double x, double y, Type type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }

    private readonly CanilleraGraph _canilleraGraph;
    private Dictionary<Guid, VertexInfo> _vertexInfo;

    public Graph()
    {
        _canilleraGraph = new();
        _vertexInfo = new();
    }


    [OperationType(OpType.Update)]
    public virtual bool AddVertex(Vertex v)
    {
        if (LookupVertices().Contains(v.id))
        {
            return false;
        }

        _canilleraGraph.AddVertex(v.id);
        _vertexInfo[v.id] = new VertexInfo(v.x, v.y, v.type);
        return true;
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveVertex(Vertex v)
    {
        if (_canilleraGraph.RemoveVertex(v.id))
        {
            _ = _vertexInfo.Remove(v.id);
            return true;
        }

        return false;
    }

    [OperationType(OpType.Update)]
    public virtual bool AddEdge(Edge e)
    {
        return _canilleraGraph.AddEdge(new CanilleraGraph.Edge(e.src, e.dst));
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveEdge(Edge e)
    {
        return _canilleraGraph.RemoveEdge(new CanilleraGraph.Edge(e.src, e.dst));
    }

    public IEnumerable<Guid> LookupVertices()
    {
        return _canilleraGraph.LookupVertices();
    }

    public Dictionary<Edge, int> EdgeCounts()
    {
        return _canilleraGraph.EdgeCounts().Where(kv => kv.Value > 0).ToDictionary(kv => new Edge(kv.Key.src, kv.Key.dst), kv => kv.Value);
    }

    public int EdgeCount(Edge edge)
    {
        return _canilleraGraph.EdgeCount(new CanilleraGraph.Edge(edge.src, edge.dst));
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not GraphMsg)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support {nameof(receivedUpdate)} type of {receivedUpdate.GetType()}");
        }

        GraphMsg received = (GraphMsg) receivedUpdate;
        _canilleraGraph.ApplySynchronizedUpdate(received.cGraphMsg);

        // _vertexInfo should contain only the vertices now in the graph
        var currVertices = LookupVertices();
        _vertexInfo = _vertexInfo.Where(kv => currVertices.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

        foreach (var kv in received.vertexInfoMsgs)
        {
            if (_vertexInfo.TryGetValue(kv.Key, out VertexInfo? vInfo))
            {
                vInfo.ApplySynchronizedUpdate(kv.Value);
            }
            else
            {
                vInfo = new();
                vInfo.ApplySynchronizedUpdate(kv.Value);
            }
        }
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        GraphMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate() => new GraphMsg(_canilleraGraph, _vertexInfo);
}
