using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindDuplicate
{
    public partial class Form1 : Form
    {
        private static int boardSize = 4;
        private static int tileSize = 64;
        private static int totalDups = boardSize * boardSize;

        private static char[] dupsList = new char[totalDups];
        private int[] indexs = new int[totalDups];

        private static Button[,] board = new Button[boardSize, boardSize];
        private static char[,] backBoard = new char[boardSize, boardSize];
        private static int[] isChoosing = new int[2];

        private Timer counter = new Timer();
        private int maxTime = totalDups; // seconds
        private bool start;
        private ProgressBar pb = new ProgressBar();

        public Form1()
        {
            InitializeComponent();
            InitDupsWarehouse();
            PlaceDups();
            InitGUI();

            this.ClientSize = new Size(tileSize * boardSize, tileSize + tileSize * boardSize);
        }

        private void InitDupsWarehouse()
        {
            dupsList[0] = 'A';
            for (int i = 1; i < totalDups / 2; i++)
            {
                dupsList[i] = (Char)(Convert.ToUInt16(dupsList[i - 1]) + 1);
                // MessageBox.Show(dupsList[i].ToString());
            }
            for (int i = totalDups / 2; i < totalDups; i++)
            {
                // MessageBox.Show(i.ToString());
                dupsList[i] = dupsList[i - totalDups / 2];
            }
            for (int i = 0; i < totalDups; i++)
            {
                indexs[i] = 0;
            }
        }

        private void PlaceDups()
        {
            Random rd = new Random();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    board[j, i] = new Button();
                    board[j, i].Location = new Point(tileSize * j, tileSize + tileSize * i);
                    board[j, i].Size = new Size(tileSize, tileSize);

                    board[j, i].BackColor = Color.Gray;
                    board[j, i].Click += ChooseBox;

                    this.Controls.Add(board[j, i]);

                    int dupsIndex = rd.Next(totalDups);
                    while (indexs[dupsIndex] == 1)
                    {
                        dupsIndex = rd.Next(totalDups);
                    }

                    backBoard[j, i] = dupsList[dupsIndex];
                    // board[j, i].Text = backBoard[j, i].ToString();
                    indexs[dupsIndex] = 1;
                }
            }

            isChoosing[0] = -1;
            isChoosing[1] = -1;
        }

        private void ChooseBox(object sender, EventArgs e)
        {
            if (!start)
            {
                counter.Start();
                start = true;
            }
            Button btn = (Button)sender;
            int j = btn.Location.X / tileSize;
            int i = (btn.Location.Y - tileSize) / tileSize;

            board[j, i].Text = backBoard[j, i].ToString();

            if (isChoosing[0] == -1 && isChoosing[1] == -1)
            {
                isChoosing[0] = j;
                isChoosing[1] = i;
                //board[isChoosing[0], isChoosing[1]].Enabled = false;
                board[isChoosing[0], isChoosing[1]].BackColor = Color.White;
            }
            else
            {
                board[j, i].Text = backBoard[j, i].ToString();
                Wait(j, i);
            }
        }

        private async void Wait(int j, int i)
        {
            await Task.Run(() =>
            {
                if (backBoard[isChoosing[0], isChoosing[1]] == backBoard[j, i] && backBoard[j, i] != ' ')
                {
                    backBoard[isChoosing[0], isChoosing[1]] = ' ';
                    backBoard[j, i] = ' ';
                    board[isChoosing[0], isChoosing[1]].BackColor = Color.White;
                    board[j, i].BackColor = Color.White;

                }
                Task.Delay(100).Wait();
            });
            board[isChoosing[0], isChoosing[1]].Text = null;
            board[j, i].Text = null;
            if (backBoard[j, i] != ' ')
            {
                board[isChoosing[0], isChoosing[1]].BackColor = Color.Gray;
                board[j, i].BackColor = Color.Gray;
            }
            else
            {
                board[isChoosing[0], isChoosing[1]].Enabled = false;
                board[j, i].Enabled = false;
            }
            isChoosing[0] = -1;
            isChoosing[1] = -1;

            if (IsWon())
            {
                MessageBox.Show("Thang roi!");
                counter.Stop();
            }
        }

        private bool IsWon()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (backBoard[j, i] != ' ') return false;
                }
            }
            return true;
        }

        private void InitGUI()
        {
            pb.Maximum = tileSize * 3;
            pb.Value = pb.Maximum;
            pb.Location = new Point(tileSize / 2, tileSize / 4);
            pb.Size = new Size(tileSize * (boardSize - 1), tileSize / 2);

            this.Controls.Add(pb);

            counter.Interval = 100;
            counter.Tick += (object sender, EventArgs args) =>
            {
                pb.Value = pb.Value > pb.Maximum / maxTime ? pb.Value - pb.Maximum / maxTime : 0;
                if (pb.Value <= 0)
                {
                    MessageBox.Show("Thua roi!");
                    counter.Dispose();
                }
            };
        }

    }
}
