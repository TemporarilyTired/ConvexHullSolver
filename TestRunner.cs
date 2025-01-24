using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Microsoft.VisualBasic;

namespace ConvexHullSolver;

class TestRunner(string resultPath)
{
    readonly string resultPath = resultPath;
    const string separator = ", ";
    IConvexHullAlgorithm jm = new JarvisMarch();
    IConvexHullAlgorithm gs = new GrahamScan();

    public void RunTests<T>(string folderPath) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparable<T>
    {
        var results = CrawlDir<T>(folderPath);
        WriteCSV(results);
    }

    List<TestCase> CrawlDir<T>(string folderPath) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparable<T>
    {
        List<TestCase> tests = [];
        if(Directory.GetDirectories(folderPath).Length > 0)
            foreach (string directory in Directory.GetDirectories(folderPath))
                tests.AddRange(CrawlDir<T>(directory));
        else
            tests.AddRange(RunOnDir<T>(folderPath));
        return tests;
    }

    void WriteCSV(List<TestCase> testCases)
    {
        string resultFilePath = Path.Join(resultPath, Directory.GetFiles(resultPath).Length.ToString() + ".csv"); 
        FileInfo fi = new(resultFilePath);
        if (!fi.Directory.Exists) 
            System.IO.Directory.CreateDirectory(fi.DirectoryName); 
        
        using (var sw = new StreamWriter(resultFilePath))
        {
            // write header
            string[] header_names = [
                "fp1",
                "fp2",
                "fp3",
                "#Points",
                "#Points on CH",
                jm.AlgorithmName + " runtime (ms)",
                jm.AlgorithmName + " validity",
                gs.AlgorithmName + " runtime (ms)",
                gs.AlgorithmName + " validity"
            ];
            string headerLine = string.Join(separator, header_names);
            sw.WriteLine(headerLine);

            foreach(TestCase tc in testCases)
            {
               string line = tc.ToCSVLine(separator);
               sw.WriteLine(line);
            }
        }
        
        
    }
    List<TestCase> RunOnDir<T>(string folderPath) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparable<T>
    {
        List<TestCase> tests = [];
        for(int i = 1;true;i++)
        {
            string newFilePath = Path.Join(folderPath, i.ToString() + ".txt");
            
            if(!File.Exists(newFilePath))
                break;

            List<(T, T)> points = InputReader<T>.ReadFile(newFilePath);
            List<(T, T)> pointsCopy = new(points);
            int nPoints = points.Count;

            TestRun[] runs = [
                TimeConvexHull(jm, points, out int nPointsOnCH1),
                TimeConvexHull(gs, pointsCopy, out int nPointsOnCH2)
            ];
            int nPointsOnCH = nPointsOnCH1 * 1000 + nPointsOnCH2;//Math.Max(nPointsOnCH1, nPointsOnCH2); 

            List<string> fileParts = ["NO DIR", "NO DIR", "NO DIR"];
            fileParts.AddRange(folderPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries));
            TestCase testCase = new(fileParts[^2], fileParts[^1], $"test {i}", nPoints, nPointsOnCH, runs);
            tests.Add(testCase);
        }
        return tests;
    }
    static TestRun TimeConvexHull<T>(IConvexHullAlgorithm convexHullAlgorithm, List<(T, T)> points, out int nPointsOnHull) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparable<T>
    {
        // Read input file
        List<(T, T)> pointsCopy = new(points);

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
            // throw new InvalidProgramException("Program did not find the convex hull correctly");
        }

        // Report results
        Console.WriteLine("{1} took {0} ms", elapsedMS, convexHullAlgorithm.AlgorithmName);

        nPointsOnHull = convexHull.Count;
        TestRun res = new(convexHullAlgorithm.AlgorithmName, elapsedMS, validity);
        return res;
    }   
}

readonly struct TestCase(string _filePathPart1, string _filePathPart2, string _filePathPart3, int _nPoints, int _nPointsOnCH, TestRun[] _testRuns)
{
    public readonly string filePathPart1 = _filePathPart1;
    public readonly string filePathPart2 = _filePathPart2;
    public readonly string filePathPart3 = _filePathPart3;
    public readonly int nPoints = _nPoints;
    public readonly int nPointsOnCH = _nPointsOnCH;

    // more properties of the test
    public readonly TestRun[] testRuns = _testRuns;
    public string ToCSVLine2(string separator, TestCase obj){
        return string.Join(separator, typeof(TestCase).GetProperties().Select(prop => prop.GetValue(obj).ToString()));
    }

    public string ToCSVLine(string separator)
    {
        List<string> elems = [
            filePathPart1.ToString(),
            filePathPart2.ToString(),
            filePathPart3.ToString(),
            nPoints.ToString(),
            nPointsOnCH.ToString()
        ];
        foreach(TestRun t in testRuns)
        {
            elems.Add(t.elapsedMS.ToString());
            elems.Add(t.correct.ToString());
        }
        return string.Join(separator, elems);
    }
}

public readonly struct TestRun(string _alg_name, long _elapsedMS, bool _correct)
{
    public readonly string algorithm_name = _alg_name;
    public readonly long elapsedMS = _elapsedMS;
    public readonly bool correct = _correct;
}