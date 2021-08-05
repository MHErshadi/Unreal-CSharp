using System.Collections.Generic;
using System.IO;

static class Core
{
    public static Context context = new Context("<program>", GenerateGlobalSymbolTable());
    public static (string, string) CodeResult(string fn, string text)
    {
        (dynamic result, Error error) = Execute(fn, text, context);
        if (error != null)
            return (null, error.ToString());
        if (result != null)
            return (result.ToString(), null);
        return (null, null);
    }
    public static (dynamic, Error) Execute(string fn, string text, Context context)
    {
        (List<Token> tokens, IllegalCharError error) = new Lexer(fn, text).MakeTokens();
        if (error != null)
            return (null, error);
        if (tokens.Count == 1)
            return (null, null);
        ParseResult ast = new Parser(tokens).Parse();
        if (ast.error != null)
            return (null, ast.error);
        RuntimeResult result = new Interpreter().Visit(ast.node, context);
        if (result.error != null)
            return (null, result.error);
        return (result.value, null);
    }
    public static void Debug(string filePath)
    {
        Error error = Execute(filePath, string.Join("\n", File.ReadAllLines(filePath)), context).Item2;
        string answerString = "";
        if (error != null)
            answerString = $"{error.errorName}: {error.details}\n{error.posStart.idx}\n{error.posEnd.idx}";
        if (!Directory.Exists(UD.unrealEditorPortal))
            Directory.CreateDirectory(UD.unrealEditorPortal);
        File.WriteAllText($@"{UD.unrealEditorPortal}\portal.upf", answerString);
    }
    public static SymbolTable GenerateGlobalSymbolTable()
    {
        SymbolTable globalSymbolTable = new SymbolTable();
        globalSymbolTable.Set("null", new Variable(ND.zero));
        globalSymbolTable.Set("empty", new Variable(CD.empty));
        globalSymbolTable.Set("print", new Variable(FD.print));
        globalSymbolTable.Set("input", new Variable(FD.input));
        globalSymbolTable.Set("isType", new Variable(FD.isType));
        globalSymbolTable.Set("isObject", new Variable(FD.isObject));
        globalSymbolTable.Set("isNone", new Variable(FD.isNone));
        globalSymbolTable.Set("isNum", new Variable(FD.isNum));
        globalSymbolTable.Set("isBool", new Variable(FD.isBool));
        globalSymbolTable.Set("isStr", new Variable(FD.isString));
        globalSymbolTable.Set("isList", new Variable(FD.isList));
        globalSymbolTable.Set("isTuple", new Variable(FD.isTuple));
        globalSymbolTable.Set("isDict", new Variable(FD.isDict));
        globalSymbolTable.Set("isSet", new Variable(FD.isSet));
        globalSymbolTable.Set("isFunction", new Variable(FD.isFunction));
        globalSymbolTable.Set("clear", new Variable(FD.clear));
        globalSymbolTable.Set("exit", new Variable(FD.exit));
        globalSymbolTable.Set("execute", new Variable(FD.execute));
        globalSymbolTable.Set("wait", new Variable(FD.wait));
        globalSymbolTable.constVars.Add("null");
        globalSymbolTable.constVars.Add("empty");
        globalSymbolTable.constVars.Add("print");
        globalSymbolTable.constVars.Add("input");
        globalSymbolTable.constVars.Add("isType");
        globalSymbolTable.constVars.Add("isObject");
        globalSymbolTable.constVars.Add("isNone");
        globalSymbolTable.constVars.Add("isNum");
        globalSymbolTable.constVars.Add("isBool");
        globalSymbolTable.constVars.Add("isStr");
        globalSymbolTable.constVars.Add("isList");
        globalSymbolTable.constVars.Add("isTuple");
        globalSymbolTable.constVars.Add("isDict");
        globalSymbolTable.constVars.Add("isSet");
        globalSymbolTable.constVars.Add("isFunction");
        globalSymbolTable.constVars.Add("clear");
        globalSymbolTable.constVars.Add("exit");
        globalSymbolTable.constVars.Add("execute");
        globalSymbolTable.constVars.Add("wait");
        return globalSymbolTable;
    }
}