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
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.01);
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
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.01);
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
			Assert.AreEqual(expectedMaxMoment, calculatedMaxMoment, 0.01);
		}
	}
}
