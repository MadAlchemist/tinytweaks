
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using System.Reflection;
using Vintagestory.API.MathTools;


namespace tinytweaks.src
{
    public class TinyTweaks : ModSystem
    {
        public ModConfig Config;

        public static WorldMapManager manager;
        public static void Stepheight(IClientPlayer player)
        {
            if (player.Entity.Controls.Sprint)
            {
                player.Entity.GetBehavior<EntityBehaviorControlledPhysics>().stepHeight = 1.0F;
            }
            else
            {
                player.Entity.GetBehavior<EntityBehaviorControlledPhysics>().stepHeight = 0.9F;
            }
        }

        public override void Start(ICoreAPI api)
        {
            try
            {
                Config = api.LoadModConfig<ModConfig>("tinytweaks.json");
            } catch
            {
                System.Diagnostics.Debug.WriteLine("Failed to load config file \"tinytweaks.json\"!");
                System.Diagnostics.Debug.WriteLine("Creating new config file \"tinytweaks.json\"!");
                Config = new ModConfig();
                api.StoreModConfig<ModConfig>(Config, "tinytweaks.json");
            }

            if(Config == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load config file \"tinytweaks.json\"!");
                System.Diagnostics.Debug.WriteLine("Creating new config file \"tinytweaks.json\"!");
                Config = new ModConfig();
                api.StoreModConfig<ModConfig>(Config, "tinytweaks.json");
            }

            base.Start(api);

        }


        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            if (Config.AutoJumpWhenSprinting)
            {
                // Game Time Tick Listener 50ms
                api.Event.RegisterGameTickListener(dt =>
                {
                    var player = api.World.Player;
                    Stepheight(player);
                }, 50); //50 ms/tick
            }
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            if(Config.DeathMarksEnabled)
            {
                api.Event.PlayerDeath += onPlayerDeath;
                manager = api.ModLoader.GetModSystem<Vintagestory.GameContent.WorldMapManager>();
            }
        }

        public override void Dispose()
        {

        }

        
        private void onPlayerDeath(IServerPlayer p, DamageSource ds)
        {
            Waypoint waypoint = new Waypoint();
            string datetime = System.DateTime.Now.ToString();

            waypoint.Text = "Death "+datetime+" Y="+p.Entity.ServerPos.Y.ToString();
            waypoint.Title = waypoint.Text;
            waypoint.Position = p.Entity.Pos.XYZ;
            waypoint.OwningPlayerUid = p.PlayerUID;
            waypoint.Icon = "circle";
            waypoint.Color = -65536;
            waypoint.Pinned = true;

            WorldMapManagerExtensions.AddWaypointToPlayer(manager, waypoint, p);
        }
    }
}