using MergeSharp;

namespace MergeSharpWebAPI.Models;

public class LWWSet<T>
{
    public int Id { get; set; }
    public MergeSharp.LWWSet<T> LwwSet { get; set; }

}
