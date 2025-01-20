namespace ConvexHullSolver;

static internal class InputReader<T> where T : IParsable<T>
{
    private const string inputFilesDirectory = "..\\..\\..\\input-files\\";
    public static List<(T x, T y)> ReadFile(string filename)
    {
        try
        {
            List<(T x, T y)> res;

            // Create an instance of StreamReader to read from a file.
            using (StreamReader sr = new StreamReader(inputFilesDirectory + filename))
            {
                if (sr is null)
                    throw new Exception("streamreader is null");

                string? line = sr.ReadLine() ?? throw new Exception("first line not readable");

                int nPoints = int.Parse(line);
                res = new List<(T x, T y)>(nPoints);

                // Read and display lines from the file until the end
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splitLine = line.Split(' ');
                    (T, T) xy = (T.Parse(splitLine[0], null), T.Parse(splitLine[1], null));
                    res.Add(xy);
                }

                if (res.Count != nPoints)
                    throw new Exception("Number of points in file not equal to the amount specified at the top of file");
            }
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read: ");
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
