using System;
using System.Collections.Generic;
using Graphing.Core.Pie.Models;
using NUnit.Framework;
using UnitRegistry;
using UnitRegistry.Formatting;

namespace Graphing.Core.Tests
{
    [TestFixture]
    public class PieGraphModelTests
    {
        [Test]
        public void PieSliceModel_StoresLabel()
        {
            var model = new PieSliceModel("id-1", "Fuel", 42.0);

            Assert.That(model.Label, Is.EqualTo("Fuel"));
        }

        [Test]
        public void PieSliceModel_StoresValue()
        {
            var model = new PieSliceModel("id-1", "Fuel", 42.0);

            Assert.That(model.Value, Is.EqualTo(42.0));
        }

        [Test]
        public void PieGraphModel_StoresTitleUnitFormatterAndSlices()
        {
            var formatter = new NumericFormatter("pie-fmt", UnitsRegistry.Default, " ", "F2");
            var slices = new IPieSliceModel[]
            {
                new PieSliceModel("id-1", "Fuel", 60.0),
                new PieSliceModel("id-2", "Mass", 40.0)
            };

            var model = new PieGraphModel("Mission Distribution", Units.Length.Meter, formatter, slices);

            Assert.That(model.Title, Is.EqualTo("Mission Distribution"));
            Assert.That(model.Unit, Is.EqualTo(Units.Length.Meter));
            Assert.That(model.Formatter, Is.SameAs(formatter));
            Assert.That(model.Slices.Count, Is.EqualTo(2));
            Assert.That(model.Slices[0].Label, Is.EqualTo("Fuel"));
            Assert.That(model.Slices[1].Value, Is.EqualTo(40.0));
        }

        [Test]
        public void PieGraphModel_Slices_DefensivelyCopiesInputCollection()
        {
            var formatter = new NumericFormatter("pie-fmt", UnitsRegistry.Default, " ", "F2");
            var source = new List<IPieSliceModel>
            {
                new PieSliceModel("id-1", "Fuel", 60.0)
            };

            var model = new PieGraphModel("Mission Distribution", Units.Length.Meter, formatter, source);
            source.Add(new PieSliceModel("id-2", "Mass", 40.0));

            Assert.That(model.Slices.Count, Is.EqualTo(1));
            Assert.That(model.Slices[0].Label, Is.EqualTo("Fuel"));
        }

        [Test]
        public void PieGraphModel_Slices_IsReadOnlyCollection()
        {
            var formatter = new NumericFormatter("pie-fmt", UnitsRegistry.Default, " ", "F2");
            var model = new PieGraphModel(
                "Mission Distribution",
                Units.Length.Meter,
                formatter,
                new[] { new PieSliceModel("id-1", "Fuel", 60.0) });

            var mutable = (ICollection<IPieSliceModel>)model.Slices;

            Assert.That(() => mutable.Add(new PieSliceModel("id-2", "Mass", 40.0)), Throws.TypeOf<NotSupportedException>());
        }
    }
}
