using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Editor
{
    public partial class Form1 : Form
    {

        bool tb41_success, tb42_success;
        int tb42x, tb42y;
        SaveFileDialog sfd;

        List<ComboBox> lstCb = new List<ComboBox>();

        public Form1()
        {
            InitializeComponent();
            tb41_success = false;
            tb42_success = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            ToolTip tt33 = new ToolTip();
            tt33.SetToolTip(this.label33, "High Ground Bonus Ratio");

            ToolTip tt34 = new ToolTip();
            tt34.SetToolTip(this.label34, "Low Ground Malus Ratio");

            ToolTip tt35 = new ToolTip();
            tt35.SetToolTip(this.label35, "Kanly Success Probability");

            ToolTip tt37 = new ToolTip();
            tt37.SetToolTip(this.label37, "Sandworm Speed");

            ToolTip tt38 = new ToolTip();
            tt38.SetToolTip(this.label38, "Sandworm Spawn Distance");

            ToolTip tt39 = new ToolTip();
            tt39.SetToolTip(this.label39, "Clone Probability");
            
            ToolTip tt40 = new ToolTip();
            tt40.SetToolTip(this.label40, "Minimum Pause Time");

            ToolTip tt41 = new ToolTip();
            tt41.SetToolTip(this.label41, "Cellular Automation");

            ToolTip ttLoadExamples = new ToolTip();
            ttLoadExamples.SetToolTip(this.btnLoadDefaults, "Loads the in the JSON schema defined values under the \"examples\" section.");

        }

#pragma warning disable CS8602 // Dereferenzierung eines möglichen Nullverweises.
        private void tbFilterInt(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))           
                e.Handled = true;          
        }

        private void tbFilterFloat(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
                e.KeyChar = ',';

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != ','))            
                e.Handled = true;
            

            // only allow one decimal point
            if ((e.KeyChar == ',') && ((sender as TextBox).Text.IndexOf(',') > -1))          
                e.Handled = true;
            

            if (e.KeyChar == '.' && (sender as TextBox).TextLength == 0)
                (sender as TextBox).AppendText("0");
        }

        private void textBox41_Leave(object sender, EventArgs e) //Cellular Automation
        {
            tb41_success = true;
            (sender as TextBox).ForeColor = Color.Black;

            string pattern = @"S[0-9][0-9]*/B[0-9][0-9]*";
            Regex r = new Regex(pattern);
            Match m = r.Match((sender as TextBox).Text);
            if (!m.Success)
            {
                tb41_success = false;
                (sender as TextBox).ForeColor = Color.Red;
            }
        }

        private void textBox42_Leave(object sender, EventArgs e)
        {
            if ((sender as TextBox).TextLength == 0)
            {
                tb42_success = false;
                return;
            }

            tb42_success = this.textBox42x.TextLength > 0 && this.textBox42y.TextLength > 0;

            (sender as TextBox).ForeColor = Color.Black;

            if ((Int32.Parse((sender as TextBox).Text) < 4) || (Int32.Parse((sender as TextBox).Text) > 16)){
                tb42_success = false;
                (sender as TextBox).ForeColor= Color.Red;
            }
        }

        private void btnGenScen_Click(object sender, EventArgs e)
        {
            const int padding = 6;
            const int comboBoxHeight = 23;
            int fieldY = btnGenScen.Location.Y + btnGenScen.Height + padding;

            if (!tb42_success)
            {
                MessageBox.Show("Error whilst generating field options.\nPlease make sure that you entered valid dimensions!\n" +
                    "(Minimum: 4; Maximum: 16)", "Error generating field options",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tb42x = Int32.Parse(this.textBox42x.Text);
            tb42y = Int32.Parse(this.textBox42y.Text);

            for (int i = gbScenOptions.Controls.Count - 1; i >= 0; i--)            
                if(gbScenOptions.Controls[i] is ComboBox)               
                    gbScenOptions.Controls.RemoveAt(i);
                               
            int comboBoxWidth = (gbScenOptions.Width - ((tb42x + 1) * padding)) / tb42x;

            gbScenOptions.Height = tb42y * comboBoxHeight + (tb42y + 1) * padding + fieldY;
            gbExportOptions.Location = new Point(gbExportOptions.Location.X, gbScenOptions.Location.Y + gbScenOptions.Height + padding);
            this.Size = new Size(this.Width, gbExportOptions.Location.Y + 2*gbExportOptions.Height);

            lstCb.Clear();

            for (int i = 0; i < tb42x; i++)
            {
                for(int j = 0; j < tb42y; j++)
                {
                    lstCb.Add(new ComboBox());
                    lstCb[j + i * tb42y].Items.AddRange(new object[] { "CITY", "MOUNTAINS", "PLATEAU", "DUNE", "FLAT_SAND", "HELIPORT" });
                    lstCb[j + i * tb42y].SelectedIndex = 4;
                    lstCb[j + i * tb42y].Width = comboBoxWidth;
                    lstCb[j + i * tb42y].Location = new Point(i * comboBoxWidth + (i + 1) * padding, fieldY + j*comboBoxHeight + (j+1)*padding);
                    lstCb[j + i * tb42y].Visible = true;
                    gbScenOptions.Controls.Add(lstCb[j + i * tb42y]);
                }
            }
        }

        private bool checkGameOptions()
        {
            if (!tb41_success || !tb42_success)
                return false;
            
            foreach (Control c in this.Controls)
                if (c is GroupBox)
                    foreach (Control tb in c.Controls)
                        if (tb is TextBox && tb.Text == "")
                            return false;

            return true;
        }

        private bool checkScenOptions()
        {
            int numberOfCities = 0;
            foreach (Control c in gbScenOptions.Controls)           
                if(c is ComboBox && (c as ComboBox).SelectedIndex == 0)             
                    numberOfCities++;
            return numberOfCities == 2;
        }

        private void btnExportGameOptions_Click(object sender, EventArgs e)
        {
            if (!checkGameOptions()) {
                MessageBox.Show("Please assign a valid value to each field!", "Export Error",
    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var gameOptions = new Dictionary<string, object>
            {
                ["noble"] = new Dictionary<string, object>
                {
                    ["maxHP"] = float.Parse(textBox7.Text),
                    ["maxMP"] = int.Parse(textBox10.Text),
                    ["maxAP"] = int.Parse(textBox11.Text),
                    ["damage"] = float.Parse(textBox8.Text),
                    ["inventorySize"] = int.Parse(textBox12.Text),
                    ["healingHP"] = int.Parse(textBox9.Text)
                },
                ["mentat"] = new Dictionary<string, object>
                {
                    ["maxHP"] = float.Parse(textBox18.Text),
                    ["maxMP"] = int.Parse(textBox15.Text),
                    ["maxAP"] = int.Parse(textBox14.Text),
                    ["damage"] = float.Parse(textBox17.Text),
                    ["inventorySize"] = int.Parse(textBox13.Text),
                    ["healingHP"] = int.Parse(textBox16.Text)
                },
                ["beneGesserit"] = new Dictionary<string, object>
                {
                    ["maxHP"] = float.Parse(textBox24.Text),
                    ["maxMP"] = int.Parse(textBox21.Text),
                    ["maxAP"] = int.Parse(textBox20.Text),
                    ["damage"] = float.Parse(textBox23.Text),
                    ["inventorySize"] = int.Parse(textBox19.Text),
                    ["healingHP"] = int.Parse(textBox22.Text)
                },
                ["fighter"] = new Dictionary<string, object>
                {
                    ["maxHP"] = float.Parse(textBox30.Text),
                    ["maxMP"] = int.Parse(textBox27.Text),
                    ["maxAP"] = int.Parse(textBox26.Text),
                    ["damage"] = float.Parse(textBox29.Text),
                    ["inventorySize"] = int.Parse(textBox25.Text),
                    ["healingHP"] = int.Parse(textBox28.Text)
                },
                ["numbOfRounds"] = int.Parse(textBox31.Text),
                ["actionTime"] = float.Parse(textBox32.Text),
                ["highGroundBonusRatio"] = float.Parse(textBox33.Text),
                ["lowGroundMalusRatio"] = float.Parse(textBox34.Text),
                ["kanlySuccessProbability"] = float.Parse(textBox35.Text),
                ["spiceMinimum"] = int.Parse(textBox36.Text),
                ["cellularAutomation"] = textBox41.Text,
                ["sandWormSpeed"] = int.Parse(textBox37.Text),
                ["sandWormSpawnDistance"] = int.Parse(textBox38.Text),
                ["cloneProbability"] = float.Parse(textBox39.Text),
                ["minPauseTime"] = int.Parse(textBox40.Text)
            };

            sfd = new SaveFileDialog();
            sfd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;

            //string fileName = "D:\\Daten\\Studium\\Semester 4\\Sopra\\Editor2\\Editor\\test.party.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(gameOptions, options);
            
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                string filename = sfd.FileName.Replace(".party.json", "");
                filename = filename.Replace(".json", "");
                File.WriteAllText((filename + ".party.json"), jsonString);
            }

        }

        private void btnExportScenOptions_Click(object sender, EventArgs e)
        {
            if (!checkScenOptions())
            {
                MessageBox.Show("Please assign a valid value to each field!\nPlease make sure that there are exactly 2 \"CITIES\" present.", "Export Error",
MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
                
            /*string[,] comboBoxes = new string[tb42x, tb42y];

            for(int i = 0; i < tb42x; i++)            
                for (int j = 0; j < tb42y; j++)                
                    comboBoxes[i, j] = lstCb[j + i * tb42y].Text;*/
                
            string[][] comboBoxes = new string[tb42x][];
            string[] cbTemp = new string[tb42y];

            for (int i = 0; i < tb42x; i++)
            {
                for (int j = 0; j < tb42y; j++)
                    cbTemp[j] = lstCb[j + i * tb42y].Text;
                comboBoxes[i] = (string[])cbTemp.Clone();
            }

            var scenOptions = new Dictionary<string, object>
            {
                ["scenario"] = comboBoxes
            };

            sfd = new SaveFileDialog();
            sfd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;

            //string fileName = "D:\\Daten\\Studium\\Semester 4\\Sopra\\Editor2\\Editor\\test.scenario.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(scenOptions, options);

            if(sfd.ShowDialog() == DialogResult.OK)
            {
                string filename = sfd.FileName.Replace(".scenario.json", "");
                filename = filename.Replace(".json", "");
                File.WriteAllText((filename + ".scenario.json"), jsonString);
            }
        }

        private void btnLoadDefaults_Click(object sender, EventArgs e)
        {
            //Noble
            textBox7.Text = "20";
            textBox8.Text = "4";
            textBox9.Text = "6";
            textBox10.Text = "21";
            textBox11.Text = "22";
            textBox12.Text = "8";

            //Mentat
            textBox18.Text = "20";
            textBox17.Text = "4";
            textBox16.Text = "6";
            textBox15.Text = "21";
            textBox14.Text = "22";
            textBox13.Text = "8";

            //Bene Gesserit
            textBox24.Text = "20";
            textBox23.Text = "4";
            textBox22.Text = "6";
            textBox21.Text = "21";
            textBox20.Text = "22";
            textBox19.Text = "8";

            //Fighter
            textBox30.Text = "20";
            textBox29.Text = "4";
            textBox28.Text = "6";
            textBox27.Text = "21";
            textBox26.Text = "22";
            textBox25.Text = "8";

            //Game options
            textBox31.Text = "10";
            textBox32.Text = "0,5";
            textBox33.Text = "0,5";
            textBox34.Text = "0,5";
            textBox35.Text = "0,5";
            textBox36.Text = "10";
            textBox37.Text = "10";
            textBox38.Text = "10";
            textBox39.Text = "0,5";
            textBox40.Text = "10";
            textBox41.Text = "S2/B3";
            textBox42x.Text = "5";
            textBox42y.Text = "5";

            tb41_success = true;
            tb42_success = true;
            tb42x = 5;
            tb42y = 5;


        }
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.Controls)
                if (c is GroupBox)
                    foreach (Control tb in c.Controls)
                        if(tb is TextBox)
                            tb.Text = "";
            
            tb41_success = false;
            tb42_success = false;
            tb42x = 0;
            tb42y = 0;

        }

    }
}
