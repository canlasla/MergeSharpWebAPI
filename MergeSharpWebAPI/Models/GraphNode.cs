using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MergeSharp;

namespace MergeSharpWebAPI.Models;

public class VertexInfoMsg : PropagationMessage
{
    public override void Decode(byte[] input) => throw new NotImplementedException();
    public override byte[] Encode() => throw new NotImplementedException();
}

public class VertexInfo : CRDT
{
    private readonly LWWRegister<double> _x;
    private readonly LWWRegister<double> _y;
    private readonly String _type;

    public VertexInfo(double x, double y, string type)
    {
        this._x = new(x);
        this._y = new(y);
        this._type = type;
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate) => throw new NotImplementedException();
    public override PropagationMessage DecodePropagationMessage(byte[] input) => throw new NotImplementedException();
    public override PropagationMessage GetLastSynchronizedUpdate() => throw new NotImplementedException();
}
