using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail
{
    public interface IManageSegments
    {
        List<FileSegment> GetAll();
        FileSegment AllocateSegment(long position);
    }

    public class SegmentManager : IManageSegments
    {
        private readonly string _name;
        private readonly bool _persistent;

        public SegmentManager(string name, bool persistent = true)
        {
            _name = name;
            _persistent = persistent;
        }

        public List<FileSegment> GetAll()
        {
            if (!_persistent)
                return new List<FileSegment>();

            return Directory
                .GetFiles(".", string.Format("{0}*.sf", _name))
                .Select(FileSegment.AsReadonly)
                .OrderBy(x => x.Position)
                .ToList();
        }

        public FileSegment AllocateSegment(long position)
        {
            var name = string.Format("{0}{1}.sf", _name, position.ToString("D12"));

            Stream backStream;

            if (_persistent)
            {
                backStream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
                backStream = new MemoryStream();

            return new FileSegment()
            {
                Name = name,
                Writer = new BinaryWriter(backStream)
            };
        }
    }

    public class FileSegment
    {
        public static FileSegment AsReadonly(string path)
        {
            var name = Path.GetFileName(path);
            var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var position = long.Parse(name.Split('.')[0]); // 000014680064.sf

            return new FileSegment()
            {
                Name = name,
                Reader = reader,
                Position = position
            };
        }

        public long Position { get; set; }
        public string Name { get; set; }
        public BinaryWriter Writer { get; set; }
        public BinaryReader Reader { get; set; }

        public override string ToString()
        {
            return Name + " readonly: " + (Writer == null) ;
        }

        public void Flush()
        {
            if(Writer!= null)
                Writer.Flush();
        }
    }

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