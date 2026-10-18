using UnityEngine;

[CreateAssetMenu(fileName = "LD60/Game Constants", menuName = "Game Constants")]
public class SOGameConstants : ScriptableObject
{
    public int playerkillplanedamage = 10;
    public float playerfallrespawntime = 2.0f;
    public float deathmenuload = 3.0f;
    public string mainmenulevel = "level_main_menu";
    public string gamelevel = "level_1";
}
