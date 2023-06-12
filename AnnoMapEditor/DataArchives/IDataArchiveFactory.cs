namespace AnnoMapEditor.DataArchives
{
    public interface IDataArchiveFactory
    {
        IDataArchive CreateDataArchive(string dataPath);
    }
}
