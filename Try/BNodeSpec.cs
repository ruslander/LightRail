using System.Collections.Generic;
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
        public void Search_for_non_exsting_key()
        {
            var tree = new BTree(2);
            tree.Insert(5);

            Assert.That(tree.Search(9999), Is.Null);
        }

        [Test]
        public void Search_returns_key_on_match()
        {
            var tree = new BTree(2);
            tree.Insert(5);

            Assert.That(tree.Search(5), Is.EqualTo(5));
        }

        [Test]
        public void Step1()
        {
            /*
            Step 1: Insert 5
                                  ___
                                 |_5_| 
            */

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

            Assert.That(tree.Root.Keys, Is.EquivalentTo(new List<int>() { 5,9 }));
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

            Assert.That(tree.Root.Keys, Is.EquivalentTo(new List<int>() { 3,5,9 }));

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

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
            Assert.That(tree.Root.Children[0].Keys[0], Is.EqualTo(3));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 7,9 }));

        }

        [Test]
        public void Step5()
        {
            /*
            Step 5: Insert 1
            It goes in with 3
                                             ___
                                            |_5_|
                                     ___ __ /   \______
                                    |_1_|_3_|  |_7_|_9_|             */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);

            tree.Insert(1);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>() { 1, 3 }));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 7, 9 }));

        }

        [Test]
        public void Step6()
        {
            /*
            Step 6: Insert 2
            It goes in with 3
                                             ___
                                            |_5_|
                                           /     \
                                   ___ __ /___    \______
                                  |_1_|_2_|_3_|  |_7_|_9_|             */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(1);

            tree.Insert(2);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>() { 1, 2, 3 }));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 7, 9 }));
        }

        [Test]
        public void Step7()
        {
            /*
            Step 7: Insert 8
            It goes in with 9
 
                                             ___
                                            |_5_|
                                           /     \
                                   ___ __ /___    \__________
                                  |_1_|_2_|_3_|  |_7_|_8_|_9_|            */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(1);
            tree.Insert(2);

            tree.Insert(8);

            Assert.That(tree.Root.Keys[0], Is.EqualTo(5));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>() { 1, 2, 3 }));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 7,8,9 }));
        }

        [Test]
        public void Step8()
        {
            /*
            Step 8: Insert 6
            It would go in with |7|8|9|, but that node is full.  So we split it,
            bringing its middle child into the root:

                                            _______
                                           |_5_|_8_|
                                          /    |   \
                                 ___ ____/__  _|_   \__
                                |_1_|_2_|_3_||_7_| |_9_|

            Then insert 6, which goes in with 7:
                                            _______
                                        ___|_5_|_8_|__
                                       /       |      \
                              ___ ____/__    __|____   \__
                             |_1_|_2_|_3_|  |_6_|_7_|  |_9_|           */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(1);
            tree.Insert(2);
            tree.Insert(8);
            
            tree.Insert(6);

            Assert.That(tree.Root.Keys, Is.EquivalentTo(new List<int>(){5,8}));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>(){1,2,3}));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>(){6,7}));
            Assert.That(tree.Root.Children[2].Keys, Is.EquivalentTo(new List<int>(){9}));
        }

        [Test]
        public void Step9()
        {
            /*
            Step 9: Insert 0

            0 would go in with |1|2|3|, which is full, so we split it, sending the middle
            child up to the root:
                                         ___________
                                        |_2_|_5_|_8_|
                                      _/    |   |    \_
                                    _/      |   |      \_
                                  _/_     __|   |______  \___
                                 |_1_|   |_3_| |_6_|_7_| |_9_| 

            Now we can put 0 in with 1
                                         ___________
                                        |_2_|_5_|_8_|
                                      _/    |   |    \_
                                    _/      |   |      \_
                              ___ _/_     __|   |______  \___
                             |_0_|_1_|   |_3_| |_6_|_7_| |_9_|          */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(1);
            tree.Insert(2);
            tree.Insert(8);
            tree.Insert(6);

            tree.Insert(0);

            Assert.That(tree.Root.Keys, Is.EquivalentTo(new List<int>() { 2, 5, 8 }));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>() { 0, 1 }));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 3 }));
            Assert.That(tree.Root.Children[2].Keys, Is.EquivalentTo(new List<int>() { 6,7 }));
            Assert.That(tree.Root.Children[3].Keys, Is.EquivalentTo(new List<int>() { 9 }));
        }

        [Test]
        public void Step10()
        {
            /*
            Step 10: Insert 4
            It would be nice to just stick 4 in with 3, but the B-Tree algorithm
            requires us to split the full root.  Note that, if we don't do this and
            one of the leaves becomes full, there would be nowhere to put the middle
            key of that split since the root would be full, thus, this split of the
            root is necessary:
                                             ___
                                            |_5_|
                                        ___/     \___
                                       |_2_|     |_8_|
                                     _/    |     |    \_
                                   _/      |     |      \_
                             ___ _/_     __|     |______  \___
                            |_0_|_1_|   |_3_|   |_6_|_7_| |_9_| 

            Now we can insert 4, assured that future insertions will work:

                                             ___
                                            |_5_|
                                        ___/     \___
                                       |_2_|     |_8_|
                                     _/    |     |    \_
                                   _/      |     |      \_
                             ___ _/_    ___|___  |_______ \____
                            |_0_|_1_|  |_3_|_4_| |_6_|_7_| |_9_|           */


            var tree = new BTree(2);
            tree.Insert(5);
            tree.Insert(9);
            tree.Insert(3);
            tree.Insert(7);
            tree.Insert(1);
            tree.Insert(2);
            tree.Insert(8);
            tree.Insert(6);
            tree.Insert(0);

            tree.Insert(4);

            Assert.That(tree.Root.Keys, Is.EquivalentTo(new List<int>() { 5 }));
            Assert.That(tree.Root.Children[0].Keys, Is.EquivalentTo(new List<int>() { 2 }));
            Assert.That(tree.Root.Children[1].Keys, Is.EquivalentTo(new List<int>() { 8 }));
        }
    }
}