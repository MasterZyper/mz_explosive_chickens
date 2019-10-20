using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mz_explosive_chickens
{
    public class Chicken : BaseScript
    {
        private readonly uint zombie_group = 1;
        private readonly Random rand = new Random();
        int explosiontype = 0;
        public Chicken()
        {
            Tick += ChickenCreator;
            Tick += ExplodeDeadChickens;
            API.AddRelationshipGroup("zombies", ref zombie_group);
            API.SetRelationshipBetweenGroups(5, (uint)API.GetHashKey("PLAYER"), (uint)API.GetHashKey("zombies"));
            API.SetRelationshipBetweenGroups(5, (uint)API.GetHashKey("zombies"), (uint)API.GetHashKey("PLAYER"));
            API.ReserveNetworkMissionPeds(9000);
            API.CanRegisterMissionPeds(9000);
            explosiontype = Convert.ToInt32(API.GetResourceMetadata(API.GetCurrentResourceName(), "chicken_explosion_type", 0));
        }
        readonly List <Ped> chicken_list = new List<Ped>();

        private async Task ExplodeDeadChickens()
        {
            foreach (Ped ped in chicken_list)
            {
                if (ped.Exists() && ped.IsDead)
                {
                    //ExplosionType.Extinguisher,GrenadeL,Molotov1,SmokeGL
                    switch (explosiontype)
                    {
                        case 0:
                            World.AddExplosion(ped.Position, ExplosionType.GrenadeL, 5f, 3f, Game.PlayerPed);
                            break;
                        case 1:
                            World.AddExplosion(ped.Position, ExplosionType.Extinguisher, 5f, 3f, Game.PlayerPed);
                            break;
                        case 2:
                            World.AddExplosion(ped.Position, ExplosionType.Molotov1, 5f, 3f, Game.PlayerPed);
                            break;
                        case 3:
                            World.AddExplosion(ped.Position, ExplosionType.SmokeGL, 5f, 3f, Game.PlayerPed);
                            break;
                        case 4:
                            World.AddExplosion(ped.Position, ExplosionType.WaterHydrant, 5f, 3f, Game.PlayerPed);
                            break;
                        default:
                            World.AddExplosion(ped.Position, ExplosionType.ProxMine, 5f, 3f, Game.PlayerPed);
                            break;
                    }
                    ped.Delete();
                }
            }
            await Delay(0);
        }
        private async Task<Ped> CreateHen() 
        {
            int x_add = rand.Next(-25, 25);
            int y_add = rand.Next(-25, 25);
            Ped chicken = await World.CreatePed(PedHash.Hen, new Vector3(Game.PlayerPed.Position.X + x_add, Game.PlayerPed.Position.Y + y_add, Game.PlayerPed.Position.Z + 1));
            chicken.Position = new Vector3(chicken.Position.X, chicken.Position.Y,World.GetGroundHeight(chicken.Position));

            /*API.SetPedAccuracy(chicken.Handle, rand.Next(0, 20));
            API.SetPedSeeingRange(chicken.Handle, rand.Next(15, 80));
            API.SetPedHearingRange(chicken.Handle, rand.Next(15, 80));
            API.SetPedFleeAttributes(chicken.Handle, 0, false);
            API.SetPedCombatAttributes(chicken.Handle, 0, false);
            API.SetPedCombatAttributes(chicken.Handle, 1, false);
            API.SetPedCombatAttributes(chicken.Handle, 16, true);
            API.SetPedCombatAttributes(chicken.Handle, 17, false);
            API.SetPedCombatAttributes(chicken.Handle, 46, true);    //Immer angreifen
            API.SetPedCombatAttributes(chicken.Handle, 1424, false); //Schusswaffen
            API.SetPedCombatAttributes(chicken.Handle, 5, true);     //Angreifen wenn er nicht bewaffnet ist aber der Spieler
            API.SetPedCombatMovement(chicken.Handle, 3);
            API.SetPedCombatRange(chicken.Handle, 2);
            API.SetPedAlertness(chicken.Handle, 3);
            API.SetAmbientVoiceName(chicken.Handle, "ALIENS");
            API.SetPedEnableWeaponBlocking(chicken.Handle, true);

            //Brain
            API.SetPedPathAvoidFire(chicken.Handle, false);
            API.SetPedPathCanDropFromHeight(chicken.Handle,true);
            API.SetPedPathCanUseClimbovers(chicken.Handle, true);
            API.SetPedPathCanUseLadders(chicken.Handle, true);
            API.SetPedPathPreferToAvoidWater(chicken.Handle, true);
            API.SetPedPathsWidthPlant(chicken.Handle, true);

            API.SetPedDiesInVehicle(chicken.Handle, true);
            API.SetPedRelationshipGroupHash(chicken.Handle, (uint)API.GetHashKey("zombies"));

            API.DisablePedPainAudio(chicken.Handle, true);
            API.SetPedDiesInWater(chicken.Handle, false);
            API.SetPedDiesWhenInjured(chicken.Handle, false);
*/
            //HUD
          //  API.SetPedAiBlip(chicken.Handle, false);
          //  API.SetPedEnemyAiBlip(chicken.Handle, false);

            chicken.Task.FollowToOffsetFromEntity(Game.PlayerPed, new Vector3(), -1, 1.2f);
            return chicken;
        }


        private async Task ChickenCreator() 
        {
            List<Ped> trash_peds = new List<Ped>();
            foreach (Ped chicken in chicken_list) 
            {
                if (!chicken.Exists() || chicken.IsDead)
                {
                    trash_peds.Add(chicken);
                }
                if (World.GetDistance(Game.PlayerPed.Position, chicken.Position) > 50) 
                {
                    trash_peds.Add(chicken);
                }
            }
            foreach (Ped ped in trash_peds) 
            {
                ped.Delete();
                chicken_list.Remove(ped);
            }
            while (chicken_list.Count < 10) 
            {
                chicken_list.Add(await CreateHen());
            }
            await Delay(5000);
        }
    }
}
