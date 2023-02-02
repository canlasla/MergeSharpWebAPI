namespace MergeSharpWebAPI.Models;

public class Node
{
    public Node(String category, int key, String loc)
    {
        this.category = category;
        this.key = key;
        this.loc = loc;
    }
    public String category { get; set; }
    public int key { get; set; }
    public String loc { get; set; }
}
