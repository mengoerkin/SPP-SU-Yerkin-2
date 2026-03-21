using System;
using System.Collections.Generic;

// ВНУТРЕННИЙ ИНТЕРФЕЙС ДОСТАВКИ
public interface IInternalDeliveryService
{
    void DeliverOrder(string orderId);
    string GetDeliveryStatus(string orderId);
    double CalculateDeliveryCost(string orderId, double weightKg);  // доп. задание
}

// ВНУТРЕННЯЯ СЛУЖБА ДОСТАВКИ
public class InternalDeliveryService : IInternalDeliveryService
{
    public void DeliverOrder(string orderId)
    {
        Log($"[Внутренняя служба] Запущена доставка заказа #{orderId}");
    }

    public string GetDeliveryStatus(string orderId)
    {
        Log($"[Внутренняя служба] Запрос статуса заказа #{orderId}");
        return $"Заказ #{orderId}: В пути (внутренняя доставка)";
    }

    public double CalculateDeliveryCost(string orderId, double weightKg)
    {
        double cost = 200 + weightKg * 50;
        Log($"[Внутренняя служба] Стоимость доставки заказа #{orderId}: {cost:F2} руб.");
        return cost;
    }

    private void Log(string msg) => Console.WriteLine(msg);
}

// СТОРОННИЕ СЛУЖБЫ (собственные интерфейсы)
// --- Служба A (работает с int-идентификаторами) ---
public class ExternalLogisticsServiceA
{
    public void ShipItem(int itemId)
        => Console.WriteLine($"  [ServiceA] Отправка посылки с ID={itemId}");

    public string TrackShipment(int shipmentId)
        => $"  [ServiceA] Статус отправки {shipmentId}: Доставляется курьером DHL";

    public double GetShippingCost(int itemId, double weight)
    {
        double cost = 350 + weight * 75;
        Console.WriteLine($"  [ServiceA] Цена доставки ID={itemId}: {cost:F2} руб.");
        return cost;
    }
}

// --- Служба B (работает со строками / трек-кодами) ---
public class ExternalLogisticsServiceB
{
    public void SendPackage(string packageInfo)
        => Console.WriteLine($"  [ServiceB] Отправка пакета: {packageInfo}");

    public string CheckPackageStatus(string trackingCode)
        => $"  [ServiceB] Трек {trackingCode}: Прибыл в сортировочный центр";

    public double EstimateDeliveryCost(string packageInfo, double weightKg)
    {
        double cost = 180 + weightKg * 60;
        Console.WriteLine($"  [ServiceB] Оценка стоимости [{packageInfo}]: {cost:F2} руб.");
        return cost;
    }
}

// --- Служба C — новая (доп. задание) ---
public class ExternalLogisticsServiceC
{
    public bool DispatchCargo(Guid cargoGuid, string destination)
    {
        Console.WriteLine($"  [ServiceC] Отправка груза {cargoGuid} → {destination}");
        return true;
    }

    public string QueryCargoLocation(Guid cargoGuid)
        => $"  [ServiceC] Груз {cargoGuid}: На таможне, г. Москва";

    public decimal PriceForCargo(Guid cargoGuid, decimal weightTons)
    {
        decimal cost = 1500m + weightTons * 1000 * 55m;
        Console.WriteLine($"  [ServiceC] Стоимость груза {cargoGuid}: {cost:F2} руб.");
        return cost;
    }
}

// АДАПТЕРЫ
// --- Адаптер для ServiceA ---
public class LogisticsAdapterA : IInternalDeliveryService
{
    private readonly ExternalLogisticsServiceA _service = new();
    private readonly Dictionary<string, int> _orderToId = new();
    private int _counter = 1000;

    private int GetOrCreateId(string orderId)
    {
        if (!_orderToId.ContainsKey(orderId))
            _orderToId[orderId] = _counter++;
        return _orderToId[orderId];
    }

    public void DeliverOrder(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterA] Адаптация: orderId '{orderId}' → int");
            _service.ShipItem(GetOrCreateId(orderId));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterA] Ошибка доставки: {ex.Message}");
        }
    }

    public string GetDeliveryStatus(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterA] Запрос статуса для '{orderId}'");
            return _service.TrackShipment(GetOrCreateId(orderId));
        }
        catch (Exception ex)
        {
            return $"[AdapterA] Ошибка статуса: {ex.Message}";
        }
    }

    public double CalculateDeliveryCost(string orderId, double weightKg)
    {
        try
        {
            Console.WriteLine($"[AdapterA] Расчёт стоимости для '{orderId}'");
            return _service.GetShippingCost(GetOrCreateId(orderId), weightKg);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterA] Ошибка расчёта: {ex.Message}");
            return -1;
        }
    }
}

// --- Адаптер для ServiceB ---
public class LogisticsAdapterB : IInternalDeliveryService
{
    private readonly ExternalLogisticsServiceB _service = new();
    private readonly Dictionary<string, string> _trackCodes = new();

    private string GetTrackCode(string orderId)
    {
        if (!_trackCodes.ContainsKey(orderId))
            _trackCodes[orderId] = $"TRK-{orderId.ToUpper()}-{DateTime.Now.Ticks % 9999}";
        return _trackCodes[orderId];
    }

    public void DeliverOrder(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterB] Адаптация: orderId '{orderId}' → packageInfo");
            _service.SendPackage($"ORDER:{orderId}|DATE:{DateTime.Now:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterB] Ошибка доставки: {ex.Message}");
        }
    }

    public string GetDeliveryStatus(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterB] Запрос статуса для '{orderId}'");
            return _service.CheckPackageStatus(GetTrackCode(orderId));
        }
        catch (Exception ex)
        {
            return $"[AdapterB] Ошибка статуса: {ex.Message}";
        }
    }

    public double CalculateDeliveryCost(string orderId, double weightKg)
    {
        try
        {
            Console.WriteLine($"[AdapterB] Расчёт стоимости для '{orderId}'");
            return _service.EstimateDeliveryCost($"ORDER:{orderId}", weightKg);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterB] Ошибка расчёта: {ex.Message}");
            return -1;
        }
    }
}

// --- Адаптер для ServiceC (доп. задание) ---
public class LogisticsAdapterC : IInternalDeliveryService
{
    private readonly ExternalLogisticsServiceC _service = new();
    private readonly Dictionary<string, Guid> _guids = new();

    private Guid GetOrCreateGuid(string orderId)
    {
        if (!_guids.ContainsKey(orderId))
            _guids[orderId] = Guid.NewGuid();
        return _guids[orderId];
    }

    public void DeliverOrder(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterC] Адаптация: orderId '{orderId}' → Guid");
            bool ok = _service.DispatchCargo(GetOrCreateGuid(orderId), "Склад №1, Москва");
            if (!ok) throw new Exception("DispatchCargo вернул false");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterC] Ошибка доставки: {ex.Message}");
        }
    }

    public string GetDeliveryStatus(string orderId)
    {
        try
        {
            Console.WriteLine($"[AdapterC] Запрос статуса для '{orderId}'");
            return _service.QueryCargoLocation(GetOrCreateGuid(orderId));
        }
        catch (Exception ex)
        {
            return $"[AdapterC] Ошибка статуса: {ex.Message}";
        }
    }

    public double CalculateDeliveryCost(string orderId, double weightKg)
    {
        try
        {
            Console.WriteLine($"[AdapterC] Расчёт стоимости для '{orderId}'");
            decimal weightTons = (decimal)(weightKg / 1000.0);
            return (double)_service.PriceForCargo(GetOrCreateGuid(orderId), weightTons);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AdapterC] Ошибка расчёта: {ex.Message}");
            return -1;
        }
    }
}

// ФАБРИКА СЛУЖБ ДОСТАВКИ
public enum DeliveryServiceType { Internal, ExternalA, ExternalB, ExternalC }

public static class DeliveryServiceFactory
{
    public static IInternalDeliveryService Create(DeliveryServiceType type)
    {
        Console.WriteLine($"[Фабрика] Создание службы: {type}");
        return type switch
        {
            DeliveryServiceType.Internal  => new InternalDeliveryService(),
            DeliveryServiceType.ExternalA => new LogisticsAdapterA(),
            DeliveryServiceType.ExternalB => new LogisticsAdapterB(),
            DeliveryServiceType.ExternalC => new LogisticsAdapterC(),
            _ => throw new ArgumentException($"Неизвестный тип службы: {type}")
        };
    }
}

// КЛИЕНТСКИЙ КОД
class Program2
{
    static void TestService(IInternalDeliveryService svc, string orderId, double weight)
    {
        svc.DeliverOrder(orderId);
        Console.WriteLine("  Статус: " + svc.GetDeliveryStatus(orderId));
        double cost = svc.CalculateDeliveryCost(orderId, weight);
        Console.WriteLine($"  Стоимость доставки: {cost:F2} руб.");
        Console.WriteLine();
    }

    static void Section(string title)
    {
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('═', 60));
    }

    static void Main()
    {
        var orders = new[] { ("ORD-001", 2.5), ("ORD-002", 10.0), ("ORD-003", 0.5) };

        foreach (DeliveryServiceType type in Enum.GetValues<DeliveryServiceType>())
        {
            Section($"Служба: {type}");
            var svc = DeliveryServiceFactory.Create(type);
            foreach (var (id, weight) in orders)
                TestService(svc, id, weight);
        }
    }
}
