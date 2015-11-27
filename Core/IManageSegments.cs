using System.Collections.Generic;

namespace LightRail.Core
{
    public interface IManageSegments
    {
        List<FileSegment> GetAll();
        FileSegment AllocateSegment(long position);
    }
}