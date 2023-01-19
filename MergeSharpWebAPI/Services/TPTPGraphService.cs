using System.Text;
using MergeSharp;
using MergeSharpWebAPI.Models;
using Newtonsoft.Json;

namespace MergeSharpWebAPI.Services;

public class TPTPGraphService<T>
{
    private List<MergeSharpWebAPI.Models.TPTPGraph> TPTPGraphs { get; set; }

    public TPTPGraphService()
    {
        TPTPGraphs = new List<MergeSharpWebAPI.Models.TPTPGraph>
        {
            new MergeSharpWebAPI.Models.TPTPGraph { Id = 1, myTptpGraph=new MergeSharp.TPTPGraph() },
        };
    }

    //get all graphs
    public List<MergeSharpWebAPI.Models.TPTPGraph> GetAll() => TPTPGraphs;

    //get a graph
    public MergeSharpWebAPI.Models.TPTPGraph? Get(int id) => TPTPGraphs.FirstOrDefault(p => p.Id == id);

    // add a graph
    public void Add(MergeSharpWebAPI.Models.TPTPGraph tptpGraph)
    {
        TPTPGraphs.Add(tptpGraph);
    }

    // remove a graph
    public void Delete(int id)
    {
        MergeSharpWebAPI.Models.TPTPGraph TPTPGraph = Get(id);
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

        TPTPGraphs[index].myTptpGraph.AddVertex(element);
    }

    // remove a vertex
    public bool RemoveVertex(int Id, Guid element)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].myTptpGraph.RemoveVertex(element);
    }

    // add an edge
    public bool AddEdge(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].myTptpGraph.AddEdge(v1, v2);
    }

    // remove an edge
    public bool RemoveEdge(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].myTptpGraph.RemoveEdge(v1, v2);
    }

    // lookup edges
    public IEnumerable<(Guid, Guid)>? LookupEdges(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return null;

        return TPTPGraphs[index].myTptpGraph.LookupEdges();
    }

    // lookup vertices
    public IEnumerable<Guid>? LookupVertices(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return null;

        return TPTPGraphs[index].myTptpGraph.LookupVertices();
    }

    public bool TPTPGraphContains(int Id, Guid v)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].myTptpGraph.Contains(v);
    }

    public bool TPTPGraphContains(int Id, Guid v1, Guid v2)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return TPTPGraphs[index].myTptpGraph.Contains(v1, v2);
    }

    public void MergeTPTPGraphs(int Id1, int Id2)
    {
        var index1 = TPTPGraphs.FindIndex(p => p.Id == Id1);
        var index2 = TPTPGraphs.FindIndex(p => p.Id == Id2);

        TPTPGraphs[index1].myTptpGraph.ApplySynchronizedUpdate(TPTPGraphs[index2].myTptpGraph.GetLastSynchronizedUpdate());
    }

    public void MergeTPTPGraphs(int Id1, MergeSharp.TPTPGraphMsg tptpgraphmsg)
    {
        var index1 = TPTPGraphs.FindIndex(p => p.Id == Id1);

        TPTPGraphs[index1].myTptpGraph.ApplySynchronizedUpdate(tptpgraphmsg);
    }

    public PropagationMessage GetLastSynchronizedUpdate(int Id)
    {
        var index = TPTPGraphs.FindIndex(p => p.Id == Id);
        return TPTPGraphs[index].myTptpGraph.GetLastSynchronizedUpdate();
    }
}
