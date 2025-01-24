using System.Diagnostics;
using System.Numerics;

namespace ConvexHullSolver;

class TestRunner(string resultPath)
{
    private static AutoResetEvent testStart = new AutoResetEvent(false);
 
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
        {
            tests.AddRange(RunOnDir<T>(folderPath));
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}]: Running on {folderPath}");
        }
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
                "Distribution",
                "Test Case",
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
            // Debug.Assert(nPointsOnCH1 == nPointsOnCH2);
            int nPointsOnCH = Math.Max(nPointsOnCH1, nPointsOnCH2);

            List<string> fileParts = ["NO DIR", "NO DIR", "NO DIR"];
            fileParts.AddRange(folderPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries));
            TestCase testCase = new(fileParts[^2], $"test {i}", nPoints, nPointsOnCH, runs);
            tests.Add(testCase);
        }
        return tests;
    }
    static TestRun TimeConvexHull<T>(IConvexHullAlgorithm convexHullAlgorithm, List<(T, T)> points, out int nPointsOnHull) where T : IParsable<T>, IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IComparable<T>
    {
        // Read input file
        List<(T, T)> pointsCopy = new(points);
        List<(T,T)> convexHull = [];
        CancelToken cancelToken = new();
        testStart.Reset();
        void RunCH (){

            convexHull = convexHullAlgorithm.CalculateConvexHull(points, cancelToken);
            testStart.Set();       
        }

        Thread t = new(RunCH);
        // Start stopwatch
        var watch = Stopwatch.StartNew();
       
        // Calculate CH
        t.Start();
        if(!testStart.WaitOne(10000)){
            //Test takes too long
            cancelToken.Stop = true;
            t.Join();
            TestRun failedRes = new(convexHullAlgorithm.AlgorithmName, null, null);
            nPointsOnHull = -1;
            return failedRes;
        }

        // Stop watch
        watch.Stop();
        var elapsedMS = watch.ElapsedMilliseconds;

        // Verify Results
        bool validity = ConvexHullVerifier.Verify(pointsCopy, convexHull);

        // Report results
        /* Console.WriteLine("{0} took {1} ms{2}",
                          convexHullAlgorithm.AlgorithmName,
                          elapsedMS,
                          validity ? "" : " but was INCORRECT!"); */

        nPointsOnHull = convexHull.Count;
        TestRun res = new(convexHullAlgorithm.AlgorithmName, elapsedMS, validity);
        return res;
    }
}

readonly struct TestCase(string _filePathPart1, string _filePathPart2, int _nPoints, int _nPointsOnCH, TestRun[] _testRuns)
{
    public readonly string filePathPart1 = _filePathPart1;
    public readonly string filePathPart2 = _filePathPart2;
    public readonly int nPoints = _nPoints;
    public readonly int nPointsOnCH = _nPointsOnCH;

    // more properties of the test
    public readonly TestRun[] testRuns = _testRuns;
    
    // public string ToCSVLine2(string separator, TestCase obj){
    //     return string.Join(separator, typeof(TestCase).GetProperties().Select(prop => prop.GetValue(obj).ToString()));
    // }

    public string ToCSVLine(string separator)
    {
        List<string> elems = [
            filePathPart1.ToString(),
            filePathPart2.ToString(),
            nPoints.ToString(),
            nPointsOnCH.ToString()
        ];
        foreach(TestRun t in testRuns)
        {
            if (t.elapsedMS is long time)
                elems.Add(time.ToString());
            else
                elems.Add("DNF");

            if (t.correct is bool correctV)
                elems.Add(correctV.ToString());
            else
                elems.Add("DNF");
        }
        return string.Join(separator, elems);
    }
}

public readonly struct TestRun(string _alg_name, long? _elapsedMS, bool? _correct)
{
    public readonly string algorithm_name = _alg_name;
    public readonly long? elapsedMS = _elapsedMS;
    public readonly bool? correct = _correct;
}

public class CancelToken
{
    public bool Stop = false;
}