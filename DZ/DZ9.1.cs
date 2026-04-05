//Телевизор
// Управляет включением, выключением и выбором канала на ТВ.
public class TV
{
    public void TurnOn()    => Console.WriteLine("[TV] Телевизор включён.");
    public void TurnOff()   => Console.WriteLine("[TV] Телевизор выключен.");
    public void SetChannel(int channel) =>
        Console.WriteLine($"[TV] Установлен канал {channel}.");
    public void SetAudioInput(string input) =>
        Console.WriteLine($"[TV] Аудиовход установлен на: {input}.");
}

//Аудиосистема
// Управляет включением, выключением и громкостью звука.
public class AudioSystem
{
    private int _volume = 20;

    public void TurnOn()  => Console.WriteLine("[Audio] Аудиосистема включена.");
    public void TurnOff() => Console.WriteLine("[Audio] Аудиосистема выключена.");

    public void SetVolume(int level)
    {
        _volume = Math.Clamp(level, 0, 100);
        Console.WriteLine($"[Audio] Громкость установлена на {_volume}%.");
    }

    public void VolumeUp(int step = 5)   => SetVolume(_volume + step);
    public void VolumeDown(int step = 5) => SetVolume(_volume - step);
}

//DVD-проигрыватель
// Отвечает за воспроизведение, паузу и остановку DVD-диска.
public class DVDPlayer
{
    public void Play(string title) =>
        Console.WriteLine($"[DVD] Воспроизведение: «{title}».");
    public void Pause()  => Console.WriteLine("[DVD] Пауза.");
    public void Stop()   => Console.WriteLine("[DVD] Остановка.");
    public void Eject()  => Console.WriteLine("[DVD] Диск извлечён.");
}

//Игровая консоль
// Включает консоль и запускает указанную игру.
public class GameConsole
{
    public void TurnOn()  => Console.WriteLine("[Console] Консоль включена.");
    public void TurnOff() => Console.WriteLine("[Console] Консоль выключена.");
    public void StartGame(string gameName) =>
        Console.WriteLine($"[Console] Запуск игры: «{gameName}».");
}

//ФАСАД
// Предоставляет единый простой интерфейс для управления
// всеми подсистемами мультимедиа центра.
public class HomeTheaterFacade
{
    private readonly TV           _tv;
    private readonly AudioSystem  _audio;
    private readonly DVDPlayer    _dvd;
    private readonly GameConsole  _console;

    public HomeTheaterFacade(TV tv, AudioSystem audio,
                              DVDPlayer dvd, GameConsole console)
    {
        _tv      = tv;
        _audio   = audio;
        _dvd     = dvd;
        _console = console;
    }

    // Сценарий 1: включить всю систему для просмотра фильма
    public void WatchMovie(string movieTitle)
    {
        Console.WriteLine("\n=== Подготовка к просмотру фильма ===");
        _tv.TurnOn();
        _tv.SetChannel(1);
        _audio.TurnOn();
        _audio.SetVolume(40);
        _dvd.Play(movieTitle);
    }

    // Сценарий 2: выключить всю систему
    public void TurnOffSystem()
    {
        Console.WriteLine("\n=== Выключение системы ===");
        _dvd.Stop();
        _dvd.Eject();
        _audio.TurnOff();
        _tv.TurnOff();
        _console.TurnOff();
    }

    // Сценарий 3: запустить игровую консоль
    public void PlayGame(string gameName)
    {
        Console.WriteLine("\n=== Запуск игровой сессии ===");
        _tv.TurnOn();
        _tv.SetChannel(3);
        _audio.TurnOn();
        _audio.SetVolume(55);
        _console.TurnOn();
        _console.StartGame(gameName);
    }

    // Сценарий 4: режим прослушивания музыки
    public void ListenToMusic()
    {
        Console.WriteLine("\n=== Режим музыки ===");
        _tv.TurnOn();
        _tv.SetAudioInput("AUX");
        _audio.TurnOn();
        _audio.SetVolume(60);
    }

    // Регулировка громкости через фасад
    public void VolumeUp()   => _audio.VolumeUp();
    public void VolumeDown() => _audio.VolumeDown();
}

//КЛИЕНТСКИЙ КОД
// Демонстрирует использование фасада в различных сценариях.
class Program
{
    static void Main()
    {
        var tv      = new TV();
        var audio   = new AudioSystem();
        var dvd     = new DVDPlayer();
        var console = new GameConsole();

        var theater = new HomeTheaterFacade(tv, audio, dvd, console);

        // Сценарий A: смотрим фильм
        theater.WatchMovie("Интерстеллар");

        // Немного тише
        theater.VolumeDown();

        // Выключаем всё
        theater.TurnOffSystem();

        // Сценарий B: играем в игру
        theater.PlayGame("The Witcher 3");

        // Сценарий C: слушаем музыку
        theater.TurnOffSystem();
        theater.ListenToMusic();
    }
}
