using System.Text;
using MergeSharp;
using MergeSharpWebAPI.Models;
using Newtonsoft.Json;

namespace MergeSharpWebAPI.Services;

public class LWWSetService<T>
{
    private List<MergeSharpWebAPI.Models.LWWSetModel<T>> LWWSets { get; set; }

    public LWWSetService()
    {
        LWWSets = new List<MergeSharpWebAPI.Models.LWWSetModel<T>>
        {
            new MergeSharpWebAPI.Models.LWWSetModel<T> { Id = 1, LwwSet=new MergeSharp.LWWSet<T>() },
        };
    }

    public List<MergeSharpWebAPI.Models.LWWSetModel<T>> GetAll() => LWWSets;

    public MergeSharpWebAPI.Models.LWWSetModel<T>? Get(int id) => LWWSets.FirstOrDefault(p => p.Id == id);

    public void Add(MergeSharpWebAPI.Models.LWWSetModel<T> lwwSet)
    {
        LWWSets.Add(lwwSet);
    }

    public void Delete(int id)
    {
        MergeSharpWebAPI.Models.LWWSetModel<T> lwwSet = Get(id);
        if (lwwSet is null)
            return;

        LWWSets.Remove(lwwSet);
    }

    public void Update(MergeSharpWebAPI.Models.LWWSetModel<T> lwwSet)
    {
        var index = LWWSets.FindIndex(p => p.Id == lwwSet.Id);
        if (index == -1)
            return;

        LWWSets[index] = lwwSet;
    }

    public void AddElement(int Id, T element)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);
        if (index == -1)
            return;

        LWWSets[index].LwwSet.Add(element);
    }

    public bool RemoveElement(int Id, T element)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);
        if (index == -1)
            return false;

        return LWWSets[index].LwwSet.Remove(element);
    }

    public void ClearLWWSet(int Id)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);
        if (index == -1)
            return;

        LWWSets[index].LwwSet.Clear();
    }

    public int CountLWWSet(int Id)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);

        return LWWSets[index].LwwSet.Count();
    }

    public bool LWWSetContains(int Id, T element)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);

        return LWWSets[index].LwwSet.Contains(element);
    }

    public void MergeLWWSets(int Id1, int Id2)
    {
        var index1 = LWWSets.FindIndex(p => p.Id == Id1);
        var index2 = LWWSets.FindIndex(p => p.Id == Id2);

        LWWSets[index1].LwwSet.ApplySynchronizedUpdate(LWWSets[index2].LwwSet.GetLastSynchronizedUpdate());
    }

    public void MergeLWWSets(int Id1, MergeSharp.LWWSetMsg<T> lwwsetmsg)
    {
        var index1 = LWWSets.FindIndex(p => p.Id == Id1);

        LWWSets[index1].LwwSet.ApplySynchronizedUpdate(lwwsetmsg);
    }

    public PropagationMessage GetLastSynchronizedUpdate(int Id)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);
        return LWWSets[index].LwwSet.GetLastSynchronizedUpdate();
    }
}
