using Newtonsoft.Json;
using System;

namespace Papyrus
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Metadata : BaseNotify
    {
        #region AlternativeTitle

        private string _alternativeTitle = default(string);
        /// <summary>
        /// Term name: "alternative".
        /// An alternative name for the resource.
        /// The distinction between titles and alternative titles is application-specific.
        /// http://purl.org/dc/terms/alternative
        /// </summary>
        [JsonProperty("alternative_title")]
        public string AlternativeTitle { get => _alternativeTitle; set => Set(ref _alternativeTitle, value); }

        #endregion AlternativeTitle

        #region Available

        private DateTime _available = default(DateTime);
        /// <summary>
        /// Term name: "available".
        /// Date (often a range) that the resource became or will become available.
        /// http://purl.org/dc/terms/available
        /// </summary>
        [JsonProperty("available")]
        public DateTime Available { get => _available; set => Set(ref _available, value); }

        #endregion Available

        #region Audience

        private string _audience = default(string);
        /// <summary>
        /// Term name: "audience".
        /// A class of entity for whom the resource is intended or useful.
        /// http://purl.org/dc/terms/audience
        /// </summary>
        [JsonProperty("audience")]
        public string Audience { get => _audience; set => Set(ref _audience, value); }

        #endregion Audience

        #region Contributor

        private string _contributor = default(string);
        /// <summary>
        /// Term name: "contributor".
        /// An entity responsible for making contributions to the resource.
        /// Examples of a Contributor include a person, an organization, or a service.
        /// http://purl.org/dc/terms/contributor
        /// </summary>
        [JsonProperty("contributor")]
        public string Contributor { get => _contributor; set => Set(ref _contributor, value); }

        #endregion Contributor

        #region Creator

        private string _creator = default(string);
        /// <summary>
        /// Term name: "creator".
        /// An entity primarily responsible for making the resource.
        /// Examples of a Creator include a person, an organization, or a service.
        /// http://purl.org/dc/terms/creator
        /// </summary>
        [JsonProperty("creator")]
        public string Creator { get => _creator; set => Set(ref _creator, value); }

        #endregion Creator

        #region Language

        private string _language = default(string);
        /// <summary>
        /// Term name: "language".
        /// A language of the resource.
        /// Recommended best practice is to use a controlled vocabulary such as RFC 4646 [RFC4646].
        /// http://purl.org/dc/terms/language
        /// </summary>
        [JsonProperty("language")]
        public string Language { get => _language; set => Set(ref _language, value); }

        #endregion Language

        #region Title

        private string _title = default(string);
        /// <summary>
        /// Term name: "title".
        /// A name given to the resource.
        /// http://purl.org/dc/terms/title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get => _title; set => Set(ref _title, value); }

        #endregion Title
    }
}
