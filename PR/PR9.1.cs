//1: Бронирование номеров
// Управляет доступностью, бронированием и отменой гостиничных номеров.
public class RoomBookingSystem
{
    private readonly Dictionary<int, bool> _rooms = new()
    {
        { 101, true }, { 102, true }, { 201, true },
        { 202, false }, { 301, true }, { 302, true }
    };

    public bool CheckAvailability(int roomNumber)
    {
        if (!_rooms.ContainsKey(roomNumber))
        {
            Console.WriteLine($"[Бронирование] Номер {roomNumber} не существует.");
            return false;
        }
        return _rooms[roomNumber];
    }

    public bool BookRoom(int roomNumber, string guestName)
    {
        if (!CheckAvailability(roomNumber))
        {
            Console.WriteLine($"[Бронирование] Номер {roomNumber} недоступен.");
            return false;
        }
        _rooms[roomNumber] = false;
        Console.WriteLine($"[Бронирование] Номер {roomNumber} забронирован для {guestName}.");
        return true;
    }

    public bool CancelBooking(int roomNumber)
    {
        if (!_rooms.ContainsKey(roomNumber) || _rooms[roomNumber])
        {
            Console.WriteLine($"[Бронирование] Нет активного бронирования для номера {roomNumber}.");
            return false;
        }
        _rooms[roomNumber] = true;
        Console.WriteLine($"[Бронирование] Бронирование номера {roomNumber} отменено.");
        return true;
    }

    public void ListAvailableRooms()
    {
        var available = _rooms.Where(r => r.Value).Select(r => r.Key).ToList();
        Console.WriteLine($"[Бронирование] Свободные номера: {string.Join(", ", available)}.");
    }
}

//2: Ресторан
// Обрабатывает бронирование столов, заказы блюд и такси.
public class RestaurantSystem
{
    private int _tableCounter = 1;

    public int BookTable(string guestName, int persons, DateTime time)
    {
        int tableId = _tableCounter++;
        Console.WriteLine($"[Ресторан] Стол #{tableId} забронирован для {guestName}" +
                          $" ({persons} чел.) на {time:HH:mm}.");
        return tableId;
    }

    public void OrderFood(int tableId, params string[] dishes)
    {
        Console.WriteLine($"[Ресторан] Стол #{tableId} — заказ: {string.Join(", ", dishes)}.");
    }

    public void CancelTableBooking(int tableId)
    {
        Console.WriteLine($"[Ресторан] Бронирование стола #{tableId} отменено.");
    }

    public void CallTaxi(string guestName, string destination)
    {
        Console.WriteLine($"[Ресторан] Такси заказано для {guestName} → {destination}.");
    }
}

//3: Управление мероприятиями
// Организует конференц-залы, оборудование и расписание событий.
public class EventManagementSystem
{
    public bool BookConferenceHall(string hallName, string organizer, DateTime date)
    {
        Console.WriteLine($"[Мероприятия] Зал «{hallName}» забронирован для «{organizer}»" +
                          $" на {date:dd.MM.yyyy HH:mm}.");
        return true;
    }

    public void OrderEquipment(string hallName, params string[] equipment)
    {
        Console.WriteLine($"[Мероприятия] Для зала «{hallName}» заказано: " +
                          $"{string.Join(", ", equipment)}.");
    }

    public void CancelEvent(string hallName)
    {
        Console.WriteLine($"[Мероприятия] Мероприятие в зале «{hallName}» отменено.");
    }
}

//4: Служба уборки
// Планирует регулярную уборку и выполняет уборку по запросу.
public class CleaningService
{
    public void ScheduleCleaning(int roomNumber, string time)
    {
        Console.WriteLine($"[Уборка] Уборка номера {roomNumber} запланирована на {time}.");
    }

    public void CleanNow(int roomNumber)
    {
        Console.WriteLine($"[Уборка] Срочная уборка номера {roomNumber} начата.");
    }

    public void CancelCleaning(int roomNumber)
    {
        Console.WriteLine($"[Уборка] Уборка номера {roomNumber} отменена.");
    }
}

//ФАСАД
// Единая точка входа: скрывает оркестрацию всех подсистем отеля.
public class HotelFacade
{
    private readonly RoomBookingSystem     _rooms;
    private readonly RestaurantSystem      _restaurant;
    private readonly EventManagementSystem _events;
    private readonly CleaningService       _cleaning;

    public HotelFacade()
    {
        _rooms      = new RoomBookingSystem();
        _restaurant = new RestaurantSystem();
        _events     = new EventManagementSystem();
        _cleaning   = new CleaningService();
    }

    //Сценарий 1: Полный заезд гостя с доп. услугами
    // Бронирует номер, планирует уборку и при желании заказывает столик.
    public void CheckInWithServices(string guestName, int roomNumber,
                                    bool withDinner = false)
    {
        Console.WriteLine($"\n{'═',1}{'═' * 50}");
        Console.WriteLine($"  ЗАЕЗД ГОСТЯ: {guestName}");
        Console.WriteLine($"{'═',1}{'═' * 50}");

        _rooms.ListAvailableRooms();

        bool booked = _rooms.BookRoom(roomNumber, guestName);
        if (!booked) return;

        _cleaning.ScheduleCleaning(roomNumber, "09:00");

        if (withDinner)
        {
            _restaurant.BookTable(guestName, 2, DateTime.Today.AddHours(19));
            _restaurant.OrderFood(1, "Стейк рибай", "Бокал вина", "Тирамису");
        }
    }

    //Сценарий 2: Организация корпоративного мероприятия
    // Бронирует зал, заказывает оборудование и номера для участников.
    public void OrganizeEvent(string organizerName, string hallName,
                              DateTime eventDate, int[] participantRooms)
    {
        Console.WriteLine($"\n{'═',1}{'═' * 50}");
        Console.WriteLine($"  ОРГАНИЗАЦИЯ МЕРОПРИЯТИЯ: {organizerName}");
        Console.WriteLine($"{'═',1}{'═' * 50}");

        _events.BookConferenceHall(hallName, organizerName, eventDate);
        _events.OrderEquipment(hallName, "Проектор", "Микрофоны", "Флипчарт");

        foreach (int room in participantRooms)
            _rooms.BookRoom(room, $"Участник ({organizerName})");

        _restaurant.BookTable(organizerName, participantRooms.Length,
                              eventDate.AddHours(1));
    }

    //Сценарий 3: Бронирование стола с вызовом такси
    // Резервирует столик в ресторане и заказывает такси по завершении.
    public void BookDinnerWithTaxi(string guestName, int persons,
                                   DateTime dinnerTime, string taxiDestination)
    {
        Console.WriteLine($"\n{'═',1}{'═' * 50}");
        Console.WriteLine($"  УЖИН + ТАКСИ: {guestName}");
        Console.WriteLine($"{'═',1}{'═' * 50}");

        int tableId = _restaurant.BookTable(guestName, persons, dinnerTime);
        _restaurant.OrderFood(tableId, "Меню дня", "Вино");
        _restaurant.CallTaxi(guestName, taxiDestination);
    }

    //Сценарий 4: Полная отмена бронирований гостя
    // Отменяет номер, уборку и столик за один вызов.
    public void CancelAllBookings(string guestName, int roomNumber, int tableId = -1)
    {
        Console.WriteLine($"\n{'═',1}{'═' * 50}");
        Console.WriteLine($"  ОТМЕНА БРОНИРОВАНИЙ: {guestName}");
        Console.WriteLine($"{'═',1}{'═' * 50}");

        _rooms.CancelBooking(roomNumber);
        _cleaning.CancelCleaning(roomNumber);

        if (tableId > 0)
            _restaurant.CancelTableBooking(tableId);
    }

    //Сценарий 5: Уборка по запросу
    // Вызывает немедленную уборку в указанном номере.
    public void RequestImmediateCleaning(int roomNumber)
    {
        Console.WriteLine($"\n[Запрос уборки] Номер {roomNumber}");
        _cleaning.CleanNow(roomNumber);
    }
}

//КЛИЕНТСКИЙ КОД
// Демонстрирует все сценарии работы с гостиницей через фасад.
class Program
{
    static void Main()
    {
        var hotel = new HotelFacade();

        // A: Заезд Алексея с ужином
        hotel.CheckInWithServices("Алексей Петров", roomNumber: 101, withDinner: true);

        // B: Корпоративное мероприятие
        hotel.OrganizeEvent(
            organizerName:    "ООО «ТехСтарт»",
            hallName:         "Конференц-зал А",
            eventDate:        DateTime.Today.AddDays(3).AddHours(10),
            participantRooms: new[] { 201, 301, 302 }
        );

        // C: Ужин и такси для гостьи
        hotel.BookDinnerWithTaxi(
            guestName:       "Мария Иванова",
            persons:         2,
            dinnerTime:      DateTime.Today.AddHours(20),
            taxiDestination: "Аэропорт Домодедово"
        );

        // D: Срочная уборка
        hotel.RequestImmediateCleaning(101);

        // E: Полная отмена
        hotel.CancelAllBookings("Алексей Петров", roomNumber: 101, tableId: 1);
    }
}
