using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WMP
{
    public class Konami
    {
        private static readonly Key[] _konami = new[] { Key.Up, Key.Up, Key.Down, Key.Down, Key.Left, Key.Right, Key.Left, Key.Right, Key.B, Key.A };
        int step;

        public Konami()
        {
            step = 0;
        }

        public bool StepKonami(Key k)
        {
            Console.WriteLine("STEP KONAMI " + step + " KEY = " + k);
            if (k == _konami[step])
                step++;
            else
                step = 0;
            if (step == _konami.Length)
            {
                Console.WriteLine("Konami valid !");
                step = 0;
                return true;
            }
            Console.WriteLine("Konami not completed");
            return false;
        }
    }
}
