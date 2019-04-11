using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MVCalc;

namespace MVCalcUnitTest
{
	[TestClass]
	public class E80ValueCalcTests
	{
		[TestMethod]
		public void TestE80MaxMoment()
		{
			// 20ft span, 412.5k (per AREMA 2017) per axle = 825k max moment at L/2 +/- 1.25ft (for 20ft = 11.25ft)
			// with 0.1% error allowance
			var analysis = new E80Analysis();
			analysis.Span = 20;
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Dictionary<string, double> vals = analysis.CalculateSingleLocation(11.25);
			double expectedMaxMoment = 825;
			double calculatedMaxMoment = vals["m"];
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.001);
		}

		[TestMethod]
		public void TestThatMaxIsMax()
		{
			// 20ft span, max moment should occur at L/2 +/- 1.25ft (for 20ft = 11.25ft)
			var analysis = new E80Analysis();
			analysis.Span = 20;
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Dictionary<string, double> vals = analysis.CalculateSingleLocation(11.25);
			double maxMoment = vals["m"];
			Dictionary<string, double> valsBefore = analysis.CalculateSingleLocation(11);
			double beforeMoment = valsBefore["m"];
			Dictionary<string, double> valsAfter = analysis.CalculateSingleLocation(11.5);
			double afterMoment = valsAfter["m"];
			Assert.IsTrue(maxMoment > beforeMoment && maxMoment > afterMoment);
		}
		
		[TestMethod]
		public void TestE80ZeroMoment1()
		{
			// moment at 0ft should be zero
			var analysis = new E80Analysis();
			analysis.Span = 20;
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Dictionary<string, double> vals = analysis.CalculateSingleLocation(0);
			double expectedMaxMoment = 0;
			double calculatedMaxMoment = vals["m"];
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.001);
		}

		[TestMethod]
		public void TestE80ZeroMoment2()
		{
			// moment at span end should be zero
			var analysis = new E80Analysis();
			analysis.Span = 20;
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Dictionary<string, double> vals = analysis.CalculateSingleLocation(analysis.Span);
			double expectedMaxMoment = 0;
			double calculatedMaxMoment = vals["m"];
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.001);
		}

		[TestMethod]
		public void TestSpanNotSet()
		{
			// moment at span end should be zero
			var analysis = new E80Analysis();
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Assert.ThrowsException<MissingFieldException>(() => analysis.CalculateSingleLocation(10));
		}

		[TestMethod]
		public void TestIncrementNotSet()
		{
			var analysis = new E80Analysis();
			analysis.ImpactFactor = 1.0;
			analysis.DistFactor = 1.0;
			analysis.Span = 20;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Assert.ThrowsException<MissingFieldException>(() => analysis.CalculateSingleLocation(10));
		}

		[TestMethod]
		public void TestImpactAndDistNotSet()
		{
			// shouldn't throw an error
			var analysis = new E80Analysis();
			analysis.Span = 20;
			analysis.IncrementInches = 1;
			analysis.TrainType = TrainConfig.E80;
			analysis.GetTrain();
			Dictionary<string, double> vals;
			vals = analysis.CalculateSingleLocation(10);
			Assert.IsNotNull(vals);
		}

		[TestMethod]
		public void TestSpanSetter()
		{
			// should not allow setting to a negative number
			var analysis = new E80Analysis();
			analysis.Span = 13;
			analysis.Span = -1;
			Assert.AreEqual(analysis.Span, 13);
		}

		[TestMethod]
		public void TestIncrementSetter()
		{
			// should not allow setting to a negative number
			var analysis = new E80Analysis();
			analysis.IncrementInches = 1;
			analysis.IncrementInches = -1;
			Assert.AreEqual(analysis.IncrementInches, 1);
		}

		[TestMethod]
		public void TestDistFactorSetter()
		{
			// should not allow setting to a negative number
			var analysis = new E80Analysis();
			analysis.DistFactor = 0.5;
			analysis.DistFactor = -0.4;
			Assert.AreEqual(analysis.DistFactor, 0.5);
		}

		[TestMethod]
		public void TestImpactFactorSetter()
		{
			// should not allow setting to a negative number
			var analysis = new E80Analysis();
			analysis.ImpactFactor = 1.33;
			analysis.ImpactFactor = -1.0;
			Assert.AreEqual(analysis.ImpactFactor, 1.33);
		}
	}
}
