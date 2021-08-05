class Token
{
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly string type;
    public readonly dynamic value;
    public Token(string type, Position posStart, Position posEnd = null, dynamic value = null)
    {
        this.posStart = posStart.Copy;
        if (posEnd == null)
        {
            this.posEnd = this.posStart.Copy;
            this.posEnd.Advance();
        }
        else
            this.posEnd = posEnd.Copy;
        this.type = type;
        this.value = value;
    }
    public bool Matches(string type, string value)
    {
        return this.type == type && this.value == value;
    }
}