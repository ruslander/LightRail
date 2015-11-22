using System.Collections.Generic;
using System.Linq;

namespace LightRail
{
    public class BTree
    {
        public BTree(int degree)
        {
            Degree = degree;
            Height = 1;
            Root = new BNode(degree);

            //DiskWrite(Root);
        }

        public BNode Root { get; private set; }
        public int Degree { get; private set; }
        public int Height { get; private set; }

        public void Insert(int k)
        {
            if (!Root.HasReachedMaxEntries)
            {
                InsertNonFull(Root, k);
                return;
            }

            var oldRoot = Root;
            Root = new BNode(Degree);
            Root.Children.Add(oldRoot);
            SplitChild(Root, 0, oldRoot);
            InsertNonFull(Root, k);

            Height++;
        }

        public object Search(int key)
        {
            return SearchInternal(Root, key);
        }

        private object SearchInternal(BNode node, int key)
        {
            int i = node.Keys.TakeWhile(entry => key.CompareTo(entry) > 0).Count();

            if (i < node.Keys.Count && node.Keys[i].CompareTo(key) == 0)
            {
                return node.Keys[i];
            }

            return node.IsLeaf ? null : SearchInternal(node.Children[i], key);
        }

        private void SplitChild(BNode internalNode, int fullNodeIndex, BNode fullNode)
        {
            var newNode = new BNode(Degree);

            internalNode.Keys.Insert(fullNodeIndex, fullNode.Keys[Degree - 1]);
            internalNode.Children.Insert(fullNodeIndex + 1, newNode);

            newNode.Keys.AddRange(fullNode.Keys.GetRange(Degree, Degree - 1));

            fullNode.Keys.RemoveRange(Degree - 1, Degree);

            if (!fullNode.IsLeaf)
            {
                newNode.Children.AddRange(fullNode.Children.GetRange(Degree, Degree));
                fullNode.Children.RemoveRange(Degree, Degree);
            }

            //DiskWrite(fullNode);
            //DiskWrite(newNode);
            //DiskWrite(internalNode);
        }

        private void InsertNonFull(BNode node, int k)
        {
            var positionToInsert = node.Keys.TakeWhile(key => k.CompareTo((int) key) >= 0).Count();

            if (node.IsLeaf)
            {
                node.Keys.Insert(positionToInsert, k);
                //DiskWrite(node);
                return;
            }

            var child = node.Children[positionToInsert];
            if (child.HasReachedMaxEntries)
            {
                SplitChild(node, positionToInsert, child);
                if (k.CompareTo(node.Keys[positionToInsert]) > 0)
                {
                    positionToInsert++;
                }
            }

            InsertNonFull(node.Children[positionToInsert], k);
        }
    }

    public class BNode
    {
        private readonly int _degree;

        public BNode(int degree)
        {
            _degree = degree;

            Children = new List<BNode>(degree);
            Keys = new List<int>(degree);
        }

        public List<BNode> Children { get; set; }
        public List<int> Keys { get; set; }

        public bool IsLeaf
        {
            get { return Children.Count == 0; }
        }

        public bool HasReachedMaxEntries
        {
            get { return Keys.Count == (2 * _degree) - 1; }
        }

        public bool HasReachedMinEntries
        {
            get { return Keys.Count == _degree - 1; }
        }

        public override string ToString()
        {
            return Keys.Count() + " " + string.Join(",", Keys);
        }
    }
}