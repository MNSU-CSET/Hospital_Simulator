using Pada1.BBCore.Framework;
using Pada1.BBCore;

namespace BBCore.Conditions
{
    /// <summary>
    /// It is a basic condition to check if Booleans have the same value.
    /// </summary>
    [Condition("Basic/CheckBool")]
    [Help("Checks whether two booleans have the same value")]
    public class CheckBool : ConditionBase
    {
        ///<value>Input First Boolean Parameter.</value>
        [InParam("valueA")]
        [Help("First value to be compared")]
        public bool valueA;

        ///<value>Input Second Boolean Parameter.</value>
        [InParam("valueB")]
        [Help("Second value to be compared")]
        public bool valueB;

        /// <summary>
        /// Checks whether two booleans have the same value.
        /// </summary>
        /// <returns>the value of compare first boolean with the second boolean.</returns>
		public override bool Check()
		{
			return valueA == valueB;
		}
    }
}