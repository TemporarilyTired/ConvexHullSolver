using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace ConvexHullSolver;

internal static class InputGenerator
{
    public static void GenerateInput(string folderPath, int numberTests, int pointsPerTest, Func<(Rational, Rational)> coordinateGenerator)
    {
        if (!Path.EndsInDirectorySeparator(folderPath))
            throw new ArgumentException("Path given isn't a folder");

        for (int i = 0; i < numberTests; i++)
        {
            string newFilePath = Path.Join(folderPath, i.ToString() + ".txt");
            FileInfo fi = new FileInfo(newFilePath);
            if (!fi.Directory.Exists) 
            { 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
            } 
            StreamWriter sw = new(newFilePath);
            for (int j = 0; j < pointsPerTest; j++)
            {
                (Rational x, Rational y) = coordinateGenerator();
                //To prevent the entire string from having to exist in memory we can write the coordinates separately
                sw.Write(x);
                sw.Write(" ");
                sw.WriteLine(y);
                sw.Flush();
            }
            sw.Close();
        }
    }

    public enum RandomFuncs{
        BigRandom, //Args expected: (#ints to multiply to create nominator), (#ints to multiply to create denominator)
        UnitRandom, // Args expected: -
        OnCircle, // Args expected: (#Halley iterations)
        DisturbedCircle, // Args expected: (#Halley iterations), (Scale factor numerator), (Scale factor denominator) 
    }

    public static Func<(Rational, Rational)> CreateRandomFunc(RandomFuncs desiredFunc, Random random, params int[] args){
        switch (desiredFunc){
            case RandomFuncs.BigRandom:
                return PairGenerator(BigRandomRational(random, args));
            case RandomFuncs.UnitRandom:
                return PairGenerator(UnitRandom(random));
            case RandomFuncs.OnCircle:
                return CircleRandom(random, args);
            case RandomFuncs.DisturbedCircle:
                return DisturbedCircleRandom(random, args);
            default:
                throw new ArgumentException("Unsupported random number function selected");
        }
    }

    public static Func<(Rational, Rational)> PairGenerator(Func<Rational> generator){
        (Rational, Rational) Pairing(){
            return (generator(), generator());
        }
        return Pairing;
    }

    public static Func<Rational> BigRandomRational(Random random, int[] args)
    {
        int numberOfIntsNumerator = args[0]; //We generate big random ints by taking the product of k integers
        int numberOfIntsDenominator = args[1];
        Rational RationalNumberGenerator(){
            BigInteger numerator = 1;
            BigInteger denominator = 1;
            for (int i = 0; i < numberOfIntsNumerator; i++)
                numerator *= random.NextInt64();
            for (int i = 0; i < numberOfIntsDenominator; i++)
                denominator*= random.NextInt64();
            return new Rational(numerator, denominator);
        }
        return RationalNumberGenerator;
    }
    public static Func<Rational> UnitRandom(Random random){
        Rational RationalNumberGenerator(){
            var a = random.NextInt64();
            var b = random.NextInt64();
            return new Rational(Math.Min(a,b), Math.Max(a,b));
        }
        return RationalNumberGenerator;
    }

    public static Func<(Rational, Rational)> CircleRandom(Random random, int[] args){
        Rational one = new Rational(1,1);
        Rational two = new Rational(2,1);
        Rational three = new Rational(3,1);
        var UnitRandomGenerator = CreateRandomFunc(RandomFuncs.UnitRandom, random);
        int halleyIterations = args[0];
        //https://en.wikipedia.org/wiki/Fast_inverse_square_root#Zero_finding
        Rational HalleyInverseSquareRoot(Rational start, Rational currentGuess){
            var temp = start * currentGuess * currentGuess;
            return currentGuess*((three+temp)/(one+three*temp));
        }
        (Rational, Rational) RationalPairGenerator(){
            (Rational x, Rational y) = UnitRandomGenerator(); 
            //Unit random generates points in [0,1[, but we need [-1, 1[
            x = (two*x-one)/two;
            y = (two*y-one)/two;
            Rational lengthVectorSquared = x*x + y*y;
            Rational inverseRoot = lengthVectorSquared;
            for(int i = 0; i < halleyIterations; i++) 
                inverseRoot = HalleyInverseSquareRoot(lengthVectorSquared, inverseRoot);
            
            x *= inverseRoot;
            y *= inverseRoot;
            Rational magTest = (x*x + y*y) -one;
            Console.Write(BigInteger.Log10(BigInteger.Abs(magTest.numerator)));
            Console.Write(" ");
            Console.WriteLine(BigInteger.Log10(BigInteger.Abs(magTest.denominator)));
            return (x,y);
        }
        return RationalPairGenerator;
    }

    public static Func<(Rational, Rational)> DisturbedCircleRandom(Random random, int[] args){
        var CircleGenerator = CreateRandomFunc(RandomFuncs.OnCircle, random, args);
        Rational scaleFactor = new Rational(args[1], args[2]);
        (Rational, Rational) Funky(){
            (Rational x, Rational y) = CircleGenerator();
            (Rational deltaX, Rational deltaY) = CircleGenerator();
            return (x+deltaX*scaleFactor, y+deltaY*scaleFactor);
        }
        return Funky;
    }

}
