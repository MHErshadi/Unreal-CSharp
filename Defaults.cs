using System;
using System.Collections.Generic;

static class UD // Unreal Defaults
{
    public const string VERSION = "Unreal version 1.0.0";
    public static readonly string unrealEditorPortal = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\unreal-portal\unreal-editor";
}

static class LD // Lexer Defaults
{
    public const string DIGITS = "0123456789";
    public const string NUMBERS = DIGITS + ".";
    public const string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string IDENTIFIER_NAME = LETTERS + DIGITS + "_";
    public static readonly Dictionary<char, char> escapeChars = new Dictionary<char, char>(9)
    {
        { '0', '\0' },
        { 'a', '\a' },
        { 'b', '\b' },
        { 'f', '\f' },
        { 'n', '\n' },
        { 'r', '\r' },
        { 's', ' ' },
        { 't', '\t' },
        { 'v', '\v' },
    };
}

static class PD // Parser Defaults
{
    public static readonly string[] keywords = new string[35]
    {
        "var", "func",
        "global", "local",
        "public", "private",
        "const", "static",
        "and", "or", "xor", "not",
        "if", "elif", "else",
        "switch", "case", "default",
        "for", "to", "step",
        "loop",
        "while",
        "try", "except",
        "return", "continue", "break",
        "in",
        "is", "are",
        "object", "none", "true", "false"
    };
}

static class ID // Interpreter Defaults
{
    public static readonly Dictionary<Type, string> varTypes = new Dictionary<Type, string>(12)
    {
        { typeof(UType), "type_" },
        { typeof(Object), "object_" },
        { typeof(None), "none_" },
        { typeof(Number), "num_" },
        { typeof(Boolean), "bool_" },
        { typeof(String), "str_" },
        { typeof(List), "list_" },
        { typeof(Tuple), "tuple_" },
        { typeof(Dictionary), "dict_" },
        { typeof(Set), "set_" },
        { typeof(Function), "function_" },
        { typeof(BuiltInFunction), "function_" }
    };
}

static class SD // Setting Defaults
{
    public static int executeMode = 1;
    public static int maxDecimalPlaces = 300;
    public static int lnCalcLimit = 30;
    public static int ePowCalcLimit = 30;
}

static class ED // Error Defaults
{
    public const string ARG_COUNT = "ArgCountError";
    public const string ARG_NOT_DEF = "ArgNotDefError";
    public const string ASSIGN_TYPE = "AssignTypeError";
    public const string BREAK = "BreakError";
    public const string CONST_VAR = "ConstVarError";
    public const string CONTINUE = "ContinueError";
    public const string DIV_BY_ZERO = "DivByZeroError";
    public const string EXECUTION = "ExecutionError";
    public const string FILE_NOT_EXIST = "FileNotExistError";
    public const string FLOAT = "FloatError";
    public const string ILLEGAL_OP = "IllegalOpError";
    public const string INV_VALUE = "InvValueError";
    public const string ITERATION = "IterationError";
    public const string KEY = "KeyError";
    public const string LEN = "LenError";
    public const string LIMIT = "LimitError";
    public const string NOT_DEF = "NotDefError";
    public const string RANGE = "RangeError";
    public const string RETURN = "ReturnError";
    public const string RETURN_TYPE = "ReturnTypeError";
    public const string TYPE = "Type";
}

static class ND // Numerical Defaults
{
    public static readonly Object @object = new Object();
    public static readonly None none = new None();
    public static readonly Number minusOne = new Number(-1);
    public static readonly Number zero = new Number(0);
    public static readonly Number one = new Number(1);
    public static readonly Number two = new Number(2);
    public static readonly Number ten = new Number(10);
    public static readonly Number logTen = GenerateLogTen();
    static Number GenerateLogTen()
    {
        Number logTen = zero;
        Number y = MathCalc.Divide(MathCalc.Subtract(ten, one), MathCalc.Add(ten, one));
        for (int i = 0; i <= SD.lnCalcLimit; i++)
        {
            Number iNumber = MathCalc.Add(MathCalc.Multiply(two, new Number(i)), one);
            logTen = MathCalc.Add(logTen, MathCalc.Divide(MathCalc.Power(y, iNumber), iNumber));
        }
        return MathCalc.Multiply(two, logTen);
    }
    public static readonly Boolean @true = new Boolean(true);
    public static readonly Boolean @false = new Boolean(false);
}

static class CD // Classified Defaults
{
    public static readonly String empty = new String("");
    public static readonly String newLine = new String("\n");
    public static readonly String space = new String(" ");
    public static readonly String tab = new String("\t");
    public static readonly List whiteSpaces = new List(new List<dynamic>(2) { space, tab });
}

static class FD // Functional Defaults
{
    public static readonly BuiltInFunction print = new BuiltInFunction("print",
                new List<string>(2) { "value", "end" },
                new List<string>(2) { null, "str_" },
                new List<dynamic>(2) { CD.empty, CD.newLine });
    public static readonly BuiltInFunction input = new BuiltInFunction("input",
                new List<string>(1) { "label" },
                new List<string>(1) { "str_" },
                new List<dynamic>(1) { CD.empty });
    public static readonly BuiltInFunction isType = new BuiltInFunction("isType",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isObject = new BuiltInFunction("isObject",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isNone = new BuiltInFunction("isNone",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isNum = new BuiltInFunction("isNum",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isBool = new BuiltInFunction("isBool",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isString = new BuiltInFunction("isStr",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isList = new BuiltInFunction("isList",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isTuple = new BuiltInFunction("isTuple",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isDict = new BuiltInFunction("isDict",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isSet = new BuiltInFunction("isSet",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction isFunction = new BuiltInFunction("isFunction",
                new List<string>(1) { "value" },
                new List<string>(1) { null },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction clear = new BuiltInFunction("clear",
                new List<string>(0) { },
                new List<string>(0) { },
                new List<dynamic>(0) { });
    public static readonly BuiltInFunction exit = new BuiltInFunction("exit",
                new List<string>(1) { "exit_code" },
                new List<string>(1) { "num_" },
                new List<dynamic>(1) { ND.zero });
    public static readonly BuiltInFunction execute = new BuiltInFunction("execute",
                new List<string>(1) { "fn" },
                new List<string>(1) { "str_" },
                new List<dynamic>(1) { null });
    public static readonly BuiltInFunction wait = new BuiltInFunction("wait",
                new List<string>(1) { "time" },
                new List<string>(1) { "num_" },
                new List<dynamic>(1) { ND.zero });
    public static readonly Dictionary<string, string[]> dollarMethodsInfo = new Dictionary<string, string[]>()
    {
        { "mode", new string[1] { "num_" } },
        { "reset", new string[0] }
    };
}