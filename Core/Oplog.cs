using System.Collections.Generic;
using System.Linq;

namespace LightRail.Core
{
    public class Oplog
    {
        public int Mb = 1048576;
        public string Name { get; set; }

        public List<FileSegment> Segments;
        public FileSegment CurrentSegment;

        public IManageSegments ManageSegments;

        public Oplog(string name = "") : this(name, new SegmentManager(name)) { }

        public Oplog(string name, IManageSegments sm)
        {
            Name = name;
            ManageSegments = sm;

            var segments = sm.GetAll();
            
            Segments = segments;
            
            if (segments.Any())
            {
                CurrentSegment = segments.LastOrDefault();
            }
            else
            {
                var newSegment = sm.AllocateSegment(0);

                CurrentSegment = newSegment;
                Segments.Add(newSegment);
            }
        }

        public void Append(byte[] payload)
        {
            var op = new Op(payload);

            var length = CurrentSegment.Writer.BaseStream.Length;

            if (length + op.Length > Mb)
            {
                var next = Segments.Count*Mb;

                var newSegment = ManageSegments.AllocateSegment(next);

                CurrentSegment = newSegment;
                Segments.Add(newSegment);
            }

            op.WriteTo(CurrentSegment.Writer);
        }

        public IEnumerable<Op> Forward(int position = 0, int sliceSize = int.MaxValue)
        {
            foreach (var segment in Segments)
            {
                while (segment.Reader.BaseStream.Position != segment.Reader.BaseStream.Length)
                {
                    yield return Op.ReadFrom(segment.Reader);
                }
            }
        }

        public void Flush()
        {
            Segments.ForEach(x=>x.Flush());
        }
    }
}