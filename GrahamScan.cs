﻿using System.Numerics;

namespace ConvexHullSolver;

internal class GrahamScan : IConvexHullAlgorithm
{
    public string AlgorithmName { get => "Graham Scan"; }

    public List<(T x, T y)> CalculateConvexHull<T>(List<(T x, T y)> points, CancelToken cancelToken) where T :
        IComparisonOperators<T, T, Boolean>,
        IEqualityOperators<T, T, Boolean>,
        IMultiplyOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IAdditiveIdentity<T, T>,
        IComparable<T>
    {
        if (points.Count < 3) return points;

        // Sort on X coordinate, then on Y if X is equal
        points.Sort();
        List<(T x, T y)> lUpper = CalculateConvexHullSegment(points, cancelToken);
        points.Reverse();
        List<(T x, T y)> lLower = CalculateConvexHullSegment(points, cancelToken);
        // Remove first and last point from the lower hull because they are in
        // the upper hull as well
        if (lLower.Count > 0) lLower.RemoveAt(0);
        if (lLower.Count > 0) lLower.RemoveAt(lLower.Count - 1);

        return [.. lUpper, .. lLower];
    }

    private static List<(T x, T y)> CalculateConvexHullSegment<T>(List<(T x, T y)> points, CancelToken cancelToken) where T : IComparisonOperators<T, T, Boolean>, IEqualityOperators<T, T, Boolean>, IMultiplyOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        if (points.Count < 3) return points;

        List<(T x, T y)> hullSegment = [];

        for (int i = 0; i < points.Count && !cancelToken.Stop; i++)
        {                                                                                                       
            hullSegment.Add(points[i]);
            while (hullSegment.Count > 2
                && IConvexHullAlgorithm.Orient2DFast(hullSegment[^3], hullSegment[^2], hullSegment[^1]) >= T.AdditiveIdentity)
            {
                hullSegment.RemoveAt(hullSegment.Count - 2);
            }
        }

        return hullSegment;
    }
}
