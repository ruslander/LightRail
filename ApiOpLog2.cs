using System.Collections.Generic;

namespace LightRail
{
    class ApiOpLog2
    {
        public List<int> Items = new List<int>();

        public void Append(int i)
        {
            Items.Add(i);
        }

        public IEnumerable<int> Forward(int position = 0, int sliceSize = int.MaxValue)
        {
            var endAt = sliceSize == int.MaxValue ? Items.Count : position + sliceSize;

            for (int i = position; i < endAt; i++)
            {
                yield return Items[i];
            }
        }

        public IEnumerable<int> Backward(int position = int.MaxValue, int sliceSize = int.MaxValue)
        {
            var startWith = position == int.MaxValue ? Items.Count - 1 : position;
            var endAt = sliceSize == int.MaxValue ? 0 : startWith - sliceSize + 1;

            for (int i = startWith; i >= endAt; i--)
            {
                yield return Items[i];
            }
        }
    }
}