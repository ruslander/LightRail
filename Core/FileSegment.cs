using System.IO;

namespace LightRail.Core
{
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
}