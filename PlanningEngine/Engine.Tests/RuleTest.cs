namespace Engine.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RuleTest
    {
        [Test]
        public void ItShouldNotValidateWhenParameterIsMissing()
        {
            IRule<int, int> rule = new Rule<int, int>();
            Assert.Throws(typeof(RuleException), () => rule.Validate());
        }

        [Test]
        public void ItShoudNotValidateWhenOperationIsMissing()
        {
            IRule<int, int> rule = new Rule<int, int> { I = new Mock<IMonthlyParameter<int>>().Object, J = new Mock<IMonthlyParameter<int>>().Object };
            Assert.Throws(typeof(RuleException), () => rule.Validate());
        }

        [Test]
        public void ItShouldValidateWhenParameterAndOperation()
        {
            IRule<int, int> rule = new Rule<int, int>
                             {
                                 I = new Mock<IMonthlyParameter<int>>().Object,
                                 J = new Mock<IMonthlyParameter<int>>().Object,
                                 Operation = new Mock<IOperation>().Object
                             };
            Assert.IsTrue(rule.Validate());
        }

        [Test]
        public void ItShouldNotGetExpressionWhenNotValidate()
        {
            var rule = new Rule<int, int>();
            Assert.Throws(typeof(RuleException), () => rule.GetExpression());
        }

        [Test]
        public void ItShouldGetExpresionWhenvalidate()
        {
            var i = new Mock<IMonthlyParameter<int>>();
            var j = new Mock<IMonthlyParameter<int>>();
            i.Setup(x => x.Value).Returns(SetDictionary(1));
            j.Setup(x => x.Value).Returns(SetDictionary(2));
            var operation = new Mock<IOperation>();
            operation.Setup(x => x.GetOperator()).Returns(ExpressionType.Add);

            var rule = new Rule<int, int>
                             {
                                 I = i.Object,
                                 J = j.Object,
                                 Operation = operation.Object
                             };
            var expression = rule.GetExpression();
            Assert.IsNotNull(expression);
            Assert.AreEqual("0", expression.Parameters[0].ToString());
            Assert.AreEqual("1", expression.Parameters[1].ToString());
            Assert.AreEqual(ExpressionType.Add, expression.Body.NodeType);
        }

        [Test]
        public void ItShouldAddValues()
        {
            var i = new Mock<IMonthlyParameter<int>>();
            var j = new Mock<IMonthlyParameter<int>>();
            i.Setup(x => x.Value).Returns(SetDictionary(1));
            j.Setup(x => x.Value).Returns(SetDictionary(2));
            var operation = new Mock<IOperation>();
            operation.Setup(x => x.GetOperator()).Returns(ExpressionType.Add);

            IRule<int, int> rule = new Rule<int, int>
            {
                I = i.Object,
                J = j.Object,
                Operation = operation.Object
            };

            Assert.AreEqual(3, rule.GetResult().Value[Month.November]);
        }


        [Test]
        public void ItShouldCompareValues()
        {
            var i = new Mock<IMonthlyParameter<int>>();
            var j = new Mock<IMonthlyParameter<int>>();
            i.Setup(x => x.Value).Returns(SetDictionary(1));
            j.Setup(x => x.Value).Returns(SetDictionary(2));
            var operation = new Mock<IOperation>();
            operation.Setup(x => x.GetOperator()).Returns(ExpressionType.GreaterThan);

            IFIlter<bool> filter = new Filter<int, bool>
            {
                Parameter1 = i.Object,
                Parameter2 = j.Object,
                Operation = operation.Object
            };

            Assert.IsFalse(filter.GetResult().Value[Month.June]);
        }

        [Test]
        public void ItShouldCompareTwoRules()
        {
            var i = new Mock<IMonthlyParameter<int>>();
            var j = new Mock<IMonthlyParameter<int>>();
            i.Setup(x => x.Value).Returns(SetDictionary(3));
            j.Setup(x => x.Value).Returns(SetDictionary(2));
            var operation1 = new Mock<IOperation>();
            operation1.Setup(x => x.GetOperator()).Returns(ExpressionType.GreaterThan);

            IFIlter<bool> filter1 = new Filter<int, bool>
            {
                Parameter1 = i.Object,
                Parameter2 = j.Object,
                Operation = operation1.Object
            };

            var y = new Mock<IMonthlyParameter<int>>();
            var z = new Mock<IMonthlyParameter<int>>();
            y.Setup(x => x.Value).Returns(SetDictionary(4));
            z.Setup(x => x.Value).Returns(SetDictionary(3));
            var operation2 = new Mock<IOperation>();
            operation2.Setup(x => x.GetOperator()).Returns(ExpressionType.GreaterThan);

            IFIlter<bool> filter2 = new Filter<int, bool>
            {
                Parameter1 = i.Object,
                Parameter2 = j.Object,
                Operation = operation2.Object
            };

            IFIlter<bool> filter3 = new Filter<bool, bool>
                                            {
                                                Parameter1 = new MonthlyParameter<bool> { Value = filter1.GetResult().Value },
                                                Parameter2 = new MonthlyParameter<bool> { Value = filter2.GetResult().Value },
                                                Operation = new Operation { Operator = ExpressionType.And }

                                            };

            Assert.IsTrue(filter3.GetResult().Value[Month.January]);
        }

        [Test]
        public void ItShouldParseParametersAndOperation()
        {
            const string ruleText = "{0} + {Parameter1}";
            var rule = new Rule<int, int>(ruleText);
            var expression = rule.GetExpression();
            Assert.AreEqual("0", expression.Parameters[0].ToString());
            Assert.AreEqual("1", expression.Parameters[1].ToString());
            Assert.AreEqual(ExpressionType.Add, expression.Body.NodeType);
        }

        [Test]
        public void ItShouldAddFromText()
        {
            const string ruleText = "{0} + {Parameter1}";
            IRule<int, int> rule = new Rule<int, int>(ruleText)
                                       {
                                           I = new MonthlyParameter<int> { Value = SetDictionary(1) }, 
                                           J = new MonthlyParameter<int> { Value = SetDictionary(2) }
                                       };
            Assert.AreEqual(3, rule.GetResult().Value[Month.July]);
        }

        [Test]
        public void ItShouldMultiplyFromText()
        {
            const string ruleText = "{0} * {Parameter1}";
            IRule<int, int> rule = new Rule<int, int>(ruleText)
                                       {
                                           I = new MonthlyParameter<int> { Value = SetDictionary(2) }, 
                                           J = new MonthlyParameter<int> { Value = SetDictionary(2) }
                                       };
            Assert.AreEqual(4, rule.GetResult().Value[Month.January]);
        }

        [Test]
        public void ItShouldParseWithConstant()
        {
            const string ruleText = "{0} * 5";
            var rule = new Rule<int, int>(ruleText) {I = new MonthlyParameter<int> {Value = SetDictionary(2)}};
            Assert.AreEqual(10, rule.GetResult().Value[Month.May]);

        }

        [Test]
        public void ItShouldSetParameterByName()
        {
            const string ruleText = "{0} * 5";
            var rule = new Rule<int, int>(ruleText) { I = new MonthlyParameter<int> { Name = "I", Value = SetDictionary(2) } };
            Assert.AreEqual(10, rule.GetResult().Value[Month.May]);
        }
      

        //[Test]
        //public void ItShouldValidateComplexRules()
        //{
        //    const string text1 = "{0} + {1}";
        //    IRule<int, int> rule1 = new Rule<int, int>(text1);
        //    const string text2 = "{0} * {1}";
        //    IRule<int, int> rule2 = new Rule<int, int>(text2);
        //    IRule<IRule<int, int>, IRule<int, int>> rule = new Rule<IRule<int, int>, IRule<int, int>>
        //    {
        //        I = new MonthlyParameter<IRule<int, int>> { Value = rule1 },
        //        J = new MonthlyParameter<IRule<int, int>> { Value = rule2 },
        //        Operation = new Operation { Operator = ExpressionType.Multiply }
        //    };

        //    Assert.IsTrue(rule.Validate());

        //}

        //[Test]
        //public void ItShouldGet15()
        //{
        //    const string text1 = "{0} + {1}";
        //    IRule<int, int> rule1 = new Rule<int, int>(text1){I = new MonthlyParameter<int>{Value = 1}, J = new MonthlyParameter<int>{Value = 2}};
        //    const string text2 = "{0} * {1}";
        //    IRule<int, int> rule2 = new Rule<int, int>(text2){I = new MonthlyParameter<int>{Value = 1}, J = new MonthlyParameter<int>{Value = 5}};
        //    IRule<int, int> rule = new Rule<int, int>
        //    {
        //        I = new MonthlyParameter<int> { Value = rule1.GetResult() },
        //        J = new MonthlyParameter<int> { Value = rule2.GetResult() },
        //        Operation = new Operation { Operator = ExpressionType.Multiply }
        //    };


        //    Assert.AreEqual(15, rule.GetResult());

        //}


        private Dictionary<Month, int> SetDictionary(int value)
        {
            var result = new Dictionary<Month, int> { };
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                result[month] = value;
            }
            return result;
        }
    }
}
