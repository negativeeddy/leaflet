﻿using NegativeEddy.Leaflet.Instructions;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace NegativeEddy.Leaflet.Memory;

/// <summary>
/// Represents a single routine and its current state (evaluation stack, 
/// return address, local variables, etc)
/// </summary>
[Serializable]
public class Routine : ISerializable
{
    private readonly int _baseAddress;

    public int ReturnAddress { get; } = -1;

    /// <summary>
    /// The Store of the call instruction. The Routine will put its return value
    /// in this location. Store should only be null if it is the first Routine 
    /// on the stack
    /// </summary>
    public ZVariable? Store { get; }
    public IList<ushort> Locals { get; }
    public Stack<ushort> EvaluationStack { get; } = new Stack<ushort>();

    public int FirstInstructionAddress
    {
        // first instruction starts after the local count, then after the local words.
        get { return _baseAddress + 1 + Locals.Count * 2; }
    }

    /// <summary>
    /// Serialization constructor for Routine. This is only used by the serializer
    /// </summary>
    protected Routine(SerializationInfo info, StreamingContext context)
    {
        _baseAddress = info.GetInt32(nameof(_baseAddress));
        ReturnAddress = info.GetInt32(nameof(ReturnAddress));
        Store = (ZVariable)info.GetValue(nameof(Store), typeof(ZVariable));
        Locals = (List<ushort>)info.GetValue(nameof(Locals), typeof(List<ushort>));
        EvaluationStack = (Stack<ushort>)info.GetValue(nameof(EvaluationStack), typeof(Stack<ushort>));
    }

    /// <summary>
    /// Constructs a new Routine object from the address in
    /// a byte array
    /// </summary>
    /// <param name="bytes">bytes representing memory</param>
    /// <param name="routineAddress">the beginning of the Routine's frame in memory</param>
    public Routine(IList<byte> bytes, int routineAddress, int returnAddress, ZVariable? returnStore, IList<ushort> localInitValues)
    {
        Debug.Assert(routineAddress % 2 == 0, "A routine is required to begin at an address in memory which can be represented by a packed address (spec 5.1)");
        _baseAddress = routineAddress;
        this.ReturnAddress = returnAddress;
        Store = returnStore;

        int localVariableCount = bytes[routineAddress];

        // initialize locals from the routine definition
        Locals = bytes.GetWords(routineAddress + 1, localVariableCount);

        // update locals with any provided arguments
        for (int i = 0; i < localInitValues.Count; i++)
        {
            if (i < localInitValues.Count)
            {
                Locals[i] = localInitValues[i];
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Frame start at: {FirstInstructionAddress:x4}");
        sb.Append("Locals ");
        for (int i = 0; i < Locals.Count; i++)
        {
            sb.Append("local");
            sb.Append(i);
            sb.Append('=');
            sb.Append(Locals[i].ToString("x4"));
            sb.Append(' ');
        }
        sb.AppendLine();

        sb.Append("Stack");
        foreach (var item in EvaluationStack)
        {
            sb.Append(item);
            sb.Append(' ');
        }
        sb.AppendLine();

        sb.AppendLine($"Resume at: {ReturnAddress:x4}");

        return sb.ToString();
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(_baseAddress), _baseAddress);
        info.AddValue(nameof(ReturnAddress), ReturnAddress);
        info.AddValue(nameof(Store), Store);
        info.AddValue(nameof(Locals), new List<ushort>(Locals));
        info.AddValue(nameof(EvaluationStack), EvaluationStack);
    }
}
