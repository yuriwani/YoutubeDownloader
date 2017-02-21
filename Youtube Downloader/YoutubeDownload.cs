using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;
using WebClient = System.Net.WebClient;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace VideoDownloader
{
    class VideoDownloader_Exception : Exception
    {
        public VideoDownloader_Exception(string message)
            : base(message)
        {
        }
    }

    static partial class YoutubeConst
    {
        public static readonly string[] ARR_STR_ITAG_PRIORITY = new string[] { "5", "6", "18", "22", "34", "35", "37", "38" };
        public const string URL_PATTERN_VIDEO = @"https:\/\/www\.youtube\.com\/watch\?v=(?<vid>[a-zA-Z0-9_-]+)";
        public const string URL_PATTERN_PLAYLIST = ".*?href=\"/watch\\?v=(?<vid>[a-zA-Z0-9_]+?)&amp;index=[0-9]{0,3}&amp;list=";
        public const string VIDEO_INFO_URL =  @"https://www.youtube.com/get_video_info?asv=3&el=detailpage&hl=en_US&video_id=";
        public const string PLAYLIST_INFO_URL = @"https://www.youtube.com/playlist?list=";

       // public const string URL_PATTERN_JSCRIPT = @"https://s.ytimg.com/yts/jsbin/player(?.*)/base.js"; //deprecated
        public const string NAME_PATTERN_DECODER_FINDER = "a\\.set\\(\"signature\",(?<DecorderName>[a-zA-Z0-9$]+)\\(";
        public const string NAME_PATTERN_FUNCTION = @"#FUNCNAME#=function\([A-Za-z0-9]+\){.*?};";
        public const string NAME_PATTERN_OBJECT =  @"var #OBJNAME#={.*?};";
        public const string NAME_PATTERN_OBJ_FINDER = @";(?<ObjectName>[A-Za-z0-9]+)\.";
        
        public const string JSCRIPT_URL = @"https://s.ytimg.com/";
        public const string URL_PATTERN_JSCRIPT = @"\/yts\/jsbin\/player.*?\/base\.js"; // jscript for HTML5
    }

    class YoutubeDownload
    {
        public struct StreamInfo
        {
            public NameValueCollection nvcParsedStreamFormat;
            public string strDescription;
            public string strFiletype;
            public string strTitle;
            public WebClient wcDownload;
        }

        public delegate void callback_ProgressReport(int nPercent, long lRecieved, long lSize);

        private const int MAX_DOWNLOADING = 10;

        private List<WebClient> m_listWebclientToDownload;
        private string m_strJScript;

        public YoutubeDownload()
        {
            m_listWebclientToDownload = new List<WebClient>(MAX_DOWNLOADING);
            m_strJScript = "";
        }

        ~YoutubeDownload()
        {
            if (m_listWebclientToDownload.Count > 0)
            {
                foreach (WebClient wc in m_listWebclientToDownload)
                    wc.Dispose();
                
                m_listWebclientToDownload.Clear();
            }
        }

        // return the filesize of the stream
        public string GetFilesizeFromStreaminfo(StreamInfo streamInfo)
        {
            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(streamInfo.nvcParsedStreamFormat["url"]);
            request.Method = "HEAD";

            using (System.Net.WebResponse response = request.GetResponse())
            {
                return response.Headers.Get("Content-Length");
            }
        }

        // return list of string of VideoIDs
        public List<string> GetVideoIDsFromURL(string strURL)
        {
            Debug.WriteLine("Entering GetVideoIDsFromURL");

            List<string> listVideoIDs = new List<string>();

            // assume that the URL is pointing to a single Video
            Regex regex1 = new Regex(YoutubeConst.URL_PATTERN_VIDEO, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection mcollect1 = regex1.Matches(strURL);

            foreach (Match match in mcollect1)
                listVideoIDs.Add(match.Groups["vid"].Value);

            string strVideoPageAsString;
            using (WebClient webclient = new WebClient())
            {
                strVideoPageAsString = webclient.DownloadString(strURL);
            }

            // if there was no match as a single Video, try interpret as a Playlist
            if ( mcollect1.Count == 0)
            {
                Regex regex2 = new Regex(YoutubeConst.URL_PATTERN_PLAYLIST, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection mcollect2 = regex2.Matches(strVideoPageAsString);
                foreach (Match match in mcollect2)
                {
                    if(listVideoIDs.Count == 0)
                        listVideoIDs.Add(match.Groups["vid"].Value);                    
                    else if( !listVideoIDs[listVideoIDs.Count-1].Equals(match.Groups["vid"].Value))
                        listVideoIDs.Add( match.Groups["vid"].Value);                    
                }
            }

            // if JScript to decode signature is not retrieved, do it now
            if (m_strJScript == "")
            {
                Debug.WriteLine("Searching for JScript URL");

                Regex reJS = new Regex(YoutubeConst.URL_PATTERN_JSCRIPT, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                MatchCollection mcollectJS = reJS.Matches(strVideoPageAsString);
                if (mcollectJS.Count == 0)
                    throw new VideoDownloader_Exception("No JavaScript URL found.");

                using (WebClient webclient = new WebClient())
                {
                    Debug.WriteLine("Retrieving JScript from " + YoutubeConst.JSCRIPT_URL + mcollectJS[0].Value);
                    m_strJScript = webclient.DownloadString(YoutubeConst.JSCRIPT_URL + mcollectJS[0].Value);
                }
            }
            return listVideoIDs;
        }

        // return list of StreamInfo of a VideoID
        public List<StreamInfo> GetStreamInfosFromVideoID(string strVideoID)
        {
            Debug.WriteLine("Entering GetStreamInfosFromVideoID");

            List<StreamInfo> listStreamInfo = new List<StreamInfo>();

            string strVideoInfoAsString = null;
            using (WebClient webclient = new WebClient())
            {
                strVideoInfoAsString = webclient.DownloadString(YoutubeConst.VIDEO_INFO_URL + strVideoID);
            }

            NameValueCollection nvcollectVideo = HttpUtility.ParseQueryString(strVideoInfoAsString);

            // get infos and store them for each stream type available to the video 
            string strTitleTemp = nvcollectVideo["title"];

            string strStreamFormatsInLine = nvcollectVideo["url_encoded_fmt_stream_map"];
            string[] arrstrStreamFormats = strStreamFormatsInLine.Split(new char[] { ',' });
            var varParsedStreamFormats = arrstrStreamFormats.Select(s => HttpUtility.ParseQueryString(s));

            string strDescTemp = "", strFiletypeTemp = "";
            foreach (NameValueCollection nvc in varParsedStreamFormats)
            {
                GetVideoDescription(nvc["itag"], ref strDescTemp, ref strFiletypeTemp);
                listStreamInfo.Add(new StreamInfo { nvcParsedStreamFormat = nvc, strDescription = strDescTemp, strFiletype = strFiletypeTemp, strTitle = strTitleTemp });
            }

            return listStreamInfo;
        }

       
        // return true when success
        public bool DownloadYoutubeVideo( StreamInfo streamInfo, string strFilename, callback_ProgressReport callbackFunc)
        {
            Debug.WriteLine("Entering DownloadYoutubeVideo");

            if (m_listWebclientToDownload.Count == m_listWebclientToDownload.Capacity)
            {
                MessageBox.Show("Too many files to download. Please try it again later.");
                return false;
            }

            if (streamInfo.nvcParsedStreamFormat == null)
                throw new VideoDownloader_Exception("nvcStream to download is null."); 

            WebClient wcTemp = new WebClient();

            try
            {
                Uri URL;
                // retrieve signature if encoded
                if (streamInfo.nvcParsedStreamFormat["sig"] == null)
                {
                    if (streamInfo.nvcParsedStreamFormat["s"] == null)
                        URL = new Uri(streamInfo.nvcParsedStreamFormat["url"]);
                    else
                    {
                        string signature = DecodeSignature(m_strJScript, streamInfo.nvcParsedStreamFormat["s"]);
                        if (signature == null)
                            throw new VideoDownloader_Exception("Failed to decode signature.");
                        else
                            URL = new Uri(streamInfo.nvcParsedStreamFormat["url"] + "&signature=" + signature);
                    }
                }
                else
                    URL = new Uri(streamInfo.nvcParsedStreamFormat["url"] + "&signature=" + streamInfo.nvcParsedStreamFormat["sig"]);


                Debug.WriteLine("s={0}",streamInfo.nvcParsedStreamFormat["s"]);
                Debug.WriteLine("sig={0}",streamInfo.nvcParsedStreamFormat["sig"]);
                Debug.WriteLine("download url:{0}", URL);
                //MessageBox.Show("Start Download");

                m_listWebclientToDownload.Add(wcTemp);
                streamInfo.wcDownload = wcTemp;
                
                // Register EventHandler
                m_listWebclientToDownload.Last().DownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(callback_DownloadProgressChanged);
                m_listWebclientToDownload.Last().DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(callback_DownloadCompleted);
                
                // Start downloading async
                m_listWebclientToDownload.Last().DownloadFileAsync(URL, strFilename, callbackFunc);
                
                return true;
            }
            catch(VideoDownloader_Exception e1)
            {
                Console.WriteLine("Unable to download the video.");
                if(m_listWebclientToDownload.Count > 0 && m_listWebclientToDownload[m_listWebclientToDownload.LastIndexOf(wcTemp)].IsBusy)
                    m_listWebclientToDownload.Last().CancelAsync();
                System.Threading.Thread.Sleep(1000);
                return false;
            }
            catch(IOException e2)
            {
                Console.WriteLine("Unexpected error in DownloadYoutubeVideo");
                if (m_listWebclientToDownload.Count > 0 && m_listWebclientToDownload[m_listWebclientToDownload.LastIndexOf(wcTemp)].IsBusy)
                    m_listWebclientToDownload.Last().CancelAsync();
                return false;
            }
        }

        
#region "private functions"
        
        private void callback_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            //if (e.ProgressPercentage % 5 == 0)
            {
                callback_ProgressReport func = (callback_ProgressReport)e.UserState;
                func(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
            }
        }

        private void callback_DownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            callback_ProgressReport func = (callback_ProgressReport)e.UserState;
            func(100, 0, 0);
            
            m_listWebclientToDownload.Remove((WebClient)sender);

            if (e.Cancelled)
                MessageBox.Show("Download was cancelled.");
            else if (e.Error != null)
                MessageBox.Show(e.Error.Message, "Error");
            else
                MessageBox.Show("Download completed.");
        }


        private void GetVideoDescription(string strItag, ref string strDescription, ref string strFiletype)
        {
            strDescription = null;
            strFiletype = null;

            switch (strItag)
            {
                case "5":
                    strDescription = "Low Quality, 240p, FLV, 400x240";
                    strFiletype = "FLV";
                    break;
                case "6":
                    strDescription = "Low Quality, FLV, 400x240, 450x270";
                    strFiletype = "FLV";
                    break;
                case "17":
                    strDescription = "Low Quality, 144p, 3GP, 0x0";
                    strFiletype = "3GP";
                    break;
                case "18":
                    strDescription = "Medium Quality, 360p, MP4, 480x360";
                    strFiletype = "MP4";
                    break;
                case "22":
                    strDescription = "High Quality, 720p, MP4, 1280x720";
                    strFiletype = "MP4";
                    break;
                case "34":
                    strDescription = "Medium Quality, 360p, FLV, 640x360";
                    strFiletype = "FLV";
                    break;
                case "35":
                    strDescription = "Standard Definition, 480p, FLV, 854x480";
                    strFiletype = "FLV";
                    break;
                case "36":
                    strDescription = "Low Quality, 240p, 3GP, 0x0";
                    strFiletype = "3GP";
                    break;
                case "37":
                    strDescription = "Full High Quality, 1080p, MP4, 1920x1080";
                    strFiletype = "MP4";
                    break;
                case "38":
                    strDescription = "Original Definition, MP4, 4096x3072";
                    strFiletype = "MP4";
                    break;
                case "43":
                    strDescription = "Medium Quality, 360p, WebM, 640x360";
                    strFiletype = "WebM";
                    break;
                case "44":
                    strDescription = "Standard Definition, 480p, WebM, 854x480";
                    strFiletype = "WebM";
                    break;
                case "45":
                    strDescription = "High Quality, 720p, WebM, 1280x720";
                    strFiletype = "WebM";
                    break;
                case "46":
                    strDescription = "Full High Quality, 1080p, WebM, 1280x720";
                    strFiletype = "WebM";
                    break;
                case "82":
                    strDescription = "Medium Quality 3D, 360p, MP4, 640x360";
                    strFiletype = "MP4";
                    break;
                case "83":
                    strDescription = "Standard High Definition 3D, MP4, 854x480";
                    strFiletype = "MP4";
                    break;
                case "84":
                    strDescription = "High Quality 3D, 720p, MP4, 1280x720";
                    strFiletype = "MP4";
                    break;
                case "85":
                    strDescription = "Full High Definition 3D, MP4, 1920x1080";
                    strFiletype = "MP4";
                    break;
                case "100":
                    strDescription = "Medium Quality 3D, 360p, WebM, 640x360";
                    strFiletype = "WebM";
                    break;
                case "101":
                    strDescription = "Standard Definition 3D, WebM, 854x480";
                    strFiletype = "WebM";
                    break;
                case "102":
                    strDescription = "High Quality 3D, 720p, WebM, 1280x720";
                    strFiletype = "WebM";
                    break;
            }
        }

#region "reference"
        /*
          var formats = {
	5:  { itag: 5, quality: 5, description: "Low Quality, 240p", format: "FLV", fres: "240p", 	mres: { width:  400, height:  240 }, acodec: "MP3", vcodec: "SVQ"},
	17: { itag: 17, quality: 4, description: "Low Quality, 144p", format: "3GP", fres: "144p", mres: { width:  0, height: 0  }, acodec: "AAC", vcodec: ""},
	18: { itag: 18, quality: 15, description: "Low Definition, 360p", format: "MP4", fres: "360p", mres: { width:  480, height:  360 }, acodec: "AAC", vcodec: "H.264"},
	22: { itag: 22, quality: 35, description: "High Definition, 720p", format: "MP4", fres: "720p",	mres: { width: 1280, height:  720 }, acodec: "AAC", vcodec: "H.264"},
	34: { itag: 34, quality: 10, description: "Low Definition, 360p", format: "FLV", fres: "360p", 	mres: { width:  640, height:  360 }, acodec: "AAC", vcodec: "H.264"},
	35: { itag: 35, quality: 25, description: "Standard Definition, 480p", format: "FLV" , fres: "480p", mres: { width:  854, height:  480 }, acodec: "AAC", vcodec: "H.264"},
	36: { itag: 36, quality: 6, description: "Low Quality, 240p", format: "3GP", fres: "240p", 	mres: { width:  0, height:  0 }, acodec: "AAC", vcodec: ""},
	37: { itag: 37, quality: 45, description: "Full High Definition, 1080p", format: "MP4", fres: "1080p", mres: {width: 1920, height: 1080}, acodec: "AAC", vcodec: "H.264"},
	38: { itag: 38, quality: 55, description: "Original Definition", format: "MP4" , fres: "Orig",	mres: { width: 4096, height: 3072 }, acodec: "AAC", vcodec: "H.264"},
	43: { itag: 43, quality: 20, description: "Low Definition, 360p", format: "WebM", fres: "360p",	mres: { width:  640, height:  360 }, acodec: "Vorbis", vcodec: "VP8"},
	44: { itag: 44, quality: 30, description: "Standard Definition, 480p", format: "WebM", fres: "480p", mres: { width:  854, height:  480 }, acodec: "Vorbis", vcodec: "VP8"},
	45: { itag: 45, quality: 40, description: "High Definition, 720p", format: "WebM", fres: "720p", mres: { width: 1280, height:  720 }, acodec: "Vorbis", vcodec: "VP8"},
	46: { itag: 46, quality: 50, description: "Full High Definition, 1080p", format: "WebM", fres: "1080p",	mres: {width: 1280, height: 720}, acodec: "Vorbis", vcodec: "VP8"},
	82: { itag: 82, quality: 16, description: "Low Definition 3D, 360p", format: "MP4",  fres: "360p", mres: { width: 640,  height:  360 }, acodec: "AAC", vcodec: "H.264"},
	84: { itag: 84, quality: 41, description: "High Definition 3D, 720p", format: "MP4",  fres: "720p",	mres: { width: 1280, height:  720 }, acodec: "AAC", vcodec: "H.264"},
	100: { itag: 100, quality: 17, description: "Low Definition 3D, 360p", format: "WebM", fres: "360p", mres: { width: 640,  height:  360 }, acodec: "Vorbis", vcodec: "VP8"},
	102: { itag: 102, quality: 42, description: "High Definition 3D, 720p", format: "WebM", fres: "720p", mres: {width: 1280, height: 720}, acodec: "Vorbis", vcodec: "VP8"},
	133: { itag: 133, description: "", format: "MP4", fres: "240p", acodec: "noaudio", vcodec: "H.264"},
	134: { itag: 134,  description: "", format: "MP4", fres: "360p", acodec: "noaudio", vcodec: "H.264"},
	135: { itag: 135, description: "MP4 480p - no audio", format: "MP4", fres: "480p", acodec: "noaudio", vcodec: "H.264"},
	136: { itag: 136,  description: "MP4 720p", format: "MP4", fres: "720p", acodec: "noaudio", vcodec: "H.264"},
	137: { itag: 137, description: "MP4 1080p - no audio", format: "MP4", fres: "1080p", acodec: "noaudio", vcodec: "H.264"},
	138: { itag: 138, description: "MP4 2160p - no audio", format: "MP4", fres: "2160p", acodec: "noaudio", vcodec: "H.264"},
	140: { itag: 140, description: "M4A 128kbps - audio", format: "m4a", fres: "128k", acodec: "AAC", vcodec: "novideo"},
	141: { itag: 141, description: "M4A 256kbps - audio", format: "m4a", fres: "256k", acodec: "AAC", vcodec: "novideo"},
    160: { itag: 160, description: "", format: "MP4",  acodec: "noaudio", fres: "144p", vcodec: "H.264/15fps"},
	264: { itag: 264, description: "MP4 1440p - no audio", format: "MP4", fres: "1440p",  acodec: "noaudio", vcodec: ""},
	298: { itag: 298, description: "MP4 720p 60fps - no audio", format: "MP4", fres: "720p",  acodec: "noaudio", vcodec: "H.264/60fps"},
	299: { itag: 299, description: "MP4 1080p 60fps - no audio", format: "MP4", fres: "1080p",  acodec: "noaudio", vcodec: "H.264/60fps"}
	}
         * */
#endregion

        // return decoded signature. return null when failed
        private string DecodeSignature(string strJScript, string encodedSignature)
        {
            Debug.WriteLine("Entering DecodeSignature");

            if( strJScript == null || encodedSignature == null)
                return null;

            else if( strJScript.Length == 0 || encodedSignature.Length == 0 )
                return null;

            //// exctract DecoderFunction from JScript
            Regex regex1 = new Regex(YoutubeConst.NAME_PATTERN_DECODER_FINDER, RegexOptions.Singleline);
            string strDecoderName = regex1.Matches(strJScript)[0].Groups["DecorderName"].Value;
            string strDecoderFunction = Regex.Match(strJScript, YoutubeConst.NAME_PATTERN_FUNCTION.Replace("#FUNCNAME#", strDecoderName)).Value;

            //// extract helper object definition used in DecoderFunction from JScript
            Regex regex2 = new Regex(YoutubeConst.NAME_PATTERN_OBJ_FINDER, RegexOptions.Singleline);
            MatchCollection objColl = regex2.Matches(strDecoderFunction);
            HashSet<string> setObjNames = new HashSet<string>();
            foreach (Match obj in objColl)
                setObjNames.Add(obj.Groups["ObjectName"].Value);
            string strObjects = "";
            foreach (string objName in setObjNames)
            {
                Regex regex = new Regex(YoutubeConst.NAME_PATTERN_OBJECT.Replace("#OBJNAME#", objName), RegexOptions.Singleline);
                strObjects += regex.Match(strJScript).Value.Replace('\n', ' ');
            }
            //// execute JavaScript to decode signature with Jurassic library
            var engine = new Jurassic.ScriptEngine();

            //engine.Evaluate("Um=function(a){a=a.split(\"\");return a.join(\"\");}"); // for debug
            engine.Evaluate(strObjects + strDecoderFunction);
            string decodedSingnature = (engine.CallGlobalFunction<string>(strDecoderName, encodedSignature));

            return decodedSingnature;
        }
#endregion

    }
}
