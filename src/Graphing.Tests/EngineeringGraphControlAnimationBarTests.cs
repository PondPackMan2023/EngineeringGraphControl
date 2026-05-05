using System;
using Graphing.Controls;
using Graphing.Controls.Interaction;
using NUnit.Framework;

namespace Graphing.Tests
{
    [TestFixture]
    public class EngineeringGraphControlAnimationBarTests
    {
        [Test]
        public void AnimationBarEnabled_SetValue_UpdatesStateAndInvalidates()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                var invalidatedCount = 0;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.AnimationBarEnabled = true;

                Assert.That(control.AnimationBarEnabled, Is.True);
                Assert.That(invalidatedCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void AnimationBarXIndex_SetNegative_ThrowsArgumentOutOfRange()
        {
            using (var control = new EngineeringGraphControl())
            {
                Assert.That(
                    () => control.AnimationBarXIndex = -1,
                    Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public void AnimationBarXIndex_SetValue_RaisesChangedEventWithExpectedPayload()
        {
            using (var control = new EngineeringGraphControl())
            {
                _ = control.Handle;
                AnimationBarIndexChangedEventArgs captured = null;
                var invalidatedCount = 0;
                control.AnimationBarXIndexChanged += (_, args) => captured = args;
                control.Invalidated += (_, __) => invalidatedCount++;

                control.AnimationBarXIndex = 4;

                Assert.That(control.AnimationBarXIndex, Is.EqualTo(4));
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.XIndex, Is.EqualTo(4));
                Assert.That(captured.PreviousXIndex, Is.Null);
                Assert.That(captured.IsUserInitiated, Is.False);
                Assert.That(invalidatedCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void AnimationBarXIndex_SecondSetValue_IncludesPreviousIndex()
        {
            using (var control = new EngineeringGraphControl())
            {
                AnimationBarIndexChangedEventArgs captured = null;
                control.AnimationBarXIndex = 2;
                control.AnimationBarXIndexChanged += (_, args) => captured = args;

                control.AnimationBarXIndex = 5;

                Assert.That(captured, Is.Not.Null);
                Assert.That(captured.XIndex, Is.EqualTo(5));
                Assert.That(captured.PreviousXIndex, Is.EqualTo(2));
            }
        }

        [Test]
        public void AnimationBarXIndex_SetSameValue_DoesNotRaiseChangedEvent()
        {
            using (var control = new EngineeringGraphControl())
            {
                var raised = 0;
                control.AnimationBarXIndex = 3;
                control.AnimationBarXIndexChanged += (_, __) => raised++;

                control.AnimationBarXIndex = 3;

                Assert.That(raised, Is.EqualTo(0));
            }
        }
    }
}
