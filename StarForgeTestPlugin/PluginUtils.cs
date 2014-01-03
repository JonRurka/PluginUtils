using System;
using System.Collections.Generic;
using CodeHatch.AI;
using CodeHatch.Common;
using System.Text;
using modCore;
using UnityEngine;

namespace PluginUtils
{
    public class PluginUtils : IPlugin
    {
        ModCore modCore;
        UtilMonitor utilMonitor;
        Monitor monitor;


        public string Name
        {
            get
            {
                return "PluginUtils";
            }
        }

        public void Init(ModCore core)
        {
            modCore = core;
            modCore.Log(Name + " started successfully!");
            monitor = modCore.monitorComp;
            utilMonitor = new GameObject("UtilMontior").AddComponent<UtilMonitor>();
            utilMonitor.modCore = core;
            addCommands();
        }

        public bool Submit(string message)
        {
            bool received = false;
            if (message.StartsWith("/"))
            {
                string[] args = message.Split(' ');

                switch (args[0].ToLower())
                {
                    case "/addtestcube":
                        #region
                        GameObject player = GameObject.Find("Character");
                        if (Application.loadedLevel == 2 && player != null)
                        {
                            Mesh mesh = null;
                            if (modCore.importedMeshes.ContainsKey("testCube"))
                                mesh = modCore.importedMeshes["testCube"];
                            if (mesh != null)
                            {
                                try
                                {
                                    int numToSpawn = 1;
                                    if (args.Length == 2)
                                    {
                                        numToSpawn = Convert.ToInt32(args[1]);
                                    }
                                    for (int i = 0; i < numToSpawn; i++)
                                    {
                                        GameObject testCube = new GameObject("testCube" + 1);
                                        MeshFilter meshfilter = testCube.AddComponent<MeshFilter>();
                                        meshfilter.mesh = mesh;
                                        testCube.AddComponent<MeshRenderer>();
                                        testCube.AddComponent<BoxCollider>();
                                        testCube.AddComponent<Rigidbody>();
                                        testCube.rigidbody.mass *= 4;
                                        testCube.renderer.material.color = Color.gray;
                                        Vector3 position = new Vector3(player.transform.position.x + 5, player.transform.position.y + 5, player.transform.position.z + i + 0.5f);
                                        testCube.transform.position = position;
                                    }
                                }
                                catch (Exception e)
                                {
                                    PrintError(e.Message);
                                    modCore.LogError(e);
                                }
                            }
                            else
                                PrintError("model failed to load.");
                        }
                        else
                            PrintWarning("Command can not be used outside of game.");
                        received = true;
                        break;
                        #endregion

                    case "/loadedlevel":
                        Print("Level: " + Application.loadedLevel + ", " + Application.loadedLevelName);
                        received = true;
                        break;

                    case "/printparent":
                        #region print parent
                        if (args.Length == 2)
                        {
                            string[] parts = args[1].Split('.');
                            string objName = string.Empty;
                            foreach (string part in parts)
                            {
                                objName += " " + part;
                            }

                            objName = objName.Trim();

                            GameObject child = GameObject.Find(objName);
                            if (child != null)
                            {
                                Transform parent = child.transform.parent;
                                if (parent != null)
                                {
                                    Print("'" + objName + "'" + " parent: " + parent.name);
                                }
                                else
                                    PrintWarning("'" + objName + "'" + " has no parent.");
                            }
                            else
                                PrintError("object cannot be found!");
                        }
                        else
                            PrintError("invalid number of args1");
                        received = true;
                        break;
                        #endregion

                    case "/printlocation":
                        #region print Location
                        GameObject obj = GameObject.Find(args[1]);
                        if (obj != null)
                            Print("Location of " + obj.name + ": " + obj.transform.position.ToString());
                        else
                            modCore.PrintError(obj.name + " not found.");
                        received = true;
                        break;
                        #endregion

                    case "/listchildren":
                        #region print children
                        if (args.Length == 2)
                        {
                            string[] parts = args[1].Split('.');
                            string objName = string.Empty;
                            foreach (string part in parts)
                            {
                                objName += " " + part;
                            }

                            objName = objName.Trim();

                            GameObject parent = GameObject.Find(objName);
                            if (parent != null)
                            {
                                Print("\nChildren of " + parent.name + ": ");
                                for (int i = 0; i < parent.transform.childCount; i++)
                                {
                                    Print("--" + parent.transform.GetChild(i).name);
                                }
                            }
                            else
                                PrintError(parent.name + " doesn't exist.");
                        }
                        else
                            PrintError("invalid number of args1");
                        received = true;
                        break;
                    #endregion

                    case "/listusingobjects":
                        #region list Using Objects
                        if (args.Length == 2)
                        {
                            List<GameObject> objects = RetrieveGoList();
                            Print("GameObjects useing " + args[1] + ":");
                            foreach (GameObject GO in objects)
                            {
                                string[] components = GetAllComponents(GO);
                                foreach (string comp in components)
                                {
                                    if (comp.ToLower().Equals(args[1].ToLower()))
                                    {
                                        Print("--" + GO.name);
                                        break;
                                    }
                                }
                            }
                            Print("");
                        }
                        else
                        {
                            PrintError("please specify a component.");
                        }
                        received = true;
                        break;
                        #endregion

                    case "/listcomponents":
                        #region list components
                        if (args.Length > 1)
                        {
                            string notFound = string.Empty;
                            int notFoundCount = 0;
                            for (int i = 1; i < args.Length; i++)
                            {
                                string[] parts = args[i].Split('.');
                                string objName = string.Empty;
                                if (parts.Length > 1)
                                {
                                    foreach (string part in parts)
                                    {
                                        objName += part + " ";
                                    }
                                }
                                else
                                    objName = parts[0];

                                objName = objName.Trim();

                                GameObject Obj = GameObject.Find(objName);
                                if (Obj != null)
                                {
                                    Print("Components of " + Obj.name + ": ");
                                    string[] components = GetAllComponents(args[1]);
                                    foreach (string comp in components)
                                    {
                                        Print("--" + comp);
                                    }
                                }
                                else
                                {
                                    notFound += args[i] + ", ";
                                    notFoundCount++;
                                }
                            }
                            if (notFoundCount > 0)
                                PrintWarning("Objects not found: " + notFound);
                        }
                        else
                        {
                            PrintError("Please specify at least 1 object.");
                        }
                        received = true;
                        break;
                        #endregion

                    case "/listgameobjects":
                        #region list all objects
                        List<GameObject> GoList = RetrieveGoList();
                        if (GoList != null)
                        {
                            if (GoList.Count > 0)
                            {
                                if (args.Length == 1)
                                {
                                    string goListString = "Objects in scene: (" + GoList.Count + ") \n";
                                    foreach (GameObject go in GoList)
                                    {
                                        goListString += go.name + "\n";
                                        Print(go.name);
                                    }
                                    modCore.Log(goListString);
                                }
                                else if (args.Length == 2 && args[1].ToLower().Equals("-c"))
                                {
                                    string textToPrint = GetAllComponetsOfEverObject();
                                    PrintWarning("To many objects to print to console. List can still be found in the games log.");
                                    modCore.Log(textToPrint);

                                }

                            }
                            else
                            {
                                modCore.LogError("GoList list is empty.");
                                PrintError("GoList list is empty.");
                            }
                        }
                        else
                        {
                            modCore.LogError("GoList list is null.");
                            PrintError("GoList list is null.");
                        }
                        received = true;
                        break;
                        #endregion

                    case "/spawn":
                        #region spawn
                        if (args.Length > 1)
                        {
                            DynamicSpawner[] spawners = GameObject.FindObjectsOfType<DynamicSpawner>();
                            if (spawners.Length > 0)
                            {

                                int numToSpawn = 1;
                                switch (args[1].ToLower())
                                {
                                    case "leech":
                                        if (args.Length == 3)
                                        {
                                            try
                                            {
                                                numToSpawn = Convert.ToInt32(args[2]);
                                            }
                                            catch (Exception e)
                                            {
                                                modCore.LogError(e.Message);
                                                PrintError(e.Message);
                                            }
                                        }

                                        for (int i = 0; i < numToSpawn; i++)
                                            spawners[0].method_17();
                                        Print(numToSpawn + " leech(s) spawned.");
                                        break;

                                    case "crate":
                                        if (args.Length == 3)
                                        {
                                            try
                                            {
                                                numToSpawn = Convert.ToInt32(args[2]);
                                            }
                                            catch (Exception e)
                                            {
                                                modCore.LogError(e.Message);
                                                PrintError(e.Message);
                                            }
                                        }

                                        Vector3 playerLoc = GetPlayerGO().transform.position;
                                        for (int i = 0; i < numToSpawn; i++)
                                        {

                                            Vector3 location = new Vector3(playerLoc.x + 5, playerLoc.y + i, playerLoc.z);
                                            spawners[0].method_18((Vector3Int)location);
                                        }
                                        Print(numToSpawn + " crate(s) spawned.");
                                        break;

                                    case "-list":
                                        Print("Available items:");
                                        Print("--leech");
                                        Print("--crate");
                                        break;

                                    default:
                                        PrintError("\"" + args[1] + "\" not found.");
                                        break;
                                }
                            }
                            else
                                PrintWarning("No dynamic spawners in scene.");
                        }
                        else
                            PrintError("Please specify an object to spawn.");
                        received = true;
                        break;
                        #endregion

                    case "/selector":
                        #region selector
                        utilMonitor.canSelect = !utilMonitor.canSelect;
                        if (utilMonitor.canSelect)
                            Print("Item selector enabled.");
                        else
                            Print("Item selector disabled.");
                        received = true;
                        break;
                        #endregion

                    case "/setlocation":
                        #region set location
                        if (args.Length > 1)
                        {
                            if (args.Length == 3)
                            {
                                try
                                {
                                    string[] parts = args[1].Split('.');
                                    string objName = string.Empty;
                                    foreach (string part in parts)
                                    {
                                        objName += " " + part;
                                    }

                                    objName = objName.Trim();


                                    Vector3 newPos = new Vector3();
                                    string[] locationParts = args[2].Split(',');
                                    newPos.x = (float)Convert.ToDouble(locationParts[0]);
                                    newPos.y = (float)Convert.ToDouble(locationParts[1]);
                                    newPos.z = (float)Convert.ToDouble(locationParts[2]);

                                    GameObject locationObject = GameObject.Find(objName);
                                    if (locationObject != null)
                                    {
                                        locationObject.transform.position = newPos;
                                    }
                                    else
                                        PrintError(objName + " doesn't exist.");
                                }
                                catch (Exception e)
                                {
                                    PrintError(e.Message);
                                    modCore.LogError(e);
                                }
                            }
                            else if (args.Length > 3)
                                PrintError("Invalid number of arguments.");
                            else if (args.Length == 2)
                                PrintError("Must specify a location.");
                            else
                                PrintError("Unknown error.");
                        }
                        else
                            PrintError("Must specify an object.");
                        received = true;
                        break;
                        #endregion

                    case "/teleport":
                        #region teleport
                        utilMonitor.teleportEnabled = !utilMonitor.teleportEnabled;
                        if (utilMonitor.teleportEnabled)
                            Print("Teleportation enabled.");
                        else
                            Print("Teleportation disabled.");
                        received = true;
                        break;
                        #endregion

                    case "/getlocation":
                        #region get location
                        if (args.Length == 2)
                        {
                            string[] parts = args[1].Split('.');
                            string objName = string.Empty;
                            foreach (string part in parts)
                            {
                                objName += " " + part;
                            }

                            objName = objName.Trim();

                            GameObject locationObject = GameObject.Find(objName);
                            if (locationObject != null)
                            {
                                Print(locationObject.name + "'s location: " + locationObject.transform.position);
                            }
                            else
                                PrintError(objName + " doesn't exist.");
                        }
                        else
                            PrintError("Must specify an object.");
                        received = true;
                        break;
                        #endregion

                }
            }
            return received;
        }

        private void addCommands()
        {
            List<CommandDescription> commands = new List<CommandDescription>();
            commands.Add(new CommandDescription("addtestcube", "[amount]", "Adds test cubes from model folder."));
            commands.Add(new CommandDescription("loadedlevel", string.Empty, "Prints the current loaded level"));
            commands.Add(new CommandDescription("printparent", "<object>", "prints parent of object"));
            commands.Add(new CommandDescription("listchildren", "<object>", "prints children of object"));
            commands.Add(new CommandDescription("listcomponents", "<object>", "list all components of object"));
            commands.Add(new CommandDescription("listgameobjects", "[-c]",  "Lists all game objects", "Lists all game objects. If optional argument -c is give, the components of the object will be listed along with it and the text will be printed to the game log instead of the console"));
            commands.Add(new CommandDescription("listusingobjects", "<component>", "Lists every object useing the specified component."));
            commands.Add(new CommandDescription("getlocation", "<object>", "Prints object location"));
            commands.Add(new CommandDescription("setlocation", "<object> <postions>", "sets the position of object", "Sets the specified objects positon. Format of position: x,y,z (no spaces)."));
            commands.Add(new CommandDescription("selector", string.Empty, "Toggles the item selector.", "When enabled, the item selector will list to the console any object you click on, including the objects behind it. List format: <object name>; <distance from player>; <global position>"));
            commands.Add(new CommandDescription("spawn", "<item>|[-list] [amount]", "Spawns mobs and crates", "Spawns the item <item>. Replace <item> with -list for a list of available items. There is an optional 3rd argument for the amount of an item to spawn"));
            commands.Add(new CommandDescription("teleport", string.Empty, "Toggles player teleportation", "If enabled, it will spawn the player at the location the player is looking at or mousing over."));
            modCore.AddCommands(Name, commands);
        }

        public GameObject GetPlayerGO()
        {
            if (Application.loadedLevel == 2)
            {
                GameObject player = null;
                DynamicSpawner[] spawners = GameObject.FindObjectsOfType<DynamicSpawner>();
                if (spawners.Length > 0)
                {
                    player = spawners[0].PlayerTransform.gameObject;
                }
                return player;
            }
            else
                return null;
        }

        public void PrintComponents(GameObject GO)
        {
            string[] compList = GetAllComponents(GO);
            string Tmp = string.Empty;
            foreach (string component in compList)
            {
                Tmp += "\t" + component + "\n";
                modCore.Print("Component of " + GO.name + ": " + component);
            }
            modCore.Log("Components of " + GO.name + ": \n" + Tmp);
        }

        public string GetAllComponetsOfEverObject()
        {
            List<GameObject> GoList = RetrieveGoList();
            return GetAllComponetsOfEverObject(GoList);
        }

        public string GetAllComponetsOfEverObject(List<GameObject> objects)
        {
            string objList = "Ever component of every object (" + objects.Count + "):\n";
            foreach (GameObject obj in objects)
            {
                objList += obj.name + ":\n";
                string[] compList = GetAllComponents(obj);
                foreach (string component in compList)
                {
                    objList += "\t" + component + "\n";
                }
            }
            return objList;
        }

        public string[] GetAllComponents(string name)
        {
            return GetAllComponents(GameObject.Find(name));
        }

        public string[] GetAllComponents(GameObject GO)
        {
            List<string> componetList = new List<string>();
            Component[] allComponents;
            if (GO != null)
            {
                allComponents = GO.GetComponents<Component>();
                foreach (Component comp in allComponents)
                {
                    if (comp != null)
                    {
                        string compName = comp.GetType().ToString();
                        componetList.Add(compName);
                    }
                }
            }
            return componetList.ToArray();
        }

        public List<GameObject> RetrieveGoList()
        {
            List<GameObject> objectsInScene = new List<GameObject>();
            object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (object thisObject in allObjects)
            {
                if (((GameObject)thisObject).activeInHierarchy)
                {
                    objectsInScene.Add((GameObject)thisObject);
                }
            }

            if (objectsInScene == null)
                modCore.LogWarning("objectsInScene is null!");

            return objectsInScene;
        }

        public void Print(object message)
        {
            modCore.Print(message);
        }

        public void PrintWarning(object message)
        {
            modCore.PrintWarning(message);
        }

        public void PrintError(object message)
        {
            modCore.PrintError(message);
        }
    }
}
