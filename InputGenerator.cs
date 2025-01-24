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
    public static void GenerateAllTests(string testFolderPath, int numberTestsPerType=5, 
    int geometricSeriesLength=2, int geometricSeriesStart=100, int geometricSeriesFactor=10)
    {
        const int BIGRANDOMINTCOUNTNUMERATOR = 3;
        const int BIGRANDOMINTCOUNTDENOMINATOR = 3;
        int[] DISTURBFACTOR = [1, 1000];
        Random random = new();
        //Big random number tests
        string bigRandomTestPath = Path.Join(testFolderPath, "BigRandom\\");
        FileInfo fi = new FileInfo(bigRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(bigRandomTestPath, numberTestsPerType, geometricSeriesLength, 
        geometricSeriesStart, geometricSeriesFactor, 
        CreateRandomFunc(RandomFuncs.BigRandom, random, 
        BIGRANDOMINTCOUNTNUMERATOR, BIGRANDOMINTCOUNTDENOMINATOR));

        //Unit random tests
        string unitRandomTestPath = Path.Join(testFolderPath, "UnitRandom\\");
        fi = new FileInfo(unitRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(unitRandomTestPath, numberTestsPerType, geometricSeriesLength, 
        geometricSeriesStart, geometricSeriesFactor, 
        CreateRandomFunc(RandomFuncs.UnitRandom, random));

        //On circle random tests
        string circleRandomTestPath = Path.Join(testFolderPath, "CircleRandom\\");
        fi = new FileInfo(circleRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(circleRandomTestPath, numberTestsPerType, geometricSeriesLength, 
        geometricSeriesStart, geometricSeriesFactor, 
        CreateRandomFunc(RandomFuncs.OnCircle, random));

        //Slightly off circle random tests
        string offCircleRandomTestPath = Path.Join(testFolderPath, "DisturbedRandom\\");
        fi = new FileInfo(offCircleRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(offCircleRandomTestPath, numberTestsPerType, geometricSeriesLength, 
        geometricSeriesStart, geometricSeriesFactor, 
        CreateRandomFunc(RandomFuncs.DisturbedCircle, random, DISTURBFACTOR));
    }

    private static void GenerateInputTypeCases(string folderPath, int numberTests, 
        int geometricSeriesLength, int geometricSeriesValue , int geometricSeriesFactor, Func<(Rational, Rational)> coordinateGenerator)
    {
        for(int i = 0; i<geometricSeriesLength; i++){
            string geoSeriesTestPath = Path.Join(folderPath,  $"n-{geometricSeriesValue}\\");
            FileInfo fi = new FileInfo(geoSeriesTestPath);
            if (!fi.Directory.Exists) 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
            GenerateTestCase<Rational>(geoSeriesTestPath, numberTests, geometricSeriesValue, coordinateGenerator);
            geometricSeriesValue*=geometricSeriesFactor;
        }
    }

    public static void GenerateTestCase<T>(string folderPath, int numberTests, int pointsPerTest, Func<(T, T)> coordinateGenerator)
    {
        if (!Path.EndsInDirectorySeparator(folderPath))
            throw new ArgumentException("Path given isn't a folder");

        for (int i = 1; i <= numberTests; i++)
        {
            string newFilePath = Path.Join(folderPath, i.ToString() + ".txt");
            FileInfo fi = new FileInfo(newFilePath);
            if (!fi.Directory.Exists) 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
                
            using(StreamWriter sw = new(newFilePath)){
                sw.WriteLine(pointsPerTest);
                for (int j = 0; j < pointsPerTest; j++)
                {
                    (T x, T y) = coordinateGenerator();
                    //To prevent the entire string from having to exist in memory we can write the coordinates separately
                    sw.Write(x);
                    sw.Write(" ");
                    sw.WriteLine(y);
                    sw.Flush();
                }
            }
        }
    }

    public enum RandomFuncs{
        BigRandom, //Args expected: (#ints to multiply to create nominator), (#ints to multiply to create denominator)
        UnitRandom, // Args expected: -
        OnCircle, // Args expected: -
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
        Rational one = new(1,1);
        // Rational two = new(2,1);
        // Rational three = new(3,1);
        // Rational half = new(1,2);
        // var UnitRandomGenerator = CreateRandomFunc(RandomFuncs.UnitRandom, random);
        // int halleyIterations = args[0];
        // //https://en.wikipedia.org/wiki/Fast_inverse_square_root#Zero_finding
        // Rational HalleyInverseSquareRoot(Rational start, Rational currentGuess){
        //     var temp = start * currentGuess * currentGuess;
        //     return Rational.ReduceFraction(currentGuess*((three+temp)/(one+three*temp)));
        // }

        // Rational HeronSquareRoot(Rational start, Rational currentGuess){
        //     return Rational.ReduceFraction(half*(currentGuess + start/currentGuess));
        // }

        const int precision = 19; //double precision limited
        double scaleFactorD = Math.Pow(10, precision);
        BigInteger scaleFactorBInt = BigInteger.Pow(10, precision);
        

        (Rational, Rational) RationalPairGenerator(){
            double angle = 2 * Math.PI * random.NextDouble();
            double xD = Math.Cos(angle) * scaleFactorD;
            double yD = Math.Sin(angle) * scaleFactorD;
            
            BigInteger xBInt = new(xD);
            BigInteger yBInt = new(yD);
                
            Rational x = Rational.ReduceFraction(new(xBInt, scaleFactorBInt));
            Rational y = Rational.ReduceFraction(new(yBInt, scaleFactorBInt));

            Rational magTest = (x*x + y*y) -one;
            // (Rational x, Rational y) = UnitRandomGenerator(); 
            // //Unit random generates points in [0,1[, but we need [-1, 1[
            // x = (two*x-one)/two;
            // y = (two*y-one)/two;
            // Rational lengthVectorSquared = x*x + y*y;
            // Rational squareRoot = lengthVectorSquared;
            // // for(int i = 0; i < halleyIterations; i++) 
            // //     inverseRoot = HalleyInverseSquareRoot(lengthVectorSquared, inverseRoot);
            // for (int i=0; i< halleyIterations; i++)
            //     squareRoot = HeronSquareRoot(lengthVectorSquared, squareRoot);
            // //inverseRoot = one/inverseRoot;
            // x /= squareRoot;
            // y /= squareRoot;
            // Console.Write(BigInteger.Log10(BigInteger.Abs(magTest.numerator)));
            // Console.Write(" ");
            // Console.WriteLine(BigInteger.Log10(BigInteger.Abs(magTest.denominator)));

            double difference = BigInteger.Log10(BigInteger.Abs(magTest.numerator)) - BigInteger.Log10(BigInteger.Abs(magTest.denominator));
            return (x,y);
        }
        return RationalPairGenerator;
    }

    public static Func<(Rational, Rational)> DisturbedCircleRandom(Random random, int[] args){
        var CircleGenerator = CreateRandomFunc(RandomFuncs.OnCircle, random, args);
        Rational scaleFactor = new(args[0], args[1]);
        (Rational, Rational) Funky(){
            (Rational x, Rational y) = CircleGenerator();
            (Rational deltaX, Rational deltaY) = CircleGenerator();
            return (x+deltaX*scaleFactor, y+deltaY*scaleFactor);
        }
        return Funky;
    }

}
