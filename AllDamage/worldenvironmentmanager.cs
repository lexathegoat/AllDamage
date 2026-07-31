using System;
using GTA;

public class WorldEnvironmentManager : Script
{
    private int lastpolicecallTime = 0;

    public WorldEnvironmentManager()
    {
        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        try
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;

            // 1. Yağmurlu havada araç kontrolü
            if (Main.Difficulty == "Simulation" && player.IsInVehicle())
            {
                Vehicle playerVehicle = player.CurrentVehicle;
                if (playerVehicle != null && playerVehicle.Exists())
                {
                    bool isRaining = World.Weather == Weather.Raining || World.Weather == Weather.ThunderStorm;
                    if (isRaining)
                    {
                        playerVehicle.EnginePowerMultiplier = 1.2f;
                    }
                }
            }

            // 2. Hasarlı araç ve ihbar mekanizması
            if (player.IsInVehicle())
            {
                Vehicle currentVehicle = player.CurrentVehicle;

                if (currentVehicle != null && currentVehicle.Exists() && currentVehicle.BodyHealth < 400f)
                {
                    if (Game.GameTime - lastpolicecallTime > 60000)
                    {
                        Ped[] pedestrianNearby = World.GetNearbyPeds(player.Position, 30f);

                        if (pedestrianNearby != null)
                        {
                            foreach (Ped pedestrian in pedestrianNearby)
                            {
                                // Peda çarptıysak veya öldüyse oyunu patlatmaması için sıkı güvenlik
                                if (pedestrian != null && pedestrian.Exists() && pedestrian != player)
                                {
                                    if (pedestrian.IsAlive && !pedestrian.IsInVehicle() && !pedestrian.IsRagdoll)
                                    {
                                        try
                                        {
                                            pedestrian.Task.UseMobilePhone();
                                            Game.Player.WantedLevel = 1;
                                            lastpolicecallTime = Game.GameTime;
                                            break;
                                        }
                                        catch
                                        {
                                            // Ped o an görev alamıyorsa oyunu patlatma
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ana döngüdeki herhangi bir beklenmedik SHVDN patlamasını engeller
        }
    }
}