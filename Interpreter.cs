using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

class Interpreter
{
    static bool inFunction = false;
    static bool inLoop = false;
    public RuntimeResult Visit(dynamic node, Context context)
    {
        MethodInfo methodToVisitNode = GetType().GetMethod($"Visit{node.GetType().Name}");
        return (RuntimeResult)methodToVisitNode.Invoke(this, new object[2] { node, context });
    }
    public RuntimeResult VisitUTypeNode(UTypeNode node, Context context)
    {
        UType value = SetValuePosContext(new UType(node.type.value), node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitObjectNode(ObjectNode node, Context context)
    {
        Object value = SetValuePosContext(ND.@object, node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitNoneNode(NoneNode node, Context context)
    {
        None value = SetValuePosContext(ND.none, node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitNumberNode(NumberNode node, Context context)
    {
        Number value = SetValuePosContext(new Number(node.value.value), node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitBoolNode(BoolNode node, Context context)
    {
        bool value = SetValuePosContext(new Boolean(node.value.value), node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitStringNode(StringNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string str = "";
        foreach (dynamic o in node.value.value.formattedObjects)
        {
            if (o.GetType() == typeof(string))
                str += o;
            else
            {
                dynamic formattedObject = res.Register(Visit(o, context));
                if (res.ShouldReturn)
                    return res;
                str += formattedObject.PrintString;
            }
        }
        String value = SetValuePosContext(new String(str), node, context);
        return new RuntimeResult().Success(value);
    }
    public RuntimeResult VisitListNode(ListNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> elements = new List<dynamic>();
        foreach (dynamic e in node.elements)
        {
            elements.Add(res.Register(Visit(e, context)));
            if (res.ShouldReturn)
                return res;
        }
        List value = SetValuePosContext(new List(elements), node, context);
        return res.Success(value);
    }
    public RuntimeResult VisitTupleNode(TupleNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> elements = new List<dynamic>();
        foreach (dynamic e in node.elements)
        {
            elements.Add(res.Register(Visit(e, context)));
            if (res.ShouldReturn)
                return res;
        }
        Tuple value = SetValuePosContext(new Tuple(elements), node, context);
        return res.Success(value);
    }
    public RuntimeResult VisitDictionaryNode(DictionaryNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> keys = new List<dynamic>();
        List<dynamic> values = new List<dynamic>();
        for (int i = 0; i < node.keys.Count; i++)
        {
            dynamic key = res.Register(Visit(node.keys[i], context));
            if (keys.Contains(key))
            {
                int duplicateMemberIndex = keys.IndexOf(key);
                keys.RemoveAt(duplicateMemberIndex);
                values.RemoveAt(duplicateMemberIndex);
            }
            keys.Add(key);
            if (res.ShouldReturn)
                return res;
            values.Add(res.Register(Visit(node.values[i], context)));
            if (res.ShouldReturn)
                return res;
        }
        Dictionary value = SetValuePosContext(new Dictionary(keys, values), node, context);
        return res.Success(value);
    }
    public RuntimeResult VisitSetNode(SetNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> elements = new List<dynamic>();
        foreach (dynamic e in node.elements)
        {
            dynamic element = res.Register(Visit(e, context));
            if (res.ShouldReturn)
                return res;
            if (!elements.Contains(element))
                elements.Add(element);
        }
        Set value = SetValuePosContext(new Set(elements), node, context);
        return res.Success(value);
    }
    public RuntimeResult VisitDollarMethodNode(DollarMethodNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> args = new List<dynamic>();
        foreach (dynamic a in node.args)
        {
            args.Add(res.Register(Visit(a, context)));
            if (res.ShouldReturn)
                return res;
        }
        res.Register(new DollarMethod(node.methodName, node.posStart, node.posEnd, context).Call(args));
        if (res.ShouldReturn)
            return res;
        return res.Success(SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitVarAccessNode(VarAccessNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string varName = node.varName.value;
        Variable variable = context.symbolTable.GetOrGetPrivate(varName);
        if (variable == null)
            return res.Failure(new RuntimeError(node.posStart, node.posEnd, $"'{varName}' isn't defined", context, ED.NOT_DEF));
        dynamic value = SetValuePosContext(variable.value, node, context);
        if (context.symbolTable.GetConst(varName))
            value = value.Copy;
        return res.Success(value);
    }
    public RuntimeResult VisitVarAssignNode(VarAssignNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string name = node.varName.value;
        string assignType = node.assignType?.type;
        if (context.symbolTable.GetConst(name))
            return res.Failure(new RuntimeError(node.varName.posStart, node.varName.posEnd, $"'{name}' is const variable", context, ED.CONST_VAR));
        Variable probableValue = context.symbolTable.GetOrGetPrivate(name);
        dynamic value = null;
        if (assignType == null)
            value = SetValuePosContext(probableValue != null ? probableValue.value : ND.none, node, context);
        else if (!new string[2] { TokenTypes.INC, TokenTypes.DEC }.Contains(assignType))
        {
            value = res.Register(Visit(node.value, context));
            if (res.ShouldReturn)
                return res;
            if (assignType != TokenTypes.ANC)
                value = value.Copy;
        }
        if (!new string[3] { TokenTypes.EQ, TokenTypes.ANC, null }.Contains(assignType))
        {
            if (probableValue == null)
                return res.Failure(new RuntimeError(node.varName.posStart, node.varName.posEnd, $"'{name}' isn't defined", context, ED.NOT_DEF));
            dynamic oldValue = SetValuePosContext(probableValue.value, node, context);
            RuntimeResult newValueRes = new RuntimeResult();
            switch (assignType)
            {
                case TokenTypes.PLUSE:
                    newValueRes = oldValue.Add(value);
                    break;
                case TokenTypes.MINUSE:
                    newValueRes = oldValue.Subtract(value);
                    break;
                case TokenTypes.MULE:
                    newValueRes = oldValue.Multiply(value);
                    break;
                case TokenTypes.DIVE:
                    newValueRes = oldValue.Divide(value);
                    break;
                case TokenTypes.REME:
                    newValueRes = oldValue.Remind(value);
                    break;
                case TokenTypes.QUOTE:
                    newValueRes = oldValue.Quotient(value);
                    break;
                case TokenTypes.POWE:
                    newValueRes = oldValue.Power(value);
                    break;
                case TokenTypes.RADE:
                    newValueRes = oldValue.Radical(value);
                    break;
                case TokenTypes.BIT_ANDE:
                    newValueRes = oldValue.BitwiseAnd(value);
                    break;
                case TokenTypes.BIT_ORE:
                    newValueRes = oldValue.BitwiseOr(value);
                    break;
                case TokenTypes.BIT_XORE:
                    newValueRes = oldValue.BitwiseXor(value);
                    break;
                case TokenTypes.LSHIFTE:
                    newValueRes = oldValue.LeftShift(value);
                    break;
                case TokenTypes.RSHIFTE:
                    newValueRes = oldValue.RightShift(value);
                    break;
                case TokenTypes.INC:
                    newValueRes = oldValue.Add(SetValuePosContext(ND.one, node, context));
                    break;
                case TokenTypes.DEC:
                    newValueRes = oldValue.Subtract(SetValuePosContext(ND.one, node, context));
                    break;
            }
            if (newValueRes.ShouldReturn)
                return res.Failure(newValueRes.error);
            value = newValueRes.value;
        }
        string valueType = ID.varTypes[value.GetType()];
        string varType = node.type?.value ?? probableValue?.type;
        if (varType != null && valueType != "none_" && valueType != varType)
            return res.Failure(new RuntimeError(value.posStart, value.posEnd, $"Can't assign <{valueType}> into '{name}' because its <{varType}>", context, ED.ASSIGN_TYPE));
        if (probableValue?.value.varName != null)
            name = probableValue.value.varName;
        SetVariable(context, name, value, varType, node.isGlobal, node.isPublic, node.isConst);
        return res.Success(value);
    }
    public RuntimeResult VisitParenNode(ParenNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        dynamic value = res.Register(Visit(node.expr, context));
        if (res.ShouldReturn)
            return res;
        return res.Success(value.SetPos(node.posStart, node.posEnd));
    }
    public RuntimeResult VisitBinOpNode(BinOpNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string op = node.op.type;
        dynamic left = res.Register(Visit(node.leftOperand, context));
        if (res.ShouldReturn)
            return res;
        if (op == TokenTypes.DOT)
            context = left.propsContext;
        dynamic right = res.Register(Visit(node.rightOperand, context));
        if (res.ShouldReturn)
            return res;
        RuntimeResult result = new RuntimeResult();
        if (op == TokenTypes.PLUS)
            result = left.Add(right);
        else if (op == TokenTypes.MINUS)
            result = left.Subtract(right);
        else if (op == TokenTypes.MUL)
            result = left.Multiply(right);
        else if (op == TokenTypes.DIV)
            result = left.Divide(right);
        else if (op == TokenTypes.REM)
            result = left.Remind(right);
        else if (op == TokenTypes.QUOT)
            result = left.Quotient(right);
        else if (op == TokenTypes.POW)
            result = left.Power(right);
        else if (op == TokenTypes.RAD)
            result = left.Radical(right);
        else if (op == TokenTypes.EE)
            result = left.Equal(right);
        else if (op == TokenTypes.NE)
            result = left.NotEqual(right);
        else if (op == TokenTypes.LT)
            result = left.LessThan(right);
        else if (op == TokenTypes.GT)
            result = left.GreaterThan(right);
        else if (op == TokenTypes.LTE)
            result = left.LessThanOrEqual(right);
        else if (op == TokenTypes.GTE)
            result = left.GreaterThanOrEqual(right);
        else if (op == TokenTypes.AND || node.op.value == "and")
            result = left.And(right);
        else if (op == TokenTypes.OR || node.op.value == "or")
            result = left.Or(right);
        else if (op == TokenTypes.XOR || node.op.value == "xor")
            result = left.Xor(right);
        else if (op == TokenTypes.BIT_AND)
            result = left.BitwiseAnd(right);
        else if (op == TokenTypes.BIT_OR)
            result = left.BitwiseOr(right);
        else if (op == TokenTypes.BIT_XOR)
            result = left.BitwiseXor(right);
        else if (op == TokenTypes.LSHIFT)
            result = left.LeftShift(right);
        else if (op == TokenTypes.RSHIFT)
            result = left.RightShift(right);
        else if (node.op.value == "in")
            result = left.In(right);
        else if (node.op.value == "is")
            result = left.Is(right);
        else if (node.op.value == "are")
            result = left.Are(right);
        else
            result.value = right.SetContext(context);
        if (result.ShouldReturn)
            return res.Failure(result.error);
        return res.Success(result.value.SetPos(node.posStart, node.posEnd));
    }
    public RuntimeResult VisitUnaryOpNode(UnaryOpNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string opTok = node.op.type;
        dynamic value = res.Register(Visit(node.unaryExpr, context));
        if (res.ShouldReturn)
            return res;
        RuntimeResult result = new RuntimeResult();
        if (opTok == TokenTypes.MINUS)
            result = value.Multiply(ND.minusOne);
        else if (opTok == TokenTypes.PLUS)
            result = value.Multiply(ND.one);
        else if (opTok == TokenTypes.NOT || node.op.value == "not")
            result = value.Not();
        else if (opTok == TokenTypes.BIT_NOT)
            result = value.BitwiseNot();
        if (result.ShouldReturn)
            return res.Failure(result.error);
        return res.Success(result.value.SetPos(node.posStart, node.posEnd));
    }
    public RuntimeResult VisitIfNode(IfNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        foreach ((dynamic condition, dynamic expr, bool shouldReturnNull) in node.cases)
        {
            dynamic conditionValue = res.Register(Visit(condition, context));
            if (res.ShouldReturn)
                return res;
            if (conditionValue.IsTrue)
            {
                dynamic ifValue = res.Register(Visit(expr, context));
                if (res.ShouldReturn)
                    return res;
                return res.Success(!shouldReturnNull ? ifValue : SetValuePosContext(ND.none, node, context));
            }
        }
        dynamic elseBody = node.elseCase.Item1;
        if (elseBody != null)
        {
            dynamic elseValue = res.Register(Visit(elseBody, context));
            if (res.ShouldReturn)
                return res;
            return res.Success(!node.elseCase.Item2 ? elseValue : SetValuePosContext(ND.none, node, context));
        }
        return res.Success(SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitSwitchNode(SwitchNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        dynamic switchValue = res.Register(Visit(node.valueToCheck, context));
        if (res.ShouldReturn)
            return res;
        bool caseMatches = false;
        foreach ((dynamic caseNode, dynamic bodyNode, bool shouldReturnNull) in node.cases)
        {
            dynamic caseCheck = res.Register(Visit(caseNode, context));
            if (res.ShouldReturn)
                return res;
            if (switchValue.Equal(caseCheck).value.IsTrue || caseMatches)
            {
                caseMatches = true;
                if (bodyNode != null && caseMatches)
                {
                    dynamic caseValue = res.Register(Visit(bodyNode, context));
                    if (res.ShouldReturn)
                        return res;
                    return res.Success(!shouldReturnNull ? caseValue : SetValuePosContext(ND.none, node, context));
                }
            }
        }
        dynamic defaultBody = node.defaultCase.Item1;
        if (defaultBody != null)
        {
            dynamic defaultValue = res.Register(Visit(defaultBody, context));
            if (res.ShouldReturn)
                return res;
            return res.Success(!node.defaultCase.Item2 ? defaultValue : SetValuePosContext(ND.none, node, context));
        }
        return res.Success(SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitForNode(ForNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string name = node.iteratorName.value;
        if (context.symbolTable.GetConst(name))
            return res.Failure(new RuntimeError(node.iteratorName.posStart, node.iteratorName.posEnd, $"'{name}' is const variable", context, ED.CONST_VAR));
        List<dynamic> elements = new List<dynamic>();
        dynamic startValue = node.startValue != null ? res.Register(Visit(node.startValue, context)) : ND.zero;
        if (res.ShouldReturn)
            return res;
        if (startValue.GetType() != typeof(Number))
            return res.Failure(new RuntimeError(node.startValue.posStart, node.startValue.posEnd, "Start value must be <num>", context, ED.TYPE));
        dynamic endValue = res.Register(Visit(node.endValue, context));
        if (res.ShouldReturn)
            return res;
        if (endValue.GetType() != typeof(Number))
            return res.Failure(new RuntimeError(node.endValue.posStart, node.endValue.posEnd, "End value must be <num>", context, ED.TYPE));
        dynamic stepValue = ND.one;
        if (node.stepValue != null)
        {
            stepValue = res.Register(Visit(node.stepValue, context));
            if (res.ShouldReturn)
                return res;
            if (stepValue.GetType() != typeof(Number))
                return res.Failure(new RuntimeError(node.stepValue.posStart, node.stepValue.posEnd, "Step value must be <num>", context, ED.TYPE));
        }
        Number i = startValue.Copy;
        Func<bool> condition = stepValue.value >= 0 ? (Func<bool>)(() => i.LessThan(endValue).value.value) : (() => i.GreaterThan(endValue).value.value);
        inLoop = true;
        while (condition())
        {
            context.symbolTable.Set(name, new Variable(i.Copy));
            i = MathCalc.Add(i, stepValue);
            dynamic forValue = res.Register(Visit(node.body, context));
            if (res.ShouldReturn && !res.loopShouldContinue && !res.loopShouldBreak)
                return res;
            if (res.loopShouldContinue)
                continue;
            if (res.loopShouldBreak)
                break;
            elements.Add(forValue);
        }
        inLoop = false;
        context.symbolTable.Remove(name);
        return res.Success(!node.shouldReturnNull ? SetValuePosContext(new List(elements), node, context) : SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitForeachNode(ForeachNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string name = node.iteratorName.value;
        if (context.symbolTable.GetConst(name))
            return res.Failure(new RuntimeError(node.iteratorName.posStart, node.iteratorName.posEnd, $"'{name}' is const variable", context, ED.CONST_VAR));
        List<dynamic> elements = new List<dynamic>();
        dynamic list = res.Register(Visit(node.listToIterate, context));
        if (res.ShouldReturn)
            return res;
        Type listType = list.GetType();
        if (!new Type[3] { typeof(String), typeof(List), typeof(Tuple) }.Contains(listType))
            return res.Failure(new RuntimeError(list.posStart, list.posEnd, $"Can't iterate inside <{ID.varTypes[listType]}>", context, ED.ITERATION));
        inLoop = true;
        if (listType == typeof(String))
            foreach (char c in list.value)
            {
                context.symbolTable.Set(name, new Variable(new String(c.ToString())));
                dynamic foreachValue = res.Register(Visit(node.body, context));
                if (res.ShouldReturn && !res.loopShouldContinue && !res.loopShouldBreak)
                    return res;
                if (res.loopShouldContinue)
                    continue;
                if (res.loopShouldBreak)
                    break;
                elements.Add(foreachValue);
            }
        else
            foreach (dynamic e in list.elements)
            {
                context.symbolTable.Set(name, new Variable(e.Copy));
                dynamic foreachValue = res.Register(Visit(node.body, context));
                if (res.ShouldReturn && !res.loopShouldContinue && !res.loopShouldBreak)
                    return res;
                if (res.loopShouldContinue)
                    continue;
                if (res.loopShouldBreak)
                    break;
                elements.Add(foreachValue);
            }
        inLoop = false;
        context.symbolTable.Remove(name);
        return res.Success(!node.shouldReturnNull ? SetValuePosContext(new List(elements), node, context) : SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitLoopNode(LoopNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string name = node.iteratorName.value;
        if (context.symbolTable.GetConst(name))
            return res.Failure(new RuntimeError(node.iteratorName.posStart, node.iteratorName.posEnd, $"'{name}' is const variable", context, ED.CONST_VAR));
        List<dynamic> elements = new List<dynamic>();
        dynamic startValue = res.Register(Visit(node.startValue, context));
        if (res.ShouldReturn)
            return res;
        context.symbolTable.Set(name, new Variable(startValue));
        inLoop = true;
        while (true)
        {
            dynamic condition = res.Register(Visit(node.condition, context));
            if (res.ShouldReturn)
                return res;
            if (!condition.IsTrue)
                break;
            dynamic loopValue = res.Register(Visit(node.body, context));
            if (res.ShouldReturn && !res.loopShouldContinue && !res.loopShouldBreak)
                return res;
            if (res.loopShouldContinue)
                continue;
            if (res.loopShouldBreak)
                break;
            elements.Add(loopValue);
            res.Register(Visit(node.step, context));
            if (res.ShouldReturn)
                return res;
        }
        inLoop = false;
        context.symbolTable.Remove(name);
        return res.Success(!node.shouldReturnNull ? SetValuePosContext(new List(elements), node, context) : SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitWhileNode(WhileNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> elements = new List<dynamic>();
        inLoop = true;
        while (true)
        {
            dynamic condition = res.Register(Visit(node.condition, context));
            if (res.ShouldReturn)
                return res;
            if (!condition.IsTrue)
                break;
            dynamic whileValue = res.Register(Visit(node.body, context));
            if (res.ShouldReturn && !res.loopShouldContinue && !res.loopShouldBreak)
                return res;
            if (res.loopShouldContinue)
                continue;
            if (res.loopShouldBreak)
                break;
            elements.Add(whileValue);
        }
        inLoop = false;
        return res.Success(!node.shouldReturnNull ? SetValuePosContext(new List(elements), node, context) : SetValuePosContext(ND.none, node, context));
    }
    public RuntimeResult VisitTryNode(TryNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        dynamic tryValue = res.Register(Visit(node.tryBody.Item1, context));
        if (res.ShouldReturn && res.error == null)
            return res;
        if (res.error != null)
        {
            RuntimeError error = res.error;
            res.error = null;
            foreach ((List<dynamic> exceptionNodes, dynamic body, bool shouldReturnNull) in node.exceptCases)
            {
                RuntimeResult exceptReturn()
                {
                    dynamic exceptValue = res.Register(Visit(body, context));
                    if (res.ShouldReturn)
                        return res;
                    return res.Success(!shouldReturnNull ? exceptValue : SetValuePosContext(ND.none, node, context));
                }
                if (exceptionNodes.Count == 0)
                    return exceptReturn();
                List<string> exceptionNames = res.Register(Exceptions(exceptionNodes, context));
                if (res.ShouldReturn)
                    return res;
                if (exceptionNames.Contains(error.details) || exceptionNames.Contains(error.type))
                    return exceptReturn();
            }
            if (node.exceptCases.Count != 0)
                return res.Failure(error);
            return res.Success(SetValuePosContext(ND.none, node, context));
        }
        return res.Success(!node.tryBody.Item2 ? tryValue : SetValuePosContext(ND.none, node, context));
    }
    RuntimeResult Exceptions(List<dynamic> exceptions, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<string> exceptionNames = new List<string>();
        foreach (dynamic exception in exceptions)
        {
            dynamic exceptionName = res.Register(Visit(exception, context));
            if (res.ShouldReturn)
                return res;
            if (exceptionName.GetType() != typeof(String))
                return res.Failure(new RuntimeError(exceptionName.posStart, exceptionName.posEnd, "Exception must be <str>", context, ED.TYPE));
            exceptionNames.Add(exceptionName.value);
        }
        return res.Success(exceptionNames);
    }
    public RuntimeResult VisitFuncDefNode(FuncDefNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string funcName = node.funcName?.value ?? "<anonymous>";
        if (context.symbolTable.constVars.Contains(funcName))
            return res.Failure(new RuntimeError(node.funcName.posStart, node.funcName.posEnd, $"'{funcName}' is const variable", context, ED.CONST_VAR));
        List<string> argNames = new List<string>();
        List<string> argTypes = new List<string>();
        List<dynamic> argValues = new List<dynamic>();
        for (int i = 0; i < node.argNames.Count; i++)
        {
            Token argType = node.argTypes[i];
            Token argName = node.argNames[i];
            dynamic argValue = node.argValues[i];
            argTypes.Add(argType?.value);
            argNames.Add(argName.value);
            if (argValue != null)
            {
                dynamic value = res.Register(Visit(argValue, context));
                if (res.ShouldReturn)
                    return res;
                string type = ID.varTypes[value.GetType()];
                if (argType?.value != null && argType.value != "none" && argType.value != type)
                    return res.Failure(new RuntimeError(argValue.posStart, argValue.posEnd, $"Can't assign <{type}> into '{argName.value}' because '{argName.value}' is <{argType.value}>", context, ED.ASSIGN_TYPE));
                argValues.Add(value);
            }
            else
                argValues.Add(null);
        }
        Function funcValue = SetValuePosContext(new Function(funcName, node.returnType?.value, node.body, argNames, argTypes, argValues, node.shouldReturnNull), node, context);
        SetVariable(context, funcName, funcValue, null, node.isGlobal, node.isPublic, node.isConst);
        return res.Success(funcValue);
    }
    public RuntimeResult VisitFuncCallNode(FuncCallNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        List<Token> argNames = new List<Token>();
        List<dynamic> argValues = new List<dynamic>();
        dynamic valueToCall = res.Register(Visit(node.funcToCall, context));
        if (res.ShouldReturn)
            return res;
        valueToCall.SetPos(node.posStart, node.posEnd);
        for (int i = 0; i < node.argValues.Count; i++)
        {
            Token argName = node.argNames[i];
            dynamic argValue = node.argValues[i];
            argNames.Add(argName);
            argValues.Add(res.Register(Visit(argValue, context)));
            if (res.ShouldReturn)
                return res;
        }
        inFunction = true;
        dynamic returnValue = res.Register(valueToCall.Execute(argValues, argNames));
        inFunction = false;
        if (res.ShouldReturn)
            return res;
        return res.Success(returnValue);
    }
    public RuntimeResult VisitIndexNode(IndexNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        dynamic value = res.Register(Visit(node.classifiedValue, context));
        if (res.ShouldReturn)
            return res;
        dynamic index = res.Register(Visit(node.index, context));
        if (res.ShouldReturn)
            return res;
        dynamic elements = res.Register(value.Index(index));
        if (res.ShouldReturn)
            return res;
        return res.Success(elements);
    }
    public RuntimeResult VisitReturnNode(ReturnNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        if (!inFunction)
            return res.Failure(new RuntimeError(node.posStart, node.posEnd, "'return' can't be outside of the function", context, ED.RETURN));
        dynamic returnValue = SetValuePosContext(ND.none, node, context);
        if (node.returnValue != null)
        {
            returnValue = res.Register(Visit(node.returnValue, context));
            if (res.ShouldReturn)
                return res;
        }
        return res.SuccessReturn(returnValue);
    }
    public RuntimeResult VisitContinueNode(ContinueNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        if (!inLoop)
            return res.Failure(new RuntimeError(node.posStart, node.posEnd, "'continue' can't be outside of the iteration statements (for, loop, foreach and while)", context, ED.CONTINUE));
        bool shouldContinue = true;
        if (node.reasonToContinue != null)
        {
            dynamic reason = res.Register(Visit(node.reasonToContinue, context));
            if (res.ShouldReturn)
                return res;
            shouldContinue = reason.IsTrue;
        }
        if (shouldContinue)
            return res.SuccessContinue();
        return res;
    }
    public RuntimeResult VisitBreakNode(BreakNode node, Context context)
    {
        RuntimeResult res = new RuntimeResult();
        if (!inLoop)
            return res.Failure(new RuntimeError(node.posStart, node.posEnd, "'break' can't be outside of the iteration statements (for, loop, foreach and while)", context, ED.BREAK));
        bool shouldBreak = true;
        if (node.reasonToBreak != null)
        {
            dynamic reason = res.Register(Visit(node.reasonToBreak, context));
            if (res.ShouldReturn)
                return res;
            shouldBreak = reason.IsTrue;
        }
        if (shouldBreak)
            return res.SuccessBreak();
        return res;
    }
    dynamic SetValuePosContext(dynamic value, dynamic node, Context context)
    {
        return value.SetContext(context).SetPos(node.posStart, node.posEnd);
    }
    void SetVariable(Context context, string name, dynamic value, string type, bool isGlobal, bool isPublic, bool isConst)
    {
        if (isGlobal)
            while (context.parent != null)
                context = context.parent;
        if (isConst)
            context.symbolTable.constVars.Add(name);
        void setVar(dynamic valueToSet)
        {
            if (isPublic)
                context.symbolTable.Set(name, new Variable(valueToSet, type));
            else
                context.symbolTable.SetPrivate(name, new Variable(valueToSet, type));
        }
        if (value.varName == null)
        {
            value.varName = name;
            setVar(value);
        }
        else
            setVar(value.varName);
    }
}