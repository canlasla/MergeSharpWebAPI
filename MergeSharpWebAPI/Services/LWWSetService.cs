using MergeSharpWebAPI.Models;
using Newtonsoft.Json;

namespace MergeSharpWebAPI.Services;

public class LWWSetService<T>
{
    private static List<LWWSet<T>> LWWSets { get; }
    private static int nextId = 3;
    static LWWSetService()
    {
        LWWSets = new List<LWWSet<T>>
        {
            new MergeSharpWebAPI.Models.LWWSet<T> { Id = 1, LwwSet=new MergeSharp.LWWSet<T>() },
        };
    }

    public static List<LWWSet<T>> GetAll() => LWWSets;

    public static LWWSet<T>? Get(int id) => LWWSets.FirstOrDefault(p => p.Id == id);

    public static void Add(LWWSet<T> lwwSet)
    {
        lwwSet.Id = nextId++;
        LWWSets.Add(lwwSet);
    }

    public static void Delete(int id)
    {
        LWWSet<T> lwwSet = Get(id);
        if (lwwSet is null)
            return;

        LWWSets.Remove(lwwSet);
    }

    public static void Update(LWWSet<T> lwwSet)
    {
        var index = LWWSets.FindIndex(p => p.Id == lwwSet.Id);
        if (index == -1)
            return;

        LWWSets[index] = lwwSet;
    }

    public static void AddElement(int Id, T element)
    {
        var index = LWWSets.FindIndex(p => p.Id == Id);
        if (index == -1)
            return;

        LWWSets[index].LwwSet.Add(element);
    }
}
