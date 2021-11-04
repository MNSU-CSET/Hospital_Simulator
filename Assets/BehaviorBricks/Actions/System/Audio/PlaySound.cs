using Pada1.BBCore.Tasks;
using Pada1.BBCore;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to play a sound in the position of the GameObject.
    /// </summary>
    [Action("Audio/PlaySound")]
    [Help("Plays an audio clip from the game object position")]
    public class PlaySound : GOAction
    {
        /// <summary>All Input Parameters of PlayAnimation action</summary>

        ///<value>The clip that must be played</value>
        [InParam("clip")]
        [Help("The clip that must be played")]
        public AudioClip clip;

        ///<value>Volume of the clip.</value>
        [InParam("volume")]
        [Help("Volume of the clip")]
        public float volume = 1f;

        ///<value>Wheter the action waits till the end of the clip to be completed.</value>
        [InParam("waitUntilFinish")]
        [Help("Wheter the action waits till the end of the clip to be completed")]
        public bool waitUntilFinish = false;

        private float elapsedTime;

        /// <summary>Initialization Method of PlaySound.</summary>
        /// <remarks>Associate the sound clip and inacialize the elapsed time.</remarks>
        public override void OnStart()
        {
            AudioSource.PlayClipAtPoint(clip, gameObject.transform.position, volume);

            elapsedTime = Time.time;
        }
        /// <summary>Method of Update of PlaySound.</summary>
        /// <remarks>Increase the elapsed time and check if the sound clip is finished.</remarks>
        public override TaskStatus OnUpdate()
        {
            elapsedTime += Time.deltaTime;
            if (!waitUntilFinish || elapsedTime >= clip.length)
                return TaskStatus.COMPLETED;
            return TaskStatus.RUNNING;
        }
    }
}
