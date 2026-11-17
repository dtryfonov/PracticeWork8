using System;
using System.Collections.Generic;
using System.Linq;

QualityControl control = new();
control.Measure(new Inspection("D-101", 1, 20.00, 20.08));
control.Measure(new Inspection("D-102", 2, 15.00, 14.92));
control.Measure(new Inspection("D-103", 1, 10.00, 10.15));
control.Measure(new Inspection("D-104", 2, 25.00, 25.03));

Print("Деталі з перевищенням номіналу", control.Choose(IsOversize));
Print("Деталі з верстата 1", control.Choose(FromMachine1));

Verdict normal = QualityControl.VerdictFor(precision: false);
Verdict precision = QualityControl.VerdictFor(precision: true);
Console.WriteLine($"Правила: {normal.Method.Name}, {precision.Method.Name}");
foreach (Inspection inspection in control.Items)
{
    Console.WriteLine($"  {inspection.DetailNumber}: звичайна — {normal(inspection)}, " +
                      $"прецизійна — {precision(inspection)}");
}

static bool IsOversize(Inspection inspection) => inspection.ActualSize > inspection.NominalSize;
static bool FromMachine1(Inspection inspection) => inspection.MachineNumber == 1;

static void Print(string title, IEnumerable<Inspection> inspections)
{
    Console.WriteLine($"{title}:");
    foreach (Inspection inspection in inspections)
        Console.WriteLine($"  {inspection}");
}

delegate bool InspectionFilter(Inspection inspection);
delegate string Verdict(Inspection inspection);

record Inspection(string DetailNumber, int MachineNumber, double NominalSize, double ActualSize)
{
    public double Deviation => ActualSize - NominalSize;
    public override string ToString() =>
        $"{DetailNumber}, верстат {MachineNumber}, номінал {NominalSize:0.00} мм, " +
        $"факт {ActualSize:0.00} мм, відхилення {Deviation:+0.00;-0.00;0.00} мм";
}

class QualityControl
{
    private readonly List<Inspection> inspections = [];
    public int Count => inspections.Count;
    public Inspection this[int index] => inspections[index];
    public IReadOnlyList<Inspection> Items => inspections;

    public void Measure(Inspection inspection) => inspections.Add(inspection);
    public List<Inspection> Choose(InspectionFilter filter) => inspections.Where(x => filter(x)).ToList();
    public static Verdict VerdictFor(bool precision) => precision ? PrecisionVerdict : NormalVerdict;

    private static string NormalVerdict(Inspection inspection) =>
        Math.Abs(inspection.Deviation) <= 0.10 ? "придатна" : "брак";

    private static string PrecisionVerdict(Inspection inspection) =>
        Math.Abs(inspection.Deviation) <= 0.02 ? "придатна" : "брак";
}
