using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConvexHullSolver;

internal class JarvisMarch : ConvexHullAlgorithm
{
    public override string AlgorithmName { get => "Jarvis March"; }

    public override List<(T x, T y)> CalculateConvexHull<T>(List<(T x, T y)> points)
    {
        List<(T, T)> convexHull = [];

        if (points == null || points.Count == 0) return convexHull;

        {
            // find index of leftmost point
            int leftMostPoint = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].x < points[leftMostPoint].x
                    || points[i].x == points[leftMostPoint].x && points[i].y < points[leftMostPoint].y)
                    leftMostPoint = i;
            }

            // Add leftmost point to CH and remove from other points
            convexHull.Add(points[leftMostPoint]);
            // swap leftmost point to index 0 to compare against later
            (points[0], points[leftMostPoint]) = (points[leftMostPoint], points[0]);
        }

        while (true)
        {
            // Take last CH point to find its successor
            (T x, T y) lastHullPoint = convexHull[convexHull.Count - 1];

            // Iterate all possible successors of the last hull point
            int newHullPoint = 0;
            for (int newPoint = 1; newPoint < points.Count; newPoint++)
            {
                if (lastHullPoint == points[newPoint])
                    continue;

                // Check if a new point lies left of line (last hull point -> new hull point)
                // also accept new if new hull point is equal to the last (when CH only has 1 point)
                if (lastHullPoint == points[newHullPoint]
                    || ConvexHullAlgorithm.Orient2DFast(lastHullPoint, points[newPoint], points[newHullPoint]) < T.AdditiveIdentity)
                {
                    newHullPoint = newPoint;
                }
            }
            // If previous loop did not find another/better hull point,
            // we have either closed the loop or ran out of new points
            if (newHullPoint == 0)
                break;

            // Add new hull point to CH and remove from other points
            convexHull.Add(points[newHullPoint]);
            // Remove newHullPoint by swapping to the end before removing it
            (points[points.Count - 1], points[newHullPoint]) = (points[newHullPoint], points[points.Count - 1]);
            points.RemoveAt(points.Count - 1);
        }
        return convexHull;
    }

    // public static List<(double x, double y)> CalculateConvexHull_OLD(List<(double x, double y)> points)
    // {
    //     if (points == null || points.Count == 0) return new List<(double x, double y)>();

    //     // find index of leftmost point
    //     int pointOnHull = 0;
    //     for (int i = 1; i < points.Count; i++)
    //     {
    //         if (points[i].x < points[pointOnHull].x
    //             || points[i].x == points[pointOnHull].x && points[i].y < points[pointOnHull].y)
    //             pointOnHull = i;
    //     }
    //     // swap leftmost point to index 0
    //     (points[0], points[pointOnHull]) = (points[pointOnHull], points[0]);

    //     // invariable: points [0..pointOnHull] are on the convex hull
    //     for (pointOnHull = 0; pointOnHull < points.Count; pointOnHull++)
    //     {
    //         // Take first hull point as initial comparison
    //         int endPoint = 0;

    //         for (int newPoint = pointOnHull + 1; newPoint < points.Count; newPoint++)
    //         {
    //             if (points[pointOnHull] == points[newPoint])
    //                 continue;

    //             if (pointOnHull == endPoint
    //                || Orient2DFast(points[pointOnHull], points[newPoint], points[endPoint]) < 0)
    //             {
    //                 endPoint = newPoint;
    //             }
    //         }
    //         if (endPoint == 0)
    //             break;

    //         // swap new hull point to end of hull
    //         (points[endPoint], points[pointOnHull + 1]) = (points[pointOnHull + 1], points[endPoint]);
    //     }
    //     return points.Take(pointOnHull + 1).ToList(); //maybe not the fastest way to return the first n elements from a list
    // }

    // Returns positive value if ABC occur in clockwise order (i.e., B is left of AC)
}
