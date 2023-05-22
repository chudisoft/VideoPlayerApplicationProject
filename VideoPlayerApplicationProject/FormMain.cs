using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Collections;



namespace WindowsFormsApplicationGUIuARM
{
    public partial class FormMain : Form
    {
        string Folder = Environment.CurrentDirectory + "\\Videos\\";

        // Playlist
        public const string Separator = ",";
        public static ArrayList searchVideo = new ArrayList();
        public static StreamWriter sw;

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            video_read();
            CreatePlayListVideo();

            axWindowsMediaPlayer_ff.settings.setMode("loop", true);
            axWindowsMediaPlayer_ff.Ctlcontrols.play();

        }

        private string Parse(string line, ref int index)
        {
            if (index == -1)
            {
                return "";
            }

            //Bypass the very first separator and start the string at the start of the data
            //if (index == 0)
            //{
            //    index = line.IndexOf(Separator, 0);
            //    index += Separator.Length;
            //}

            //Get the next separator position
            int tempNo = line.IndexOf(Separator, index);

            string sTemp = string.Empty;
            //Get the data in between the separators
            if (tempNo == -1)
            {
                sTemp = line.Substring(index);
                index = tempNo;
            }
            else
            {
                sTemp = line.Substring(index, tempNo - index);
                //Set the index to the start of the next set of data
                index = tempNo + Separator.Length;
            }

            // Remove Double quotes in a string.
            sTemp = sTemp.Trim('"', ' ');

            return sTemp.Trim();
        }

        private void video_read()
        {
            string sFormatIni = "formats.ini";
            string sTemp;
            int index;

            FileStream fs = new FileStream(Folder + sFormatIni, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            using (fs)
            {
                string fileLine = string.Empty;
                try
                {
                    while ((fileLine = sr.ReadLine()) != null)
                    {
                        if (fileLine == string.Empty)
                            continue;

                        if (fileLine[0] == '#')     // Process only uncommented lines
                            continue;

                        index = 0;
                        sTemp = Parse(fileLine, ref index);
                        switch (sTemp.ToUpper())
                        {
                            /*case "AF":
                                sTemp = Parse(fileLine, ref index);
                                while (string.IsNullOrEmpty(sTemp) == false)
                                {
                                    searchAudio.Add(sTemp);

                                    sTemp = Parse(fileLine, ref index);
                                }
                                break;*/
                            case "VF":
                                sTemp = Parse(fileLine, ref index);
                                while (string.IsNullOrEmpty(sTemp) == false)
                                {
                                    searchVideo.Add(sTemp);

                                    sTemp = Parse(fileLine, ref index);
                                }
                                break;
                            default:
                                break;

                        }   // End switch
                    }       // End while
                }           // End Try
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                }
                finally
                {
                    // Close File handles
                    sr.Close();
                    fs.Close();
                }
            }           // End Using
        }

        
        private void CreatePlayListVideo()
        {
            // Open a file to write
            string sFileName = "My_Videos.wpl";
            FileStream fs = File.Create(Folder + sFileName);
            sw = new StreamWriter(fs);

            try
            {
                sw.WriteLine("<?wpl version=\"1.0\"?>");    // File Header
                sw.WriteLine("<smil>");                     // Start of File Tag

                sw.WriteLine("\t<head>");                     // Playlist File Header Information Start Tag
                sw.WriteLine("\t\t<meta name=\"Generator\" content=\"Microsoft Windows Media Player -- 10.0.0.4036\"/>");
                sw.WriteLine("\t\t<author> Ebube </author>");
                sw.WriteLine("\t\t<title> Playlist Video </title>");
                sw.WriteLine("\t</head>");                    // Playlist File Header Information End Tag

                sw.WriteLine("\t<body>");                     // Start of body Tag
                sw.WriteLine("\t\t<seq>");                      // Start of filelist Tag


                // Get Directory's File list and Add files
                DirectoryListing(Folder);

                sw.WriteLine("\t\t</seq>");                      // End of filelist Tag
                sw.WriteLine("\t</body>");                    // End of body Tag
                sw.WriteLine("</smil>");                    // End of File Tag

                sFileName = sFileName + " Successfully created.";

                //MessageBox.Show(sFileName, "Create Playlis");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create Playlist: Error");

                sFileName = sFileName + " Unsuccessful.";

                MessageBox.Show(sFileName, "Create Playlis");
            }
            finally
            {
                sw.Close();
                fs.Close();
            }

            axWindowsMediaPlayer_ff.URL = Folder + "\\My_Videos.wpl";
            axWindowsMediaPlayer_ff.Ctlcontrols.play();
        }
        private int DirectoryListing(string sPath)
        {
            int iFileCount = 0;

            if (string.IsNullOrEmpty(sPath) == true)
            {
                MessageBox.Show("Directory not specified. Please select Valid directory.");
                return iFileCount;
            }

            //            if (Directory.Exists(sPath) == false)
            //            {
            //                MessageBox.Show("Directory not exist. Please select Valid directory.");
            //                return iFileCount;
            //            }

            ArrayList searchList = new ArrayList();

            /*switch (cmbType.SelectedIndex)
            {
                case 0:                                         // Audio files
                    searchList = searchAudio;
                    break;
                case 1:                                         // Video files
                */
            searchList = searchVideo;
            /*   break;
           case 2:                                         // All Audio Video Files
               searchList = searchAudio;
               searchList.AddRange(searchVideo);
               break;
           case 3:                                         // All files
               //searchList = null;
               break;
           default:
               //searchList = null;
               break;
       }*/

            if (File.Exists(sPath))
            {
                // This path is a file
                iFileCount = ProcessFile(sPath, searchList);
            }
            else if (Directory.Exists(sPath))
            {
                // This path is a directory
                iFileCount = ProcessDirectory(sPath, searchList);
            }
            else
            {
                MessageBox.Show(sPath + " is not a valid file or directory.");
            }

            return iFileCount;

        }

        // Insert logic for processing found files here.
        public static int ProcessFile(string fileName, ArrayList searchList)
        {
            string fileLine;
            string sFileExt;
            int iFileCount = 0;

            if (string.IsNullOrEmpty(fileName) == true)
                return iFileCount;

            if (searchList.Count != 0)                     // If it's not All files
            {
                sFileExt = fileName.Substring(fileName.IndexOf('.'));
                if (searchList.IndexOf(sFileExt) == -1)
                    return iFileCount;
            }

            fileLine = "\t\t\t<media src=\"";
            fileLine = fileLine + fileName + "\"/>";
            sw.WriteLine(fileLine);

            return (++iFileCount);
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static int ProcessDirectory(string targetDirectory, ArrayList searchList)
        {
            int iFileCount = 0;

            if (string.IsNullOrEmpty(targetDirectory) == true)
                return iFileCount;

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            if (fileEntries.Length > 0)
            {
                foreach (string fileInfo in fileEntries)
                    iFileCount += ProcessFile(fileInfo, searchList);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            if (subdirectoryEntries.Length > 0)
            {
                foreach (string subdirectory in subdirectoryEntries)
                    ProcessDirectory(subdirectory, searchList);
            }
            return iFileCount;
        }
        string[] arr = new string[4];
        ListViewItem itm;
       
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
        }

        private DialogResult PreClosingConfirmation()
        {
            DialogResult res = System.Windows.Forms.MessageBox.Show("Do you want to close?", "", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return res;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            axWindowsMediaPlayer_ff.Ctlcontrols.stop();
        }
    }
}
