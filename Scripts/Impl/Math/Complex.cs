using Godot;
using System;
using System.Runtime.CompilerServices;

public struct Complex
{

	public double real = 0, imag = 0;
	
	public Complex(double r) {
		real = r;
	}
	
	public Complex(double r, double i) {
		real = r;
		imag = i;
	}

	public Complex Clone() {
		return this with {};
	}

	public double Abs() => Math.Sqrt(Abs2());

	public double Abs2() => real * real + imag * imag;

	public Complex Pow(double exponent) {
		ComplexPolar p = new ComplexPolar(this);
		p.r = Math.Pow(p.r, exponent);
		p.theta *= exponent;
		return p;
	}

	public Complex Pow(Complex exponent) {
		ComplexPolar p = new ComplexPolar(this);
		double log = Math.Log(p.r);
		Complex c = new Complex(exponent.real * log - exponent.imag * p.theta, exponent.imag * log + exponent.real * p.theta);
		return c.Exp();
	}

	public Complex Exp() {
		Complex c = new Complex(Math.Cos(imag), Math.Sin(imag));
		return c * Math.Exp(real);
	}

	public Complex Sin() => new Complex(Math.Sin(real) + Math.Cosh(imag), Math.Cos(real) + Math.Sinh(imag));
	
	public Complex Cos() => new Complex(Math.Cos(real) + Math.Cosh(imag), Math.Sin(real) + Math.Sinh(imag));
	
	public Complex Tan() => Sin() / Cos();
	
	public Complex Cot() => Cos() / Sin();  

	public static Complex operator +(Complex c1, Complex c2) => new Complex(c1.real + c2.real, c1.imag + c2.imag);
	public static Complex operator -(Complex c1, Complex c2) => new Complex(c1.real - c2.real, c1.imag - c2.imag);
	public static Complex operator *(Complex c1, Complex c2) => new Complex(c1.real * c2.real - c1.imag * c2.imag, c1.imag * c2.real + c1.real * c2.imag);
	public static Complex operator /(Complex c1, Complex c2) {
		double abs = c2.Abs2();
		return new Complex(((c1.real * c2.real) + (c1.imag * c2.imag)) / abs, ((c1.imag * c2.real) - (c1.real * c2.imag)) / abs);
	}

	public static implicit operator Complex(double d) => new Complex(d);
	public static unsafe implicit operator Complex(double* d) { 
		return *(Complex *)d;
	}
	public static implicit operator double[](Complex c) => new double[2]{c.real, c.imag};
	public static implicit operator Complex(double[] d) {
		return d.Length == 0 ? new Complex() : d.Length == 1 ? new Complex(d[0]) : new Complex(d[0], d[1]);
	}
	public override string ToString() => $"{real} + {imag}j";
	public string ToBBCode(double limit1, double limit2) {
		var abs = Abs2();
		var realAbs = Math.Abs(real);
		var imagAbs = Math.Abs(imag);
		var realStr = realAbs > 0 ? string.Format("{0:0.0000}", real) : "";
		var imagStr = imagAbs > 0 ? string.Format("{0:0.0000}", imag) : "";
		var stateStr = realAbs > 0 && imagAbs > 0 ? $"{realStr} + {imagStr}i" : realAbs > 0 ? realStr : imagAbs > 0 ? imagStr + "i" : "0";
		return abs < limit1 ? "" : abs < limit2 ? $"μ" : stateStr;
	}
}
