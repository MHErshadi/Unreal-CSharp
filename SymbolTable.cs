using System.Collections.Generic;

class SymbolTable
{
    public Dictionary<string, Variable> symbols;
    public Dictionary<string, Variable> privateSymbols;
    public List<string> constVars;
    public List<string> staticVars;
    public SymbolTable parent;
    public SymbolTable(SymbolTable parent = null)
    {
        symbols = new Dictionary<string, Variable>();
        privateSymbols = new Dictionary<string, Variable>();
        constVars = new List<string>();
        staticVars = new List<string>();
        this.parent = parent;
    }
    public Variable GetOrGetPrivate(string name)
    {
        Variable value = Get(name);
        if (value == null)
            value = GetPrivate(name);
        return value;
    }
    public Variable Get(string name)
    {
        if (!symbols.TryGetValue(name, out Variable value) && parent != null)
            return parent.Get(name);
        if (value?.value?.GetType() == typeof(string))
            return Get(value.value);
        return value;
    }
    public Variable GetPrivate(string name)
    {
        if (!privateSymbols.TryGetValue(name, out Variable value) && parent != null)
            return parent.GetPrivate(name);
        if (value?.value.GetType() == typeof(string))
            return GetPrivate(value.value);
        return value;
    }
    public bool GetConst(string name)
    {
        bool contains = constVars.Contains(name);
        if (!contains && parent != null)
            return parent.GetConst(name);
        return contains;
    }
    public bool GetStatic(string name)
    {
        bool contains = staticVars.Contains(name);
        if (!contains && parent != null)
            return parent.GetStatic(name);
        return contains;
    }
    public void Set(string name, Variable value)
    {
        symbols[name] = value;
    }
    public void SetPrivate(string name, Variable value)
    {
        privateSymbols[name] = value;
    }
    public void Remove(string name)
    {
        symbols.Remove(name);
    }
    public void RemovePrivate(string name)
    {
        symbols.Remove(name);
    }
}

class Variable
{
    public readonly dynamic value;
    public readonly string type;
    public Variable(dynamic value, string type = null)
    {
        this.value = value;
        this.type = type != "none_" ? type : null;
    }
}