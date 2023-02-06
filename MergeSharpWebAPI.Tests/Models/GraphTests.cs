namespace MergeSharpWebAPI.Models.Tests;

public class GraphTests
{
    [Fact]
    public void EmptyInitialization()
    {
        Graph g = new();

        Assert.Empty(g.Vertices);
        Assert.Empty(g.Edges);
    }

    [Fact]
    public void SingleGraphAddRemove()
    {
        Graph g = new();
        Graph.Vertex v1 = new(Guid.NewGuid(), 1, 2.3, Graph.Vertex.Type.Nand);
        Graph.Vertex v2 = new(Guid.NewGuid(), -4.5, -7, Graph.Vertex.Type.XNor);

        g.AddVertex(v1);
        g.AddVertex(v2);

        Graph.Edge e1 = new(v1.guid, v2.guid);
        Graph.Edge e2 = new(v1.guid, v2.guid);
        Graph.Edge e3 = new(v1.guid, v1.guid);
        Graph.Edge e4 = new(v2.guid, v1.guid);

        g.AddEdge(e1);
        g.AddEdge(e2);
        g.AddEdge(e3);

        Assert.Equal(new List<Guid>() { v1.guid, v2.guid }, g.Vertices);

        Assert.Equal(2, g.Edges[e1]);
        Assert.Equal(2, g.Edges[e2]);
        Assert.Equal(1, g.Edges[e3]);
        Assert.False(g.Edges.ContainsKey(e4));
    }
}
