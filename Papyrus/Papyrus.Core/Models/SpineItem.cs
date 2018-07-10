namespace Papyrus
{
	public class SpineItem : BaseNotify
	{
		#region IdRef

		private string _idRef;
		public string IdRef
		{
			get => _idRef;
			set => Set(ref _idRef, value);
		}

		#endregion IdRef
	}
}
