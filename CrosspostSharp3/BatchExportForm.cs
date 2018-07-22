﻿using Newtonsoft.Json;
using SourceWrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrosspostSharp3 {
	public partial class BatchExportForm : Form {
		public BatchExportForm(IEnumerable<IPagedWrapperConsumer> wrappers) {
			InitializeComponent();
			foreach (var w in wrappers) listBox1.Items.Add(w);
		}

		public IEnumerable<IPagedWrapperConsumer> SelectedWrappers {
			get {
				foreach (var o in listBox1.SelectedItems) {
					if (o is IPagedWrapperConsumer w) yield return w;
				}
			}
		}

		private async void btnOk_Click(object sender, EventArgs e) {
			if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) {
				return;
			}

			panel1.Enabled = false;

			progressBar1.Maximum = (int)numericUpDown1.Value;
			progressBar1.Value = 0;

			try {
				if (SelectedWrappers.Count() != 1) {
					throw new NotImplementedException();
				}

				var consumer = SelectedWrappers.Single();

				var posts = await consumer.FetchAllAsync((int)numericUpDown1.Value);

				foreach (var submission in posts) {
					progressBar1.Value++;
					var artworkData = submission is SavedPhotoPost s ? s
						: submission is IRemotePhotoPost p ? await PostConverter.DownloadAsync(p)
						: null;
					if (artworkData == null) continue;

					string imagePath = Path.Combine(folderBrowserDialog1.SelectedPath, PostConverter.CreateFilename(artworkData));

					if (chkExportImage.Checked) {
						File.WriteAllBytes(imagePath, artworkData.data);
					}
					if (chkExportCps.Checked) {
						File.WriteAllText(imagePath + ".cps", JsonConvert.SerializeObject(artworkData, Formatting.Indented));
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			progressBar1.Value = 0;
			panel1.Enabled = true;
		}
	}
}
