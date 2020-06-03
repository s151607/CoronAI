using _02285_Programming_Project.AI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;

namespace _02285_Programming_Project.Planning
{
    // Based on: http://csclab.murraystate.edu/~bob.pilgrim/445/munkres.html

    /// <summary>
    /// Solve the assignment problem for a n by m matrix
    /// </summary>
    public class HungarianBipartiteMatching
    {
        // Mask and covers - map is at most 50 x 50, so at most 50 x 50 assignments
        private static int[,] C;
        private static int[,] M;
        private static int[,] path = new int[61, 2];
        private static int[] RowCover = new int[50];
        private static int[] ColCover = new int[50];
        private static int nrow;
        private static int ncol;
        private static int path_count = 0;
        private static int path_row_0;
        private static int path_col_0;
        private static int step;

        public static int[,] SolveMinAssignment(int[,] costMatrix)
        {
            step = 1;
            C = costMatrix.Clone() as int[,];
            nrow = costMatrix.GetLength(0);
            ncol = costMatrix.GetLength(1);
            M = new int[nrow, ncol];

            bool done = false;
            while (!done)
            {
                if (step == 1) StepOne();
                else if (step == 2) StepTwo();
                else if (step == 3) StepThree();
                else if (step == 4) StepFour();
                else if (step == 5) StepFive();
                else if (step == 6) StepSix();
                else if (step == 7)
                {
                    done = true;
                }
            }

            int[,] res = M.Clone() as int[,];
            for (int i = 0; i < nrow; i++)
            {
                for (int j = 0; i < nrow; i++)
                {
                    res[i, j] = M[i, j];
                }
            }
            ResetMaskAndCovers();
            return res;
        }
        private static void ResetMaskAndCovers()
        {
            for (int r = 0; r < nrow; r++)
            {
                RowCover[r] = 0;
                for (int c = 0; c < ncol; c++)
                {
                    M[r, c] = 0;
                }
            }
            for (int c = 0; c < ncol; c++)
                ColCover[c] = 0;
        }
        private static void ResetCovers()
        {
            for (int r = 0; r < nrow; r++) RowCover[r] = 0;
            for (int c = 0; c < ncol; c++) ColCover[c] = 0;
        }
        #region Hungarian Algorithm Steps
        private static void StepOne()
        {
            int minInRow;
            for (int row = 0; row < nrow; row++)
            {
                // Find smallest value in row
                minInRow = C[row, 0];
                for (int col = 0; col < ncol; col++)
                {
                    if (C[row, col] < minInRow) minInRow = C[row, col];
                }
                // Subtract from 
                for (int col = 0; col < ncol; col++) C[row, col] -= minInRow;
            }
            step = 2;
        }
        private static void StepTwo()
        {
            for (int row = 0; row < nrow; row++)
            {
                for (int col = 0; col < ncol; col++)
                {
                    if (C[row, col] == 0 && RowCover[row] == 0 && ColCover[row] == 0)
                    {
                        M[row, col] = 1;
                        RowCover[row] = 1;
                        ColCover[col] = 1;
                    }
                }
            }
            ResetCovers();
            step = 3;
        }
        private static void StepThree()
        {
            for (int row = 0; row < nrow; row++)
            {
                for (int col = 0; col < ncol; col++)
                {
                    if (M[row, col] == 1) ColCover[col] = 1;
                }
            }


            int colCount = 0;
            for (int col = 0; col < ncol; col++)
            {
                if (ColCover[col] == 1) colCount++;
            }

            if (colCount >= ncol || colCount >= nrow) step = 7;
            else step = 4;
        }
        private static void StepFour()
        {
            int row = -1;
            int col = -1;
            bool done;

            done = false;
            while (!done)
            {
                find_a_zero(ref row, ref col);
                if (row == -1)
                {
                    done = true;
                    step = 6;
                }
                else
                {
                    M[row, col] = 2;
                    if (star_in_row(row))
                    {
                        find_star_in_row(row, ref col);
                        RowCover[row] = 1;
                        ColCover[col] = 0;
                    }
                    else
                    {
                        done = true;
                        step = 5;
                        path_row_0 = row;
                        path_col_0 = col;
                    }
                }
            }
        }
        private static void StepFive()
        {
            bool done;
            int r = -1;
            int c = -1;

            path_count = 1;
            path[path_count - 1, 0] = path_row_0;
            path[path_count - 1, 1] = path_col_0;
            done = false;
            while (!done)
            {
                find_star_in_col(path[path_count - 1, 1], ref r);
                if (r > -1)
                {
                    path_count += 1;
                    path[path_count - 1, 0] = r;
                    path[path_count - 1, 1] = path[path_count - 2, 1];
                }
                else
                    done = true;
                if (!done)
                {
                    find_prime_in_row(path[path_count - 1, 0], ref c);
                    path_count += 1;
                    path[path_count - 1, 0] = path[path_count - 2, 0];
                    path[path_count - 1, 1] = c;
                }
            }
            augment_path();
            ResetCovers();
            erase_primes();
            step = 3;
        }
        private static void StepSix()
        {
            int minval = int.MaxValue;
            find_smallest(ref minval);
            for (int r = 0; r < nrow; r++)
                for (int c = 0; c < ncol; c++)
                {
                    if (RowCover[r] == 1)
                        C[r, c] += minval;
                    if (ColCover[c] == 0)
                        C[r, c] -= minval;
                }
            step = 4;
        }
        #endregion
        #region Hungarian Algorithm Auxiliary Functions
        #region Step 4 Auxiliaries
        private static void find_a_zero(ref int row, ref int col)
        {
            int r = 0;
            int c;
            bool done;
            row = -1;
            col = -1;
            done = false;
            while (!done)
            {
                c = 0;
                while (true)
                {
                    if (C[r, c] == 0 && RowCover[r] == 0 && ColCover[c] == 0)
                    {
                        row = r;
                        col = c;
                        done = true;
                    }
                    c += 1;
                    if (c >= ncol || done)
                        break;
                }
                r += 1;
                if (r >= nrow)
                    done = true;
            }
        }

        private static bool star_in_row(int row)
        {
            bool tmp = false;
            for (int c = 0; c < ncol; c++)
                if (M[row, c] == 1)
                    tmp = true;
            return tmp;
        }

        private static void find_star_in_row(int row, ref int col)
        {
            col = -1;
            for (int c = 0; c < ncol; c++)
                if (M[row, c] == 1)
                    col = c;
        }
        #endregion
        #region Step 5 Auxiliaries
        private static void find_star_in_col(int c, ref int r)
        {
            r = -1;
            for (int i = 0; i < nrow; i++)
                if (M[i, c] == 1)
                    r = i;
        }

        private static void find_prime_in_row(int r, ref int c)
        {
            for (int j = 0; j < ncol; j++)
                if (M[r, j] == 2)
                    c = j;
        }

        private static void augment_path()
        {
            for (int p = 0; p < path_count; p++)
                if (M[path[p, 0], path[p, 1]] == 1)
                    M[path[p, 0], path[p, 1]] = 0;
                else
                    M[path[p, 0], path[p, 1]] = 1;
        }

        private static void erase_primes()
        {
            for (int r = 0; r < nrow; r++)
                for (int c = 0; c < ncol; c++)
                    if (M[r, c] == 2)
                        M[r, c] = 0;
        }
        #endregion
        #region Step 6 Auxiliaries
        private static void find_smallest(ref int minval)
        {
            for (int r = 0; r < nrow; r++)
                for (int c = 0; c < ncol; c++)
                    if (RowCover[r] == 0 && ColCover[c] == 0)
                        if (minval > C[r, c])
                            minval = C[r, c];
        }
        #endregion
        #endregion
    }
}
