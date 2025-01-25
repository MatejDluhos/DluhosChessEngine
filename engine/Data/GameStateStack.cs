using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Data
{
    internal class GameStateStack
    {
        private Stack<GameState> stack;

        public GameStateStack()
        {
            stack = new Stack<GameState>();
        }

        public void Push(GameState state)
        {
            stack.Push(state);
        }

        public GameState Pop()
        {
            return stack.Pop();
        }

        public GameState Peek()
        {
            return stack.Peek();
        }

        public void Clear()
        {
            stack.Clear();
        }

        public int Count()
        {
            return stack.Count;
        }

        public bool IsEmpty()
        {
            return stack.Count == 0;
        }
    }
}
