﻿using SourceWrappers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtSourceWrapper {
	public class PostWrapperWrapper : ISubmissionWrapper {
		private IPostWrapper _post;

		public PostWrapperWrapper(IPostWrapper post) {
			_post = post ?? throw new ArgumentNullException(nameof(post));
		}

		public string Title => _post.Title;
		public string HTMLDescription => _post.HTMLDescription;
		public bool Mature => _post.Mature;
		public bool Adult => _post.Adult;
		public IEnumerable<string> Tags => _post.Tags;
		public DateTime Timestamp => _post.Timestamp;
		public string ViewURL => _post.ViewURL;
		public string ImageURL => _post.ImageURL;
		public string ThumbnailURL => _post.ThumbnailURL;
		public Color? BorderColor => null;
	}

	public class SourceWrapperWrapper<TCursor> : SiteWrapper<PostWrapperWrapper, TCursor> where TCursor : struct {
		private readonly ISourceWrapper<TCursor> _source;

		public SourceWrapperWrapper(ISourceWrapper<TCursor> source) {
			_source = source ?? throw new ArgumentNullException(nameof(source));
			BatchSize = _source.SuggestedBatchSize;
		}

		public override string WrapperName => _source.Name;
		public override bool SubmissionsFiltered => true;

		public override int BatchSize { get; set; }

		public override int MinBatchSize => 1;
		public override int MaxBatchSize => 200;

		public override Task<string> WhoamiAsync() {
			return _source.WhoamiAsync();
		}

		public override Task<string> GetUserIconAsync(int size) {
			return _source.GetUserIconAsync(size);
		}

		protected async override Task<InternalFetchResult> InternalFetchAsync(TCursor? startPosition, int count) {
			var got = startPosition is TCursor cursor
				? await _source.MoreAsync(cursor, count)
				: await _source.StartAsync(count);
			return new InternalFetchResult(got.Posts.Select(w => new PostWrapperWrapper(w)), got.Next, !got.HasMore);
		}
	}
}