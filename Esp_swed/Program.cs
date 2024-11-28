using Multi_ESP;
using Swed64;
using System.Numerics;
using System.Reflection.PortableExecutable;

//main logic

//init swed
Swed swed = new Swed("cs2");

//get client.dll
IntPtr client = swed.GetModuleBase("client.dll");

//init render
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));

renderThread.Start();

//get screen size
Vector2 screenSize = renderer.screeenSize;

//store enteties
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

// offsets

//offsets.cs
int dwEntityList = 0x19F2488; 
int dwLocalPlayerPawn = 0x1855CE8;
int dwViewMatrix = 0x1A54550;

//cliend.dll offsets
int m_vOldOrigin = 0x1324;
int m_iTeamNum = 0x3E3;
int m_lifeState = 0x348;
int m_hPlayerPawn = 0x80C;
int m_vecViewOffset = 0xCB0;

int m_iHealth = 0x344;// new offset for hp
int m_iszPlayerName = 0x660;// name

int m_entitySpottedState = 0x23D0;
int m_bSpotted = 0x8;
int m_bOldIsScoped = 0x242C; // bool

int m_modelState = 0x170;
int m_pGameSceneNode = 0x328;


//esp loop
while (true)
{
    entities.Clear();

    //get entity list
    IntPtr entityList = swed.ReadPointer(client, dwEntityList);

    //make entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    //get localPlayerPawn
    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);

    //getting our team
    localPlayer.team =  swed.ReadInt(localPlayerPawn , m_iTeamNum);

    //loop through entity list
    for( int i = 1; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero) continue;

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero) continue;

        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);

        if (pawnHandle == 0) continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        //check lifestate
        int lifeState = swed.ReadInt(currentPawn, m_lifeState);
        if (lifeState !=256) continue;

        IntPtr sceneNode = swed.ReadPointer(currentPawn, m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, m_modelState + 0x80);

        //get matrix
        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        //populate entities
        Entity entity = new Entity();

        entity.spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted);
        entity.scoped = swed.ReadBool(currentPawn, m_bOldIsScoped);

        entity.name = swed.ReadString(currentController , m_iszPlayerName, 16).Split("\0")[0];// reading name

        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.health = swed.ReadInt(currentPawn, m_iHealth);// reading hp

        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);

        entity.position2d = Calculate.WordToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WordToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);

        /*entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, renderer.screeenSize);*/
        entities.Add(entity);
        Console.WriteLine($"entity pos: {entity.position.X} ,{entity.position.Y}, {entity.position.Z}. team : {entity.team}. ID: {i} ");
    }
    //Console.Clear();
    //update render

    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);
    Thread.Sleep(5);

}
