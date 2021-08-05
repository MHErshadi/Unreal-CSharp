using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

struct NumberGen
{
    public readonly string intSection;
    public readonly string decSection;
    public NumberGen(string intSection, string decSection)
    {
        this.intSection = intSection.TrimStart('0');
        this.decSection = decSection.TrimEnd('0');
        if (this.decSection.Length > SD.maxDecimalPlaces)
            this.decSection = this.decSection.Remove(SD.maxDecimalPlaces).TrimEnd('0');
    }
    public NumberGen(string number)
    {
        number = number.Contains(".") ? number.Trim('0') : number.TrimStart('0');
        string[] split = number.Split('.');
        if (split.Length == 2)
        {
            intSection = split[0];
            decSection = split[1];
        }
        else
        {
            intSection = split[0];
            decSection = "";
        }
        if (decSection.Length > SD.maxDecimalPlaces)
            decSection = decSection.Remove(SD.maxDecimalPlaces).TrimEnd('0');
    }
    public override string ToString()
    {
        return decSection != "" ? $"{intSection}.{decSection}" : intSection;
    }
}

struct StringGen
{
    public List<dynamic> formattedObjects;
    public StringGen(List<dynamic> formattedObjects)
    {
        this.formattedObjects = formattedObjects;
    }
}

class Value
{
    public Context context;
    public Position posStart;
    public Position posEnd;
    public string varName = null;
    public Value()
    {
        SetContext();
        SetPos();
    }
    public dynamic SetContext(Context context = null)
    {
        this.context = context;
        return this;
    }
    public dynamic SetPos(Position posStart = null, Position posEnd = null)
    {
        this.posStart = posStart;
        this.posEnd = posEnd;
        return this;
    }
    public virtual RuntimeResult Add(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Subtract(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Multiply(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Divide(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Remind(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Quotient(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Power(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Radical(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult Equal(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult NotEqual(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult LessThan(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult GreaterThan(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult LessThanOrEqual(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult GreaterThanOrEqual(dynamic other)
    {
        return IllegalOperation(other);
    }
    public RuntimeResult And(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(IsTrue && other.IsTrue).SetContext(context));
    }
    public RuntimeResult Or(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(IsTrue || other.IsTrue).SetContext(context));
    }
    public RuntimeResult Xor(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean((IsTrue && !other.IsTrue) || (!IsTrue && other.IsTrue)).SetContext(context));
    }
    public virtual RuntimeResult BitwiseAnd(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult BitwiseOr(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult BitwiseXor(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult LeftShift(dynamic other)
    {
        return IllegalOperation(other);
    }
    public virtual RuntimeResult RightShift(dynamic other)
    {
        return IllegalOperation(other);
    }
    public RuntimeResult Not()
    {
        return new RuntimeResult().Success(new Boolean(!IsTrue).SetContext(context));
    }
    public virtual RuntimeResult In(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (!new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
            return res.Failure(new RuntimeError(posStart, other.posEnd, "Other must be low level classified variable", context, ED.TYPE));
        return res.Success(new Boolean(other.elements.Contains(this)).SetContext(context));
    }
    public RuntimeResult Is(dynamic type)
    {
        RuntimeResult res = new RuntimeResult();
        if (type.GetType() != typeof(UType))
            return res.Failure(new RuntimeError(type.posStart, type.posEnd, "Type must be <type>", context, ED.TYPE));
        return res.Success(new Boolean(ID.varTypes[GetType()] == type.type).SetContext(context));
    }
    public virtual RuntimeResult Are(dynamic type)
    {
        return new RuntimeResult().Failure(new RuntimeError(posStart, posEnd, "Value must be low level classified variable", context, ED.TYPE));
    }
    public virtual RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Failure(new RuntimeError(posStart, posEnd, $"<{ID.varTypes[GetType()]}> isn't numerical type", context, ED.ILLEGAL_OP));
    }
    public virtual RuntimeResult Execute(List<dynamic> args, List<Token> argNames)
    {
        return new RuntimeResult().Failure(new RuntimeError(posStart, posEnd, $"<{ID.varTypes[GetType()]}> isn't functional type", context, ED.ILLEGAL_OP));
    }
    public virtual RuntimeResult Index(dynamic index)
    {
        return new RuntimeResult().Failure(new RuntimeError(posStart, posEnd, $"<{ID.varTypes[GetType()]}> isn't low level classified type", context, ED.ILLEGAL_OP));
    }
    public virtual bool IsTrue { get { return true; } }
    public RuntimeResult IllegalOperation(dynamic other)
    {
        string type = ID.varTypes[GetType()];
        return new RuntimeResult().Failure(new RuntimeError(posStart, other.posEnd, $"Illegal operation between <{type}> and <{type}>", context, ED.ILLEGAL_OP));
    }
    public virtual string PrintString { get { return ToString(); } }
    public bool CheckLists(List<dynamic> left, List<dynamic> right)
    {
        if (left.Count != right.Count)
            return false;
        for (int i = 0; i < left.Count; i++)
            if (!left[i].Equals(right[i]))
                return false;
        return true;
    }
    public List<dynamic> CopyElements(List<dynamic> elements)
    {
        List<dynamic> copy = new List<dynamic>();
        foreach (dynamic v in elements)
            copy.Add(v.Copy);
        return copy;
    }
}

class Object : Value
{
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Success(ND.none.SetContext(context));
    }
    public Object Copy
    {
        get
        {
            Object copy = new Object();
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return "object";
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Object);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class None : Value
{
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Success(ND.@object.SetContext(context));
    }
    public None Copy
    {
        get
        {
            None copy = new None();
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override bool IsTrue { get { return false; } }
    public override string ToString()
    {
        return "none";
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(None);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class UType : Value
{
    public string type;
    public UType(string type)
    {
        this.type = type;
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Success(ND.none.SetContext(context));
    }
    public UType Copy
    {
        get
        {
            UType copy = new UType(type);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return $"<class {type}>";
    }
    public override string PrintString { get { return type; } }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(UType) && ((UType)obj).type == type;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class Number : Value
{
    public BigInteger value;
    public BigInteger decimalCount;
    public string show;
    public Number(NumberGen value)
    {
        string strValue = value.intSection + value.decSection;
        if (strValue == "")
            strValue = "0";
        this.value = BigInteger.Parse(strValue);
        decimalCount = value.decSection.Length;
        show = value.intSection == ""
            ? value.decSection != "" ? $"0.{value.decSection}" : "0"
            : value.decSection != "" ? $"{value.intSection}.{value.decSection}" : value.intSection;
    }
    public Number(BigInteger value)
    {
        this.value = value;
        decimalCount = 0;
        show = this.value.ToString();
    }
    public Number(BigInteger value, BigInteger decimalCount, string show)
    {
        this.value = value;
        this.decimalCount = decimalCount;
        this.show = show;
    }
    public override RuntimeResult Add(dynamic other)
    {
        if (other.GetType() == typeof(Number))
            return new RuntimeResult().Success(MathCalc.Add(this, other).SetContext(context));
        return IllegalOperation(other);
    }
    public override RuntimeResult Subtract(dynamic other)
    {
        if (other.GetType() == typeof(Number))
            return new RuntimeResult().Success(MathCalc.Subtract(this, other).SetContext(context));
        return IllegalOperation(other);
    }
    public override RuntimeResult Multiply(dynamic other)
    {
        if (other.GetType() == typeof(Number))
            return new RuntimeResult().Success(MathCalc.Multiply(this, other).SetContext(context));
        return IllegalOperation(other);
    }
    public override RuntimeResult Divide(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.value == 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, "Division by zero", context, ED.DIV_BY_ZERO));
            return res.Success(MathCalc.Divide(this, other).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Remind(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.value == 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, "Division by zero", context, ED.DIV_BY_ZERO));
            return res.Success(MathCalc.Remind(this, other).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Quotient(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.value == 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, "Division by zero", context, ED.DIV_BY_ZERO));
            return res.Success(MathCalc.Quotient(this, other).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Power(dynamic other)
    {
        if (other.GetType() == typeof(Number))
            return new RuntimeResult().Success(MathCalc.Power(this, other).SetContext(context));
        return IllegalOperation(other);
    }
    public override RuntimeResult Radical(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.value == 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, "Division by zero", context, ED.DIV_BY_ZERO));
            return res.Success(MathCalc.Power(this, MathCalc.Divide(ND.one, other)).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult LessThan(dynamic other)
    {
        if (other.GetType() == typeof(Number))
        {
            (Number left, Number right) = MathCalc.Balancer(Copy, (Number)other.Copy);
            return new RuntimeResult().Success(new Boolean(left.value < right.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult GreaterThan(dynamic other)
    {
        if (other.GetType() == typeof(Number))
        {
            (Number left, Number right) = MathCalc.Balancer(Copy, (Number)other.Copy);
            return new RuntimeResult().Success(new Boolean(left.value > right.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult LessThanOrEqual(dynamic other)
    {
        if (other.GetType() == typeof(Number))
        {
            (Number left, Number right) = MathCalc.Balancer(Copy, (Number)other.Copy);
            return new RuntimeResult().Success(new Boolean(left.value <= right.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult GreaterThanOrEqual(dynamic other)
    {
        if (other.GetType() == typeof(Number))
        {
            (Number left, Number right) = MathCalc.Balancer(Copy, (Number)other.Copy);
            return new RuntimeResult().Success(new Boolean(left.value >= right.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult BitwiseAnd(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (decimalCount != 0)
                return res.Failure(new RuntimeError(posStart, posEnd, $"Number must be int", context, ED.FLOAT));
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Other must be int", context, ED.FLOAT));
            return res.Success(new Number(value & other.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult BitwiseOr(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (decimalCount != 0)
                return res.Failure(new RuntimeError(posStart, posEnd, $"Number must be int", context, ED.FLOAT));
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Other must be int", context, ED.FLOAT));
            return res.Success(new Number(value | other.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult BitwiseXor(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (decimalCount != 0)
                return res.Failure(new RuntimeError(posStart, posEnd, $"Number must be int", context, ED.FLOAT));
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Other must be int", context, ED.FLOAT));
            return res.Success(new Number(value ^ other.value).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult LeftShift(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (decimalCount != 0)
                return res.Failure(new RuntimeError(posStart, posEnd, $"Number must be int", context, ED.FLOAT));
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Shift must be int", context, ED.FLOAT));
            BigInteger result = value;
            foreach (int i in other.GetInt())
                result <<= i;
            return res.Success(new Number(result).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult RightShift(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (decimalCount != 0)
                return res.Failure(new RuntimeError(posStart, posEnd, $"Number must be int", context, ED.FLOAT));
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Shift must be int", context, ED.FLOAT));
            BigInteger result = value;
            foreach (int i in other.GetInt())
                result <<= i;
            return res.Success(new Number(result).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Success(new Number(~value).SetContext(context));
    }
    public override bool IsTrue { get { return value != 0; } }
    public Number Copy
    {
        get
        {
            Number copy = new Number(value, decimalCount, show);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return show;
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Number) && value == ((Number)obj).value && decimalCount == ((Number)obj).decimalCount;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
    public int[] GetInt()
    {
        if (decimalCount != 0)
            return null;
        BigInteger copy = value;
        List<int> ints = new List<int>();
        while (copy > int.MaxValue)
        {
            copy -= int.MaxValue;
            ints.Add(int.MaxValue);
        }
        while (copy < int.MinValue)
        {
            copy -= int.MinValue;
            ints.Add(int.MinValue);
        }
        if (copy != 0)
            ints.Add((int)copy);
        return ints.ToArray();
    }
}

class Boolean : Value
{
    public bool value;
    public string show;
    public Boolean(string value)
    {
        this.value = value == "true";
        show = value;
    }
    public Boolean(bool value)
    {
        this.value = value;
        show = value.ToString().ToLower();
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult BitwiseNot()
    {
        return new RuntimeResult().Success(new Boolean(!value).SetContext(context));
    }
    public override bool IsTrue { get { return value; } }
    public Boolean Copy
    {
        get
        {
            Boolean copy = new Boolean(show);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return show;
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Boolean) && value == ((Boolean)obj).value;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class String : Value
{
    public string value;
    public String(string value)
    {
        this.value = value;
    }
    public override RuntimeResult Add(dynamic other)
    {
        if (other.GetType() == typeof(String))
            return new RuntimeResult().Success(new String(value + other.value).SetContext(context));
        return IllegalOperation(other);
    }
    public override RuntimeResult Subtract(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        int indexMinus = 0;
        int indexPlus = 0;
        string result = value;
        RuntimeResult remove(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            if (v.value >= 0)
                v.value -= indexMinus++;
            else
                v.value -= indexPlus--;
            int i = v.value >= 0 ? (int)v.value : result.Length + (int)v.value;
            if (i < 0 || i >= result.Length)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            result = result.Remove(i, 1);
            return res.Success(new String(result).SetContext(context));
        }
        if (other.GetType() == typeof(Number))
            return remove(other);
        else if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
        {
            foreach (dynamic v in other.elements)
            {
                if (v.GetType() != typeof(Number))
                    return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be <num>", context, ED.TYPE));
                remove(v);
                if (res.ShouldReturn)
                    return res;
            }
            return res;
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Multiply(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Multiplier must be int", context, ED.FLOAT));
            if (other.value < 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Multiplier can't be less than 0", context, ED.LIMIT));
            string result = "";
            foreach (int i in other.GetInt())
                result += string.Concat(Enumerable.Repeat(value, i));
            return res.Success(new String(result).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult In(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(String))
            return res.Success(new Boolean(other.value.Contains(value)).SetContext(context));
        if (!new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
            return res.Failure(new RuntimeError(posStart, other.posEnd, "Other must be <str> or low level classified variable", context, ED.TYPE));
        return res.Success(new Boolean(other.elements.Contains(this)).SetContext(context));
    }
    public override RuntimeResult Index(dynamic index)
    {
        RuntimeResult res = new RuntimeResult();
        RuntimeResult finder(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            int i = v.value >= 0 ? (int)v.value : value.Length + (int)v.value;
            if (i < 0 || i >= value.Length)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            return res.Success(new String(value[i].ToString()).SetContext(context));
        }
        if (index.GetType() == typeof(Number))
            return finder(index);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)index.GetType()))
        {
            List<dynamic> chars = new List<dynamic>();
            foreach (dynamic v in index.elements)
            {
                if (v.GetType() != typeof(Number))
                    return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be <num>", context, ED.TYPE));
                chars.Add(finder(v).value);
                if (res.ShouldReturn)
                    return res;
            }
            return res.Success(new List(chars).SetContext(context));
        }
        return res.Failure(new RuntimeError(index.posStart, index.posEnd, $"Index must be <num> or low level classified variable", context, ED.TYPE));
    }
    public override bool IsTrue { get { return value.Length != 0; } }
    public String Copy
    {
        get
        {
            String copy = new String(value);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return $"\"{value}\"";
    }
    public override string PrintString { get { return value; } }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(String) && value == ((String)obj).value;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class List : Value
{
    public List<dynamic> elements;
    public List(List<dynamic> elements)
    {
        this.elements = elements;
    }
    public override RuntimeResult Add(dynamic other)
    {
        List<dynamic> copy = CopyElements(elements);
        if (other.GetType() == typeof(List))
            copy = copy.Concat(CopyElements((List<dynamic>)other.elements)).ToList();
        else
            copy.Add(other.Copy);
        return new RuntimeResult().Success(new List(copy).SetContext(context));
    }
    public override RuntimeResult Subtract(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        int indexMinus = 0;
        int indexPlus = 0;
        List<dynamic> copy = CopyElements(elements);
        RuntimeResult remove(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            if (v.value >= 0)
                v.value -= indexMinus++;
            else
                v.value -= indexPlus--;
            int i = v.value >= 0 ? (int)v.value : copy.Count + (int)v.value;
            if (i < 0 || i >= copy.Count)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            copy.RemoveAt(i);
            return res.Success(new List(copy).SetContext(context));
        }
        if (other.GetType() == typeof(Number))
            return remove(other);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
        {
            foreach (dynamic v in other.elements)
            {
                if (v.GetType() != typeof(Number))
                    return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be <num>", context, ED.TYPE));
                remove(v);
                if (res.ShouldReturn)
                    return res;
            }
            return res;
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Multiply(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        if (other.GetType() == typeof(Number))
        {
            if (other.decimalCount != 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Multiplier must be int", context, ED.FLOAT));
            if (other.value < 0)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"Multiplier can't be less than 0", context, ED.LIMIT));
            List<dynamic> result = new List<dynamic>();
            foreach (int i in other.GetInt())
                for (int j = 0; j < i; j++)
                    result = result.Concat(CopyElements(elements)).ToList();
            return res.Success(new List(result).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Are(dynamic type)
    {
        RuntimeResult res = new RuntimeResult();
        if (type.GetType() != typeof(UType))
            return res.Failure(new RuntimeError(type.posStart, type.posEnd, "Type must be <type>", context, ED.TYPE));
        bool result = false;
        foreach (dynamic v in elements)
        {
            if (ID.varTypes[v.GetType()] != type.type)
            {
                result = false;
                break;
            }
            result = true;
        }
        return res.Success(new Boolean(result).SetContext(context));
    }
    public override RuntimeResult Index(dynamic index)
    {
        RuntimeResult res = new RuntimeResult();
        RuntimeResult finder(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            int i = v.value >= 0 ? (int)v.value : elements.Count + (int)v.value;
            if (i < 0 || i >= elements.Count)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            return res.Success(elements[i]);
        }
        if (index.GetType() == typeof(Number))
            return finder(index);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)index.GetType()))
        {
            List<dynamic> values = new List<dynamic>();
            foreach (dynamic v in index.elements)
            {
                values.Add(finder(v).value.Copy);
                if (res.ShouldReturn)
                    return res;
            }
            return res.Success(new List(values).SetContext(context));
        }
        return res.Failure(new RuntimeError(index.posStart, index.posEnd, $"Index must be <num> or low level classified variable", context, ED.TYPE));
    }
    public override bool IsTrue { get { return elements.Count != 0; } }
    public List Copy
    {
        get
        {
            List copy = new List(CopyElements(elements));
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return $"[{string.Join(", ", elements)}]";
    }
    public override string PrintString { get { return string.Join(", ", elements); } }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(List) && CheckLists(elements, ((List)obj).elements);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class Tuple : Value
{
    public List<dynamic> elements;
    public Tuple(List<dynamic> elements)
    {
        this.elements = elements;
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Are(dynamic type)
    {
        RuntimeResult res = new RuntimeResult();
        if (type.GetType() != typeof(UType))
            return res.Failure(new RuntimeError(type.posStart, type.posEnd, "Type must be <type>", context, ED.TYPE));
        bool result = false;
        foreach (dynamic v in elements)
        {
            if (ID.varTypes[v.GetType()] != type.type)
            {
                result = false;
                break;
            }
            result = true;
        }
        return res.Success(new Boolean(result).SetContext(context));
    }
    public override RuntimeResult Index(dynamic index)
    {
        RuntimeResult res = new RuntimeResult();
        RuntimeResult finder(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            int i = v.value >= 0 ? (int)v.value : elements.Count + (int)v.value;
            if (i < 0 || i >= elements.Count)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            return res.Success(elements[i]);
        }
        if (index.GetType() == typeof(Number))
            return finder(index);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)index.GetType()))
        {
            List<dynamic> values = new List<dynamic>();
            foreach (dynamic v in index.elements)
            {
                values.Add(finder(v).value.Copy);
                if (res.ShouldReturn)
                    return res;
            }
            return res.Success(new Tuple(values).SetContext(context));
        }
        return res.Failure(new RuntimeError(index.posStart, index.posEnd, $"Index must be <num> or low level classified variable", context, ED.TYPE));
    }
    public override bool IsTrue { get { return elements.Count != 0; } }
    public Tuple Copy
    {
        get
        {
            Tuple copy = new Tuple(CopyElements(elements));
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return $"({string.Join(", ", elements)})";
    }
    public override string PrintString { get { return string.Join(", ", elements); } }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Tuple) && CheckLists(elements, ((Tuple)obj).elements);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class Dictionary : Value
{
    public List<dynamic> keys;
    public List<dynamic> values;
    public Dictionary(List<dynamic> keys, List<dynamic> values)
    {
        this.keys = keys;
        this.values = values;
    }
    public override RuntimeResult Add(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> copyKeys = CopyElements(keys);
        List<dynamic> copyValues = CopyElements(values);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
        {
            if (other.elements.Count != 2)
                return res.Failure(new RuntimeError(other.posStart, other.posEnd, "Other must have two elements (key and value)", context, ED.LEN));
            copyKeys.Add(other.elements[0].Copy);
            copyValues.Add(other.elements[1].Copy);
            return res.Success(new Dictionary(copyKeys, copyValues).SetContext(context));
        }
        else if (other.GetType() == typeof(Dictionary))
        {
            copyKeys = copyKeys.Concat(CopyElements((List<dynamic>)other.keys)).ToList();
            copyValues = copyValues.Concat(CopyElements((List<dynamic>)other.values)).ToList();
            return res.Success(new Dictionary(copyKeys, copyValues).SetContext(context));
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Subtract(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        List<dynamic> copyKeys = CopyElements(keys);
        List<dynamic> copyValues = CopyElements(values);
        if (!keys.Contains(other))
            return res.Failure(new RuntimeError(other.posStart, other.posEnd, $"The dictionary has no key with the value of {other}", context, ED.KEY));
        copyValues.RemoveAt(keys.IndexOf(other));
        copyKeys.Remove(other);
        return res.Success(new Dictionary(copyKeys, copyValues).SetContext(context));
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Index(dynamic index)
    {
        RuntimeResult res = new RuntimeResult();
        if (!keys.Contains(index))
            return res.Failure(new RuntimeError(index.posStart, index.posEnd, $"The dictionary has no key with the value of {index}", context, ED.KEY));
        return res.Success(values[keys.IndexOf(index)]);
    }
    public override bool IsTrue { get { return keys.Count != 0; } }
    public Dictionary Copy
    {
        get
        {
            Dictionary copy = new Dictionary(CopyElements(keys), CopyElements(values));
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        string show = "{";
        for (int i = 0; i < keys.Count; i++)
            show += $"{keys[i]}: {values[i]}, ";
        show = show.Remove(show.Length - 2) + "}";
        return show;
    }
    public override string PrintString
    {
        get
        {
            string show = "";
            for (int i = 0; i < keys.Count; i++)
                show += $"{keys[i]}: {values[i]}, ";
            show = show.Remove(show.Length - 2);
            return show;
        }
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Dictionary) && CheckLists(keys, ((Dictionary)obj).keys) && CheckLists(values, ((Dictionary)obj).values);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class Set : Value
{
    public List<dynamic> elements;
    public Set(List<dynamic> elements)
    {
        this.elements = elements;
    }
    public override RuntimeResult Add(dynamic other)
    {
        List<dynamic> copy = CopyElements(elements);
        if (other.GetType() == typeof(Set))
            copy = copy.Concat(CopyElements((List<dynamic>)other.elements)).ToList();
        else
            copy.Add(other.Copy);
        return new RuntimeResult().Success(new Set(copy).SetContext(context));
    }
    public override RuntimeResult Subtract(dynamic other)
    {
        RuntimeResult res = new RuntimeResult();
        int indexMinus = 0;
        int indexPlus = 0;
        List<dynamic> copy = CopyElements(elements);
        RuntimeResult remove(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            if (v.value >= 0)
                v.value -= indexMinus++;
            else
                v.value -= indexPlus--;
            int i = v.value >= 0 ? (int)v.value : copy.Count + (int)v.value;
            if (i < 0 || i >= copy.Count)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            copy.RemoveAt(i);
            return res.Success(new Set(copy).SetContext(context));
        }
        if (other.GetType() == typeof(Number))
            return remove(other);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)other.GetType()))
        {
            foreach (dynamic v in other.elements)
            {
                if (v.GetType() != typeof(Number))
                    return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be <num>", context, ED.TYPE));
                remove(v);
                if (res.ShouldReturn)
                    return res;
            }
            return res;
        }
        return IllegalOperation(other);
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Are(dynamic type)
    {
        RuntimeResult res = new RuntimeResult();
        if (type.GetType() != typeof(UType))
            return res.Failure(new RuntimeError(type.posStart, type.posEnd, "Type must be <type>", context, ED.TYPE));
        bool result = false;
        foreach (dynamic v in elements)
        {
            if (ID.varTypes[v.GetType()] != type.type)
            {
                result = false;
                break;
            }
            result = true;
        }
        return res.Success(new Boolean(result).SetContext(context));
    }
    public override RuntimeResult Index(dynamic index)
    {
        RuntimeResult res = new RuntimeResult();
        RuntimeResult finder(dynamic v)
        {
            if (v.decimalCount != 0)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index must be int", context, ED.FLOAT));
            if (v.value > int.MaxValue || v.value < int.MinValue)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            int i = v.value >= 0 ? (int)v.value : elements.Count + (int)v.value;
            if (i < 0 || i >= elements.Count)
                return res.Failure(new RuntimeError(v.posStart, v.posEnd, $"Index out of range", context, ED.RANGE));
            return res.Success(elements[i]);
        }
        if (index.GetType() == typeof(Number))
            return finder(index);
        if (new Type[3] { typeof(List), typeof(Tuple), typeof(Set) }.Contains((Type)index.GetType()))
        {
            List<dynamic> values = new List<dynamic>();
            foreach (dynamic v in index.elements)
            {
                values.Add(finder(v).value.Copy);
                if (res.ShouldReturn)
                    return res;
            }
            return res.Success(new Set(values).SetContext(context));
        }
        return res.Failure(new RuntimeError(index.posStart, index.posEnd, $"Index must be <num> or low level classified variable", context, ED.TYPE));
    }
    public override bool IsTrue { get { return elements.Count != 0; } }
    public Set Copy
    {
        get
        {
            Set copy = new Set(CopyElements(elements));
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return copy;
        }
    }
    public override string ToString()
    {
        return $"{{{string.Join(", ", elements)}}}";
    }
    public override string PrintString { get { return string.Join(", ", elements); } }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Set) && CheckLists(elements, ((Set)obj).elements);
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class BaseFunction : Value
{
    public string name;
    public List<string> argNames;
    public List<string> argTypes;
    public List<dynamic> argValues;
    public Context GenerateNewContext()
    {
        SymbolTable symbolTable = new SymbolTable(context.symbolTable);
        for (int i = 0; i < argNames.Count; i++)
            symbolTable.Set(argNames[i], new Variable(argValues[i], argTypes[i]));
        return new Context(name, symbolTable, context, posStart);
    }
    public RuntimeResult CheckArgs(Context funcContext, List<dynamic> argValues, List<Token> argNames, Position posStart = null, Position posEnd = null)
    {
        if (posStart == null)
            posStart = this.posStart;
        if (posEnd == null)
            posEnd = this.posEnd;
        RuntimeResult res = new RuntimeResult();
        if (argValues.Count > this.argNames.Count)
            return res.Failure(new RuntimeError(posStart, posEnd, $"{argValues.Count - this.argNames.Count} too many argument(s) passed into '{name}'", context, ED.ARG_COUNT));
        res.Register(PopulateArgs(funcContext, argValues, argNames));
        if (res.ShouldReturn)
            return res;
        int nullCount = 0;
        foreach (Variable v in funcContext.symbolTable.symbols.Values)
            if (v.value == null)
                nullCount++;
        if (nullCount != 0)
            return res.Failure(new RuntimeError(posStart, posEnd, $"{nullCount} too few argument(s) passed into '{name}'", context, ED.ARG_COUNT));
        return res.Success(null);
    }
    public RuntimeResult PopulateArgs(Context funcContext, List<dynamic> argValues, List<Token> argNames)
    {
        RuntimeResult res = new RuntimeResult();
        List<string> names = new List<string>();
        for (int i = 0; i < argValues.Count; i++)
        {
            Token argName = argNames[i];
            dynamic argValue = argValues[i];
            if (argName != null)
            {
                if (!this.argNames.Contains(argName.value))
                    return res.Failure(new RuntimeError(argName.posStart, argName.posEnd, $"Function '{name}' doesn't have the '{argName.value}' argument", context, ED.ARG_NOT_DEF));
                foreach (Token t in argNames)
                    names.Add(t?.value);
            }
            else
                names = new List<string>(this.argNames);
            string varName = names[i];
            Variable var = funcContext.symbolTable.Get(varName);
            string type = ID.varTypes[argValue.GetType()];
            if (var.type != null && var.type != type)
                return res.Failure(new RuntimeError(argValue.posStart, argValue.posEnd, $"Can't assign <{type}> to '{varName}' because '{varName}' is <{var.type}>", context, ED.ASSIGN_TYPE));
            funcContext.symbolTable.Set(varName, new Variable(argValue, var.type));
        }
        return res.Success(null);
    }
}

class Function : BaseFunction
{
    public dynamic body;
    public string returnType;
    public bool shouldReturnNull;
    public Function(string name, string returnType, dynamic body, List<string> argNames, List<string> argTypes, List<dynamic> argValues, bool shouldReturnNull)
    {
        this.name = name;
        this.returnType = returnType;
        this.body = body;
        this.argNames = argNames;
        this.argTypes = argTypes;
        this.argValues = argValues;
        this.shouldReturnNull = shouldReturnNull;
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Execute(List<dynamic> argValues, List<Token> argNames)
    {
        RuntimeResult res = new RuntimeResult();
        Interpreter interpreter = new Interpreter();
        Context funcContext = GenerateNewContext();
        res.Register(CheckArgs(funcContext, argValues, argNames));
        if (res.ShouldReturn)
            return res;
        dynamic funcReturn = res.Register(interpreter.Visit(body, funcContext));
        if (res.ShouldReturn && res.funcReturnValue == null)
            return res;
        dynamic value = !shouldReturnNull
            ? funcReturn ?? res.funcReturnValue
            : res.funcReturnValue ?? ND.none.SetContext(context).SetPos(posStart, posEnd);
        string valueType = ID.varTypes[value.GetType()];
        if (returnType != null && valueType != returnType)
            return res.Failure(new RuntimeError(value.posStart, value.posEnd, $"Can't return <{valueType}> because '{name}' returns <{returnType}>", context, ED.RETURN_TYPE));
        return res.Success(value);
    }
    public Function Copy
    {
        get
        {
            Function copy = new Function(name, returnType, body, argNames, argTypes, argValues, shouldReturnNull);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return this;
        }
    }
    public override string ToString()
    {
        return $"<function {name}>";
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(Function) && name == ((Function)obj).name;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class BuiltInFunction : BaseFunction
{
    public BuiltInFunction(string name, List<string> argNames, List<string> argTypes, List<dynamic> argValues)
    {
        this.name = name;
        this.argNames = argNames;
        this.argTypes = argTypes;
        this.argValues = argValues;
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Execute(List<dynamic> args, List<Token> argNames)
    {
        RuntimeResult res = new RuntimeResult();
        Context funcContext = GenerateNewContext();
        res.Register(CheckArgs(funcContext, args, argNames));
        if (res.ShouldReturn)
            return res;
        dynamic returnValue = res.Register((RuntimeResult)GetType().GetMethod($"Function_{name}").Invoke(this, new object[1] { funcContext }));
        if (res.ShouldReturn)
            return res;
        return res.Success(returnValue);
    }
    public RuntimeResult Function_print(Context context)
    {
        string text = context.symbolTable.Get("value").value.PrintString;
        string end = context.symbolTable.Get("end").value.value;
        if (SD.executeMode != 3)
            Console.Write(text + end);
        return new RuntimeResult().Success(new String(text));
    }
    public RuntimeResult Function_input(Context context)
    {
        string label = context.symbolTable.Get("label").value.value;
        if (SD.executeMode != 3)
            Console.Write(label);
        return new RuntimeResult().Success(new String(Console.ReadLine()));
    }
    public RuntimeResult Function_isType(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(UType)));
    }
    public RuntimeResult Function_isObject(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Object)));
    }
    public RuntimeResult Function_isNone(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(None)));
    }
    public RuntimeResult Function_isNum(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Number)));
    }
    public RuntimeResult Function_isBool(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Boolean)));
    }
    public RuntimeResult Function_isStr(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(String)));
    }
    public RuntimeResult Function_isList(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(List)));
    }
    public RuntimeResult Function_isTuple(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Tuple)));
    }
    public RuntimeResult Function_isDict(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Dictionary)));
    }
    public RuntimeResult Function_isSet(Context context)
    {
        return new RuntimeResult().Success(new Boolean(context.symbolTable.Get("value").value.GetType() == typeof(Set)));
    }
    public RuntimeResult Function_isFunction(Context context)
    {
        return new RuntimeResult().Success(new Boolean(new Type[2] { typeof(Function), typeof(BuiltInFunction) }.Contains((Type)context.symbolTable.Get("value").value.GetType())));
    }
    public RuntimeResult Function_clear(Context _)
    {
        Console.Clear();
        return new RuntimeResult().Success(ND.none);
    }
    public RuntimeResult Function_exit(Context context)
    {
        RuntimeResult res = new RuntimeResult();
        Number exitCode = context.symbolTable.Get("exit_code").value;
        if (exitCode.decimalCount != 0)
            return res.Failure(new RuntimeError(exitCode.posStart, exitCode.posEnd, "'exit_code' must be int", context, ED.FLOAT));
        if (exitCode.value > int.MaxValue || exitCode.value < int.MinValue)
            return res.Failure(new RuntimeError(exitCode.posStart, exitCode.posEnd, $"'exit_code' can't be greater than {int.MaxValue} or less than {int.MinValue}", context, ED.LIMIT));
        Environment.Exit((int)exitCode.value);
        return new RuntimeResult().Success(ND.none);
    }
    public RuntimeResult Function_execute(Context context)
    {
        RuntimeResult res = new RuntimeResult();
        string fn = context.symbolTable.Get("fn").value.value;
        Context execContext = new Context("<executed-program>", Core.GenerateGlobalSymbolTable());
        if (!File.Exists(fn))
            return res.Failure(new RuntimeError(posStart, posEnd, $"{fn} doesn't exist", context, ED.FILE_NOT_EXIST));
        string[] lines = File.ReadAllLines(fn);
        (dynamic result, Error error) = Core.Execute(fn, string.Join("\n", lines), execContext);
        if (error != null)
            return res.Failure(new RuntimeError(posStart, posEnd, $"Failed to finish executing {fn}:\n\n{error}", context, ED.EXECUTION));
        return res.Success(result);
    }
    public RuntimeResult Function_wait(Context context)
    {
        RuntimeResult res = new RuntimeResult();
        Number time = context.symbolTable.Get("time").value;
        if (time.decimalCount != 0)
            return res.Failure(new RuntimeError(time.posStart, time.posEnd, "'time' must be int", context, ED.TYPE));
        foreach (int i in time.GetInt())
            Thread.Sleep(i);
        return res.Success(ND.none.SetContext(context));
    }
    public BuiltInFunction Copy
    {
        get
        {
            BuiltInFunction copy = new BuiltInFunction(name, argNames, argTypes, argValues);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return this;
        }
    }
    public override string ToString()
    {
        return $"<built-in function {name}>";
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(BuiltInFunction) && name == ((BuiltInFunction)obj).name;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class ValueMethod : BaseFunction
{
    public Func<Context, RuntimeResult> body;
    public ValueMethod(string name, Func<Context, RuntimeResult> body, List<string> argNames, List<string> argTypes, List<dynamic> argValues)
    {
        this.name = name;
        this.body = body;
        this.argNames = argNames;
        this.argTypes = argTypes;
        this.argValues = argValues;
    }
    public override RuntimeResult Equal(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(Equals(other)).SetContext(context));
    }
    public override RuntimeResult NotEqual(dynamic other)
    {
        return new RuntimeResult().Success(new Boolean(!Equals(other)).SetContext(context));
    }
    public override RuntimeResult Execute(List<dynamic> args, List<Token> argNames)
    {
        RuntimeResult res = new RuntimeResult();
        Context methodContext = GenerateNewContext();
        res.Register(CheckArgs(methodContext, args, argNames));
        if (res.ShouldReturn)
            return res;
        dynamic returnValue = res.Register(body(methodContext));
        if (res.ShouldReturn)
            return res;
        return res.Success(returnValue);
    }
    public ValueMethod Copy
    {
        get
        {
            ValueMethod copy = new ValueMethod(name, body, argNames, argTypes, argValues);
            copy.SetContext(context);
            copy.SetPos(posStart, posEnd);
            return this;
        }
    }
    public override string ToString()
    {
        return $"<method {name}>";
    }
    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(ValueMethod) && name == ((ValueMethod)obj).name;
    }
    public override int GetHashCode() { return base.GetHashCode(); }
}

class DollarMethod
{
    readonly Token name;
    readonly Position posStart;
    readonly Position posEnd;
    readonly Context context;
    public DollarMethod(Token name, Position posStart, Position posEnd, Context context)
    {
        this.name = name;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.context = context;
    }
    public RuntimeResult Call(List<dynamic> argValues)
    {
        RuntimeResult res = new RuntimeResult();
        if (!FD.dollarMethodsInfo.ContainsKey(name.value))
            return res.Failure(new RuntimeError(name.posStart, name.posEnd, $"'{name.value}' isn't defined", context, ED.NOT_DEF));
        string[] argTypes = FD.dollarMethodsInfo[name.value];
        if (argValues.Count > argTypes.Length)
            return res.Failure(new RuntimeError(posStart, posEnd, $"{argValues.Count - argTypes.Length} too many argument(s) passed into '{name}'", context, ED.ARG_COUNT));
        if (argValues.Count < argTypes.Length)
            return res.Failure(new RuntimeError(posStart, posEnd, $"{argTypes.Length - argValues.Count} too few argument(s) passed into '{name}'", context, ED.ARG_COUNT));
        for (int i = 0; i < argValues.Count; i++)
        {
            dynamic a = argValues[i];
            if (ID.varTypes[a.GetType()] != argTypes[i])
                return res.Failure(new RuntimeError(a.posStart, a.posEnd, $"Argument {i + 1} must be <{argTypes[i]}>", context, ED.TYPE));
        }
        res.Register((RuntimeResult)GetType().GetMethod($"Method_{name.value}").Invoke(this, new object[1] { argValues }));
        if (res.ShouldReturn)
            return res;
        return res.Success(null);
    }
    public RuntimeResult Method_mode(List<dynamic> argValues)
    {
        RuntimeResult res = new RuntimeResult();
        Number mode = argValues[0];
        if (mode.decimalCount != 0 || !new BigInteger[3] { 1, 2, 3 }.Contains(mode.value))
            return res.Failure(new RuntimeError(mode.posStart, mode.posEnd, "mode must be 1 (build), 2 (develop) or 3 (none)", context, ED.INV_VALUE));
        SD.executeMode = (int)mode.value;
        return res.Success(null);
    }
    public RuntimeResult Method_reset(List<dynamic> _)
    {
        RuntimeResult res = new RuntimeResult();
        Core.context = new Context("<program>", Core.GenerateGlobalSymbolTable());
        return res.Success(null);
    }
}