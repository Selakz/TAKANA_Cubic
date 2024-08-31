using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

namespace Takana3.MusicGame.LevelSelect
{
    public class StartButton : MonoBehaviour
    {
        // Serializable and Public
        public int id = 0;

        // Private

        // Static

        // Defined Function
        public void StartGame()
        {
            SongInfo songInfo = new SongList().GetSongInfo(id);
            LevelInfo levelInfo = new(songInfo, 1);
            InfoReader.SetInfo(levelInfo);
            SceneLoader.LoadScene("Playfield", "Shutter", "SceneLoadDone");
        }

        // System Function

        //private void OnEnable()
        //{
        //    InputSystem.onAnyButtonPress.CallOnce(OnAnyButtonPress);
        //}

        //private void OnAnyButtonPress(InputControl control)
        //{
        //    Debug.Log($"Key {control.displayName} was pressed.");
        //}
    }
}
