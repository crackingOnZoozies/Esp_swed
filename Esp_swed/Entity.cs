using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Multi_ESP
{
    public  class Entity
    {
        public Vector3 position { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector2 position2d { get; set; }
        public Vector2 viewPosition2D { get; set; }

        //bones
        public Vector3 origin { get; set; }
        public List<Vector3> bones { get; set; }
        public List<Vector2> bones2d { get; set; }

        public float distance { get; set; }
        //////////

        public short currentWeaponIndex { get; set; }
        public string currentWeaponName { get; set; }

        public int team {  get; set; }
        public int health { get; set; }

        public string name { get; set; }

        public bool spotted { get; set; }
        public bool scoped { get; set; }
    }
    public enum BonesIds
    {
        Wais = 0, //0
        Neck = 5, //1
        Head = 6, //2
        ShoulderLeft = 8, //3
        ForeLeft = 9, //4
        HandLeft = 11, //5
        ShoulderRight = 13, //6
        ForeRight = 14, //7
        HandRight = 16, //8
        KneeLeft = 23,//9
        FeetLeft = 24, //10
        KneeRight = 26, //11
        FeetRight = 27, //12
    }
    public enum Weapon
    {
        Deagle = 1,
        elite = 2,
        FiveSeven,
        Glock,
        Ak47 = 7,
        Aug = 8,
        AWP = 9,
        Famas = 10,
        G3Sg1,
        M249 = 14,
        Mac10 = 17,
        P90 = 19,
        UMP = 24,
        Xm1014 = 25,
        Bizon = 26,
        Mag7 = 27,
        Negev = 28,
        SawedOff = 29,
        Tec9 = 30,
        Zeus = 31,
        P2000 = 32,
        Mp7 = 33,
        Mp9 = 34,
        Nova = 35,
        P250 = 36,
        Scar20 = 38,
        Sg556 = 39,
        Scout = 40,
        ct_knife = 42,
        FlashBang = 43,
        HE = 44,
        Smoke = 45,
        Molotov = 46,
        Decoy = 47,
        Molotov_CT = 48,
        C4 = 49,
        M4A4 = 16,
        UspS = 61,
        M4A1_Silesed = 60,
        Cz75A = 63,
        revolver = 64,
        t_knife = 59
    }
}
