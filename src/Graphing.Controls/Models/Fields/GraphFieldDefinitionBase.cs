using System;
using UnitRegistry;

namespace Graphing.Controls.Models
{
    public abstract class GraphFieldDefinitionBase : IGraphFieldDefinition
    {
        public GraphFieldDefinitionBase(string name, string label, Unit unit)
        {
            Name = name;
            Label = label;
            Unit = unit;
        }

        public string Label { get; }
        public string Name { get; }
        public Unit Unit { get; }

        public abstract Array GetValues();
    }
}
