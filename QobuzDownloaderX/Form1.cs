﻿using QobuzDownloaderX.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Drawing.Imaging;
using TagLib;
using TagLib.Flac;
using TagLib.Id3v2;
using System.Globalization;
using System.Threading;

namespace QobuzDownloaderX
{
    public partial class QobuzDownloaderX : Form
    {
        public QobuzDownloaderX()
        {
            InitializeComponent();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public string eMail { get; set; }
        public string appid { get; set; }
        public string password { get; set; }
        public string userAuth { get; set; }
        public string profilePic { get; set; }
        public string displayName { get; set; }
        public string userID { get; set; }
        public string accountType { get; set; }
        public string appSecret { get; set; }
        public string albumId { get; set; }
        public string trackIdString { get; set; }
        public string formatIdString { get; set; }
        public string audioFileType { get; set; }
        public string trackRequest { get; set; }
        public string artSize { get; set; }
        public int MaxLength { get; set; }
        public int devClickEggThingValue { get; set; }

        public int poprockid { get; set; }

        searchForm searchF = new searchForm();

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set main form size on launch and bring to center.
            this.Height = 533;
            this.CenterToScreen();

            // Welcome the user after successful login.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("Welcome " + displayName + "!\r\n")));

            // Show account type if user logged in normally.
            if (accountType == null | accountType == "")
            {
                output.Invoke(new Action(() => output.AppendText("\r\n")));
            }
            else
            {
                output.Invoke(new Action(() => output.AppendText("Qobuz Account Type - " + accountType + "\r\n\r\n")));
            }

            output.Invoke(new Action(() => output.AppendText("Your user_auth_token has been set for this session!")));

            // Get and display version number.
            verNumLabel.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Set app_id & auth_token for the Search Form
            searchF.appid = appid;
            searchF.userAuth = userAuth;

            // Set a placeholder image for Cover Art box.
            albumArtPicBox.ImageLocation = "https://static.qobuz.com/images/covers/01/00/2013072600001_150.jpg";
            profilePictureBox.ImageLocation = profilePic;

            // Change account info for logout button
            string oldText = logoutLabel.Text;
            logoutLabel.Text = oldText.Replace("%name%", displayName);

            #region Load Saved Settings
            // Set saved settings to correct places.
            folderBrowserDialog.SelectedPath = Settings.Default.savedFolder.ToString();
            albumCheckbox.Checked = Settings.Default.albumTag;
            albumArtistCheckbox.Checked = Settings.Default.albumArtistTag;
            artistCheckbox.Checked = Settings.Default.artistTag;
            commentCheckbox.Checked = Settings.Default.commentTag;
            commentTextbox.Text = Settings.Default.commentText;
            composerCheckbox.Checked = Settings.Default.composerTag;
            copyrightCheckbox.Checked = Settings.Default.copyrightTag;
            discNumberCheckbox.Checked = Settings.Default.discTag;
            discTotalCheckbox.Checked = Settings.Default.totalDiscsTag;
            genreCheckbox.Checked = Settings.Default.genreTag;
            isrcCheckbox.Checked = Settings.Default.isrcTag;
            explicitCheckbox.Checked = Settings.Default.explicitTag;
            trackTitleCheckbox.Checked = Settings.Default.trackTitleTag;
            trackNumberCheckbox.Checked = Settings.Default.trackTag;
            trackTotalCheckbox.Checked = Settings.Default.totalTracksTag;
            upcCheckbox.Checked = Settings.Default.upcTag;
            releaseCheckbox.Checked = Settings.Default.yearTag;
            imageCheckbox.Checked = Settings.Default.imageTag;
            mp3Checkbox.Checked = Settings.Default.quality1;
            flacLowCheckbox.Checked = Settings.Default.quality2;
            flacMidCheckbox.Checked = Settings.Default.quality3;
            flacHighCheckbox.Checked = Settings.Default.quality4;
            formatIdString = Settings.Default.qualityFormat;
            audioFileType = Settings.Default.audioType;
            artSizeSelect.SelectedIndex = Settings.Default.savedArtSize;
            artSize = artSizeSelect.Text;
            #endregion

            // Check if there's no selected path saved.
            if (folderBrowserDialog.SelectedPath == null | folderBrowserDialog.SelectedPath == "")
            {
                // If there is NOT a saved path.
                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText("No default path has been set! Remember to Choose a Folder!\r\n")));
            }
            else
            {
                // If there is a saved path.
                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText("Using the last folder you've selected as your selected path!\r\n")));
                output.Invoke(new Action(() => output.AppendText("\r\n")));
                output.Invoke(new Action(() => output.AppendText("Default Folder:\r\n")));
                output.Invoke(new Action(() => output.AppendText(folderBrowserDialog.SelectedPath + "\r\n")));
            }

            // Run anything put into the debug events (For Testing)
            debuggingEvents(sender, e);
        }

        private void debuggingEvents(object sender, EventArgs e)
        {
            #region Debug Events, For Testing

            devClickEggThingValue = 0;

            // Show app_secret value.
            //output.Invoke(new Action(() => output.AppendText("\r\n\r\napp_secret = " + appSecret)));

            // Show format_id value.
            //output.Invoke(new Action(() => output.AppendText("\r\n\r\nformat_id = " + formatIdString)));

            #endregion
        }

        //// Set DateTime for new date formatting.
        //System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

        //public string DateConvertion(string Input)
        //{
        //    var date = DateTime.ParseExact(Input, "M/d/yyyy hh:mm:ss tt",
        //                                    CultureInfo.InvariantCulture);

        //    return date.ToString("yyyy-MM-dd");
        //}


        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Create a WebClient named "wc" to be used anywhere.
        WebClient wc = new WebClient();

        private void openSearch_Click(object sender, EventArgs e)
        {
            searchF.ShowDialog();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            getLinkTypeBG.RunWorkerAsync();
        }

        private void albumUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                getLinkTypeBG.RunWorkerAsync();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
         private void createURL(object sender, EventArgs e)
        {
            // Create unix timestamp for "request_ts=" and hashing to make request signature.
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string time = unixTimestamp.ToString();


            // Generate the string that will be hashed using MD5 (utf-8). Example string - "trackgetFileUrlformat_id27intentstreamtrack_id6891469115724574501b4d2f1aca8d4c8ef4z07984c5aa6712" (example shows a fake app_secret)
            string md5HashMe = "trackgetFileUrlformat_id" + formatIdString + "intentstreamtrack_id" + trackIdString + time + appSecret;

            // Generate the MD5 hash using the string created above.
            using (MD5 md5Hash = MD5.Create())
            {
                string requestSignature = GetMd5Hash(md5Hash, md5HashMe);

                if (VerifyMd5Hash(md5Hash, md5HashMe, requestSignature))
                {
                    // If the MD5 hash is verified, proceed to get the streaming URL.
                    WebRequest wrGetFile = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/getFileUrl?request_ts=" + time + "&request_sig=" + requestSignature + "&track_id=" + trackIdString + "&format_id=" + formatIdString + "&intent=stream&app_id=" + appid + "&user_auth_token=" + userAuth);

                    try
                    {
                        // Grab response from API when grabbing the streaming URL.
                        WebResponse ws = wrGetFile.GetResponse();
                        StreamReader sr = new StreamReader(ws.GetResponseStream());

                        string getFileRequest = sr.ReadToEnd();
                        string text = getFileRequest;

                        // Grab stream URL.
                        var streamUrlLog = Regex.Match(getFileRequest, "url\":\"(?<streamUrl>[^\"]+)").Groups;
                        var streamUrl = streamUrlLog[1].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string pattern = @"(?<streamUrlFix>[^\\]+)";
                        string input = streamUrl;
                        RegexOptions options = RegexOptions.Multiline;

                        // Place proper stream URL into the stream URL textbox.
                        testURLBox.Invoke(new Action(() => testURLBox.Text = String.Empty));
                        foreach (Match m in Regex.Matches(input, pattern, options))
                        {
                            testURLBox.Invoke(new Action(() => testURLBox.AppendText(string.Format("{0}", m.Value))));
                        }
                    }
                    catch (Exception ex)
                    {
                        // If connection to API fails, or something is incorrect, show error info.
                        string getError = ex.ToString();
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("Failed to get streaming URL. Error information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(getError)));
                        
                    }
                }
                else
                {
                    // If the hash can't be verified.
                    output.Invoke(new Action(() => output.AppendText("The hash can't be verified. Please retry.\r\n")));
                    mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                    flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                    flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                    flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                    downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    return;
                }
            }
        }

        #region Choosing / Opening folder
        private void selectFolder_Click(object sender, EventArgs e)
        {
            Thread t = new Thread((ThreadStart)(() =>
            {
                // Open Folder Browser to select path & Save the selection
                folderBrowserDialog.ShowDialog();
                Settings.Default.savedFolder = folderBrowserDialog.SelectedPath;
                Settings.Default.Save();
            }));

            // Run your code from a thread that joins the STA Thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            // Open selcted folder
            if (folderBrowserDialog.SelectedPath == null | folderBrowserDialog.SelectedPath == "")
            {
                // If there's no selected path.
                MessageBox.Show("No path selected!", "ERROR",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                return;
            }
            else
            {
                // If selected path doesn't exist, create it. (Will be ignored if it does)
                System.IO.Directory.CreateDirectory(folderBrowserDialog.SelectedPath);
                // Open selcted folder
                Process.Start(@folderBrowserDialog.SelectedPath);
            }
        }
        #endregion

        #region Getting Type of URL
        private void getLinkTypeBG_DoWork(object sender, DoWorkEventArgs e)
        {
            mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = false));
            flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = false));
            flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = false));
            flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = false));
            downloadButton.Invoke(new Action(() => downloadButton.Enabled = false));

            // Check if there's no selected path.
            if (folderBrowserDialog.SelectedPath == null | folderBrowserDialog.SelectedPath == "")
            {
                // If there is NOT a saved path.
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("No path has been set! Remember to Choose a Folder!\r\n")));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                return;
            }

            string albumLink = albumUrl.Text;

            var albumLinkIdGrab = Regex.Match(albumLink, "https:\\/\\/(?:.*?).qobuz.com\\/(?<type>.*?)\\/(?<id>.*?)$").Groups;
            var linkType = albumLinkIdGrab[1].Value;
            var albumLinkId = albumLinkIdGrab[2].Value;

            albumId = albumLinkId;

            if (linkType == "album")
            {
                downloadAlbumBG.RunWorkerAsync();
            }
            else if (linkType == "track")
            {
                downloadTrackBG.RunWorkerAsync();
            }
            else if (linkType == "artist")
            {
                downloadDiscogBG.RunWorkerAsync();
            }
            else if (linkType == "label")
            {
                downloadLabelBG.RunWorkerAsync();
            }

            else if (linkType == "featured")
            {

                downloadFeaturedBG.RunWorkerAsync();
            }
            else if (linkType == "user")
            {
                if (albumId == @"library/favorites/albums")
                {
                    downloadFaveAlbumsBG.RunWorkerAsync();
                }
                //else if (albumId == @"library/favorites/artists")
                //{
                //    downloadFaveArtistsBG.RunWorkerAsync();
                //}
                else
                {
                    output.Invoke(new Action(() => output.Text = String.Empty));
                    output.Invoke(new Action(() => output.AppendText("Downloading favorites only works on favorite albums at the moment. More options will be added in the future.\r\n\r\nIf you'd like to go ahead and grab your favorite albums, paste this link in the URL section - https://play.qobuz.com/user/library/favorites/albums")));
                    mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                    flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                    flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                    flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                    downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    return;
                }
            }
            else if (linkType == "playlist")
            {
                // Say what isn't available at the moment.
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Downloading playlists or artists is not available right now. Maybe in the future. Sorry.")));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                return;
            }
            else
            {
                // Say what isn't available at the moment.
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("URL not understood. Is there a typo?")));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                return;
            }
        }
        #endregion

        #region Downloading Based on URL
        // For downloading "artist" links [MOSTLY WORKING]
        private async void downloadDiscogBG_DoWork(object sender, DoWorkEventArgs e)
        {
            #region If URL has "artist"
            string loc = folderBrowserDialog.SelectedPath;

            trackIdString = albumId;

            WebRequest artistwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/artist/get?artist_id=" + albumId + "&extra=albums%2Cfocus&offset=0&limit=500&sort=release_desc&app_id=" + appid + "&user_auth_token=" + userAuth);

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("Grabbing Album IDs...\r\n\r\n")));

            try
            {
                WebResponse artistws = artistwr.GetResponse();
                StreamReader artistsr = new StreamReader(artistws.GetResponseStream());

                string artistRequest = artistsr.ReadToEnd();

                // Grab all Track IDs listed on the API.
                string artistAlbumIdspattern = ",\"maximum_channel_count\":(?<notUsed>.*?),\"id\":\"(?<albumIds>.*?)\",";
                string input = artistRequest;
                RegexOptions options = RegexOptions.Multiline;

                foreach (Match m in Regex.Matches(input, artistAlbumIdspattern, options))
                {
                    string albumIdDiscog = string.Format("{0}", m.Groups["albumIds"].Value);

                    WebRequest wr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/get?album_id=" + albumIdDiscog + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                    // Empty output, then say Starting Downloads.
                    output.Invoke(new Action(() => output.Text = String.Empty));
                    output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

                    try
                    {
                        // Make sure buttons are disabled during downloads.
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = false));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = false));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = false));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = false));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = false));

                        // Set "loc" as the selected path.
                        loc = folderBrowserDialog.SelectedPath;

                        WebResponse ws = wr.GetResponse();
                        StreamReader sr = new StreamReader(ws.GetResponseStream());

                        string albumRequest = sr.ReadToEnd();

                        string text = albumRequest;

                        var tracksLog = Regex.Match(albumRequest, "tracks_count\":(?<numoftracks>\\d+)").Groups;
                        var tracks = tracksLog[1].Value;

                        // Album Name tag
                        var discogAlbumLog = Regex.Match(albumRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                        var discogAlbum = discogAlbumLog[1].Value;

                        // For converting unicode characters to ASCII
                        string unicodeDiscogAlbum = discogAlbum;
                        string decodedDiscogAlbum = DecodeEncodedNonAsciiCharacters(unicodeDiscogAlbum);
                        discogAlbum = decodedDiscogAlbum;

                        output.Invoke(new Action(() => output.AppendText("Downloading Album - " + discogAlbum + " ......\r\n\r\n")));

                        #region Cover Art URL
                        // Grab Cover Art URL
                        var frontCoverLog = Regex.Match(albumRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
                        var frontCover = frontCoverLog[1].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string imagepattern = @"(?<imageUrlFix>[^\\]+)";
                        string imageinput = frontCover;
                        RegexOptions imageoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
                        }

                        string frontCoverImg = imageURLTextbox.Text;
                        string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
                        frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

                        albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        #region "Goodies" URL (Digital Booklets)
                        // Look for "Goodies" (digital booklet)
                        var goodiesLog = Regex.Match(albumRequest, "\"goodies\":\\[{(?<notUsed>.*?),\"url\":\"(?<booklet>.*?)\",").Groups;
                        var goodies = goodiesLog[2].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string bookpattern = @"(?<imageUrlFix>[^\\]+)";
                        string bookinput = goodies;
                        RegexOptions bookoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mBook in Regex.Matches(bookinput, bookpattern, bookoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mBook.Value))));
                        }

                        string goodiesPDF = imageURLTextbox.Text;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        // Grab sample rate and bit depth for album.
                        var qualityLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

                        var bitDepthLog = Regex.Match(albumRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
                        var sampleRateLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

                        var bitDepth = bitDepthLog[1].Value;
                        var sampleRate = sampleRateLog[1].Value;
                        var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
                        var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

                        if (formatIdString == "5")
                        {
                            quality = "MP3 320kbps CBR";
                            qualityPath = "MP3";
                        }
                        else if (formatIdString == "6")
                        {
                            quality = "FLAC (16bit/44.1kHz)";
                            qualityPath = "FLAC (16bit-44.1kHz)";
                        }
                        else if (formatIdString == "7")
                        {
                            if (quality == "FLAC (24bit/192kHz)")
                            {
                                quality = "FLAC (24bit/96kHz)";
                                qualityPath = "FLAC (24bit-96kHz)";
                            }
                        }

                        // Grab all Track IDs listed on the API.
                        string trackIdspattern = "\"version\":(?:.*?),\"id\":(?<trackId>.*?),";
                        string trackinput = text;
                        RegexOptions trackoptions = RegexOptions.Multiline;


                        foreach (Match mtrack in Regex.Matches(trackinput, trackIdspattern, trackoptions))
                        {
                            // Set default value for max length.
                            const int MaxLength = 36;

                            //output.Invoke(new Action(() => output.AppendText(string.Format("{0}\r\n", m.Groups["trackId"].Value))));
                            trackIdString = string.Format("{0}", mtrack.Groups["trackId"].Value);

                            WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + trackIdString + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                            WebResponse trackws = trackwr.GetResponse();
                            StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

                            string trackRequest = tracksr.ReadToEnd();

                            #region Availability Check (Valid Link?)
                            // Check if available at all.
                            var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
                            var errorCheck = errorCheckLog[1].Value;

                            if (errorCheck == "No result matching given argument")
                            {
                                output.Invoke(new Action(() => output.Text = String.Empty));
                                output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                                output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                                return;
                            }
                            #endregion

                            // Display album quality in quality textbox.
                            qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

                            #region Get Information (Tags, Titles, etc.)
                            // Track Number tag
                            var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
                            var trackNumber = trackNumberLog[1].Value;

                            // Disc Number tag
                            var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
                            var discNumber = discNumberLog[1].Value;

                            // Album Artist tag
                            var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
                            var albumArtist = albumArtistLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumArtist = albumArtist;
                            string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
                            albumArtist = decodedAlbumArtist;

                            albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumArtistPath = albumArtist.Replace(@"\", "-").Replace(@"/", "-").Replace("\\\"", "-").Replace("\\\"", "-").Replace("\"", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album artist in text box under cover art.
                            albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

                            // If name goes over 200 characters, limit it to 200
                            if (albumArtistPath.Length > MaxLength)
                            {
                                albumArtistPath = albumArtistPath.Substring(0, MaxLength);
                            }

                            // Track Artist tag
                            var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
                            var performerName = performerNameLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodePerformerName = performerName;
                            string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
                            performerName = decodedPerformerName;

                            performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (performerNamePath.Length > MaxLength)
                            {
                                performerNamePath = performerNamePath.Substring(0, MaxLength);
                            }

                            // Track Composer tag
                            var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
                            var composerName = composerNameLog[2].Value;

                            // Track Explicitness 
                            var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
                            var advisory = advisoryLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeComposerName = composerName;
                            string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
                            composerName = decodedComposerName;

                            composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Album Name tag
                            var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                            var albumName = albumNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumName = albumName;
                            string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
                            albumName = decodedAlbumName;

                            albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album name in text box under cover art.
                            albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

                            // If name goes over 200 characters, limit it to 200
                            if (albumNamePath.Length > MaxLength)
                            {
                                albumNamePath = albumNamePath.Substring(0, MaxLength);
                            }

                            // Track Name tag
                            var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
                            var trackName = trackNameLog[2].Value;
                            trackName = trackName.Trim(); // Remove spaces from end of track name

                            // For converting unicode characters to ASCII
                            string unicodeTrackName = trackName;
                            string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
                            trackName = decodedTrackName;

                            trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (trackNamePath.Length > MaxLength)
                            {
                                trackNamePath = trackNamePath.Substring(0, MaxLength);
                            }

                            // Version Name tag
                            var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
                            var versionName = versionNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeVersionName = versionName;
                            string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
                            versionName = decodedVersionName;

                            versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Genre tag
                            var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
                            var genre = genreLog[3].Value;

                            // For converting unicode characters to ASCII
                            string unicodeGenre = genre;
                            string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
                            genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Release Date tag, grabs the available "stream" date
                            var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
                            var releaseDate = releaseDateLog[1].Value;

                            // Display release date in text box under cover art.
                            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

                            // Copyright tag
                            var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
                            var copyright = copyrightLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeCopyright = copyright;
                            string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
                            copyright = decodedCopyright;

                            copyright = copyright.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

                            // UPC tag
                            var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
                            var upc = upcLog[1].Value;

                            // Display UPC in text box under cover art.
                            upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

                            // ISRC tag
                            var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
                            var isrc = isrcLog[1].Value;

                            // Total Tracks tag
                            var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+)").Groups;
                            var trackTotal = trackTotalLog[1].Value;

                            // Display Total Tracks in text box under cover art.
                            totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

                            // Total Discs tag
                            var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
                            var discTotal = discTotalLog[1].Value;
                            #endregion

                            #region Filename Number Padding
                            // Set default track number padding length
                            var paddingLength = 2;

                            // Prepare track number padding in filename.
                            string paddingLog = trackTotal.Length.ToString();
                            if (paddingLog == "1")
                            {
                                paddingLength = 2;
                            }
                            else
                            {
                                paddingLength = trackTotal.Length;
                            }

                            // Set default disc number padding length
                            var paddingDiscLength = 2;

                            // Prepare disc number padding in filename.
                            string paddingDiscLog = discTotal.Length.ToString();
                            if (paddingDiscLog == "1")
                            {
                                paddingDiscLength = 1;
                            }
                            else
                            {
                                paddingDiscLength = discTotal.Length;
                            }
                            #endregion

                            #region Create Directories
                            // Create strings for disc folders
                            string discFolder = null;
                            string discFolderCreate = null;

                            // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
                            if (discTotal != "1")
                            {
                                discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                                discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                            }

                            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]");
                            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate);

                            string discogPath = loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate;
                            #endregion

                            #region Availability Check (Streamable?)
                            // Check if available for streaming.
                            var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
                            var streamCheck = streamCheckLog[2].Value;

                            if (streamCheck != "true")
                            {
                                if (streamableCheckbox.Checked == true)
                                {
                                    output.Invoke(new Action(() => output.AppendText("Track " + trackNumber + " \"" + trackName + "\" is not available for streaming. Skipping track...\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("\r\nTrack " + trackNumber + " \"" + trackName + "\" is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                                }
                            }
                            #endregion

                            #region Check if File Exists
                            // Check if there is a version name.
                            if (versionName == null | versionName == "")
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            else
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            #endregion

                            // Close web request and create streaming URL.
                            trackwr.Abort();
                            createURL(sender, e);

                            try
                            {
                                #region Downloading
                                // Check if there is a version name.
                                if (versionName == null | versionName == "")
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                                }
                                // Being download process.
                                var client = new HttpClient();
                                // Run through TLS to allow secure connection.
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                                // Set "range" header to nearly unlimited.
                                client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                                // Set user-agent to Firefox.
                                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                                // Set referer URL to album ID.
                                client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumIdDiscog);
                                // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                                using (var stream = await client.GetStreamAsync(testURLBox.Text))

                                    // Save single track in selected path.
                                    if (versionNamePath == null | versionNamePath == "")
                                    {
                                        // If there is NOT a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                #endregion

                                #region Cover Art Saving
                                if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg"))
                                {
                                    // Skip, don't re-download.

                                    // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                    using (WebClient imgClient = new WebClient())
                                    {
                                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                    }
                                }
                                else
                                {
                                    if (imageCheckbox.Checked == true)
                                    {
                                        // Save cover art to selected path (Currently happens every time a track is downloaded).
                                        using (WebClient imgClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg");

                                            // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                            imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                        }
                                    }
                                }
                                #endregion

                                #region Tagging
                                // Check if audio file type is FLAC or MP3
                                if (audioFileType == ".mp3")
                                {
                                    #region MP3 Tagging (Needs Work)
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for MP3 file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to MP3 file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region FLAC Tagging
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Digital Booklet
                                // If a booklet was found, save it.
                                if (goodiesPDF == null | goodiesPDF == "")
                                {
                                    // No need to download something that doesn't exist.
                                }
                                else
                                {
                                    if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf"))
                                    {
                                        // Skip, don't re-download.
                                    }
                                    else
                                    {
                                        // Save digital booklet to selected path
                                        output.Invoke(new Action(() => output.AppendText("\r\nGoodies found, downloading...")));
                                        using (WebClient bookClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            bookClient.DownloadFile(new Uri(goodiesPDF), loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf");
                                        }
                                    }
                                }
                                #endregion
                            }
                            catch (Exception downloadError)
                            {
                                // If there is an issue trying to, or during the download, show error info.
                                string error = downloadError.ToString();
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText(error)));
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\nIf some tracks aren't available for streaming on the album you're trying to download, try to manually download the available tracks individually.")));
                                
                                
                            }

                            // Delete image file used for tagging
                            if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg"))
                            {
                                System.IO.File.Delete(loc + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }

                            // Say when a track is done downloading, then wait for the next track / end.
                            output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n")));
                            System.Threading.Thread.Sleep(400);
                        }

                        // Say that downloading is completed.
                        output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Downloading job completed! All downloaded files will be located in your chosen path.")));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    }
                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("Failed to download (First Phase). Error information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(error)));
                        
                        
                    }
                }
            }
            catch (Exception downloadError)
            {
                // If there is an issue trying to, or during the download, show error info.
                string error = downloadError.ToString();
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Artist Download ERROR. Information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
               
                
            }
            #endregion
        }

        // For downloading "label" links [IN DEV]
        private async void downloadLabelBG_DoWork(object sender, DoWorkEventArgs e)
        {
            #region If URL has "label"
            string loc = folderBrowserDialog.SelectedPath;

            trackIdString = albumId;

            WebRequest artistwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/label/get?label_id=" + albumId + "&extra=albums%2Cfocus&offset=0&limit=999999999999&app_id=" + appid + "&user_auth_token=" + userAuth);

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("LABEL DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
            output.Invoke(new Action(() => output.AppendText("Grabbing Album IDs...\r\n\r\n")));

            try
            {
                WebResponse artistws = artistwr.GetResponse();
                StreamReader artistsr = new StreamReader(artistws.GetResponseStream());

                string artistRequest = artistsr.ReadToEnd();

                // Grab Label Name
                var labelNameLog = Regex.Match(artistRequest, "\"name\":\"(?<label>.*?)\",").Groups;
                var labelName = labelNameLog[1].Value;

                labelName = labelName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                // Grab all Track IDs listed on the API.
                string artistAlbumIdspattern = ",\"maximum_channel_count\":(?<notUsed>.*?),\"id\":\"(?<albumIds>.*?)\",";
                string input = artistRequest;
                RegexOptions options = RegexOptions.Multiline;

                foreach (Match m in Regex.Matches(input, artistAlbumIdspattern, options))
                {
                    string albumIdDiscog = string.Format("{0}", m.Groups["albumIds"].Value);

                    WebRequest wr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/get?album_id=" + albumIdDiscog + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                    // Empty output, then say Starting Downloads.
                    output.Invoke(new Action(() => output.Text = String.Empty));
                    output.Invoke(new Action(() => output.AppendText("LABEL DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
                    output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

                    try
                    {
                        // Make sure buttons are disabled during downloads.
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = false));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = false));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = false));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = false));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = false));

                        // Set "loc" as the selected path.
                        loc = folderBrowserDialog.SelectedPath;

                        WebResponse ws = wr.GetResponse();
                        StreamReader sr = new StreamReader(ws.GetResponseStream());

                        string albumRequest = sr.ReadToEnd();

                        string text = albumRequest;

                        var tracksLog = Regex.Match(albumRequest, "tracks_count\":(?<numoftracks>\\d+)").Groups;
                        var tracks = tracksLog[1].Value;

                        // Album Name tag
                        var labelDiscogAlbumLog = Regex.Match(albumRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                        var labelDiscogAlbum = labelDiscogAlbumLog[1].Value;

                        // For converting unicode characters to ASCII
                        string unicodeDiscogAlbum = labelDiscogAlbum;
                        string decodedDiscogAlbum = DecodeEncodedNonAsciiCharacters(unicodeDiscogAlbum);
                        labelDiscogAlbum = decodedDiscogAlbum;

                        output.Invoke(new Action(() => output.AppendText("Downloading Album - " + labelDiscogAlbum + " ......\r\n\r\n")));

                        #region Cover Art URL
                        // Grab Cover Art URL
                        var frontCoverLog = Regex.Match(albumRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
                        var frontCover = frontCoverLog[1].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string imagepattern = @"(?<imageUrlFix>[^\\]+)";
                        string imageinput = frontCover;
                        RegexOptions imageoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
                        }

                        string frontCoverImg = imageURLTextbox.Text;
                        string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
                        frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

                        albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        #region "Goodies" URL (Digital Booklets)
                        // Look for "Goodies" (digital booklet)
                        var goodiesLog = Regex.Match(albumRequest, "\"goodies\":\\[{(?<notUsed>.*?),\"url\":\"(?<booklet>.*?)\",").Groups;
                        var goodies = goodiesLog[2].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string bookpattern = @"(?<imageUrlFix>[^\\]+)";
                        string bookinput = goodies;
                        RegexOptions bookoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mBook in Regex.Matches(bookinput, bookpattern, bookoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mBook.Value))));
                        }

                        string goodiesPDF = imageURLTextbox.Text;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        // Grab sample rate and bit depth for album.
                        var qualityLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

                        var bitDepthLog = Regex.Match(albumRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
                        var sampleRateLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

                        var bitDepth = bitDepthLog[1].Value;
                        var sampleRate = sampleRateLog[1].Value;
                        var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
                        var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

                        if (formatIdString == "5")
                        {
                            quality = "MP3 320kbps CBR";
                            qualityPath = "MP3";
                        }
                        else if (formatIdString == "6")
                        {
                            quality = "FLAC (16bit/44.1kHz)";
                            qualityPath = "FLAC (16bit-44.1kHz)";
                        }
                        else if (formatIdString == "7")
                        {
                            if (quality == "FLAC (24bit/192kHz)")
                            {
                                quality = "FLAC (24bit/96kHz)";
                                qualityPath = "FLAC (24bit-96kHz)";
                            }
                        }

                        // Grab all Track IDs listed on the API.
                        string trackIdspattern = "\"version\":(?:.*?),\"id\":(?<trackId>.*?),";
                        string trackinput = text;
                        RegexOptions trackoptions = RegexOptions.Multiline;


                        foreach (Match mtrack in Regex.Matches(trackinput, trackIdspattern, trackoptions))
                        {
                            // Set default value for max length.
                            const int MaxLength = 36;

                            //output.Invoke(new Action(() => output.AppendText(string.Format("{0}\r\n", m.Groups["trackId"].Value))));
                            trackIdString = string.Format("{0}", mtrack.Groups["trackId"].Value);

                            WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + trackIdString + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                            WebResponse trackws = trackwr.GetResponse();
                            StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

                            string trackRequest = tracksr.ReadToEnd();

                            #region Availability Check (Valid Link?)
                            // Check if available at all.
                            var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
                            var errorCheck = errorCheckLog[1].Value;

                            if (errorCheck == "No result matching given argument")
                            {
                                output.Invoke(new Action(() => output.Text = String.Empty));
                                output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                                output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                                return;
                            }
                            #endregion

                            // Display album quality in quality textbox.
                            qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

                            #region Get Information (Tags, Titles, etc.)
                            // Track Number tag
                            var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
                            var trackNumber = trackNumberLog[1].Value;

                            // Disc Number tag
                            var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
                            var discNumber = discNumberLog[1].Value;

                            // Album Artist tag
                            var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
                            var albumArtist = albumArtistLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumArtist = albumArtist;
                            string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
                            albumArtist = decodedAlbumArtist;

                            albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumArtistPath = albumArtist.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album artist in text box under cover art.
                            albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

                            // If name goes over 200 characters, limit it to 200
                            if (albumArtistPath.Length > MaxLength)
                            {
                                albumArtistPath = albumArtistPath.Substring(0, MaxLength);
                            }

                            // Track Artist tag
                            var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
                            var performerName = performerNameLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodePerformerName = performerName;
                            string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
                            performerName = decodedPerformerName;

                            performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (performerNamePath.Length > MaxLength)
                            {
                                performerNamePath = performerNamePath.Substring(0, MaxLength);
                            }

                            // Track Composer tag
                            var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
                            var composerName = composerNameLog[2].Value;

                            // Track Explicitness 
                            var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
                            var advisory = advisoryLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeComposerName = composerName;
                            string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
                            composerName = decodedComposerName;

                            composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Album Name tag
                            var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                            var albumName = albumNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumName = albumName;
                            string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
                            albumName = decodedAlbumName;

                            albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album name in text box under cover art.
                            albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

                            // If name goes over 200 characters, limit it to 200
                            if (albumNamePath.Length > MaxLength)
                            {
                                albumNamePath = albumNamePath.Substring(0, MaxLength);
                            }

                            // Track Name tag
                            var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
                            var trackName = trackNameLog[2].Value;
                            trackName = trackName.Trim(); // Remove spaces from end of track name

                            // For converting unicode characters to ASCII
                            string unicodeTrackName = trackName;
                            string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
                            trackName = decodedTrackName;

                            trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (trackNamePath.Length > MaxLength)
                            {
                                trackNamePath = trackNamePath.Substring(0, MaxLength);
                            }

                            // Version Name tag
                            var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
                            var versionName = versionNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeVersionName = versionName;
                            string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
                            versionName = decodedVersionName;

                            versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Genre tag
                            var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
                            var genre = genreLog[3].Value;

                            // For converting unicode characters to ASCII
                            string unicodeGenre = genre;
                            string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
                            genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Release Date tag, grabs the available "stream" date
                            var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
                            var releaseDate = releaseDateLog[1].Value;

                            // Display release date in text box under cover art.
                            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

                            // Copyright tag
                            var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
                            var copyright = copyrightLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeCopyright = copyright;
                            string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
                            copyright = decodedCopyright;

                            copyright = copyright.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

                            // UPC tag
                            var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
                            var upc = upcLog[1].Value;

                            // Display UPC in text box under cover art.
                            upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

                            // ISRC tag
                            var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
                            var isrc = isrcLog[1].Value;

                            // Total Tracks tag
                            var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+)").Groups;
                            var trackTotal = trackTotalLog[1].Value;

                            // Display Total Tracks in text box under cover art.
                            totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

                            // Total Discs tag
                            var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
                            var discTotal = discTotalLog[1].Value;
                            #endregion

                            #region Filename Number Padding
                            // Set default track number padding length
                            var paddingLength = 2;

                            // Prepare track number padding in filename.
                            string paddingLog = trackTotal.Length.ToString();
                            if (paddingLog == "1")
                            {
                                paddingLength = 2;
                            }
                            else
                            {
                                paddingLength = trackTotal.Length;
                            }

                            // Set default disc number padding length
                            var paddingDiscLength = 2;

                            // Prepare disc number padding in filename.
                            string paddingDiscLog = discTotal.Length.ToString();
                            if (paddingDiscLog == "1")
                            {
                                paddingDiscLength = 1;
                            }
                            else
                            {
                                paddingDiscLength = discTotal.Length;
                            }
                            #endregion

                            #region Create Directories
                            // Create strings for disc folders
                            string discFolder = null;
                            string discFolderCreate = null;

                            // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
                            if (discTotal != "1")
                            {
                                discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                                discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                            }

                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]");
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate);

                            string discogPath = loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate;
                            #endregion

                            #region Availability Check (Streamable?)
                            // Check if available for streaming.
                            var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
                            var streamCheck = streamCheckLog[2].Value;

                            if (streamCheck != "true")
                            {
                                if (streamableCheckbox.Checked == true)
                                {
                                    output.Invoke(new Action(() => output.AppendText("Track " + trackNumber + " \"" + trackName + "\" is not available for streaming. Skipping track...\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("\r\nTrack " + trackNumber + " \"" + trackName + "\" is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                                }
                            }
                            #endregion

                            #region Check if File Exists
                            // Check if there is a version name.
                            if (versionName == null | versionName == "")
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            else
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            #endregion

                            // Close web request and create streaming URL.
                            trackwr.Abort();
                            createURL(sender, e);

                            try
                            {
                                #region Downloading
                                // Check if there is a version name.
                                if (versionName == null | versionName == "")
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                                }
                                // Being download process.
                                var client = new HttpClient();
                                // Run through TLS to allow secure connection.
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                                // Set "range" header to nearly unlimited.
                                client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                                // Set user-agent to Firefox.
                                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                                // Set referer URL to album ID.
                                client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumIdDiscog);
                                // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                                using (var stream = await client.GetStreamAsync(testURLBox.Text))

                                    // Save single track in selected path.
                                    if (versionNamePath == null | versionNamePath == "")
                                    {
                                        // If there is NOT a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                #endregion

                                #region Cover Art Saving
                                if (System.IO.File.Exists(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg"))
                                {
                                    // Skip, don't re-download.

                                    // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                    using (WebClient imgClient = new WebClient())
                                    {
                                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                    }
                                }
                                else
                                {
                                    if (imageCheckbox.Checked == true)
                                    {
                                        // Save cover art to selected path (Currently happens every time a track is downloaded).
                                        using (WebClient imgClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg");

                                            // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                            imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                        }
                                    }
                                }
                                #endregion

                                #region Tagging
                                // Check if audio file type is FLAC or MP3
                                if (audioFileType == ".mp3")
                                {
                                    #region MP3 Tagging (Needs Work)
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for MP3 file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to MP3 file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region FLAC Tagging
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Digital Booklet
                                // If a booklet was found, save it.
                                if (goodiesPDF == null | goodiesPDF == "")
                                {
                                    // No need to download something that doesn't exist.
                                }
                                else
                                {
                                    if (System.IO.File.Exists(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf"))
                                    {
                                        // Skip, don't re-download.
                                    }
                                    else
                                    {
                                        // Save digital booklet to selected path
                                        output.Invoke(new Action(() => output.AppendText("\r\nGoodies found, downloading...")));
                                        using (WebClient bookClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            bookClient.DownloadFile(new Uri(goodiesPDF), loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf");
                                        }
                                    }
                                }
                                #endregion
                            }
                            catch (Exception downloadError)
                            {
                                // If there is an issue trying to, or during the download, show error info.
                                string error = downloadError.ToString();
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText(error)));
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\nIf some tracks aren't available for streaming on the album you're trying to download, try to manually download the available tracks individually.")));
                                
                                
                            }

                            // Delete image file used for tagging
                            if (System.IO.File.Exists(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg"))
                            {
                                System.IO.File.Delete(loc + "\\" + "- Labels" + "\\" + labelName + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }

                            // Say when a track is done downloading, then wait for the next track / end.
                            output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n")));
                            System.Threading.Thread.Sleep(400);
                        }

                        // Say that downloading is completed.
                        output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Downloading job completed! All downloaded files will be located in your chosen path.")));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    }
                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("Failed to download (First Phase). Error information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(error)));
                        
                    }
                }
            }
            catch (Exception downloadError)
            {
                // If there is an issue trying to, or during the download, show error info.
                string error = downloadError.ToString();
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Label Download ERROR. Information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
             
            }
            #endregion
        }

        // For downloading "favorites" (Albums only at the moment) [IN DEV]

        #region If URL is for "favorites"

        // Favorite Albums
        private async void downloadFaveAlbumsBG_DoWork(object sender, DoWorkEventArgs e)
        {
            #region Albums
            string loc = folderBrowserDialog.SelectedPath;

            trackIdString = albumId;

            WebRequest artistwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/favorite/getUserFavorites?type=albums&limit=9999999999&user_id=" + userID + "&app_id=" + appid + "&user_auth_token=" + userAuth);

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("FAVORITE DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE, AND CURRENTLY ONLY SUPPORTS FAVORITED ALBUMS. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
            output.Invoke(new Action(() => output.AppendText("Grabbing Album IDs...\r\n\r\n")));

            try
            {
                WebResponse artistws = artistwr.GetResponse();
                StreamReader artistsr = new StreamReader(artistws.GetResponseStream());

                string artistRequest = artistsr.ReadToEnd();

                // Grab all Track IDs listed on the API.
                string artistAlbumIdspattern = ",\"maximum_channel_count\":(?<notUsed>.*?),\"id\":\"(?<albumIds>.*?)\",";
                string input = artistRequest;
                RegexOptions options = RegexOptions.Multiline;

                foreach (Match m in Regex.Matches(input, artistAlbumIdspattern, options))
                {
                    string albumIdDiscog = string.Format("{0}", m.Groups["albumIds"].Value);

                    WebRequest wr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/get?album_id=" + albumIdDiscog + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                    // Empty output, then say Starting Downloads.
                    output.Invoke(new Action(() => output.Text = String.Empty));
                    output.Invoke(new Action(() => output.AppendText("FAVORITE DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE, AND CURRENTLY ONLY SUPPORTS FAVORITED ALBUMS. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
                    output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

                    try
                    {
                        // Make sure buttons are disabled during downloads.
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = false));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = false));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = false));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = false));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = false));

                        // Set "loc" as the selected path.
                        loc = folderBrowserDialog.SelectedPath;

                        WebResponse ws = wr.GetResponse();
                        StreamReader sr = new StreamReader(ws.GetResponseStream());

                        string albumRequest = sr.ReadToEnd();

                        string text = albumRequest;

                        var tracksLog = Regex.Match(albumRequest, "tracks_count\":(?<numoftracks>\\d+)").Groups;
                        var tracks = tracksLog[1].Value;

                        // Album Name tag
                        var labelDiscogAlbumLog = Regex.Match(albumRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                        var labelDiscogAlbum = labelDiscogAlbumLog[1].Value;

                        // For converting unicode characters to ASCII
                        string unicodeDiscogAlbum = labelDiscogAlbum;
                        string decodedDiscogAlbum = DecodeEncodedNonAsciiCharacters(unicodeDiscogAlbum);
                        labelDiscogAlbum = decodedDiscogAlbum;

                        output.Invoke(new Action(() => output.AppendText("Downloading Album - " + labelDiscogAlbum + " ......\r\n\r\n")));

                        #region Cover Art URL
                        // Grab Cover Art URL
                        var frontCoverLog = Regex.Match(albumRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
                        var frontCover = frontCoverLog[1].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string imagepattern = @"(?<imageUrlFix>[^\\]+)";
                        string imageinput = frontCover;
                        RegexOptions imageoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
                        }

                        string frontCoverImg = imageURLTextbox.Text;
                        string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
                        frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

                        albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        #region "Goodies" URL (Digital Booklets)
                        // Look for "Goodies" (digital booklet)
                        var goodiesLog = Regex.Match(albumRequest, "\"goodies\":\\[{(?<notUsed>.*?),\"url\":\"(?<booklet>.*?)\",").Groups;
                        var goodies = goodiesLog[2].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string bookpattern = @"(?<imageUrlFix>[^\\]+)";
                        string bookinput = goodies;
                        RegexOptions bookoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mBook in Regex.Matches(bookinput, bookpattern, bookoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mBook.Value))));
                        }

                        string goodiesPDF = imageURLTextbox.Text;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        // Grab sample rate and bit depth for album.
                        var qualityLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

                        var bitDepthLog = Regex.Match(albumRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
                        var sampleRateLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

                        var bitDepth = bitDepthLog[1].Value;
                        var sampleRate = sampleRateLog[1].Value;
                        var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
                        var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

                        if (formatIdString == "5")
                        {
                            quality = "MP3 320kbps CBR";
                            qualityPath = "MP3";
                        }
                        else if (formatIdString == "6")
                        {
                            quality = "FLAC (16bit/44.1kHz)";
                            qualityPath = "FLAC (16bit-44.1kHz)";
                        }
                        else if (formatIdString == "7")
                        {
                            if (quality == "FLAC (24bit/192kHz)")
                            {
                                quality = "FLAC (24bit/96kHz)";
                                qualityPath = "FLAC (24bit-96kHz)";
                            }
                        }

                        // Grab all Track IDs listed on the API.
                        string trackIdspattern = "\"version\":(?:.*?),\"id\":(?<trackId>.*?),";
                        string trackinput = text;
                        RegexOptions trackoptions = RegexOptions.Multiline;


                        foreach (Match mtrack in Regex.Matches(trackinput, trackIdspattern, trackoptions))
                        {
                            // Set default value for max length.
                            const int MaxLength = 36;

                            //output.Invoke(new Action(() => output.AppendText(string.Format("{0}\r\n", m.Groups["trackId"].Value))));
                            trackIdString = string.Format("{0}", mtrack.Groups["trackId"].Value);

                            WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + trackIdString + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                            WebResponse trackws = trackwr.GetResponse();
                            StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

                            string trackRequest = tracksr.ReadToEnd();

                            #region Availability Check (Valid Link?)
                            // Check if available at all.
                            var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
                            var errorCheck = errorCheckLog[1].Value;

                            if (errorCheck == "No result matching given argument")
                            {
                                output.Invoke(new Action(() => output.Text = String.Empty));
                                output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                                output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                                return;
                            }
                            #endregion

                            // Display album quality in quality textbox.
                            qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

                            #region Get Information (Tags, Titles, etc.)
                            // Track Number tag
                            var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
                            var trackNumber = trackNumberLog[1].Value;

                            // Disc Number tag
                            var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
                            var discNumber = discNumberLog[1].Value;

                            // Album Artist tag
                            var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
                            var albumArtist = albumArtistLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumArtist = albumArtist;
                            string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
                            albumArtist = decodedAlbumArtist;

                            albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumArtistPath = albumArtist.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album artist in text box under cover art.
                            albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

                            // If name goes over 200 characters, limit it to 200
                            if (albumArtistPath.Length > MaxLength)
                            {
                                albumArtistPath = albumArtistPath.Substring(0, MaxLength);
                            }

                            // Track Artist tag
                            var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
                            var performerName = performerNameLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodePerformerName = performerName;
                            string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
                            performerName = decodedPerformerName;

                            performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (performerNamePath.Length > MaxLength)
                            {
                                performerNamePath = performerNamePath.Substring(0, MaxLength);
                            }

                            // Track Composer tag
                            var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
                            var composerName = composerNameLog[2].Value;

                            // Track Explicitness 
                            var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
                            var advisory = advisoryLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeComposerName = composerName;
                            string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
                            composerName = decodedComposerName;

                            composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Album Name tag
                            var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                            var albumName = albumNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumName = albumName;
                            string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
                            albumName = decodedAlbumName;

                            albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album name in text box under cover art.
                            albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

                            // If name goes over 200 characters, limit it to 200
                            if (albumNamePath.Length > MaxLength)
                            {
                                albumNamePath = albumNamePath.Substring(0, MaxLength);
                            }

                            // Track Name tag
                            var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
                            var trackName = trackNameLog[2].Value;
                            trackName = trackName.Trim(); // Remove spaces from end of track name

                            // For converting unicode characters to ASCII
                            string unicodeTrackName = trackName;
                            string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
                            trackName = decodedTrackName;

                            trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (trackNamePath.Length > MaxLength)
                            {
                                trackNamePath = trackNamePath.Substring(0, MaxLength);
                            }

                            // Version Name tag
                            var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
                            var versionName = versionNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeVersionName = versionName;
                            string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
                            versionName = decodedVersionName;

                            versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Genre tag
                            var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
                            var genre = genreLog[3].Value;

                            // For converting unicode characters to ASCII
                            string unicodeGenre = genre;
                            string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
                            genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Release Date tag, grabs the available "stream" date
                            var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
                            var releaseDate = releaseDateLog[1].Value;

                            // Display release date in text box under cover art.
                            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

                            // Copyright tag
                            var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
                            var copyright = copyrightLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeCopyright = copyright;
                            string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
                            copyright = decodedCopyright;

                            copyright = copyright.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

                            // UPC tag
                            var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
                            var upc = upcLog[1].Value;

                            // Display UPC in text box under cover art.
                            upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

                            // ISRC tag
                            var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
                            var isrc = isrcLog[1].Value;

                            // Total Tracks tag
                            var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+)").Groups;
                            var trackTotal = trackTotalLog[1].Value;

                            // Display Total Tracks in text box under cover art.
                            totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

                            // Total Discs tag
                            var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
                            var discTotal = discTotalLog[1].Value;
                            #endregion

                            #region Filename Number Padding
                            // Set default track number padding length
                            var paddingLength = 2;

                            // Prepare track number padding in filename.
                            string paddingLog = trackTotal.Length.ToString();
                            if (paddingLog == "1")
                            {
                                paddingLength = 2;
                            }
                            else
                            {
                                paddingLength = trackTotal.Length;
                            }

                            // Set default disc number padding length
                            var paddingDiscLength = 2;

                            // Prepare disc number padding in filename.
                            string paddingDiscLog = discTotal.Length.ToString();
                            if (paddingDiscLog == "1")
                            {
                                paddingDiscLength = 1;
                            }
                            else
                            {
                                paddingDiscLength = discTotal.Length;
                            }
                            #endregion

                            #region Create Directories
                            // Create strings for disc folders
                            string discFolder = null;
                            string discFolderCreate = null;

                            // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
                            if (discTotal != "1")
                            {
                                discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                                discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                            }

                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]");
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate);

                            string discogPath = loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate;
                            #endregion

                            #region Availability Check (Streamable?)
                            // Check if available for streaming.
                            var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
                            var streamCheck = streamCheckLog[2].Value;

                            if (streamCheck != "true")
                            {
                                if (streamableCheckbox.Checked == true)
                                {
                                    output.Invoke(new Action(() => output.AppendText("Track " + trackNumber + " \"" + trackName + "\" is not available for streaming. Skipping track...\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("\r\nTrack " + trackNumber + " \"" + trackName + "\" is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                                }
                            }
                            #endregion

                            #region Check if File Exists
                            // Check if there is a version name.
                            if (versionName == null | versionName == "")
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            else
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            #endregion

                            // Close web request and create streaming URL.
                            trackwr.Abort();
                            createURL(sender, e);

                            try
                            {
                                #region Downloading
                                // Check if there is a version name.
                                if (versionName == null | versionName == "")
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                                }
                                // Being download process.
                                var client = new HttpClient();
                                // Run through TLS to allow secure connection.
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                                // Set "range" header to nearly unlimited.
                                client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                                // Set user-agent to Firefox.
                                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                                // Set referer URL to album ID.
                                client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumIdDiscog);
                                // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                                using (var stream = await client.GetStreamAsync(testURLBox.Text))

                                    // Save single track in selected path.
                                    if (versionNamePath == null | versionNamePath == "")
                                    {
                                        // If there is NOT a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                #endregion

                                #region Cover Art Saving
                                if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg"))
                                {
                                    // Skip, don't re-download.

                                    // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                    using (WebClient imgClient = new WebClient())
                                    {
                                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                    }
                                }
                                else
                                {
                                    if (imageCheckbox.Checked == true)
                                    {
                                        // Save cover art to selected path (Currently happens every time a track is downloaded).
                                        using (WebClient imgClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg");

                                            // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                            imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                        }
                                    }
                                }
                                #endregion

                                #region Tagging
                                // Check if audio file type is FLAC or MP3
                                if (audioFileType == ".mp3")
                                {
                                    #region MP3 Tagging (Needs Work)
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for MP3 file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to MP3 file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region FLAC Tagging
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Digital Booklet
                                // If a booklet was found, save it.
                                if (goodiesPDF == null | goodiesPDF == "")
                                {
                                    // No need to download something that doesn't exist.
                                }
                                else
                                {
                                    if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf"))
                                    {
                                        // Skip, don't re-download.
                                    }
                                    else
                                    {
                                        // Save digital booklet to selected path
                                        output.Invoke(new Action(() => output.AppendText("\r\nGoodies found, downloading...")));
                                        using (WebClient bookClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            bookClient.DownloadFile(new Uri(goodiesPDF), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf");
                                        }
                                    }
                                }
                                #endregion
                            }
                            catch (Exception downloadError)
                            {
                                // If there is an issue trying to, or during the download, show error info.
                                string error = downloadError.ToString();
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText(error)));
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\nIf some tracks aren't available for streaming on the album you're trying to download, try to manually download the available tracks individually.")));
                                
                            }

                            // Delete image file used for tagging
                            if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg"))
                            {
                                System.IO.File.Delete(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }

                            // Say when a track is done downloading, then wait for the next track / end.
                            output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n")));
                            System.Threading.Thread.Sleep(400);
                        }

                        // Say that downloading is completed.
                        output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Downloading job completed! All downloaded files will be located in your chosen path.")));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    }
                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("Failed to download (First Phase). Error information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(error)));
                     
                    }
                }
            }
            catch (Exception downloadError)
            {
                // If there is an issue trying to, or during the download, show error info.
                string error = downloadError.ToString();
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Label Download ERROR. Information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
                
            }
            #endregion
        }

        // Favorite Artists [Not worked on at all yet]
        private async void downloadFaveArtistsBG_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        #endregion

        // For downloading "album" links
        private async void downloadAlbumBG_DoWork(object sender, DoWorkEventArgs e)
        {
            #region If URL has "album"
            WebRequest albumWR = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/get?album_id=" + albumId + "&app_id=" + appid + "&user_auth_token=" + userAuth);

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

            try
            {
                // Set "loc" as the selected path.
                String loc = folderBrowserDialog.SelectedPath;

                WebResponse albumWS = albumWR.GetResponse();
                StreamReader albumSR = new StreamReader(albumWS.GetResponseStream());

                string albumRequest = albumSR.ReadToEnd();

                string text = albumRequest;

                #region Cover Art URL
                // Grab Cover Art URL
                var frontCoverLog = Regex.Match(albumRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
                var frontCover = frontCoverLog[1].Value;

                // Remove backslashes from the stream URL to have a proper URL.
                string imagepattern = @"(?<imageUrlFix>[^\\]+)";
                string imageinput = frontCover;
                RegexOptions imageoptions = RegexOptions.Multiline;

                imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
                {
                    imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
                }

                string frontCoverImg = imageURLTextbox.Text;
                string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
                frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

                albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

                imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                #endregion

                #region "Goodies" URL (Digital Booklets)
                // Look for "Goodies" (digital booklet)
                var goodiesLog = Regex.Match(albumRequest, "\"goodies\":\\[{(?<notUsed>.*?),\"url\":\"(?<booklet>.*?)\",").Groups;
                var goodies = goodiesLog[2].Value;

                // Remove backslashes from the stream URL to have a proper URL.
                string bookpattern = @"(?<imageUrlFix>[^\\]+)";
                string bookinput = goodies;
                RegexOptions bookoptions = RegexOptions.Multiline;

                imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                foreach (Match mBook in Regex.Matches(bookinput, bookpattern, bookoptions))
                {
                    imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mBook.Value))));
                }

                string goodiesPDF = imageURLTextbox.Text;

                imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                #endregion

                // Grab sample rate and bit depth for album.
                var qualityLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

                var bitDepthLog = Regex.Match(albumRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
                var sampleRateLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

                var bitDepth = bitDepthLog[1].Value;
                var sampleRate = sampleRateLog[1].Value;
                var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
                var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

                if (formatIdString == "5")
                {
                    quality = "MP3 320kbps CBR";
                    qualityPath = "MP3";
                }
                else if (formatIdString == "6")
                {
                    quality = "FLAC (16bit/44.1kHz)";
                    qualityPath = "FLAC (16bit-44.1kHz)";
                }
                else if (formatIdString == "7")
                {
                    if (quality == "FLAC (24bit/192kHz)")
                    {
                        quality = "FLAC (24bit/96kHz)";
                        qualityPath = "FLAC (24bit-96kHz)";
                    }
                }

                // Grab all Track IDs listed on the API.
                string trackIdspattern = "\"version\":(?:.*?),\"id\":(?<trackId>.*?),";
                string input = text;
                RegexOptions options = RegexOptions.Multiline;


                foreach (Match m in Regex.Matches(input, trackIdspattern, options))
                {
                    // Set default value for max length.
                    const int MaxLength = 36;

                    // Grab matches for Track IDs
                    trackIdString = string.Format("{0}", m.Groups["trackId"].Value);

                    WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + trackIdString + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                    WebResponse trackws = trackwr.GetResponse();
                    StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

                    string trackRequest = tracksr.ReadToEnd();

                    #region Availability Check (Valid Link?)
                    // Check if available at all.
                    var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
                    var errorCheck = errorCheckLog[1].Value;

                    if (errorCheck == "No result matching given argument")
                    {
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                        output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                        return;
                    }
                    #endregion

                    // Display album quality in quality textbox.
                    qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

                    #region Get Information (Tags, Titles, etc.)
                    // Track Number tag
                    var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
                    var trackNumber = trackNumberLog[1].Value;

                    // Total Tracks tag
                    var tracksLog = Regex.Match(albumRequest, "tracks_count\":(?<numoftracks>\\d+)").Groups;
                    var tracks = tracksLog[1].Value;

                    // Disc Number tag
                    var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
                    var discNumber = discNumberLog[1].Value;

                    // Album Artist tag
                    var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
                    var albumArtist = albumArtistLog[2].Value;

                    // For converting unicode characters to ASCII
                    string unicodeAlbumArtist = albumArtist;
                    string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
                    albumArtist = decodedAlbumArtist;

                    // Replace double slashes & path unfriendly characters
                    albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                    var albumArtistPath = albumArtist.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                    // Display album artist in text box under cover art.
                    albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

                    // If name goes over 200 characters, limit it to 200
                    if (albumArtistPath.Length > MaxLength)
                    {
                        albumArtistPath = albumArtistPath.Substring(0, MaxLength);
                    }

                    // Track Artist tag
                    var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
                    var performerName = performerNameLog[2].Value;

                    // For converting unicode characters to ASCII
                    string unicodePerformerName = performerName;
                    string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
                    performerName = decodedPerformerName;

                    // Replace double slashes & path unfriendly characters
                    performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                    var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                    // If name goes over 200 characters, limit it to 200
                    if (performerNamePath.Length > MaxLength)
                    {
                        performerNamePath = performerNamePath.Substring(0, MaxLength);
                    }

                    // Track Composer tag
                    var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
                    var composerName = composerNameLog[2].Value;

                    // Track Explicitness 
                    var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
                    var advisory = advisoryLog[1].Value;

                    // For converting unicode characters to ASCII
                    string unicodeComposerName = composerName;
                    string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
                    composerName = decodedComposerName;

                    // Replace double slashes
                    composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                    // Album Name tag
                    var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                    var albumName = albumNameLog[1].Value;

                    // For converting unicode characters to ASCII
                    string unicodeAlbumName = albumName;
                    string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
                    albumName = decodedAlbumName;

                    // Replace double slashes & path unfriendly characters
                    albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                    var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                    // Display album name in text box under cover art.
                    albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

                    // If name goes over 200 characters, limit it to 200
                    if (albumNamePath.Length > MaxLength)
                    {
                        albumNamePath = albumNamePath.Substring(0, MaxLength);
                    }

                    // Track Name tag
                    var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
                    var trackName = trackNameLog[2].Value;
                    trackName = trackName.Trim(); // Remove spaces from end of track name

                    // For converting unicode characters to ASCII
                    string unicodeTrackName = trackName;
                    string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
                    trackName = decodedTrackName;

                    // Replace double slashes & path unfriendly characters
                    trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                    var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                    // If name goes over 200 characters, limit it to 200
                    if (trackNamePath.Length > MaxLength)
                    {
                        trackNamePath = trackNamePath.Substring(0, MaxLength);
                    }

                    // Version Name tag
                    var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
                    var versionName = versionNameLog[1].Value;

                    // For converting unicode characters to ASCII
                    string unicodeVersionName = versionName;
                    string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
                    versionName = decodedVersionName;

                    // Replace double slashes & path unfriendly characters
                    versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                    var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                    //// If name goes over 200 characters, limit it to 200
                    //if (trackNamePath.Length + versionNamePath.Length > MaxLength)
                    //{
                    //    versionNamePath = null;
                    //}

                    // Genre tag
                    var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
                    var genre = genreLog[3].Value;

                    // For converting unicode characters to ASCII
                    string unicodeGenre = genre;
                    string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
                    genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                    // Release Date tag, grabs the available "stream" date
                    var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
                    var releaseDate = releaseDateLog[1].Value;

                    // Display release date in text box under cover art.
                    releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

                    // Copyright tag
                    var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
                    var copyright = copyrightLog[2].Value;

                    // For converting unicode characters to ASCII
                    string unicodeCopyright = copyright;
                    string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
                    copyright = decodedCopyright;

                    copyright = copyright.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

                    // UPC tag
                    var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
                    var upc = upcLog[1].Value;

                    // Display UPC in text box under cover art.
                    upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

                    // ISRC tag
                    var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
                    var isrc = isrcLog[1].Value;

                    // Total Tracks tag
                    var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+)").Groups;
                    var trackTotal = trackTotalLog[1].Value;

                    // Display Total Tracks in text box under cover art.
                    totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

                    // Total Discs tag
                    var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
                    var discTotal = discTotalLog[1].Value;
                    #endregion

                    #region Filename Number Padding
                    // Set default track number padding length
                    var paddingLength = 2;

                    // Prepare track number padding in filename.
                    string paddingLog = trackTotal.Length.ToString();
                    if (paddingLog == "1")
                    {
                        paddingLength = 2;
                    }
                    else
                    {
                        paddingLength = trackTotal.Length;
                    }

                    // Set default disc number padding length
                    var paddingDiscLength = 2;

                    // Prepare disc number padding in filename.
                    string paddingDiscLog = discTotal.Length.ToString();
                    if (paddingDiscLog == "1")
                    {
                        paddingDiscLength = 1;
                    }
                    else
                    {
                        paddingDiscLength = discTotal.Length;
                    }
                    #endregion

                    #region Create Directories
                    // Create strings for disc folders
                    string discFolder = null;
                    string discFolderCreate = null;

                    // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
                    if (discTotal != "1")
                    {
                        discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                        discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                    }

                    // Create directories
                    System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath);
                    System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath);
                    System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath);
                    System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + discFolderCreate);

                    // Set albumPath to the created directories.
                    string albumPath = loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + discFolderCreate;
                    #endregion

                    #region Availability Check (Streamable?)
                    // Check if available for streaming.
                    var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
                    var streamCheck = streamCheckLog[2].Value;

                    if (streamCheck != "true")
                    {
                        if (streamableCheckbox.Checked == true)
                        {
                            output.Invoke(new Action(() => output.AppendText("Track " + trackNumber + " \"" + trackName + "\" is not available for streaming. Skipping track...\r\n")));
                            System.Threading.Thread.Sleep(400);
                            continue;
                        }
                        else
                        {
                            output.Invoke(new Action(() => output.AppendText("\r\nTrack " + trackNumber + " \"" + trackName + "\" is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                        }
                    }
                    #endregion

                    #region Check if File Exists
                    // Check if there is a version name.
                    if (versionName == null | versionName == "")
                    {
                        if (System.IO.File.Exists(albumPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                        {
                            output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                            System.Threading.Thread.Sleep(400);
                            continue;
                        }
                    }
                    else
                    {
                        if (System.IO.File.Exists(albumPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                        {
                            output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                            System.Threading.Thread.Sleep(400);
                            continue;
                        }
                    }
                    #endregion

                    // Close web request and create streaming URL.
                    trackwr.Abort();
                    createURL(sender, e);

                    try
                    {
                        #region Downloading
                        // Check if there is a version name.
                        if (versionName == null | versionName == "")
                        {
                            output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                        }
                        else
                        {
                            output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                        }

                        // Being download process.
                        var client = new HttpClient();
                        // Run through TLS to allow secure connection.
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                        // Set "range" header to nearly unlimited.
                        client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                        // Set user-agent to Firefox.
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                        // Set referer URL to album ID.
                        client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumId);
                        // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                        using (var stream = await client.GetStreamAsync(testURLBox.Text))

                            // Save single track in selected path.
                            if (versionNamePath == null | versionNamePath == "")
                            {
                                // If there is NOT a version name.
                                using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                {
                                    await stream.CopyToAsync(output);
                                }
                            }
                            else
                            {
                                // If there is a version name.
                                using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                {
                                    await stream.CopyToAsync(output);
                                }
                            }
                        #endregion

                        #region Cover Art Saving
                        if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Cover.jpg"))
                        {
                            // Skip, don't re-download.

                            // Save cover art to selected path (Currently happens every time a track is downloaded).
                            using (WebClient imgClient = new WebClient())
                            {
                                // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }
                        }
                        else
                        {
                            // Save cover art to selected path (Currently happens every time a track is downloaded).
                            using (WebClient imgClient = new WebClient())
                            {
                                // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Cover.jpg");

                                // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }
                        }
                        #endregion

                        #region Tagging
                        // Check if audio file type is FLAC or MP3
                        if (audioFileType == ".mp3")
                        {
                            #region MP3 Tagging (Needs Work)
                            // Select the downloaded file to prepare for tagging.
                            // Check if there's a version name or not
                            if (versionName == null | versionName == "")
                            {
                                // If there is NOT a version name.
                                var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                // For custom / troublesome tags.
                                TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                // Saving cover art to file(s)
                                if (imageCheckbox.Checked == true)
                                {
                                    // Define cover art to use for MP3 file(s)
                                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                    pic.TextEncoding = TagLib.StringType.Latin1;
                                    pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                    pic.Type = TagLib.PictureType.FrontCover;
                                    pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                    // Save cover art to MP3 file.
                                    tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                    tfile.Save();
                                }

                                // Track Title tag
                                if (trackTitleCheckbox.Checked == true)
                                {
                                    tfile.Tag.Title = trackName;
                                }

                                // Album Title tag
                                if (albumCheckbox.Checked == true)
                                {
                                    tfile.Tag.Album = albumName;
                                }

                                // Album Artits tag
                                if (albumArtistCheckbox.Checked == true)
                                {
                                    tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                }

                                // Track Artist tag
                                if (artistCheckbox.Checked == true)
                                {
                                    tfile.Tag.Performers = new string[] { performerName };
                                }

                                // Composer tag
                                if (composerCheckbox.Checked == true)
                                {
                                    tfile.Tag.Composers = new string[] { composerName };
                                }

                                // Release Date tag
                                if (releaseCheckbox.Checked == true)
                                {
                                    releaseDate = releaseDate.Substring(0, 4);
                                    tfile.Tag.Year = UInt32.Parse(releaseDate);
                                }

                                // Genre tag
                                if (genreCheckbox.Checked == true)
                                {
                                    tfile.Tag.Genres = new string[] { genre };
                                }

                                // Track Number tag
                                if (trackNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Track = UInt32.Parse(trackNumber);
                                }

                                // Disc Number tag
                                if (discNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Disc = UInt32.Parse(discNumber);
                                }

                                // Total Discs tag
                                if (discTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                }

                                // Total Tracks tag
                                if (trackTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                }

                                // Comment tag
                                if (commentCheckbox.Checked == true)
                                {
                                    tfile.Tag.Comment = commentTextbox.Text;
                                }

                                // Copyright tag
                                if (copyrightCheckbox.Checked == true)
                                {
                                    tfile.Tag.Copyright = copyright;
                                }
                                // UPC tag
                                if (upcCheckbox.Checked == true)
                                {
                                    // Not available on MP3 at the moment
                                }

                                // ISRC tag
                                if (isrcCheckbox.Checked == true)
                                {
                                    TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                    tag.SetTextFrame("TSRC", isrc);
                                }

                                // Explicit tag
                                if (explicitCheckbox.Checked == true)
                                {
                                    // Not available on MP3 at the moment
                                }

                                // Save all selected tags to file
                                tfile.Save();
                            }
                            else
                            {
                                // If there is a version name.
                                var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                // For custom / troublesome tags.
                                TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                // Saving cover art to file(s)
                                if (imageCheckbox.Checked == true)
                                {
                                    // Define cover art to use for FLAC file(s)
                                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                    pic.TextEncoding = TagLib.StringType.Latin1;
                                    pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                    pic.Type = TagLib.PictureType.FrontCover;
                                    pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                    // Save cover art to FLAC file.
                                    tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                    tfile.Save();
                                }

                                // Track Title tag
                                if (trackTitleCheckbox.Checked == true)
                                {
                                    tfile.Tag.Title = trackName + " (" + versionName + ")";
                                }

                                // Album Title tag
                                if (albumCheckbox.Checked == true)
                                {
                                    tfile.Tag.Album = albumName;
                                }

                                // Album Artits tag
                                if (albumArtistCheckbox.Checked == true)
                                {
                                    tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                }

                                // Track Artist tag
                                if (artistCheckbox.Checked == true)
                                {
                                    tfile.Tag.Performers = new string[] { performerName };
                                }

                                // Composer tag
                                if (composerCheckbox.Checked == true)
                                {
                                    tfile.Tag.Composers = new string[] { composerName };
                                }

                                // Release Date tag
                                if (releaseCheckbox.Checked == true)
                                {
                                    releaseDate = releaseDate.Substring(0, 4);
                                    tfile.Tag.Year = UInt32.Parse(releaseDate);
                                }

                                // Genre tag
                                if (genreCheckbox.Checked == true)
                                {
                                    tfile.Tag.Genres = new string[] { genre };
                                }

                                // Track Number tag
                                if (trackNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Track = UInt32.Parse(trackNumber);
                                }

                                // Disc Number tag
                                if (discNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Disc = UInt32.Parse(discNumber);
                                }

                                // Total Discs tag
                                if (discTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                }

                                // Total Tracks tag
                                if (trackTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                }

                                // Comment tag
                                if (commentCheckbox.Checked == true)
                                {
                                    tfile.Tag.Comment = commentTextbox.Text;
                                }

                                // Copyright tag
                                if (copyrightCheckbox.Checked == true)
                                {
                                    tfile.Tag.Copyright = copyright;
                                }
                                // UPC tag
                                if (upcCheckbox.Checked == true)
                                {
                                    // Not available on MP3 at the moment
                                }

                                // ISRC tag
                                if (isrcCheckbox.Checked == true)
                                {
                                    TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                    tag.SetTextFrame("TSRC", isrc);
                                }

                                // Explicit tag
                                if (explicitCheckbox.Checked == true)
                                {
                                    // Not available on MP3 at the moment
                                }

                                // Save all selected tags to file
                                tfile.Save();
                            }
                            #endregion
                        }
                        else
                        {
                            #region FLAC Tagging
                            // Select the downloaded file to prepare for tagging.
                            // Check if there's a version name or not
                            if (versionName == null | versionName == "")
                            {
                                // If there is NOT a version name.
                                var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                // For custom / troublesome tags.
                                var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                // Saving cover art to file(s)
                                if (imageCheckbox.Checked == true)
                                {
                                    // Define cover art to use for FLAC file(s)
                                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                    pic.TextEncoding = TagLib.StringType.Latin1;
                                    pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                    pic.Type = TagLib.PictureType.FrontCover;
                                    pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                    // Save cover art to FLAC file.
                                    tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                    tfile.Save();
                                }

                                // Track Title tag
                                if (trackTitleCheckbox.Checked == true)
                                {
                                    tfile.Tag.Title = trackName;
                                }

                                // Album Title tag
                                if (albumCheckbox.Checked == true)
                                {
                                    tfile.Tag.Album = albumName;
                                }

                                // Album Artits tag
                                if (albumArtistCheckbox.Checked == true)
                                {
                                    custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                }

                                // Track Artist tag
                                if (artistCheckbox.Checked == true)
                                {
                                    custom.SetField("ARTIST", new string[] { performerName });
                                }

                                // Composer tag
                                if (composerCheckbox.Checked == true)
                                {
                                    custom.SetField("COMPOSER", new string[] { composerName });
                                }

                                // Release Date tag
                                if (releaseCheckbox.Checked == true)
                                {
                                    custom.SetField("YEAR", new string[] { releaseDate });
                                }

                                // Genre tag
                                if (genreCheckbox.Checked == true)
                                {
                                    custom.SetField("GENRE", new string[] { genre });
                                }

                                // Track Number tag
                                if (trackNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Track = UInt32.Parse(trackNumber);
                                }

                                // Disc Number tag
                                if (discNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Disc = UInt32.Parse(discNumber);
                                }

                                // Total Discs tag
                                if (discTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                }

                                // Total Tracks tag
                                if (trackTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                }

                                // Comment tag
                                if (commentCheckbox.Checked == true)
                                {
                                    custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                }

                                // Copyright tag
                                if (copyrightCheckbox.Checked == true)
                                {
                                    custom.SetField("COPYRIGHT", new string[] { copyright });
                                }
                                // UPC tag
                                if (upcCheckbox.Checked == true)
                                {
                                    custom.SetField("UPC", new string[] { upc });
                                }

                                // ISRC tag
                                if (isrcCheckbox.Checked == true)
                                {
                                    custom.SetField("ISRC", new string[] { isrc });
                                }

                                // Explicit tag
                                if (explicitCheckbox.Checked == true)
                                {
                                    if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                }

                                // Save all selected tags to file
                                tfile.Save();
                            }
                            else
                            {
                                // If there is a version name.
                                var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                // For custom / troublesome tags.
                                var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                // Saving cover art to file(s)
                                if (imageCheckbox.Checked == true)
                                {
                                    // Define cover art to use for FLAC file(s)
                                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                    pic.TextEncoding = TagLib.StringType.Latin1;
                                    pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                    pic.Type = TagLib.PictureType.FrontCover;
                                    pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                    // Save cover art to FLAC file.
                                    tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                    tfile.Save();
                                }

                                // Track Title tag
                                if (trackTitleCheckbox.Checked == true)
                                {
                                    tfile.Tag.Title = trackName + " (" + versionName + ")";
                                }

                                // Album Title tag
                                if (albumCheckbox.Checked == true)
                                {
                                    tfile.Tag.Album = albumName;
                                }

                                // Album Artits tag
                                if (albumArtistCheckbox.Checked == true)
                                {
                                    custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                }

                                // Track Artist tag
                                if (artistCheckbox.Checked == true)
                                {
                                    custom.SetField("ARTIST", new string[] { performerName });
                                }

                                // Composer tag
                                if (composerCheckbox.Checked == true)
                                {
                                    custom.SetField("COMPOSER", new string[] { composerName });
                                }

                                // Release Date tag
                                if (releaseCheckbox.Checked == true)
                                {
                                    custom.SetField("YEAR", new string[] { releaseDate });
                                }

                                // Genre tag
                                if (genreCheckbox.Checked == true)
                                {
                                    custom.SetField("GENRE", new string[] { genre });
                                }

                                // Track Number tag
                                if (trackNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Track = UInt32.Parse(trackNumber);
                                }

                                // Disc Number tag
                                if (discNumberCheckbox.Checked == true)
                                {
                                    tfile.Tag.Disc = UInt32.Parse(discNumber);
                                }

                                // Total Discs tag
                                if (discTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                }

                                // Total Tracks tag
                                if (trackTotalCheckbox.Checked == true)
                                {
                                    tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                }

                                // Comment tag
                                if (commentCheckbox.Checked == true)
                                {
                                    custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                }

                                // Copyright tag
                                if (copyrightCheckbox.Checked == true)
                                {
                                    custom.SetField("COPYRIGHT", new string[] { copyright });
                                }
                                // UPC tag
                                if (upcCheckbox.Checked == true)
                                {
                                    custom.SetField("UPC", new string[] { upc });
                                }

                                // ISRC tag
                                if (isrcCheckbox.Checked == true)
                                {
                                    custom.SetField("ISRC", new string[] { isrc });
                                }

                                // Explicit tag
                                if (explicitCheckbox.Checked == true)
                                {
                                    if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                }

                                // Save all selected tags to file
                                tfile.Save();
                            }
                            #endregion
                        }
                        #endregion

                        #region Digital Booklet
                        // If a booklet was found, save it.
                        if (goodiesPDF == null | goodiesPDF == "")
                        {
                            // No need to download something that doesn't exist.
                        }
                        else
                        {
                            if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Digital Booklet.pdf"))
                            {
                                // Skip, don't re-download.
                            }
                            else
                            {
                                if (trackNumber == trackTotal)
                                {
                                    // Save digital booklet to selected path
                                    output.Invoke(new Action(() => output.AppendText("Goodies found, downloading...")));
                                    using (WebClient bookClient = new WebClient())
                                    {
                                        // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                        bookClient.DownloadFile(new Uri(goodiesPDF), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Digital Booklet.pdf");
                                    }
                                }
                                else
                                {
                                    // Skip, don't download until final track.
                                }
                            }
                        }
                        #endregion
                    }
                    catch (Exception downloadError)
                    {
                        // If there is an issue trying to, or during the download, show error info.
                        string error = downloadError.ToString();
                        output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(error)));
                        
                    }

                    // Delete image file used for tagging
                    if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg"))
                    {
                        System.IO.File.Delete(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
                    }

                    // Say when a track is done downloading, then wait for the next track / end.
                    output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n")));
                    System.Threading.Thread.Sleep(400);
                }

                // Say that downloading is completed.
                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText("Downloading job completed! All downloaded files will be located in your chosen path.")));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                //output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Failed to download (First Phase). Error information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
                
            }
            #endregion
        }
        //for downloading "new releases" links
        private async void downloadFeaturedBG_DoWork(object sender, DoWorkEventArgs e)
        {
         
            #region If URL has "new releases"
            string loc = folderBrowserDialog.SelectedPath;

            trackIdString = albumId;
            WebRequest artistwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/getFeatured?app_id=" + appid + "&type=new-releases-full&genre-ids=112,64,127,80,94,91,10&limit=99999");

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("FAVORITE DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE, AND CURRENTLY ONLY SUPPORTS FAVORITED ALBUMS. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
            output.Invoke(new Action(() => output.AppendText("Grabbing Album IDs...\r\n\r\n")));

            try
            {
                WebResponse artistws = artistwr.GetResponse();
                StreamReader artistsr = new StreamReader(artistws.GetResponseStream());

                string artistRequest = artistsr.ReadToEnd();

                // Grab all Track IDs listed on the API.
                string artistAlbumIdspattern = ",\"maximum_channel_count\":(?<notUsed>.*?),\"id\":\"(?<albumIds>.*?)\",";
                string input = artistRequest;
                RegexOptions options = RegexOptions.Multiline;

                foreach (Match m in Regex.Matches(input, artistAlbumIdspattern, options))
                {
                    string albumIdDiscog = string.Format("{0}", m.Groups["albumIds"].Value);

                    WebRequest wr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/album/get?album_id=" + albumIdDiscog + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                    // Empty output, then say Starting Downloads.
                    output.Invoke(new Action(() => output.Text = String.Empty));
                    output.Invoke(new Action(() => output.AppendText("FAVORITE DOWNLOADS MAY HAVE SOME ERRORS, THIS IS A NEW FEATURE, AND CURRENTLY ONLY SUPPORTS FAVORITED ALBUMS. IF YOU RUN INTO AN ISSUE, PLEASE REPORT IT ON GITHUB!\r\n")));
                    output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

                    try
                    {
                        // Make sure buttons are disabled during downloads.
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = false));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = false));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = false));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = false));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = false));

                        // Set "loc" as the selected path.
                        loc = folderBrowserDialog.SelectedPath;

                        WebResponse ws = wr.GetResponse();
                        StreamReader sr = new StreamReader(ws.GetResponseStream());

                        string albumRequest = sr.ReadToEnd();

                        string text = albumRequest;

                        var tracksLog = Regex.Match(albumRequest, "tracks_count\":(?<numoftracks>\\d+)").Groups;
                        var tracks = tracksLog[1].Value;

                        // Album Name tag
                        var labelDiscogAlbumLog = Regex.Match(albumRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                        var labelDiscogAlbum = labelDiscogAlbumLog[1].Value;

                        // For converting unicode characters to ASCII
                        string unicodeDiscogAlbum = labelDiscogAlbum;
                        string decodedDiscogAlbum = DecodeEncodedNonAsciiCharacters(unicodeDiscogAlbum);
                        labelDiscogAlbum = decodedDiscogAlbum;

                        output.Invoke(new Action(() => output.AppendText("Downloading Album - " + labelDiscogAlbum + " ......\r\n\r\n")));

                        #region Cover Art URL
                        // Grab Cover Art URL
                        var frontCoverLog = Regex.Match(albumRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
                        var frontCover = frontCoverLog[1].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string imagepattern = @"(?<imageUrlFix>[^\\]+)";
                        string imageinput = frontCover;
                        RegexOptions imageoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
                        }

                        string frontCoverImg = imageURLTextbox.Text;
                        string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
                        frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

                        albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        #region "Goodies" URL (Digital Booklets)
                        // Look for "Goodies" (digital booklet)
                        var goodiesLog = Regex.Match(albumRequest, "\"goodies\":\\[{(?<notUsed>.*?),\"url\":\"(?<booklet>.*?)\",").Groups;
                        var goodies = goodiesLog[2].Value;

                        // Remove backslashes from the stream URL to have a proper URL.
                        string bookpattern = @"(?<imageUrlFix>[^\\]+)";
                        string bookinput = goodies;
                        RegexOptions bookoptions = RegexOptions.Multiline;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

                        foreach (Match mBook in Regex.Matches(bookinput, bookpattern, bookoptions))
                        {
                            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mBook.Value))));
                        }

                        string goodiesPDF = imageURLTextbox.Text;

                        imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
                        #endregion

                        // Grab sample rate and bit depth for album.
                        var qualityLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

                        var bitDepthLog = Regex.Match(albumRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
                        var sampleRateLog = Regex.Match(albumRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

                        var bitDepth = bitDepthLog[1].Value;
                        var sampleRate = sampleRateLog[1].Value;
                        var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
                        var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

                        if (formatIdString == "5")
                        {
                            quality = "MP3 320kbps CBR";
                            qualityPath = "MP3";
                        }
                        else if (formatIdString == "6")
                        {
                            quality = "FLAC (16bit/44.1kHz)";
                            qualityPath = "FLAC (16bit-44.1kHz)";
                        }
                        else if (formatIdString == "7")
                        {
                            if (quality == "FLAC (24bit/192kHz)")
                            {
                                quality = "FLAC (24bit/96kHz)";
                                qualityPath = "FLAC (24bit-96kHz)";
                            }
                        }

                        // Grab all Track IDs listed on the API.
                        string trackIdspattern = "\"version\":(?:.*?),\"id\":(?<trackId>.*?),";
                        string trackinput = text;
                        RegexOptions trackoptions = RegexOptions.Multiline;


                        foreach (Match mtrack in Regex.Matches(trackinput, trackIdspattern, trackoptions))
                        {
                            // Set default value for max length.
                            const int MaxLength = 36;

                            //output.Invoke(new Action(() => output.AppendText(string.Format("{0}\r\n", m.Groups["trackId"].Value))));
                            trackIdString = string.Format("{0}", mtrack.Groups["trackId"].Value);

                            WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + trackIdString + "&app_id=" + appid + "&user_auth_token=" + userAuth);

                            WebResponse trackws = trackwr.GetResponse();
                            StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

                            string trackRequest = tracksr.ReadToEnd();

                            #region Availability Check (Valid Link?)
                            // Check if available at all.
                            var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
                            var errorCheck = errorCheckLog[1].Value;

                            if (errorCheck == "No result matching given argument")
                            {
                                output.Invoke(new Action(() => output.Text = String.Empty));
                                output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                                output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                                return;
                            }
                            #endregion

                            // Display album quality in quality textbox.
                            qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

                            #region Get Information (Tags, Titles, etc.)
                            // Track Number tag
                            var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
                            var trackNumber = trackNumberLog[1].Value;

                            // Disc Number tag
                            var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
                            var discNumber = discNumberLog[1].Value;

                            // Album Artist tag
                            var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
                            var albumArtist = albumArtistLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumArtist = albumArtist;
                            string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
                            albumArtist = decodedAlbumArtist;

                            albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumArtistPath = albumArtist.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album artist in text box under cover art.
                            albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

                            // If name goes over 200 characters, limit it to 200
                            if (albumArtistPath.Length > MaxLength)
                            {
                                albumArtistPath = albumArtistPath.Substring(0, MaxLength);
                            }

                            // Track Artist tag
                            var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
                            var performerName = performerNameLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodePerformerName = performerName;
                            string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
                            performerName = decodedPerformerName;

                            performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (performerNamePath.Length > MaxLength)
                            {
                                performerNamePath = performerNamePath.Substring(0, MaxLength);
                            }

                            // Track Composer tag
                            var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
                            var composerName = composerNameLog[2].Value;

                            // Track Explicitness 
                            var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
                            var advisory = advisoryLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeComposerName = composerName;
                            string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
                            composerName = decodedComposerName;

                            composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Album Name tag
                            var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
                            var albumName = albumNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeAlbumName = albumName;
                            string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
                            albumName = decodedAlbumName;

                            albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Display album name in text box under cover art.
                            albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

                            // If name goes over 200 characters, limit it to 200
                            if (albumNamePath.Length > MaxLength)
                            {
                                albumNamePath = albumNamePath.Substring(0, MaxLength);
                            }

                            // Track Name tag
                            var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
                            var trackName = trackNameLog[2].Value;
                            trackName = trackName.Trim(); // Remove spaces from end of track name

                            // For converting unicode characters to ASCII
                            string unicodeTrackName = trackName;
                            string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
                            trackName = decodedTrackName;

                            trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // If name goes over 200 characters, limit it to 200
                            if (trackNamePath.Length > MaxLength)
                            {
                                trackNamePath = trackNamePath.Substring(0, MaxLength);
                            }

                            // Version Name tag
                            var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
                            var versionName = versionNameLog[1].Value;

                            // For converting unicode characters to ASCII
                            string unicodeVersionName = versionName;
                            string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
                            versionName = decodedVersionName;

                            versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
                            var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

                            // Genre tag
                            var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
                            var genre = genreLog[3].Value;

                            // For converting unicode characters to ASCII
                            string unicodeGenre = genre;
                            string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
                            genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

                            // Release Date tag, grabs the available "stream" date
                            var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
                            var releaseDate = releaseDateLog[1].Value;

                            // Display release date in text box under cover art.
                            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

                            // Copyright tag
                            var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
                            var copyright = copyrightLog[2].Value;

                            // For converting unicode characters to ASCII
                            string unicodeCopyright = copyright;
                            string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
                            copyright = decodedCopyright;

                            copyright = copyright.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

                            // UPC tag
                            var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
                            var upc = upcLog[1].Value;

                            // Display UPC in text box under cover art.
                            upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

                            // ISRC tag
                            var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
                            var isrc = isrcLog[1].Value;

                            // Total Tracks tag
                            var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+)").Groups;
                            var trackTotal = trackTotalLog[1].Value;

                            // Display Total Tracks in text box under cover art.
                            totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

                            // Total Discs tag
                            var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
                            var discTotal = discTotalLog[1].Value;
                            #endregion

                            #region Filename Number Padding
                            // Set default track number padding length
                            var paddingLength = 2;

                            // Prepare track number padding in filename.
                            string paddingLog = trackTotal.Length.ToString();
                            if (paddingLog == "1")
                            {
                                paddingLength = 2;
                            }
                            else
                            {
                                paddingLength = trackTotal.Length;
                            }

                            // Set default disc number padding length
                            var paddingDiscLength = 2;

                            // Prepare disc number padding in filename.
                            string paddingDiscLog = discTotal.Length.ToString();
                            if (paddingDiscLog == "1")
                            {
                                paddingDiscLength = 1;
                            }
                            else
                            {
                                paddingDiscLength = discTotal.Length;
                            }
                            #endregion

                            #region Create Directories
                            // Create strings for disc folders
                            string discFolder = null;
                            string discFolderCreate = null;

                            // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
                            if (discTotal != "1")
                            {
                                discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                                discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                            }

                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]");
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath);
                            System.IO.Directory.CreateDirectory(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate);

                            string discogPath = loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + discFolderCreate;
                            #endregion

                            #region Availability Check (Streamable?)
                            // Check if available for streaming.
                            var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
                            var streamCheck = streamCheckLog[2].Value;

                            if (streamCheck != "true")
                            {
                                if (streamableCheckbox.Checked == true)
                                {
                                    output.Invoke(new Action(() => output.AppendText("Track " + trackNumber + " \"" + trackName + "\" is not available for streaming. Skipping track...\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("\r\nTrack " + trackNumber + " \"" + trackName + "\" is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                                }
                            }
                            #endregion

                            #region Check if File Exists
                            // Check if there is a version name.
                            if (versionName == null | versionName == "")
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            else
                            {
                                if (System.IO.File.Exists(discogPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                {
                                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                                    System.Threading.Thread.Sleep(400);
                                    continue;
                                }
                            }
                            #endregion

                            // Close web request and create streaming URL.
                            trackwr.Abort();
                            createURL(sender, e);

                            try
                            {
                                #region Downloading
                                // Check if there is a version name.
                                if (versionName == null | versionName == "")
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                                }
                                else
                                {
                                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                                }
                                // Being download process.
                                var client = new HttpClient();
                                // Run through TLS to allow secure connection.
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                                // Set "range" header to nearly unlimited.
                                client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                                // Set user-agent to Firefox.
                                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                                // Set referer URL to album ID.
                                client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumIdDiscog);
                                // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                                using (var stream = await client.GetStreamAsync(testURLBox.Text))

                                    // Save single track in selected path.
                                    if (versionNamePath == null | versionNamePath == "")
                                    {
                                        // If there is NOT a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        using (var output = System.IO.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                                        {
                                            await stream.CopyToAsync(output);
                                        }
                                    }
                                #endregion

                                #region Cover Art Saving
                                if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg"))
                                {
                                    // Skip, don't re-download.

                                    // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                    using (WebClient imgClient = new WebClient())
                                    {
                                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                    }
                                }
                                else
                                {
                                    if (imageCheckbox.Checked == true)
                                    {
                                        // Save cover art to selected path (Currently happens every time a track is downloaded).
                                        using (WebClient imgClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Cover.jpg");

                                            // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                                            imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                                        }
                                    }
                                }
                                #endregion

                                #region Tagging
                                // Check if audio file type is FLAC or MP3
                                if (audioFileType == ".mp3")
                                {
                                    #region MP3 Tagging (Needs Work)
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for MP3 file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to MP3 file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Performers = new string[] { performerName };
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Composers = new string[] { composerName };
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            releaseDate = releaseDate.Substring(0, 4);
                                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Genres = new string[] { genre };
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Comment = commentTextbox.Text;
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Copyright = copyright;
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                                            tag.SetTextFrame("TSRC", isrc);
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            // Not available on MP3 at the moment
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region FLAC Tagging
                                    // Select the downloaded file to prepare for tagging.
                                    // Check if there's a version name or not
                                    if (versionName == null | versionName == "")
                                    {
                                        // If there is NOT a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName;
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    else
                                    {
                                        // If there is a version name.
                                        var tfile = TagLib.File.Create(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                                        // For custom / troublesome tags.
                                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                                        // Saving cover art to file(s)
                                        if (imageCheckbox.Checked == true)
                                        {
                                            // Define cover art to use for FLAC file(s)
                                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                                            pic.TextEncoding = TagLib.StringType.Latin1;
                                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                                            pic.Type = TagLib.PictureType.FrontCover;
                                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");

                                            // Save cover art to FLAC file.
                                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                                            tfile.Save();
                                        }

                                        // Track Title tag
                                        if (trackTitleCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                                        }

                                        // Album Title tag
                                        if (albumCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Album = albumName;
                                        }

                                        // Album Artits tag
                                        if (albumArtistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                                        }

                                        // Track Artist tag
                                        if (artistCheckbox.Checked == true)
                                        {
                                            custom.SetField("ARTIST", new string[] { performerName });
                                        }

                                        // Composer tag
                                        if (composerCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMPOSER", new string[] { composerName });
                                        }

                                        // Release Date tag
                                        if (releaseCheckbox.Checked == true)
                                        {
                                            custom.SetField("YEAR", new string[] { releaseDate });
                                        }

                                        // Genre tag
                                        if (genreCheckbox.Checked == true)
                                        {
                                            custom.SetField("GENRE", new string[] { genre });
                                        }

                                        // Track Number tag
                                        if (trackNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                                        }

                                        // Disc Number tag
                                        if (discNumberCheckbox.Checked == true)
                                        {
                                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                                        }

                                        // Total Discs tag
                                        if (discTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                                        }

                                        // Total Tracks tag
                                        if (trackTotalCheckbox.Checked == true)
                                        {
                                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                                        }

                                        // Comment tag
                                        if (commentCheckbox.Checked == true)
                                        {
                                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                                        }

                                        // Copyright tag
                                        if (copyrightCheckbox.Checked == true)
                                        {
                                            custom.SetField("COPYRIGHT", new string[] { copyright });
                                        }
                                        // UPC tag
                                        if (upcCheckbox.Checked == true)
                                        {
                                            custom.SetField("UPC", new string[] { upc });
                                        }

                                        // ISRC tag
                                        if (isrcCheckbox.Checked == true)
                                        {
                                            custom.SetField("ISRC", new string[] { isrc });
                                        }

                                        // Explicit tag
                                        if (explicitCheckbox.Checked == true)
                                        {
                                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                                        }

                                        // Save all selected tags to file
                                        tfile.Save();
                                    }
                                    #endregion
                                }
                                #endregion

                                #region Digital Booklet
                                // If a booklet was found, save it.
                                if (goodiesPDF == null | goodiesPDF == "")
                                {
                                    // No need to download something that doesn't exist.
                                }
                                else
                                {
                                    if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf"))
                                    {
                                        // Skip, don't re-download.
                                    }
                                    else
                                    {
                                        // Save digital booklet to selected path
                                        output.Invoke(new Action(() => output.AppendText("\r\nGoodies found, downloading...")));
                                        using (WebClient bookClient = new WebClient())
                                        {
                                            // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                                            bookClient.DownloadFile(new Uri(goodiesPDF), loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + "Digital Booklet.pdf");
                                        }
                                    }
                                }
                                #endregion
                            }
                            catch (Exception downloadError)
                            {
                                // If there is an issue trying to, or during the download, show error info.
                                string error = downloadError.ToString();
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                                output.Invoke(new Action(() => output.AppendText(error)));
                                output.Invoke(new Action(() => output.AppendText("\r\n\r\nIf some tracks aren't available for streaming on the album you're trying to download, try to manually download the available tracks individually.")));
                                
                            }

                            // Delete image file used for tagging
                            if (System.IO.File.Exists(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg"))
                            {
                                System.IO.File.Delete(loc + "\\" + "- Favorites" + "\\" + albumArtistPath + "\\" + albumNamePath + " [" + albumIdDiscog + "]" + "\\" + qualityPath + "\\" + artSize + ".jpg");
                            }

                            // Say when a track is done downloading, then wait for the next track / end.
                            output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n")));
                            System.Threading.Thread.Sleep(400);
                        }

                        // Say that downloading is completed.
                        output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText("Downloading job completed! All downloaded files will be located in your chosen path.")));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                        flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                        flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                        downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    }
                    catch (Exception ex)
                    {
                        string error = ex.ToString();
                        output.Invoke(new Action(() => output.Text = String.Empty));
                        output.Invoke(new Action(() => output.AppendText("Failed to download (First Phase). Error information below.\r\n\r\n")));
                        output.Invoke(new Action(() => output.AppendText(error)));
                        mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                        
                    }
                }
            }
            catch (Exception downloadError)
            {
                // If there is an issue trying to, or during the download, show error info.
                string error = downloadError.ToString();
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("Label Download ERROR. Information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
                
            }
            #endregion
        }

    
    // For downloading "track" links
    private async void downloadTrackBG_DoWork(object sender, DoWorkEventArgs e)
        {
            #region If URL has "track"
            // Set default value for max length.
            const int MaxLength = 36;

            // Set "loc" as the selected path.
            String loc = folderBrowserDialog.SelectedPath;

            // Set Track ID to the ID in the provided Qobuz link.
            trackIdString = albumId;

            WebRequest trackwr = WebRequest.Create("https://www.qobuz.com/api.json/0.2/track/get?track_id=" + albumId + "&app_id=" + appid + "&user_auth_token=" + userAuth);

            // Empty output, then say Starting Downloads.
            output.Invoke(new Action(() => output.Text = String.Empty));
            output.Invoke(new Action(() => output.AppendText("Starting Downloads...\r\n\r\n")));

            WebResponse trackws = trackwr.GetResponse();
            StreamReader tracksr = new StreamReader(trackws.GetResponseStream());

            string trackRequest = tracksr.ReadToEnd();

            // Grab sample rate and bit depth for album track is from.
            var qualityLog = Regex.Match(trackRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),(?:.*?)\"maximum_bit_depth\":(?<bitDepth>.*?),\"duration\"").Groups;

            var bitDepthLog = Regex.Match(trackRequest, "\"maximum_bit_depth\":(?<bitDepth>.*?),").Groups;
            var sampleRateLog = Regex.Match(trackRequest, "\"maximum_sampling_rate\":(?<sampleRate>.*?),").Groups;

            var bitDepth = bitDepthLog[1].Value;
            var sampleRate = sampleRateLog[1].Value;
            var quality = "FLAC (" + bitDepth + "bit/" + sampleRate + "kHz)";
            var qualityPath = quality.Replace(@"\", "-").Replace(@"/", "-");

            if (formatIdString == "5")
            {
                quality = "MP3 320kbps CBR";
                qualityPath = "MP3";
            }
            else if (formatIdString == "6")
            {
                quality = "FLAC (16bit/44.1kHz)";
                qualityPath = "FLAC (16bit-44.1kHz)";
            }
            else if (formatIdString == "7")
            {
                if (quality == "FLAC (24bit/192kHz)")
                {
                    quality = "FLAC (24bit/96kHz)";
                    qualityPath = "FLAC (24bit-96kHz)";
                }
            }

            #region Cover Art URL
            // Grab Cover Art URL
            var frontCoverLog = Regex.Match(trackRequest, "\"image\":{\"large\":\"(?<frontCover>[A-Za-z0-9:().,\\\\\\/._\\-']+)").Groups;
            var frontCover = frontCoverLog[1].Value;

            // Remove backslashes from the stream URL to have a proper URL.
            string imagepattern = @"(?<imageUrlFix>[^\\]+)";
            string imageinput = frontCover;
            RegexOptions imageoptions = RegexOptions.Multiline;

            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = String.Empty));

            foreach (Match mImg in Regex.Matches(imageinput, imagepattern, imageoptions))
            {
                imageURLTextbox.Invoke(new Action(() => imageURLTextbox.AppendText(string.Format("{0}", mImg.Value))));
            }

            string frontCoverImg = imageURLTextbox.Text;
            string frontCoverImgBox = frontCoverImg.Replace("_600.jpg", "_150.jpg");
            frontCoverImg = frontCoverImg.Replace("_600.jpg", "_max.jpg");

            albumArtPicBox.Invoke(new Action(() => albumArtPicBox.ImageLocation = frontCoverImgBox));

            imageURLTextbox.Invoke(new Action(() => imageURLTextbox.Text = Settings.Default.savedEmail));
            #endregion

            #region Availability Check (Valid Link?)
            // Check if available at all.
            var errorCheckLog = Regex.Match(trackRequest, "\"code\":404,\"message\":\"(?<error>.*?)\\\"").Groups;
            var errorCheck = errorCheckLog[1].Value;

            if (errorCheck == "No result matching given argument")
            {
                output.Invoke(new Action(() => output.Text = String.Empty));
                output.Invoke(new Action(() => output.AppendText("ERROR: 404\r\n")));
                output.Invoke(new Action(() => output.AppendText("Error message is \"No result matching given argument\"\r\n")));
                output.Invoke(new Action(() => output.AppendText("This could mean either the link is invalid, or isn't available in the region you're downloading from (even if the account is in the correct region). If the latter is true, use a VPN for the region it's available in to download.")));
                mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                return;
            }
            #endregion

            // Display album quality in quality textbox.
            qualityTextbox.Invoke(new Action(() => qualityTextbox.Text = quality));

            #region Get Information (Tags, Titles, etc.)
            // Track Number tag
            var trackNumberLog = Regex.Match(trackRequest, "\"track_number\":(?<trackNumber>[0-9]+)").Groups;
            var trackNumber = trackNumberLog[1].Value;

            // Disc Number tag
            var discNumberLog = Regex.Match(trackRequest, "\"media_number\":(?<discNumber>.*?),\\\"").Groups;
            var discNumber = discNumberLog[1].Value;

            // Album Artist tag
            var albumArtistLog = Regex.Match(trackRequest, "\"artist\":{(?<notUsed>.*?)\"name\":\"(?<albumArtist>.*?)\",").Groups;
            var albumArtist = albumArtistLog[2].Value;

            // For converting unicode characters to ASCII
            string unicodeAlbumArtist = albumArtist;
            string decodedAlbumArtist = DecodeEncodedNonAsciiCharacters(unicodeAlbumArtist);
            albumArtist = decodedAlbumArtist;

            albumArtist = albumArtist.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
            var albumArtistPath = albumArtist.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

            // Display album artist in text box under cover art.
            albumArtistTextBox.Invoke(new Action(() => albumArtistTextBox.Text = albumArtist));

            // If name goes over 200 characters, limit it to 200
            if (albumArtistPath.Length > MaxLength)
            {
                albumArtistPath = albumArtistPath.Substring(0, MaxLength);
            }

            // Track Artist tag
            var performerNameLog = Regex.Match(trackRequest, "\"performer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<trackArtist>.*?)\"},\\\"").Groups;
            var performerName = performerNameLog[2].Value;

            // For converting unicode characters to ASCII
            string unicodePerformerName = performerName;
            string decodedPerformerName = DecodeEncodedNonAsciiCharacters(unicodePerformerName);
            performerName = decodedPerformerName;

            performerName = performerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
            var performerNamePath = performerName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

            // If name goes over 200 characters, limit it to 200
            if (performerNamePath.Length > MaxLength)
            {
                performerNamePath = performerNamePath.Substring(0, MaxLength);
            }

            // Track Composer tag
            var composerNameLog = Regex.Match(trackRequest, "\"composer\":{\"id\":(?<notUsed>.*?),\"name\":\"(?<composer>.*?)\",").Groups;
            var composerName = composerNameLog[2].Value;

            // Track Explicitness 
            var advisoryLog = Regex.Match(trackRequest, "\"performers\":(?:.*?)\"parental_warning\":(?<advisory>.*?),").Groups;
            var advisory = advisoryLog[1].Value;

            // For converting unicode characters to ASCII
            string unicodeComposerName = composerName;
            string decodedComposerName = DecodeEncodedNonAsciiCharacters(unicodeComposerName);
            composerName = decodedComposerName;

            composerName = composerName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

            // Album Name tag
            var albumNameLog = Regex.Match(trackRequest, "\"title\":\"(?<albumTitle>.*?)\",\\\"").Groups;
            var albumName = albumNameLog[1].Value;

            // For converting unicode characters to ASCII
            string unicodeAlbumName = albumName;
            string decodedAlbumName = DecodeEncodedNonAsciiCharacters(unicodeAlbumName);
            albumName = decodedAlbumName;

            albumName = albumName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
            var albumNamePath = albumName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

            // Display album name in text box under cover art.
            albumTextBox.Invoke(new Action(() => albumTextBox.Text = albumName));

            // If name goes over 200 characters, limit it to 200
            if (albumNamePath.Length > MaxLength)
            {
                albumNamePath = albumNamePath.Substring(0, MaxLength);
            }

            // Track Name tag
            var trackNameLog = Regex.Match(trackRequest, "\"isrc\":\"(?<notUsed>.*?)\",\"title\":\"(?<trackName>.*?)\",\"").Groups;
            var trackName = trackNameLog[2].Value;
            trackName = trackName.Trim(); // Remove spaces from end of track name

            // For converting unicode characters to ASCII
            string unicodeTrackName = trackName;
            string decodedTrackName = DecodeEncodedNonAsciiCharacters(unicodeTrackName);
            trackName = decodedTrackName;

            trackName = trackName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
            var trackNamePath = trackName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

            // If name goes over 200 characters, limit it to 200
            if (trackNamePath.Length > MaxLength)
            {
                trackNamePath = trackNamePath.Substring(0, MaxLength).ToString();
            }

            // Version Name tag
            var versionNameLog = Regex.Match(trackRequest, "\"version\":\"(?<version>.*?)\",\\\"").Groups;
            var versionName = versionNameLog[1].Value;

            // For converting unicode characters to ASCII
            string unicodeVersionName = versionName;
            string decodedVersionName = DecodeEncodedNonAsciiCharacters(unicodeVersionName);
            versionName = decodedVersionName;

            versionName = versionName.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");
            var versionNamePath = versionName.Replace("\\\"", "-").Replace("\"", "-").Replace(@"\", "-").Replace(@"/", "-").Replace(":", "-").Replace("<", "-").Replace(">", "-").Replace("|", "-").Replace("?", "-").Replace("*", "-");

            // Genre tag
            var genreLog = Regex.Match(trackRequest, "\"genre\":{\"id\":(?<notUsed>.*?),\"color\":\"(?<notUsed2>.*?)\",\"name\":\"(?<genreName>.*?)\",\\\"").Groups;
            var genre = genreLog[3].Value;

            // For converting unicode characters to ASCII
            string unicodeGenre = genre;
            string decodedGenre = DecodeEncodedNonAsciiCharacters(unicodeGenre);
            genre = decodedGenre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

            genre = genre.Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/");

            // Release Date tag, grabs the available "stream" date
            var releaseDateLog = Regex.Match(trackRequest, "\"release_date_stream\":\"(?<releaseDate>.*?)\",\\\"").Groups;
            var releaseDate = releaseDateLog[1].Value;

            // Display release date in text box under cover art.
            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

            // Display release date in text box under cover art.
            releaseDateTextBox.Invoke(new Action(() => releaseDateTextBox.Text = releaseDate));

            // Copyright tag
            var copyrightLog = Regex.Match(trackRequest, "\"copyright\":\"(?<notUsed>.*?)\"copyright\":\"(?<copyrigh>.*?)\\\"").Groups;
            var copyright = copyrightLog[2].Value;

            // For converting unicode characters to ASCII
            string unicodeCopyright = copyright;
            string decodedCopyright = DecodeEncodedNonAsciiCharacters(unicodeCopyright);
            copyright = decodedCopyright;

            copyright = copyright.Replace("\\/", @"/").Replace(@"\/", @"/").Replace("\\\"", "\"").Replace(@"\\/", @"/").Replace(@"\\", @"\").Replace(@"\/", @"/").Replace(@"\u2117", @"℗");

            // UPC tag
            var upcLog = Regex.Match(trackRequest, "\"upc\":\"(?<upc>.*?)\",\\\"").Groups;
            var upc = upcLog[1].Value;

            // Display UPC in text box under cover art.
            upcTextBox.Invoke(new Action(() => upcTextBox.Text = upc));

            // ISRC tag
            var isrcLog = Regex.Match(trackRequest, "\"isrc\":\"(?<isrc>.*?)\",\\\"").Groups;
            var isrc = isrcLog[1].Value;

            // Total Tracks tag
            var trackTotalLog = Regex.Match(trackRequest, "\"tracks_count\":(?<trackCount>[0-9]+),").Groups;
            var trackTotal = trackTotalLog[1].Value;
            totalTracksTextbox.Invoke(new Action(() => totalTracksTextbox.Text = trackTotal));

            // Total Discs tag
            var discTotalLog = Regex.Match(trackRequest, "\"media_count\":(?<discTotal>[0-9]+)").Groups;
            var discTotal = discTotalLog[1].Value;
            #endregion

            #region Filename Number Padding
            // Set default track number padding length
            var paddingLength = 2;

            // Prepare track number padding in filename.
            string paddingLog = trackTotal.Length.ToString();
            if (paddingLog == "1")
            {
                paddingLength = 2;
            }
            else
            {
                paddingLength = trackTotal.Length;
            }

            // Set default disc number padding length
            var paddingDiscLength = 2;

            // Prepare disc number padding in filename.
            string paddingDiscLog = discTotal.Length.ToString();
            if (paddingDiscLog == "1")
            {
                paddingDiscLength = 1;
            }
            else
            {
                paddingDiscLength = discTotal.Length;
            }
            #endregion

            #region Create Directories
            // Create strings for disc folders
            string discFolder = null;
            string discFolderCreate = null;

            // If more than 1 disc, create folders for discs. Otherwise, strings will remain null.
            if (discTotal != "1")
            {
                discFolder = "CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
                discFolderCreate = "\\CD " + discNumber.PadLeft(paddingDiscLength, '0') + "\\";
            }

            // Create directories
            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath);
            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath);
            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath);
            System.IO.Directory.CreateDirectory(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + discFolderCreate);

            // Set albumPath to the created directories.
            string trackPath = loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + discFolderCreate;
            #endregion

            #region Availability Check (Streamable?)
            // Check if available for streaming.
            var streamCheckLog = Regex.Match(trackRequest, "\"track_number\":(?<notUsed>.*?)\"streamable\":(?<streamCheck>.*?),\"").Groups;
            var streamCheck = streamCheckLog[2].Value;

            if (streamCheck != "true")
            {
                if (streamableCheckbox.Checked == true)
                {
                    output.Invoke(new Action(() => output.AppendText("Track is not available for streaming. Unable to download.\r\n")));
                    System.Threading.Thread.Sleep(400);
                    mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                    flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                    flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                    flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                    downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    return;
                }
                else
                {
                    output.Invoke(new Action(() => output.AppendText("Track is not available for streaming. But stremable check is being ignored for debugging, or messed up releases. Attempting to download...\r\n")));
                }
            }
            #endregion

            #region Check if File Exists
            // Check if there is a version name.
            if (versionName == null | versionName == "")
            {
                if (System.IO.File.Exists(trackPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                {
                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + "\" already exists. Skipping.\r\n")));
                    System.Threading.Thread.Sleep(400);
                    mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                    flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                    flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                    flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                    downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    return;
                }
            }
            else
            {
                if (System.IO.File.Exists(trackPath + "\\" + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                {
                    output.Invoke(new Action(() => output.AppendText("File for \"" + trackNumber.PadLeft(paddingLength, '0') + " " + trackName + " (" + versionName + ")" + "\" already exists. Skipping.\r\n")));
                    System.Threading.Thread.Sleep(400);
                    mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
                    flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
                    flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
                    flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
                    downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
                    return;
                }
            }
            #endregion

            // Close web request and create streaming URL.
            trackwr.Abort();
            createURL(sender, e);

            try
            {

                #region Downloading
                // Check if there is a version name.
                if (versionName == null | versionName == "")
                {
                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " ......")));
                }
                else
                {
                    output.Invoke(new Action(() => output.AppendText("Downloading - " + trackNumber.PadLeft(paddingLength, '0') + " - " + trackName + " (" + versionName + ")" + " ......")));
                }
                // Being download process.
                var client = new HttpClient();
                // Run through TLS to allow secure connection.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                // Set "range" header to nearly unlimited.
                client.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 999999999999);
                // Set user-agent to Firefox.
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0");
                // Set referer URL to album ID.
                client.DefaultRequestHeaders.Add("Referer", "https://play.qobuz.com/album/" + albumId);
                // Download the URL in the "Streamed URL" Textbox (Will most likely be replaced).
                using (var stream = await client.GetStreamAsync(testURLBox.Text))

                    // Save single track in selected path.
                    if (versionNamePath == null | versionNamePath == "")
                    {
                        // If there is NOT a version name.
                        using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType))
                        {
                            await stream.CopyToAsync(output);
                        }
                    }
                    else
                    {
                        // If there is a version name.
                        using (var output = System.IO.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType))
                        {
                            await stream.CopyToAsync(output);
                        }
                    }
                #endregion

                #region Cover Art Saving
                if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Cover.jpg"))
                {
                    // Skip, don't re-download.

                    // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                    using (WebClient imgClient = new WebClient())
                    {
                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
                    }
                }
                else
                {
                    // Save cover art to selected path.
                    using (WebClient imgClient = new WebClient())
                    {
                        // Download max quality Cover Art to "Cover.jpg" file in chosen path. 
                        imgClient.DownloadFile(new Uri(frontCoverImg), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + "Cover.jpg");

                        // Download selected cover art size for tagging files (Currently happens every time a track is downloaded).
                        imgClient.DownloadFile(new Uri(frontCoverImg.Replace("_max", "_" + artSize)), loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
                    }
                }
                #endregion

                #region Tagging
                // Check if audio file type is FLAC or MP3
                if (audioFileType == ".mp3")
                {
                    #region MP3 Tagging (Needs Work)
                    // Select the downloaded file to prepare for tagging.
                    // Check if there's a version name or not
                    if (versionName == null | versionName == "")
                    {
                        // If there is NOT a version name.
                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                        // For custom / troublesome tags.
                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                        // Saving cover art to file(s)
                        if (imageCheckbox.Checked == true)
                        {
                            // Define cover art to use for MP3 file(s)
                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                            pic.TextEncoding = TagLib.StringType.Latin1;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                            // Save cover art to MP3 file.
                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                            tfile.Save();
                        }

                        // Track Title tag
                        if (trackTitleCheckbox.Checked == true)
                        {
                            tfile.Tag.Title = trackName;
                        }

                        // Album Title tag
                        if (albumCheckbox.Checked == true)
                        {
                            tfile.Tag.Album = albumName;
                        }

                        // Album Artits tag
                        if (albumArtistCheckbox.Checked == true)
                        {
                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                        }

                        // Track Artist tag
                        if (artistCheckbox.Checked == true)
                        {
                            tfile.Tag.Performers = new string[] { performerName };
                        }

                        // Composer tag
                        if (composerCheckbox.Checked == true)
                        {
                            tfile.Tag.Composers = new string[] { composerName };
                        }

                        // Release Date tag
                        if (releaseCheckbox.Checked == true)
                        {
                            releaseDate = releaseDate.Substring(0, 4);
                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                        }

                        // Genre tag
                        if (genreCheckbox.Checked == true)
                        {
                            tfile.Tag.Genres = new string[] { genre };
                        }

                        // Track Number tag
                        if (trackNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                        }

                        // Disc Number tag
                        if (discNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                        }

                        // Total Discs tag
                        if (discTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                        }

                        // Total Tracks tag
                        if (trackTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                        }

                        // Comment tag
                        if (commentCheckbox.Checked == true)
                        {
                            tfile.Tag.Comment = commentTextbox.Text;
                        }

                        // Copyright tag
                        if (copyrightCheckbox.Checked == true)
                        {
                            tfile.Tag.Copyright = copyright;
                        }
                        // UPC tag
                        if (upcCheckbox.Checked == true)
                        {
                            // Not available on MP3 at the moment
                        }

                        // ISRC tag
                        if (isrcCheckbox.Checked == true)
                        {
                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                            tag.SetTextFrame("TSRC", isrc);
                        }

                        // Explicit tag
                        if (explicitCheckbox.Checked == true)
                        {
                            // Not available on MP3 at the moment
                        }

                        // Save all selected tags to file
                        tfile.Save();
                    }
                    else
                    {
                        // If there is a version name.
                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                        // For custom / troublesome tags.
                        TagLib.Id3v2.Tag t = (TagLib.Id3v2.Tag)tfile.GetTag(TagLib.TagTypes.Id3v2);


                        // Saving cover art to file(s)
                        if (imageCheckbox.Checked == true)
                        {
                            // Define cover art to use for FLAC file(s)
                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                            pic.TextEncoding = TagLib.StringType.Latin1;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                            // Save cover art to FLAC file.
                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                            tfile.Save();
                        }

                        // Track Title tag
                        if (trackTitleCheckbox.Checked == true)
                        {
                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                        }

                        // Album Title tag
                        if (albumCheckbox.Checked == true)
                        {
                            tfile.Tag.Album = albumName;
                        }

                        // Album Artits tag
                        if (albumArtistCheckbox.Checked == true)
                        {
                            tfile.Tag.AlbumArtists = new string[] { albumArtist };
                        }

                        // Track Artist tag
                        if (artistCheckbox.Checked == true)
                        {
                            tfile.Tag.Performers = new string[] { performerName };
                        }

                        // Composer tag
                        if (composerCheckbox.Checked == true)
                        {
                            tfile.Tag.Composers = new string[] { composerName };
                        }

                        // Release Date tag
                        if (releaseCheckbox.Checked == true)
                        {
                            releaseDate = releaseDate.Substring(0, 4);
                            tfile.Tag.Year = UInt32.Parse(releaseDate);
                        }

                        // Genre tag
                        if (genreCheckbox.Checked == true)
                        {
                            tfile.Tag.Genres = new string[] { genre };
                        }

                        // Track Number tag
                        if (trackNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                        }

                        // Disc Number tag
                        if (discNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                        }

                        // Total Discs tag
                        if (discTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                        }

                        // Total Tracks tag
                        if (trackTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                        }

                        // Comment tag
                        if (commentCheckbox.Checked == true)
                        {
                            tfile.Tag.Comment = commentTextbox.Text;
                        }

                        // Copyright tag
                        if (copyrightCheckbox.Checked == true)
                        {
                            tfile.Tag.Copyright = copyright;
                        }
                        // UPC tag
                        if (upcCheckbox.Checked == true)
                        {
                            // Not available on MP3 at the moment
                        }

                        // ISRC tag
                        if (isrcCheckbox.Checked == true)
                        {
                            TagLib.Id3v2.Tag tag = (TagLib.Id3v2.Tag)tfile.GetTag(TagTypes.Id3v2, true);
                            tag.SetTextFrame("TSRC", isrc);
                        }

                        // Explicit tag
                        if (explicitCheckbox.Checked == true)
                        {
                            // Not available on MP3 at the moment
                        }

                        // Save all selected tags to file
                        tfile.Save();
                    }
                    #endregion
                }
                else
                {
                    #region FLAC Tagging
                    // Select the downloaded file to prepare for tagging.
                    // Check if there's a version name or not
                    if (versionName == null | versionName == "")
                    {
                        // If there is NOT a version name.
                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + audioFileType);
                        // For custom / troublesome tags.
                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                        // Saving cover art to file(s)
                        if (imageCheckbox.Checked == true)
                        {
                            // Define cover art to use for FLAC file(s)
                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                            pic.TextEncoding = TagLib.StringType.Latin1;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                            // Save cover art to FLAC file.
                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                            tfile.Save();
                        }

                        // Track Title tag
                        if (trackTitleCheckbox.Checked == true)
                        {
                            tfile.Tag.Title = trackName;
                        }

                        // Album Title tag
                        if (albumCheckbox.Checked == true)
                        {
                            tfile.Tag.Album = albumName;
                        }

                        // Album Artits tag
                        if (albumArtistCheckbox.Checked == true)
                        {
                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                        }

                        // Track Artist tag
                        if (artistCheckbox.Checked == true)
                        {
                            custom.SetField("ARTIST", new string[] { performerName });
                        }

                        // Composer tag
                        if (composerCheckbox.Checked == true)
                        {
                            custom.SetField("COMPOSER", new string[] { composerName });
                        }

                        // Release Date tag
                        if (releaseCheckbox.Checked == true)
                        {
                            custom.SetField("YEAR", new string[] { releaseDate });
                        }

                        // Genre tag
                        if (genreCheckbox.Checked == true)
                        {
                            custom.SetField("GENRE", new string[] { genre });
                        }

                        // Track Number tag
                        if (trackNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                        }

                        // Disc Number tag
                        if (discNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                        }

                        // Total Discs tag
                        if (discTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                        }

                        // Total Tracks tag
                        if (trackTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                        }

                        // Comment tag
                        if (commentCheckbox.Checked == true)
                        {
                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                        }

                        // Copyright tag
                        if (copyrightCheckbox.Checked == true)
                        {
                            custom.SetField("COPYRIGHT", new string[] { copyright });
                        }
                        // UPC tag
                        if (upcCheckbox.Checked == true)
                        {
                            custom.SetField("UPC", new string[] { upc });
                        }

                        // ISRC tag
                        if (isrcCheckbox.Checked == true)
                        {
                            custom.SetField("ISRC", new string[] { isrc });
                        }

                        // Explicit tag
                        if (explicitCheckbox.Checked == true)
                        {
                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                        }

                        // Save all selected tags to file
                        tfile.Save();
                    }
                    else
                    {
                        // If there is a version name.
                        var tfile = TagLib.File.Create(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + discFolder + trackNumber.PadLeft(paddingLength, '0') + " " + trackNamePath.Trim() + " (" + versionNamePath + ")" + audioFileType);
                        // For custom / troublesome tags.
                        var custom = (TagLib.Ogg.XiphComment)tfile.GetTag(TagLib.TagTypes.Xiph);


                        // Saving cover art to file(s)
                        if (imageCheckbox.Checked == true)
                        {
                            // Define cover art to use for FLAC file(s)
                            TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame();
                            pic.TextEncoding = TagLib.StringType.Latin1;
                            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                            pic.Type = TagLib.PictureType.FrontCover;
                            pic.Data = TagLib.ByteVector.FromPath(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");

                            // Save cover art to FLAC file.
                            tfile.Tag.Pictures = new TagLib.IPicture[1] { pic };
                            tfile.Save();
                        }

                        // Track Title tag
                        if (trackTitleCheckbox.Checked == true)
                        {
                            tfile.Tag.Title = trackName + " (" + versionName + ")";
                        }

                        // Album Title tag
                        if (albumCheckbox.Checked == true)
                        {
                            tfile.Tag.Album = albumName;
                        }

                        // Album Artits tag
                        if (albumArtistCheckbox.Checked == true)
                        {
                            custom.SetField("ALBUMARTIST", new string[] { albumArtist });
                        }

                        // Track Artist tag
                        if (artistCheckbox.Checked == true)
                        {
                            custom.SetField("ARTIST", new string[] { performerName });
                        }

                        // Composer tag
                        if (composerCheckbox.Checked == true)
                        {
                            custom.SetField("COMPOSER", new string[] { composerName });
                        }

                        // Release Date tag
                        if (releaseCheckbox.Checked == true)
                        {
                            custom.SetField("YEAR", new string[] { releaseDate });
                        }

                        // Genre tag
                        if (genreCheckbox.Checked == true)
                        {
                            custom.SetField("GENRE", new string[] { genre });
                        }

                        // Track Number tag
                        if (trackNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Track = UInt32.Parse(trackNumber);
                        }

                        // Disc Number tag
                        if (discNumberCheckbox.Checked == true)
                        {
                            tfile.Tag.Disc = UInt32.Parse(discNumber);
                        }

                        // Total Discs tag
                        if (discTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.DiscCount = UInt32.Parse(discTotal);
                        }

                        // Total Tracks tag
                        if (trackTotalCheckbox.Checked == true)
                        {
                            tfile.Tag.TrackCount = UInt32.Parse(trackTotal);
                        }

                        // Comment tag
                        if (commentCheckbox.Checked == true)
                        {
                            custom.SetField("COMMENT", new string[] { commentTextbox.Text });
                        }

                        // Copyright tag
                        if (copyrightCheckbox.Checked == true)
                        {
                            custom.SetField("COPYRIGHT", new string[] { copyright });
                        }
                        // UPC tag
                        if (upcCheckbox.Checked == true)
                        {
                            custom.SetField("UPC", new string[] { upc });
                        }

                        // ISRC tag
                        if (isrcCheckbox.Checked == true)
                        {
                            custom.SetField("ISRC", new string[] { isrc });
                        }

                        // Explicit tag
                        if (explicitCheckbox.Checked == true)
                        {
                            if (advisory == "false") { custom.SetField("ITUNESADVISORY", new string[] { "0" }); } else { custom.SetField("ITUNESADVISORY", new string[] { "1" }); }
                        }

                        // Save all selected tags to file
                        tfile.Save();
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception downloadError)
            {
                // If there is an issue trying to, or during the download, show error info.
                string error = downloadError.ToString();
                output.Invoke(new Action(() => output.AppendText("\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText("Track Download ERROR. Information below.\r\n\r\n")));
                output.Invoke(new Action(() => output.AppendText(error)));
               
            }

            // Delete image file used for tagging
            if (System.IO.File.Exists(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg"))
            {
                System.IO.File.Delete(loc + "\\" + albumArtistPath + "\\" + albumNamePath + "\\" + qualityPath + "\\" + artSize + ".jpg");
            }

            // Say that downloading is completed.
            output.Invoke(new Action(() => output.AppendText("Track Download Done!\r\n\r\n")));
            output.Invoke(new Action(() => output.AppendText("File will be located in your selected path.")));
            mp3Checkbox.Invoke(new Action(() => mp3Checkbox.Visible = true));
            flacLowCheckbox.Invoke(new Action(() => flacLowCheckbox.Visible = true));
            flacMidCheckbox.Invoke(new Action(() => flacMidCheckbox.Visible = true));
            flacHighCheckbox.Invoke(new Action(() => flacHighCheckbox.Visible = true));
            downloadButton.Invoke(new Action(() => downloadButton.Enabled = true));
            #endregion
        }
        #endregion

        #region Tagging Options
        private void tagsLabel_Click(object sender, EventArgs e)
        {
            if (this.Height == 533)
            {
                //New Height
                this.Height = 733;
                tagsLabel.Text = "🠉 Choose which tags to save (click me) 🠉";
            }
            else if (this.Height == 733)
            {
                //New Height
                this.Height = 533;
                tagsLabel.Text = "🠋 Choose which tags to save (click me) 🠋";
            }

        }

        private void albumCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.albumTag = albumCheckbox.Checked;
            Settings.Default.Save();
        }

        private void albumArtistCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.albumArtistTag = albumArtistCheckbox.Checked;
            Settings.Default.Save();
        }

        private void trackTitleCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.trackTitleTag = trackTitleCheckbox.Checked;
            Settings.Default.Save();
        }

        private void artistCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.artistTag = artistCheckbox.Checked;
            Settings.Default.Save();
        }

        private void trackNumberCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.trackTag = trackTitleCheckbox.Checked;
            Settings.Default.Save();
        }

        private void trackTotalCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.totalTracksTag = trackTotalCheckbox.Checked;
            Settings.Default.Save();
        }

        private void discNumberCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.discTag = discNumberCheckbox.Checked;
            Settings.Default.Save();
        }

        private void discTotalCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.totalDiscsTag = discTotalCheckbox.Checked;
            Settings.Default.Save();
        }

        private void releaseCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.yearTag = releaseCheckbox.Checked;
            Settings.Default.Save();
        }

        private void genreCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.genreTag = genreCheckbox.Checked;
            Settings.Default.Save();
        }

        private void composerCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.composerTag = composerCheckbox.Checked;
            Settings.Default.Save();
        }

        private void copyrightCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.copyrightTag = copyrightCheckbox.Checked;
            Settings.Default.Save();
        }

        private void isrcCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.isrcTag = isrcCheckbox.Checked;
            Settings.Default.Save();
        }

        private void upcCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.upcTag = upcCheckbox.Checked;
            Settings.Default.Save();
        }

        private void explicitCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.explicitTag = explicitCheckbox.Checked;
            Settings.Default.Save();
        }

        private void commentCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.commentTag = commentCheckbox.Checked;
            Settings.Default.Save();
        }

        private void imageCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.imageTag = imageCheckbox.Checked;
            Settings.Default.Save();
        }

        private void commentTextbox_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.commentText = commentTextbox.Text;
            Settings.Default.Save();
        }
        private void PopRockCB_CheckedChanged(object sender, EventArgs e)
        {
            poprockid = 112;
        }
        #endregion

        #region Quality Options
        private void flacHighCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.quality4 = flacHighCheckbox.Checked;
            Settings.Default.Save();

            if (flacHighCheckbox.Checked == true)
            {
                formatIdString = "27";
                audioFileType = ".flac";
                Settings.Default.qualityFormat = formatIdString;
                Settings.Default.audioType = audioFileType;
                downloadButton.Enabled = true;
                flacMidCheckbox.Checked = false;
                flacLowCheckbox.Checked = false;
                mp3Checkbox.Checked = false;
            }
            else
            {
                if (flacMidCheckbox.Checked == false & flacLowCheckbox.Checked == false & mp3Checkbox.Checked == false)
                {
                    downloadButton.Enabled = false;
                }
            }
        }

        private void flacMidCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.quality3 = flacMidCheckbox.Checked;
            Settings.Default.Save();

            if (flacMidCheckbox.Checked == true)
            {
                formatIdString = "7";
                audioFileType = ".flac";
                Settings.Default.qualityFormat = formatIdString;
                Settings.Default.audioType = audioFileType;
                downloadButton.Enabled = true;
                flacHighCheckbox.Checked = false;
                flacLowCheckbox.Checked = false;
                mp3Checkbox.Checked = false;
            }
            else
            {
                if (flacHighCheckbox.Checked == false & flacLowCheckbox.Checked == false & mp3Checkbox.Checked == false)
                {
                    downloadButton.Enabled = false;
                }
            }
        }

        private void flacLowCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.quality2 = flacLowCheckbox.Checked;
            Settings.Default.Save();

            if (flacLowCheckbox.Checked == true)
            {
                formatIdString = "6";
                audioFileType = ".flac";
                Settings.Default.qualityFormat = formatIdString;
                Settings.Default.audioType = audioFileType;
                downloadButton.Enabled = true;
                flacHighCheckbox.Checked = false;
                flacMidCheckbox.Checked = false;
                mp3Checkbox.Checked = false;
            }
            else
            {
                if (flacHighCheckbox.Checked == false & flacMidCheckbox.Checked == false & mp3Checkbox.Checked == false)
                {
                    downloadButton.Enabled = false;
                }
            }
        }

        private void mp3Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.quality1 = mp3Checkbox.Checked;
            Settings.Default.Save();

            if (mp3Checkbox.Checked == true)
            {
                formatIdString = "5";
                audioFileType = ".mp3";
                Settings.Default.qualityFormat = formatIdString;
                Settings.Default.audioType = audioFileType;
                downloadButton.Enabled = true;
                flacHighCheckbox.Checked = false;
                flacMidCheckbox.Checked = false;
                flacLowCheckbox.Checked = false;
            }
            else
            {
                if (flacHighCheckbox.Checked == false & flacMidCheckbox.Checked == false & flacLowCheckbox.Checked == false)
                {
                    downloadButton.Enabled = false;
                }
            }
        }
        #endregion

        #region Form moving, closing, minimizing, etc.
        private void exitLabel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void minimizeLabel_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void minimizeLabel_MouseHover(object sender, EventArgs e)
        {
            minimizeLabel.ForeColor = Color.FromArgb(0, 112, 239);
        }

        private void minimizeLabel_MouseLeave(object sender, EventArgs e)
        {
            minimizeLabel.ForeColor = Color.White;
        }

        private void exitLabel_MouseHover(object sender, EventArgs e)
        {
            exitLabel.ForeColor = Color.FromArgb(0, 112, 239);
        }

        private void exitLabel_MouseLeave(object sender, EventArgs e)
        {
            exitLabel.ForeColor = Color.White;
        }

        private void QobuzDownloaderX_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void QobuzDownloaderX_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        #endregion

        private void logoBox_Click(object sender, EventArgs e)
        {
            devClickEggThingValue = devClickEggThingValue + 1;

            if (devClickEggThingValue >= 3)
            {
                streamableCheckbox.Visible = true;
                displaySecretButton.Visible = true;
                secretTextbox.Visible = true;
                hiddenTextPanel.Visible = true;
            }
            else
            {
                streamableCheckbox.Visible = false;
                displaySecretButton.Visible = false;
                secretTextbox.Visible = false;
                hiddenTextPanel.Visible = false;
            }
        }

        private void displaySecretButton_Click(object sender, EventArgs e)
        {
            secretTextbox.Text = appSecret;
        }

        private void logoutLabel_MouseHover(object sender, EventArgs e)
        {
            logoutLabel.ForeColor = Color.FromArgb(0, 112, 239);
        }

        private void logoutLabel_MouseLeave(object sender, EventArgs e)
        {
            logoutLabel.ForeColor = Color.FromArgb(88, 92, 102);
        }

        private void logoutLabel_Click(object sender, EventArgs e)
        {
            // Could use some work, but this works.
            Process.Start("QobuzDownloaderX.exe");
            Application.Exit();
        }

        private void artSizeSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set artSize to selected value, and save selected option to settings.
            artSize = artSizeSelect.Text;
            Settings.Default.savedArtSize = artSizeSelect.SelectedIndex;
            Settings.Default.Save();
        }
    }
}
