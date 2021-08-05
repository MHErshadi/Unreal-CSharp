class Position
{
    public int idx;
    public int ln;
    public int col;
    public readonly string fn;
    public readonly string ftxt;
    public Position(int idx, int ln, int col, string fn, string ftxt)
    {
        this.idx = idx;
        this.ln = ln;
        this.col = col;
        this.fn = fn;
        this.ftxt = ftxt;
    }
    public void Advance(char currentChar = char.MinValue)
    {
        idx++;
        col++;
        if (currentChar == '\n')
        {
            ln++;
            col = 0;
        }
    }
    public Position Copy { get { return new Position(idx, ln, col, fn, ftxt); } }
}