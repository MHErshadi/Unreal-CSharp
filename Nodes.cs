using System.Collections.Generic;

class VarAccessNode
{
    public readonly Token varName;
    public readonly Position posStart;
    public readonly Position posEnd;
    public VarAccessNode(Token varName)
    {
        this.varName = varName;
        posStart = this.varName.posStart;
        posEnd = this.varName.posEnd;
    }
}

class VarAssignNode
{
    public readonly Token varName;
    public readonly Token assignType;
    public readonly dynamic value;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly Token type;
    public readonly bool isGlobal;
    public readonly bool isPublic;
    public readonly bool isConst;
    public readonly bool isStatic;
    public VarAssignNode(Token varName, Token assignType, dynamic value, Position posStart, Position posEnd, Token type = null, bool isGlobal = false, bool isPublic = true, bool isConst = false, bool isStatic = false)
    {
        this.varName = varName;
        this.assignType = assignType;
        this.value = value;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.type = type;
        this.isGlobal = isGlobal;
        this.isPublic = isPublic;
        this.isConst = isConst;
        this.isStatic = isStatic;
    }
}

class ObjectNode
{
    public readonly Position posStart;
    public readonly Position posEnd;
    public ObjectNode(Token tok)
    {
        posStart = tok.posStart;
        posEnd = tok.posEnd;
    }
}

class NoneNode
{
    public readonly Position posStart;
    public readonly Position posEnd;
    public NoneNode(Token tok)
    {
        posStart = tok.posStart;
        posEnd = tok.posEnd;
    }
}

class UTypeNode
{
    public readonly Token type;
    public readonly Position posStart;
    public readonly Position posEnd;
    public UTypeNode(Token type)
    {
        this.type = type;
        posStart = this.type.posStart;
        posEnd = this.type.posEnd;
    }
}

class NumberNode
{
    public readonly Token value;
    public readonly Position posStart;
    public readonly Position posEnd;
    public NumberNode(Token value)
    {
        this.value = value;
        posStart = this.value.posStart;
        posEnd = this.value.posEnd;
    }
}

class BoolNode
{
    public readonly Token value;
    public readonly Position posStart;
    public readonly Position posEnd;
    public BoolNode(Token value)
    {
        this.value = value;
        posStart = this.value.posStart;
        posEnd = this.value.posEnd;
    }
}

class StringNode
{
    public readonly Token value;
    public readonly Position posStart;
    public readonly Position posEnd;
    public StringNode(Token value)
    {
        this.value = value;
        posStart = this.value.posStart;
        posEnd = this.value.posEnd;
    }
}

class ListNode
{
    public readonly List<dynamic> elements;
    public readonly Position posStart;
    public readonly Position posEnd;
    public ListNode(List<dynamic> elements, Position posStart, Position posEnd)
    {
        this.elements = elements;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class TupleNode
{
    public readonly List<dynamic> elements;
    public readonly Position posStart;
    public readonly Position posEnd;
    public TupleNode(List<dynamic> elements, Position posStart, Position posEnd)
    {
        this.elements = elements;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class DictionaryNode
{
    public readonly List<dynamic> keys;
    public readonly List<dynamic> values;
    public readonly Position posStart;
    public readonly Position posEnd;
    public DictionaryNode(List<dynamic> keys, List<dynamic> values, Position posStart, Position posEnd)
    {
        this.keys = keys;
        this.values = values;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class SetNode
{
    public readonly List<dynamic> elements;
    public readonly Position posStart;
    public readonly Position posEnd;
    public SetNode(List<dynamic> elements, Position posStart, Position posEnd)
    {
        this.elements = elements;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class DollarMethodNode
{
    public readonly Token methodName;
    public readonly List<dynamic> args;
    public readonly Position posStart;
    public readonly Position posEnd;
    public DollarMethodNode(Token methodName, List<dynamic> args, Position posStart, Position posEnd)
    {
        this.methodName = methodName;
        this.args = args;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class ParenNode
{
    public readonly dynamic expr;
    public readonly Position posStart;
    public readonly Position posEnd;
    public ParenNode(dynamic expr, Position posStart, Position posEnd)
    {
        this.expr = expr;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class BinOpNode
{
    public readonly dynamic leftOperand;
    public readonly Token op;
    public readonly dynamic rightOperand;
    public readonly Position posStart;
    public readonly Position posEnd;
    public BinOpNode(dynamic leftOperand, Token op, dynamic rightOperand)
    {
        this.leftOperand = leftOperand;
        this.op = op;
        this.rightOperand = rightOperand;
        posStart = this.leftOperand.posStart;
        posEnd = this.rightOperand.posEnd;
    }
}

class UnaryOpNode
{
    public readonly Token op;
    public readonly dynamic unaryExpr;
    public readonly Position posStart;
    public readonly Position posEnd;
    public UnaryOpNode(Token op, dynamic unaryExpr)
    {
        this.op = op;
        this.unaryExpr = unaryExpr;
        posStart = this.op.posStart;
        posEnd = this.unaryExpr.posEnd;
    }
}

class IfNode
{
    public readonly List<(dynamic, dynamic, bool)> cases;
    public readonly (dynamic, bool) elseCase;
    public readonly Position posStart;
    public readonly Position posEnd;
    public IfNode(List<(dynamic, dynamic, bool)> cases, (dynamic, bool) elseCase, Position posStart, Position posEnd)
    {
        this.cases = cases;
        this.elseCase = elseCase;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class SwitchNode
{
    public readonly dynamic valueToCheck;
    public readonly List<(dynamic, dynamic, bool)> cases;
    public readonly (dynamic, bool) defaultCase;
    public readonly Position posStart;
    public readonly Position posEnd;
    public SwitchNode(dynamic valueToCheck, List<(dynamic, dynamic, bool)> cases, (dynamic, bool) defaultCase, Position posStart, Position posEnd)
    {
        this.valueToCheck = valueToCheck;
        this.cases = cases;
        this.defaultCase = defaultCase;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class ForNode
{
    public readonly Token iteratorName;
    public readonly dynamic startValue;
    public readonly dynamic endValue;
    public readonly dynamic stepValue;
    public readonly dynamic body;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly bool shouldReturnNull;
    public ForNode(Token iteratorName, dynamic startValue, dynamic endValue, dynamic stepValue, dynamic body, Position posStart, Position posEnd, bool shouldReturnNull)
    {
        this.iteratorName = iteratorName;
        this.startValue = startValue;
        this.endValue = endValue;
        this.stepValue = stepValue;
        this.body = body;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.shouldReturnNull = shouldReturnNull;
    }
}

class ForeachNode
{
    public readonly Token iteratorName;
    public readonly dynamic listToIterate;
    public readonly dynamic body;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly bool shouldReturnNull;
    public ForeachNode(Token iteratorName, dynamic listToIterate, dynamic body, Position posStart, Position posEnd, bool shouldReturnNull)
    {
        this.iteratorName = iteratorName;
        this.listToIterate = listToIterate;
        this.body = body;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.shouldReturnNull = shouldReturnNull;
    }
}

class LoopNode
{
    public readonly Token iteratorName;
    public readonly dynamic startValue;
    public readonly dynamic condition;
    public readonly dynamic step;
    public readonly dynamic body;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly bool shouldReturnNull;
    public LoopNode(Token iteratorName, dynamic startValue, dynamic condition, dynamic step, dynamic body, Position posStart, Position posEnd, bool shouldReturnNull)
    {
        this.iteratorName = iteratorName;
        this.startValue = startValue;
        this.condition = condition;
        this.step = step;
        this.body = body;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.shouldReturnNull = shouldReturnNull;
    }
}

class WhileNode
{
    public readonly dynamic condition;
    public readonly dynamic body;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly bool shouldReturnNull;
    public WhileNode(dynamic condition, dynamic body, Position posStart, Position posEnd, bool shouldReturnNull)
    {
        this.condition = condition;
        this.body = body;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.shouldReturnNull = shouldReturnNull;
    }
}

class FuncDefNode
{
    public readonly Token funcName;
    public readonly Token returnType;
    public readonly List<Token> argNames;
    public readonly List<Token> argTypes;
    public readonly List<dynamic> argValues;
    public readonly dynamic body;
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly bool shouldReturnNull;
    public readonly bool isGlobal;
    public readonly bool isPublic;
    public readonly bool isConst;
    public readonly bool isStatic;
    public FuncDefNode(Token funcName, Token returnType, List<Token> argNames, List<Token> argTypes, List<dynamic> argValues, dynamic body, Position posStart, Position posEnd, bool shouldReturnNull, bool isGlobal, bool isPublic, bool isConst, bool isStatic)
    {
        this.funcName = funcName;
        this.returnType = returnType;
        this.argNames = argNames;
        this.argTypes = argTypes;
        this.argValues = argValues;
        this.body = body;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.shouldReturnNull = shouldReturnNull;
        this.isGlobal = isGlobal;
        this.isPublic = isPublic;
        this.isConst = isConst;
        this.isStatic = isStatic;
    }
}

class FuncCallNode
{
    public readonly dynamic funcToCall;
    public readonly List<Token> argNames;
    public readonly List<dynamic> argValues;
    public readonly Position posStart;
    public readonly Position posEnd;
    public FuncCallNode(dynamic funcToCall, List<Token> argNames, List<dynamic> argValues, Position posStart, Position posEnd)
    {
        this.funcToCall = funcToCall;
        this.argNames = argNames;
        this.argValues = argValues;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class IndexNode
{
    public readonly dynamic classifiedValue;
    public readonly dynamic index;
    public readonly Position posStart;
    public readonly Position posEnd;
    public IndexNode(dynamic classifiedValue, dynamic index, Position posStart, Position posEnd)
    {
        this.classifiedValue = classifiedValue;
        this.index = index;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class TryNode
{
    public (dynamic, bool) tryBody;
    public List<(List<dynamic>, dynamic, bool)> exceptCases;
    public Position posStart;
    public Position posEnd;
    public TryNode((dynamic, bool) tryBody, List<(List<dynamic>, dynamic, bool)> exceptCases, Position posStart, Position posEnd)
    {
        this.tryBody = tryBody;
        this.exceptCases = exceptCases;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class ReturnNode
{
    public dynamic returnValue;
    public readonly Position posStart;
    public readonly Position posEnd;
    public ReturnNode(dynamic returnValue, Position posStart, Position posEnd)
    {
        this.returnValue = returnValue;
        this.posStart = posStart;
        this.posEnd = posEnd;
    }
}

class ContinueNode
{
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly dynamic reasonToContinue;
    public ContinueNode(Position posStart, Position posEnd, dynamic reasonToContinue)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.reasonToContinue = reasonToContinue;
    }
}

class BreakNode
{
    public readonly Position posStart;
    public readonly Position posEnd;
    public readonly dynamic reasonToBreak;
    public BreakNode(Position posStart, Position posEnd, dynamic reasonToBreak)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.reasonToBreak = reasonToBreak;
    }
}