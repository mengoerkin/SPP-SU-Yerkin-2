using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ИНТЕРФЕЙС ОТЧЁТА
public interface IReport
{
    string Generate();
}

// БАЗОВЫЕ КЛАССЫ ОТЧЁТОВ
public class SalesReport : IReport
{
    private readonly List<(string Date, string Product, int Qty, double Amount)> _data = new()
    {
        ("2024-01-05", "Ноутбук",   2,  150000.00),
        ("2024-01-12", "Мышь",     15,    7500.00),
        ("2024-02-03", "Монитор",   5,  125000.00),
        ("2024-02-18", "Клавиатура",8,   16000.00),
        ("2024-03-07", "Наушники", 10,   45000.00),
        ("2024-03-21", "Ноутбук",   1,   75000.00),
        ("2024-04-14", "Мышь",     20,   10000.00),
        ("2024-04-30", "Монитор",   3,   75000.00),
    };

    public List<(string Date, string Product, int Qty, double Amount)> GetRawData() => _data;

    public string Generate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ОТЧЁТ ПО ПРОДАЖАМ ===");
        sb.AppendLine($"{"Дата",-12} {"Товар",-15} {"Кол-во",8} {"Сумма",12}");
        sb.AppendLine(new string('-', 50));
        foreach (var r in _data)
            sb.AppendLine($"{r.Date,-12} {r.Product,-15} {r.Qty,8} {r.Amount,11:F2}");
        sb.AppendLine(new string('-', 50));
        sb.AppendLine($"{"Итого:",-28} {_data.Sum(r => r.Amount),11:F2}");
        return sb.ToString();
    }
}

public class UserReport : IReport
{
    private readonly List<(string RegDate, string Name, string City, int Orders)> _data = new()
    {
        ("2024-01-10", "Иванов Алексей",   "Москва",       12),
        ("2024-01-25", "Петрова Мария",     "Санкт-Петербург", 5),
        ("2024-02-08", "Сидоров Дмитрий",  "Казань",        8),
        ("2024-02-19", "Козлова Анна",      "Новосибирск",   3),
        ("2024-03-03", "Морозов Игорь",     "Екатеринбург", 20),
        ("2024-03-30", "Волкова Светлана",  "Москва",        1),
        ("2024-04-11", "Новиков Павел",     "Самара",        7),
    };

    public List<(string RegDate, string Name, string City, int Orders)> GetRawData() => _data;

    public string Generate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== ОТЧЁТ ПО ПОЛЬЗОВАТЕЛЯМ ===");
        sb.AppendLine($"{"Дата рег.",- 12} {"Имя",-22} {"Город",-20} {"Заказы",7}");
        sb.AppendLine(new string('-', 64));
        foreach (var r in _data)
            sb.AppendLine($"{r.RegDate,-12} {r.Name,-22} {r.City,-20} {r.Orders,7}");
        sb.AppendLine(new string('-', 64));
        sb.AppendLine($"Всего пользователей: {_data.Count}  |  Всего заказов: {_data.Sum(r => r.Orders)}");
        return sb.ToString();
    }
}

// АБСТРАКТНЫЙ ДЕКОРАТОР
public abstract class ReportDecorator : IReport
{
    protected IReport _report;
    public ReportDecorator(IReport report) => _report = report;
    public virtual string Generate() => _report.Generate();
}

// ДЕКОРАТОР: ФИЛЬТР ПО ДАТАМ
public class DateFilterDecorator : ReportDecorator
{
    private readonly DateTime _from;
    private readonly DateTime _to;

    public DateFilterDecorator(IReport report, DateTime from, DateTime to)
        : base(report) { _from = from; _to = to; }

    public override string Generate()
    {
        var raw = _report.Generate();
        var sb = new StringBuilder();
        sb.AppendLine($"[Фильтр по датам: {_from:yyyy-MM-dd} → {_to:yyyy-MM-dd}]");
        foreach (var line in raw.Split('\n'))
        {
            if (DateTime.TryParse(line.Substring(0, Math.Min(10, line.Length)), out var d))
            {
                if (d >= _from && d <= _to) sb.AppendLine(line.TrimEnd());
            }
            else
            {
                sb.AppendLine(line.TrimEnd());
            }
        }
        return sb.ToString();
    }
}

// ДЕКОРАТОР: СОРТИРОВКА
public enum SortCriteria { ByDate, ByAmount, ByName }

public class SortingDecorator : ReportDecorator
{
    private readonly SortCriteria _criteria;

    public SortingDecorator(IReport report, SortCriteria criteria)
        : base(report) { _criteria = criteria; }

    public override string Generate()
    {
        var raw = _report.Generate();
        var lines = raw.Split('\n').ToList();

        // Находим строки с данными (начинаются с даты YYYY-MM-DD)
        var header = new List<string>();
        var dataLines = new List<string>();
        var footer = new List<string>();
        bool dataSection = false;

        foreach (var line in lines)
        {
            if (line.Length >= 10 && DateTime.TryParse(line.Substring(0, 10), out _))
            {
                dataSection = true;
                dataLines.Add(line);
            }
            else if (dataSection && (line.StartsWith("---") || line.StartsWith("Ито") || line.StartsWith("Всег")))
            {
                footer.Add(line);
            }
            else if (!dataSection)
            {
                header.Add(line);
            }
            else
            {
                footer.Add(line);
            }
        }

        var sorted = _criteria switch
        {
            SortCriteria.ByDate   => dataLines.OrderBy(l => l.Substring(0, 10)).ToList(),
            SortCriteria.ByAmount => dataLines.OrderByDescending(l =>
            {
                var parts = l.TrimEnd().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                return double.TryParse(parts.Last(), out var v) ? v : 0;
            }).ToList(),
            SortCriteria.ByName   => dataLines.OrderBy(l =>
            {
                var parts = l.TrimEnd().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : l;
            }).ToList(),
            _ => dataLines
        };

        var sb = new StringBuilder();
        sb.AppendLine($"[Сортировка: {_criteria}]");
        foreach (var l in header)  sb.AppendLine(l.TrimEnd());
        foreach (var l in sorted)  sb.AppendLine(l.TrimEnd());
        foreach (var l in footer)  sb.AppendLine(l.TrimEnd());
        return sb.ToString();
    }
}

// ДЕКОРАТОР: ЭКСПОРТ В CSV
public class CsvExportDecorator : ReportDecorator
{
    public CsvExportDecorator(IReport report) : base(report) { }

    public override string Generate()
    {
        var raw = _report.Generate();
        var sb = new StringBuilder();
        sb.AppendLine("[ЭКСПОРТ: CSV]");
        sb.AppendLine(raw);

        // Имитация CSV-блока
        sb.AppendLine("--- CSV-представление ---");
        foreach (var line in raw.Split('\n'))
        {
            if (line.Length >= 10 && DateTime.TryParse(line.Substring(0, 10), out _))
            {
                var csv = string.Join(";",
                    line.TrimEnd()
                        .Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries));
                sb.AppendLine(csv);
            }
        }
        sb.AppendLine("--- Конец CSV ---");
        return sb.ToString();
    }
}

// ДЕКОРАТОР: ЭКСПОРТ В PDF (симуляция)
public class PdfExportDecorator : ReportDecorator
{
    public PdfExportDecorator(IReport report) : base(report) { }

    public override string Generate()
    {
        var raw = _report.Generate();
        var sb = new StringBuilder();
        sb.AppendLine("[ЭКСПОРТ: PDF]");
        sb.AppendLine("%PDF-1.4 (симуляция)");
        sb.AppendLine(new string('*', 50));
        sb.AppendLine(raw);
        sb.AppendLine(new string('*', 50));
        sb.AppendLine("[Файл сохранён: report.pdf]");
        return sb.ToString();
    }
}

// ДЕКОРАТОР: ФИЛЬТР ПО СУММЕ ПРОДАЖ (доп. задание)
public class AmountFilterDecorator : ReportDecorator
{
    private readonly double _minAmount;

    public AmountFilterDecorator(IReport report, double minAmount)
        : base(report) { _minAmount = minAmount; }

    public override string Generate()
    {
        var raw = _report.Generate();
        var sb = new StringBuilder();
        sb.AppendLine($"[Фильтр по сумме: от {_minAmount:F2} руб.]");
        foreach (var line in raw.Split('\n'))
        {
            if (line.Length >= 10 && DateTime.TryParse(line.Substring(0, 10), out _))
            {
                var parts = line.TrimEnd().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                if (double.TryParse(parts.Last(), out var amount) && amount >= _minAmount)
                    sb.AppendLine(line.TrimEnd());
            }
            else
            {
                sb.AppendLine(line.TrimEnd());
            }
        }
        return sb.ToString();
    }
}

// МЕХАНИЗМ ДИНАМИЧЕСКОГО ВЫБОРА ДЕКОРАТОРОВ (доп. задание)
public class ReportBuilder
{
    private IReport _report;

    public ReportBuilder(IReport baseReport) => _report = baseReport;

    public ReportBuilder WithDateFilter(DateTime from, DateTime to)
    {
        _report = new DateFilterDecorator(_report, from, to);
        return this;
    }

    public ReportBuilder WithSorting(SortCriteria criteria)
    {
        _report = new SortingDecorator(_report, criteria);
        return this;
    }

    public ReportBuilder WithAmountFilter(double minAmount)
    {
        _report = new AmountFilterDecorator(_report, minAmount);
        return this;
    }

    public ReportBuilder WithCsvExport()
    {
        _report = new CsvExportDecorator(_report);
        return this;
    }

    public ReportBuilder WithPdfExport()
    {
        _report = new PdfExportDecorator(_report);
        return this;
    }

    public IReport Build() => _report;
}

// КЛИЕНТСКИЙ КОД
class Program1
{
    static void Section(string title)
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('═', 60));
    }

    static void Main()
    {
        // ── Тест 1: Простой отчёт по продажам ──────────────────
        Section("Тест 1: Базовый отчёт по продажам");
        IReport report = new SalesReport();
        Console.WriteLine(report.Generate());

        // ── Тест 2: Отчёт по продажам + фильтр по датам ────────
        Section("Тест 2: Продажи за февраль–март 2024");
        IReport filtered = new DateFilterDecorator(
            new SalesReport(),
            new DateTime(2024, 2, 1),
            new DateTime(2024, 3, 31));
        Console.WriteLine(filtered.Generate());

        // ── Тест 3: Отчёт пользователи + сортировка по имени ───
        Section("Тест 3: Пользователи, отсортированные по имени");
        IReport sorted = new SortingDecorator(
            new UserReport(),
            SortCriteria.ByName);
        Console.WriteLine(sorted.Generate());

        // ── Тест 4: Продажи + фильтр суммы + CSV ───────────────
        Section("Тест 4: Продажи от 50 000 руб. → экспорт CSV");
        IReport csvReport = new CsvExportDecorator(
            new AmountFilterDecorator(
                new SalesReport(), 50000));
        Console.WriteLine(csvReport.Generate());

        // ── Тест 5: Builder — сложная комбинация ────────────────
        Section("Тест 5: Builder — дата + сортировка + PDF");
        IReport complex = new ReportBuilder(new SalesReport())
            .WithDateFilter(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31))
            .WithSorting(SortCriteria.ByAmount)
            .WithPdfExport()
            .Build();
        Console.WriteLine(complex.Generate());

        // ── Тест 6: Отчёт по пользователям + CSV ────────────────
        Section("Тест 6: Пользователи → экспорт CSV");
        IReport userCsv = new ReportBuilder(new UserReport())
            .WithSorting(SortCriteria.ByDate)
            .WithCsvExport()
            .Build();
        Console.WriteLine(userCsv.Generate());
    }
}
