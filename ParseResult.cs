class ParseResult
{
    public dynamic node;
    public InvalidSyntaxError error;
    public bool lastRegisteredAdvanced;
    public int advanceCount;
    public int toReverseCount;
    public ParseResult()
    {
        node = null;
        error = null;
        lastRegisteredAdvanced = false;
        advanceCount = 0;
        toReverseCount = 0;
    }
    public void RegisterAdvancement()
    {
        lastRegisteredAdvanced = true;
        advanceCount++;
    }
    public dynamic Register(ParseResult res)
    {
        lastRegisteredAdvanced = res.advanceCount != 0;
        advanceCount += res.advanceCount;
        if (res.HasError)
            error = res.error;
        return res.node;
    }
    public dynamic TryRegister(ParseResult res)
    {
        if (res.HasError)
        {
            toReverseCount = res.advanceCount;
            return null;
        }
        return Register(res);
    }
    public ParseResult Success(dynamic node)
    {
        this.node = node;
        return this;
    }
    public ParseResult Failure(InvalidSyntaxError error)
    {
        if (this.error == null || !lastRegisteredAdvanced)
            this.error = error;
        return this;
    }
    public bool HasError { get { return error != null; } }
}