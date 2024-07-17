using System;

namespace Easing
{
	public static class Ease
	{
		public static double Clamp01(double n) => Math.Clamp(n, 0, 1);
		public static double Lerp(double t, double a, double b) => a + (b-a)*t;
		public static double InverseLerp(double t, double a, double b) => (t-a)/(b-a);
		public static double Smoothstep(double t) => t*t*(3-2*t);
		public static double AngleDistance(double start, double end)
		{
		    double diff = (end - start + Math.PI) % (Math.PI*2) - Math.PI;
		    return diff < -Math.PI ? diff + Math.PI*2 : diff;
		}
	}
}
