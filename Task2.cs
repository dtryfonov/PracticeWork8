using System;
using System.Collections.Generic;

QualityControl control = new();
DeviationCounter counter = new();

control.Measured += ShowMeasurement;
control.Measured += counter.Record;
control.Measured += StopMachineIfCritical;
Console.WriteLine($"Обробників до вилучення: {HandlerCount(control.Measured)}");

control.Measure(new Inspection("D-201", 1, 20.00, 20.08));
control.Measure(new Inspection("D-202", 2, 15.00, 14.42));

control.Measured -= ShowMeasurement;
Console.WriteLine($"Обробників після вилучення: {HandlerCount(control.Measured)}");
control.Measure(new Inspection("D-203", 1, 10.00, 10.15));
control.Measure(new Inspection("D-204", 2, 25.00, 25.03));

Console.WriteLine($"Деталей з відхиленням понад 0,1 мм: {counter.Count}");
control.Sort(ByDeviationDescending);

Console.WriteLine("За абсолютним відхиленням (спадання):");
foreach (Inspection inspection in control.Items)
    Console.WriteLine($"  {inspection}");

static void ShowMeasurement(Inspection inspection) =>
    Console.WriteLine($"Верстат {inspection.MachineNumber}: {inspection.DetailNumber}, " +
                      $"відхилення {inspection.Deviation:+0.00;-0.00;0.00} мм");

static void StopMachineIfCritical(Inspection inspection)
{
    if (Math.Abs(inspection.Deviation) > 0.50)
        Console.WriteLine($"УВАГА: зупинити верстат {inspection.MachineNumber}; " +
                          $"критичне відхилення для {inspection.DetailNumber}.");
}

static int HandlerCount(Action<Inspection>? handlers) => handlers?.GetInvocationList().Length ?? 0;
static int ByDeviationDescending(Inspection a, Inspection b) =>
    Math.Abs(b.Deviation).CompareTo(Math.Abs(a.Deviation));

record Inspection(string DetailNumber, int MachineNumber, double NominalSize, double ActualSize)
{
    public double Deviation => ActualSize - NominalSize;
    public override string ToString() =>
        $"{DetailNumber}, верстат {MachineNumber}, відхилення {Deviation:+0.00;-0.00;0.00} мм";
}

class QualityControl
{
    private readonly List<Inspection> inspections = [];
    public Action<Inspection>? Measured { get; set; }
    public IReadOnlyList<Inspection> Items => inspections;

    public void Measure(Inspection inspection)
    {
        inspections.Add(inspection);
        Measured?.Invoke(inspection);
    }

    public void Sort(Comparison<Inspection> comparison) => inspections.Sort(comparison);
}

class DeviationCounter
{
    public int Count { get; private set; }
    public void Record(Inspection inspection)
    {
        if (Math.Abs(inspection.Deviation) > 0.10)
            Count++;
    }
}
