﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Sudoku
{
    public partial class Main : Form
    {
        private static Color CURSOR_COLOR = Color.FromArgb(100, 0, 255, 255);

        private Game game;
        private Label[,] cells;
        private int cursori, cursorj;

        public Main()
        {
            InitializeComponent();

            Padding margin = new Padding(0, 0, 0, 0);
            cells = new Label[Game.FIELD_SIZE, Game.FIELD_SIZE];
            for (int i = 0; i < Game.FIELD_SIZE; ++i)
                for (int j = 0; j < Game.FIELD_SIZE; ++j)
                {
                    Label cell = new Label();
                    cell.Font = new Font(new FontFamily("Courier New"), 36, FontStyle.Bold);
                    cell.AutoSize = false;
                    cell.Dock = DockStyle.Fill;
                    cell.TextAlign = ContentAlignment.MiddleCenter;
                    cell.BackColor = Color.Transparent;
                    cell.ForeColor = Color.Black;
                    cell.Click += onCellClicked;
                    cell.MouseEnter += (sender, args) =>
                    {
                        cells[cursori, cursorj].BackColor = Color.Transparent;




                        (sender as Label).BackColor = CURSOR_COLOR;
                        cursori = grid.GetRow(cell);
                        cursorj = grid.GetColumn(cell);
                    };
                    cell.MouseLeave += (sender, args) =>
                    {
                        (sender as Label).BackColor = Color.Transparent;
                    };
                    cell.Margin = margin;
                    cells[i, j] = cell;
                    grid.Controls.Add(cell, j, i);
                }

            game = new Game();
            restart(0);
        }

        private void onCellClicked(object sender, EventArgs e)
        {
            Label cell = sender as Label;
            int i = grid.GetRow(cell), j = grid.GetColumn(cell);
            if (game.InitialState[i, j] == 0)
            {
                int digit = -1;
                switch ((e as MouseEventArgs).Button)
                {
                    case MouseButtons.Left:
                        if (подсказкаToolStripMenuItem.Checked)
                        {
                            digit = game.Solution[i, j];
                            switchHintMode(false);
                        }
                        else
                        {
                            Dial f = new Dial();
                            f.ShowDialog();
                            digit = f.getResult();
                        }
                        break;
                    case MouseButtons.Right:
                        digit = 0;
                        break;
                }

                if (digit >= 0 && digit <= 9)
                    handleCellInput(cell, digit);
            }
        }

        private void handleCellInput(Label cell, int input)
        {
            int i = grid.GetRow(cell), j = grid.GetColumn(cell);
            int[,] field = game.Field;
            int oldValue = field[i, j];
            field[i, j] = 0;
            checkCell(i, j, oldValue, true);              
            field[i, j] = input;
            cell.Text = input == 0 ? "" : input.ToString();                        
            checkCell(i, j, 0, true);
            refreshStatusLabel();

            if (game.CorrectCells == Game.FIELD_SIZE * Game.FIELD_SIZE)
            {
                Thread victoryThread = new Thread(victory);
                victoryThread.Start();
            }
        }

        private void restart(int difficulty)
        {         
            game.generateLevel(difficulty);
            fillCells();
            refreshStatusLabel();
        }

        private void clear()
        {
            game.setInitialState();
            fillCells();
            refreshStatusLabel();
        }

        private void refreshStatusLabel()
        {
            statusLabel.Text = game.CorrectCells + " / " + Game.FIELD_SIZE * Game.FIELD_SIZE;
        }
        
        private void victory()
        {
            int waveCount = Game.FIELD_SIZE * 2 - Game.FIELD_SIZE % 2;
            for (int l = 0; l < waveCount; ++l)
            {
                Thread anotherThread = new Thread(() =>
                {
                    int i = l;
                    for (int k = 200; k >= 0; k -= 10)
                    {
                        for (int j = 0; j <= i; ++j)
                        {
                            if (i - j < Game.FIELD_SIZE && j < Game.FIELD_SIZE)
                            {
                                cells[i - j, j].BackColor = Color.FromArgb(k, 0, 255, 0);
                            }
                        }
                        Thread.Sleep(30);
                    }
                });
                anotherThread.Start();
                Thread.Sleep(1000 / waveCount);
            }
        }

        private void fillCells()
        {
            int[,] field = game.Field;
            int[,] initialState = game.InitialState;
            for (int i = 0; i < Game.FIELD_SIZE; ++i)
                for (int j = 0; j < Game.FIELD_SIZE; ++j)
                {
                    cells[i, j].Text = field[i, j] == 0 ? "" : field[i, j].ToString();
                    if (initialState[i, j] > 0)
                        cells[i, j].ForeColor = Color.DarkGreen;
                    else
                        cells[i, j].ForeColor = Color.Black;
                }
        }

        private void checkCell(int i, int j, int oldValue, bool recursive)
        {
            Label target = cells[i, j];
            int[,] field = game.Field;
            int value = field[i, j];
            int checkingFor = oldValue > 0 ? oldValue : field[i, j];
            int boxi = i / Game.BASE, boxj = j / Game.BASE;

            if (checkingFor > 0)
            {
                List<Point> repetitions = game.findRepetitionsInSubmatrix(i, 0, i, Game.FIELD_SIZE - 1, checkingFor, i, j);
                repetitions.AddRange(game.findRepetitionsInSubmatrix(0, j, Game.FIELD_SIZE - 1, j, checkingFor, i, j));
                repetitions.AddRange(game.findRepetitionsInSubmatrix(boxi * Game.BASE, boxj * Game.BASE,
                    boxi * Game.BASE + Game.BASE - 1, boxj * Game.BASE + Game.BASE - 1, checkingFor, i, j));

                if (target.ForeColor != Color.DarkGreen)
                {
                    bool correct;
                    if (repetitions.Count == 0)
                    {
                        target.ForeColor = Color.Black;
                        correct = true;
                    }
                    else
                    {
                        target.ForeColor = Color.DarkRed;
                        correct = false;
                    }

                    if (correct && value > 0 && !game.CorrectnessTable[i, j])
                    {
                        ++game.CorrectCells;
                        game.CorrectnessTable[i, j] = true;
                    }
                    else if ((!correct || value == 0) && game.CorrectnessTable[i, j])
                    {
                        --game.CorrectCells;
                        game.CorrectnessTable[i, j] = false;
                    }
                }

                if (recursive)
                    for (int k = 0; k < repetitions.Count; ++k)
                        checkCell(repetitions[k].X, repetitions[k].Y, oldValue, false);
            }
        }

        private void switchHintMode(bool hintMode)
        {
            подсказкаToolStripMenuItem.Checked = hintMode;
            grid.Cursor = hintMode ? Cursors.Help : Cursors.Default;
        }

        private void очиститьПолеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите очистить поле?", "Подтвердите действие", MessageBoxButtons.YesNo) == DialogResult.Yes)
                clear();
        }

        private void подсказкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchHintMode(подсказкаToolStripMenuItem.Checked);
        }

        private void победитьToolStripMenuItem_Click(object sender, EventArgs e) //Метод для отладки
        {
            for (int i = 0; i < Game.FIELD_SIZE; ++i)
                for (int j = 0; j < Game.FIELD_SIZE; ++j)
                {
                    game.Field[i, j] = game.Solution[i, j];
                    game.CorrectnessTable[i, j] = true;
                }
            game.CorrectCells = Game.FIELD_SIZE*Game.FIELD_SIZE;
            fillCells();
            refreshStatusLabel();

            Thread victoryThread = new Thread(victory);
            victoryThread.Start();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue >= 37 && e.KeyValue <= 40)
            {
                cells[cursori, cursorj].BackColor = Color.Transparent;
                switch (e.KeyValue)
                {
                    case 37:
                        if (cursorj > 0)
                            --cursorj;
                        break;
                    case 38:
                        if (cursori > 0)
                            --cursori;
                        break;
                    case 39:
                        if (cursorj < Game.FIELD_SIZE - 1)
                            ++cursorj;
                        break;
                    case 40:
                        if (cursori < Game.FIELD_SIZE - 1)
                            ++cursori;
                        break;
                }
                cells[cursori, cursorj].BackColor = CURSOR_COLOR;
            }
            else if (game.InitialState[cursori, cursorj] == 0)
            {
                if (e.KeyValue >= '0' && e.KeyValue <= '9' || e.KeyValue >= 96 /*Num0*/ && e.KeyValue <= 105 /*Num9*/)
                    handleCellInput(cells[cursori, cursorj], e.KeyValue - '0' * (e.KeyValue / '0'));
                else if (e.KeyCode == Keys.Delete)
                    handleCellInput(cells[cursori, cursorj], 0);
            }
        }

        private bool newGameConfirmation()
        {
            return MessageBox.Show("Вы действительно хотите начать новую игру?", "Подтвердите действие",
                MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void оченьЛегкоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGameConfirmation())
                restart(-2);
        }

        private void легкоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGameConfirmation())
                restart(-1);
        }

        private void сToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGameConfirmation())

                restart(0);
        }

        private void сложноToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGameConfirmation())
                restart(1);
        }

        private void оченьСложноToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (newGameConfirmation())
                restart(2);
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reference reference = new Reference();
            reference.ShowDialog();
        }
    }
}