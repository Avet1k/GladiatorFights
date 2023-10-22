namespace GladiatorFights;
class Program
{
    static void Main(string[] args)
    {
        ConsoleLogger consoleLogger = new();
        Game newGame = new Game(consoleLogger);
        
        List<int> a = new();
        
        newGame.PickWarriors();
        newGame.Fight();
    }
}

interface ILogger
{
    void ShowTakenDamage(string name, int dealtDamage);

    void ShowStats(string name, int health, int damage, int armor);

    void ShowUsingPaladinAbility(string name, int healValue);

    void ShowUsingDuelistAbility(string name);

    void ShowUsingMageAbility(string name, int manaPotionValue);

    void ShowUsingVampireAbility(string name, int healthToRestore);

    void ShowMageStats(string name, int health, int damage, int armor, int mana);
}

class ConsoleLogger : ILogger
{
    public void ShowTakenDamage(string name, int dealtDamage)
    {
        Console.WriteLine($"Бойцу {name} нанесли {dealtDamage} урона!");
    }
    
    public void ShowStats(string name, int health, int damage, int armor)
    {
        Console.WriteLine($"{name}\nОЗ: {health}   УР: {damage}   БР: {armor}");
    }

    public void ShowUsingPaladinAbility(string name, int healValue)
    {
        Console.WriteLine($"Боец {name} вылечил себе {healValue} ОЗ");
    }
    
    public void ShowUsingDuelistAbility(string name)
    {
        Console.WriteLine($"Боец {name} увернулся от удара!");
    }

    public void ShowUsingMageAbility(string name, int manaPotionValue)
    {
        Console.WriteLine($"Боец {name} восстановил {manaPotionValue} маны.");
    }

    public void ShowUsingVampireAbility(string name, int healthToRestore)
    {
        Console.WriteLine($"Боец {name} восстановил {healthToRestore} ОЗ");
    }

    public void ShowMageStats(string name, int health, int damage, int armor, int mana)
    {
        Console.WriteLine($"{name}\nОЗ: {health}   УР: {damage}   БР: {armor}   ОМ: {mana}");
    }
}

abstract class Warrior
{
    protected ILogger Logger;
    
    protected Warrior(string name, int damage, int health, int armor, ILogger logger)
    {
        Name = name;
        Health = health;
        Damage = damage;
        Armor = armor;
        Logger = logger;
    }

    public string Name { get; }
    public int Health { get; protected set; }
    protected int Damage { get; }
    protected int Armor { get; }

    public virtual void TakeDamage(int damage)
    {
        float converter = 100.0f;
        int dealtDamage = Convert.ToInt32(damage * (converter / (converter + Armor)));
        
        Health -= dealtDamage;
        
        Logger.ShowTakenDamage(Name, dealtDamage);
    }

    public virtual void Attack(Warrior enemy)
    {
        enemy.TakeDamage(Damage);
    }

    public virtual void ShowStats()
    {
        Logger.ShowStats(Name, Health, Damage, Armor);
    }

    public abstract Warrior Create();
}

class Paladin : Warrior
{
    private float _healCastPercent = 10f;
    private int _healCastRestoreValue;
    private int _cooldown = 5;
    private int _roundCount;

    public Paladin(ILogger consoleLogger) : base(name: "Ашот Железный",
        damage: 50, health: 500, armor: 120, consoleLogger)
    {
        float percetageConverter = 100f;
        
        _healCastRestoreValue = Convert.ToInt32(_healCastPercent * Health / percetageConverter);
    }

    public override void Attack(Warrior enemy)
    {
        _roundCount++;
        
        if (_roundCount % _cooldown != 0)
            base.Attack(enemy);
        else
            Ability();
    }

    public override Warrior Create()
    {
        return new Paladin(Logger);
    }

    private void Ability()
    {
        Health += _healCastRestoreValue;
        
        Logger.ShowUsingPaladinAbility(Name, _healCastRestoreValue);
    }
}

class Archer : Warrior
{
    private int _critChance = 3;
    private int _critMultiplier = 3;
    private Random _random = new();
    
    public Archer(ILogger consoleLogger) : base(name: "Айцемник", damage: 60, health: 300, armor: 90,
        consoleLogger)
    {
    }
    
    public override void Attack(Warrior enemy)
    {
        if (_random.Next(_critChance) == _critChance - 1)
            enemy.TakeDamage(Damage * _critMultiplier);
        else
            base.Attack(enemy);
    }

    public override Warrior Create()
    {
        return new Archer(Logger);
    }
}

class Duelist : Warrior
{
    private int _dodgeChance = 3;
    private Random _random = new();
    
    public Duelist(ILogger consoleLogger) : base(name: "Борис Бритва", damage: 55, health: 400, armor: 80,
        consoleLogger)
    {
    }

    public override void TakeDamage(int damage)
    {
        if (_random.Next(_dodgeChance) != _dodgeChance - 1)
            base.TakeDamage(damage);
        else
            Logger.ShowUsingDuelistAbility(Name);
    }

    public override Warrior Create()
    {
        return new Duelist(Logger);
    }
}

class Mage : Warrior
{
    private int _manaForCast = 25;
    private int _manaPotionValue = 60;
    private int _mana;
    
    public Mage(ILogger consoleLogger) : base(name: "Анаит Просветлённая", damage: 100, health: 400, armor: 95,
        consoleLogger)
    {
        _mana = 100;
    }

    public override void Attack(Warrior enemy)
    {
        if (_mana < _manaForCast)
        {
            _mana += _manaPotionValue;
            Logger.ShowUsingMageAbility(Name, _manaPotionValue);
        }

        _mana -= _manaForCast;
        base.Attack(enemy);
    }

    public override Warrior Create()
    {
        return new Mage(Logger);
    }

    public override void ShowStats()
    {
        Logger.ShowMageStats(Name, Health, Damage, Armor, _mana);
    }
}

class Vampire : Warrior
{
    private int _maxHealth;
    private int _healthRestorePercentage = 30;

    public Vampire(ILogger consoleLogger) : base(name: "Аракская Ночница", damage: 60, health: 400, armor: 50,
        consoleLogger)
    {
        _maxHealth = Health;
    }

    public override void Attack(Warrior enemy)
    {
        int healthBefore = enemy.Health;
        
        base.Attack(enemy);
        
        RestoreHealth(healthBefore - enemy.Health);
    }

    public override Warrior Create()
    {
        return new Vampire(Logger);
    }

    private void RestoreHealth(int dealtDamage)
    {
        int percentConverter = 100;
        int healthToRestore = _healthRestorePercentage * dealtDamage / percentConverter;
        
        Health += healthToRestore;

        if (Health > _maxHealth)
            Health = _maxHealth;

        Logger.ShowUsingVampireAbility(Name, healthToRestore);
    }
}

class Game
{
    private List<Warrior> _warriorsExamples;
    private Warrior _firstWarrior;
    private Warrior _secondWarrior;
    
    public Game(ILogger consoleLogger)
    {
        _warriorsExamples = new List<Warrior>()
        {
            new Paladin(consoleLogger),
            new Archer(consoleLogger),
            new Duelist(consoleLogger),
            new Mage(consoleLogger),
            new Vampire(consoleLogger)
        };
    }
    
    public void PickWarriors()
    {
        string firstNumberName = "первого";
        string secondNumberName = "второго";
        int warriorNumber = 1;

        Console.Write("Добро пожаловать на арену! Выберите двух бойцов.\n\n");
        
        foreach (var warrior in _warriorsExamples)
        {
            Console.Write(warriorNumber++ + ". ");
            warrior.ShowStats();
            Console.WriteLine();
        }

        _firstWarrior = HandleWarriorInput(firstNumberName);
        
        Console.WriteLine();

        _secondWarrior = HandleWarriorInput(secondNumberName);

        Console.WriteLine();
    }

    private Warrior HandleWarriorInput(string numberLitteral)
    {
        char userInput;
        int warriorIndex = 0;
        bool isWarriorPicked = false;

        while (isWarriorPicked == false)
        {
            Console.Write($"Введите цифру, чтобы выбрать {numberLitteral} война: ");
            userInput = Console.ReadKey(true).KeyChar;

            if (int.TryParse(userInput.ToString(), out warriorIndex)
                && warriorIndex <= _warriorsExamples.Count
                && warriorIndex > 0)
            {
                Console.Write(_warriorsExamples[warriorIndex - 1].Name);
                isWarriorPicked = true;
                Thread.Sleep(500);
            }
            
            Console.WriteLine();
        }
       
        return _warriorsExamples[warriorIndex - 1].Create();
    }
    
    public void Fight()
    {
        string winnerName;
        string loserName;
        
        Console.Clear();
        Console.Write($"{_firstWarrior.Name} против {_secondWarrior.Name}\n\n" +
                      "Нажмите любую кнопку для начала сражения...");
        Console.ReadKey();
        Console.WriteLine();
        
        while (_firstWarrior.Health > 0 && _secondWarrior.Health > 0)
        {
            Console.WriteLine();

            _secondWarrior.Attack(_firstWarrior);
            _firstWarrior.Attack(_secondWarrior);
            
            Console.WriteLine();
            _firstWarrior.ShowStats();
            _secondWarrior.ShowStats();
            
            Console.WriteLine();
            Console.WriteLine(new string('#', 25));
            Console.ReadKey();
        }

        if (_firstWarrior.Health <= 0 && _secondWarrior.Health <= 0)
        {
            Console.WriteLine("\n\nОба война пали. Победителя нет...");
            return;
        }

        if (_firstWarrior.Health <= 0)
        {
            winnerName = _secondWarrior.Name;
            loserName = _firstWarrior.Name;
        }
        else
        {
            winnerName = _firstWarrior.Name;
            loserName = _secondWarrior.Name;
        }
         
        Console.WriteLine($"\n\nБоец {loserName} обагрил арену своей кровью. Победил боец {winnerName}");
    }
}
