using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;

namespace ConvexHullSolver;

internal static class ConvexHullVerifier
{
    public static bool Verify<T>(List<(T x, T y)> points, List<(T x, T y)> convexHull) where T :
        IComparisonOperators<T, T, Boolean>,
        IEqualityOperators<T, T, Boolean>,
        IMultiplyOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IAdditiveIdentity<T, T>
    {
        if (convexHull.Count == 0) return points.Count == 0;
        if (convexHull.Count == 1) return points.Any(p => !convexHull[0].Equals(p));

        if (!VerifyClockwiseConvexity(convexHull))
        {
            Console.WriteLine("Convex Hull was not in convex clockwise order");
            return false;
        }
        
        for(int i = 0; i < convexHull.Count; i++)
        {
            if (!VerifyAllRightOfSegment(points, convexHull[i], convexHull[(i + 1) % convexHull.Count]))
            {
                Console.WriteLine("Some segment in convex hull has some other point to its counterclockwise side");
                return false;
            }
        }
        return true;
    }
    public static bool VerifyAllRightOfSegment<T>(List<(T x, T y)> points, (T x, T y) segmentStart, (T x, T y) segmentEnd) where T :
        IComparisonOperators<T, T, Boolean>,
        IEqualityOperators<T, T, Boolean>,
        IMultiplyOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IAdditiveIdentity<T, T>
    {
        var zero = T.AdditiveIdentity;
        (T x, T y) topright;
        (T x, T y) botleft;
        if(segmentStart.y > segmentEnd.y
           || segmentStart.y == segmentEnd.y && segmentStart.x > segmentEnd.x)
        {
            topright = segmentStart;
            botleft = segmentEnd;
        } else {
            topright = segmentEnd;
            botleft = segmentStart;
        }

        for(int i = 0; i < points.Count; i++){
            var curPoint = points[i];
            var orient = IConvexHullAlgorithm.Orient2DFast(
                                                segmentStart, 
                                                segmentEnd,
                                                curPoint);
               
            if (orient < zero) return false; 
            if (orient == zero){
                // point is colinear with segment:
                // check if point is in between segment endpoints
                if (topright.x < curPoint.x &&
                    topright.y < curPoint.y &&
                    botleft.x > curPoint.x &&
                    botleft.y > curPoint.y)
                {
                    // point is not ON segment but
                    // on its extension: invalid CH
                    return false;
                }
            }

        }
        return true;
    }
    public static bool VerifyClockwiseConvexity<T>(List<(T x, T y)> convexHull) where T :
        IComparisonOperators<T, T, Boolean>,
        IEqualityOperators<T, T, Boolean>,
        IMultiplyOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IAdditiveIdentity<T, T>
    {
        var zero = T.AdditiveIdentity;
        if(convexHull.Count < 3) return true;

        for(int i = 0; i < convexHull.Count; i++){
            if(IConvexHullAlgorithm.Orient2DFast(convexHull[i], 
                    convexHull[(i + 1) % convexHull.Count], 
                    convexHull[(i + 1) % convexHull.Count]) <= zero)
            {   
                return false;
            }

        }
        return true;
    }
}