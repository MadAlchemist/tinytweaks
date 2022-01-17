using HarmonyLib;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace tinytweaks.src
{
    [HarmonyPatch(typeof(EntitySidedProperties))]
    public class BreatheOverride
    {
        [HarmonyPrepare]
        static bool Prepare()
        {
            return true;
        }

        [HarmonyPatch("loadBehaviors")]
        [HarmonyPostfix]
        static void ChangeToAir(Entity entity, EntityProperties properties, EntitySidedProperties __instance, JsonObject[] ___BehaviorsAsJsonObj)
        {
            for (int i = 0; i < __instance.Behaviors.Count; i++)
            {
                if (__instance.Behaviors[i] is EntityBehaviorBreathe)
                {
                    EntityBehavior air = new EntityBehaviorAir(entity);
                    air.Initialize(properties, ___BehaviorsAsJsonObj[i]);

                    __instance.Behaviors[i] = air;
                    break;
                }
            }
        }
    }
}
