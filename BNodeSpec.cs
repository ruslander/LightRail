using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace LightRail
{
    // http://www.cs.utexas.edu/users/djimenez/utsa/cs3343/lecture17.html
    // https://github.com/rdcastro/btree-dotnet/blob/master/BTree/BTree.cs

    [TestFixture]
    public class BNodeSpec
    {
        [Test]
        public void BTree_initially_height_1()
        {
            Assert.That(new BTree(3).Height, Is.EqualTo(1));
        }

        [Test]
        public void Step1()
        {
/*
            Step 1: Insert 5
                                  ___
                                 |_5_| */

            var tree = new BTree(2);
            tree.Insert(5);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
        }

        [Test]
        public void Step2()
        {
            /*
            Step 2: Insert 9
            B-Tree-Insert simply calls B-Tree-Insert-Nonfull, putting 9 to the
            right of 5:
                                             _______
                                            |_5_|_9_|            */

            var tree = new BTree(2);
            tree.Insert(5);
            
            tree.Insert(9);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
            Assert.That(tree.Root.Keys[1], Is.EqualTo(9));
        }

        [Test]
        public void Step3()
        {
            /*
            Step 3: Insert 3
            Again, B-Tree-Insert-Nonfull is called
                                           ___ _______
                                          |_3_|_5_|_9_|             */

            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            
            tree.Insert(3);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(3));
            Assert.That(tree.Root.Keys[1], Is.EqualTo(5));
            Assert.That(tree.Root.Keys[2], Is.EqualTo(9));
        }

        [Test]
        public void Step4()
        {
            /*
            Step 4: Insert 7
            Tree is full.  We allocate a new (empty) node, make it the root, split
            the former root, then pull 5 into the new root:
                                             ___
                                            |_5_|
                                         __ /   \__
                                        |_3_|  |_9_|

            Then insert we insert 7; it goes in with 9
                                             ___
                                            |_5_|
                                         __ /   \______
                                        |_3_|  |_7_|_9_|             */

            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            
            tree.Insert(7);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(3));
            Assert.That(tree.Root.Keys[1], Is.EqualTo(5));
            Assert.That(tree.Root.Keys[2], Is.EqualTo(9));
        }

    }

    public class BTree
    {
        public BNode Root { get; private set; }
        public int Degree { get; private set; }
        public int Height { get; private set; }

        public BTree(int degree)
        {
            Degree = degree;
            Height = 1;
            Root = new BNode(degree);
        }

        public void Insert(int k)
        {
            if (!Root.HasReachedMaxEntries)
            {
                InsertNonFull(Root, k);
                return;
            }
        }

        private void InsertNonFull(BNode node, int k)
        {
            int positionToInsert = node.Keys.TakeWhile(key => k.CompareTo(key) >= 0).Count();

            if (node.IsLeaf)
            {
                node.Keys.Insert(positionToInsert, k);
                return;
            }
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
    }
}
