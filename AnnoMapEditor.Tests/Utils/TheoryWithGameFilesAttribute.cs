namespace AnnoMapEditor.Tests.Utils
{
    internal class TheoryWithGameFilesAttribute : TheoryAttribute
    {
        /// <summary>
        /// This Theory will be skipped if the game files are not automatically found by the App Settings.
        /// </summary>
        public TheoryWithGameFilesAttribute()
        {
            //Use a timer to show us how long we had to wait until we knew that we weren't gonna run this theory :D
            System.Diagnostics.Stopwatch waitTimer = new ();
            waitTimer.Start();
            Utilities.Settings.Instance.WaitForLoadingBlocking();
            waitTimer.Stop();

            if (!Utilities.Settings.Instance.IsValidDataPath)
            {
                Skip = "The curring test environment has not detected the game files required for this unit test. " +
                    $"Waited {waitTimer.ElapsedMilliseconds}ms for this information...";
            }
        }
    }
}
