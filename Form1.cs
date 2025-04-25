using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace TP_LINK_ART_TOOL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var exeFile = typeof(Form1).Assembly.Location;
            if (File.Exists(exeFile))
            {
                Icon = Icon.ExtractAssociatedIcon(exeFile);
            }
        }
        const string filter = "(*.bin)|*.bin";
        private void button1_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog()
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = filter
            })
            {
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    var file = openFile.FileName;
                    if (File.Exists(file))
                    {
                        textBox1.Text = file;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var file = textBox1.Text;
                if (File.Exists(file))
                {
                    using (var fs = File.OpenRead(file))
                    {
                        long offsetValue = long.Parse(offset.Text, System.Globalization.NumberStyles.HexNumber);
                        long pos = long.Parse(start.Text, System.Globalization.NumberStyles.HexNumber);
                        long endValue = long.Parse(end.Text, System.Globalization.NumberStyles.HexNumber) + 1;
                        var data = new byte[endValue - pos];
                        fs.Position = pos;
                        fs.Read(data, 0, data.Length);
                        using (var saveFile = new SaveFileDialog() { FileName = "art.bin", Filter = filter })
                        {
                            if (saveFile.ShowDialog() == DialogResult.OK)
                            {
                                var sFile = saveFile.FileName;
                                if (!string.IsNullOrWhiteSpace(sFile))
                                {
                                    using (var art = File.Create(sFile))
                                    {
                                        for (int i = 0; i < 65536; i++)
                                        {
                                            art.WriteByte(0xff);
                                        }
                                        art.Position = offsetValue;
                                        art.Write(data, 0, data.Length);
                                        art.Position = 0;
                                        using (var sha256 = SHA256.Create())
                                        {
                                            byte[] sha256sum = sha256.ComputeHash(art);
                                            MessageBox.Show("ART 提取完成!\r\n SHA256: " + BitConverter.ToString(sha256sum).Replace("-", ""), null, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("https://github.com/lalakii");
            }
            catch { }
        }
        class ArtConfig
        {
            public long Index;
            public string Model;
            public string Start;
            public string End;
            public string Offset;
        }
        readonly List<ArtConfig> artConfigs = new List<ArtConfig>();
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                using (var cfs = File.OpenRead("art_config.ini"))
                {
                    using (var reader = new StreamReader(cfs, System.Text.Encoding.UTF8, true))
                    {
                        long i = 0;
                        while (reader.Peek() != -1)
                        {
                            var configItem = reader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(configItem))
                            {
                                var item = configItem.Trim();
                                if (!item.StartsWith("#"))
                                {
                                    var itemArr = item.Split('\\');
                                    var itemArt = new ArtConfig
                                    {
                                        Model = itemArr[0],
                                        Start = itemArr[1],
                                        End = itemArr[2],
                                        Offset = itemArr[3],
                                        Index = i,
                                    };
                                    i++;
                                    model.Items.Add(itemArt.Model);
                                    artConfigs.Add(itemArt);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
            if (artConfigs.Count > 0)
            {
                model.SelectedIndex = 0;
                var firstConfig = artConfigs.FirstOrDefault();
                start.Text = firstConfig.Start;
                end.Text = firstConfig.End;
                offset.Text = firstConfig.Offset;
            }
        }

        private void model_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = model.SelectedIndex;
            var item = artConfigs[index];
            start.Text = item.Start;
            end.Text = item.End;
            offset.Text = item.Offset;
        }
    }
}
