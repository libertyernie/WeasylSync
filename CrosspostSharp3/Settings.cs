﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Tweetinvi.Models;

namespace CrosspostSharp3 {
	public class Settings {
		public abstract class AccountCredentials {
			public string Username { get; set; }
		}

		public class DeviantArtSettings {
			public string RefreshToken { get; set; }
		}

		public DeviantArtSettings DeviantArt { get; set; }

		public class FurAffinitySettings : AccountCredentials {
			public string b;
			public string a;
		}

		public List<FurAffinitySettings> FurAffinity = new List<FurAffinitySettings>();

		public class TwitterSettings : AccountCredentials {
			public string tokenKey;
			public string tokenSecret;

			public ITwitterCredentials GetCredentials() {
				return new TwitterCredentials(OAuthConsumer.Twitter.CONSUMER_KEY, OAuthConsumer.Twitter.CONSUMER_SECRET, tokenKey, tokenSecret);
			}
		}

		public List<TwitterSettings> Twitter = new List<TwitterSettings>();

		public static Settings Load(string filename = "CrosspostSharp.json") {
			Settings s = new Settings();
			if (filename != null && File.Exists(filename)) {
				s = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filename));
			}
			return s;
		}

		public void Save(string filename = "CrosspostSharp.json") {
			File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}