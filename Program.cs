using ConvexHullSolver;
using System.Numerics;
using System;
using System.IO;
using System.Runtime.CompilerServices;

// string testLoc = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\input-files\\serious-tests\\";
// string allLoc = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\";
// string sosLoc = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\sos-input-files\\";
// string sosLoc2 = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\sos-input-files\\illlllee\\";
// string sosLoc3 = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\sos-input-files\\help\\";
// string bigSuite = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\sos-input-files\\illlllee\\";
// string bigSuite = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\input-files\\big-suite\\";
string smallSuite = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\input-files\\small-suite\\";
string resultLoc = "C:\\Users\\r\\Documents\\myd\\edu\\msc\\ga\\v2\\ConvexHullSolver\\output-files\\";
bool createTests = false;

if (createTests)
    // InputGenerator.GenerateTestCase<double>(sosLoc2, 10, 100, generateDisturbedCircleThing(new Random()));
    InputGenerator.GenerateAllTests(smallSuite, [100, 250, 500, 1000, 1500, 2000, 3000], 10);
    // InputGenerator.GenerateAllTests(testLoc);
else{
    var runner = new TestRunner(resultLoc);
    runner.RunTests<Rational>(smallSuite);
    Console.WriteLine("Program terminated"); // Console.ReadKey();
}

Func<(double, double)> generateCircleThing(Random random){
    (double, double) meh(){
        double angle = 2 * Math.PI * random.NextDouble();
        double xD = Math.Cos(angle);
        double yD = Math.Sin(angle);
        return (xD, yD);
    }
    return meh;
}
Func<(double, double)> generateDisturbedCircleThing(Random random){
    (double, double) meh(){
        double angle = 2 * Math.PI * random.NextDouble();
        double xD = Math.Cos(angle);
        double yD = Math.Sin(angle);
        double angle2 = 2 * Math.PI * random.NextDouble();
        double xD2 = Math.Cos(angle);
        double yD2 = Math.Sin(angle);
        return (xD + xD2 / 1000.0d, yD + yD2 / 1000.0d);
    }
    return meh;
}