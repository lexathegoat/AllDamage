using System;
using GTA;
using GTA.Native;

public class PlayerSurvivalManager : Script
{
    private int lastBleedingTime = 0;

    private bool brokenLeg = false;
    private bool brokenArm = false;
    private int headTrauma = 0;

    public PlayerSurvivalManager()
    {
        Tick += OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped player = Game.Player.Character;
        if (player == null || !player.IsAlive) return;
        if (player.Health <= 105) 
        {
            Function.Call(Hash.SET_PED_TO_RAGDOLL, player, 10000, 10000, 0, false, false, false);
        }
        if (Main.Difficulty != "Normal")
        {
            float canYuzdesi = (float)(player.Health - 100) / (player.MaxHealth - 100);
            if (canYuzdesi < 0.50f)
            {
                if (Game.GameTime - lastBleedingTime >= 1000) 
                {
                    player.Health -= 1;
                    lastBleedingTime = Game.GameTime;

                    if (new Random().Next(0, 10) == 0) Function.Call(Hash.PLAY_PAIN, player, 7, 0);
                }
            }
        }

        RegionalDamageControl(player);
    }

    private void RegionalDamageControl(Ped player)
    {
        if (player.HasBeenDamagedByAnyWeapon())
        {
            OutputArgument outBone = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_PED_LAST_DAMAGE_BONE, player, outBone))
            {
                int boneID = outBone.GetResult<int>();

                if (boneID == 58271 || boneID == 63931 || boneID == 14201 || boneID == 51826)
                    brokenLeg = true;

                if (boneID == 61163 || boneID == 40892)
                    brokenArm = true;

                if (boneID == 31086)
                    headTrauma = Game.GameTime + 10000;
            }
            Function.Call(Hash.CLEAR_PED_LAST_DAMAGE_BONE, player);
        }

        if (brokenLeg)
        {
            Game.DisableControlThisFrame(Control.Sprint);
            Function.Call(Hash.REQUEST_CLIP_SET, "move_m@injured");
            if (Function.Call<bool>(Hash.HAS_CLIP_SET_LOADED, "move_m@injured"))
                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, player, "move_m@injured", 1.0f);
        }

        if (brokenArm && player.IsAiming) GameplayCamera.Shake(CameraShake.Hand, 1.0f);

        if (Game.GameTime < headTrauma)
        {
            Function.Call(Hash.SET_TIMECYCLE_MODIFIER, "Drunk");
        }
        else
        {
            Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
        }
    }
}