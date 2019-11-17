namespace ZipAppModel
{
    public class Chunk
    {
        public Chunk(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }
        public Chunk() { }
        public int Id { get; set; }
        public byte[] Data { get; set; }
    }
}