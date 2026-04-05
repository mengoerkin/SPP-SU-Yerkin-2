//АБСТРАКТНЫЙ КОМПОНЕНТ
// Единый контракт для сотрудников и отделов — клиент не различает их.
public abstract class OrganizationComponent
{
    public string Name     { get; protected set; }
    public string Position { get; protected set; }

    protected OrganizationComponent(string name, string position)
    {
        Name     = name;
        Position = position;
    }

    // Отобразить элемент (и детей, если есть)
    public abstract void Display(int depth = 0);

    // Рассчитать бюджет (зарплаты штатных сотрудников)
    public abstract decimal GetBudget();

    // Подсчитать штатный персонал (контракторы — не в счёт)
    public abstract int GetHeadcount();

    // Поиск по имени (возвращает первое совпадение)
    public abstract OrganizationComponent? FindByName(string name);

    // Вывести плоский список всех сотрудников
    public abstract void ListAllEmployees(List<string> result);

    protected string Indent(int d) => new(' ', d * 3);
}

//ЛИСТ: Штатный сотрудник
// Конечный узел — хранит личные данные и входит в бюджет отдела.
public class Employee : OrganizationComponent
{
    public decimal Salary { get; private set; }

    public Employee(string name, string position, decimal salary)
        : base(name, position)
    {
        if (salary < 0) throw new ArgumentException("Зарплата не может быть отрицательной.");
        Salary = salary;
    }

    // Изменить зарплату с автоматическим пересчётом бюджетов выше
    public void SetSalary(decimal newSalary)
    {
        if (newSalary < 0) throw new ArgumentException("Зарплата не может быть отрицательной.");
        Console.WriteLine($"[HR] Зарплата {Name}: {Salary:N0} → {newSalary:N0} ₽.");
        Salary = newSalary;
    }

    public override void Display(int depth = 0)
    {
        Console.WriteLine($"{Indent(depth)}👤 {Name} | {Position} | {Salary:N0} ₽/мес");
    }

    public override decimal GetBudget()    => Salary;
    public override int     GetHeadcount() => 1;

    public override OrganizationComponent? FindByName(string name) =>
        Name.Equals(name, StringComparison.OrdinalIgnoreCase) ? this : null;

    public override void ListAllEmployees(List<string> result) =>
        result.Add($"{Name} ({Position})");
}

//ЛИСТ: Контрактор (дополнительное задание)
// Временный сотрудник — учитывается в штате, но не в бюджете отдела.
public class Contractor : OrganizationComponent
{
    public decimal FixedRate { get; private set; }

    public Contractor(string name, string position, decimal fixedRate)
        : base(name, position)
    {
        FixedRate = fixedRate;
    }

    public override void Display(int depth = 0)
    {
        Console.WriteLine($"{Indent(depth)}📋 {Name} | {Position} | контрактор ({FixedRate:N0} ₽/мес, вне бюджета)");
    }

    public override decimal GetBudget()    => 0m;   // не входит в бюджет
    public override int     GetHeadcount() => 1;    // считается в штате

    public override OrganizationComponent? FindByName(string name) =>
        Name.Equals(name, StringComparison.OrdinalIgnoreCase) ? this : null;

    public override void ListAllEmployees(List<string> result) =>
        result.Add($"{Name} ({Position}, контрактор)");
}

//КОМПОНОВЩИК: Отдел / Департамент
// Контейнер, который хранит сотрудников и вложенные отделы,
// делегируя расчёты рекурсивно всем дочерним компонентам.
public class Department : OrganizationComponent
{
    private readonly List<OrganizationComponent> _children = new();

    public Department(string name) : base(name, "Отдел") { }

    // Добавить компонент с проверкой на дублирование
    public void Add(OrganizationComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (_children.Contains(component))
        {
            Console.WriteLine($"[Предупреждение] «{component.Name}» уже есть в «{Name}».");
            return;
        }
        _children.Add(component);
    }

    // Удалить компонент с проверкой на существование
    public void Remove(OrganizationComponent component)
    {
        if (!_children.Remove(component))
            Console.WriteLine($"[Предупреждение] «{component.Name}» не найден в «{Name}».");
        else
            Console.WriteLine($"[Структура] {component.Name} удалён из «{Name}».");
    }

    // Рекурсивно отобразить всё дерево отдела
    public override void Display(int depth = 0)
    {
        Console.WriteLine($"{Indent(depth)}🏢 {Name}  " +
                          $"[бюджет: {GetBudget():N0} ₽ | штат: {GetHeadcount()} чел.]");
        foreach (var child in _children)
            child.Display(depth + 1);
    }

    // Рекурсивная сумма зарплат всех штатных сотрудников
    public override decimal GetBudget()    => _children.Sum(c => c.GetBudget());

    // Рекурсивный подсчёт всего персонала (штат + контракторы)
    public override int     GetHeadcount() => _children.Sum(c => c.GetHeadcount());

    // Поиск по имени вглубь дерева
    public override OrganizationComponent? FindByName(string name)
    {
        if (Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return this;
        foreach (var child in _children)
        {
            var found = child.FindByName(name);
            if (found != null) return found;
        }
        return null;
    }

    // Плоский список всех сотрудников отдела и вложенных отделов
    public override void ListAllEmployees(List<string> result)
    {
        foreach (var child in _children)
            child.ListAllEmployees(result);
    }
}

//КЛИЕНТСКИЙ КОД
// Строит реалистичную корпоративную иерархию и демонстрирует все операции.
class Program
{
    static void Main()
    {
        //Создание сотрудников
        var ceo        = new Employee("Игорь Смирнов",    "CEO",               350_000m);

        var devLead    = new Employee("Анна Козлова",     "Тимлид",            200_000m);
        var dev1       = new Employee("Дмитрий Фролов",   "Senior Developer",  160_000m);
        var dev2       = new Employee("Ольга Белова",     "Middle Developer",  120_000m);
        var contractor = new Contractor("Джон Доу",       "Freelance Dev",      80_000m);

        var hrLead     = new Employee("Светлана Мирова",  "HR Manager",        130_000m);
        var hr1        = new Employee("Павел Тихонов",    "HR Specialist",      90_000m);

        var mktLead    = new Employee("Елена Захарова",   "CMO",               180_000m);
        var mkt1       = new Employee("Роман Власов",     "Маркетолог",        100_000m);

        //Построение иерархии
        var devDept = new Department("Отдел разработки");
        devDept.Add(devLead);
        devDept.Add(dev1);
        devDept.Add(dev2);
        devDept.Add(contractor);  // контрактор — не в бюджет

        var hrDept = new Department("HR-отдел");
        hrDept.Add(hrLead);
        hrDept.Add(hr1);

        var mktDept = new Department("Отдел маркетинга");
        mktDept.Add(mktLead);
        mktDept.Add(mkt1);

        var company = new Department("ООО «ТехКор»");
        company.Add(ceo);
        company.Add(devDept);
        company.Add(hrDept);
        company.Add(mktDept);

        //Вывод полной структуры
        Console.WriteLine("══════════ СТРУКТУРА КОМПАНИИ ══════════\n");
        company.Display();

        //Итоговые показатели
        Console.WriteLine($"\n Общий бюджет (штатные):  {company.GetBudget():N0} ₽/мес");
        Console.WriteLine($" Общий штат (все):        {company.GetHeadcount()} чел.");

        //Поиск сотрудника
        Console.WriteLine("\n══════════ ПОИСК ══════════");
        var found = company.FindByName("Ольга Белова");
        if (found is Employee emp)
        {
            Console.WriteLine($"Найден: {emp.Name} | {emp.Position} | {emp.Salary:N0} ₽");

            // Повышение зарплаты
            emp.SetSalary(135_000m);
            Console.WriteLine($"Новый бюджет отдела разработки: {devDept.GetBudget():N0} ₽");
        }

        //Плоский список сотрудников
        Console.WriteLine("\n══════════ ВСЕ СОТРУДНИКИ КОМПАНИИ ══════════");
        var allStaff = new List<string>();
        company.ListAllEmployees(allStaff);
        allStaff.ForEach((p, i) => Console.WriteLine($"  {i + 1,2}. {p}"));

        //Удаление и проверка пересчёта
        Console.WriteLine("\n══════════ УДАЛЕНИЕ СОТРУДНИКА ══════════");
        devDept.Remove(dev1);
        Console.WriteLine($"Бюджет отдела разработки после удаления: {devDept.GetBudget():N0} ₽");
        Console.WriteLine($"Штат отдела разработки: {devDept.GetHeadcount()} чел.");
    }
}
