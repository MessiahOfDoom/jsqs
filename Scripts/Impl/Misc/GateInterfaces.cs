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

public interface IMultiInputGate {
    public int GetSlotCount();
}

public interface IColorableGate {
    public void SetSlotColors(Color qbitColor, Color bitColor) {
        if(this is GraphNode node) {
            for(int i = 0; i < node.GetInputPortCount(); ++i) {
                var type = node.GetInputPortType(i);
                node.SetSlotColorLeft(i, type == 0 ? qbitColor : type == 1 ? bitColor : Colors.Black);
            }
            for(int i = 0; i < node.GetOutputPortCount(); ++i) {
                var type = node.GetOutputPortType(i);
                node.SetSlotColorRight(i, type == 0 ? qbitColor : type == 1 ? bitColor : Colors.Black);
            }
        }
    }
}

public interface ICompileableGateWithBits {
    public LazyMatrix compile(int QBitCount, Array<int> ForQBits, Array<int> classicBits, ClassicControlBitMatrix.GetMatrixIndexByBits getter);
}