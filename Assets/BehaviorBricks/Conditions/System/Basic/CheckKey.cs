using Pada1.BBCore.Framework;
using Pada1.BBCore;
using UnityEngine;

namespace BBCore.Conditions
{
    /// <summary>
    /// It is a basic condition to check if a key has been pressed.
    /// </summary>
    [Condition("Basic/CheckKey")]
    [Help("Checks whether a key is pressed")]
    public class CheckKey : ConditionBase
    {
        ///<value>Input key expected to be pressed Parameter, KeyCode.None by default.</value>
        [InParam("key", DefaultValue = KeyCode.None)]
        [Help("Key expected to be pressed")]
        public KeyCode key = KeyCode.None;
        /*
        public enum MouseAction {down, up, during}
        [InParam("mouseAction", DefaultValue = MouseAction.during)]
        public MouseAction mouseAction = MouseAction.during;*/


        /// <summary>
        /// Checks whether the key is pressed.
        /// </summary>
        /// <returns>True if the key is pressed.</returns>
        public override bool Check()
        {
            /*switch (mouseAction)
            {
                case MouseAction.down:
                    return Input.GetMouseButtonDown(button);

                case MouseAction.up:
                    return Input.GetMouseButtonUp(button);

                case MouseAction.during:*/
                    return Input.GetKey(key);
            /*}
            return false;*/
		}
    }
}