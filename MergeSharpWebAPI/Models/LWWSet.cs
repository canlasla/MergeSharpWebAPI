using MergeSharp;

namespace MergeSharpWebAPI.Models;

public class LWWSetModel<T>
{
    public int Id { get; set; }
    public LWWSet<T> LwwSet { get; set; }

}
