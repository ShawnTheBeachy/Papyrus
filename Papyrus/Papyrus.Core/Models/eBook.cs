using System.Collections.Generic;

namespace Papyrus
{
	public class EBook : BaseNotify
    {
		public string BaseDirectory { get; }

		public EBook()
		{
			// Empty.
		}

		public EBook(string directory)
		{
			BaseDirectory = directory;
			this.Initialize();
		}

		#region ContentLocation

		private string _contentLocation;
		public string ContentLocation
		{
			get => _contentLocation;
			set => Set(ref _contentLocation, value);
		}

		#endregion ContentLocation

		#region CoverLocation

		private string _coverLocation;
		public string CoverLocation
		{
			get => _coverLocation;
			set => Set(ref _coverLocation, value);
		}

		#endregion CoverLocation

		#region Manifest

		public Dictionary<string, ManifestItem> Manifest { get; } = new Dictionary<string, ManifestItem>();

		#endregion Manifest

		#region Metadata

		private Metadata _metadata;
		public Metadata Metadata
		{
			get => _metadata;
			set => Set(ref _metadata, value);
		}

		#endregion Metadata

		#region Spine

		private Spine _spine;
		public Spine Spine
		{
			get => _spine;
			set => Set(ref _spine, value);
		}

		#endregion Spine

		#region TableOfContents

		private TableOfContents _tableOfContents;
		public TableOfContents TableOfContents
		{
			get => _tableOfContents;
			set => Set(ref _tableOfContents, value);
		}

		#endregion TableOfContents
	}
}
