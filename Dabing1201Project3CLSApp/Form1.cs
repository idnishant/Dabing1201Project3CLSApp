using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Dabing1201Project3CLSApp
{
    public partial class Form1 : Form
    {
        private Icon m_info= new Icon(SystemIcons.Information, 40, 40);
        private Icon m_error=new Icon(SystemIcons.Error, 40, 40);

         private Icon m_ready=new Icon(SystemIcons.Application,40,40);

        public Form1()
        {
            InitializeComponent();
            eventLog1.Log = "Application";
eventLog1.Source = "WinService";


        }

        private void tabSource_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSource.Text="D:\\Creative\\Source\\";
            txtProcessedFile.Text = "D:\\Creative\\Processed\\";
            txtDest.Text = "D:\\Creative\\Destination\\";
            optGenerateLog.Checked = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtSource.Text))
            {
                errMessage.SetError(txtSource, "Invalid Source Directory");
                txtSource.Focus();
                tabControl1.SelectedTab = tabSource;
                return;
            }
            else
                errMessage.SetError(txtSource, "");
            if (!Directory.Exists(txtDest.Text))
            {
                errMessage.SetError(txtDest, "Invalid Destination Directory");
                txtDest.Focus();
                tabControl1.SelectedTab = tabDest;
                return;
            }
            else
                errMessage.SetError(txtDest, "");
            if (!Directory.Exists(txtProcessedFile.Text))
            {
                errMessage.SetError(txtProcessedFile, "Invalid Source Directory");
                txtProcessedFile.Focus();
                tabControl1.SelectedTab = tabSource;
                return;
            }
            else
                errMessage.SetError(txtProcessedFile, "");
            //watch dir
            watchDir.Path = txtSource.Text; 
            watchDir.EnableRaisingEvents=true;
            
            //notify 
           icuNotify.Icon = m_ready;
           icuNotify.Visible = true;
           this.ShowInTaskbar = false;
           this.Hide();
        }

        private void txtSource_KeyUp(object sender, KeyEventArgs e)
        {
            if (Directory.Exists(txtSource.Text))
            {
                txtSource.BackColor = Color.White;
            }
            else
                txtSource.BackColor = Color.Pink;
        }

        private void txtDest_KeyUp(object sender, KeyEventArgs e)
        {
            if (Directory.Exists(txtDest.Text))
            {
                txtDest.BackColor = Color.White;
            }
            else
                txtDest.BackColor = Color.Pink;
        }

        private void txtProcessedFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (Directory.Exists(txtProcessedFile.Text))
            {
                txtProcessedFile.BackColor = Color.White;
            }
            else
                txtProcessedFile.BackColor = Color.Pink;
        }

        private void mnuConfigure_Click(object sender, EventArgs e)
        {
            icuNotify.Visible = false;
            this.ShowInTaskbar = true;
            this.Show();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void icuNotify_DoubleClick(object sender, EventArgs e)
        {
            icuNotify.Visible = false;
            this.ShowInTaskbar=true;
            this.Show();
        }

        private void watchDir_Created(object sender, FileSystemEventArgs e)
        {
            watchDir.EnableRaisingEvents = false;
             icuNotify.Icon = m_info;
             icuNotify.Text = "Processed: " + e.Name;
            // to access the Word application
            Microsoft.Office.Interop.Word.Application wdApp = new Microsoft.Office.Interop.Word.Application();
            object optional = System.Reflection.Missing.Value;
            //writrer for  XML
            XmlTextWriter xmlTextWriter = new XmlTextWriter(txtDest.Text + "summary.xml", null);
            try
            {
                Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
                object filename = e.FullPath;
                doc = wdApp.Documents.Open(ref filename, ref optional, ref optional, ref optional, ref optional, ref optional,
                    ref optional, ref optional, ref optional, ref optional, ref optional);
                
                Microsoft.Office.Interop.Word.Range wdRange;
                wdRange = doc.Paragraphs[2].Range;//check
                
                string strMemo, strAmount;
                int intParacount;
                //read memo
                strMemo = wdRange.Text;
                strMemo = strMemo.Substring(13, 4);

                intParacount = doc.Paragraphs.Count;
                intParacount = intParacount - 2;
                //read 
                wdRange = doc.Paragraphs[intParacount].Range;
                Object count = -1;
                Object wdCharecter = "1";

                wdRange.MoveEnd(ref wdCharecter, ref count);
                strAmount = wdRange.Text;

                strAmount = strAmount.Substring(21);
                //write xml
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteDocType("Sales", null, null, null);
                xmlTextWriter.WriteComment("Summary of sales at Creative Learning");
                xmlTextWriter.WriteStartElement("Sales");
                xmlTextWriter.WriteStartElement(Convert.ToString(DateTime.Today));
                xmlTextWriter.WriteElementString("Memo", strMemo);
                xmlTextWriter.WriteElementString("Amount", strAmount);
                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndElement();
                icuNotify.Icon = m_ready;

            }
            catch (Exception ex) {
                icuNotify.Icon = m_error;
                icuNotify.Text = "Error in " + e.Name;
                if (optGenerateLog.Checked == true) {
                   EventLog.WriteEntry("CrealtiveLearning",e.Name + ": " + ex.Message);
                }
            }
            finally
            {
                xmlTextWriter.Flush();
                xmlTextWriter.Close();//close xml

                wdApp.Quit(ref optional, ref optional, ref optional);
                wdApp = null;
                watchDir.EnableRaisingEvents = true;

            }
            tryAgain:
            try
            {
                File.Move(e.FullPath, txtProcessedFile.Text + e.Name);
            }
            catch
            {
                goto tryAgain;
            }
        }  //end                                                                                                                                         

        private void icuNotify_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            lstEvents.Items.Clear();
            eventLog1.Log = "Application";
            eventLog1.MachineName = ".";
            foreach(EventLogEntry logEntry in eventLog1.Entries)
            {
                if (logEntry.Source == "CrealtiveLearning")
                {
                    lstEvents.Items.Add(logEntry.Message);
                }
            }
        }

        private void btnViewSummary_Click(object sender, EventArgs e)
        {
            StreamReader strRead;
            try
            {
                strRead = new StreamReader(txtDest.Text + "Summary.xml");
                MessageBox.Show(strRead.ReadToEnd(), txtDest.Text + "Summary.xml", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                strRead.Close();    
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Error was returned : "+ex.Message+" Please check the destination folder for summary");
            }

        }
    }
}
