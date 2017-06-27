﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku
{
    class Game
    {
        public const int BASE = 3;
        public const int FIELD_SIZE = BASE * BASE;
        public const int SWAPS = 20;
        public const int MIN_INIT_INDEX = 3;
        public const int ELIGIBLE_INDEX = 4;
        public const int MAX_INIT_INDEX = 5;
        
        private int[,] initialState;
        private int[,] field;
        private int[,] solution;

        public Game()
        {
            initialState = new int[FIELD_SIZE, FIELD_SIZE];
            field = new int[FIELD_SIZE, FIELD_SIZE];
            solution = new int[FIELD_SIZE, FIELD_SIZE];
        }

        public void generateLevel()
        {
            //Подготовка
            Random randomizer = new Random();
            int[] seed = new int[9];

            int pos;
            for (int i = 1; i <= FIELD_SIZE; ++i)
            {
                pos = randomizer.Next(FIELD_SIZE);
                while (seed[pos] != 0)
                    pos = (pos + 1) % FIELD_SIZE;
                seed[pos] = i;
            }

            //Заполнение
            for (int i = 0; i < FIELD_SIZE; ++i)
            {
                int reali = i * BASE % FIELD_SIZE + i * BASE / FIELD_SIZE;
                for (int j = 0; j < FIELD_SIZE; ++j)
                {
                    solution[reali, j] = seed[(j + i) % FIELD_SIZE];
                }
            }

            //Перемешивание
            int street, a, b, buf;
            for (int i = 0; i < SWAPS; ++i)
            {
                street = randomizer.Next(BASE);
                a = street * BASE + randomizer.Next(BASE);
                b = street * BASE + randomizer.Next(BASE);
                if (a != b)
                    for (int j = 0; j < FIELD_SIZE; ++j)
                    {
                        buf = solution[a, j];
                        solution[a, j] = solution[b, j];
                        solution[b, j] = buf;
                    }

                street = randomizer.Next(BASE);
                a = street * BASE + randomizer.Next(BASE);
                b = street * BASE + randomizer.Next(BASE);
                if (a != b)
                    for (int j = 0; j < FIELD_SIZE; ++j)
                    {
                        buf = solution[j, a];
                        solution[j, a] = solution[j, b];
                        solution[j, b] = buf;
                    }
            }

            for (int i = 0; i < SWAPS/4; ++i)
            {
                a = randomizer.Next(BASE);
                b = randomizer.Next(BASE);
                if (a != b)
                    for (int j = 0; j < BASE; ++j)
                        for (int k = 0; k < FIELD_SIZE; ++k)
                        {
                            buf = solution[a * BASE + j, k];
                            solution[a * BASE + j, k] = solution[b * BASE + j, k];
                            solution[b * BASE + j, k] = buf;
                        }

                a = randomizer.Next(BASE);
                b = randomizer.Next(BASE);
                if (a != b)
                    for (int j = 0; j < BASE; ++j)
                        for (int k = 0; k < FIELD_SIZE; ++k)
                        {
                            buf = solution[k, a * BASE + j];
                            solution[k, a * BASE + j] = solution[k, b * BASE + j];
                            solution[k, b * BASE + j] = buf;
                        }
            }

            //Определение начального состояния
            Array.Clear(initialState, 0, initialState.Length);
            int[] columnVolumes = new int[FIELD_SIZE];
            int[,] boxVolumes = new int[BASE,BASE];
            for (int i = 0; i < FIELD_SIZE; ++i)
            {
                int rowIndex = 0;
                for (int j = 0; j < 3 && rowIndex != ELIGIBLE_INDEX; ++j)
                    rowIndex = randomizer.Next(MIN_INIT_INDEX, MAX_INIT_INDEX + 1);
                do
                {
                    int attempts = 0;
                    bool success = false;
                    pos = randomizer.Next(FIELD_SIZE);
                    do
                    {
                        if (initialState[i, pos] == 0 && columnVolumes[pos] < MAX_INIT_INDEX && boxVolumes[i / BASE, pos / BASE] < MAX_INIT_INDEX)
                        {
                            initialState[i, pos] = solution[i, pos];
                            ++columnVolumes[pos];
                            ++boxVolumes[i / BASE, pos / BASE];
                            success = true;
                        }
                        else
                        {
                            pos = (pos + 1) % FIELD_SIZE;
                        }
                    } while (!success && attempts++ < FIELD_SIZE);
                } while (--rowIndex > 0);
            }
            setInitState();
        }

        public void setInitState()
        {
            field = (int[,])initialState.Clone();
        }

        public int[,] getInitialState()
        {
            return initialState;
        }

        public int[,] getField()
        {
            return field;
        }

        public int[,] getSolution()
        {
            return solution;
        }
    }
}
