using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace Graphical_DomTurn
{
    public partial class frmMain : Form
    {
        private DomTurnSaver saver = new DomTurnSaver();
        private String dataPath;
        private bool populated;

        TreeNode currentGame = null;

        public frmMain()
        {
            InitializeComponent();

            refreshGameList();
        }

        private void btnGameName_Click(object sender, EventArgs e)
        {
            String name = textBox1.Text.Trim();
            bool succeeded = saver.SetGame(name);

            if (succeeded)
            {
                populate(name);
                populated = true;

                treeView1.ExpandAll();

                timer1.Enabled = true;
            }
            else
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add("oops, game does not exist");
            }
        }

        private void btnArm_Click(object sender, EventArgs e)
        {
            btnReset.Enabled = !btnReset.Enabled;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.Print("clear clicked");

            if (populated) saver.Clear(dataPath);

            treeView1.Nodes.Clear();
            currentGame = null;
            timer1.Enabled = false;

            btnReset.Enabled = false;
        }

        private void populate(String name)
        {
            dataPath = Path.Combine(Directory.GetCurrentDirectory(), "dom4turnsaver", name);

            Game root = saver.getGame(Path.Combine(dataPath, "t"));

            TreeNode node = treeView1.Nodes.Add(root.name);
            node.Tag = root;

            recursivelyAddChildren(root, node);

            treeView1.Focus();
            treeView1.SelectedNode = currentGame;

            populated = true;
        }

        private void recursivelyAddChildren(Game game, TreeNode node)
        {
            String[] children = saver.getChildrenPaths(game.path);

            if (currentGame == null)
            {
                if (saver.equalsCurrentGame(game))
                {
                    currentGame = node;
                }
            }

            foreach (String path in children)
            {
                Game newGame = saver.getGame(path);

                TreeNode newNode = node.Nodes.Add(newGame.name);
                newNode.Tag = newGame;

                recursivelyAddChildren(newGame, newNode);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool changed = saver.checkUpdate();

            if (changed)
            {
                Game newGame = saver.saveGame(((Game)currentGame.Tag).path);

                TreeNode newNode = currentGame.Nodes.Add(newGame.name);
                newNode.Tag = newGame;

                currentGame = newNode;
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            saver.SetTo((Game) treeView1.SelectedNode.Tag);
            currentGame = treeView1.SelectedNode;

            timer1.Enabled = true;
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            String input = "turn";
            ShowInputDialog(ref input);

            treeView1.SelectedNode.Text = input;

            Game game = (Game)treeView1.SelectedNode.Tag;
            game.name = input;

            saver.changeName(game);
        }

        private static DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshGameList();
        }

        private void refreshGameList()
        {
            String[] names = saver.getGames();

            listBox1.Items.Clear();

            for (int n = 0; n < names.Length; n++)
            {
                listBox1.Items.Add(names[n]);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.Text;
        }
    }
}

