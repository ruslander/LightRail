using System;
using LightRail;
using NUnit.Framework;

namespace Specs
{
    [Serializable]
    class ClassWithOneProp
    {
        public string Name { get; set; } 
    }

    [TestFixture]
    public class OplogSpec : SpecificationWithFile
    {
        [Test]
        public void OpBinaryPackUnpack()
        {
            var o1 = new Op() { Body = new ClassWithOneProp() { Name = "hello" } };
            var payload = o1.ToBinary();


            var o2 = Op.FromBinary(payload);
            var obj = (ClassWithOneProp)o2.Body;

            Assert.That(obj.Name, Is.EqualTo("hello"));
        }

        [Test]
        public void Oplog()
        {
            var lw = new Oplog(Cfg4K);
            lw.Append(new Op() {Body = 333});
            lw.Dispose();

            var lr = new Oplog(Cfg4K);
            var op = lw.Head();
            lr.Dispose();


            Assert.That(op.Body, Is.EqualTo(333));
        }
    }

    
}