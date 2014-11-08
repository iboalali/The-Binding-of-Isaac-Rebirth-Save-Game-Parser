﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BindingOfIsaacRebirthSaveGameParser {
    public partial class Form1 : Form {

        private byte[] buffer;
        private BinaryReader br;

        /// <summary>
        /// The "My Documnet" folder path
        /// </summary>
        private string MyDocument_Path = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

        /// <summary>
        /// The path inside the "My Document" Folder
        /// </summary>
        private string SaveGameFile_Path = @"My Games\Binding of Isaac Rebirth";

        /// <summary>
        /// Default file name for the first save game file
        /// </summary>
        private string File_Name = "persistentgamedata1.dat";

        /// <summary>
        /// Known location of the statistics
        /// Key: Name, Value: Location (in decimel)
        /// </summary>
        private Dictionary<string, int> info_Location
            = new Dictionary<string, int>() 
                        { 
                            { "Deaths", 259 } 
                            // ...
                        };

        /// <summary>
        /// Known location of the statistics
        /// Key: Location (in decimel), Value: Name
        /// </summary>
        private Dictionary<int, string> info_Location_R
            = new Dictionary<int, string>() 
                        { 
                            { 259, "Deaths" } 
                            // ...
                        };

        public Form1 () {
            InitializeComponent();

        }

        private void Form1_Load ( object sender, EventArgs e ) {

            string completePath = Path.Combine( MyDocument_Path, SaveGameFile_Path ) + "\\" + File_Name;

            LoadBinaryFile( completePath, ref this.buffer );
            ParseFile();

        }

        private void btnExit_Click ( object sender, EventArgs e ) {
            Environment.Exit( Environment.ExitCode );
        }

        private void btnShowFile_Click ( object sender, EventArgs e ) {
            new SaveFileHex( this.buffer ).Show();
        }

        private void btnShowChanges_Click ( object sender, EventArgs e ) {
            new ShowChangesInSaveGame( info_Location_R ).Show();
        }

        private void btnOpenSaveGame_Click ( object sender, EventArgs e ) {
            OpenFileDialog ofd = new OpenFileDialog();

            if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {

                LoadBinaryFile( ofd.FileName, ref this.buffer );
                ParseFile();
            }
        }

        /// <summary>
        /// Loads the file as binary into a byte array
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="buffer">The byte array to fill</param>
        private void LoadBinaryFile ( string path, ref byte[] buffer ) {
            using ( br = new BinaryReader( File.Open( path, FileMode.Open ) ) ) {
                buffer = new byte[br.BaseStream.Length];
                int r = br.Read( buffer, 0, buffer.Length );

            }

        }

        /// <summary>
        /// Extract using the known location of the information
        /// </summary>
        private void ParseFile () {
            if ( this.buffer == null ) {
                return;
            }

            lblDeaths.Text = this.buffer[info_Location["Deaths"]].ToString();

        }


    }
}