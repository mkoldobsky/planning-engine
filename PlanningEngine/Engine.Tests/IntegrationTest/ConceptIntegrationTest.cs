namespace Engine.Core.Tests.IntegrationTest
{
    using NUnit.Framework;
    using System.Collections.Generic;
    using System;

    [TestFixture]
    public class ConceptIntegrationTest
    {
        [Test]
        // 		4001		FTE		1		Sueldo Mensual		10101		Fuera de Conv.		600125		=sueldo anual / 13 * (1 + Merit)		SI		N/A		Merit		Oct. A Sept.		 1.000 	 1.000 	 1.000 	 1.000 	 1.250 	 1.250 	 1.250 	 1.250 	 1.250 	 1.250 	 1.250 	 1.250 	 1.250 	 14.250 			

        public void ConceptoSueldoMensual4001Test()
        {
            Concept<decimal> concept = GetAndRunSueldoMensualConcept();
            Assert.AreEqual(1000M, concept.Output1.Value[Month.October]);
            Assert.AreEqual(1250M, concept.Output1.Value[Month.January]);
            Assert.AreEqual(14250M, concept.Output1.Total);
        }


        [Test]
        // 		4001		FTE		1		Sueldo Mensual Sindicato		10103		En sindicato		600125		=sueldo anual / 13 * (1 + Aum. Sind.)		SI		N/A		Aum. Sind.		Oct. A Sept.		 1.000 	 1.140 	 1.140 	 1.140 	 1.200 	 1.200 	 1.240 	 1.240 	 1.240 	 1.240 	 1.240 	 1.240 	 1.240 	 14.500 			
        public void ConceptoSueldoMensualSindicato()
        {
            Concept<decimal> concept = GetAndRunSueldoMensualSindicalizado();
            Assert.AreEqual(1140M, concept.Output1.Value[Month.October]);
            Assert.AreEqual(1200M, concept.Output1.Value[Month.January]);
            Assert.AreEqual(1240M, concept.Output1.Value[Month.September]);
            Assert.AreEqual(14500M, concept.Output1.Total);
        }


        [Test]
        // 		4001		FTE		1		Comisión		11510		Tiene Com.?		603001		N/A		N/A		Valor Com.		N/A		Dic / Mar / Jun / Oct		 2.000 	 -   	 -   	 500 	 -   	 -   	 500 	 -   	 -   	 500 	 -   	 -   	 500 	 2.000 		Como lo cargamos? Tengo que cargarle 4 comisiones? Una para cada Q por ejemplo?	
        public void ConceptoComision()
        {
            var comision = new Global<decimal>
            {
                Name = "comision",
                Value = new Dictionary<Month, decimal>
                                                         {
                                                             {Month.October, 0},
                                                             {Month.November, 0},
                                                             {Month.December, 500M},
                                                             {Month.January, 0},
                                                             {Month.February, 0},
                                                             {Month.March, 500M},
                                                             {Month.April, 0},
                                                             {Month.May, 0},
                                                             {Month.June, 500M},
                                                             {Month.July, 0},
                                                             {Month.August, 0},
                                                             {Month.September, 500M},

                                                         }
            };

            var concept = new Concept<decimal>("{comision} * 1");
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { comision });

            concept.Run();
            Assert.AreEqual(0M, concept.Output1.Value[Month.October]);
            Assert.AreEqual(500M, concept.Output1.Value[Month.December]);
            Assert.AreEqual(500M, concept.Output1.Value[Month.September]);
            Assert.AreEqual(2000M, concept.Output1.Total);
        }


        //	4001		FTE		1		Bono Cash		17010		Tiene Bono?		870101		Sueldo anual * % Bono 		
        // SI		N/A		% Bono		Enero		 1.300 	 -   	 -   	 1.300 	 -   	 -   	 -   	 -   	 -   	 -   	 -   	 -   	 -   	 1.300 		No forma parte del Full Labor Cost - Si debe quedar como Param. Salida	
        [Test]
        public void BonoCashTest()
        {
            var bonoCash = new Global<decimal>
                               {
                                   Name = "bonoCash",
                                   Value = new Dictionary<Month, decimal>
                                                         {
                                                             {Month.October, 0},
                                                             {Month.November, 0},
                                                             {Month.December, .1M},
                                                             {Month.January, 0},
                                                             {Month.February, 0},
                                                             {Month.March, 0},
                                                             {Month.April, 0},
                                                             {Month.May, 0},
                                                             {Month.June, 0},
                                                             {Month.July, 0},
                                                             {Month.August, 0},
                                                             {Month.September, 0},

                                                         }
                               };


            var concept = new Concept<decimal>("{SueldoAnual} * {bonoCash}");
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { SetDictionary("SueldoAnual", 13000M), bonoCash });

            concept.Run();
            Assert.AreEqual(0M, concept.Output1.Value[Month.October]);
            Assert.AreEqual(1300M, concept.Output1.Value[Month.December]);
            Assert.AreEqual(0, concept.Output1.Value[Month.September]);
            Assert.AreEqual(1300M, concept.Output1.Total);
        }

        [Test]
        public void SpecialBonusConceptTest()
        {
            var specialBonus = new Global<decimal>
            {
                Name = "specialBonus",
                Value = new Dictionary<Month, decimal>
                                                         {
                                                             {Month.October, 0},
                                                             {Month.November, 0},
                                                             {Month.December, 0},
                                                             {Month.January, 0},
                                                             {Month.February, 0},
                                                             {Month.March, 0},
                                                             {Month.April, 0},
                                                             {Month.May, 0},
                                                             {Month.June, 30000M},
                                                             {Month.July, 0},
                                                             {Month.August, 0},
                                                             {Month.September, 0},

                                                         }
            };


            var concept = new Concept<decimal>("{specialBonus} * 1");
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { specialBonus });

            concept.Run();
            Assert.AreEqual(0M, concept.Output1.Value[Month.October]);
            Assert.AreEqual(0, concept.Output1.Value[Month.December]);
            Assert.AreEqual(30000M, concept.Output1.Value[Month.June]);
            Assert.AreEqual(30000M, concept.Output1.Total);
        }

        // 		4001		FTE		2		Aguinaldo		85910		N/A - Todos los ID del esquema		601185		=Sueldo mensual / 12		SI		Parametro Salida Sueldo		N/A		Oct. A Sept.		 1.000 	 178 	 178 	 178 	 204 	 204 	 208 	 208 	 208 	 208 	 208 	 208 	 208 	 2.396 			

        [Test]
        public void AguinaldoTest()
        {
            var sueldoMensual = GetAndRunSueldoMensualConcept();
            var sueldoMensualSindicalizado = GetAndRunSueldoMensualSindicalizado();

            var sumarConcept = AddConcepts(sueldoMensual, sueldoMensualSindicalizado);

            var concept = new Concept<decimal>("{Add} / 12");
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { sumarConcept.Output1 });
            concept.Run();

            Assert.AreEqual(178M, Math.Truncate(concept.Output1.Value[Month.October]));
            Assert.AreEqual(178M, Math.Truncate(concept.Output1.Value[Month.November]));
            Assert.AreEqual(204M, Math.Truncate(concept.Output1.Value[Month.January]));
            Assert.AreEqual(207M, Math.Truncate(concept.Output1.Value[Month.September]));
            Assert.AreEqual(2395M, Math.Truncate(concept.Output1.Total));
        }


        [Test]
        public void PlusVacacionalTest()
        {
            var sueldoMensual = GetAndRunSueldoMensualConcept();
            var sueldoMensualSindicalizado = GetAndRunSueldoMensualSindicalizado();

            var sumarConcept = AddConcepts(sueldoMensual, sueldoMensualSindicalizado);


            var iConcept = new Concept<decimal>("( {Add} / 25 ) * ( 14 * 1 )") { Title = "I" };
            iConcept.SetParameters(new List<IMonthlyParameter<decimal>> { sumarConcept.Output1});
            iConcept.Run();


            var jConcept = new Concept<decimal>("( {Add} / 30 ) * ( 14 * 1 )") { Title = "J" };
            jConcept.SetParameters(new List<IMonthlyParameter<decimal>> { sumarConcept.Output1 });
            jConcept.Run();

            var plusVacional = new Concept<decimal>("{I} - {J}");
            plusVacional.SetParameters(new List<IMonthlyParameter<decimal>> { iConcept.Output1, jConcept.Output1 });
            plusVacional.Run();

            Assert.AreEqual(200M, Math.Round(plusVacional.Output1.Value[Month.October], MidpointRounding.ToEven));
            Assert.AreEqual(229M, Math.Round(plusVacional.Output1.Value[Month.January]));
            Assert.AreEqual(232M, Math.Round(plusVacional.Output1.Value[Month.September]));
            Assert.AreEqual(2683M, Math.Round(plusVacional.Output1.Total));


        }

        private Concept<decimal> AddConcepts(Concept<decimal> a, Concept<decimal> b)
        {
            var sumarConcept = new Concept<decimal>("{" + a.Output1.Name + "} + {" + b.Output1.Name + "}") { Title = "Add" };
            sumarConcept.SetParameters(new List<IMonthlyParameter<decimal>> { a.Output1, b.Output1 });
            sumarConcept.Run();
            return sumarConcept;
        }


        private Concept<decimal> GetAndRunSueldoMensualConcept()
        {
            var merit = new Global<decimal>
                            {
                                Name = "merit",
                                Value = new Dictionary<Month, decimal>
                                            {
                                                {Month.October, 0},
                                                {Month.November, 0},
                                                {Month.December, 0},
                                                {Month.January, .25M},
                                                {Month.February, .25M},
                                                {Month.March, .25M},
                                                {Month.April, .25M},
                                                {Month.May, .25M},
                                                {Month.June, .25M},
                                                {Month.July, .25M},
                                                {Month.August, .25M},
                                                {Month.September, .25M},

                                            }
                            };


            var concept = new Concept<decimal>("({SueldoAnual} / 13) * ({merit} + 1)") { Title = "SueldoMensual" };
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { SetDictionary("SueldoAnual", 13000M), merit });

            concept.Run();
            return concept;
        }

        private Concept<decimal> GetAndRunSueldoMensualSindicalizado()
        {
            var aumSindicato = new Global<decimal>
            {
                Name = "aumSindicato",
                Value = new Dictionary<Month, decimal>
                                                   {
                                                       {Month.October, .14M},
                                                       {Month.November, .14M},
                                                       {Month.December, .14M},
                                                       {Month.January, .20M},
                                                       {Month.February, .20M},
                                                       {Month.March, .24M},
                                                       {Month.April, .24M},
                                                       {Month.May, .24M},
                                                       {Month.June, .24M},
                                                       {Month.July, .24M},
                                                       {Month.August, .24M},
                                                       {Month.September, .24M},

                                                   }
            };


            var concept = new Concept<decimal>("({SueldoAnual} / 13) * ({aumSindicato} + 1)") { Title = "SueldoMensualSindicalizado" };
            concept.SetParameters(new List<IMonthlyParameter<decimal>> { SetDictionary("SueldoAnual", 13000M), aumSindicato });
            concept.Run();
            return concept;
        }

        private IMonthlyParameter<decimal> SetDictionary(string name, decimal value)
        {
            var result = new MonthlyParameter<decimal> { Name = name };
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                result.Value[month] = value;
            }
            return result;
        }
    }
}
