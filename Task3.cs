using System;
using System.Collections.Generic;
using System.Linq;

QualityControl control = new();
control.Measure(new Inspection("D-301", 1, 20.00, 20.08));
control.Measure(new Inspection("D-302", 2, 15.00, 14.70));
control.Measure(new Inspection("D-303", 2, 20.00, 20.15));
control.Measure(new Inspection("D-304", 1, 10.00, 10.03));

InspectionFilter over005 = QualityControl.DeviationOver(0.05);
InspectionFilter over020 = QualityControl.DeviationOver(0.20);
Print("Відхилення понад 0,05 мм (фабрика)", control.Choose(over005));
Print("Відхилення понад 0,20 мм (фабрика)", control.Choose(over020));
Print("Деталі номіналом 20 мм (лямбда)", control.Choose(x => x.NominalSize == 20.00));

int machine2Count = 0;
control.Measured += inspection =>
{
    if (inspection.MachineNumber == 2)
        machine2Count++;
};

// Лямбда-обробник рахує виміри верстата 2.
control.Measure(new Inspection("D-305", 2, 12.00, 12.07));
control.Measure(new Inspection("D-306", 1, 18.00, 17.97));
Console.WriteLine($"Нових вимірів з верстата 2: {machine2Count}");

control.Sort((a, b) => string.Compare(a.DetailNumber, b.DetailNumber, StringComparison.Ordinal));
Print("За номером деталі (лямбда)", control.Items);

static void Print(string title, IEnumerable<Inspection> inspections)
{
    Console.WriteLine($"{title}:");
    foreach (Inspection inspection in inspections)
        Console.WriteLine($"  {inspection}");
}

delegate bool InspectionFilter(Inspection inspection);

record Inspection(string DetailNumber, int MachineNumber, double NominalSize, double ActualSize)
{
    public double Deviation => ActualSize - NominalSize;
    public override string ToString() =>
        $"{DetailNumber}, верстат {MachineNumber}, номінал {NominalSize:0.00} мм, " +
        $"відхилення {Deviation:+0.00;-0.00;0.00} мм";
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

    public List<Inspection> Choose(InspectionFilter filter) =>
        inspections.Where(x => filter(x)).ToList();

    public void Sort(Comparison<Inspection> comparison) => inspections.Sort(comparison);

    public static InspectionFilter DeviationOver(double mm) =>
        inspection => Math.Abs(inspection.Deviation) > mm;
}
