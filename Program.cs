using ConvexHullSolver;
using System.Numerics;
using System;
using System.IO;
IConvexHullAlgorithm jm = new JarvisMarch();
IConvexHullAlgorithm gs = new GrahamScan();

// TestConvexHull<double>(jm, "000.txt");
// TestConvexHull<double>(gs, "001.txt");
var random = new Random();

string testLoc = "..\\..\\..\\input-files\\test-test\\";
InputGenerator.GenerateInput(testLoc, 10, 100, InputGenerator.CreateRandomFunc(InputGenerator.RandomFuncs.OnCircle, random, 1));

Console.WriteLine("Program terminated"); // Console.ReadKey();

static void TestConvexHull<T>(IConvexHullAlgorithm convexHullAlgorithm, string file) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>
{
    // Read input file
    List<(T, T)> points = InputReader<T>.ReadFile(file);
    List<(T, T)> pointsCopy = new(points);

    // for debug: print input
    Console.WriteLine("Input: ");
    foreach (var point in points)
        Console.WriteLine(point.ToString());

    // Start stopwatch
    var watch = System.Diagnostics.Stopwatch.StartNew();
   
    // Calculate CH
    List<(T, T)> convexHull = convexHullAlgorithm.CalculateConvexHull<T>(points);

    // Stop watch
    watch.Stop();
    var elapsedMS = watch.ElapsedMilliseconds;

    // Verify Results
    bool validity = ConvexHullVerifier.Verify(pointsCopy, convexHull);
    if(!validity){
        Console.WriteLine("INVALID CH: idk what to do now"); //TODO: do stuff
    }

    // Report results
    Console.WriteLine("Jarvis march took {0} ms", elapsedMS);
    Console.WriteLine("Calculated convex hull:");
    foreach (var point in convexHull)
        Console.WriteLine(point.ToString());

    // TODO: verify results maybe?
}