using System;
using UnitRegistry;

namespace Graphing.Controls.Models
{
    public interface IGraphFieldDefinition
    {
        string Label { get; }

        string Name { get; }

        Unit Unit { get; }

        Array GetValues();
    }
}