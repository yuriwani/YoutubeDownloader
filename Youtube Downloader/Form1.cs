using System;
using System.Collections.Generic;
using System.Windows.Forms;

using VideoDownloader;


namespace Youtube_Downloader
{

    public partial class frmMain : Form
    {
        //public const string PATH_DOWNLOAD_FOLDER = @"C:\Users\yuri\Desktop\test videos\";
        public const string PATH_DOWNLOAD_FOLDER = @".\";
        const string FORBIDDENCHARS = "<>|*:!?\"/";

        YoutubeDownload clsYoutubeDownloader;
        List<YoutubeDownload.StreamInfo> listStreamInfo;
        List<string> listStrVideoID;

        public frmMain()
        {
            InitializeComponent();
            
            clsYoutubeDownloader = new YoutubeDownload();
            listStreamInfo = new List<YoutubeDownload.StreamInfo>();
            listStrVideoID = new List<string>();

            btnDownload.Enabled = false;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if(listboxFormat.Text == "")
            {
                MessageBox.Show("Please select a format to download.");
                return;
            }

            int intIndex = listboxFormat.SelectedIndex;

            string strFilename = listStreamInfo[intIndex].strTitle + "." + listStreamInfo[intIndex].strFiletype;
            // sanitize filename
            foreach (char c in FORBIDDENCHARS)
                strFilename = strFilename.Replace(c, ' ');

            while (System.IO.File.Exists(PATH_DOWNLOAD_FOLDER + strFilename))
            {
                strFilename = "_" + strFilename;
            }
            clsYoutubeDownloader.DownloadYoutubeVideo(listStreamInfo[intIndex], PATH_DOWNLOAD_FOLDER + strFilename, ShowProgressReport);

            textboxURL.Enabled = true;
            btnFind.Enabled = true;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            textboxURL.Enabled = false;
            btnFind.Enabled = false;
            btnDownload.Enabled = false;

            listStrVideoID.Clear();
            listboxFormat.Items.Clear();
            listboxVideo.Items.Clear();
            labelInfomation.Text = "";

            listStrVideoID = clsYoutubeDownloader.GetVideoIDsFromURL(textboxURL.Text);

            if (listStrVideoID.Count == 1)
            {
                UpdateFormatListbox(listStrVideoID[0]);
            }
            else if (listStrVideoID.Count > 1)
            {
                foreach (string strVideoID in listStrVideoID)
                {
                    listboxVideo.Items.Add(strVideoID);
                }
                labelInfomation.Text = string.Format("{0} videos found in the Playlist", listStrVideoID.Count);
                btnDownload.Enabled = true;
            }
            else
            {
                MessageBox.Show("No Youtube Video found.");
                textboxURL.Enabled = true;
                btnFind.Enabled = true;
                return;
            }                          
        }

        private void listboxFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnDownload.Enabled = true;
        }

        private void listboxVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFormatListbox(listStrVideoID[listboxVideo.SelectedIndex]);
        }

        private void btnClipboard_Click(object sender, EventArgs e)
        {
#if false // for debug
            textboxURL.Text = "https://www.youtube.com/watch?v=XZq2l0iLV7Y";

            var engine = new Jurassic.ScriptEngine();
            var tmpstr = "5F5FAC49DB3FE629E3126EC2FA25151A0182035FD36DB37B7F1DDE520DCD7FC088B9604551F6CD70FF.F.";
            engine.Evaluate("var Tm={u1:function(a,b){var c=a[0];a[0]=a[b%a.length];a[b]=c}, wk:function(a,b){a.splice(0,b)}, o3:function(a){a.reverse()}};Um=function(a){a=a.split(\"\");Tm.wk(a,2);Tm.u1(a,48);Tm.o3(a,74);Tm.wk(a,2);Tm.u1(a,40);Tm.o3(a,54);Tm.u1(a,5);Tm.o3(a,45);return a.join(\"\")};");
            //engine.Evaluate(strObjects + strDecoderFunction);
            string decodedSingnature = (engine.CallGlobalFunction<string>("Um", tmpstr));
            //textboxURL.Text = decodedSingnature;
#else
            IDataObject data = Clipboard.GetDataObject();

            if (data.GetDataPresent(DataFormats.Text))
            {
                textboxURL.Text = (string)data.GetData(DataFormats.Text);
            }
#endif
        }

#region userfunctions
        private void UpdateFormatListbox(string strVideoID)
        {          
            listStreamInfo.Clear();
            listStreamInfo = clsYoutubeDownloader.GetStreamInfosFromVideoID(strVideoID);

            foreach (YoutubeDownload.StreamInfo stInfo in listStreamInfo)
            {
                listboxFormat.Items.Add(stInfo.strDescription);
            }
        }

        public void ShowProgressReport(int nPercent, long lRecieved, long lSize)
        {
            if (nPercent == 100)
                this.Text = "Youtube Downloader";
            else
                this.Text = String.Format("Downloading... {0}% complete. ({1}/{2} Bytes)", nPercent, lRecieved, lSize);           
        }
        

#endregion

    }
}
