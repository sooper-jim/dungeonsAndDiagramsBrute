using System;
using System.Collections.Generic;

namespace DungeonsAndDiagramsSolver
{
    class Program
    {
        private static bool flip = false;
        private static ulong tries = 0;
        private static readonly ulong patience = 1000000000;
        private static int[] col_count = new int[8];
        private static int[] row_count = new int[8];
        private static int[] used = new int[8];
        private static bool[,] m = new bool[8, 8];
        private static bool[,] c = new bool[8, 8];
        private static bool[,] w = new bool[8, 8];

        static void Main(string[] args)
        {
            var linehd = "51467433";
            var line0 = "6     m m";
            var line1 = "5 m      ";
            var line2 = "4   m    ";
            var line3 = "1        ";
            var line4 = "6        ";
            var line5 = "4        ";
            var line6 = "4        ";
            var line7 = "3c      m";


            for (int i = 0; i < 8; i++)
                col_count[i] = int.Parse(linehd[i].ToString());

            setupLine(line0, 0);
            setupLine(line1, 1);
            setupLine(line2, 2);
            setupLine(line3, 3);
            setupLine(line4, 4);
            setupLine(line5, 5);
            setupLine(line6, 6);
            setupLine(line7, 7);

            //total row permuations
            ulong rowtot = 1;
            for (int i = 0; i < 8; i++)
            {
                int spaces = 8;
                for (int j = 0; j < 8; j++)
                    if (c[i, j] || m[i, j])
                        spaces--;
                rowtot *= (ulong)intNChooseK(spaces, row_count[i]);
            }
            Console.WriteLine("{0} row permutations", rowtot);

            ulong coltot = 1;
            for (int i = 0; i < 8; i++)
            {
                int spaces = 8;
                for (int j = 0; j < 8; j++)
                    if (c[j, i] || m[j, i])
                        spaces--;
                coltot *= (ulong)intNChooseK(spaces, col_count[i]);
            }
            Console.WriteLine("{0} col permutations", coltot);

            flip = coltot < rowtot;
            if (flip)
            {
                Console.WriteLine("Flip it");
                var t = row_count;
                row_count = col_count;
                col_count = t;
                transpBool(ref m);
                transpBool(ref c);
                Console.WriteLine("count to {0}", coltot / patience);
            }
            else { Console.WriteLine("count to {0}", rowtot / patience); }

            Console.WriteLine("{0:HH:mm:ss}", DateTime.Now);
            Console.WriteLine();

            var yn = recurse(0, 0);

            Console.WriteLine(tries);
            Console.WriteLine("{0:HH:mm:ss}", DateTime.Now);
            if (yn)
            {
                print();
            }
            else
            {
                Console.WriteLine("Nope :'(");
            }
            Console.Read();
        }

        static void setupLine(string line, int r)
        {
            row_count[r] = int.Parse(line[0].ToString());
            for (int i = 0; i < 8; i++)
                if (line[i + 1] == 'm') { m[r, i] = true; } else if (line[i + 1] == 'c') { c[r, i] = true; }
        }

        static bool tryThis()
        {
            tries++;
            if (tries % patience == 0)
                Console.WriteLine("{0} - {1:HH:mm:ss}", tries / patience, DateTime.Now);

            //check column numbers
            for (int i = 0; i < 8; i++)
            {
                int v = 0;
                for (int j = 0; j < 8; j++)
                    if (w[j, i])
                        v++;
                if (v != col_count[i])
                    return false;
            }

            //monster
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (!w[i, j])
                    {
                        int s = 0;
                        if (j < 7 && !w[i, j + 1])
                            s++;
                        if (i < 7 && !w[i + 1, j])
                            s++;
                        if (j > 0 && !w[i, j - 1])
                            s++;
                        if (i > 0 && !w[i - 1, j])
                            s++;
                        if ((s == 1) != m[i, j])
                            return false;
                    }

            //chest
            var c_space = new bool[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (c[i, j])
                    {
                        int x = 0, y = 0;
                        if (!chestAreaTopLeft(i, j, ref x, ref y))
                            return false;

                        int spacesAround = 0;
                        if (y > 0)
                            for (int a = x; a <= x + 2; a++)
                                if (!w[a, y - 1])
                                    spacesAround++;
                        if (x + 3 < 7)
                            for (int b = y; b <= y + 2; b++)
                                if (!w[x + 3, b])
                                    spacesAround++;
                        if (y + 3 < 7)
                            for (int a = x; a <= x + 2; a++)
                                if (!w[a, y + 3])
                                    spacesAround++;
                        if (x > 0)
                            for (int b = y; b <= y + 2; b++)
                                if (!w[x - 1, b])
                                    spacesAround++;
                        if (spacesAround != 1)
                            return false;

                        int chestsInBlock = 0;
                        for (int a = x; a <= x + 2; a++)
                            for (int b = y; b <= y + 2; b++)
                                if (c[a, b])
                                    chestsInBlock++;
                        if (chestsInBlock != 1)
                            return false;

                        for (int a = x; a <= x + 2; a++)
                            for (int b = y; b <= y + 2; b++)
                                c_space[a, b] = true;
                    }

            //no 2x2
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    if(!w[i,j] && !c_space[i, j])
                    {
                        if (!w[i + 1, j] && !w[i, j + 1] && !w[i + 1, j + 1] &&
                            !c_space[i + 1, j] && !c_space[i, j + 1] && !c_space[i + 1, j + 1])
                            return false;
                    }

            //contiguous space
            int startX = -1, startY = -1;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (!w[i, j])
                    {
                        startX = i;
                        startY = j;
                    }
            var seen = new Dictionary<Tuple<int, int>, bool>();
            var t = Tuple.Create(startX, startY);
            seen.Add(t, false);
            var todo = new Stack<Tuple<int, int>>();
            todo.Push(t);
            while (todo.Count > 0)
            {
                var v = todo.Pop();
                
                if (v.Item1 < 7)
                {
                    t = Tuple.Create(v.Item1 + 1, v.Item2);
                    if (!w[t.Item1, t.Item2] && !seen.ContainsKey(t))
                    {
                        seen.Add(t, false);
                        todo.Push(t);
                    }
                }
                
                if (v.Item2 < 7)
                {
                    t = Tuple.Create(v.Item1, v.Item2 + 1);
                    if (!w[t.Item1, t.Item2] && !seen.ContainsKey(t))
                    {
                        seen.Add(t, false);
                        todo.Push(t);
                    }
                }
                
                if (v.Item1 > 0)
                {
                    t = Tuple.Create(v.Item1 - 1, v.Item2);
                    if (!w[t.Item1, t.Item2] && !seen.ContainsKey(t))
                    {
                        seen.Add(t, false);
                        todo.Push(t);
                    }
                }

                if (v.Item2 > 0)
                {
                    t = Tuple.Create(v.Item1, v.Item2 - 1);
                    if (!w[t.Item1, t.Item2] && !seen.ContainsKey(t))
                    {
                        seen.Add(t, false);
                        todo.Push(t);
                    }
                }
            }
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (!w[i, j] && !seen.ContainsKey(Tuple.Create(i, j)))
                        return false;

            return true;
        }
        static void print()
        {
            if (flip)
            {
                transpBool(ref w);
                transpBool(ref m);
                transpBool(ref c);
            }

            Console.WriteLine("--------");
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                    if (w[i, j])
                    {
                        Console.Write('X');
                    }
                    else if (m[i, j])
                    {
                        Console.Write('O');
                    }
                    else if (c[i, j])
                    {
                        Console.Write('#');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                Console.WriteLine();
            }

            if (flip)
            {
                transpBool(ref w);
                transpBool(ref m);
                transpBool(ref c);
            }
        }

        static int factorial(int x)
        {
            int result = 1;
            for (int i = 1; i <= x; i++)
                result *= i;
            return result;
        }

        static int intNChooseK(int n, int k)
        {
            return factorial(n) / (factorial(k) * factorial(n - k));
        }

        static bool recurse(int row, int pos)
        {
            if (row == 8)
            {
                return tryThis();
            }
            else if (used[row] < row_count[row])
            {
                if (!c[row, pos] && !m[row, pos])
                {
                    w[row, pos] = true;
                    used[row]++;
                    if (pos == 7)
                    {
                        if (used[row] != row_count[row])
                        {
                            w[row, pos] = false;
                            used[row]--;
                            return false;
                        }
                        if (recurse(row + 1, 0))
                            return true;
                    }
                    else
                    {
                        if (recurse(row, pos + 1))
                            return true;
                    }
                    w[row, pos] = false;
                    used[row]--;
                }

                if (pos == 7)
                {
                    return false;
                }
                else
                {
                    if (recurse(row, pos + 1))
                        return true;
                }
            }
            else
            {
                if (recurse(row + 1, 0))
                    return true;
            }
            return false;
        }

        static void transpBool(ref bool[,] x)
        {
            var len = Math.Min(x.GetUpperBound(0), x.GetUpperBound(1));
            bool t;
            for (int i = 0; i <= len; i++)
                for (int j = i + 1; j <= len; j++)
                {
                    t = x[i, j];
                    x[i, j] = x[j, i];
                    x[j, i] = t;
                }
        }

        static bool chestAreaTopLeft(int i, int j, ref int x, ref int y)
        {
            for (x = i - 2; x <= i; x++)
            {
                if (x < 0 || x + 2 > 7)
                    continue;
                for (y = j - 2; y <= j; y++)
                {
                    if (y < 0 || y + 2 > 7)
                        continue;
                    bool valid = true;
                    for (int a = x; a <= x + 2; a++)
                        for (int b = y; b <= y + 2; b++)
                            if (w[a, b])
                            {
                                valid = false;
                                a = 99; //ugly
                                break;
                            }
                    if (valid)
                        return true;
                }
            }
            return false;
        }
    }
}
