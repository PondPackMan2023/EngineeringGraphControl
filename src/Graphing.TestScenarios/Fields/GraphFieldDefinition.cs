using Graphing.Controls.Models;
using System;
using UnitRegistry;

namespace Graphing.TestScenarios.Fields
{
    internal class GraphFieldDefinition : GraphFieldDefinitionBase
    {
        public GraphFieldDefinition(string name, string label, Unit unit, Array values)
            : base(name, label, unit)
        {
            _values = values;
        }

        public override Array GetValues()
        {
            return _values;
        }

        private Array _values;
    }
}
