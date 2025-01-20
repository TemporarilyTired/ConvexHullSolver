using ConvexHullSolver;
using System.Numerics;

ConvexHullAlgorithm jm = new JarvisMarch();
ConvexHullAlgorithm gs = new GrahamScan();

TestConvexHull<double>(jm, "000.txt");
TestConvexHull<double>(gs, "001.txt");

Console.WriteLine("Program terminated"); Console.ReadKey();

static void TestConvexHull<T>(ConvexHullAlgorithm solver, string file) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>
{
    // Read input file
    List<(T, T)> points = InputReader<T>.ReadFile(file);

    // for debug: print input
    Console.WriteLine("Input: ");
    foreach (var point in points)
        Console.WriteLine(point.ToString());

    // Start stopwatch
    var watch = System.Diagnostics.Stopwatch.StartNew();

    // Calculate CH
    List<(T, T)> convexHull = solver.CalculateConvexHull<T>(points);

    // Stop watch
    watch.Stop();
    var elapsedMS = watch.ElapsedMilliseconds;

    // Report results
    Console.WriteLine("Jarvis march took {0} ms", elapsedMS);
    Console.WriteLine("Calculated convex hull:");
    foreach (var point in convexHull)
        Console.WriteLine(point.ToString());

    // TODO: verify results maybe?
}