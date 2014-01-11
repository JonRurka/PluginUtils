using System;
using System.Collections.Generic;
using System.Text;
using modCore;
using UnityEngine;

namespace PluginUtils
{
    class UtilMonitor : MonoBehaviour
    {
        public ModCore modCore;
        public ModApi api;
        public bool canSelect = false;
        public bool teleportEnabled = false;
        public bool digEnabled = false;
        public bool debugInfo = false;
        public List<GameObject> prefabs;
        string mouseOverLocation = string.Empty;
        string distance = string.Empty;
        private float updateInterval = 0.5f;
        private float accum = 0;
        private int frames = 0;
        private float timeleft = 0;
        private string format = string.Empty;
        private float _fps = 0;

        GameObject player;

        void Start()
        {
            DontDestroyOnLoad(this);
            InvokeRepeating("CheckForPlayer", 1, 1);
        }

        void Update()
        {
            if (canSelect)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 100000f);
                    player = api.GetNetworkPlayer();
                    modCore.Print("Objects hit: " + hits.Length);
                    Debug.Log("Objects hit: " + hits.Length);
                    foreach (RaycastHit hit in hits)
                    {
                        string dist = string.Empty;

                        if (player != null)
                            dist = Vector3.Distance(player.transform.position, hit.point).ToString();
                        else
                            dist = "NaN";

                        modCore.Print("--" + hit.transform.name + "; " + dist + "; " + hit.point.ToString());
                    }
                }
            }

            if (player != null)
            {
                Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        if (digEnabled)
                        {
                            //modCore.Print("Digging.");
                            Vector3 endPoint = new Ray(hit.point, ray.direction).GetPoint(1);
                            api.RemoveLinearTerrain(hit.point, endPoint, 1);
                        }
                        else if (teleportEnabled)
                        {
                            player.transform.position = new Vector3(hit.point.x, hit.point.y + 5f, hit.point.z);
                            player.rigidbody.velocity = Vector3.zero;
                        }

                    }
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        if (digEnabled)
                        {
                            //modCore.Print("Placing.");
                            api.AddLinearTerrain(hit.point, hit.point + Vector3.up, 1);
                        }
                    }
                    mouseOverLocation = hit.point.ToString();
                    distance = Vector3.Distance(player.transform.position, hit.point).ToString("0") + "m";
                }
                else
                {
                    mouseOverLocation = string.Empty;
                    distance = string.Empty;
                }
            }

            if (debugInfo)
            {
                timeleft -= Time.deltaTime;
                accum += Time.timeScale / Time.deltaTime;
                ++frames;

                if (timeleft <= 0.0)
                {
                    _fps = accum / frames;
                    format = "FPS: " + Math.Round(_fps).ToString();
                    timeleft = updateInterval;
                    accum = 0.0f;
                    frames = 0;
                }
            }
        }

        void CheckForPlayer()
        {
            int level = Application.loadedLevel;
            if (level == 2 && player == null)
            {
                player = api.GetNetworkPlayer();
                if (player != null)
                {
                    CancelInvoke("CheckForPlayer");
                }
            }
        }

        void OnGUI()
        {
            if (debugInfo && player != null)
            {
                float nine_tenths_width = Screen.width * 9 / 10;
                float boxWidth = Screen.width * 9 / 10;
                float boxHeight = Screen.height * 1 / 10;

                GUI.Label(new Rect(Screen.width / 2 + 10, Screen.height / 2 + 10, 100, 20), distance);
                GUI.Box(new Rect(nine_tenths_width, 0, boxWidth, 220), "");
                GUI.Label(new Rect(nine_tenths_width + 10, 0, 200, 20), "Performance:");
                GUI.Label(new Rect(nine_tenths_width + 10, 20, 200, 20), "-" + format);
                GUI.Label(new Rect(nine_tenths_width + 10, 60, 200, 20), "Player stats:");
                GUI.Label(new Rect(nine_tenths_width + 10, 80, 200, 20), "-Pos: " + player.transform.position.ToString());
                GUI.Label(new Rect(nine_tenths_width + 10, 100, 200, 20), "-Vel: " + player.rigidbody.velocity.ToString());
                GUI.Label(new Rect(nine_tenths_width + 10, 120, 200, 20), "-Rot: " + player.transform.rotation.eulerAngles.ToString());
                GUI.Label(new Rect(nine_tenths_width + 10, 140, 200, 20), "-Quat: " + player.transform.rotation.ToString());
                GUI.Label(new Rect(nine_tenths_width + 10, 180, 200, 20), "Memory Usage:");
                GUI.Label(new Rect(nine_tenths_width + 10, 200, 200, 20), "-Mono: " + System.GC.GetTotalMemory(true).ToString());
            }
        }

        public void InstantiateTruck()
        {
            player = api.GetNetworkPlayer();
            if (player != null)
            {
                //GameObject truck = (GameObject)Resources.Load("StarForge Truck");
                GameObject truck = null;
                if (truck != null)
                    Instantiate(truck, new Vector3(player.transform.position.x, player.transform.position.y + 5, player.transform.position.z + 5), Quaternion.identity);
                else
                    modCore.PrintError("Failed to located truck in Resources folder.");
            }
            else
                modCore.LogError("Failed to located player.");
        }

    }
}
