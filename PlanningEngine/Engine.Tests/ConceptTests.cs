namespace Engine.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConceptTests
    {
        [Test]
        public void ItShouldNotFilterWhenNoFilter()
        {
            var concept = new Concept<int>();
            Assert.IsTrue(concept.Filter(Month.January));
        }

        [Test]
        public void ItShouldNotFilterWhenFilterFail()
        {
            var filter = new Filter<int, bool>("{0} < {1}")
                             {
                                 Parameter1 = SetDictionary("0", 10) ,
                                 Parameter2 = SetDictionary("1", 1) 
                             };
            var concept = new Concept<int>();
            concept.AddFilter(filter);
            Assert.IsFalse(concept.Filter(Month.January));
        }

        [Test]
        public void ItShouldFilterWhenFilterSuccess()
        {
            var filter = new Filter<int, bool>("{0} > {1}")
                             {
                                 Parameter1 = SetDictionary("0", 10),
                                 Parameter2 = SetDictionary("1", 1)
                             };
            var concept = new Concept<int>();
            concept.AddFilter(filter);
            Assert.IsTrue(concept.Filter(Month.January));

        }

        [Test]
        public void ItShouldFilterComplexAndFilter()
        {
            var filter1 = new Filter<int, bool>("{0} > {1}") {Sequence = 1};
            filter1.SetParameters(new List<IMonthlyParameter<int>>{SetDictionary("0", 10), SetDictionary("1", 1)});
            var filter2 = new Filter<int, bool>("{2} = {3}") { Sequence = 2 };
            filter2.SetConnector("AND");
            filter2.SetParameters(new List<IMonthlyParameter<int>>{SetDictionary("2", 10), SetDictionary("3", 10)});
            var concept = new Concept<int>();
            concept.AddFilter(filter1);
            concept.AddFilter(filter2);
            Assert.IsTrue(concept.Filter(Month.January));

        }

        [Test]
        public void ItShouldNotFilterComplexAndFiltersWhenFirstNotFilter()
        {
            var filter1 = new Filter<int, bool>("{0} > {1}") { Sequence = 1 };
            filter1.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 20) });
            var filter2 = new Filter<int, bool>("{2} = {3}") { Sequence = 2 };
            filter2.SetConnector("AND");
            filter2.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("2", 10), SetDictionary("3", 10) });
            var concept = new Concept<int>();
            concept.AddFilter(filter1);
            concept.AddFilter(filter2);
            Assert.IsFalse(concept.Filter(Month.January));

            
        }

        [Test]
        public void ItShouldNotFilterComplexAndFiltersWhenSecondNotFilter()
        {
            var filter1 = new Filter<int, bool>("{0} > {1}") { Sequence = 1 };
            filter1.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 2) });
            var filter2 = new Filter<int, bool>("{2} = {3}") { Sequence = 2 };
            filter2.SetConnector("AND");
            filter2.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("2", 10), SetDictionary("3", 20) });
            var concept = new Concept<int>();
            concept.AddFilter(filter1);
            concept.AddFilter(filter2);
            Assert.IsFalse(concept.Filter(Month.January));


        }

        [Test]
        public void ItShouldFilterComplexOrFilter()
        {
            var filter1 = new Filter<int, bool>("{0} > {1}") { Sequence = 1 };
            filter1.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var filter2 = new Filter<int, bool>("{2} = {3}") { Sequence = 2 };
            filter2.SetConnector("OR");
            filter2.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("2", 10), SetDictionary("3", 10) });
            var concept = new Concept<int>();
            concept.AddFilter(filter1);
            concept.AddFilter(filter2);
            Assert.IsTrue(concept.Filter(Month.January));
        }

        [Test]
        public void ItShouldFilterComplexOrFilterWhenFirstTrueSecondFalse()
        {
            var filter1 = new Filter<int, bool>("{0} > {1}") { Sequence = 1 };
            filter1.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var filter2 = new Filter<int, bool>("{2} = {3}") { Sequence = 2 };
            filter2.SetConnector("OR");
            filter2.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("2", 10), SetDictionary("3", 20) });
            var concept = new Concept<int>();
            concept.AddFilter(filter1);
            concept.AddFilter(filter2);
            Assert.IsTrue(concept.Filter(Month.July));
        }
        //[Test]
        //public void ItShoudThrowExceptionWhenTryingToRunWhenNotFilter()
        //{
        //    var filter = new Filter<int, bool>("{0} < {1}")
        //                     {
        //                         I = new Parameter<int> { Value = 10 },
        //                         J = new Parameter<int> { Value = 1 }
        //                     };
        //    var concept = new Concept<int>();
        //    concept.AddFilter(filter);
        //    Assert.Throws(typeof(RuleException), concept.Run);

        //}

        [Test]
        public void ItShouldRunWhenNotFiltersAndOperation()
        {
            var concept = new Concept<int>();
            var operation = new Mock<IRule<int, int>>();
            operation.Setup(x => x.GetResult()).Returns(SetDictionary("0", 1));
            concept.AddOperation(operation.Object);
            Assert.DoesNotThrow(concept.Run);
        }

        [Test]
        public void ItShouldRunWhenFilterAndOperations()
        {
            var filter = new Filter<int, bool>("{0} > {1}")
                             {
                                 Parameter1 = SetDictionary("0", 10 ),
                                 Parameter2 = SetDictionary("1", 1) 
                             };
            var concept = new Concept<int>();
            concept.AddFilter(filter);
            var operation = new Mock<IRule<int, int>>();
            operation.Setup(x => x.GetResult()).Returns(SetDictionary("0", 1));
            concept.AddOperation(operation.Object);
            Assert.DoesNotThrow(concept.Run);
        }

        [Test]
        public void ItShouldNotRunWhenNotFiltersAndNotOperation()
        {
            var concept = new Concept<int>();
            Assert.Throws(typeof(RuleException), concept.Run);
        }

        [Test]
        public void ItShouldGet15OnOutput()
        {
            var filter = new Filter<int, bool>("{0} > {1}")
                             {
                                 Parameter1 = SetDictionary("0", 10),
                                 Parameter2 = SetDictionary("1", 1) 
                             };
            var concept = new Concept<int>();
            concept.AddFilter(filter);
            var operation = new Mock<IRule<int, int>>();
            operation.Setup(x => x.GetResult()).Returns(SetDictionary("0", 15));
            concept.AddOperation(operation.Object);
            concept.Run();
            Assert.AreEqual(15, concept.Output1.Value[Month.March]);
        }

        [Test]
        public void ItShouldGet16OnComplexOperation()
        {
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})");
            concept.AddFilter(filter);
            concept.SetParameters(
                new List<IMonthlyParameter<int>> { SetDictionary("0", 2), SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });
            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
        }

        [Test]
        public void ItShouldReplaceOutputValue()
        {
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var concept = new Concept<int>();
            concept.AddFilter(filter);
            const string text = "({0} * {1}) + ({2} * {3})";
            var operation = new RuleTree<int, int>(text);
            operation.SetParameters(
                new List<IMonthlyParameter<int>> { SetDictionary("0", 2), SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });
            concept.AddOperation(operation);
            var output = SetDictionary("output", 25);
            concept.AddOutputParameter1(output);

            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
            Assert.AreEqual(16, output.Value[Month.August]);
        }

        [Test]
        public void ItShouldAddValueToOutputParameter()
        {
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})");
            concept.AddFilter(filter);
            concept.SetParameters(
                new List<IMonthlyParameter<int>> { SetDictionary("0", 2), SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });
            var output = SetDictionary("output", 10);
            output.IsAccumulator = true;
            concept.AddOutputParameter1(output);

            concept.Run();
            Assert.AreEqual(26, concept.Output1.Value[Month.June]);
            Assert.AreEqual(26, output.Value[Month.February]);
        }

        [Test]
        public void ItShouldRunWhenValid()
        {
            var concept = new Concept<int>();
            var operation = new Mock<IRule<int, int>>();
            operation.Setup(x => x.GetResult()).Returns(SetDictionary("0", 1));
            concept.AddOperation(operation.Object);
            Assert.DoesNotThrow(concept.Run);
        }

        [Test]
        public void ItShouldGetDifferentValuesForJanuary()
        {
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})");
            concept.AddFilter(filter);
            var january = SetDictionary("0", 2);
            january.Value[Month.January] = 3;
            concept.SetParameters(
                new List<IMonthlyParameter<int>> { january, SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });
            var output = SetDictionary("output", 25);
            concept.AddOutputParameter1(output);

            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
            Assert.AreEqual(16, output.Value[Month.August]);
            Assert.AreEqual(19, concept.Output1.Value[Month.January]);
            Assert.AreEqual(19, output.Value[Month.January]);
        }


        [Test]
        public void ItShouldCreateOperationFromConcept()
        {
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})");
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10),SetDictionary("1", 1) });
            concept.AddFilter(filter);
            var january = SetDictionary("0", 2);
            january.Value[Month.January] = 3;
            concept.SetParameters(new List<IMonthlyParameter<int>>
                                      {january, SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5)});
            var output = SetDictionary("output", 25);
            concept.AddOutputParameter1(output);

            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
            Assert.AreEqual(16, output.Value[Month.August]);
            Assert.AreEqual(19, concept.Output1.Value[Month.January]);
            Assert.AreEqual(19, output.Value[Month.January]);

        }

        [Test]
        public void ItShouldKeepOutputName()
        {
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})") {Title = "Concept"};
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            concept.AddFilter(filter);
            var january = SetDictionary("0", 2);
            january.Value[Month.January] = 3;
            concept.SetParameters(new List<IMonthlyParameter<int>> { january, SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });
            var output = SetDictionary("output", 25);
            concept.AddOutputParameter1(output);

            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
            Assert.AreEqual(16, output.Value[Month.August]);
            Assert.AreEqual(19, concept.Output1.Value[Month.January]);
            Assert.AreEqual(19, output.Value[Month.January]);
            Assert.AreEqual("output", output.Name);

        }

        [Test]
        public void ItShouldNameOutputWithConceptTitleWhenNoName()
        {
            var concept = new Concept<int>("({0} * {1}) + ({2} * {3})") { Title = "Concept" };
            var filter = new Filter<int, bool>("{0} > {1}");
            filter.SetParameters(new List<IMonthlyParameter<int>> { SetDictionary("0", 10), SetDictionary("1", 1) });
            concept.AddFilter(filter);
            var january = SetDictionary("0", 2);
            january.Value[Month.January] = 3;
            concept.SetParameters(new List<IMonthlyParameter<int>> { january, SetDictionary("1", 3), SetDictionary("2", 2), SetDictionary("3", 5) });


            concept.Run();
            Assert.AreEqual(16, concept.Output1.Value[Month.May]);
            Assert.AreEqual(19, concept.Output1.Value[Month.January]);
            Assert.AreEqual("Concept", concept.Output1.Name);

        }

        private IMonthlyParameter<int> SetDictionary(string name, int value)
        {
            var result = new MonthlyParameter<int> { Name = name };
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                result.Value[month] = value;
            }
            return result;
        }

        private Dictionary<Month, int> SetDictionary(int value)
        {
            var result = new Dictionary<Month, int>();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                result[month] = value;
            }
            return result;
        }

    }
}
