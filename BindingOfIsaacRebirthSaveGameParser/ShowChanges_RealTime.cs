﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BindingOfIsaacRebirthSaveGameParser {

    [Serializable]
    public struct SaveGameSnapShot {
        public int Index { get; set; }
        public byte[] SnapShot { get; set; }
        public DateTime Date { get; set; }
        public static int Counter { get; set; }

    }

    public partial class ShowChanges_RealTime : Form {
        private FileSystemWatcher watcher;
        private List<SaveGameSnapShot> SaveGame_TimeLine;
        private string FileName { get; set; }
        private string Path { get; set; }
        private BinaryReader br;
        private bool recordingStarted;
        private byte[] firstSnapShot;
        private bool didSomethingChange;

        public ShowChanges_RealTime ( string path, string fileName ) {
            InitializeComponent();

            this.Icon = Form1.appIcon;

            this.Path = path;
            this.FileName = fileName;
            this.didSomethingChange = false;

            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = fileName;
            watcher.Path = path;
            watcher.Changed += watcher_Changed;
            watcher.Error += watcher_Error;


        }

        void watcher_Error ( object sender, ErrorEventArgs e ) {
            MessageBox.Show( "Couldn't track the changes", "Error" );

        }

        void watcher_Changed ( object sender, FileSystemEventArgs e ) {
            byte[] snapshot;

            while ( true ) {
                try {
                    LoadFile( Path + '\\' + FileName, out snapshot );
                    SaveGameSnapShot sgss = new SaveGameSnapShot();
                    sgss.Date = DateTime.Now;
                    sgss.Index = SaveGameSnapShot.Counter++;
                    sgss.SnapShot = snapshot;
                    SaveGame_TimeLine.Add( sgss );

                    SetControlPropertyThreadSafe( lblCounter, "Text", SaveGameSnapShot.Counter.ToString() );

                    ShowChanges();
                    break;
                    // Delete from the begining if it gets too large
                    //if ( SaveGame_OverTime.Count >= 50000 ) {
                    //    
                    //}

                } catch ( IOException ) {
                    continue;

                }
            }
        }

        private void btnStart_Stop_Click ( object sender, EventArgs e ) {
            if ( btnStart.Text == "Start Tracking" ) {
                btnStart.Text = "Stop Tracking";

                SaveGame_TimeLine = new List<SaveGameSnapShot>();
                SaveGameSnapShot.Counter = 0;

                LoadFile( Path + '\\' + FileName, out firstSnapShot );
                
                SaveGameSnapShot sgss = new SaveGameSnapShot();
                sgss.Date = DateTime.Now;
                sgss.Index = SaveGameSnapShot.Counter++;
                sgss.SnapShot = firstSnapShot;
                SaveGame_TimeLine.Add( sgss );

                lblCounter.Text = SaveGameSnapShot.Counter.ToString();
                lblSnapShot1.Text = DateTime.Now.ToString();

                listView.Items.Clear();


                watcher.EnableRaisingEvents = true;
                
                recordingStarted = true;

            } else {
                btnStart.Text = "Start Tracking";
                watcher.EnableRaisingEvents = false;

                if ( recordingStarted ) {
                    recordingStarted = false;

                    if ( SaveGame_TimeLine.Count < 2 ) {
                        MessageBox.Show( "You need more than two snapshots.", "Too Few Snapshots" );
                        return;

                    }
                    new SnapShotTimeLine_Form( SaveGame_TimeLine ).Show();

                }
            }

        }

        private void LoadFile ( string path, out byte[] buffer ) {
            using ( br = new BinaryReader( File.Open( path, FileMode.Open ) ) ) {
                buffer = new byte[br.BaseStream.Length];
                int r = br.Read( buffer, 0, buffer.Length );

            }

        }

        private void ShowChanges () {
            if ( firstSnapShot == null || SaveGame_TimeLine.Count < 1 ) {
                return;

            }

            List<string[]> lists = new List<string[]>();
            //if ( SaveGame_OverTime.Count == 1 ) {
            //    for ( int i = 0; i < firstSnapShot.Length; i++ ) {
            //        SetControlPropertyThreadSafe( lblSnapShot1, "Text", SaveGame_OverTime.Last().Date.ToString() );
            //        ClearItemsInListViewThreadSafe( listView );

            //        if ( firstSnapShot[i] != SaveGame_OverTime.Last().SnapShot[i] ) {
            //            if ( StatLocation.ContainsLocation( i ) ) {
            //                lists.Add( new string[]{
            //                        i.ToString( "X4" ),
            //                        "---",
            //                        SaveGame_OverTime.Last().SnapShot[i].ToString(),
            //                        StatLocation.GetLocation_Name( i )
            //                    }
            //                );
            //                i += StatLocation.GetNumberOfByteMinusOne( i );

            //            } else {

            //                lists.Add( new string[]{
            //                        i.ToString( "X4" ),
            //                        "---",
            //                        SaveGame_OverTime.Last().SnapShot[i].ToString()
            //                    }
            //                );
            //            } // end if ( StatLocation.ContainsLocation( i ) ) else
            //            didSomethingChange = true;
            //        } // end if ( firstSnapShot[i] != SaveGame_OverTime.Last().SnapShot[i] )
            //    } // end for ( int i = 0; i < firstSnapShot.Length; i++ )
            //} else {
            for ( int i = 0; i < firstSnapShot.Length; i++ ) {
                SetControlPropertyThreadSafe( lblSnapShot1, "Text", SaveGame_TimeLine.Last().Date.ToString() );

                if ( SaveGame_TimeLine[SaveGame_TimeLine.Count - 2].SnapShot[i] != SaveGame_TimeLine.Last().SnapShot[i] ) {
                    if ( StatLocation.ContainsLocation( i ) ) {
                        lists.Add( new string[]{
                                    i.ToString( "X4" ) + " - " + ( i + StatLocation.GetNumberOfByteMinusOne( i ) ).ToString( "X4" ),
                                    StatLocation.GetValue(SaveGame_TimeLine[SaveGame_TimeLine.Count - 2].SnapShot, i).ToString(),
                                    StatLocation.GetValue(SaveGame_TimeLine.Last().SnapShot, i).ToString(),
                                    StatLocation.GetLocation_Name( i )
                                }
                        );
                        i += StatLocation.GetNumberOfByteMinusOne( i );

                    } else {

                        lists.Add( new string[]{
                                    i.ToString( "X4" ),
                                    SaveGame_TimeLine[SaveGame_TimeLine.Count - 2].SnapShot[i].ToString(),
                                    SaveGame_TimeLine.Last().SnapShot[i].ToString()
                                }
                        );
                    } // end if ( StatLocation.ContainsLocation( i ) ) else
                    didSomethingChange = true;
                } // end if ( SaveGame_OverTime[SaveGame_OverTime.Count - 2].SnapShot[i] != SaveGame_OverTime.Last().SnapShot[i] )
            } // end for ( int i = 0; i < firstSnapShot.Length; i++ )
            //} // end if ( SaveGame_OverTime.Count == 1 ) else


            if ( !didSomethingChange ) {
                SaveGame_TimeLine.RemoveAt( SaveGame_TimeLine.Count - 1 );
                SaveGameSnapShot.Counter--;
                SetControlPropertyThreadSafe( lblCounter, "Text", SaveGameSnapShot.Counter.ToString() );

            } else {
                ClearItemsInListViewThreadSafe( listView );
                SetItemsInListViewThreadSafe( listView, lists );

            }
            didSomethingChange = false;

        }

        private void btnLocationToTrack_Click ( object sender, EventArgs e ) {
            new SnapShotTimeLine_Form().Show();

        }

        private delegate void SetControlPropertyThreadSafeDelegate ( Control control, string propertyName, object propertyValue );

        public static void SetControlPropertyThreadSafe ( Control control, string propertyName, object propertyValue ) {
            if ( control.InvokeRequired ) {
                control.Invoke( new SetControlPropertyThreadSafeDelegate( SetControlPropertyThreadSafe ), new object[] { control, propertyName, propertyValue } );
            } else {
                control.GetType().InvokeMember( propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue } );
            }
        }

        private delegate void SetItemsInListViewThreadSafeDelegate ( ListView control, List<string[]> lists );

        public static void SetItemsInListViewThreadSafe ( ListView control, List<string[]> lists ) {
            if ( control.InvokeRequired ) {
                control.Invoke( new SetItemsInListViewThreadSafeDelegate( SetItemsInListViewThreadSafe ), new object[] { control, lists } );
            } else {
                foreach ( var item in lists ) {
                    control.Items.Add( new ListViewItem( item ) );
                    //ListViewItem lvi = new ListViewItem();
                    //foreach ( var s in item ) {
                    //    lvi.SubItems.Add( s );

                    //}
                    //control.Items.Add( lvi );

                }

            }
        }

        private delegate void ClearItemsInListViewThreadSafeDelegate ( ListView control );

        public static void ClearItemsInListViewThreadSafe ( ListView control ) {
            if ( control.InvokeRequired ) {
                control.Invoke( new ClearItemsInListViewThreadSafeDelegate( ClearItemsInListViewThreadSafe ), new object[] { control } );
            } else {
                control.Items.Clear();

            }
        }

        private void ShowChanges_RealTime_FormClosing ( object sender, FormClosingEventArgs e ) {
            watcher.EnableRaisingEvents = false;

        }

    }
}
