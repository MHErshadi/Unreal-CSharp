using System.Collections.Generic;
using System.Linq;

class Lexer
{
    readonly string text;
    readonly Position pos;
    char currentChar;
    string currentCharStr;
    char NextChar { get { return pos.idx + 1 < text.Length ? text[pos.idx + 1] : char.MinValue; } }
    public Lexer(string fn, string text)
    {
        this.text = text;
        pos = new Position(-1, 0, -1, fn, text);
        Advance();
    }
    void Advance()
    {
        pos.Advance(currentChar);
        currentChar = pos.idx < text.Length ? text[pos.idx] : char.MinValue;
        currentCharStr = currentChar.ToString();
    }
    public (List<Token>, IllegalCharError) MakeTokens(bool isFormattedString = false)
    {
        List<Token> tokens = new List<Token>();
        char endChar = !isFormattedString ? char.MinValue : '}';
        while (currentChar != endChar)
        {
            if (" \t".Contains(currentCharStr))
                Advance();
            else if (";\n".Contains(currentCharStr))
            {
                tokens.Add(new Token(TokenTypes.NEW_LINE, pos));
                Advance();
            }
            else if (currentChar == '.' && !LD.NUMBERS.Contains(NextChar.ToString()))
            {
                tokens.Add(new Token(TokenTypes.DOT, pos));
                Advance();
            }
            else if (LD.NUMBERS.Contains(currentCharStr))
                tokens.Add(MakeNumber());
            else if ("'\"".Contains(currentCharStr))
            {
                (Token token, IllegalCharError error) = MakeString(tokens);
                if (error != null)
                    return (null, error);
                tokens.Add(token);
            }
            else if (LD.LETTERS.Contains(currentCharStr))
                tokens.Add(MakeIdentifier());
            else if (currentChar == '@')
                SkipComments();
            else if (currentChar == '$')
            {
                tokens.Add(new Token(TokenTypes.DOLLAR, pos));
                Advance();
            }
            else if (currentChar == '+')
                tokens.Add(CreatePlus());
            else if (currentChar == '-')
                tokens.Add(CreateMinus());
            else if (currentChar == '*')
                tokens.Add(CreateDouble('=', TokenTypes.MUL, TokenTypes.MULE));
            else if (currentChar == '/')
                tokens.Add(CreateDiv());
            else if (currentChar == '%')
                tokens.Add(CreateDouble('=', TokenTypes.REM, TokenTypes.REME));
            else if (currentChar == '^')
                tokens.Add(CreatePow());
            else if (currentChar == '&')
                tokens.Add(CreateAnd());
            else if (currentChar == '|')
                tokens.Add(CreateOr());
            else if (currentChar == '~')
                tokens.Add(CreateXor());
            else if (currentChar == '!')
                tokens.Add(CreateNE());
            else if (currentChar == '=')
                tokens.Add(CreateDouble('=', TokenTypes.EQ, TokenTypes.EE));
            else if (currentChar == '<')
                tokens.Add(CreateLT());
            else if (currentChar == '>')
                tokens.Add(CreateGT());
            else if (currentChar == ':')
            {
                tokens.Add(new Token(TokenTypes.COL, pos));
                Advance();
            }
            else if (currentChar == ',')
            {
                tokens.Add(new Token(TokenTypes.COMMA, pos));
                Advance();
            }
            else if (currentChar == '(')
            {
                tokens.Add(new Token(TokenTypes.LPAREN, pos));
                Advance();
            }
            else if (currentChar == ')')
            {
                tokens.Add(new Token(TokenTypes.RPAREN, pos));
                Advance();
            }
            else if (currentChar == '[')
            {
                tokens.Add(new Token(TokenTypes.LSQUARE, pos));
                Advance();
            }
            else if (currentChar == ']')
            {
                tokens.Add(new Token(TokenTypes.RSQUARE, pos));
                Advance();
            }
            else if (currentChar == '{')
            {
                tokens.Add(new Token(TokenTypes.LCURLY, pos));
                Advance();
            }
            else if (currentChar == '}')
            {
                tokens.Add(new Token(TokenTypes.RCURLY, pos));
                Advance();
            }
            else if (currentChar == '\\')
            {
                tokens.Add(new Token(TokenTypes.BSLASH, pos));
                Advance();
            }
            else
            {
                Position posStart = pos.Copy;
                char chr = currentChar;
                Advance();
                return (null, new IllegalCharError(posStart, pos, $"'{chr}'"));
            }
        }
        tokens.Add(new Token(TokenTypes.EOF, pos));
        return (tokens, null);
    }
    Token MakeNumber()
    {
        string intSection = "";
        string decimalSection = "";
        bool isFloat = false;
        Position posStart = pos.Copy;
        while (LD.NUMBERS.Contains(currentCharStr))
        {
            if (currentChar == '.')
            {
                if (isFloat)
                    break;
                isFloat = true;
            }
            else
            {
                if (!isFloat)
                    intSection += currentChar;
                else
                    decimalSection += currentChar;
            }
            Advance();
        }
        return new Token(TokenTypes.NUMBER, posStart, pos, new NumberGen(intSection, decimalSection));
    }
    (Token, IllegalCharError) MakeString(List<Token> tokens)
    {
        string strValue = "";
        char endQoute = currentChar;
        Position posStart = pos.Copy;
        List<dynamic> formattedObjects = new List<dynamic>();
        bool isFormattedPrefix = StringPrefix(tokens, TokenTypes.IDENTIFIER, "f");
        bool backSlashPrefix = StringPrefix(tokens, TokenTypes.BSLASH);
        if (!isFormattedPrefix)
            isFormattedPrefix = StringPrefix(tokens, TokenTypes.IDENTIFIER, "f");
        bool escapeCharacter = false;
        Advance();
        IllegalCharError AddChar()
        {
            if (isFormattedPrefix && currentChar == '{')
            {
                formattedObjects.Add(strValue);
                strValue = "";
                Advance();
                (List<Token> formattedTokens, IllegalCharError error) = MakeTokens(true);
                if (error != null)
                    return error;
                formattedObjects.Add(formattedTokens);
            }
            else
                strValue += currentChar;
            return null;
        }
        while (currentChar != char.MinValue && (currentChar != endQoute || escapeCharacter))
        {
            if (escapeCharacter)
            {
                strValue += LD.escapeChars.ContainsKey(currentChar) ? LD.escapeChars[currentChar] : currentChar;
                escapeCharacter = false;
            }
            else
            {
                if (currentChar == '\\' && !backSlashPrefix)
                    escapeCharacter = true;
                else
                {
                    IllegalCharError error = AddChar();
                    if (error != null)
                        return (null, error);
                }
            }
            Advance();
        }
        Advance();
        if (strValue != "")
            formattedObjects.Add(strValue);
        return (new Token(TokenTypes.STRING, posStart, pos, new StringGen(formattedObjects)), null);
    }
    bool StringPrefix(List<Token> tokens, string type, string value = null)
    {
        int previousIndex = tokens.Count - 1;
        if (previousIndex >= 0 && tokens[previousIndex].Matches(type, value))
        {
            tokens.RemoveAt(previousIndex);
            return true;
        }
        return false;
    }
    Token MakeIdentifier()
    {
        string identifierId = "";
        Position posStart = pos.Copy;
        while (LD.IDENTIFIER_NAME.Contains(currentCharStr))
        {
            identifierId += currentChar;
            Advance();
        }
        string tokType = TokenTypes.IDENTIFIER;
        if (PD.keywords.Contains(identifierId))
            tokType = TokenTypes.KEYWORD;
        else if (ID.varTypes.ContainsValue(identifierId))
            tokType = TokenTypes.TYPE;
        return new Token(tokType, posStart, pos, identifierId);
    }
    void SkipComments()
    {
        Advance();
        if (currentChar == '*')
        {
            Advance();
            while (currentChar != char.MinValue && currentChar != '*' && NextChar != '@')
                Advance();
            Advance();
            Advance();
        }
        else
            while (currentChar != char.MinValue && currentChar != '\n')
                Advance();
    }
    Token CreateDouble(char diffrence, string tokTypeA, string tokTypeB)
    {
        string tokType = tokTypeA;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == diffrence)
        {
            tokType = tokTypeB;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreatePlus()
    {
        string tokType = TokenTypes.PLUS;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '+')
        {
            tokType = TokenTypes.INC;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.PLUSE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateMinus()
    {
        string tokType = TokenTypes.MINUS;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '-')
        {
            tokType = TokenTypes.DEC;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.MINUSE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    Token CreateDiv()
    {
        string tokType = TokenTypes.DIV;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '=')
        {
            tokType = TokenTypes.DIVE;
            Advance();
        }
        else if (currentChar == '/')
        {
            tokType = TokenTypes.QUOT;
            Advance();
            if (currentChar == '=')
            {
                tokType = TokenTypes.QUOTE;
                Advance();
            }
        }
        return new Token(tokType, posStart, pos);
    }
    Token CreatePow()
    {
        string tokType = TokenTypes.POW;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '=')
        {
            tokType = TokenTypes.POWE;
            Advance();
        }
        else if (currentChar == '^')
        {
            tokType = TokenTypes.RAD;
            Advance();
            if (currentChar == '=')
            {
                tokType = TokenTypes.RADE;
                Advance();
            }
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateAnd()
    {
        string tokType = TokenTypes.BIT_AND;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '&')
        {
            tokType = TokenTypes.AND;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.BIT_ANDE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateOr()
    {
        string tokType = TokenTypes.BIT_OR;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '|')
        {
            tokType = TokenTypes.OR;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.BIT_ORE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateXor()
    {
        string tokType = TokenTypes.BIT_XOR;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '~')
        {
            tokType = TokenTypes.XOR;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.BIT_XORE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateNE()
    {
        string tokType = TokenTypes.BIT_NOT;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '!')
        {
            tokType = TokenTypes.NOT;
            Advance();
        }
        else if (currentChar == '=')
        {
            tokType = TokenTypes.NE;
            Advance();
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateLT()
    {
        string tokType = TokenTypes.LT;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '=')
        {
            tokType = TokenTypes.LTE;
            Advance();
        }
        else if (currentChar == '-')
        {
            tokType = TokenTypes.ANC;
            Advance();
        }
        else if (currentChar == '<')
        {
            tokType = TokenTypes.LSHIFT;
            Advance();
            if (currentChar == '=')
            {
                tokType = TokenTypes.LSHIFTE;
                Advance();
            }
        }
        return new Token(tokType, posStart, pos);
    }
    public Token CreateGT()
    {
        string tokType = TokenTypes.GT;
        Position posStart = pos.Copy;
        Advance();
        if (currentChar == '=')
        {
            tokType = TokenTypes.GTE;
            Advance();
        }
        else if (currentChar == '>')
        {
            tokType = TokenTypes.RSHIFT;
            Advance();
            if (currentChar == '=')
            {
                tokType = TokenTypes.RSHIFTE;
                Advance();
            }
        }
        return new Token(tokType, posStart, pos);
    }
}