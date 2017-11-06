using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Grab
{
    public partial class TrialConfig : Form
    {
        public Parameters[] stimParams = null;
        public TrialConfig()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Loads in the specified Configuration file and fills in the textbox values.
            openFileDialog1.ShowDialog();
            System.IO.FileStream fs =
                    (System.IO.FileStream)openFileDialog1.OpenFile();
            BinaryReader loadcfg = new BinaryReader(fs);
            this.comboBox1.Text = loadcfg.ReadInt32().ToString();
            textBox68.Text = loadcfg.ReadSingle().ToString();
            textBox65.Text = loadcfg.ReadInt32().ToString();
            textBox66.Text = loadcfg.ReadInt32().ToString();
            textBox67.Text = loadcfg.ReadInt32().ToString();
            checkBox1.Checked = loadcfg.ReadBoolean();
            IEnumerable<Control> groups = GetAll(this, typeof(GroupBox));
            
            
            int groupnum = 0;
            
            while (loadcfg.PeekChar() != -1)
            {
                GroupBox nextgroup = groups.OfType<GroupBox>().ElementAt<GroupBox>(groupnum);
                IEnumerable<Control> combos = GetAll(nextgroup, typeof(ComboBox));
                ComboBox nextcombo = combos.OfType<ComboBox>().ElementAt<ComboBox>(0);
                nextcombo.Text = loadcfg.ReadBoolean() ? "Radial" : "Approach";
                IEnumerable<Control> txt = GetAll(nextgroup, typeof(TextBox));
                foreach (TextBox nexttxt in txt)
                {
                    if (nexttxt.Visible)
                        nexttxt.Text = loadcfg.ReadSingle().ToString();
                }
                groupnum++;
            }
            loadcfg.Close();
            fs.Close();
        }

        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool filledIn = true;
            IEnumerable<Control> allctrls = GetAll(this,typeof(TextBox));
            foreach (TextBox ctrl in allctrls)
            {
                if (ctrl.Visible && ctrl.Text == "")
                    filledIn = false;
            }
            allctrls = GetAll(this, typeof(ComboBox));
            foreach (ComboBox ctrl in allctrls)
            {
                if (ctrl.Visible && ctrl.Text == "")
                    filledIn = false;
            }
            if (filledIn)
            {
                saveFileDialog1.Filter = "Behavioral Configuration|*.bcfg";
                saveFileDialog1.Title = "Save the Behavioral Configuration File";
                saveFileDialog1.ShowDialog();
                try
                {
                    System.IO.FileStream fs =
                    (System.IO.FileStream)saveFileDialog1.OpenFile();
                    BinaryWriter config = new BinaryWriter(fs);
                    config.Write(Int32.Parse(comboBox1.Text));
                    config.Write(float.Parse(textBox68.Text));
                    config.Write(Int32.Parse(textBox65.Text));
                    config.Write(Int32.Parse(textBox66.Text));
                    config.Write(Int32.Parse(textBox67.Text));
                    config.Write(checkBox1.Checked);
                    foreach (GroupBox grp in this.Controls.OfType<GroupBox>())
                    {
                        if (grp.Visible)
                        {
                            ComboBox combo = grp.Controls.OfType<ComboBox>().ElementAt<ComboBox>(0);
                            config.Write((combo.Text == "Radial") ? true : false);
                            foreach (Panel pnl in grp.Controls.OfType<Panel>())
                            {
                                if (pnl.Visible)
                                {
                                    foreach (TextBox txt in pnl.Controls.OfType<TextBox>())
                                    {
                                        if (txt.Visible)
                                            config.Write(float.Parse(txt.Text));
                                    }
                                }
                            }
                            foreach (TextBox txt in grp.Controls.OfType<TextBox>())
                            {
 
                                        if (txt.Visible)
                                            config.Write(float.Parse(txt.Text));

                            }
                        }
                    }
                    config.Close();
                    fs.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.ToString());
                }

            }
            else
                MessageBox.Show("Enter a value in all available fields before saving");


        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            int numStims = Int32.Parse(comboBox1.Text);
           
            foreach (GroupBox lbl in this.Controls.OfType<GroupBox>())
            {
                string nm = lbl.Name;
                if (Int32.Parse(nm.Substring(nm.Length-1)) <= numStims)
                    lbl.Visible = true;
                else
                    lbl.Visible = false;
            }

        }


        private void comboBox2_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox2.Text == "Radial")
            {
                panel1.Visible = true;
                panel2.Visible = false;
                panel1.BringToFront();
                panel2.SendToBack();
            }
            else
            {
                panel1.Visible = false;
                panel2.Visible = true;
                panel2.BringToFront();
                panel1.SendToBack();
            }
            
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.Text == "Approach")
            {
                panel3.Visible = true;
                panel4.Visible = false;
                panel3.BringToFront();
                panel4.SendToBack();
            }
            else
            {
                panel3.Visible = false;
                panel4.Visible = true;
                panel4.BringToFront();
                panel3.SendToBack();
            }
            
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.Text == "Approach")
            {
                panel5.Visible = true;
                panel6.Visible = false;
                panel5.BringToFront();
                panel6.SendToBack();
            }
            else
            {
                panel5.Visible = false;
                panel6.Visible = true;
                panel6.BringToFront();
                panel5.SendToBack();
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.Text == "Approach")
            {
                panel7.Visible = true;
                panel8.Visible = false;
                panel7.BringToFront();
                panel8.SendToBack();
            }
            else
            {
                panel7.Visible = false;
                panel8.Visible = true;
                panel8.BringToFront();
                panel7.SendToBack();
            }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox6.Text == "Approach")
            {
                panel9.Visible = true;
                panel10.Visible = false;
                panel9.BringToFront();
                panel10.SendToBack();
            }
            else
            {
                panel9.Visible = false;
                panel10.Visible = true;
                panel10.BringToFront();
                panel9.SendToBack();
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox7.Text == "Approach")
            {
                panel11.Visible = true;
                panel12.Visible = false;
                panel11.BringToFront();
                panel12.SendToBack();
            }
            else
            {
                panel11.Visible = false;
                panel12.Visible = true;
                panel12.BringToFront();
                panel11.SendToBack();
            }
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox8.Text == "Approach")
            {
                panel13.Visible = true;
                panel14.Visible = false;
                panel13.BringToFront();
                panel14.SendToBack();
            }
            else
            {
                panel13.Visible = false;
                panel14.Visible = true;
                panel14.BringToFront();
                panel13.SendToBack();
            }
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox9.Text == "Approach")
            {
                panel15.Visible = true;
                panel16.Visible = false;
                panel15.BringToFront();
                panel16.SendToBack();
            }
            else
            {
                panel15.Visible = false;
                panel16.Visible = true;
                panel16.BringToFront();
                panel15.SendToBack();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Loops through all comboBoxes and TextBoxes and assigns values to Paramaters class
            stimParams = new Parameters[Int32.Parse(comboBox1.Text)];
            IEnumerable<Control> stimGroups = GetAll(this,typeof(GroupBox));
            float convFactor = float.Parse(textBox68.Text);
            int triL = Int32.Parse(textBox65.Text);
            int stimL = Int32.Parse(textBox66.Text);
            int triI = Int32.Parse(textBox67.Text);
            bool velo = checkBox1.Checked;
            for (int i = 0; i < stimParams.Length; i++)
            {
                GroupBox nextgroup = stimGroups.OfType<GroupBox>().ElementAt<GroupBox>(i);
                TrialType nextType = (nextgroup.Controls.OfType<ComboBox>().ElementAt<ComboBox>(0).Text == "Radial") ? TrialType.Radial : TrialType.Approach;
                if (veloWalkBox.Checked)
                    nextType = TrialType.VeloWalk;
                IEnumerable<Control> tBoxes = GetAll(nextgroup, typeof(TextBox));
                float[] vars = new float[6];
                int count = 0;
                foreach (TextBox nextbox in tBoxes)
                {
                    if (nextbox.Visible)
                    {
                        vars[count] = float.Parse(nextbox.Text);
                        count++;
                    }

                   
                }
                stimParams[i] = new Parameters(nextType, vars[0], vars[1], vars[2], vars[3], vars[4], vars[5], triL, stimL, triI, velo, convFactor);

            }
            //MessageBox.Show("Trial Type: " + stimParams[0].trialType.ToString() + " start Size: " + stimParams[0].begRadSize.ToString() +
            //    " End Size: " + stimParams[0].endRadSize.ToString() + " Speed: " + stimParams[0].radSpeed.ToString() + " Contrast: " +
            //    stimParams[0].contrast.ToString() + " Distoff: " + stimParams[0].distoff.ToString() + " Angle: " + stimParams[0].angle.ToString());
         
            this.DialogResult = DialogResult.OK;
            this.Close();
            
        }
    }
}
