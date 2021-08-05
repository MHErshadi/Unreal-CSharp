class Error
{
    public Position posStart;
    public Position posEnd;
    public string errorName;
    public string details;
    public override string ToString()
    {
        string error = $"\n{(details != "" ? $"{errorName}: {details}" : errorName)}\n";
        error += $"File {posStart.fn}, line {posStart.ln + 1}\n";
        return error;
    }
}

class IllegalCharError : Error
{
    public IllegalCharError(Position posStart, Position posEnd, string details)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        errorName = "Illegal Character Error";
        this.details = details;
    }
}

class InvalidSyntaxError : Error
{
    public InvalidSyntaxError(Position posStart, Position posEnd, string details = "")
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        errorName = "Invalid Syntax Error";
        this.details = details;
    }
}

class RuntimeError : Error
{
    readonly Context context;
    public readonly string type;
    public RuntimeError(Position posStart, Position posEnd, string details, Context context, string type)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        errorName = "Runtime Error";
        this.details = details;
        this.context = context;
        this.type = type;
    }
    public override string ToString()
    {
        string error = GenerateTraceback();
        error += $"\n{errorName}: {details}\n";
        error += $"Error type: {type}\n";
        return error;
    }
    string GenerateTraceback()
    {
        string traceback = "";
        Position pos = posStart;
        Context ctx = context;
        while (ctx != null)
        {
            traceback = $"   File {pos.fn}, line {pos.ln + 1}, in {ctx.name}\n{traceback}";
            pos = ctx.parentEntryPos;
            ctx = ctx.parent;
        }
        return $"\nTraceback (most recent call last):\n{traceback}";
    }
}