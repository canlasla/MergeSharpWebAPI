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
    public override void Decode(byte[] input) => throw new NotImplementedException();
    public override byte[] Encode() => throw new NotImplementedException();
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
        if (this.LookupVertices().Contains(v.id)) {
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

    public override PropagationMessage GetLastSynchronizedUpdate() => throw new NotImplementedException();
    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate) => throw new NotImplementedException();
    public override PropagationMessage DecodePropagationMessage(byte[] input) => throw new NotImplementedException();
}
