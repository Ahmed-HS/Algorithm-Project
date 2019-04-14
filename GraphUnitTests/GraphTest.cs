using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphUnitTests
{
    [TestClass]
    public class GraphTest
    {
        private FileStream MyResult, Soultion;
        private StreamReader ResultReader, SolutionReader;

        [TestMethod]
        public void QueriesTest()
        {

            string TargetDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\"));
            TargetDirectory += "AlgorithmProject\\bin\\Debug\\";

            MyResult = new FileStream(TargetDirectory + "QueriesResult.txt", FileMode.Open);
            ResultReader = new StreamReader(MyResult); ;
            Soultion = new FileStream(TargetDirectory + "Solution.txt", FileMode.Open);
            SolutionReader = new StreamReader(Soultion);

            string MyOutput = "", Expected = "";

            while (ResultReader.Peek() != -1)
            {
                ResultReader.ReadLine();
                MyOutput += ResultReader.ReadLine();
                SolutionReader.ReadLine();
                Expected += SolutionReader.ReadLine();
                

                ResultReader.ReadLine();
                ResultReader.ReadLine();
                ResultReader.ReadLine();

                SolutionReader.ReadLine();
                SolutionReader.ReadLine();
                SolutionReader.ReadLine();
            }

            bool Verdict = (MyOutput == Expected);

            Assert.IsTrue(Verdict);

        }
    }
}
