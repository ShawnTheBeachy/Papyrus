namespace Papyrus
{
	public class ManifestItem : BaseNotify
    {
		#region ContentLocation

		private string _contentLocation;
		public string ContentLocation
		{
			get => _contentLocation;
			set => Set(ref _contentLocation, value);
		}

		#endregion ContentLocation

		#region Id

		private string _id;
		public string Id
		{
			get => _id;
			set => Set(ref _id, value);
		}

		#endregion Id

		#region MediaType

		private string _mediaType;
		public string MediaType
		{
			get => _mediaType;
			set => Set(ref _mediaType, value);
		}

		#endregion MediaType
	}
}
