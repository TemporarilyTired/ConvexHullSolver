using System.Numerics;

namespace ConvexHullSolver;

internal readonly struct Rational :
    IEquatable<Rational>,
    IComparisonOperators<Rational, Rational, Boolean>,
    IEqualityOperators<Rational, Rational, Boolean>,
    IParsable<Rational>,
    IMultiplyOperators<Rational, Rational, Rational>,
    IDivisionOperators<Rational, Rational, Rational>,
    ISubtractionOperators<Rational, Rational, Rational>,
    IAdditionOperators<Rational, Rational, Rational>,
    IAdditiveIdentity<Rational, Rational>
{
    public readonly BigInteger numerator;
    public readonly BigInteger denominator;

    public Rational(BigInteger numerator, BigInteger denominator)
    {
        this.numerator = numerator;
        this.denominator = denominator;
    }

    public static Rational AdditiveIdentity
    {
        get
        {
            return new Rational(0, 1);
        }
    }

    // 8093487041873/870780 or 212.0231 or 1233
    // 880809870/899080/8907.9807087097 should be rejected
    // We only accept dot-separated numbers, not comma separated.
    public static Rational Parse(string rational, IFormatProvider? provider)
    {
        string[] division = rational.Split("/");
        if (division.Length > 1)
        {
            if (division.Length == 2 && division[0].Length > 0 && division[1].Length > 0)
                return new Rational(BigInteger.Parse(division[0], provider), BigInteger.Parse(division[1], provider));
            throw new ArgumentException("/ is used, but input cannot be parsed");
        }
        string[] decimalSeparated = rational.Split(".");
        if (decimalSeparated.Length > 1)
        {
            if (decimalSeparated.Length == 2 && decimalSeparated[0].Length > 0 && decimalSeparated[1].Length > 0)
                return new Rational(BigInteger.Parse(decimalSeparated[0] + decimalSeparated[1], provider), BigInteger.Pow(10, decimalSeparated[1].Length));
            throw new ArgumentException(". is used as separator, but input cannot be parsed");
        }
        return new Rational(BigInteger.Parse(rational, provider), 1);
    }

    public static bool TryParse(string? rational, IFormatProvider? provider, out Rational result)
    {
        throw new NotImplementedException("Use Parse");
    }

    public static Rational operator *(Rational left, Rational right)
    {
        return new Rational(left.numerator * right.numerator, left.denominator * right.denominator);
    }

    public static Rational operator /(Rational numerator, Rational denominator){
        return new Rational(numerator.numerator * denominator.denominator, numerator.denominator * denominator.numerator);
    }

    public static Rational operator +(Rational left, Rational right){
        (Rational equalizedLeft, Rational equalizedRight) = Rational.EqualizeDenominator(left, right);
        return new Rational(equalizedLeft.numerator+equalizedRight.numerator, equalizedLeft.denominator);
    }

    public static Rational operator -(Rational left, Rational right)
    {
        (Rational equalizedLeft, Rational equalizedRight) = Rational.EqualizeDenominator(left, right);
        Rational result = new(equalizedLeft.numerator - equalizedRight.numerator,
                              equalizedLeft.denominator);
        return result;
    }

    public static Boolean operator ==(Rational left, Rational right)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(left, right);
        return r1.numerator == r2.numerator;
    }

    public static Boolean operator !=(Rational left, Rational right)
    {
        return !(left == right);
    }

    public static Boolean operator >(Rational left, Rational right)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(left, right);
        return r1.numerator > r2.numerator;
    }

    public static Boolean operator >=(Rational left, Rational right)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(left, right);
        return r1.numerator >= r2.numerator;
    }

    public static Boolean operator <(Rational left, Rational right)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(left, right);
        return r1.numerator < r2.numerator;
    }

    public static Boolean operator <=(Rational left, Rational right)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(left, right);
        return r1.numerator <= r2.numerator;
    }

    public bool Equals(Rational otherRational)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(this, otherRational);
        return r1.numerator.Equals(r2.numerator);
    }

    public override bool Equals(Object? obj)
    {
        if (obj is Rational other)
            return Equals(other);
        return false;
    }

    public int CompareTo(Rational otherRational)
    {
        (Rational r1, Rational r2) = EqualizeDenominator(this, otherRational);
        return r1.numerator.CompareTo(r2.numerator);
    }

    //We do not reduce this, as this takes up more performance than we gain in our use case 
    //TODO: verify above statement
    public static (Rational, Rational) EqualizeDenominator(Rational r1, Rational r2)
    {
        Rational newr1 = new(r1.numerator * r2.denominator, r1.denominator * r2.denominator);
        Rational newr2 = new(r2.numerator * r1.denominator, r2.denominator * r1.denominator);
        return (newr1, newr2);
    }

    public static (Rational, Rational) ProperEqualizeDenominator(Rational r1, Rational r2)
    {
        //Euclidian algorithm
        static BigInteger GCD(BigInteger b1, BigInteger b2)
        {
            BigInteger temp;
            while (b2 != 0)
            {
                temp = b2;
                b2 = b1 % b2;
                b1 = temp;
            }
            return b1;
        }
        BigInteger gcd = GCD(r1.denominator, r2.denominator);
        BigInteger r1mult = r2.denominator / gcd;
        BigInteger r2mult = r1.denominator / gcd;
        return (new Rational(r1.numerator * r1mult, r1.denominator * r1mult), new Rational(r2.numerator * r2mult, r2.denominator * r2mult));
    }

    public override string ToString()
    {
        return numerator.ToString() + "/" + denominator.ToString();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
