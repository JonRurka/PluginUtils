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
        public bool canSelect = false;
        public bool teleportEnabled = false;

        GameObject player;

        void Start()
        {
            DontDestroyOnLoad(this);
        }

        void Update()
        {
            if (canSelect)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                    RaycastHit[] hits = Physics.RaycastAll(ray, 100000f);
                    player = GameObject.Find("Character");
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

            if (teleportEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    player = GameObject.Find("Character");
                    if (player != null)
                    {
                        Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100000f))
                        {
                            player.transform.position = new Vector3(hit.point.x, hit.point.y + 5f, hit.point.z);
                        }
                    }
                    else
                    {
                        modCore.PrintError("Player not found.");
                    }
                }
            }

        }
    }
}
