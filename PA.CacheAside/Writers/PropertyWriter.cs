namespace PA.CacheAside.Writers;

internal class PropertyWriter
{
    public string Name { get; set; }
    public string TypeName { get; set; }
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public string GetterBody { get; set; }
    public string SetterBody { get; set; }

    public override string ToString()
    {
        var getterText = GetterBody is null ? "get;" : $"get {{ {GetterBody}}}";
        var setterText = SetterBody is null ? "set;" : $"set {{ {SetterBody} }}";
        return $@"
    public {TypeName} {Name} 
    {{ 
        {(HasGetter ? getterText : "")} 
        {(HasSetter ? setterText : "")} 
    }}";
    }
}
