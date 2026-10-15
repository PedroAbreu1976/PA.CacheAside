namespace PA.CacheAside.Tests.TestData;

public class RepositoryForCaching : IRepositoryForCaching
{
    private List<TestAction> _actions = new List<TestAction>();
    private bool _isValid;
    private readonly Dictionary<int, string> _items = new Dictionary<int, string>
    {
        {1, "A"},
        {2, "B"},
        {3, "C"}
    };

    public IEnumerable<TestAction> Actions => _actions.AsEnumerable();

    public bool IsValid
    {
        get
        {
            AddAction(TestActionType.Get, nameof(IsValid));
            return _isValid;
        }
        set
        {
            AddAction(TestActionType.Set, nameof(IsValid), value);
            _isValid = value;
        }
    }
    public bool IsValidGetter
    {
        get
        {
            AddAction(TestActionType.Get, nameof(IsValidGetter));
            return _isValid;

        }
    }
    public bool IsValidSetter
    {
        set
        {
            AddAction(TestActionType.Set, nameof(IsValidSetter), value);
            _isValid = value;
        }
    }

    public Task Create(string item, CancellationToken ct)
    {
        AddAction(TestActionType.Set, nameof(Create), item);
        _items.Add(_items.Count + 1, item);
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetAll()
    {
        AddAction(TestActionType.Get, nameof(GetAll));
        return _items.Values;
    }

    public Task<IEnumerable<string>> GetAllAsync(CancellationToken ct)
    {
        AddAction(TestActionType.Get, nameof(GetAllAsync));
        return Task.FromResult<IEnumerable<string>>(_items.Values);
    }

    public Task<string> GetByIdAsync(int id, CancellationToken ct)
    {
        AddAction(TestActionType.Get, nameof(GetByIdAsync), id);
        return Task.FromResult(_items[id]);
    }

    public Task<string> GetFirstAsync(CancellationToken token)
    {
        AddAction(TestActionType.Get, nameof(GetFirstAsync));
        return Task.FromResult(_items.Values.First());
    }

    private void AddAction(TestActionType type, string method, params object[] args)
    {
        _actions.Add(new TestAction { Type = type, Method = method, Args = args });
    }

    public class TestAction
    {
        public string Method { get; set; }
        public object[] Args { get; set; }
        public TestActionType Type { get; set; }

        public override string ToString()
        {
            return $"{Type}: {Method}({string.Join(", ", Args)})";
        }
    }

    public enum TestActionType
    {
        Get,
        Set
    }
}
