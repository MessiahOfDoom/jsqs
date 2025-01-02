using Godot;
using Godot.Collections;

public interface IResizeableGate {

    public void SetSlotCount(int slotCount);

    public int GetSlotCount();

}

public interface ICheckpointGate {}

public interface ICompileableGate {

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits);

}

public interface ISaveableGate {
    public Dictionary<string, Variant> Save();

    public void Load(Dictionary<string, Variant> dict);
}

public interface IMeasurementGate {}

public interface IMultiInputGate {
    public int GetSlotCount();
}