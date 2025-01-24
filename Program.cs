﻿using ConvexHullSolver;

string inputPath = "The absolute path to the input folder here";
string resultPath = "The absolute path to the output folder here";
bool createTests = false;

if (createTests)
    InputGenerator.GenerateAllTests(inputPath, [100, 250, 500, 1000, 1500, 2000, 3000], 10);
else{
    var runner = new TestRunner(resultPath);
    runner.RunTests<Rational>(inputPath);
    Console.WriteLine("Program terminated");
}