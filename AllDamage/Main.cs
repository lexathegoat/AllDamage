using System;
using GTA;
using LemonUI;
using LemonUI.Menus;

public class Main : Script
{
    private ObjectPool pool;
    private NativeMenu mainMenu;
    private NativeListItem<string> DifficultySettings;

    public static string Difficulty = "Hardcore";

    public Main()
    {
        pool = new ObjectPool();
        mainMenu = new NativeMenu("Realistic Mod", "SURVIVAL SETTINGS");

        DifficultySettings = new NativeListItem<string>("Difficulty Level", new string[] { "Normal", "Hardcore", "Simulation" });
        DifficultySettings.ItemChanged += (sender, e) => { Difficulty = DifficultySettings.SelectedItem; };

        mainMenu.Add(DifficultySettings);
        pool.Add(mainMenu);

        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void OnTick(object sender, EventArgs e)
    {
        pool.Process();
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == System.Windows.Forms.Keys.F10)
        {
            if (mainMenu.Visible) mainMenu.Visible = false;
            else mainMenu.Visible = true;
        }
    }
}