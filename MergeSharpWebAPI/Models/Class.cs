namespace MergeSharpWebAPI.Models;

public class Node
{
    public Node(string category, int key, string loc)
    {
        this.category = category;
        this.key = key;
        this.loc = loc;
    }
    public string category { get; set; }
    public int key { get; set; }
    public string loc { get; set; }
}
