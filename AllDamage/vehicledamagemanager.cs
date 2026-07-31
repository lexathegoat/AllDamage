using System;
using GTA;
using GTA.Native;

public class VehicleDamageManager : Script
{
    private Vehicle lastVehicle = null;
    private float lastBodyHealth = 0f;
    private Random rnd = new Random();

    private bool isRadiatorPunctured = false;
    private int radiatorTimer = 0;

    public VehicleDamageManager()
    {
        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;
        if (player == null || !player.IsInVehicle() || player.SeatIndex != VehicleSeat.Driver)
        {
            lastVehicle = null;
            return;
        }

        Vehicle vehicle = player.CurrentVehicle;
        float currentBodyHealth = vehicle.BodyHealth;

        if (lastVehicle != vehicle)
        {
            lastVehicle = vehicle;
            lastBodyHealth = currentBodyHealth;
            isRadiatorPunctured = false; 
            return;
        }

        if ((lastBodyHealth - currentBodyHealth) > 80f)
        {
            ApplyHeavyAccidentPhysics(vehicle);
        }

        float engineHealth = vehicle.EngineHealth;
        if (engineHealth <= 500f && engineHealth > 300f)
        {
            vehicle.EnginePowerMultiplier = 0.6f;
        }
        else if (engineHealth <= 300f && engineHealth > 100f)
        {
            vehicle.EnginePowerMultiplier = 0.3f;
            Function.Call(Hash.SET_VEHICLE_TYRES_CAN_BURST, vehicle, true);
        }
        else if (engineHealth <= 100f && engineHealth > 0f)
        {
            if (rnd.Next(0, 100) < 5) vehicle.IsEngineRunning = false;
            Function.Call(Hash.SET_VEHICLE_STEER_BIAS, vehicle, 1.0f);
        }

        if (isRadiatorPunctured)
        {
            int elapsedTime = Game.GameTime - radiatorTimer;
            if (elapsedTime > 30000 && elapsedTime < 60000)
            {
                vehicle.EngineHealth -= 0.5f;
            }
            else if (elapsedTime > 120000)
            {
                vehicle.EngineHealth = -100f; 
                vehicle.IsDriveable = false;
            }
        }

        lastBodyHealth = currentBodyHealth;
    }

    private void ApplyHeavyAccidentPhysics(Vehicle vehicle)
    {
        int tireProbability = rnd.Next(1, 101);
        int randomWheel = rnd.Next(0, 4);

        if (tireProbability <= 30)
        {
            Function.Call(Hash.SET_VEHICLE_TYRE_BURST, vehicle, randomWheel, true, 1000.0f);
        }
        else if (tireProbability <= 40)
        {
            Function.Call((Hash)0x19BD35B07A25055F, vehicle, randomWheel, false, true, true, true);
        }

        if (rnd.Next(1, 100) < 30)
        {
            Function.Call(Hash.SET_VEHICLE_DOOR_BROKEN, vehicle, 0, false);
        }

        if (rnd.Next(1, 100) < 50 && !isRadiatorPunctured)
        {
            isRadiatorPunctured = true;
            radiatorTimer = Game.GameTime;
        }

        if (Main.Difficulty == "Hardcore" || Main.Difficulty == "Simulation")
        {
            vehicle.FuelLevel -= 20f;
        }

        GameplayCamera.Shake(CameraShake.Jolt, 2.0f);
        Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "Bed", "WastedSounds", true);
    }
}