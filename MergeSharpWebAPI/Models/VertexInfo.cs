using MergeSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharpWebAPI.Models;

public class VertexInfoMsg : PropagationMessage
{
    [JsonInclude]
    public LWWRegisterMsg<double> xMsg { get; private set; }

    [JsonInclude]
    public LWWRegisterMsg<double> yMsg { get; private set; }

    [JsonInclude]
    public string type { get; private set; }
    public VertexInfoMsg()
    {
        this.xMsg = new();
        this.yMsg = new();
        this.type = "";
    }

    public VertexInfoMsg(LWWRegister<double> x, LWWRegister<double> y, string type)
    {

        this.xMsg = (LWWRegisterMsg<double>) x.GetLastSynchronizedUpdate();
        this.yMsg = (LWWRegisterMsg<double>) y.GetLastSynchronizedUpdate();
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
    private readonly LWWRegister<double> _x;
    private readonly LWWRegister<double> _y;
    private string _type;

    public VertexInfo(double x, double y, string type)
    {
        this._x = new(x);
        this._y = new(y);
        this._type = type;
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not VertexInfoMsg)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support {nameof(receivedUpdate)} type of {receivedUpdate.GetType()}");
        }

        VertexInfoMsg received = (VertexInfoMsg) receivedUpdate;
        this._x.ApplySynchronizedUpdate(received.xMsg);
        this._y.ApplySynchronizedUpdate(received.yMsg);

        // the types should NEVER be different, but in case they are, implement the following merge policy:
        this._type = this._type.CompareTo(received.type) > 0 ? this._type : received.type;
    }
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        VertexInfoMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate() => new VertexInfoMsg(this._x, this._y, this._type);
}
