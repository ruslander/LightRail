using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightRail.Core
{
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
}