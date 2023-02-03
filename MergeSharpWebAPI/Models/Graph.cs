using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        this.cGraphMsg = new();
        this.vertexInfoMsgs = new();
    }

    public GraphMsg(CanilleraGraph cGraph, Dictionary<Guid, VertexInfo> vertexInfo)
    {
        this.cGraphMsg = (CanilleraGraphMsg) cGraph.GetLastSynchronizedUpdate();
        this.vertexInfoMsgs = vertexInfo.ToDictionary(kv => kv.Key, kv => (VertexInfoMsg) kv.Value.GetLastSynchronizedUpdate());
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
    public struct Edge
    {
        public Guid src { get; set; }
        public Guid dst { get; set; }
        public Edge(Guid v1, Guid v2)
        {
            this.src = v1;
            this.dst = v2;
        }
    }

    public struct Vertex
    {
        public readonly Guid id { get; }
        public double x { get; set; }
        public double y { get; set; }
        public readonly string type { get; }

        public Vertex(Guid id, double x, double y, string type)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }

    private readonly CanilleraGraph _canilleraGraph;
    private readonly Dictionary<Guid, VertexInfo> _vertexInfo;

    public Graph()
    {
        this._canilleraGraph = new();
        this._vertexInfo = new();
    }


    [OperationType(OpType.Update)]
    public virtual bool AddVertex(Vertex v)
    {
        if (this.LookupVertices().Contains(v.id))
        {
            return false;
        }

        this._canilleraGraph.AddVertex(v.id);
        this._vertexInfo[v.id] = new VertexInfo(v.x, v.y, v.type);
        return true;
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveVertex(Vertex v)
    {
        if (this._canilleraGraph.RemoveVertex(v.id))
        {
            _ = this._vertexInfo.Remove(v.id);
            return true;
        }

        return false;
    }

    [OperationType(OpType.Update)]
    public virtual bool AddEdge(Edge e)
    {
        return this._canilleraGraph.AddEdge(new CanilleraGraph.Edge(e.src, e.dst));
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveEdge(Edge e)
    {
        return this._canilleraGraph.RemoveEdge(new CanilleraGraph.Edge(e.src, e.dst));
    }

    public IEnumerable<Guid> LookupVertices()
    {
        return this._canilleraGraph.LookupVertices();
    }

    public Dictionary<Edge, int> EdgeCounts()
    {
        return this._canilleraGraph.EdgeCounts().Where(kv => kv.Value > 0).ToDictionary(kv => new Edge(kv.Key.src, kv.Key.dst), kv => kv.Value);
    }

    public int EdgeCount(Edge edge)
    {
        return this._canilleraGraph.EdgeCount(new CanilleraGraph.Edge(edge.src, edge.dst));
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not GraphMsg)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support {nameof(receivedUpdate)} type of {receivedUpdate.GetType()}");
        }

        GraphMsg received = (GraphMsg) receivedUpdate;
        this._canilleraGraph.ApplySynchronizedUpdate(received.cGraphMsg);
        foreach (var kv in received.vertexInfoMsgs)
        {
            if (this._vertexInfo.TryGetValue(kv.Key, out VertexInfo? vInfo))
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

    public override PropagationMessage GetLastSynchronizedUpdate() => new GraphMsg(this._canilleraGraph, this._vertexInfo);
}
