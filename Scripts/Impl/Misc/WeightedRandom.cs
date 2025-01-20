using Godot;
using System;
using System.Collections.Generic;

public record WeightedRandom<T> {

    public List<WeightedRandomObj<T>> objects = new();
    public double totalWeight = 0;
    public void AddItem(T value, double chance) {
        objects.Add(new WeightedRandomObj<T>(value, chance));
        totalWeight += chance;
    }

    public T Draw() {
        var f = GD.RandRange(0, totalWeight);
        for(int i = 0; i < objects.Count; ++i) {
            f -= objects[i].weight;
            if(f <= 0) return objects[i].value;
        }
        return objects[objects.Count - 1].value;
    }

}

public record WeightedRandomObj<T> {
    public T value;
    public double weight;
    public WeightedRandomObj(T value, double weight) {
        this.value = value;
        this.weight = weight;
    }

}