﻿using System.Numerics;

namespace ConvexHullSolver;

internal static class InputGenerator
{
    public static void GenerateAllTests(string testFolderPath, int[] testSizes, int numberTestsPerType=5)
    {
        int[] DISTURBFACTOR = [1, 1000];
        Random random = new();

        //Big random number tests
        string bigRandomTestPath = Path.Join(testFolderPath, "ConstantHull\\");
        FileInfo fi = new FileInfo(bigRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(bigRandomTestPath, numberTestsPerType, testSizes,
        CreateRandomFunc(RandomFuncs.UnitRandom, random));
        
        //Unit random tests
        string unitRandomTestPath = Path.Join(testFolderPath, "UnitRandom\\");
        fi = new FileInfo(unitRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(unitRandomTestPath, numberTestsPerType, testSizes, 
        CreateRandomFunc(RandomFuncs.UnitRandom, random));

        //On circle random tests
        string circleRandomTestPath = Path.Join(testFolderPath, "CircleRandom\\");
        fi = new FileInfo(circleRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(circleRandomTestPath, numberTestsPerType, testSizes, 
        CreateRandomFunc(RandomFuncs.OnCircle, random));

        //Slightly off circle random tests
        string offCircleRandomTestPath = Path.Join(testFolderPath, "DisturbedCircle\\");
        fi = new FileInfo(offCircleRandomTestPath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        GenerateInputTypeCases(offCircleRandomTestPath, numberTestsPerType, testSizes, 
        CreateRandomFunc(RandomFuncs.DisturbedCircle, random, DISTURBFACTOR));
    }

    private static void GenerateInputTypeCases(string folderPath, int numberTests, 
        int[] testSizes, Func<(Rational, Rational)> coordinateGenerator)
    {
        for(int i = 0; i<testSizes.Length; i++){
            string geoSeriesTestPath = Path.Join(folderPath,  $"n-{testSizes[i]}\\");
            FileInfo fi = new FileInfo(geoSeriesTestPath);
            if (!fi.Directory.Exists) 
                System.IO.Directory.CreateDirectory(fi.DirectoryName); 
            GenerateTestCase<Rational>(geoSeriesTestPath, numberTests, testSizes[i], coordinateGenerator);
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
                sw.WriteLine("0 0");
                sw.WriteLine("1 1");
                sw.WriteLine("0 1");
                sw.WriteLine("1 0");
                for (int j = 0; j < pointsPerTest-4; j++)
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
        DisturbedCircle, // Args expected: (Scale factor numerator), (Scale factor denominator) 
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
            var resX = x+deltaX*scaleFactor;
            var resY = y+deltaY*scaleFactor;

            return (resX, resY);
        }
        return Funky;
    }

}
