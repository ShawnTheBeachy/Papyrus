namespace Papyrus
{
    public class ManifestItem : PapyrusItem
    {
        #region ContentLocation

        private string _contentLocation = default(string);
        public string ContentLocation { get => _contentLocation; set => Set(ref _contentLocation, value); }

        #endregion ContentLocation

        #region Id

        private string _id = default(string);
        public string Id { get => _id; set => Set(ref _id, value); }

        #endregion Id

        #region MediaType

        private string _mediaType = default(string);
        public string MediaType { get => _mediaType; set => Set(ref _mediaType, value); }

        #endregion MediaType
    }
}
