using NamedPipeWrapper.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ExampleGUI
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonClient_Click(object sender, EventArgs e)
        {
            Hide();
            new FormClient().ShowDialog(this);
            Close();
        }

        private void buttonServer_Click(object sender, EventArgs e)
        {
            Hide();
            new FormServer().ShowDialog(this);
            Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {

            _streamWrapper.SetEncryptionKey(Convert.FromBase64String("AAECAwQFBgcICQoLDA0ODw=="));
            Console.WriteLine("Set encryption key.");
        }
    }
}
