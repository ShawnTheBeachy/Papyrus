using System;
using System.Collections.Generic;

namespace Papyrus
{
	public class Metadata : BaseNotify
	{
		#region AlternativeTitle

		private string _alternativeTitle;
		/// <summary>
		/// Term name: "alternative".
		/// An alternative name for the resource.
		/// The distinction between titles and alternative titles is application-specific.
		/// http://purl.org/dc/terms/alternative
		/// </summary>
		public string AlternativeTitle
		{
			get => _alternativeTitle;
			set => Set(ref _alternativeTitle, value);
		}

		#endregion AlternativeTitle

		#region Audience

		private string _audience;
		/// <summary>
		/// Term name: "audience".
		/// A class of entity for whom the resource is intended or useful.
		/// http://purl.org/dc/terms/audience
		/// </summary>
		public string Audience
		{
			get => _audience;
			set => Set(ref _audience, value);
		}

		#endregion Audience

		#region Available

		private DateTime _available;
		/// <summary>
		/// Term name: "available".
		/// Date (often a range) that the resource became or will become available.
		/// http://purl.org/dc/terms/available
		/// </summary>
		public DateTime Available
		{
			get => _available;
			set => Set(ref _available, value);
		}

		#endregion Available

		#region Contributor

		private string _contributor;
		/// <summary>
		/// Term name: "contributor".
		/// An entity responsible for making contributions to the resource.
		/// Examples of a Contributor include a person, an organization, or a service.
		/// http://purl.org/dc/terms/contributor
		/// </summary>
		public string Contributor
		{
			get => _contributor;
			set => Set(ref _contributor, value);
		}

		#endregion Contributor

		#region Created

		private DateTime _created;
		/// <summary>
		/// Date of creation of the resource.
		/// http://purl.org/dc/terms/created
		/// </summary>
		public DateTime Created
		{
			get => _created;
			set => Set(ref _created, value);
		}

		#endregion Created

		#region Creator

		private string _creator;
		/// <summary>
		/// Term name: "creator".
		/// An entity primarily responsible for making the resource.
		/// Examples of a Creator include a person, an organization, or a service.
		/// http://purl.org/dc/terms/creator
		/// </summary>
		public string Creator
		{
			get => _creator;
			set => Set(ref _creator, value);
		}

		#endregion Creator

		#region Dates

		private IList<DateTime> _dates = new List<DateTime>();
		/// <summary>
		/// Term name: "date".
		/// A point or period of time associated with an event in the lifecycle of the resource.
		/// Date may be used to express temporal information at any level of granularity.
		/// Recommended best practice is to use an encoding scheme, such as the W3CDTF profile of ISO 8601 [W3CDTF].
		/// http://purl.org/dc/terms/date
		/// </summary>
		public IList<DateTime> Dates
		{
			get => _dates;
			set => Set(ref _dates, value);
		}

		#endregion Dates

		#region Description

		private string _description;
		/// <summary>
		/// Term name: "description".
		/// An account of the resource.
		/// Description may include but is not limited to: an abstract, a table of contents, 
		/// a graphical representation, or a free-text account of the resource.
		/// http://purl.org/dc/terms/description
		/// </summary>
		public string Description
		{
			get => _description;
			set => Set(ref _description, value);
		}

		#endregion Description

		#region Language

		private string _language;
		/// <summary>
		/// Term name: "language".
		/// A language of the resource.
		/// Recommended best practice is to use a controlled vocabulary such as RFC 4646 [RFC4646].
		/// http://purl.org/dc/terms/language
		/// </summary>
		public string Language
		{
			get => _language;
			set => Set(ref _language, value);
		}

		#endregion Language

		#region Title

		private string _title;
		/// <summary>
		/// Term name: "title".
		/// A name given to the resource.
		/// http://purl.org/dc/terms/title
		/// </summary>
		public string Title
		{
			get => _title;
			set => Set(ref _title, value);
		}

		#endregion Title
	}
}
