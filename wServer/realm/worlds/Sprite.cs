namespace wServer.realm.worlds
{
    public class SpriteWorld : World
    {
        public SpriteWorld()
        {
            Name = "Sprite World";
            Background = 1;
            AllowTeleport = true;
            base.FromWorldMap(
                typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.sprite.wmap"));
        }

        public override World GetInstance(ClientProcessor psr)
        {
            return RealmManager.AddWorld(new SpriteWorld());
        }
    }
}