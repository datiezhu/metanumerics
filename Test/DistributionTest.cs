﻿using Meta.Numerics;
using Meta.Numerics.Functions;
using Meta.Numerics.Statistics;

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Test
{
    
    
    /// <summary>
    ///This is a test class for DistributionTest and is intended
    ///to contain all DistributionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DistributionTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private Distribution[] distributions = new Distribution[] {
            new UniformDistribution(Interval.FromEndpoints(-2.0,1.0)),
            new NormalDistribution(3.0,2.0),
            new ExponentialDistribution(2.0),
            new ChiSquaredDistribution(3),
            new StudentDistribution(5),
            new LogNormalDistribution(0.2,0.4),
            new KolmogorovDistribution()
        };

        private double[] probabilities = new double[] {
            0.00001, 0.05, 0.5, 0.95, 0.99999
        };

        [TestMethod]
        public void DistributionMedianTest () {
            foreach (Distribution distribution in distributions) {
                Assert.IsTrue(TestUtilities.IsNearlyEqual(distribution.Median, distribution.InverseLeftProbability(0.5)), String.Format("{0} m={1} P(0.5)={2}", distribution.GetType().Name, distribution.Median, distribution.InverseLeftProbability(0.5)));
            }
        }

        [TestMethod]
        public void DistributionMomentsSpecialCasesTest () {
            foreach (Distribution distribution in distributions) {
                Assert.IsTrue(distribution.Moment(0) == 1.0);
                Assert.IsTrue(distribution.Moment(1) == distribution.Mean);
                Assert.IsTrue(distribution.MomentAboutMean(0) == 1.0);
                Assert.IsTrue(distribution.MomentAboutMean(1) == 0.0);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(distribution.MomentAboutMean(2), Math.Pow(distribution.StandardDeviation, 2.0)));
            }
        }

        [TestMethod]
        public void DistributionMomentSumsTest () {
            foreach (Distribution distribution in distributions) {
                // C2 = M2 - M1^2
                Assert.IsTrue(TestUtilities.IsNearlyEqual(distribution.MomentAboutMean(2) + Math.Pow(distribution.Moment(1), 2.0), distribution.Moment(2)), String.Format("{0} C2={1} M1={2} M2={3}",distribution.GetType().Name, distribution.MomentAboutMean(2), distribution.Moment(1), distribution.Moment(2)));
                // C3 = M3 - 3 M2 M1 + 2 M1^3
                Assert.IsTrue(TestUtilities.IsNearlyEqual(distribution.MomentAboutMean(3) + 3.0 * distribution.Moment(2) * distribution.Moment(1), distribution.Moment(3) + 2.0 * Math.Pow(distribution.Moment(1), 3.0)));
                // C4 = M4 - 4 M3 M1 + 6 M2 M1^2 - 3 M1^4
            }
        }

        [TestMethod]
        public void DistributionCentralInequalityTest () {
            foreach (Distribution distribution in distributions) {
                Assert.IsTrue(Math.Abs(distribution.Mean - distribution.Median) <= distribution.StandardDeviation);
            }
        }

        [TestMethod]
        public void DistributionMonotonicityTest () {
            foreach (Distribution distribution in distributions) {
                for (int i = 0; i < (probabilities.Length - 1); i++) {
                    Assert.IsTrue(distribution.InverseLeftProbability(probabilities[i]) < distribution.InverseLeftProbability(probabilities[i+1]));
                }
            }
        }

        [TestMethod]
        public void DistributionProbabilityTest () {
            foreach (Distribution distribution in distributions) {
                // some of these x's will be outside range,
                // but that should just produce zero probability values
                foreach (double x in TestUtilities.GenerateRealValues(1.0E-2, 1.0E2, 5)) {
                    DistributionProbabilityTestHelper(distribution, x);
                    DistributionProbabilityTestHelper(distribution, -x);
                }
            }
        }

        private void DistributionProbabilityTestHelper (Distribution distribution, double x) {
            double P = distribution.LeftProbability(x);
            double Q = distribution.RightProbability(x);
            Assert.IsTrue((0.0 <= P) && (P <= 1.0));
            Assert.IsTrue((0.0 <= Q) && (Q <= 1.0));
            Assert.IsTrue(TestUtilities.IsNearlyEqual(P + Q, 1.0));
            // this is a rather poor test; we can do much more when we get integration
            double p = distribution.ProbabilityDensity(x);
            Assert.IsTrue(p >= 0.0);
        }

        [TestMethod]
        public void DistributionUnitarityIntegralTest () {
            foreach (Distribution distribution in distributions) {
                double M0 = FunctionMath.Integrate(distribution.ProbabilityDensity, distribution.Support);
                Console.WriteLine("{0} 1 {1}", distribution.GetType().Name, M0);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M0, 1.0));
            }
        }

        [TestMethod]
        public void DistributionMeanIntegralTest () {
            foreach (Distribution distribution in distributions) {
                Function<double, double> f = delegate(double x) {
                    return (distribution.ProbabilityDensity(x) * x);
                };
                double M1 = FunctionMath.Integrate(f, distribution.Support);
                Console.WriteLine("{0} {1} {2}", distribution.GetType().Name, distribution.Mean, M1);
                if (distribution.Mean == 0.0) {
                    Assert.IsTrue(Math.Abs(M1) <= TestUtilities.TargetPrecision);
                } else {
                    Assert.IsTrue(TestUtilities.IsNearlyEqual(M1, distribution.Mean));
                }
            }
        }

        // test variance
        [TestMethod]
        public void DistributionVarianceIntegralTest () {
            foreach (Distribution distribution in distributions) {
                Function<double, double> f = delegate(double x) {
                    double z = x - distribution.Mean;
                    return (distribution.ProbabilityDensity(x) * z * z);
                };
                double C2 = FunctionMath.Integrate(f, distribution.Support);
                Console.WriteLine("{0} {1} {2}", distribution.GetType().Name, distribution.StandardDeviation, Math.Sqrt(C2));
                Assert.IsTrue(TestUtilities.IsNearlyEqual(Math.Sqrt(C2), distribution.StandardDeviation));
            }
        }

        // test higher raw moments

        // test higher central moments

        // test P values
        [TestMethod]
        public void DistributionProbabilityIntegralTest () {
            Random rng = new Random(1);
            foreach (Distribution distribution in distributions) {
                for (int i = 0; i < 3; i++) {
                    double x;
                    if (Double.IsNegativeInfinity(distribution.Support.LeftEndpoint) && Double.IsPositiveInfinity(distribution.Support.RightEndpoint)) {
                        // pick an exponentially distributed random point with a random sign
                        double y = rng.NextDouble();
                        x = - Math.Log(y);
                        if (rng.NextDouble() < 0.5) x = -x;
                    } else if (Double.IsPositiveInfinity(distribution.Support.RightEndpoint)) {
                        // pick an exponentialy distributed random point
                        double y = rng.NextDouble();
                        x = distribution.Support.LeftEndpoint - Math.Log(y);
                    } else {
                        // pick a random point within the support
                        x = distribution.Support.LeftEndpoint + rng.NextDouble() * distribution.Support.Width;
                    }
                    Console.WriteLine("{0} {1}", distribution.GetType().Name, x);
                    double P = FunctionMath.Integrate(distribution.ProbabilityDensity, Interval.FromEndpoints(distribution.Support.LeftEndpoint, x));
                    double Q = FunctionMath.Integrate(distribution.ProbabilityDensity, Interval.FromEndpoints(x, distribution.Support.RightEndpoint));
                    Assert.IsTrue(TestUtilities.IsNearlyEqual(P, distribution.LeftProbability(x)));
                    Assert.IsTrue(TestUtilities.IsNearlyEqual(Q, distribution.RightProbability(x)));
                }

            }
        }

    }
}
