using MergeSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharpWebAPI.Models;

public class VertexInfoMsg : PropagationMessage
{
    [JsonInclude]
    public LWWRegisterMsg<int> xMsg { get; private set; }

    [JsonInclude]
    public LWWRegisterMsg<int> yMsg { get; private set; }

    [JsonInclude]
    public Graph.Vertex.Category category { get; private set; }
    public VertexInfoMsg()
    {
        xMsg = new();
        yMsg = new();
        category = Graph.Vertex.Category.Invalid;
    }

    public VertexInfoMsg(LWWRegister<int> x, LWWRegister<int> y, Graph.Vertex.Category category)
    {
        xMsg = (LWWRegisterMsg<int>) x.GetLastSynchronizedUpdate();
        yMsg = (LWWRegisterMsg<int>) y.GetLastSynchronizedUpdate();
        this.category = category;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<VertexInfoMsg>(input);
        if (json is null)
        {
            return;
        }

        this.xMsg = json.xMsg;
        this.yMsg = json.yMsg;
        this.category = json.category;
    }
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

public class VertexInfo : CRDT
{
    private readonly LWWRegister<int> _x;
    private readonly LWWRegister<int> _y;

    public int X
    {
        get => _x.Value;
        set => _x.Value = value;
    }

    public int Y
    {
        get => _y.Value;
        set => _y.Value = value;
    }

    public Graph.Vertex.Category Category { get; private set; }

    public VertexInfo()
    {
        _x = new();
        _y = new();
        Category = Graph.Vertex.Category.Invalid;
    }

    public VertexInfo(int x, int y, Graph.Vertex.Category category)
    {
        _x = new(x);
        _y = new(y);
        Category = category;
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not VertexInfoMsg)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support {nameof(receivedUpdate)} type of {receivedUpdate.GetType()}");
        }

        VertexInfoMsg received = (VertexInfoMsg) receivedUpdate;
        _x.ApplySynchronizedUpdate(received.xMsg);
        _y.ApplySynchronizedUpdate(received.yMsg);

        // this should only occur if this.Category == Graph.Vertex.Category.Invalid
        // NOTE: Graph.Vertex.Category.Invalid is the smallest enum
        Category = Category > received.category ? Category : received.category;
    }
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        VertexInfoMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate() => new VertexInfoMsg(this._x, this._y, this.Category);
}
