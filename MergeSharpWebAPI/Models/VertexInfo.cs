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
    public Graph.Vertex.Type type { get; private set; }
    public VertexInfoMsg()
    {
        xMsg = new();
        yMsg = new();
        type = Graph.Vertex.Type.Invalid;
    }

    public VertexInfoMsg(LWWRegister<int> x, LWWRegister<int> y, Graph.Vertex.Type type)
    {
        xMsg = (LWWRegisterMsg<int>) x.GetLastSynchronizedUpdate();
        yMsg = (LWWRegisterMsg<int>) y.GetLastSynchronizedUpdate();
        this.type = type;
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
        this.type = json.type;
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
    private Graph.Vertex.Type _type;

    public VertexInfo() {
        _x = new();
        _y = new();
        _type = Graph.Vertex.Type.Invalid;
     }

    public VertexInfo(int x, int y, Graph.Vertex.Type type)
    {
        _x = new(x);
        _y = new(y);
        _type = type;
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

        // this should only occur if this._type == Graph.Vertex.Type.Invalid
        // NOTE: Graph.Vertex.Type.Invalid is the smallest enum
        _type = _type > received.type ? _type : received.type;
    }
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        VertexInfoMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate() => new VertexInfoMsg(this._x, this._y, this._type);
}
