using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonsAndDiagramsSolver
{
  class Program
  {
    private static int[] col_count = new int[8];
    private static int[] row_count = new int[8];
    private static bool[,] m = new bool[8, 8];
    private static bool[,] c = new bool[8, 8];
    private static int[] col_priority = new int[8];
    private static bool[,] w = new bool[8, 8];
    private static int[] row_used = new int[8];
    private static int[] col_used = new int[8];

    static void Main(string[] args)
    {
      //eg
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

      col_priority = descOrder(col_count);

      Console.WriteLine("{0:HH:mm:ss}", DateTime.Now);
      Console.WriteLine();

      var yn = recurse(0, 0);

      Console.WriteLine("{0:HH:mm:ss}", DateTime.Now);
      if (yn)
      {
        print();
      }
      else
      {
        Console.WriteLine("Check your input :'(");
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
            if (x != 0)
              for (int b = y; b <= y + 2; b++)
                if (!w[x - 1, b])
                  spacesAround++;
            if (y != 0)
              for (int a = x; a <= x + 2; a++)
                if (!w[a, y - 1])
                  spacesAround++;
            if (x + 2 != 7)
              for (int b = y; b <= y + 2; b++)
                if (!w[x + 3, b])
                  spacesAround++;
            if (y + 2 != 7)
              for (int a = x; a <= x + 2; a++)
                if (!w[a, y + 3])
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

      //monster
      for (int i = 0; i < 8; i++)
        for (int j = 0; j < 8; j++)
          if (!w[i, j] && !c_space[i, j])
          {
            int s = 0;
            if (j != 7 && !w[i, j + 1])
              s++;
            if (i != 7 && !w[i + 1, j])
              s++;
            if (j != 0 && !w[i, j - 1])
              s++;
            if (i != 0 && !w[i - 1, j])
              s++;
            if ((s == 1) != m[i, j])
              return false;
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
    }

    static bool recurse(int row, int pos)
    {
      if (row == 8)
      {
        return tryThis();
      }
      else if (row_used[row] < row_count[row])
      {
        var pseudo_pos = col_priority[pos];
        if (!c[row, pseudo_pos] && !m[row, pseudo_pos] && col_used[pseudo_pos] < col_count[pseudo_pos])
        {
          w[row, pseudo_pos] = true;
          row_used[row]++;
          col_used[pseudo_pos]++;
          if (pos == 7)
          {
            if (row_used[row] != row_count[row])
            {
              w[row, pseudo_pos] = false;
              row_used[row]--;
              col_used[pseudo_pos]--;
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
          w[row, pseudo_pos] = false;
          row_used[row]--;
          col_used[pseudo_pos]--;
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

    static int[] descOrder(int[] arr)
    {
      var ret = new int[arr.Length];
      int i = 0, max = arr.Max();
      while (i < arr.Length)
      {
        for (int j = 0; j < arr.Length; j++)
          if (arr[j] == max)
          {
            ret[i] = j;
            i++;
          }
        max--; //since expect small differences
      }
      return ret;
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
