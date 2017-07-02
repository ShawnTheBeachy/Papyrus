namespace Papyrus
{
    public class Metadata : BaseNotify
    {
        #region Title

        private string _title = default(string);
        /// <summary>
        /// The title of this metadata.
        /// </summary>
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
