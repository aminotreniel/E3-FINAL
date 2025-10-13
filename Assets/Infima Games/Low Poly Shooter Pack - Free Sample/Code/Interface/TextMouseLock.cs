// Copyright 2021, Infima Games. All Rights Reserved.

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// This component handles warning developers whether their mouse is locked or not by
    /// updating a text in the interface.
    /// </summary>
    public class TextMouseLock : ElementText
    {
        #region METHODS

        protected override void Tick()
        {
            //Hide the cursor lock text in-game by clearing it.
            if(textMesh != null)
                textMesh.text = "";
        }

        #endregion
    }
}