using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Services;

public class LWWSetService<T>
{
    static List<LWWSet<T>> LWWSets { get; }
    static int nextId = 3;
    static LWWSetService()
    {
        LWWSets = new List<LWWSet<T>>
        {
            new LWWSet<T> { Id = 1 },
            new LWWSet<T> { Id = 2 }
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
        var lwwSet = Get(id);
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
}
