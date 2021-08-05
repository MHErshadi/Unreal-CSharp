using System;
using System.Collections.Generic;
using System.Linq;

class Parser
{
    readonly List<Token> tokens;
    int tokIdx;
    Token CurrentTok { get { return tokIdx < tokens.Count ? tokens[tokIdx] : null; } }
    Token NextTok { get { return tokIdx + 1 < tokens.Count ? tokens[tokIdx + 1] : null; } }
    static readonly string[] assignmentToks = new string[17]
    {
        TokenTypes.EQ, TokenTypes.ANC, TokenTypes.PLUSE, TokenTypes.MINUSE, TokenTypes.MULE, TokenTypes.DIVE,
        TokenTypes.REME, TokenTypes.QUOTE, TokenTypes.POWE, TokenTypes.RADE, TokenTypes.BIT_ANDE, TokenTypes.BIT_ORE,
        TokenTypes.BIT_XORE, TokenTypes.LSHIFTE, TokenTypes.RSHIFTE, TokenTypes.INC, TokenTypes.DEC
    };
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        tokIdx = 0;
    }
    void Advance(ParseResult res)
    {
        res.RegisterAdvancement();
        tokIdx++;
    }
    void Reverse(int amount = 1)
    {
        tokIdx -= amount;
        if (tokIdx < 0)
            tokIdx = 0;
    }
    void AdvanceNewLine(ParseResult res)
    {
        while (CurrentTok.type == TokenTypes.NEW_LINE)
            Advance(res);
    }
    public ParseResult Parse()
    {
        ParseResult res = Statements();
        if (!res.HasError && CurrentTok.type != TokenTypes.EOF)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "EOF Error"));
        return res;
    }
    public ParseResult Statements()
    {
        ParseResult res = new ParseResult();
        AdvanceNewLine(res);
        Position posStart = CurrentTok.posStart.Copy;
        dynamic firstStatement = res.Register(Statement());
        if (res.HasError)
            return res;
        List<dynamic> statements = new List<dynamic> { firstStatement };
        while (true)
        {
            if (CurrentTok.type != TokenTypes.NEW_LINE)
                break;
            AdvanceNewLine(res);
            dynamic statement = res.TryRegister(Statement());
            if (statement == null)
            {
                Reverse(res.toReverseCount);
                break;
            }
            statements.Add(statement);
        }
        if (statements.Count == 1)
            return res.Success(firstStatement);
        return res.Success(new ListNode(statements, posStart, CurrentTok.posStart.Copy));
    }
    public ParseResult Statement()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        ParseResult FindReason()
        {
            Advance(res);
            dynamic reasonNode = res.Register(GeneralList());
            if (res.HasError)
                return res;
            return res.Success(reasonNode);
        }
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "return"))
        {
            Advance(res);
            dynamic returnValue = res.TryRegister(GeneralList());
            if (returnValue == null)
                Reverse(res.toReverseCount);
            return res.Success(new ReturnNode(returnValue, posStart, CurrentTok.posStart.Copy));
        }
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "continue"))
        {
            Advance(res);
            dynamic reasonToContinue = null;
            if (CurrentTok.Matches(TokenTypes.KEYWORD, "if"))
            {
                reasonToContinue = res.Register(FindReason());
                if (res.HasError)
                    return res;
            }
            return res.Success(new ContinueNode(posStart, CurrentTok.posStart.Copy, reasonToContinue));
        }
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "break"))
        {
            Advance(res);
            dynamic reasonToBreak = null;
            if (CurrentTok.Matches(TokenTypes.KEYWORD, "if"))
            {
                reasonToBreak = res.Register(FindReason());
                if (res.HasError)
                    return res;
            }
            return res.Success(new BreakNode(posStart, CurrentTok.posStart.Copy, reasonToBreak));
        }
        return GeneralList();
    }
    public ParseResult GeneralList()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        bool generalList = false;
        if (CurrentTok.type == TokenTypes.COMMA)
        {
            Advance(res);
            generalList = true;
        }
        dynamic firstElement = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        List<dynamic> elements = new List<dynamic> { firstElement };
        while (CurrentTok.type == TokenTypes.COMMA)
        {
            Advance(res);
            elements.Add(res.Register(TypeExpr()));
            if (res.HasError)
                return res;
        }
        if (elements.Count == 1 && !generalList)
            return res.Success(firstElement);
        return res.Success(new TupleNode(elements, posStart, CurrentTok.posStart.Copy));
    }
    public ParseResult TypeExpr()
    {
        ParseResult res = new ParseResult();
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "var"))
        {
            VarAssignNode varExpr = res.Register(VarExpr());
            if (res.HasError)
                return res;
            return res.Success(varExpr);
        }
        return BinOp(ContainExpr, toks: new (string, string)[2] { (TokenTypes.KEYWORD, "is"), (TokenTypes.KEYWORD, "are") });
    }
    public ParseResult ContainExpr()
    {
        return BinOp(BitwiseExpr, toks: new (string, string)[1] { (TokenTypes.KEYWORD, "in") });
    }
    public ParseResult BitwiseExpr()
    {
        ParseResult res = new ParseResult();
        if (CurrentTok.type == TokenTypes.BIT_NOT)
        {
            Token op = CurrentTok;
            Advance(res);
            dynamic unaryNode = res.Register(BitwiseExpr());
            if (res.HasError)
                return res;
            return res.Success(new UnaryOpNode(op, unaryNode));
        }
        return BinOp(ConsensusExpr, new string[5] { TokenTypes.BIT_AND, TokenTypes.BIT_OR, TokenTypes.BIT_XOR, TokenTypes.LSHIFT, TokenTypes.RSHIFT });
    }
    public ParseResult ConsensusExpr()
    {
        return BinOp(LogicalExpr,
           new string[3] { TokenTypes.AND, TokenTypes.OR, TokenTypes.XOR },
           new (string, string)[3] { (TokenTypes.KEYWORD, "and"), (TokenTypes.KEYWORD, "or"), (TokenTypes.KEYWORD, "xor") }
           );
    }
    public ParseResult LogicalExpr()
    {
        ParseResult res = new ParseResult();
        if (CurrentTok.type == TokenTypes.NOT || CurrentTok.Matches(TokenTypes.KEYWORD, "not"))
        {
            Token op = CurrentTok;
            Advance(res);
            dynamic unaryNode = res.Register(LogicalExpr());
            if (res.HasError)
                return res;
            return res.Success(new UnaryOpNode(op, unaryNode));
        }
        return BinOp(CompExpr, new string[2] { TokenTypes.EE, TokenTypes.NE });
    }
    public ParseResult CompExpr()
    {
        return BinOp(ArithExpr, new string[4] { TokenTypes.LT, TokenTypes.GT, TokenTypes.LTE, TokenTypes.GTE });
    }
    public ParseResult ArithExpr()
    {
        return BinOp(Term, new string[2] { TokenTypes.PLUS, TokenTypes.MINUS });
    }
    public ParseResult Term()
    {
        return BinOp(Factor, new string[4] { TokenTypes.MUL, TokenTypes.DIV, TokenTypes.REM, TokenTypes.QUOT });
    }
    public ParseResult Factor()
    {
        ParseResult res = new ParseResult();
        if (new string[2] { TokenTypes.PLUS, TokenTypes.MINUS }.Contains(CurrentTok.type))
        {
            Token op = CurrentTok;
            Advance(res);
            dynamic unaryNode = res.Register(Factor());
            if (res.HasError)
                return res;
            return res.Success(new UnaryOpNode(op, unaryNode));
        }
        return PowerExpr();
    }
    public ParseResult PowerExpr()
    {
        return BinOp(ClassifiedIndexExpr, new string[2] { TokenTypes.POW, TokenTypes.RAD }, funcB: Factor);
    }
    public ParseResult ClassifiedIndexExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        dynamic expr = res.Register(FuncCallExpr());
        if (res.HasError)
            return res;
        if (CurrentTok.type == TokenTypes.LSQUARE)
        {
            Advance(res);
            dynamic clssifiedIndex = res.Register(GeneralList());
            if (res.HasError)
                return res;
            if (CurrentTok.type != TokenTypes.RSQUARE)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ']'"));
            Advance(res);
            IndexNode indexNode = new IndexNode(expr, clssifiedIndex, posStart, CurrentTok.posStart.Copy);
            if (assignmentToks.Contains(CurrentTok.type))
            {
                Token assignType = CurrentTok;
                Advance(res);
                dynamic newValue = null;
                if (!new string[2] { TokenTypes.INC, TokenTypes.DEC }.Contains(CurrentTok.type))
                {
                    newValue = res.Register(GeneralList());
                    if (res.HasError)
                        return res;
                }
                return res.Success(new BinOpNode(indexNode, assignType, newValue));
            }
            return res.Success(indexNode);
        }
        return res.Success(expr);
    }
    public ParseResult FuncCallExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        dynamic expr = res.Register(AccessExpr());
        if (res.HasError)
            return res;
        if (CurrentTok.type == TokenTypes.LPAREN)
        {
            Advance(res);
            List<Token> argNames = new List<Token>();
            List<dynamic> argValues = new List<dynamic>();
            if (CurrentTok.type == TokenTypes.RPAREN)
                Advance(res);
            else
            {
                while (true)
                {
                    if (CurrentTok.type == TokenTypes.IDENTIFIER && NextTok.type == TokenTypes.COL)
                    {
                        argNames.Add(CurrentTok);
                        Advance(res);
                        Advance(res);
                    }
                    else
                        argNames.Add(null);
                    argValues.Add(res.Register(TypeExpr()));
                    if (res.HasError)
                        return res;
                    if (CurrentTok.type != TokenTypes.COMMA)
                        break;
                    Advance(res);
                }
                if (CurrentTok.type != TokenTypes.RPAREN)
                    return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ')'"));
                Advance(res);
            }
            return res.Success(new FuncCallNode(expr, argNames, argValues, posStart, CurrentTok.posStart.Copy));
        }
        return res.Success(expr);
    }
    public ParseResult AccessExpr()
    {
        return Atom();
        //return BinOp(Atom, new string[1] { TokenTypes.DOT });
    }
    public ParseResult Atom()
    {
        ParseResult res = new ParseResult();
        Token tok = CurrentTok;
        if (tok.Matches(TokenTypes.KEYWORD, "object"))
        {
            Advance(res);
            return res.Success(new ObjectNode(tok));
        }
        if (tok.Matches(TokenTypes.KEYWORD, "none"))
        {
            Advance(res);
            return res.Success(new NoneNode(tok));
        }
        if (tok.type == TokenTypes.TYPE)
        {
            Advance(res);
            return res.Success(new UTypeNode(tok));
        }
        if (tok.type == TokenTypes.NUMBER)
        {
            Advance(res);
            return res.Success(new NumberNode(tok));
        }
        if (tok.Matches(TokenTypes.KEYWORD, "true") || tok.Matches(TokenTypes.KEYWORD, "false"))
        {
            Advance(res);
            return res.Success(new BoolNode(tok));
        }
        if (tok.type == TokenTypes.STRING)
        {
            Advance(res);
            List<dynamic> formattedObjects = new List<dynamic>();
            foreach (dynamic f in tok.value.formattedObjects)
            {
                if (f.GetType() == typeof(string))
                    formattedObjects.Add(f);
                else
                {
                    formattedObjects.Add(res.Register(new Parser(f).Parse()));
                    if (res.HasError)
                        return res;
                }
            }
            tok.value.formattedObjects = formattedObjects;
            return res.Success(new StringNode(tok));
        }
        if (tok.type == TokenTypes.LSQUARE)
        {
            ListNode listExpr = res.Register(ListExpr());
            if (res.HasError)
                return res;
            return res.Success(listExpr);
        }
        if (tok.type == TokenTypes.LCURLY)
        {
            dynamic dictionaryExpr = res.Register(DictionaryExpr());
            if (res.HasError)
                return res;
            return res.Success(dictionaryExpr);
        }
        if (tok.type == TokenTypes.DOLLAR)
        {
            DollarMethodNode dollarMethodExpr = res.Register(DollarMethodExpr());
            if (res.HasError)
                return res;
            return res.Success(dollarMethodExpr);
        }
        if (tok.type == TokenTypes.IDENTIFIER)
        {
            Position posStart = CurrentTok.posStart.Copy;
            Advance(res);
            if (assignmentToks.Contains(CurrentTok.type))
            {
                Token assignType = CurrentTok;
                Advance(res);
                dynamic value = null;
                if (!new string[2] { TokenTypes.INC, TokenTypes.DEC }.Contains(assignType.type))
                {
                    value = res.Register(GeneralList());
                    if (res.HasError)
                        return res;
                }
                return res.Success(new VarAssignNode(tok, assignType, value, posStart, CurrentTok.posStart.Copy));
            }
            return res.Success(new VarAccessNode(tok));
        }
        if (tok.type == TokenTypes.LPAREN)
        {
            Position posStart = CurrentTok.posStart.Copy;
            Advance(res);
            dynamic expr = res.Register(Statements());
            if (res.HasError)
                return res;
            if (CurrentTok.type != TokenTypes.RPAREN)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ')'"));
            Advance(res);
            return res.Success(new ParenNode(expr, posStart, CurrentTok.posStart.Copy));
        }
        if (tok.Matches(TokenTypes.KEYWORD, "if"))
        {
            IfNode ifExpr = res.Register(IfExpr());
            if (res.HasError)
                return res;
            return res.Success(ifExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "switch"))
        {
            SwitchNode switchExpr = res.Register(SwitchExpr());
            if (res.HasError)
                return res;
            return res.Success(switchExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "for"))
        {
            dynamic forExpr = res.Register(ForExpr());
            if (res.HasError)
                return res;
            return res.Success(forExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "loop"))
        {
            LoopNode loopExpr = res.Register(LoopExpr());
            if (res.HasError)
                return res;
            return res.Success(loopExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "while"))
        {
            WhileNode whileExpr = res.Register(WhileExpr());
            if (res.HasError)
                return res;
            return res.Success(whileExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "try"))
        {
            TryNode tryExpr = res.Register(TryExpr());
            if (res.HasError)
                return res;
            return res.Success(tryExpr);
        }
        if (tok.Matches(TokenTypes.KEYWORD, "func"))
        {
            FuncDefNode funcDef = res.Register(FuncDef());
            if (res.HasError)
                return res;
            return res.Success(funcDef);
        }
        return res.Failure(new InvalidSyntaxError(tok.posStart, tok.posEnd));
    }
    ParseResult BinOp(Func<ParseResult> funcA, string[] ops = null, (string, string)[] toks = null, Func<ParseResult> funcB = null)
    {
        if (funcB == null)
            funcB = funcA;
        ParseResult res = new ParseResult();
        dynamic leftOperand = res.Register(funcA());
        if (res.HasError)
            return res;
        while ((ops != null && ops.Contains(CurrentTok.type)) || (toks != null && toks.Contains((CurrentTok.type, $"{CurrentTok.value}"))))
        {
            Token op = CurrentTok;
            Advance(res);
            dynamic rightOperand = res.Register(funcB());
            if (res.HasError)
                return res;
            leftOperand = new BinOpNode(leftOperand, op, rightOperand);
        }
        return res.Success(leftOperand);
    }
    ParseResult VarExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        (bool isGlobal, bool isPublic, bool isConst, bool isStatic) = VarProps(res);
        Token type = null;
        if (CurrentTok.type == TokenTypes.TYPE)
        {
            type = CurrentTok;
            Advance(res);
        }
        if (CurrentTok.type != TokenTypes.IDENTIFIER)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected identifier"));
        Token varName = CurrentTok;
        Advance(res);
        Token assignType = null;
        dynamic value = null;
        if (assignmentToks.Contains(CurrentTok.type))
        {
            assignType = CurrentTok;
            Advance(res);
            if (!new string[2] { TokenTypes.INC, TokenTypes.DEC }.Contains(CurrentTok.type))
            {
                value = res.Register(GeneralList());
                if (res.HasError)
                    return res;
            }
        }
        return res.Success(new VarAssignNode(varName, assignType, value, posStart, CurrentTok.posStart, type, isGlobal, isPublic, isConst, isStatic));
    }
    ParseResult ListExpr()
    {
        ParseResult res = new ParseResult();
        List<dynamic> elements = new List<dynamic>();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        AdvanceNewLine(res);
        if (CurrentTok.type != TokenTypes.RSQUARE)
        {
            while (true)
            {
                elements.Add(res.Register(TypeExpr()));
                if (res.HasError)
                    return res;
                AdvanceNewLine(res);
                if (CurrentTok.type != TokenTypes.COMMA)
                    break;
                Advance(res);
                AdvanceNewLine(res);
            }
            if (CurrentTok.type != TokenTypes.RSQUARE)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ']'"));
        }
        Advance(res);
        return res.Success(new ListNode(elements, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult DictionaryExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        AdvanceNewLine(res);
        if (CurrentTok.type == TokenTypes.RCURLY)
        {
            Advance(res);
            return res.Success(new SetNode(new List<dynamic>(), posStart, CurrentTok.posStart.Copy));
        }
        dynamic firstKey = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        AdvanceNewLine(res);
        if (new string[2] { TokenTypes.COMMA, TokenTypes.RCURLY }.Contains(CurrentTok.type))
        {
            SetNode setExpr = res.Register(SetExpr(firstKey, posStart));
            if (res.HasError)
                return res;
            return res.Success(setExpr);
        }
        if (CurrentTok.type != TokenTypes.COL)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ':'"));
        Advance(res);
        AdvanceNewLine(res);
        dynamic firstValue = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        AdvanceNewLine(res);
        List<dynamic> keys = new List<dynamic> { firstKey };
        List<dynamic> values = new List<dynamic> { firstValue };
        while (CurrentTok.type == TokenTypes.COMMA)
        {
            Advance(res);
            AdvanceNewLine(res);
            keys.Add(res.Register(TypeExpr()));
            if (res.HasError)
                return res;
            AdvanceNewLine(res);
            if (CurrentTok.type != TokenTypes.COL)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ':'"));
            Advance(res);
            AdvanceNewLine(res);
            values.Add(res.Register(TypeExpr()));
            if (res.HasError)
                return res;
            AdvanceNewLine(res);
        }
        if (CurrentTok.type != TokenTypes.RCURLY)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '}'"));
        Advance(res);
        return res.Success(new DictionaryNode(keys, values, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult SetExpr(dynamic firstValue, Position posStart)
    {
        ParseResult res = new ParseResult();
        List<dynamic> elements = new List<dynamic> { firstValue };
        while (CurrentTok.type == TokenTypes.COMMA)
        {
            Advance(res);
            AdvanceNewLine(res);
            elements.Add(res.Register(TypeExpr()));
            if (res.HasError)
                return res;
            AdvanceNewLine(res);
        }
        if (CurrentTok.type != TokenTypes.RCURLY)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '}'"));
        Advance(res);
        return res.Success(new SetNode(elements, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult DollarMethodExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        if (CurrentTok.type != TokenTypes.IDENTIFIER)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected identifier"));
        Token methodName = CurrentTok;
        Advance(res);
        List<dynamic> args = new List<dynamic>();
        if (CurrentTok.type == TokenTypes.COL)
        {
            Advance(res);
            while (true)
            {
                args.Add(res.Register(TypeExpr()));
                if (res.HasError)
                    return res;
                if (CurrentTok.type != TokenTypes.COMMA)
                    break;
                Advance(res);
            }
        }
        return res.Success(new DollarMethodNode(methodName, args, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult IfExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        dynamic allCases = res.Register(IfExprCases());
        if (res.HasError)
            return res;
        return res.Success(new IfNode(allCases.Item1, allCases.Item2, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult ElifElseExpr()
    {
        ParseResult res = new ParseResult();
        List<(dynamic, dynamic, bool)> cases = new List<(dynamic, dynamic, bool)>();
        (dynamic, bool) elseCase = (null, false);
        bool newLinesAdvanced = CurrentTok.type == TokenTypes.NEW_LINE;
        AdvanceNewLine(res);
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "elif"))
        {
            dynamic allCases = res.Register(IfExprCases());
            if (res.HasError)
                return res;
            cases = allCases.Item1;
            elseCase = allCases.Item2;
        }
        else if (CurrentTok.Matches(TokenTypes.KEYWORD, "else"))
        {
            Advance(res);
            dynamic elseExpr = res.Register(ElseExpr());
            if (res.HasError)
                return res;
            elseCase = elseExpr;
        }
        else if (newLinesAdvanced)
            Reverse();
        return res.Success((cases, elseCase));
    }
    ParseResult ElseExpr()
    {
        ParseResult res = new ParseResult();
        dynamic elseCase = res.Register(StatementBody());
        if (res.HasError)
            return res;
        return res.Success(elseCase);
    }
    ParseResult IfExprCases()
    {
        ParseResult res = new ParseResult();
        List<(dynamic, dynamic, bool)> cases = new List<(dynamic, dynamic, bool)>();
        Advance(res);
        dynamic condition = res.Register(GeneralList());
        if (res.HasError)
            return res;
        dynamic body = res.Register(StatementBody());
        if (res.HasError)
            return res;
        cases.Add((condition, body.Item1, body.Item2));
        dynamic allCases = res.Register(ElifElseExpr());
        if (res.HasError)
            return res;
        cases = cases.Concat((List<(dynamic, dynamic, bool)>)allCases.Item1).ToList();
        return res.Success((cases, allCases.Item2));
    }
    ParseResult SwitchExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        dynamic valueToCheck = res.Register(GeneralList());
        if (res.HasError)
            return res;
        List<(dynamic, dynamic, bool)> cases = new List<(dynamic, dynamic, bool)>();
        (dynamic, bool) defaultCase = (null, false);
        AdvanceNewLine(res);
        if (CurrentTok.type != TokenTypes.LCURLY)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '{'"));
        Advance(res);
        AdvanceNewLine(res);
        while (CurrentTok.Matches(TokenTypes.KEYWORD, "case"))
        {
            cases.Add(res.Register(CaseExpr()));
            if (res.HasError)
                return res;
            AdvanceNewLine(res);
        }
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "default"))
        {
            defaultCase = res.Register(DefaultExpr());
            if (res.HasError)
                return res;
            AdvanceNewLine(res);
        }
        if (CurrentTok.type != TokenTypes.RCURLY)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '}'"));
        Advance(res);
        return res.Success(new SwitchNode(valueToCheck, cases, defaultCase, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult CaseExpr()
    {
        ParseResult res = new ParseResult();
        Advance(res);
        (dynamic, dynamic, bool) currentCase = (res.Register(GeneralList()), null, false);
        if (res.HasError)
            return res;
        AdvanceNewLine(res);
        if (!CurrentTok.Matches(TokenTypes.KEYWORD, "case"))
        {
            dynamic body = res.Register(StatementBody());
            if (res.HasError)
                return res;
            currentCase.Item2 = body.Item1;
            currentCase.Item3 = body.Item2;
        }
        return res.Success(currentCase);
    }
    ParseResult DefaultExpr()
    {
        ParseResult res = new ParseResult();
        Advance(res);
        dynamic defaultCase = res.Register(StatementBody());
        if (res.HasError)
            return res;
        return res.Success(defaultCase);
    }
    ParseResult ForExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        if (CurrentTok.type != TokenTypes.IDENTIFIER)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected identifier"));
        Token varName = CurrentTok;
        Advance(res);
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "in"))
        {
            Advance(res);
            dynamic listToIterate = res.Register(GeneralList());
            if (res.HasError)
                return res;
            dynamic foreachBody = res.Register(StatementBody());
            if (res.HasError)
                return res;
            return res.Success(new ForeachNode(varName, listToIterate, foreachBody.Item1, posStart, CurrentTok.posStart.Copy, foreachBody.Item2));
        }
        if (CurrentTok.type != TokenTypes.EQ)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '='"));
        Advance(res);
        dynamic startValue = null;
        if (!CurrentTok.Matches(TokenTypes.KEYWORD, "to"))
        {
            startValue = res.Register(GeneralList());
            if (res.HasError)
                return res;
            if (!CurrentTok.Matches(TokenTypes.KEYWORD, "to"))
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected 'to'"));
        }
        Advance(res);
        dynamic endValue = res.Register(GeneralList());
        if (res.HasError)
            return res;
        dynamic stepValue = null;
        if (CurrentTok.Matches(TokenTypes.KEYWORD, "step"))
        {
            Advance(res);
            stepValue = res.Register(GeneralList());
            if (res.HasError)
                return res;
        }
        dynamic body = res.Register(StatementBody());
        if (res.HasError)
            return res;
        return res.Success(new ForNode(varName, startValue, endValue, stepValue, body.Item1, posStart, CurrentTok.posStart.Copy, body.Item2));
    }
    ParseResult LoopExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        if (CurrentTok.type != TokenTypes.IDENTIFIER)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected identifier"));
        Token varName = CurrentTok;
        Advance(res);
        if (CurrentTok.type != TokenTypes.EQ)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '='"));
        Advance(res);
        dynamic startValue = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        if (CurrentTok.type != TokenTypes.COMMA)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ','"));
        Advance(res);
        dynamic condition = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        if (CurrentTok.type != TokenTypes.COMMA)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ','"));
        Advance(res);
        dynamic step = res.Register(TypeExpr());
        if (res.HasError)
            return res;
        dynamic body = res.Register(StatementBody());
        if (res.HasError)
            return res;
        return res.Success(new LoopNode(varName, startValue, condition, step, body.Item1, posStart, CurrentTok.posStart.Copy, body.Item2));
    }
    ParseResult WhileExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        dynamic condition = res.Register(GeneralList());
        if (res.HasError)
            return res;
        dynamic body = res.Register(StatementBody());
        if (res.HasError)
            return res;
        return res.Success(new WhileNode(condition, body.Item1, posStart, CurrentTok.posStart.Copy, body.Item2));
    }
    ParseResult TryExpr()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        dynamic tryBody = res.Register(StatementBody());
        if (res.HasError)
            return res;
        List<(List<dynamic>, dynamic, bool)> exceptCases = new List<(List<dynamic>, dynamic, bool)>();
        bool newLinesAdvanced = CurrentTok.type == TokenTypes.NEW_LINE;
        AdvanceNewLine(res);
        while (CurrentTok.Matches(TokenTypes.KEYWORD, "except"))
        {
            (List<dynamic>, dynamic, bool) exceptCase = (new List<dynamic>(), null, false);
            Advance(res);
            while (true)
            {
                dynamic exception = res.TryRegister(TypeExpr());
                if (exception == null)
                {
                    Reverse(res.toReverseCount);
                    break;
                }
                exceptCase.Item1.Add(exception);
                if (CurrentTok.type != TokenTypes.COMMA)
                    break;
                Advance(res);
            }
            dynamic body = res.Register(StatementBody());
            if (res.HasError)
                return res;
            exceptCase.Item2 = body.Item1;
            exceptCase.Item3 = body.Item2;
            exceptCases.Add(exceptCase);
            newLinesAdvanced = CurrentTok.type == TokenTypes.NEW_LINE;
            AdvanceNewLine(res);
        }
        if (newLinesAdvanced)
            Reverse();
        return res.Success(new TryNode(tryBody, exceptCases, posStart, CurrentTok.posStart.Copy));
    }
    ParseResult FuncDef()
    {
        ParseResult res = new ParseResult();
        Position posStart = CurrentTok.posStart.Copy;
        Advance(res);
        (bool isGlobal, bool isPublic, bool isConst, bool isStatic) = VarProps(res);
        Token returnType = null;
        if (CurrentTok.type == TokenTypes.TYPE)
        {
            returnType = CurrentTok;
            Advance(res);
        }
        Token funcName = null;
        if (CurrentTok.type == TokenTypes.IDENTIFIER)
        {
            funcName = CurrentTok;
            Advance(res);
        }
        if (CurrentTok.type != TokenTypes.LPAREN)
            return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '('"));
        Advance(res);
        List<Token> argNames = new List<Token>();
        List<Token> argTypes = new List<Token>();
        List<dynamic> argValues = new List<dynamic>();
        if (CurrentTok.type == TokenTypes.RPAREN)
            Advance(res);
        else
        {
            while (true)
            {
                if (CurrentTok.type == TokenTypes.TYPE)
                {
                    argTypes.Add(CurrentTok);
                    Advance(res);
                }
                else
                    argTypes.Add(null);
                if (CurrentTok.type != TokenTypes.IDENTIFIER)
                    return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected identifier"));
                argNames.Add(CurrentTok);
                Advance(res);
                if (CurrentTok.type == TokenTypes.EQ)
                {
                    Advance(res);
                    argValues.Add(res.Register(TypeExpr()));
                    if (res.HasError)
                        return res;
                }
                else
                    argValues.Add(null);
                if (CurrentTok.type != TokenTypes.COMMA)
                    break;
                Advance(res);
            }
            if (CurrentTok.type != TokenTypes.RPAREN)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ')'"));
            Advance(res);
        }
        dynamic returnValue = res.Register(StatementBody());
        if (res.error != null)
            return res;
        return res.Success(new FuncDefNode(funcName, returnType, argNames, argTypes, argValues, returnValue.Item1, posStart, CurrentTok.posStart.Copy, returnValue.Item2, isGlobal, isPublic, isConst, isStatic));
    }
    (bool, bool, bool, bool) VarProps(ParseResult res)
    {
        (bool, bool) VarProp(string propName, string propNameOpp = null)
        {
            if (CurrentTok.Matches(TokenTypes.KEYWORD, propName))
            {
                Advance(res);
                return (true, true);
            }
            else if (CurrentTok.Matches(TokenTypes.KEYWORD, propNameOpp))
            {
                Advance(res);
                return (false, true);
            }
            return (false, false);
        }
        bool globalPropGiven = false;
        bool publicPropGiven = false;
        bool isPublic = false;
        bool isGlobal = false;
        bool isConst = false;
        bool isStatic = false;
        for (int i = 0; i < 4; i++)
        {
            if (!globalPropGiven)
                (isGlobal, globalPropGiven) = VarProp("global", "local");
            if (!publicPropGiven)
                (isPublic, publicPropGiven) = VarProp("public", "private");
            if (!isConst)
                isConst = VarProp("const").Item1;
            if (!isStatic)
                isStatic = VarProp("static").Item1;
            if (!globalPropGiven && !publicPropGiven && !isConst && !isStatic)
                break;
        }
        return (isGlobal, isPublic, isConst, isStatic);
    }
    ParseResult StatementBody()
    {
        ParseResult res = new ParseResult();
        (dynamic, bool) body;
        AdvanceNewLine(res);
        if (CurrentTok.type == TokenTypes.LCURLY)
        {
            Advance(res);
            body = (res.Register(Statements()), true);
            if (res.HasError)
                return res;
            if (CurrentTok.type != TokenTypes.RCURLY)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected '}'"));
            Advance(res);
        }
        else
        {
            if (CurrentTok.type != TokenTypes.COL)
                return res.Failure(new InvalidSyntaxError(CurrentTok.posStart, CurrentTok.posEnd, "Expected ':'"));
            Advance(res);
            AdvanceNewLine(res);
            body = (res.Register(Statement()), false);
            if (res.HasError)
                return res;
        }
        return res.Success(body);
    }
}