namespace Engine.Core.Tests
{
    using NUnit.Framework;
    using Moq;
    using System.Collections.Generic;


    [TestFixture]
    public class SchemeTests
    {
        [Test]
        public void ItShouldAddConceptToScheme()
        {
            var concept = new Concept<int>();
            var scheme = new Scheme<int>();
            scheme.AddConcept(concept);
            Assert.AreEqual(1, scheme.Concepts.Count);
        }

        [Test]
        public void ItShouldDeleteConceptFromScheme()
        {
            var concept = new Concept<int>();
            var scheme = new Scheme<int>();
            scheme.AddConcept(concept);
            scheme.DeleteConcept(concept);
            Assert.AreEqual(0, scheme.Concepts.Count);

        }

        [Test]
        public void ItShouldRunEachConcept()
        {
            var concept1 = new Mock<IConcept>();
            concept1.Setup(x => x.Run()).Verifiable();
            var concept2 = new Mock<IConcept>();
            concept2.Setup(x => x.Run()).Verifiable();
            var scheme = new Scheme<int>();

            scheme.AddConcept(concept1.Object);
            scheme.AddConcept(concept2.Object);

            scheme.Run();

            concept1.Verify(x => x.Run());
            concept2.Verify(x => x.Run());


        }

        [Test]
        public void ItShouldThrowExceptionWhenNoConcepts()
        {
            var scheme = new Scheme<int>();
            Assert.Throws(typeof(SchemeException), () => scheme.Validate());
        }

        [Test]
        public void ItShouldNotValidateWhenConceptsHaveRepeatedSequence()
        {
            var concept1 = new Concept<int> { Sequence = 1 };

            var concept2 = new Concept<int> { Sequence = 1 };

            var scheme = new Scheme<int>();

            scheme.AddConcept(concept1);
            scheme.AddConcept(concept2);

            Assert.IsFalse(scheme.Validate());

        }

        [Test]
        public void ItShouldValidateWhenConceptsHaveNotRepeatSequence()
        {
            var concept1 = new Concept<int> { Sequence = 1 };

            var concept2 = new Concept<int> { Sequence = 2 };

            var scheme = new Scheme<int>();

            scheme.AddConcept(concept1);
            scheme.AddConcept(concept2);

            Assert.IsTrue(scheme.Validate());


        }

        [Test]
        public void ItShouldNotValidateWhenConceptsHaveNoSequence()
        {
            var concept1 = new Concept<int>();

            var concept2 = new Concept<int> { Sequence = 2 };

            var scheme = new Scheme<int>();

            scheme.AddConcept(concept1);
            scheme.AddConcept(concept2);

            Assert.IsFalse(scheme.Validate());
        }

        [Test]
        public void ItShouldRunEachConceptInProperSequence()
        {
            var concept1 = new Mock<IConcept>();
            concept1.Setup(x => x.Run()).Verifiable();
            concept1.Setup(x => x.Sequence).Returns(1);
            concept1.Setup(x => x.HasSequence()).Returns(true);
            var concept2 = new Mock<IConcept>();
            concept2.Setup(x => x.Run()).Verifiable();
            concept2.Setup(x => x.Sequence).Returns(2);
            concept2.Setup(x => x.HasSequence()).Returns(true);
            var scheme = new Scheme<int>();

            scheme.AddConcept(concept1.Object);
            scheme.AddConcept(concept2.Object);

            scheme.Run();

            int callOrder = 0;
            concept1.Setup(x => x.Run()).Callback(() => Assert.That(callOrder++, Is.EqualTo(0)));
            concept2.Setup(x => x.Run()).Callback(() => Assert.That(callOrder++, Is.EqualTo(1)));


        }

        [Test]
        public void ItShouldGetFunctionsToPopulate()
        {
            var function1 = new Function<int> {Id = 1};
            var function2 = new Function<int> {Id = 2};
            var concept1 = new Concept<int>() {Parameter1 = function1};
            var concept2 = new Concept<int>() {Parameter1 = function2, Parameter2 = function1};
            
            var scheme = new Scheme<int>();
            scheme.AddConcept(concept1);
            scheme.AddConcept(concept2);
            var functions = scheme.GetFunctionsToPopulate();
            Assert.AreEqual(3, functions.Count);
            Assert.AreEqual(1, functions[0]);
        }

        [Test]
        public void ItShouldGetGlobalsToPopulate()
        {
            var global1 = new Global<int> { Id = 1, IsModifiable = true};
            var global2 = new Global<int> { Id = 2, IsModifiable = true};
            var global3 = new Global<int> {Id = 3, IsModifiable = false};
            var concept1 = new Concept<int>() { Parameter1 = global1, Parameter2 = global3};
            var concept2 = new Concept<int>() { Parameter1 = global2, Parameter2 = global1, Parameter3 = global3};

            var scheme = new Scheme<int>();
            scheme.AddConcept(concept1);
            scheme.AddConcept(concept2);
            var functions = scheme.GetModifiableGlobalsToPopulate();
            Assert.AreEqual(3, functions.Count);
            Assert.AreEqual(1, functions[0]);
        }
    }
}


