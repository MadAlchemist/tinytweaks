using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace tinytweaks.src
{
    public class EntityBehaviorAir : EntityBehavior
    {
        ITreeAttribute airTree;

        public bool waterBreather = false;
        float damageOn = 0;
        double timer;
        
        public float Air
        {
            get { return airTree.GetFloat("currentair"); }
            set { airTree.SetFloat("currentair", GameMath.Clamp(value, 0, MaxAir)); entity.WatchedAttributes.MarkPathDirty("air"); }
        }

        public float MaxAir
        {
            get { return airTree.GetFloat("maxair"); }
            set { airTree.SetFloat("maxair", value); entity.WatchedAttributes.MarkPathDirty("air"); }
        }

        public float BaseMaxAir
        {
            get { return airTree.GetFloat("basemaxair"); }
            set
            {
                airTree.SetFloat("basemaxair", value);
                entity.WatchedAttributes.MarkPathDirty("air");
            }
        }

        public Dictionary<string, float> MaxAirModifiers = new Dictionary<string, float>();

        public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
        {
            timer = entity.World.Calendar.TotalHours;
            waterBreather = typeAttributes.IsTrue("waterBreather");
            airTree = entity.WatchedAttributes.GetTreeAttribute("air");

            if (airTree == null)
            {
                entity.WatchedAttributes.SetAttribute("air", airTree = new TreeAttribute());
                Air = typeAttributes["currentair"].AsFloat(20);
                BaseMaxAir = typeAttributes["maxair"].AsFloat(20);
                UpdateMaxAir();
                return;
            }

            Air = airTree.GetFloat("currentair");
            BaseMaxAir = airTree.GetFloat("basemaxair");

            if (BaseMaxAir == 0) BaseMaxAir = typeAttributes["maxair"].AsFloat(20);

            UpdateMaxAir();
        }

        public void UpdateMaxAir()
        {
            float totalMaxAir = BaseMaxAir;
            foreach (var val in MaxAirModifiers) totalMaxAir += val.Value;

            totalMaxAir += entity.Stats.GetBlended("maxairExtraPoints") - 1;

            bool wasFullAir = Air >= MaxAir;

            MaxAir = totalMaxAir;

            if (wasFullAir) Air = MaxAir;
        }

        public override void OnGameTick(float deltaTime)
        {
            if (!entity.Alive) return;
            if (EntityUnderwater())
            {
                //In water
                if (waterBreather)
                {
                    if (Air < MaxAir) { Air += MaxAir; }
                }
                else
                {
                    if (Air > 0) Air -= MaxAir*deltaTime/50;
                }
            }
            else
            {
                //On land
                if (waterBreather)
                {
                    if (Air > 0) Air -= MaxAir*deltaTime/50;
                }
                else
                {
                    if (Air < MaxAir) { Air += MaxAir; }
                }
            }

            if (Air <= 0)
            {
                damageOn += deltaTime;

                if (damageOn >= 1)
                {
                    entity.ReceiveDamage(new DamageSource() { Type = EnumDamageType.Suffocation, Source = EnumDamageSource.Drown }, 1f);
                    damageOn = 0;
                }
            }
        }

        public bool EntityUnderwater()
        {
            if (!entity.Swimming) return false;

            Vec3d head = entity.SidedPos.XYZ.AddCopy(0, entity.CollisionBox.Height, 0);
            Block liquid = entity.World.BlockAccessor.GetBlock(head.AsBlockPos);

            return liquid.IsLiquid() && (entity.World.BlockAccessor.GetBlock(head.AsBlockPos.Add(0, 1, 0)).IsLiquid() || head.Y - (0.25 * entity.CollisionBox.Height) < head.AsBlockPos.Y + ((liquid.LiquidLevel + 1) / 8));
        }

        public EntityBehaviorAir(Entity entity) : base(entity)
        {
        }

        public override string PropertyName()
        {
            return "air";
        }
    }
}
