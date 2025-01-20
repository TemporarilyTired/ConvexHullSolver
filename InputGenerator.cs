using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConvexHullSolver;

internal static class InputGenerator
{
    public static void GenerateInput(string folderPath, int numberTests, int pointsPerTest, Func<Rational> randomFunc)
    {
        if (!Path.EndsInDirectorySeparator(folderPath))
            throw new ArgumentException("Path given isn't a folder");

        for (int i = 0; i < numberTests; i++)
        {
            string newFilePath = Path.Join(folderPath, i.ToString() + ".txt");
            StreamWriter sw = new(newFilePath);
            for (int j = 0; j < pointsPerTest; j++)
            {
                Rational x = randomFunc();
                Rational y = randomFunc();
                //To prevent the entire string from having to exist in memory we can write the coordinates separately
                sw.Write(x);
                sw.Write(" ");
                sw.WriteLine(y);
                sw.Flush();
            }
            sw.Close();
        }
    }


    public static Rational BigRandomRational()
    {
        var rand = new Random();

        return new(1, 1);
    }
}
