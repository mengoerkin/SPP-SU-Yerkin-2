using System;

// БАЗОВЫЙ ИНТЕРФЕЙС
public abstract class Beverage
{
    public abstract string GetDescription();
    public abstract double Cost();

    public override string ToString()
        => $"{GetDescription()} — {Cost():F2} руб.";
}

// БАЗОВЫЕ НАПИТКИ
public class Espresso : Beverage
{
    public override string GetDescription() => "Эспрессо";
    public override double Cost() => 100.0;
}

public class Tea : Beverage
{
    public override string GetDescription() => "Чай";
    public override double Cost() => 60.0;
}

public class Latte : Beverage
{
    public override string GetDescription() => "Латте";
    public override double Cost() => 150.0;
}

public class Mocha : Beverage
{
    public override string GetDescription() => "Мокко";
    public override double Cost() => 160.0;
}

// АБСТРАКТНЫЙ ДЕКОРАТОР
public abstract class BeverageDecorator : Beverage
{
    protected Beverage _beverage;

    public BeverageDecorator(Beverage beverage)
    {
        _beverage = beverage;
    }

    public override string GetDescription() => _beverage.GetDescription();
    public override double Cost() => _beverage.Cost();
}

// КОНКРЕТНЫЕ ДЕКОРАТОРЫ (ДОБАВКИ)
public class Milk : BeverageDecorator
{
    public Milk(Beverage beverage) : base(beverage) { }

    public override string GetDescription()
        => _beverage.GetDescription() + ", Молоко";

    public override double Cost()
        => _beverage.Cost() + 20.0;
}

public class Sugar : BeverageDecorator
{
    public Sugar(Beverage beverage) : base(beverage) { }

    public override string GetDescription()
        => _beverage.GetDescription() + ", Сахар";

    public override double Cost()
        => _beverage.Cost() + 5.0;
}

public class WhippedCream : BeverageDecorator
{
    public WhippedCream(Beverage beverage) : base(beverage) { }

    public override string GetDescription()
        => _beverage.GetDescription() + ", Взбитые сливки";

    public override double Cost()
        => _beverage.Cost() + 35.0;
}

public class Syrup : BeverageDecorator
{
    private readonly string _flavor;

    public Syrup(Beverage beverage, string flavor = "Ванильный") : base(beverage)
    {
        _flavor = flavor;
    }

    public override string GetDescription()
        => _beverage.GetDescription() + $", {_flavor} сироп";

    public override double Cost()
        => _beverage.Cost() + 30.0;
}

public class Cinnamon : BeverageDecorator
{
    public Cinnamon(Beverage beverage) : base(beverage) { }

    public override string GetDescription()
        => _beverage.GetDescription() + ", Корица";

    public override double Cost()
        => _beverage.Cost() + 10.0;
}

public class ExtraShot : BeverageDecorator
{
    public ExtraShot(Beverage beverage) : base(beverage) { }

    public override string GetDescription()
        => _beverage.GetDescription() + ", Двойной шот";

    public override double Cost()
        => _beverage.Cost() + 50.0;
}

// КЛИЕНТСКИЙ КОД
class Program
{
    static void PrintOrder(string label, Beverage beverage)
    {
        Console.WriteLine($"\n  {label}");
        Console.WriteLine($"  Состав : {beverage.GetDescription()}");
        Console.WriteLine($"  Итого  : {beverage.Cost():F2} руб.");
    }

    static void Main()
    {
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║         КАФЕ — СИСТЕМА ЗАКАЗОВ               ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");

        // --- Заказ 1: Простой эспрессо ---
        Console.WriteLine("\n┌─ Заказ 1 ─────────────────────────────────┐");
        Beverage order1 = new Espresso();
        PrintOrder("Эспрессо без добавок:", order1);

        // --- Заказ 2: Эспрессо + молоко + сахар ---
        Console.WriteLine("\n┌─ Заказ 2 ─────────────────────────────────┐");
        Beverage order2 = new Espresso();
        order2 = new Milk(order2);
        order2 = new Sugar(order2);
        PrintOrder("Эспрессо с молоком и сахаром:", order2);

        // --- Заказ 3: Латте + ванильный сироп + взбитые сливки ---
        Console.WriteLine("\n┌─ Заказ 3 ─────────────────────────────────┐");
        Beverage order3 = new Latte();
        order3 = new Syrup(order3, "Ванильный");
        order3 = new WhippedCream(order3);
        PrintOrder("Латте с ванильным сиропом и сливками:", order3);

        // --- Заказ 4: Мокко + двойной шот + корица + взбитые сливки ---
        Console.WriteLine("\n┌─ Заказ 4 ─────────────────────────────────┐");
        Beverage order4 = new Mocha();
        order4 = new ExtraShot(order4);
        order4 = new Cinnamon(order4);
        order4 = new WhippedCream(order4);
        PrintOrder("Мокко делюкс:", order4);

        // --- Заказ 5: Чай + молоко + сахар + сахар ---
        Console.WriteLine("\n┌─ Заказ 5 ─────────────────────────────────┐");
        Beverage order5 = new Tea();
        order5 = new Milk(order5);
        order5 = new Sugar(order5);
        order5 = new Sugar(order5);  // двойной сахар
        order5 = new Cinnamon(order5);
        PrintOrder("Чай масала:", order5);

        // --- Заказ 6: Максимальная кастомизация ---
        Console.WriteLine("\n┌─ Заказ 6 ─────────────────────────────────┐");
        Beverage order6 = new Espresso();
        order6 = new ExtraShot(order6);
        order6 = new Milk(order6);
        order6 = new Syrup(order6, "Карамельный");
        order6 = new WhippedCream(order6);
        order6 = new Cinnamon(order6);
        PrintOrder("Карамельный макиато (всё включено):", order6);

        Console.WriteLine("\n╚══════════════════════════════════════════════╝");
        Console.WriteLine("  Спасибо за заказ! Приятного кофепития ☕");
        Console.WriteLine("╚══════════════════════════════════════════════╝\n");
    }
}
