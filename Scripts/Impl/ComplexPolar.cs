using Godot;
using System;

public struct ComplexPolar
{

	public double r = 0, theta = 0;
	public ComplexPolar(Complex complex) {
		r = complex.Abs();
		theta = Math.Atan2(complex.imag, complex.real);
	}

	public static implicit operator Complex(ComplexPolar polar) => new Complex(polar.r * Math.Cos(polar.theta), polar.r * Math.Sin(polar.theta));
}
