namespace Engine.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RuleTreeTests
    {
        /// <summary>
        /// Ignored no validation, we need to log
        /// </summary>
        [Ignore]
        [Test]
        public void ItShouldNotValidateWhenNoRules()
        {
            var rule = new RuleTree<int, int>();
            Assert.Throws(typeof (RuleException), () => rule.Validate());
        }

        [Ignore]
        [Test]
        public void ItShouldNotValidateWhenNoOperations()
        {
            var rule = new RuleTree<int, int>
                           {I = new Mock<Rule<int, int>>().Object, J = new Mock<Rule<int, int>>().Object};
            Assert.Throws(typeof(RuleException), () => rule.Validate());
        }

        [Test]
        public void ItShouldvalidateWhenRulesAndOperation()
        {
            var rule = new RuleTree<int, int> { I = new Mock<Rule<int, int>>().Object, J = new Mock<IRule<int, int>>().Object ,
            Operation = new Mock<IOperation>().Object};

            Assert.IsTrue(rule.Validate());
        }

        [Ignore]
        [Test]
        public void ItShouldNotGetExpressionWhenNotValidate()
        {
            var rule = new RuleTree<int, int>();
            Assert.Throws(typeof(RuleException), () => rule.GetExpression());
        }

        [Test]
        public void ItShouldGetExpresionWhenvalidate()
        {
            var i = new Mock<IRule<int, int>>();
            var j = new Mock<IRule<int, int>>();
            i.Setup(x => x.GetResult()).Returns(SetDictionary("1", 1));
            j.Setup(x => x.GetResult()).Returns(SetDictionary("2", 2));
            var operation = new Mock<IOperation>();
            operation.Setup(x => x.GetOperator()).Returns(ExpressionType.Add);

            var rule = new RuleTree<int, int>
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
            var i = new Mock<IRule<int, int>>();
            var j = new Mock<IRule<int, int>>();
            i.Setup(x => x.GetResult()).Returns(SetDictionary("1", 1));
            j.Setup(x => x.GetResult()).Returns(SetDictionary("2", 2));
            var operation = new Mock<IOperation>();
            operation.Setup(x => x.GetOperator()).Returns(ExpressionType.Add);

            var rule = new RuleTree<int, int>
            {
                I = i.Object,
                J = j.Object,
                Operation = operation.Object
            };

            Assert.AreEqual(3, rule.GetResult().Value[Month.November]);
        }


        [Test]
        public void ItShouldParseExpression()
        {
            const string text = "( {0} + {1} ) * ( {2} * 1 )";

            var rule = new RuleTree<int, int>(text);

            var expression = rule.GetExpression();
            Assert.AreEqual("0", expression.Parameters[0].ToString());
            Assert.AreEqual("1", expression.Parameters[1].ToString());
            Assert.AreEqual(ExpressionType.Multiply, expression.Body.NodeType);
        }

        [Test]
        public void ItShouldGet15FromRule()
        {
            const string text = "( {0} + {1} ) * ( {2} * 1 )";

            var rule = new RuleTree<int, int>(text);
            var parameters = new List<IMonthlyParameter<int>> {SetDictionary("0",4), SetDictionary("1", 1), SetDictionary("2", 3)};
            rule.SetParameters(parameters);

            Assert.AreEqual(15, rule.GetResult().Value[Month.February]);
        }

        [Test]
        public void ItShouldGet30FromRule()
        {
            const string text = "( {0} / {1} ) + ({2} * 1)";

            var rule = new RuleTree<int, int>(text);
            var parameters = new List<IMonthlyParameter<int>> { SetDictionary("0", 125), SetDictionary("1", 5), SetDictionary("2", 5) };
            rule.SetParameters(parameters);

            Assert.AreEqual(30, rule.GetResult().Value[Month.August]);
        }

        [Test]
        public void ItShouldParseComplexOperations()
        {
            const string text = "( {0} / 25 ) * 14";

            var rule = new RuleTree<int, int>(text);
            var parameters = new List<IMonthlyParameter<int>> { SetDictionary("0", 250) };
            rule.SetParameters(parameters);

            Assert.AreEqual(140, rule.GetResult().Value[Month.February]);
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
    }
}
