using System.Numerics;

static class MathCalc
{
    public static Number Add(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        (left, right) = Balancer(left, right);
        left.value += right.value;
        return ZeroCleaner(left);
    }
    public static Number Subtract(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        (left, right) = Balancer(left, right);
        left.value -= right.value;
        return ZeroCleaner(left);
    }
    public static Number Multiply(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        left.value *= right.value;
        left.decimalCount += right.decimalCount;
        return ZeroCleaner(left);
    }
    public static Number Divide(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        int count = 1;
        (left, right) = Balancer(left, right);
        left.decimalCount = 0;
        BigInteger remind = left.value % right.value;
        BigInteger result = left.value / right.value;
        while (remind != 0 && count <= SD.maxDecimalPlaces)
        {
            count++;
            left.value = BigInteger.Parse($"{left.value}0");
            left.decimalCount++;
            remind = left.value % right.value;
            result = left.value / right.value;
        }
        left.value = result;
        return ZeroCleaner(left);
    }
    public static Number Remind(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        (left, right) = Balancer(left, right);
        left.value %= right.value;
        left.decimalCount = 0;
        return ZeroCleaner(left);
    }
    public static Number Quotient(Number left, Number right)
    {
        left = left.Copy;
        right = right.Copy;
        (left, right) = Balancer(left, right);
        left.value /= right.value;
        left.decimalCount = 0;
        return ZeroCleaner(left);
    }
    public static Number Power(Number number, Number exponent)
    {
        number = number.Copy;
        exponent = exponent.Copy;
        bool isNeg = false;
        if (exponent.value < 0)
        {
            exponent.value *= -1;
            isNeg = true;
        }
        if (exponent.decimalCount == 0)
        {
            BigInteger result = 1;
            number.decimalCount *= exponent.value;
            while (exponent.value != 0)
            {
                if ((exponent.value & 1) == 1)
                    result *= number.value;
                number.value = BigInteger.Pow(number.value, 2);
                exponent.value >>= 1;
            }
            number.value = result;
        }
        else
            number = EPower(Multiply(exponent, NaturalLog(number)));
        if (isNeg)
            return Divide(ND.one, number);
        return ZeroCleaner(number);
    }
    public static Number EPower(Number exponent)
    {
        Number pow = ND.one;
        for (int i = 1; i <= SD.ePowCalcLimit; i++)
        {
            Number iNumber = new Number(i);
            pow = Add(pow, Divide(Power(exponent, iNumber), Factorial(iNumber)));
        }
        return pow;
    }
    public static Number NaturalLog(Number number)
    {
        (Number sciNumber, int exponent) = ScientificNumber(number);
        Number log = ND.zero;
        Number y = Divide(Subtract(sciNumber, ND.one), Add(sciNumber, ND.one));
        for (int i = 0; i <= SD.lnCalcLimit; i++)
        {
            Number iNumber = Add(Multiply(ND.two, new Number(i)), ND.one);
            log = Add(log, Divide(Power(y, iNumber), iNumber));
        }
        log = Multiply(ND.two, log);
        return Add(Multiply(new Number(exponent), ND.logTen), log);
    }
    public static Number Factorial(Number limit)
    {
        limit = limit.Copy;
        Number fact = ND.one;
        if (limit.decimalCount == 0 && limit.value >= 1)
            for (int i = 1; i <= limit.value; i++)
                fact = Multiply(fact, new Number(i));
        return fact;
    }
    public static (Number, int) ScientificNumber(Number number)
    {
        Number ten = ND.ten;
        number = number.Copy;
        int decimalCount = (int)number.decimalCount;
        int exponent = 0;
        string valueStr = number.value.ToString().Replace("-", "");
        while (decimalCount < valueStr.Length - 1)
        {
            number = Divide(number, ten);
            decimalCount++;
            exponent++;
        }
        while (decimalCount > valueStr.Length)
        {
            number = Multiply(number, ten);
            decimalCount--;
            exponent--;
        }
        return (number, exponent);
    }
    public static (Number, Number) Balancer(Number left, Number right)
    {
        if (left.decimalCount >= right.decimalCount)
            for (int i = (int)right.decimalCount; i < left.decimalCount; i++)
            {
                right.value *= 10;
                right.decimalCount++;
            }
        else
            for (int i = (int)left.decimalCount; i < right.decimalCount; i++)
            {
                left.value *= 10;
                left.decimalCount++;
            }
        return (left, right);
    }
    static Number ZeroCleaner(Number number)
    {
        bool isNeg = false;
        string valueStr = number.value.ToString();
        if (valueStr[0] == '-')
        {
            valueStr = valueStr.Remove(0, 1);
            isNeg = true;
        }
        while (valueStr.Length <= number.decimalCount)
            valueStr = $"0{valueStr}";
        if (number.decimalCount > SD.maxDecimalPlaces)
        {
            valueStr = valueStr.Remove(valueStr.Length - ((int)number.decimalCount - SD.maxDecimalPlaces));
            number.decimalCount = SD.maxDecimalPlaces;
        }
        valueStr = valueStr.Insert(valueStr.Length - (int)number.decimalCount, ".");
        if (isNeg)
            valueStr = $"-{valueStr}";
        return new Number(new NumberGen(valueStr));
    }
}