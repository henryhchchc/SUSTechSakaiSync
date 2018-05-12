namespace SUSTechSakaiSync.CLI
{
    public class SyncItem
    {
        public string Uri { get; set; }
        public string LocalPath { get; set; }

        public override string ToString() => $"[{Uri}] -> [{LocalPath}]";
    }
}