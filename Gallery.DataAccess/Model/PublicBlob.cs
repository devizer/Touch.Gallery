namespace Gallery.Logic.Model
{
    public class PublicBlob
    {
        public string Id;
        public int Width;
        public int Height;
        public int File;
        public int Position;
        public int Length;
        public string IdContent;

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(File)}: {File}, {nameof(Position)}: {Position}, {nameof(Length)}: {Length}, {nameof(IdContent)}: {IdContent}";
        }
    }
}