//АБСТРАКТНЫЙ КОМПОНЕНТ
// Общий интерфейс для файлов и папок — клиент работает
// с любым объектом через этот контракт.
public abstract class FileSystemComponent
{
    protected readonly string Name;

    protected FileSystemComponent(string name)
    {
        Name = name;
    }

    // Вывести информацию об объекте (рекурсивно для папок)
    public abstract void Display(int depth = 0);

    // Получить размер: у файла — собственный, у папки — сумма детей
    public abstract long GetSize();

    // Вспомогательный отступ для наглядного вывода дерева
    protected string Indent(int depth) => new string(' ', depth * 2);
}

//ЛИСТ: Файл
// Конечный элемент дерева — хранит имя и размер, не содержит детей.
public class File : FileSystemComponent
{
    private readonly long _size; // в байтах

    public File(string name, long sizeBytes) : base(name)
    {
        if (sizeBytes < 0)
            throw new ArgumentException("Размер файла не может быть отрицательным.");
        _size = sizeBytes;
    }

    public override void Display(int depth = 0)
    {
        Console.WriteLine($"{Indent(depth)}📄 {Name}  ({FormatSize(_size)})");
    }

    public override long GetSize() => _size;

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
        >= 1_024     => $"{bytes / 1_024.0:F1} KB",
        _            => $"{bytes} B"
    };
}

//КОМПОНОВЩИК: Папка
// Контейнер, который может хранить файлы и другие папки,
// делегируя операции своим дочерним компонентам.
public class Directory : FileSystemComponent
{
    private readonly List<FileSystemComponent> _children = new();

    public Directory(string name) : base(name) { }

    // Добавить компонент с проверкой на дубликат
    public void Add(FileSystemComponent component)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        if (_children.Contains(component))
        {
            Console.WriteLine($"[Предупреждение] «{component}» уже есть в «{Name}».");
            return;
        }
        _children.Add(component);
    }

    // Удалить компонент с проверкой на существование
    public void Remove(FileSystemComponent component)
    {
        if (!_children.Contains(component))
        {
            Console.WriteLine($"[Предупреждение] «{component}» не найден в «{Name}».");
            return;
        }
        _children.Remove(component);
    }

    // Рекурсивно вывести всё дерево содержимого папки
    public override void Display(int depth = 0)
    {
        Console.WriteLine($"{Indent(depth)}📁 {Name}/  " +
                          $"({FormatSize(GetSize())}, {_children.Count} эл.)");
        foreach (var child in _children)
            child.Display(depth + 1);
    }

    // Рекурсивно суммировать размеры всех вложенных объектов
    public override long GetSize() =>
        _children.Sum(c => c.GetSize());

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1_048_576 => $"{bytes / 1_048_576.0:F1} MB",
        >= 1_024     => $"{bytes / 1_024.0:F1} KB",
        _            => $"{bytes} B"
    };
}

//КЛИЕНТСКИЙ КОД
// Строит иерархию и работает со всеми объектами единообразно.
class Program
{
    static void Main()
    {
        // Корневая папка
        var root = new Directory("C:\\Projects");

        // Папка Documents с файлами
        var docs = new Directory("Documents");
        docs.Add(new File("readme.txt",    2_048));
        docs.Add(new File("report.pdf",    5_242_880));
        docs.Add(new File("notes.docx",    102_400));

        // Вложенная папка Backup
        var backup = new Directory("Backup");
        backup.Add(new File("backup_v1.zip", 10_485_760));
        backup.Add(new File("backup_v2.zip", 12_582_912));
        docs.Add(backup);

        // Папка Source с кодом
        var src = new Directory("Source");
        src.Add(new File("Program.cs",    8_192));
        src.Add(new File("Facade.cs",     4_096));
        src.Add(new File("Composite.cs",  3_584));

        // Собираем корневую папку
        root.Add(docs);
        root.Add(src);
        root.Add(new File("solution.sln", 1_024));

        //Вывод дерева
        Console.WriteLine("=== Структура файловой системы ===\n");
        root.Display();

        Console.WriteLine($"\n Общий размер корня: {root.GetSize() / 1_048_576.0:F1} MB");

        //Проверка дубликата
        Console.WriteLine("\n=== Тест: добавляем дубликат ===");
        docs.Add(new File("readme.txt", 2_048)); // другой объект — ОК
        // docs.Add(существующий объект) — вызовет предупреждение

        //Удаление и повторный вывод
        Console.WriteLine("\n=== После удаления Backup из Documents ===");
        docs.Remove(backup);
        root.Display();
    }
}
