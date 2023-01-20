using MergeSharp;

namespace MergeSharpWebAPI.Models;

public class LWWSetModel<T>
{
    public int Id { get; set; }
    public MergeSharp.LWWSet<T> LwwSet { get; set; }

}
