using MergeSharp;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Services;

public class TPTPGraphService
{
    private List<TPTPGraphModel> TPTPGraphs { get; set; }

    public TPTPGraphService()
    {
        TPTPGraphs = new List<TPTPGraphModel>{
            new TPTPGraphModel { Id = 1, TptpGraph = new TPTPGraph() },
        };
    }

    //get all graphs
    public List<TPTPGraphModel> GetAll() => TPTPGraphs;

    //get a graph
    public TPTPGraphModel? Get(int id) => TPTPGraphs.FirstOrDefault(p => p.Id == id);

    // add a graph
    public void Add(TPTPGraphModel tptpGraph)
    {
        TPTPGraphs.Add(tptpGraph);
    }

    // remove a graph
    public void Delete(int id)
    {
        TPTPGraphModel TPTPGraph = Get(id);
        if (TPTPGraph is null)
            return;

        TPTPGraphs.Remove(TPTPGraph);
    }

    // add a vertex
    public void AddVertex(int Id, Guid element)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return;

        TPTPGraphs[index].TptpGraph.AddVertex(element);
    }

    // remove a vertex
    public bool RemoveVertex(int Id, Guid element)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].TptpGraph.RemoveVertex(element);
    }

    // add an edge
    public bool AddEdge(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].TptpGraph.AddEdge(v1, v2);
    }

    // remove an edge
    public bool RemoveEdge(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].TptpGraph.RemoveEdge(v1, v2);
    }

    // lookup edges
    public IEnumerable<(Guid, Guid)>? LookupEdges(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return null;

        return TPTPGraphs[index].TptpGraph.Edges;
    }

    // lookup vertices
    public IEnumerable<Guid>? LookupVertices(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return null;

        return TPTPGraphs[index].TptpGraph.Vertices;
    }

    // check if graph contains vertex
    public bool TPTPGraphContains(int Id, Guid v)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].TptpGraph.Contains(v);
    }

    // check if graph contains edge
    public bool TPTPGraphContains(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].TptpGraph.Contains(v1, v2);
    }

    // merge graph id2 into graph id1
    public void MergeTPTPGraphs(int Id1, int Id2)
    {
        var index1 = TPTPGraphs.FindIndex(p => p.Id == Id1);
        var index2 = TPTPGraphs.FindIndex(p => p.Id == Id2);

        TPTPGraphs[index1].TptpGraph.ApplySynchronizedUpdate(TPTPGraphs[index2].TptpGraph.GetLastSynchronizedUpdate());
    }

    // merge graph msg into graph id1
    public void MergeTPTPGraphs(int Id1, MergeSharp.TPTPGraphMsg tptpgraphmsg)
    {
        var index1 = TPTPGraphs.FindIndex(p => p.Id == Id1);

        TPTPGraphs[index1].TptpGraph.ApplySynchronizedUpdate(tptpgraphmsg);
    }

    public PropagationMessage GetLastSynchronizedUpdate(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        return TPTPGraphs[index].TptpGraph.GetLastSynchronizedUpdate();
    }
}
