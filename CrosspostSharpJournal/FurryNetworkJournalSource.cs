﻿using ArtSourceWrapper;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using FurryNetworkLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CrosspostSharpJournal {
	public class FurryNetworkJournalSource : JournalSource<FurryNetworkJournalWrapper, int>, IJournalDestination {
		private readonly FurryNetworkClient _client;
		private string _preferredCharacterName;
		private string _status;

		public FurryNetworkJournalSource(FurryNetworkClient client, string characterName = null, string journalStatus = "public") {
			_client = client;
			_preferredCharacterName = characterName;
			_status = journalStatus;
		}

		public override int BatchSize { get; set; } = 20;
		public override int MinBatchSize => 1;
		public override int MaxBatchSize => 20;

		public string SiteName => $"Furry Network ({_preferredCharacterName}) ({_status})";

		public Task<string> WhoamiAsync() {
			return Task.FromResult(_preferredCharacterName);
		}

		protected override async Task<InternalFetchResult> InternalFetchAsync(int? startPosition, int maxCount) {
			int page = startPosition ?? 1;
			var response = await _client.GetJournalsAsync(_preferredCharacterName, page, status: _status);
			return new InternalFetchResult(
				response.Results.Select(j => new FurryNetworkJournalWrapper(j)),
				response.Page + 1,
				response.Page_count <= response.Page);
		}

		public async Task PostAsync(string title, string text, string teaser) {
			await _client.PostJournalAsync(_preferredCharacterName, new NewJournal {
				Content = text,
				Description = teaser,
				Status = _status,
				Title = title
			});
		}
	}

	public class FurryNetworkJournalWrapper : IJournalWrapper {
		public readonly Journal Journal;

		public FurryNetworkJournalWrapper(Journal journal) {
			Journal = journal;
		}

		public string Title => Journal.Title;
		public string HTMLDescription => Journal.Content;
		public DateTime Timestamp => Journal.Created;
		public string ViewURL => $"https://beta.furrynetwork.com/submissions/journal/public/{Journal.Id}";
	}
}
