class Context
{
    public readonly string name;
    public readonly Context parent;
    public readonly Position parentEntryPos;
    public readonly SymbolTable symbolTable;
    public Context(string name, SymbolTable symbolTable, Context parent = null, Position parentEntryPos = null)
    {
        this.name = name;
        this.parent = parent;
        this.parentEntryPos = parentEntryPos;
        this.symbolTable = symbolTable;
    }
}